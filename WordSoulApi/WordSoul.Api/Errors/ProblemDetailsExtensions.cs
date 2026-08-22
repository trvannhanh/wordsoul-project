using Microsoft.AspNetCore.Mvc;
using WordSoul.Api.Middlewares;

namespace WordSoul.Api.Errors;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddWordSoulProblemDetails(
        this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
                ApiProblemDetails.Enrich(context.HttpContext, context.ProblemDetails);
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                var errors = actionContext.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "The supplied value is invalid."
                                : error.ErrorMessage)
                            .ToArray(),
                        StringComparer.OrdinalIgnoreCase);

                var problem = ApiProblemDetails.Create(
                    actionContext.HttpContext,
                    ApiErrorCatalog.ValidationFailed,
                    errors: errors);

                return new ObjectResult(problem)
                {
                    StatusCode = ApiErrorCatalog.ValidationFailed.Status,
                    ContentTypes = { ApiProblemDetails.ContentType }
                };
            };
        });

        services.AddTransient<GlobalExceptionMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseWordSoulProblemDetails(
        this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseStatusCodePages(async statusCodeContext =>
        {
            var httpContext = statusCodeContext.HttpContext;
            await ApiProblemDetails.WriteAsync(
                httpContext,
                ApiProblemDetails.CreateForStatus(
                    httpContext,
                    httpContext.Response.StatusCode),
                httpContext.RequestAborted);
        });

        return app;
    }
}
