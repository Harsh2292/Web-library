using System.Diagnostics;

namespace WebLibrary.AgenticApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = context.TraceIdentifier;
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "Request started {RequestId} {Method} {Path}",
                requestId,
                context.Request.Method,
                context.Request.Path);

            await _next(context);

            stopwatch.Stop();

            var logLevel = context.Response.StatusCode >= 500
                ? LogLevel.Error
                : context.Response.StatusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            _logger.Log(
                logLevel,
                "Request finished {RequestId} {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
