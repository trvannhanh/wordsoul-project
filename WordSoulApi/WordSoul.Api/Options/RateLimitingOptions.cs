namespace WordSoul.Api.Options;

/// <summary>
/// Root options object bound from the "RateLimiting" section of appsettings.json.
/// All rate limit thresholds are driven by configuration — zero magic numbers in code.
/// </summary>
public sealed class RateLimitingOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Policy A — anonymous/unauthenticated IP-based fallback guard.
    /// Algorithm: Fixed Window.
    /// </summary>
    public FixedWindowPolicyOptions GlobalIp { get; init; } = new();

    /// <summary>
    /// Policy B — per-user general throttle for all authenticated endpoints.
    /// Algorithm: Sliding Window.
    /// </summary>
    public SlidingWindowPolicyOptions AuthenticatedUser { get; init; } = new();

    /// <summary>
    /// Policy C — Gemini AI vocabulary generation (cost + quota sensitive).
    /// Algorithm: Token Bucket.
    /// Applied to: POST /api/vocabulary-sets/ai-preview, POST /api/vocabulary-sets/ai-create.
    /// </summary>
    public TokenBucketPolicyOptions AiVocabulary { get; init; } = new();

    /// <summary>
    /// Policy D — Azure Speech TTS generation (CPU + cost intensive).
    /// Algorithm: Fixed Window.
    /// Applied to: endpoints that trigger TTS audio generation.
    /// </summary>
    public FixedWindowPolicyOptions AudioGeneration { get; init; } = new();

    /// <summary>
    /// Policy E — PvP matchmaking queue entry (prevent queue spam / abuse).
    /// Algorithm: Fixed Window.
    /// Applied to: POST /api/pvp/queue/join.
    /// </summary>
    public FixedWindowPolicyOptions MatchmakingJoin { get; init; } = new();

    /// <summary>
    /// Policy F — Login / Register brute-force &amp; credential-stuffing protection.
    /// Algorithm: Fixed Window.
    /// Applied to: POST /api/auth/login, POST /api/auth/register.
    /// Partition key: IP address (not userId — user may not yet exist).
    /// </summary>
    public FixedWindowPolicyOptions AuthEndpoints { get; init; } = new();

    /// <summary>
    /// Policy G — Gym battle initiation (prevent rapid XP/item farming).
    /// Algorithm: Token Bucket.
    /// Applied to: POST /api/gym/{gymId}/battle/start.
    /// </summary>
    public TokenBucketPolicyOptions GymBattleStart { get; init; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────
//  Primitive option types
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Settings for a Fixed Window rate limiting policy.
/// </summary>
public sealed class FixedWindowPolicyOptions
{
    /// <summary>Maximum number of permits (requests) allowed in the window.</summary>
    public int PermitLimit { get; init; } = 100;

    /// <summary>Duration of the window in seconds.</summary>
    public int WindowSeconds { get; init; } = 60;

    /// <summary>Number of parallel queue slots for requests that arrive when the limit is full (0 = no queue).</summary>
    public int QueueLimit { get; init; } = 0;
}

/// <summary>
/// Settings for a Sliding Window rate limiting policy.
/// </summary>
public sealed class SlidingWindowPolicyOptions
{
    /// <summary>Maximum permits allowed across the entire window.</summary>
    public int PermitLimit { get; init; } = 300;

    /// <summary>Total window duration in seconds.</summary>
    public int WindowSeconds { get; init; } = 60;

    /// <summary>Number of segments the window is divided into for fine-grained sliding.</summary>
    public int SegmentsPerWindow { get; init; } = 6;

    /// <summary>Queue depth for excess requests.</summary>
    public int QueueLimit { get; init; } = 0;
}

/// <summary>
/// Settings for a Token Bucket rate limiting policy.
/// </summary>
public sealed class TokenBucketPolicyOptions
{
    /// <summary>Maximum tokens the bucket can hold (also the initial fill level).</summary>
    public int TokenLimit { get; init; } = 20;

    /// <summary>Number of tokens added to the bucket on each refill tick.</summary>
    public int RefillAmount { get; init; } = 5;

    /// <summary>Interval between refill ticks, in seconds.</summary>
    public int RefillSeconds { get; init; } = 30;

    /// <summary>Queue depth for requests that arrive when the bucket is empty (0 = immediate 429).</summary>
    public int QueueLimit { get; init; } = 0;
}
