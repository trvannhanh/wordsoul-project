using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using WordSoul.Domain.Entities;
using WordSoul.Infrastructure.BackgroundServices;

namespace WordSoul.Api.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, SystemLogQueue logQueue)
        {
            // Bỏ qua log cho các endpoint không cần thiết hoặc file uploads (multipart)
            var contentType = context.Request.ContentType;
            if (contentType != null && contentType.Contains("multipart/form-data"))
            {
                await _next(context);
                return;
            }

            // Bỏ qua logging các đường dẫn tĩnh hoặc thư mục không cần thiết
            if (context.Request.Path.Value != null && 
               (context.Request.Path.Value.Contains("/notificationHub") || 
                context.Request.Path.Value.Contains("/battleHub") ||
                context.Request.Path.Value.Contains("/swagger") ||
                context.Request.Path.Value.Contains("/scalar")))
            {
                await _next(context);
                return;
            }

            var stopWatch = Stopwatch.StartNew();

            // 1. Read Request
            context.Request.EnableBuffering();
            var requestPayload = await ReadStreamInChunks(context.Request.Body);
            context.Request.Body.Position = 0;

            // 2. Prepare to intercept Response
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;
            var responsePayload = string.Empty;

            try
            {
                // Gọi next middleware
                await _next(context);
            }
            finally
            {
                stopWatch.Stop();

                try
                {
                    // 3. Read the intercepted response and copy it to the real stream.
                    responseBodyStream.Position = 0;
                    responsePayload = await ReadStreamInChunks(responseBodyStream);
                    responseBodyStream.Position = 0;
                    await responseBodyStream.CopyToAsync(
                        originalBodyStream,
                        context.RequestAborted);
                }
                catch (Exception ex)
                {
                    // Observability must never replace the actual request exception
                    // or turn a valid response into a server error.
                    _logger.LogWarning(
                        ex,
                        "Could not capture response body for {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);
                }
                finally
                {
                    // Outer middleware (for example StatusCodePages or the global
                    // exception handler) may still need to write to the response.
                    context.Response.Body = originalBodyStream;
                }

                try
                {
                    // 4. Send to Queue
                    var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                    // Truncate payloads if they are excessively large to prevent DB issues
                    if (requestPayload.Length > 4000) requestPayload = requestPayload.Substring(0, 4000) + "...[truncated]";
                    if (responsePayload.Length > 4000) responsePayload = responsePayload.Substring(0, 4000) + "...[truncated]";

                    var systemLog = new SystemLog
                    {
                        Timestamp = DateTime.UtcNow,
                        Method = context.Request.Method,
                        Path = context.Request.Path.Value ?? string.Empty,
                        StatusCode = context.Response.StatusCode,
                        DurationMs = stopWatch.ElapsedMilliseconds,
                        RequestPayload = requestPayload,
                        ResponsePayload = responsePayload,
                        IpAddress = ipAddress,
                        UserId = userId
                    };

                    await logQueue.EnqueueAsync(systemLog, context.RequestAborted);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Could not enqueue request log for {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);
                }
            }
        }

        private static async Task<string> ReadStreamInChunks(Stream stream)
        {
            if (stream.Length == 0)
                return string.Empty;

            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream, Encoding.UTF8, true, readChunkBufferLength, true);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;
            do
            {
                readChunkLength = await reader.ReadBlockAsync(readChunk, 0, readChunkBufferLength);
                await textWriter.WriteAsync(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);

            return textWriter.ToString();
        }
    }
}
