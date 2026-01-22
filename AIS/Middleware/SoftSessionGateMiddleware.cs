using System;
using System.Threading.Tasks;
using AIS.Services;
using Microsoft.AspNetCore.Http;

namespace AIS.Middleware
{
    public class SoftSessionGateMiddleware
    {
        private readonly RequestDelegate _next;

        public SoftSessionGateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var pathBase = context.Request.PathBase.HasValue ? context.Request.PathBase.Value : string.Empty;
            var path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
            var fullPath = (pathBase + path).ToLowerInvariant();

            if (IsBypassed(context.Request, fullPath))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Cookies["IAS_SESSION"];
            if (string.IsNullOrWhiteSpace(token))
            {
                if (LoginRedirectHelper.IsApiRequest(context.Request))
                {
                    await LoginRedirectHelper.WriteUnauthorizedAsync(context);
                    return;
                }

                LoginRedirectHelper.RedirectToLogin(context);
                return;
            }

            await _next(context);
        }

        private static bool IsBypassed(HttpRequest request, string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return true;
            }

            if (IsWhitelistedPath(fullPath))
            {
                return true;
            }

            if (fullPath.Contains("/apicalls", StringComparison.Ordinal))
            {
                return true;
            }

            if (PageIdPathHelper.IsExempt(request))
            {
                return true;
            }

            if (fullPath.Contains("/css", StringComparison.Ordinal) ||
                fullPath.Contains("/js", StringComparison.Ordinal) ||
                fullPath.Contains("/lib", StringComparison.Ordinal) ||
                fullPath.Contains("/images", StringComparison.Ordinal) ||
                fullPath.Contains("/favicon", StringComparison.Ordinal))
            {
                return true;
            }

            if (fullPath.Contains("/error", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        private static bool IsWhitelistedPath(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return false;
            }

            return fullPath.Equals("/login/index", StringComparison.OrdinalIgnoreCase) ||
                   fullPath.Equals("/login/dologin", StringComparison.OrdinalIgnoreCase) ||
                   fullPath.Equals("/home/change_password", StringComparison.OrdinalIgnoreCase) ||
                   fullPath.Equals("/home/dochangepassword", StringComparison.OrdinalIgnoreCase);
        }
    }
}
