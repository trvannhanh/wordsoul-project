using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WordSoul.Api.Errors;
using WordSoul.Api.Middlewares;
using WordSoul.Application.Exceptions;

namespace WordSoul.Tests.Errors;

public sealed class ApiProblemDetailsTests
{
    [Fact]
    public void CreateForStatus_AddsStableCodeTraceIdAndInstance()
    {
        var context = CreateHttpContext("/api/missing");
        context.TraceIdentifier = "trace-test";

        var problem = ApiProblemDetails.CreateForStatus(
            context,
            StatusCodes.Status404NotFound);

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("/api/missing", problem.Instance);
        Assert.Equal(ErrorCodes.ResourceNotFound, problem.Extensions["code"]);
        Assert.Equal("trace-test", problem.Extensions["traceId"]);
    }

    [Fact]
    public async Task Middleware_WritesValidationProblemWithFieldErrors()
    {
        var middleware = CreateMiddleware(Environments.Production);
        var context = CreateHttpContext("/api/vocabulary-sets");
        var exception = new ValidationException(new Dictionary<string, string[]>
        {
            ["title"] = ["Title is required."]
        });

        await middleware.InvokeAsync(
            context,
            _ => Task.FromException(exception));

        using var json = await ReadResponseAsync(context);
        var root = json.RootElement;

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, context.Response.StatusCode);
        Assert.Equal(ApiProblemDetails.ContentType, context.Response.ContentType);
        Assert.Equal(ErrorCodes.ValidationFailed, root.GetProperty("code").GetString());
        Assert.Equal(
            "Title is required.",
            root.GetProperty("errors").GetProperty("title")[0].GetString());
        Assert.True(root.TryGetProperty("traceId", out _));
        Assert.False(root.TryGetProperty("debugInfo", out _));
    }

    [Fact]
    public async Task Middleware_WritesProductionSafeInternalServerError()
    {
        var middleware = CreateMiddleware(Environments.Production);
        var context = CreateHttpContext("/api/failing");

        await middleware.InvokeAsync(
            context,
            _ => Task.FromException(new InvalidOperationException("secret database detail")));

        using var json = await ReadResponseAsync(context);
        var root = json.RootElement;

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal(ApiErrorCodes.InternalServerError, root.GetProperty("code").GetString());
        Assert.DoesNotContain("secret database detail", root.GetRawText());
        Assert.False(root.TryGetProperty("debugInfo", out _));
    }

    [Theory]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest, ErrorCodes.BadRequest)]
    [InlineData(typeof(KeyNotFoundException), StatusCodes.Status404NotFound, ErrorCodes.ResourceNotFound)]
    [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status403Forbidden, ErrorCodes.AccessForbidden)]
    public async Task Middleware_MapsLegacyExceptionsDuringMigration(
        Type exceptionType,
        int expectedStatus,
        string expectedCode)
    {
        var middleware = CreateMiddleware(Environments.Production);
        var context = CreateHttpContext("/api/legacy");
        var exception = (Exception)Activator.CreateInstance(exceptionType, "legacy failure")!;

        await middleware.InvokeAsync(
            context,
            _ => Task.FromException(exception));

        using var json = await ReadResponseAsync(context);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.Equal(
            expectedCode,
            json.RootElement.GetProperty("code").GetString());
    }

    private static GlobalExceptionMiddleware CreateMiddleware(string environmentName)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.EnvironmentName).Returns(environmentName);

        return new GlobalExceptionMiddleware(
            NullLogger<GlobalExceptionMiddleware>.Instance,
            environment.Object);
    }

    private static DefaultHttpContext CreateHttpContext(string path)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOptions();
        serviceCollection.AddProblemDetails();
        var services = serviceCollection.BuildServiceProvider();
        var context = new DefaultHttpContext
        {
            RequestServices = services
        };
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
