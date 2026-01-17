using System;
using System.Text.Json;
using System.Threading.Tasks;
using AIS.Session;
using Microsoft.AspNetCore.Http;

namespace AIS.Middleware
{
    public class ForcePasswordChangeMiddleware
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;

        public ForcePasswordChangeMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context?.User?.Identity?.IsAuthenticated == true)
            {
                var mustChangePassword = context.Session?.GetString(SessionKeys.MustChangePassword);
                if (string.Equals(mustChangePassword, "1", StringComparison.Ordinal))
                {
                    if (IsAllowedPath(context.Request))
                    {
                        await _next(context);
                        return;
                    }

                    if (IsAjaxRequest(context.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var payload = JsonSerializer.Serialize(new { status = false, message = "Password change required" }, SerializerOptions);
                        await context.Response.WriteAsync(payload);
                        return;
                    }

                    context.Response.Redirect(BuildChangePasswordPath(context.Request));
                    return;
                }
            }

            await _next(context);
        }

        private static bool IsAllowedPath(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }

            if (IsStaticAssetRequest(request.Path))
            {
                return true;
            }

            var path = request.Path.Value ?? string.Empty;
            if (path.Equals("/Home/Change_Password", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/Home/DoChangePassword", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/Login/Logout", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsStaticAssetRequest(PathString path)
        {
            return path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWithSegments("/favicon", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            var requestedWith = request?.Headers["X-Requested-With"].ToString();
            return string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildChangePasswordPath(HttpRequest request)
        {
            var pathBase = request?.PathBase.HasValue == true ? request.PathBase.Value : string.Empty;
            return string.Concat(pathBase, "/Home/Change_Password");
        }
    }
}
