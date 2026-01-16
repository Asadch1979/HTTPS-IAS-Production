using System;
using AIS.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AIS.Services
    {
    public class PageKeyPermissionGuard
        {
        private readonly SessionHandler _sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PageKeyPermissionGuard> _logger;

        public PageKeyPermissionGuard(
            SessionHandler sessionHandler,
            IPermissionService permissionService,
            ILogger<PageKeyPermissionGuard> logger)
            {
            _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        public bool TryAuthorize(HttpContext httpContext, out IActionResult result)
            {
            result = null;

            if (httpContext == null)
                {
                return true;
                }

            _sessionHandler.TryGetUser(out var user);

            // No session user → let higher layers handle redirect
            if (user == null)
                {
                _logger.LogWarning(
                    "Permission check bypassed for path {Path} because session user is missing.",
                    httpContext.Request?.Path);
                return true;
                }

            // Super user bypass
            if (_sessionHandler.IsSuperUser())
                {
                _logger.LogDebug(
                    "Granting super-user bypass for path {Path}.",
                    httpContext.Request?.Path);
                return true;
                }

            // -----------------------------
            // API REQUEST AUTHORIZATION
            // -----------------------------
            var isApiRequest = LoginRedirectHelper.IsApiRequest(httpContext.Request);
            _logger.LogDebug(
                "API request classification for {Method} {PathBase}{Path}: {IsApiRequest}.",
                httpContext.Request?.Method,
                httpContext.Request?.PathBase.Value,
                httpContext.Request?.Path.Value,
                isApiRequest);

            if (isApiRequest)
                {
                if (IsApiPermissionExempt(httpContext.Request))
                    {
                    _logger.LogDebug(
                        "Bypassing API permission check for exempt path {Path}.",
                        httpContext.Request?.Path);
                    return true;
                    }

                _permissionService.EnsurePermissionsCached(user);

                var hasApiPermission = _permissionService.HasApiPermissionForPath(
                    user,
                    httpContext.Request?.Method,
                    httpContext.Request?.PathBase.Value,
                    httpContext.Request?.Path.Value);

                if (!hasApiPermission)
                    {
                    _logger.LogWarning(
                        "Permission denied for user {User} on API {Method} {PathBase}{Path}.",
                        user?.PPNumber ?? user?.ID.ToString(),
                        httpContext.Request?.Method,
                        httpContext.Request?.PathBase.Value,
                        httpContext.Request?.Path.Value);

                    result = new JsonResult(new
                        {
                        error = "forbidden",
                        message = "You don't have access."
                        })
                        {
                        StatusCode = StatusCodes.Status403Forbidden
                        };

                    return false;
                    }

                return true;
                }

            // -----------------------------
            // VIEW REQUEST AUTHORIZATION
            // -----------------------------

            if (PageIdPathHelper.IsExempt(httpContext.Request))
                {
                _logger.LogDebug(
                    "Bypassing PAGE_ID requirement for exempt path {Path}.",
                    httpContext.Request?.Path);
                return true;
                }

            if (!PageIdPathHelper.IsViewPageRequest(httpContext.Request))
                {
                return true;
                }

            _permissionService.EnsurePermissionsCached(user);

            var pageId = _sessionHandler.GetPageId();
            if (pageId <= 0)
                {
                _logger.LogWarning(
                    "Rejecting view request for {Method} {PathBase}{Path} because PAGE_ID is missing.",
                    httpContext.Request?.Method,
                    httpContext.Request?.PathBase.Value,
                    httpContext.Request?.Path.Value);

                result = new ContentResult
                    {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Content = "You don't have access.",
                    ContentType = "text/plain"
                    };

                return false;
                }

            var hasViewPermission = _permissionService.HasViewPermission(user, pageId);
            if (!hasViewPermission)
                {
                _logger.LogWarning(
                    "Permission denied for user {User} on view {Method} {PathBase}{Path} with PAGE_ID {PageId}.",
                    user?.PPNumber ?? user?.ID.ToString(),
                    httpContext.Request?.Method,
                    httpContext.Request?.PathBase.Value,
                    httpContext.Request?.Path.Value,
                    pageId);

                result = new ContentResult
                    {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Content = "You don't have access.",
                    ContentType = "text/plain"
                    };

            return false;
            }

        return true;
        }

        private static bool IsApiPermissionExempt(HttpRequest request)
            {
            var path = request?.Path.Value ?? string.Empty;
            var method = request?.Method ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
                {
                return false;
                }

            if (path.Equals("/Home/DoChangePassword", StringComparison.OrdinalIgnoreCase) &&
                HttpMethods.IsPost(method))
                {
                return true;
                }

            if (path.Equals("/Home/Change_Password", StringComparison.OrdinalIgnoreCase) &&
                HttpMethods.IsGet(method))
                {
                return true;
                }

            if (path.Equals("/Home/Logout", StringComparison.OrdinalIgnoreCase) &&
                (HttpMethods.IsGet(method) || HttpMethods.IsPost(method)))
                {
                return true;
                }

            if (path.Equals("/Home/KeepAlive", StringComparison.OrdinalIgnoreCase) &&
                HttpMethods.IsGet(method))
                {
                return true;
                }

            if (path.Equals("/Login/DoLogin", StringComparison.OrdinalIgnoreCase) &&
                HttpMethods.IsPost(method))
                {
                return true;
                }

            if (path.Equals("/Login/Index", StringComparison.OrdinalIgnoreCase) &&
                HttpMethods.IsGet(method))
                {
                return true;
                }

            if (path.Equals("/Login/Index_Dev", StringComparison.OrdinalIgnoreCase) &&
                HttpMethods.IsGet(method))
                {
                return true;
                }

            if (path.Equals("/Login/Maintenance", StringComparison.OrdinalIgnoreCase) &&
                HttpMethods.IsGet(method))
                {
                return true;
                }

            return false;
            }

        }
    }
