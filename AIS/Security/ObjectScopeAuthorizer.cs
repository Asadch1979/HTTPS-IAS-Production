using AIS.Controllers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace AIS.Security
    {
    public interface IObjectScopeAuthorizerService
        {
        bool CanAccessEng(long ppno, int pageId, long engId);
        bool CanAccessCom(long ppno, int pageId, long comId);
        }

    public sealed class ObjectScopeAuthorizer : IObjectScopeAuthorizerService
        {
        private const string EngPermissionCacheKey = "ObjectScopeAuthorizer.EngPermissions";
        private const string ComPermissionCacheKey = "ObjectScopeAuthorizer.ComPermissions";

        private readonly DBConnection _dbConnection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ObjectScopeAuthorizer(DBConnection dbConnection, IHttpContextAccessor httpContextAccessor)
            {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            }

        public bool CanAccessEng(long ppno, int pageId, long engId)
            {
            if (ppno <= 0 || pageId <= 0 || engId <= 0)
                {
                return false;
                }

            var permissions = GetOrLoadEngPermissions(ppno);
            return permissions != null && permissions.Contains((pageId, engId));
            }

        public bool CanAccessCom(long ppno, int pageId, long comId)
            {
            if (ppno <= 0 || pageId <= 0 || comId <= 0)
                {
                return false;
                }

            var permissions = GetOrLoadComPermissions(ppno);
            return permissions != null && permissions.Contains((pageId, comId));
            }

        private HashSet<(int pageId, long engId)> GetOrLoadEngPermissions(long ppno)
            {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                {
                return null;
                }

            if (context.Items.TryGetValue(EngPermissionCacheKey, out var cached)
                && cached is PermissionCache<(int pageId, long engId)> cachedPermissions
                && cachedPermissions.Ppno == ppno)
                {
                return cachedPermissions.Permissions;
                }

            var dbPermissions = _dbConnection.GetEngPagePermissionsByPpno(ppno);
            var set = new HashSet<(int pageId, long engId)>();
            if (dbPermissions != null)
                {
                foreach (var permission in dbPermissions)
                    {
                    if (permission.pageId > 0 && permission.engId > 0)
                        {
                        set.Add((permission.pageId, permission.engId));
                        }
                    }
                }

            context.Items[EngPermissionCacheKey] = new PermissionCache<(int pageId, long engId)>(ppno, set);
            return set;
            }

        private HashSet<(int pageId, long comId)> GetOrLoadComPermissions(long ppno)
            {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                {
                return null;
                }

            if (context.Items.TryGetValue(ComPermissionCacheKey, out var cached)
                && cached is PermissionCache<(int pageId, long comId)> cachedPermissions
                && cachedPermissions.Ppno == ppno)
                {
                return cachedPermissions.Permissions;
                }

            var dbPermissions = _dbConnection.GetComPagePermissionsByPpno(ppno);
            var set = new HashSet<(int pageId, long comId)>();
            if (dbPermissions != null)
                {
                foreach (var permission in dbPermissions)
                    {
                    if (permission.pageId > 0 && permission.comId > 0)
                        {
                        set.Add((permission.pageId, permission.comId));
                        }
                    }
                }

            context.Items[ComPermissionCacheKey] = new PermissionCache<(int pageId, long comId)>(ppno, set);
            return set;
            }

        private sealed class PermissionCache<T>
            {
            public PermissionCache(long ppno, HashSet<T> permissions)
                {
                Ppno = ppno;
                Permissions = permissions ?? new HashSet<T>();
                }

            public long Ppno { get; }
            public HashSet<T> Permissions { get; }
            }
        }
    }
