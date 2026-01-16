using System;
using System.Linq;
using AIS;
using AIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AIS.Filters
    {
    public class PageKeyPermissionFilter : IActionFilter
        {
        private readonly PageKeyPermissionGuard _permissionGuard;
        private readonly SessionHandler _sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;

        public PageKeyPermissionFilter(PageKeyPermissionGuard permissionGuard, SessionHandler sessionHandler, IPermissionService permissionService, IPageIdResolver pageIdResolver)
            {
            _permissionGuard = permissionGuard ?? throw new ArgumentNullException(nameof(permissionGuard));
            _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _pageIdResolver = pageIdResolver ?? throw new ArgumentNullException(nameof(pageIdResolver));
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

            var httpContext = context.HttpContext;

            if (PageIdPathHelper.IsViewPageRequest(httpContext.Request))
                {
                if (PageIdPathHelper.IsExempt(httpContext.Request))
                    {
                    _sessionHandler.SetPageId(0);
                    if (context.Controller is Controller exemptController)
                        {
                        exemptController.ViewData["PageId"] = 0;
                        }
                    }
                else
                    {
                    var pageId = _pageIdResolver.ResolvePageId(httpContext);
                    _sessionHandler.SetPageId(pageId);

                    if (context.Controller is Controller controller)
                        {
                        controller.ViewData["PageId"] = pageId;
                        }
                    }
                }

            if (_sessionHandler.TryGetUser(out var user))
                {
                _permissionService.EnsurePermissionsCached(user);
                }

            if (!_permissionGuard.TryAuthorize(httpContext, out var permissionResult))
                {
                context.Result = permissionResult;
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
