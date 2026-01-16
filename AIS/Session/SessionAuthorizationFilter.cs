using AIS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace AIS.Session
{
    public class SessionAuthorizationFilter : IActionFilter
    {
        private readonly SessionHandler _sessionHandler;

        public SessionAuthorizationFilter(SessionHandler sessionHandler)
        {
            _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (ShouldSkip(context))
            {
                return;
            }

            if (!_sessionHandler.TryGetUser(out _))
            {
                if (Middleware.LoginRedirectHelper.IsApiRequest(context.HttpContext.Request))
                {
                    context.Result = new JsonResult(new { error = "unauthenticated", message = "Session missing or expired." })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }

                var redirectUrl = Middleware.LoginRedirectHelper.BuildLoginRedirectUrl(context.HttpContext);
                context.Result = new RedirectResult(redirectUrl);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private static bool ShouldSkip(ActionExecutingContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                return true;
            }

            return false;
        }
    }
}
