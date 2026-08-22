using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WordSoul.Api.Errors;
using WordSoul.Api.Middlewares;
using WordSoul.Infrastructure.BackgroundServices;

namespace WordSoul.Tests.Middlewares;

public sealed class RequestResponseLoggingMiddlewareTests
{
    [Fact]
    public async Task RestoresResponseStreamBeforeOuterStatusCodePageWrites()
    {
        var context = CreateHttpContext("/");
        var originalBody = context.Response.Body;
        var middleware = new RequestResponseLoggingMiddleware(
            next: _ =>
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            },
            NullLogger<RequestResponseLoggingMiddleware>.Instance);

        await middleware.InvokeAsync(context, new SystemLogQueue());

        Assert.Same(originalBody, context.Response.Body);

        await ApiProblemDetails.WriteAsync(
            context,
            ApiProblemDetails.CreateForStatus(
                context,
                StatusCodes.Status404NotFound));

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(
            "RESOURCE_NOT_FOUND",
            json.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RestoresResponseStreamWhenDownstreamThrows()
    {
        var context = CreateHttpContext("/api/failing");
        var originalBody = context.Response.Body;
        var expectedException = new InvalidOperationException("boom");
        var middleware = new RequestResponseLoggingMiddleware(
            _ => Task.FromException(expectedException),
            NullLogger<RequestResponseLoggingMiddleware>.Instance);

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context, new SystemLogQueue()));

        Assert.Same(expectedException, actualException);
        Assert.Same(originalBody, context.Response.Body);

        await context.Response.WriteAsync("outer middleware can still write");
        Assert.True(context.Response.Body.Length > 0);
    }

    private static DefaultHttpContext CreateHttpContext(string path)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOptions();
        serviceCollection.AddProblemDetails();

        var context = new DefaultHttpContext
        {
            RequestServices = serviceCollection.BuildServiceProvider()
        };
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }
}
