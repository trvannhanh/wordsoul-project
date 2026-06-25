using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using WordSoul.Application.Exceptions;

namespace WordSoul.Api.Middlewares;

/// <summary>
/// Global exception handling middleware.
/// Implements <see cref="IMiddleware"/> for DI-friendly registration (transient).
/// <para>
/// Pipeline position: must be registered FIRST (outermost) so that it wraps
/// all subsequent middleware including authentication and routing.
/// </para>
/// <para>
/// Thread-safety: all state is request-scoped via the injected
/// <see cref="HttpContext"/>; no shared mutable state exists.
/// </para>
/// </summary>
public sealed class GlobalExceptionMiddleware : IMiddleware
{
    // SignalR hub path prefix – errors inside hubs are handled by the SignalR
    // pipeline itself; we only log and skip writing an HTTP problem-details body.
    private const string HubPathPrefix = "/hubs/";

    // Known SignalR hub paths in this application.
    private static readonly string[] KnownHubPaths =
    [
        "/notificationHub",
        "/battleHub"
    ];

    // Stateless: IWebHostEnvironment is injected per-request via InvokeAsync
    // to keep the middleware thread-safe (no stored instance state).

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Core dispatch
    // ──────────────────────────────────────────────────────────────────────────

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var isHubPath = IsSignalRHubPath(path);

        if (exception is WordSoulException wordSoulEx)
        {
            var statusCode = (int)wordSoulEx.StatusCode;

            // 4xx → Warning; 5xx → Error (WordSoulException subtypes are all 4xx/502)
            if (statusCode >= 500)
            {
                Log.Error(exception,
                    "Server-side WordSoulException | {ExceptionType} | {Path} | {StatusCode}",
                    exception.GetType().Name, path, statusCode);
            }
            else
            {
                Log.Warning(
                    "Client error | {ExceptionType} | {Path} | {StatusCode} | {Message}",
                    exception.GetType().Name, path, statusCode, exception.Message);
            }

            if (isHubPath)
            {
                // SignalR manages its own error propagation to clients.
                // Log is sufficient; do not overwrite the response stream.
                return;
            }

            // Resolve environment from DI (IWebHostEnvironment is always registered)
            var env = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>();

            if (wordSoulEx is ValidationException validationEx)
            {
                await WriteValidationProblemAsync(context, validationEx, env);
            }
            else
            {
                await WriteWordSoulProblemAsync(context, wordSoulEx, env);
            }
        }
        else
        {
            // Unknown / unhandled exception → 500
            Log.Error(exception,
                "Unhandled exception | {ExceptionType} | {Path}",
                exception.GetType().Name, path);

            if (isHubPath)
            {
                return;
            }

            var env = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>();

            await WriteInternalServerErrorAsync(context, exception, env);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Writers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Writes a standard <see cref="ProblemDetails"/> body for typed WordSoul exceptions.</summary>
    private static async Task WriteWordSoulProblemAsync(
        HttpContext context,
        WordSoulException ex,
        IWebHostEnvironment env)
    {
        var statusCode = (int)ex.StatusCode;

        var problem = new ProblemDetails
        {
            Type     = ex.Type,
            Title    = ex.Title,
            Status   = statusCode,
            Detail   = ex.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] =
            Activity.Current?.Id ?? context.TraceIdentifier;

        // Add Retry-After hint for rate-limit responses
        if (ex is RateLimitException rateLimitEx && rateLimitEx.RetryAfterSeconds.HasValue)
        {
            context.Response.Headers["Retry-After"] =
                rateLimitEx.RetryAfterSeconds.Value.ToString();
        }

        // In Development: attach full debug info; in Production: omit entirely
        if (env.IsDevelopment())
        {
            problem.Extensions["debugInfo"] = ex.ToString();
        }

        await WriteProblemJsonAsync(context, statusCode, problem);
    }

    /// <summary>
    /// Writes a <see cref="ValidationProblemDetails"/> body that includes
    /// field-level error messages from <see cref="ValidationException.Errors"/>.
    /// </summary>
    private static async Task WriteValidationProblemAsync(
        HttpContext context,
        ValidationException ex,
        IWebHostEnvironment env)
    {
        var statusCode = (int)ex.StatusCode;

        // Build the field-level errors dictionary (string[] values required by ValidationProblemDetails)
        var errors = ex.Errors.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value,
            StringComparer.OrdinalIgnoreCase);

        var problem = new ValidationProblemDetails(errors)
        {
            Type     = ex.Type,
            Title    = ex.Title,
            Status   = statusCode,
            Detail   = ex.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] =
            Activity.Current?.Id ?? context.TraceIdentifier;

        if (env.IsDevelopment())
        {
            problem.Extensions["debugInfo"] = ex.ToString();
        }

        await WriteProblemJsonAsync(context, statusCode, problem);
    }

    /// <summary>
    /// Writes a production-safe 500 <see cref="ProblemDetails"/> body.
    /// Stack traces are never exposed outside Development.
    /// </summary>
    private static async Task WriteInternalServerErrorAsync(
        HttpContext context,
        Exception ex,
        IWebHostEnvironment env)
    {
        const int statusCode = (int)HttpStatusCode.InternalServerError;

        var problem = new ProblemDetails
        {
            Type     = "https://wordsoul.app/errors/internal-server-error",
            Title    = "Internal Server Error",
            Status   = statusCode,
            Detail   = "An unexpected error occurred. Please try again later.",
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] =
            Activity.Current?.Id ?? context.TraceIdentifier;

        if (env.IsDevelopment())
        {
            problem.Extensions["debugInfo"] = ex.ToString();
        }

        await WriteProblemJsonAsync(context, statusCode, problem);
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Low-level HTTP write helper
    // ──────────────────────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition      = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented               = false
    };

    private static async Task WriteProblemJsonAsync(
        HttpContext context,
        int statusCode,
        object problem)
    {
        // Guard: if response has already started (e.g., streaming), we cannot
        // change headers or write a problem body.
        if (context.Response.HasStarted)
        {
            Log.Warning(
                "Response has already started; cannot write ProblemDetails. " +
                "TraceId: {TraceId}", context.TraceIdentifier);
            return;
        }

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static bool IsSignalRHubPath(string path)
    {
        if (path.StartsWith(HubPathPrefix, StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var hubPath in KnownHubPaths)
        {
            if (path.StartsWith(hubPath, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
