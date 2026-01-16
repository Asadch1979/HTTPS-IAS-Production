using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace AIS.Controllers
    {
    public static class PermissionExtensions
        {
        public static bool UserHasPagePermissionForCurrentAction(this Controller controller, SessionHandler sessionHandler)
            {
            if (controller == null)
                {
                throw new ArgumentNullException(nameof(controller));
                }

            if (sessionHandler == null)
                {
                throw new ArgumentNullException(nameof(sessionHandler));
                }

            var httpContext = controller.HttpContext;
            if (httpContext == null)
                {
                return false;
                }

            var permissionService = httpContext.RequestServices.GetService<IPermissionService>();
            var logger = httpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("PermissionExtensions");

            if (permissionService == null)
                {
                logger?.LogWarning("Permission services are unavailable for {Path}.", httpContext.Request?.Path);
                return false;
                }

            if (PageIdPathHelper.IsExempt(httpContext.Request))
                {
                return true;
                }

            if (!sessionHandler.TryGetUser(out var user))
                {
                logger?.LogWarning("Session user missing while checking permissions for {Path}.", httpContext.Request?.Path);
                return false;
                }

            var pageId = sessionHandler.GetPageId();
            var hasPermission = permissionService.HasViewPermission(user, pageId);
            if (!hasPermission)
                {
                logger?.LogWarning("Permission denied for user {UserId} on page {PageId}.", user?.ID, pageId);
                }

            return hasPermission;
            }

        public static IReadOnlyList<string> GetPermissionCandidatesForCurrentAction(this Controller controller)
            {
            // Controller/action derived permission candidates are deprecated. All permission decisions
            // now flow exclusively through the session-cached page identifiers resolved from the database menu.
            // Returning an empty list prevents any downstream consumers from attempting alternative
            // matching strategies.
            return Array.Empty<string>();
            }
        }
    }
