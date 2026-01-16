using AIS.Session;
using AIS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AIS.Middleware
    {
    public class SessionExceptionHandlingMiddleware
        {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionExceptionHandlingMiddleware> _logger;

        public SessionExceptionHandlingMiddleware(RequestDelegate next, ILogger<SessionExceptionHandlingMiddleware> logger)
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
            catch (Exception ex) when (IsAuthenticationException(ex))
                {
                await HandleUnauthenticatedAsync(context, ex);
                }
            }

        private async Task HandleUnauthenticatedAsync(HttpContext context, Exception exception)
            {
            if (context == null)
                {
                throw new ArgumentNullException(nameof(context));
                }

            _logger.LogWarning(
                "Authentication/authorization exception for {Method} {PathBase}{Path}.",
                context.Request?.Method,
                context.Request?.PathBase.Value,
                context.Request?.Path.Value);

            if (context.Response.HasStarted)
                {
                throw exception;
                }

            _logger.LogWarning(exception, "Request for {Path} failed authentication validation.", context.Request.Path);

            var isForbidden = exception is UnauthorizedAccessException;
            if (LoginRedirectHelper.IsApiRequest(context.Request))
                {
                if (isForbidden)
                    {
                    await LoginRedirectHelper.WriteForbiddenAsync(context);
                    return;
                    }

                await LoginRedirectHelper.WriteUnauthorizedAsync(context);
                return;
                }

            context.Response.Clear();
            if (!context.Response.HasStarted)
                {
                LoginRedirectHelper.RedirectToLogin(context);
                }
            }

        private static bool IsAuthenticationException(Exception exception)
            {
            return exception is UnauthenticatedException
                || exception is SessionMissingException
                || exception is UnauthorizedAccessException;
            }
        }
    }
