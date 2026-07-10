using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using WordSoul.Api.Extensions;

namespace WordSoul.Tests.RateLimiting;

// ═══════════════════════════════════════════════════════════════════════════════
//  Section 1 — Unit tests for the OnRejected callback behaviour
//
//  These tests call RateLimitingExtensions.WriteRateLimitRejectionAsync directly,
//  with a fake DefaultHttpContext so no real web host is needed.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit tests for <see cref="RateLimitingExtensions.WriteRateLimitRejectionAsync"/>.
/// </summary>
public class RateLimitRejectionWriterTests
{
    // ── Helper ────────────────────────────────────────────────────────────────

    private static DefaultHttpContext BuildDefaultHttpContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        ctx.Request.Path  = "/api/test";
        return ctx;
    }

    // ── Status code ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Returns_Status429_OnRejection()
    {
        var ctx = BuildDefaultHttpContext();
        await RateLimitingExtensions.WriteRateLimitRejectionAsync(ctx, "auth-endpoints", CancellationToken.None);
        Assert.Equal(StatusCodes.Status429TooManyRequests, ctx.Response.StatusCode);
    }

    // ── Content-Type ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Returns_ProblemJsonContentType_OnRejection()
    {
        var ctx = BuildDefaultHttpContext();
        await RateLimitingExtensions.WriteRateLimitRejectionAsync(ctx, "ai-vocabulary", CancellationToken.None);
        Assert.Equal("application/problem+json", ctx.Response.ContentType);
    }

    // ── Retry-After header ────────────────────────────────────────────────────

    [Fact]
    public async Task Sets_RetryAfterHeader_WithPositiveInteger()
    {
        var ctx = BuildDefaultHttpContext();
        await RateLimitingExtensions.WriteRateLimitRejectionAsync(ctx, "auth-endpoints", CancellationToken.None);

        Assert.True(ctx.Response.Headers.ContainsKey("Retry-After"));
        Assert.True(int.TryParse(ctx.Response.Headers["Retry-After"], out var seconds));
        Assert.True(seconds > 0);
    }

    [Fact]
    public async Task Sets_RetryAfterHeader_From_Items_Override()
    {
        var ctx = BuildDefaultHttpContext();
        ctx.Items["_rl_retry_after"] = 42;

        await RateLimitingExtensions.WriteRateLimitRejectionAsync(ctx, "global-ip", CancellationToken.None);

        Assert.Equal("42", (string?)ctx.Response.Headers["Retry-After"]);
    }

    // ── Problem Details body ──────────────────────────────────────────────────

    [Fact]
    public async Task Writes_ValidProblemDetailsBody()
    {
        var ms  = new MemoryStream();
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = ms;
        ctx.Request.Path  = "/api/auth/login";

        await RateLimitingExtensions.WriteRateLimitRejectionAsync(ctx, "auth-endpoints", CancellationToken.None);

        ms.Position = 0;
        var json = await new StreamReader(ms).ReadToEndAsync();
        using var doc  = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("https://wordsoul.app/errors/rate-limit", root.GetProperty("type").GetString());
        Assert.Equal("Too Many Requests",                       root.GetProperty("title").GetString());
        Assert.Equal(429,                                       root.GetProperty("status").GetInt32());
        Assert.True(root.TryGetProperty("detail", out _));
        Assert.True(root.TryGetProperty("retryAfter", out _));
    }

    // ── Guard: response already started ──────────────────────────────────────

    [Fact]
    public async Task DoesNotThrow_When_ResponseHasStarted()
    {
        var ctx = BuildDefaultHttpContext();
        // First write — this simulates a response already in progress
        await ctx.Response.WriteAsync("streaming...");

        // Must not throw even if headers are already committed
        var ex = await Record.ExceptionAsync(() =>
            RateLimitingExtensions.WriteRateLimitRejectionAsync(ctx, "audio-generation", CancellationToken.None));

        Assert.Null(ex);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  Section 2 — Integration tests using WebApplicationFactory
//
//  A minimal in-process ASP.NET Core 9 host is constructed with a stub
//  POST /api/auth/login endpoint and the auth-endpoints rate limiter policy
//  configured with a very low limit (PermitLimit=2, Window=10s) so that
//  the 3rd request within the window is rejected with 429.
//
//  WHY minimal host instead of the full WordSoul host:
//    • No DB connection required
//    • No Redis required
//    • Tests run in <1 second
//    • Fully deterministic
// ═══════════════════════════════════════════════════════════════════════════════

public class RateLimitingIntegrationTests : IClassFixture<RateLimitingTestWebAppFactory>
{
    private readonly HttpClient _client;

    public RateLimitingIntegrationTests(RateLimitingTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// The first N requests are not rate-limited (return 401 from stub).
    /// The (N+1)th request — over the per-IP Fixed Window limit — must return 429
    /// with a valid RFC 7807 Problem Details body and a <c>Retry-After</c> header.
    ///
    /// Test limit: PermitLimit=2 / 10s, so the 3rd request is 429.
    /// </summary>
    [Fact]
    public async Task AuthLogin_Third_Request_Returns429_WithProblemDetails()
    {
        // Allowed requests (return 401, not 429)
        for (int i = 0; i < 2; i++)
        {
            var r = await _client.PostAsJsonAsync("/api/auth/login",
                new { username = "test@wordsoul.app", password = "WrongPw!1" });

            // Assert it is NOT 429 yet (will be 401 from the stub)
            Assert.NotEqual(HttpStatusCode.TooManyRequests, r.StatusCode);
        }

        // Third request — must be 429
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "test@wordsoul.app", password = "WrongPw!1" });

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);

        // Content-Type must be application/problem+json
        Assert.Equal("application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        // Body must be a valid Problem Details object
        var body = await response.Content.ReadAsStringAsync();
        using var doc  = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.Equal(429,                                       root.GetProperty("status").GetInt32());
        Assert.Equal("Too Many Requests",                       root.GetProperty("title").GetString());
        Assert.Equal("https://wordsoul.app/errors/rate-limit",  root.GetProperty("type").GetString());

        // Retry-After header must be present and numeric
        Assert.True(response.Headers.Contains("Retry-After"));
        var retryAfter = response.Headers.GetValues("Retry-After").FirstOrDefault();
        Assert.True(int.TryParse(retryAfter, out var seconds) && seconds > 0);
    }

    /// <summary>
    /// Health-check endpoints must never be rate-limited, even under high load.
    /// </summary>
    [Fact]
    public async Task HealthEndpoint_NeverReturns_429()
    {
        for (int i = 0; i < 50; i++)
        {
            var response = await _client.GetAsync("/health");
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
    }

    /// <summary>
    /// Different X-Forwarded-For IPs must get isolated rate-limit buckets.
    /// Exhausting IP 10.0.0.1's bucket must not affect IP 10.0.0.2.
    /// </summary>
    [Fact]
    public async Task Auth_DifferentIPs_HaveIsolated_RateLimitBuckets()
    {
        // Exhaust budget for 10.0.0.1 (> 2 requests)
        for (int i = 0; i < 3; i++)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login");
            req.Headers.Add("X-Forwarded-For", "10.0.0.1");
            req.Content = JsonContent.Create(new { username = "u", password = "p" });
            await _client.SendAsync(req);
        }

        // A fresh request from a different IP should NOT be 429
        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login");
        req2.Headers.Add("X-Forwarded-For", "10.0.0.2");
        req2.Content = JsonContent.Create(new { username = "u", password = "p" });
        var response2 = await _client.SendAsync(req2);

        Assert.NotEqual(HttpStatusCode.TooManyRequests, response2.StatusCode);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  Section 3 — Minimal WebApplicationFactory
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Minimal <see cref="WebApplicationFactory{TProgram}"/> for rate-limiting integration tests.
///
/// Configures:
///   • auth-endpoints policy: PermitLimit=2, Window=10s (fast assertion — 3rd req = 429)
///   • POST /api/auth/login stub that returns 401
///   • GET /health endpoint with DisableRateLimiting
/// </summary>
public class RateLimitingTestWebAppFactory : WebApplicationFactory<RateLimitingTestWebAppFactory>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddLogging(l => l.SetMinimumLevel(LogLevel.Warning));

            // Register rate limiter with a test-tuned auth-endpoints policy (2 req / 10s)
            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, ct) =>
                    await RateLimitingExtensions.WriteRateLimitRejectionAsync(
                        context.HttpContext, "auth-endpoints", ct);

                // auth-endpoints: Fixed Window, 2 req per 10s per IP (test override)
                options.AddPolicy(RateLimitingExtensions.AuthEndpoints, httpContext =>
                {
                    var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                             ?? httpContext.Connection.RemoteIpAddress?.ToString()
                             ?? "127.0.0.1";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: $"auth:{ip}",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit          = 2,  // intentionally low for tests
                            Window               = TimeSpan.FromSeconds(10),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit           = 0,
                        });
                });
            });

            services.AddRouting();
        });

        builder.Configure(app =>
        {
            app.UseRateLimiter();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Stub: POST /api/auth/login — returns 401 (simulated failed login).
                // The rate limiter intercepts BEFORE this handler when the limit is exceeded.
                endpoints.MapPost("/api/auth/login", () => Results.Unauthorized())
                    .WithMetadata(new EnableRateLimitingAttribute(RateLimitingExtensions.AuthEndpoints));

                // Health check — must bypass rate limiting entirely
                endpoints.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
                    .DisableRateLimiting();
            });
        });
    }
}
