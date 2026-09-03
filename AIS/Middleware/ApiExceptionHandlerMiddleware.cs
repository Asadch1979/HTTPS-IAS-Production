using System;
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
                    _logger.LogDebug(ex, "Passing API exception to global system error middleware.");
                }

                throw;
            }
        }
    }
}
