using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIS.Middleware
{
    /// <summary>
    /// Ensures API/AJAX requests always receive JSON responses for unhandled exceptions.
    /// </summary>
    public class ApiExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionHandlerMiddleware> _logger;

        public ApiExceptionHandlerMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (LoginRedirectHelper.IsApiRequest(context.Request))
                {
                    if (context.Response.HasStarted)
                    {
                        _logger.LogError(ex, "Failed to write API error because the response has already started.");
                        throw;
                    }

                    await WriteApiErrorAsync(context, ex);
                    return;
                }

                throw;
            }
        }

        private async Task WriteApiErrorAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}.", context.Request.Path);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                error = "server_error",
                message = "An unexpected error occurred. Please try again later."
            });

            await context.Response.WriteAsync(payload);
        }
    }
}
