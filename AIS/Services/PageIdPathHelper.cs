using System;
using System.Collections.Generic;
using AIS.Middleware;
using Microsoft.AspNetCore.Http;

namespace AIS.Services
    {
    public static class PageIdPathHelper
        {
        private static readonly HashSet<string> ExemptPaths = new(StringComparer.OrdinalIgnoreCase)
            {
            "/Home",
            "/Home/Index",
            "/Home/Logout",
            "/Home/KeepAlive",
            "/Home/DoChangePassword",
            "/Home/Change_Password",
            "/Login/DoLogin",
            "/Login/Index",
            "/Login/Index_Dev",
            "/Login/Maintenance"
            };

        public static bool IsExempt(HttpRequest request)
            {
            if (request == null)
                {
                return false;
                }

            return IsExempt(request.Path.Value);
            }

        public static bool IsExempt(string path)
            {
            var normalized = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(normalized))
                {
                return false;
                }

            return ExemptPaths.Contains(normalized);
            }

        public static bool IsViewPageRequest(HttpRequest request)
            {
            if (request == null)
                {
                return false;
                }

            if (!(HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method)))
                {
                return false;
                }

            if (LoginRedirectHelper.IsApiRequest(request))
                {
                return false;
                }

            if (IsAjaxRequest(request))
                {
                return false;
                }

            return AcceptsHtml(request);
            }

        public static string NormalizePath(string path)
            {
            if (string.IsNullOrWhiteSpace(path))
                {
                return string.Empty;
                }

            var normalized = path.Trim();
            var queryIndex = normalized.IndexOf('?', StringComparison.Ordinal);
            if (queryIndex >= 0)
                {
                normalized = normalized.Substring(0, queryIndex);
                }

            normalized = normalized.Replace('\\', '/');
            if (!normalized.StartsWith("/", StringComparison.Ordinal))
                {
                normalized = "/" + normalized;
                }

            return normalized.TrimEnd('/');
            }

        private static bool IsAjaxRequest(HttpRequest request)
            {
            var requestedWith = request.Headers["X-Requested-With"].ToString();
            return string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            }

        private static bool AcceptsHtml(HttpRequest request)
            {
            var accept = request.Headers["Accept"].ToString();
            if (!string.IsNullOrWhiteSpace(accept) &&
                (accept.Contains("text/html", StringComparison.OrdinalIgnoreCase) ||
                 accept.Contains("application/xhtml+xml", StringComparison.OrdinalIgnoreCase)))
                {
                return true;
                }

            var fetchMode = request.Headers["Sec-Fetch-Mode"].ToString();
            return string.Equals(fetchMode, "navigate", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
