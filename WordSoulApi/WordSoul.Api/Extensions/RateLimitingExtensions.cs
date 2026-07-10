using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using WordSoul.Api.Options;

namespace WordSoul.Api.Extensions;

/// <summary>
/// Extension method that registers all 7 rate-limiting policies for WordSoulApi
/// on the <see cref="IServiceCollection"/>.
/// 
/// Policies:
///   A) "global-ip"          — Fixed Window, 100 req/min per anonymous IP
///   B) "authenticated-user" — Sliding Window, 300 req/min per userId
///   C) "ai-vocabulary"      — Token Bucket, 20 tokens / refill 5 per 30s per userId (+ Redis fallback)
///   D) "audio-generation"   — Fixed Window, 30 req/5min per userId (+ Redis fallback)
///   E) "matchmaking-join"   — Fixed Window, 10 req/min per userId
///   F) "auth-endpoints"     — Fixed Window, 10 req/15min per IP
///   G) "gym-battle-start"   — Token Bucket, 5 tokens / refill 1 per 2min per userId
/// </summary>
public static class RateLimitingExtensions
{
    // ─────────────────────────────────────────────────────────────────────────
    //  Policy name constants — use these to decorate controllers/actions.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Policy A: Anonymous IP-based Fixed Window guard (100 req/min).</summary>
    public const string GlobalIp = "global-ip";

    /// <summary>Policy B: Per-user Sliding Window throttle (300 req/min).</summary>
    public const string AuthenticatedUser = "authenticated-user";

    /// <summary>Policy C: Gemini AI generation Token Bucket (20 tokens / +5 per 30s).</summary>
    public const string AiVocabulary = "ai-vocabulary";

    /// <summary>Policy D: Azure Speech TTS Fixed Window (30 req/5min).</summary>
    public const string AudioGeneration = "audio-generation";

    /// <summary>Policy E: PvP matchmaking queue Fixed Window (10 req/min).</summary>
    public const string MatchmakingJoin = "matchmaking-join";

    /// <summary>Policy F: Auth endpoints IP Fixed Window (10 req/15min).</summary>
    public const string AuthEndpoints = "auth-endpoints";

    /// <summary>Policy G: Gym battle start Token Bucket (5 tokens / +1 per 2min).</summary>
    public const string GymBattleStart = "gym-battle-start";

    // ─────────────────────────────────────────────────────────────────────────
    //  Internal worker bypass constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Custom header set by internal background workers.
    /// Only trusted on loopback addresses; forged headers from external IPs are silently ignored.
    /// </summary>
    private const string InternalWorkerHeader = "X-Internal-Worker";

    // ─────────────────────────────────────────────────────────────────────────
    //  Registration entry point
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Registers all WordSoul rate limiting policies.
    /// Call this from <c>Program.cs</c> before <c>builder.Build()</c>.
    /// </summary>
    public static IServiceCollection AddWordSoulRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind strongly-typed options
        var opts = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.Configure<RateLimitingOptions>(
            configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddRateLimiter(limiterOptions =>
        {
            // ── Default rejection handler ────────────────────────────────────
            // This runs for any endpoint that has a rate limiter policy but no
            // per-policy OnRejected override. Policies below override individually.
            limiterOptions.OnRejected = async (context, cancellationToken) =>
                await WriteRateLimitRejectionAsync(
                    context.HttpContext,
                    "default",
                    cancellationToken);

            // ── Policy A: global-ip ──────────────────────────────────────────
            // Fixed Window, partitioned by remote IP.
            // Protects unauthenticated endpoints from bulk scraping / anonymous abuse.
            limiterOptions.AddPolicy(GlobalIp, httpContext =>
            {
                // Internal workers on loopback bypass all rate limiting.
                if (IsInternalWorkerRequest(httpContext))
                    return RateLimitPartition.GetNoLimiter("internal-worker");

                var ip = GetClientIp(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"global-ip:{ip}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit          = opts.GlobalIp.PermitLimit,
                        Window               = TimeSpan.FromSeconds(opts.GlobalIp.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = opts.GlobalIp.QueueLimit,
                    });
            });

            // ── Policy B: authenticated-user ─────────────────────────────────
            // Sliding Window, partitioned by userId claim.
            // Falls back to IP for unauthenticated requests so the policy never crashes.
            limiterOptions.AddPolicy(AuthenticatedUser, httpContext =>
            {
                if (IsInternalWorkerRequest(httpContext))
                    return RateLimitPartition.GetNoLimiter("internal-worker");

                var userId = GetUserId(httpContext);
                var key    = string.IsNullOrEmpty(userId)
                    ? $"auth-user:anon:{GetClientIp(httpContext)}"
                    : $"auth-user:{userId}";

                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: key,
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit          = opts.AuthenticatedUser.PermitLimit,
                        Window               = TimeSpan.FromSeconds(opts.AuthenticatedUser.WindowSeconds),
                        SegmentsPerWindow    = opts.AuthenticatedUser.SegmentsPerWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = opts.AuthenticatedUser.QueueLimit,
                    });
            });

            // ── Policy C: ai-vocabulary ──────────────────────────────────────
            // Token Bucket per userId. The OnRejected handler here is per-policy
            // so that it can embed the correct policy name in the log / response.
            // For distributed (multi-instance) rate limiting, the actual work is
            // done by RedisRateLimiter (Infrastructure layer); this in-memory
            // policy acts as the local fallback when Redis is unavailable.
            limiterOptions.AddPolicy(AiVocabulary, httpContext =>
            {
                if (IsInternalWorkerRequest(httpContext))
                    return RateLimitPartition.GetNoLimiter("internal-worker");

                var userId = GetUserId(httpContext) ?? GetClientIp(httpContext);

                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: $"ai-vocab:{userId}",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit               = opts.AiVocabulary.TokenLimit,
                        ReplenishmentPeriod      = TimeSpan.FromSeconds(opts.AiVocabulary.RefillSeconds),
                        TokensPerPeriod          = opts.AiVocabulary.RefillAmount,
                        AutoReplenishment        = true,
                        QueueProcessingOrder     = QueueProcessingOrder.OldestFirst,
                        QueueLimit               = opts.AiVocabulary.QueueLimit,
                    });
            });

            // ── Policy D: audio-generation ───────────────────────────────────
            // Fixed Window per userId (TTS is CPU + Azure cost intensive).
            // Like ai-vocabulary, the Redis-backed distributed limiter sits
            // alongside this in-memory policy for cross-instance correctness.
            limiterOptions.AddPolicy(AudioGeneration, httpContext =>
            {
                if (IsInternalWorkerRequest(httpContext))
                    return RateLimitPartition.GetNoLimiter("internal-worker");

                var userId = GetUserId(httpContext) ?? GetClientIp(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"audio-gen:{userId}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit          = opts.AudioGeneration.PermitLimit,
                        Window               = TimeSpan.FromSeconds(opts.AudioGeneration.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = opts.AudioGeneration.QueueLimit,
                    });
            });

            // ── Policy E: matchmaking-join ───────────────────────────────────
            // Fixed Window per userId — prevents queue spam.
            limiterOptions.AddPolicy(MatchmakingJoin, httpContext =>
            {
                if (IsInternalWorkerRequest(httpContext))
                    return RateLimitPartition.GetNoLimiter("internal-worker");

                var userId = GetUserId(httpContext) ?? GetClientIp(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"mm-join:{userId}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit          = opts.MatchmakingJoin.PermitLimit,
                        Window               = TimeSpan.FromSeconds(opts.MatchmakingJoin.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = opts.MatchmakingJoin.QueueLimit,
                    });
            });

            // ── Policy F: auth-endpoints ─────────────────────────────────────
            // Fixed Window per IP — brute-force / credential-stuffing shield.
            // NOTE: partitioned by IP (not userId) because the user may not yet
            // exist (register) or the credential may be wrong (login).
            limiterOptions.AddPolicy(AuthEndpoints, httpContext =>
            {
                // Internal workers never hit auth endpoints; keep bypass for consistency.
                if (IsInternalWorkerRequest(httpContext))
                    return RateLimitPartition.GetNoLimiter("internal-worker");

                var ip = GetClientIp(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"auth:{ip}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit          = opts.AuthEndpoints.PermitLimit,
                        Window               = TimeSpan.FromSeconds(opts.AuthEndpoints.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = opts.AuthEndpoints.QueueLimit,
                    });
            });

            // ── Policy G: gym-battle-start ───────────────────────────────────
            // Token Bucket per userId — prevents rapid battle farming for XP/items.
            limiterOptions.AddPolicy(GymBattleStart, httpContext =>
            {
                if (IsInternalWorkerRequest(httpContext))
                    return RateLimitPartition.GetNoLimiter("internal-worker");

                var userId = GetUserId(httpContext) ?? GetClientIp(httpContext);

                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: $"gym-battle:{userId}",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit           = opts.GymBattleStart.TokenLimit,
                        ReplenishmentPeriod  = TimeSpan.FromSeconds(opts.GymBattleStart.RefillSeconds),
                        TokensPerPeriod      = opts.GymBattleStart.RefillAmount,
                        AutoReplenishment    = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = opts.GymBattleStart.QueueLimit,
                    });
            });
        });

        return services;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Shared OnRejected handler
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Writes a RFC 7807 Problem Details response for a rate-limited request.
    /// Handles the response entirely in-place — does NOT throw or rely on
    /// <c>GlobalExceptionMiddleware</c>.
    /// </summary>
    public static async Task WriteRateLimitRejectionAsync(
        HttpContext httpContext,
        string      policyName,
        CancellationToken cancellationToken)
    {
        // Guard: response headers/body cannot be changed once streaming has started.
        if (httpContext.Response.HasStarted)
            return;

        // ── Resolve Retry-After from the rate limit lease metadata ───────────
        int retryAfterSeconds = 60; // safe fallback

        // Try to obtain retryAfter from the OnRejected context (set by the caller)
        if (httpContext.Items.TryGetValue("_rl_retry_after", out var retryObj) &&
            retryObj is int retryFromLease && retryFromLease > 0)
        {
            retryAfterSeconds = retryFromLease;
        }

        // ── Structured logging ────────────────────────────────────────────────
        var userId = GetUserId(httpContext) ?? "anonymous";
        var ip     = GetClientIp(httpContext);
        var path   = httpContext.Request.Path.ToString();

        Serilog.Log.Warning(
            "Rate limit exceeded | Policy={Policy} | UserId={UserId} | IP={IP} | Path={Path} | RetryAfterSeconds={RetryAfterSeconds}",
            policyName, userId, ip, path, retryAfterSeconds);

        // ── Write HTTP response ───────────────────────────────────────────────
        httpContext.Response.StatusCode  = StatusCodes.Status429TooManyRequests;
        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();

        var problemDetails = new
        {
            type       = "https://wordsoul.app/errors/rate-limit",
            title      = "Too Many Requests",
            status     = 429,
            detail     = "You have exceeded the request limit. Please try again later.",
            instance   = path,
            retryAfter = retryAfterSeconds,
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented        = false,
        });

        await httpContext.Response.WriteAsync(json, cancellationToken);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helper: extract userId from JWT claims
    // ─────────────────────────────────────────────────────────────────────────

    private static string? GetUserId(HttpContext context)
    {
        // ClaimTypes.NameIdentifier maps to the "nameid" / "sub" JWT claim
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId) ? null : userId;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helper: resolve the real client IP respecting X-Forwarded-For
    // ─────────────────────────────────────────────────────────────────────────

    private static string GetClientIp(HttpContext context)
    {
        // Respect proxy / Azure Load Balancer forwarding header
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            // X-Forwarded-For can be a comma-separated list; leftmost = original client
            var firstIp = forwarded.Split(',', StringSplitOptions.TrimEntries)[0];
            if (IPAddress.TryParse(firstIp, out _))
                return firstIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helper: detect internal background worker calls
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <see langword="true"/> when the request carries the internal worker
    /// header <b>AND</b> originates from the loopback interface (127.0.0.1 / ::1).
    /// 
    /// The loopback check prevents external callers from spoofing the header
    /// to bypass rate limiting.
    /// </summary>
    private static bool IsInternalWorkerRequest(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(InternalWorkerHeader))
            return false;

        var remoteIp = context.Connection.RemoteIpAddress;

        return remoteIp is not null && IPAddress.IsLoopback(remoteIp);
    }
}
