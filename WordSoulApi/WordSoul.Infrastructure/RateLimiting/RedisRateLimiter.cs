using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace WordSoul.Infrastructure.RateLimiting;

/// <summary>
/// A Redis-backed distributed rate limiter for cost-sensitive endpoints.
/// 
/// Used by:
///   • "ai-vocabulary"   — Gemini AI generation (Token Bucket semantics)
///   • "audio-generation" — Azure Speech TTS (Fixed Window semantics)
/// 
/// Design:
///   ┌─────────────────────────────────────────────────────────────────────┐
///   │  Each call to AcquireAsync performs an atomic Lua script in Redis.  │
///   │  If Redis is unavailable, the limiter falls back to "allow" with a  │
///   │  warning log (circuit-breaker pattern) so the app stays available.  │
///   └─────────────────────────────────────────────────────────────────────┘
/// 
/// Thread-safety:
///   The class is stateless (all state lives in Redis); it is safe to use as
///   a Singleton and to call from concurrent request threads simultaneously.
/// </summary>
public sealed class RedisRateLimiter : IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimiter> _logger;

    // ─────────────────────────────────────────────────────────────────────────
    //  Lua scripts for atomic Redis operations
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fixed Window INCR + EXPIRE script.
    /// 
    /// KEYS[1] = rate-limit key (e.g., "rl:audio-gen:user:42")
    /// ARGV[1] = window duration in seconds
    /// ARGV[2] = max permits per window
    /// 
    /// Returns: current count after increment (integer).
    ///   If count > max, the caller treats it as a rejection.
    /// </summary>
    private static readonly string FixedWindowLuaScript = @"
local current = redis.call('INCR', KEYS[1])
if current == 1 then
    redis.call('EXPIRE', KEYS[1], ARGV[1])
end
return current
";

    /// <summary>
    /// Token Bucket script using two Redis keys per partition:
    ///   KEYS[1] = token count  (e.g., "rl:ai-vocab:user:42:tokens")
    ///   KEYS[2] = last-refill timestamp (e.g., "rl:ai-vocab:user:42:ts")
    /// 
    /// ARGV[1] = token limit (bucket capacity)
    /// ARGV[2] = tokens to add per refill period
    /// ARGV[3] = refill period in seconds
    /// ARGV[4] = tokens to consume per request (always 1 for WordSoul)
    /// ARGV[5] = TTL for the keys (token_limit × refill_seconds × 2, generous margin)
    /// 
    /// Returns: 1 if the request is allowed, 0 if rejected.
    /// </summary>
    private static readonly string TokenBucketLuaScript = @"
local now      = tonumber(redis.call('TIME')[1])
local tokens   = tonumber(redis.call('GET', KEYS[1]) or ARGV[1])
local last_ts  = tonumber(redis.call('GET', KEYS[2]) or now)
local limit    = tonumber(ARGV[1])
local refill   = tonumber(ARGV[2])
local period   = tonumber(ARGV[3])
local cost     = tonumber(ARGV[4])
local ttl      = tonumber(ARGV[5])

-- Compute elapsed periods and add tokens
local elapsed  = math.max(0, now - last_ts)
local periods  = math.floor(elapsed / period)
tokens = math.min(limit, tokens + periods * refill)

if tokens < cost then
    return 0
end

tokens = tokens - cost
redis.call('SET', KEYS[1], tokens, 'EX', ttl)
redis.call('SET', KEYS[2], now,    'EX', ttl)
return 1
";

    // ─────────────────────────────────────────────────────────────────────────
    //  Constructor
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the <see cref="RedisRateLimiter"/>.
    /// </summary>
    /// <param name="redis">
    /// The <see cref="IConnectionMultiplexer"/> already registered in DI
    /// (configured in Program.cs via <c>AddStackExchangeRedisCache</c>).
    /// Do NOT create a new connection here — reuse the shared one.
    /// </param>
    /// <param name="logger">Logger injected by DI.</param>
    public RedisRateLimiter(
        IConnectionMultiplexer redis,
        ILogger<RedisRateLimiter> logger)
    {
        _redis  = redis  ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to acquire a permit using a Fixed Window algorithm in Redis.
    /// 
    /// Each call increments an atomic counter in Redis. The counter resets after
    /// <paramref name="windowSeconds"/> seconds (set via EXPIRE on first write).
    /// </summary>
    /// <param name="key">
    /// Unique partition key, e.g., <c>"rl:audio-gen:user:42"</c>.
    /// Should include policy name + user/IP to avoid cross-policy collisions.
    /// </param>
    /// <param name="permitLimit">Maximum requests allowed in the window.</param>
    /// <param name="windowSeconds">Window duration in seconds.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if the request is allowed;
    /// <see langword="false"/> if the limit has been exceeded.
    /// On Redis failure, always returns <see langword="true"/> (fail-open).
    /// </returns>
    public async Task<bool> TryAcquireFixedWindowAsync(
        string key,
        int    permitLimit,
        int    windowSeconds,
        CancellationToken ct = default)
    {
        try
        {
            var db = _redis.GetDatabase();

            var result = (long)await db.ScriptEvaluateAsync(
                FixedWindowLuaScript,
                keys:   [new RedisKey(key)],
                values: [new RedisValue(windowSeconds.ToString()), new RedisValue(permitLimit.ToString())]);

            var allowed = result <= permitLimit;

            if (!allowed)
            {
                _logger.LogDebug(
                    "Redis Fixed Window REJECT | Key={Key} | Count={Count} | Limit={Limit}",
                    key, result, permitLimit);
            }

            return allowed;
        }
        catch (RedisException ex)
        {
            // ── Circuit-breaker fallback: Redis unavailable → fail-open ────
            _logger.LogWarning(ex,
                "Redis unavailable for rate limiting (key={Key}). Falling back to allow-all.",
                key);
            return true; // fail-open: never block legitimate traffic due to Redis outage
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in RedisRateLimiter.TryAcquireFixedWindowAsync (key={Key}).",
                key);
            return true;
        }
    }

    /// <summary>
    /// Attempts to acquire a permit using a Token Bucket algorithm in Redis.
    /// 
    /// Tokens are replenished every <paramref name="refillSeconds"/> seconds by
    /// an atomic Lua script that computes elapsed time on each request.
    /// </summary>
    /// <param name="key">Unique partition key (policy name + userId/IP).</param>
    /// <param name="tokenLimit">Maximum bucket capacity (initial tokens = limit).</param>
    /// <param name="refillAmount">Tokens added per refill period.</param>
    /// <param name="refillSeconds">Seconds between refill ticks.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if a token was consumed (request allowed);
    /// <see langword="false"/> if the bucket is empty.
    /// On Redis failure, returns <see langword="true"/> (fail-open).
    /// </returns>
    public async Task<bool> TryAcquireTokenBucketAsync(
        string key,
        int    tokenLimit,
        int    refillAmount,
        int    refillSeconds,
        CancellationToken ct = default)
    {
        try
        {
            var db  = _redis.GetDatabase();
            var ttl = tokenLimit * refillSeconds * 2; // generous TTL, cleans up idle keys

            var result = (long)await db.ScriptEvaluateAsync(
                TokenBucketLuaScript,
                keys: [
                    new RedisKey($"{key}:tokens"),
                    new RedisKey($"{key}:ts"),
                ],
                values: [
                    new RedisValue(tokenLimit.ToString()),
                    new RedisValue(refillAmount.ToString()),
                    new RedisValue(refillSeconds.ToString()),
                    new RedisValue("1"),       // cost = 1 token per request
                    new RedisValue(ttl.ToString()),
                ]);

            var allowed = result == 1;

            if (!allowed)
            {
                _logger.LogDebug(
                    "Redis Token Bucket REJECT | Key={Key}", key);
            }

            return allowed;
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex,
                "Redis unavailable for token-bucket rate limiting (key={Key}). Falling back to allow-all.",
                key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in RedisRateLimiter.TryAcquireTokenBucketAsync (key={Key}).",
                key);
            return true;
        }
    }

    /// <summary>
    /// Returns the remaining window TTL (in seconds) for a given rate-limit key.
    /// Used to populate the <c>Retry-After</c> header.
    /// Returns <c>60</c> as a safe fallback on any error.
    /// </summary>
    public async Task<int> GetRetryAfterSecondsAsync(string key, int fallbackSeconds = 60)
    {
        try
        {
            var db  = _redis.GetDatabase();
            var ttl = await db.KeyTimeToLiveAsync(key);
            return ttl.HasValue ? (int)Math.Ceiling(ttl.Value.TotalSeconds) : fallbackSeconds;
        }
        catch
        {
            return fallbackSeconds;
        }
    }

    // IDisposable — nothing to dispose (Redis connection is managed by DI)
    public void Dispose() { }
}
