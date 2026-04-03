using System;
using System.Security.Claims;
using System.Text.Json;
using AIS.Controllers;
using AIS.Middleware;
using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIS.Services
    {
    public class PageKeyPermissionGuard
        {
        private readonly SessionHandler _sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection _dbConnection;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<PageKeyPermissionGuard> _logger;

        public PageKeyPermissionGuard(
            SessionHandler sessionHandler,
            IPermissionService permissionService,
            DBConnection dbConnection,
            IHostEnvironment hostEnvironment,
            ILogger<PageKeyPermissionGuard> logger)
            {
            _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
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
            var request = httpContext.Request;
            var fullPath = string.Concat(request?.PathBase.Value ?? string.Empty, request?.Path.Value ?? string.Empty);
            var isAuthenticated = httpContext.User?.Identity?.IsAuthenticated ?? false;
            var claimPpNo = httpContext.User?.FindFirst("PPNO")?.Value
                ?? httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
            var hasUserName = !string.IsNullOrWhiteSpace(httpContext.User?.Identity?.Name)
                || !string.IsNullOrWhiteSpace(claimPpNo)
                || !string.IsNullOrWhiteSpace(user?.PPNumber);

            // No session user → let higher layers handle redirect
            if (user == null)
                {
                WriteUatTrace(
                    user,
                    httpContext,
                    fullPath,
                    isApiPermissionExempt: false,
                    reason: "DeniedBySessionMissing",
                    isAuthenticated: isAuthenticated,
                    hasUserNameOrPpNo: hasUserName);

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
            var isApiPermissionExempt = isApiRequest && IsApiPermissionExempt(httpContext.Request);
            _logger.LogDebug(
                "API request classification for {Method} {PathBase}{Path}: {IsApiRequest}.",
                httpContext.Request?.Method,
                httpContext.Request?.PathBase.Value,
                httpContext.Request?.Path.Value,
                isApiRequest);

            if (isApiRequest)
                {
                WriteUatTrace(
                    user,
                    httpContext,
                    fullPath,
                    isApiPermissionExempt,
                    reason: isApiPermissionExempt ? "AllowedByApiPermissionExempt" : "PendingApiPermission",
                    isAuthenticated: isAuthenticated,
                    hasUserNameOrPpNo: hasUserName);
                }

            if (isApiRequest)
                {
                if (isApiPermissionExempt)
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
                    WriteUatTrace(
                        user,
                        httpContext,
                        fullPath,
                        isApiPermissionExempt,
                        reason: "DeniedByApiPermission",
                        isAuthenticated: isAuthenticated,
                        hasUserNameOrPpNo: hasUserName);

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

            if (path.Equals("/ApiCalls/get_all_public_holidays", StringComparison.OrdinalIgnoreCase) &&
                (HttpMethods.IsGet(method) || HttpMethods.IsPost(method)))
                {
                return true;
                }

            if (path.Equals("/ApiCalls/get_user_name", StringComparison.OrdinalIgnoreCase) &&
                (HttpMethods.IsGet(method) || HttpMethods.IsPost(method)))
                {
                return true;
                }

            if (HttpMethods.IsPost(method) &&
                (path.Equals("/ApiCalls/UploadIidInqStatementFile", StringComparison.OrdinalIgnoreCase) ||
                 path.Equals("/ApiCalls/AddIidInqEvidenceFile", StringComparison.OrdinalIgnoreCase) ||
                 path.Equals("/ApiCalls/DeleteIidInqEvidenceFile", StringComparison.OrdinalIgnoreCase)))
                {
                return true;
                }

            return false;
            }

        private void WriteUatTrace(
            SessionUser user,
            HttpContext context,
            string fullPath,
            bool isApiPermissionExempt,
            string reason,
            bool isAuthenticated,
            bool hasUserNameOrPpNo)
            {
            try
                {
                var entityId = user?.UserEntityID.GetValueOrDefault() ?? 0;
                var roleId = user?.UserRoleID ?? 0;
                var ppNo = int.TryParse(user?.PPNumber, out var parsedPpNo) ? parsedPpNo : 0;
                var pageId = _sessionHandler.GetPageId();

                var payload = JsonSerializer.Serialize(new
                    {
                    Trace = "UAT",
                    Request = new
                        {
                        PathBase = context?.Request?.PathBase.Value ?? string.Empty,
                        Path = context?.Request?.Path.Value ?? string.Empty,
                        FullPath = fullPath
                        },
                    Http = new
                        {
                        Method = context?.Request?.Method ?? string.Empty,
                        Environment = _hostEnvironment.EnvironmentName
                        },
                    Whitelist = new
                        {
                        IsApiPermissionExempt = isApiPermissionExempt
                        },
                    Auth = new
                        {
                        IsAuthenticated = isAuthenticated,
                        HasUserNameOrPpNo = hasUserNameOrPpNo
                        },
                    Reason = reason
                    });

                _dbConnection.AddActivityLog(entityId, roleId, ppNo, pageId, payload);
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Failed to write UAT trace activity log for {Path}.", context?.Request?.Path.Value);
                }
            }

        }
    }
