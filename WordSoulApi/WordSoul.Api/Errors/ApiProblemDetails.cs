using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WordSoul.Api.Errors;

/// <summary>
/// Creates and writes the canonical RFC 7807 error contract used by WordSoul API.
/// </summary>
public static class ApiProblemDetails
{
    public const string ContentType = "application/problem+json";

    public static ProblemDetails Create(
        HttpContext httpContext,
        ApiErrorDescriptor descriptor,
        string? detail = null,
        IDictionary<string, string[]>? errors = null)
    {
        ProblemDetails problem = errors is null
            ? new ProblemDetails()
            : new ValidationProblemDetails(errors);

        problem.Status = descriptor.Status;
        problem.Title = descriptor.Title;
        problem.Detail = detail ?? descriptor.Detail;
        problem.Type = descriptor.Type;
        problem.Instance = httpContext.Request.Path;
        problem.Extensions["code"] = descriptor.Code;
        problem.Extensions["traceId"] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return problem;
    }

    public static ProblemDetails CreateForStatus(HttpContext httpContext, int status) =>
        Create(httpContext, ApiErrorCatalog.ForStatus(status));

    public static void Enrich(HttpContext httpContext, ProblemDetails problem)
    {
        var status = problem.Status ?? httpContext.Response.StatusCode;
        var defaults = ApiErrorCatalog.ForStatus(status);

        problem.Status = status;
        problem.Type ??= defaults.Type;
        problem.Title ??= defaults.Title;
        problem.Detail ??= defaults.Detail;
        problem.Instance ??= httpContext.Request.Path;

        if (!problem.Extensions.ContainsKey("code"))
        {
            problem.Extensions["code"] = defaults.Code;
        }

        if (!problem.Extensions.ContainsKey("traceId"))
        {
            problem.Extensions["traceId"] =
                Activity.Current?.Id ?? httpContext.TraceIdentifier;
        }
    }

    public static async Task WriteAsync(
        HttpContext httpContext,
        ProblemDetails problem,
        CancellationToken cancellationToken = default)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        Enrich(httpContext, problem);

        httpContext.Response.StatusCode =
            problem.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = ContentType;

        var serviceProvider = httpContext.RequestServices;
        var service = serviceProvider?.GetService<IProblemDetailsService>();
        var written = service is not null
            && await service.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem
            });

        if (!written)
        {
            await httpContext.Response.WriteAsJsonAsync(
                problem,
                options: null,
                contentType: ContentType,
                cancellationToken: cancellationToken);
        }
    }
}
