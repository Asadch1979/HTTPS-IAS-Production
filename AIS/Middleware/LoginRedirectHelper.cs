using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AIS.Middleware
{
    public static class LoginRedirectHelper
    {
        private const string LoginPathSuffix = "/Login/Index";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string BuildLoginRedirectUrl(HttpContext context)
        {
            var loginPath = BuildLoginPath(context);
            var returnUrl = BuildReturnUrl(context);

            return string.Concat(loginPath, "?ReturnUrl=", WebUtility.UrlEncode(returnUrl));
        }

        public static string BuildLoginPath(HttpContext context)
        {
            var pathBase = context?.Request?.PathBase.HasValue == true
                ? context.Request.PathBase.Value
                : string.Empty;

            return string.Concat(pathBase, LoginPathSuffix);
        }

        public static string BuildReturnUrl(HttpContext context)
        {
            if (context?.Request == null)
            {
                return string.Empty;
            }

            var request = context.Request;
            var pathBase = request.PathBase.HasValue ? request.PathBase.Value : string.Empty;
            var path = request.Path.HasValue ? request.Path.Value : string.Empty;
            var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;

            return string.Concat(pathBase, path, query);
        }

        public static bool IsApiRequest(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }

            var pathBase = request.PathBase.HasValue ? request.PathBase.Value : string.Empty;
            var path = request.Path.HasValue ? request.Path.Value : string.Empty;
            var combinedPath = string.Concat(pathBase, path);

            if (combinedPath.StartsWith("/apicalls", StringComparison.OrdinalIgnoreCase) ||
                combinedPath.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }

            var requestedWith = request.Headers["X-Requested-With"].ToString();
            return string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        public static void RedirectToLogin(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var redirectUrl = BuildLoginRedirectUrl(context);
            context.Response.Redirect(redirectUrl);
        }

        public static async Task WriteUnauthorizedAsync(HttpContext context)
        {
            await WriteJsonAsync(context, StatusCodes.Status401Unauthorized, "unauthenticated", "Session missing or expired.");
        }

        public static async Task WriteForbiddenAsync(HttpContext context)
        {
            await WriteJsonAsync(context, StatusCodes.Status403Forbidden, "forbidden", "You do not have access to this resource.");
        }

        private static async Task WriteJsonAsync(HttpContext context, int statusCode, string error, string message)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new { error, message }, SerializerOptions);
            await context.Response.WriteAsync(payload);
        }
    }
}
