using System;
using AIS.Models;
using AIS;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using AIS.Controllers;

namespace AIS.Services
    {
    public class PermissionService : IPermissionService
        {
        private readonly DBConnection _dbConnection;
        private readonly SessionHandler _sessionHandler;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(DBConnection dbConnection, SessionHandler sessionHandler, ILogger<PermissionService> logger)
            {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        public void EnsurePermissionsCached(SessionUser user)
            {
            if (user == null)
                {
                return;
                }

            var hasViewIds = _sessionHandler.TryGetAllowedViewIds(out _);
            var hasApiMap = _sessionHandler.HasCachedApiPermissions();

            if (hasViewIds && hasApiMap)
                {
                return;
                }

            try
                {
                if (!hasViewIds)
                    {
                    var menuPages = _dbConnection.GetTopMenuPages(user) ?? new List<MenuPagesModel>();
                    _sessionHandler.CacheMenuPages(menuPages);
                    _sessionHandler.TryGetAllowedViewIds(out _);
                    }

                if (!hasApiMap)
                    {
                    var apiPermissions = _dbConnection.GetApiPermissions(user) ?? new List<ApiPermissionModel>();
                    _sessionHandler.CacheApiPermissions(apiPermissions);
                    if (!_sessionHandler.HasCachedApiPermissions())
                        {
                        _logger.LogWarning("API permission map is missing from session for user {UserId}. API authorization will allow by default until cache is rebuilt.", user?.ID);
                        }
                    }
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Failed to rebuild permission cache for user {UserId}. Authorization will allow by default until cache is rebuilt.", user?.ID);
                }
            }

        public void PreloadPermissions(SessionUser user)
            {
            if (user == null)
                {
                return;
                }

            try
                {
                var menuPages = _dbConnection.GetTopMenuPages(user) ?? new List<MenuPagesModel>();
                _sessionHandler.CacheMenuPages(menuPages);

                var apiPermissions = _dbConnection.GetApiPermissions(user) ?? new List<ApiPermissionModel>();
                _sessionHandler.CacheApiPermissions(apiPermissions);
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Failed to preload permissions for user {UserId}. Authorization will allow by default until cache is rebuilt.", user?.ID);
                }
            }

        public bool HasViewPermission(SessionUser user, int pageId)
            {
            if (_sessionHandler.IsSuperUser())
                {
                return true;
                }

            if (pageId <= 0)
                {
                _logger.LogWarning("Page identifier missing while evaluating view permissions.");
                return false;
                }

            EnsurePermissionsCached(user);

            if (!_sessionHandler.TryGetAllowedViewIds(out var allowedViewIds))
                {
                _logger.LogWarning("Allowed view identifiers are unavailable in session.");
                return false;
                }

            return allowedViewIds.Contains(pageId);
            }

        public bool HasApiPermissionForPath(SessionUser user, string method, string pathBase, string path)
            {
            if (_sessionHandler.IsSuperUser())
                {
                return true;
                }

            EnsurePermissionsCached(user);

            return _sessionHandler.HasApiPermissionForPath(method, pathBase, path);
            }

        public bool HasPermissionToExecuteAction(SessionUser user, string actionId)
            {
            if (!int.TryParse(actionId, out var pageId))
                {
                return false;
                }

            return HasViewPermission(user, pageId);
            }

        public bool HasRole(SessionUser user, string roleCode)
            {
            if (user == null)
                {
                throw new ArgumentNullException(nameof(user));
                }

            if (string.IsNullOrWhiteSpace(roleCode))
                {
                return false;
                }

            if (!string.IsNullOrWhiteSpace(user.UserRoleName) &&
                string.Equals(user.UserRoleName, roleCode, StringComparison.OrdinalIgnoreCase))
                {
                return true;
                }

            if (int.TryParse(roleCode, out var roleId))
                {
                return user.UserRoleID == roleId;
                }

            return false;
            }

        }
    }
