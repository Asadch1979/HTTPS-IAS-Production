using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AIS.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IDBConnection db)
        {
            var token = context.Request.Cookies["IAS_SESSION"];

            var path = context.Request.Path.Value?.ToLower();
            if ((path?.StartsWith("/css") ?? false) ||
                (path?.StartsWith("/js") ?? false) ||
                (path?.StartsWith("/lib") ?? false) ||
                (path?.StartsWith("/images") ?? false) ||
                (path?.StartsWith("/favicon") ?? false))
            {
                await _next(context);
                return;
            }
            if (IsPreSessionPath(path))
            {
                await _next(context);
                return;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                if (LoginRedirectHelper.IsApiRequest(context.Request) ||
                    LoginRedirectHelper.IsAjaxRequest(context.Request))
                {
                    await LoginRedirectHelper.WriteUnauthorizedAsync(context);
                    return;
                }

                LoginRedirectHelper.RedirectToLogin(context);
                return;
            }

            bool isValid = db.IsSessionValid(token);
            if (!isValid)
            {
                if (LoginRedirectHelper.IsApiRequest(context.Request) ||
                    LoginRedirectHelper.IsAjaxRequest(context.Request))
                {
                    await LoginRedirectHelper.WriteUnauthorizedAsync(context);
                    return;
                }

                LoginRedirectHelper.RedirectToLogin(context);
                return;
            }

            await _next(context);
        }

        private static bool IsPreSessionPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return path.Equals("/login/index", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/login/dologin", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/login/resetpassword", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/login/killsession", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/home/change_password", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/home/dochangepassword", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/login/logout", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/login/index_dev", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/login/maintenance", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/api/auth", StringComparison.OrdinalIgnoreCase);
        }
    }
}
