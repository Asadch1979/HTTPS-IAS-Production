using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIS.Middleware
    {
    public class SystemErrorNotificationMiddleware
        {
        private readonly RequestDelegate _next;
        private readonly ILogger<SystemErrorNotificationMiddleware> _logger;

        public SystemErrorNotificationMiddleware(RequestDelegate next, ILogger<SystemErrorNotificationMiddleware> logger)
            {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        public async Task InvokeAsync(HttpContext context, SystemErrorMonitor monitor)
            {
            try
                {
                await _next(context);
                if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
                    {
                    var record = await monitor.ReportStatusCodeAsync(context, context.Response.StatusCode);
                    _logger.LogError("HTTP {StatusCode} system error persisted as {Reference}.", context.Response.StatusCode, record.ErrorReference);
                    }
                }
            catch (Exception ex)
                {
                SystemErrorRecord record = null;
                try
                    {
                    record = await monitor.ReportAsync(ex, context);
                    }
                catch (Exception monitorException)
                    {
                    _logger.LogError(monitorException, "System error monitoring failed while preserving original exception.");
                    }

                _logger.LogError(ex, "Unhandled system error persisted as {Reference}.", record?.ErrorReference ?? "IAS-ERR-UNKNOWN");

                if (context.Response.HasStarted)
                    {
                    throw;
                    }

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                if (LoginRedirectHelper.IsApiRequest(context.Request))
                    {
                    context.Response.ContentType = "application/json";
                    var payload = JsonSerializer.Serialize(new
                        {
                        error = "server_error",
                        message = SystemErrorMonitor.BuildSafeUserMessage(record),
                        reference = record?.ErrorReference ?? "IAS-ERR-UNKNOWN"
                        });
                    await context.Response.WriteAsync(payload);
                    return;
                    }

                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(SystemErrorMonitor.BuildSafeUserMessage(record));
                }
            }
        }
    }
