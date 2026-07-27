using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordSoul.Api.Errors;
using WordSoul.Api.Routing;
using WordSoul.Application.Exceptions;

namespace WordSoul.Api.Middlewares;

/// <summary>
/// Converts all unhandled HTTP request exceptions to the canonical RFC 7807
/// response. SignalR owns error propagation for hub requests, so those failures
/// are logged but no HTTP body is written.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    ILogger<GlobalExceptionMiddleware> logger,
    IWebHostEnvironment environment) : IMiddleware
{
    private static readonly string[] HubPaths =
    [
        ApiRoutes.ConventionalHubPrefix,
        ApiRoutes.NotificationHub,
        ApiRoutes.BattleHub
    ];

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug(
                "Request cancelled by client | {Method} {Path} | TraceId={TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = MapException(context, exception);
        var status = problem.Status ?? StatusCodes.Status500InternalServerError;
        var code = problem.Extensions["code"]?.ToString();
        var traceId = problem.Extensions["traceId"]?.ToString()
            ?? context.TraceIdentifier;
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled server error | {Method} {Path} | Status={Status} | Code={Code} | UserId={UserId} | TraceId={TraceId}",
                context.Request.Method,
                context.Request.Path,
                status,
                code,
                userId,
                traceId);
        }
        else
        {
            logger.LogWarning(
                "Request failed | {ExceptionType} | {Method} {Path} | Status={Status} | Code={Code} | UserId={UserId} | TraceId={TraceId}",
                exception.GetType().Name,
                context.Request.Method,
                context.Request.Path,
                status,
                code,
                userId,
                traceId);
        }

        if (IsHubRequest(context.Request.Path))
        {
            return;
        }

        if (environment.IsDevelopment())
        {
            problem.Extensions["debugInfo"] = exception.ToString();
        }

        if (exception is RateLimitException { RetryAfterSeconds: int retryAfter })
        {
            context.Response.Headers.RetryAfter = retryAfter.ToString();
            problem.Extensions["retryAfter"] = retryAfter;
        }

        if (exception is ExternalServiceException externalServiceException)
        {
            problem.Extensions["service"] = externalServiceException.ServiceName;
        }

        await ApiProblemDetails.WriteAsync(
            context,
            problem,
            context.RequestAborted);
    }

    private static ProblemDetails MapException(HttpContext context, Exception exception)
    {
        if (exception is WordSoulException wordSoulException)
        {
            IDictionary<string, string[]>? errors =
                wordSoulException is ValidationException validationException
                    ? validationException.Errors.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase)
                    : null;
            var descriptor = ApiErrorCatalog.ForApplicationCode(
                wordSoulException.Code);
            var safeDetail = ReferenceEquals(
                descriptor,
                ApiErrorCatalog.InternalServerError)
                    ? null
                    : wordSoulException.Message;

            return ApiProblemDetails.Create(
                context,
                descriptor,
                safeDetail,
                errors);
        }

        return exception switch
        {
            KeyNotFoundException => ApiProblemDetails.Create(
                context,
                ApiErrorCatalog.ResourceNotFound,
                exception.Message),

            ArgumentException => ApiProblemDetails.Create(
                context,
                ApiErrorCatalog.BadRequest,
                exception.Message),

            UnauthorizedAccessException => ApiProblemDetails.Create(
                context,
                ApiErrorCatalog.AccessForbidden,
                exception.Message),

            DbUpdateConcurrencyException => ApiProblemDetails.Create(
                context,
                ApiErrorCatalog.Conflict,
                "The resource was modified by another request. Please reload and try again."),

            _ => ApiProblemDetails.CreateForStatus(
                context,
                StatusCodes.Status500InternalServerError)
        };
    }

    private static bool IsHubRequest(PathString path)
    {
        return HubPaths.Any(prefix =>
            path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
