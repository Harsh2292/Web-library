using System.Net;
using System.Text.Json;
using WebLibrary.AgenticApi.Models;

namespace WebLibrary.AgenticApi.Middleware
{
    public class GlobalErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorMiddleware> _logger;

        public GlobalErrorMiddleware(
            RequestDelegate next,
            ILogger<GlobalErrorMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception for request {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var (statusCode, errorCode, message) = exception switch
            {
                ArgumentException =>
                    (HttpStatusCode.BadRequest,
                     "INVALID_REQUEST",
                     exception.Message),

                HttpRequestException =>
                    (HttpStatusCode.BadGateway,
                     "EXTERNAL_SERVICE_ERROR",
                     "Failed to reach an external service"),

                TaskCanceledException =>
                    (HttpStatusCode.GatewayTimeout,
                     "REQUEST_TIMEOUT",
                     "The request timed out"),

                InvalidOperationException =>
                    (HttpStatusCode.UnprocessableEntity,
                     "UNPROCESSABLE_PDF",
                      exception.Message),

                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized,
                     "UNAUTHORIZED",
                     "Unauthorized access"),

                _ =>
                    (HttpStatusCode.InternalServerError,
                     "INTERNAL_ERROR",
                     "An unexpected error occurred")
            };

            var requestId = context.TraceIdentifier;

            var response = new BookMetadataResult
            {
                Success = false,
                ErrorMessage = message,
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonCamelCase()
            });

            await context.Response.WriteAsync(json);
        }

        private static JsonNamingPolicy JsonCamelCase()
            => JsonNamingPolicy.CamelCase;
    }
}
