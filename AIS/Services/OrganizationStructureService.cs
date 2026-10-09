using AIS.Controllers;
using AIS.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Services
{
    public class OrganizationStructureService
    {
        private readonly DBConnection database;
        private readonly SessionHandler session;
        private readonly IMemoryCache cache;
        private readonly int cacheSeconds;
        private static readonly object HierarchyLock = new object();
        private static CancellationTokenSource hierarchyChange = new CancellationTokenSource();

        public OrganizationStructureService(DBConnection database, SessionHandler session, IMemoryCache cache, IConfiguration configuration)
        {
            this.database = database;
            this.session = session;
            this.cache = cache;
            cacheSeconds = Math.Clamp(configuration.GetValue<int?>("OrganizationStructure:CacheSeconds") ?? 15, 1, 15);
        }

        public OrganizationMovePreview PreviewMove(int entityId, int newParentId)
        {
            RequireSuperUser();
            return database.PreviewOrganizationMove(entityId, newParentId);
        }

        public string Move(OrganizationMoveRequest move)
        {
            RequireSuperUser();
            var moveId = database.MoveOrganizationEntity(move, session.GetUserOrThrow().PPNumber);
            // The procedure has committed before invalidating every user's scoped entry.
            lock (HierarchyLock)
            {
                var previous = hierarchyChange;
                hierarchyChange = new CancellationTokenSource();
                previous.Cancel();
                previous.Dispose();
            }
            return moveId;
        }

        private void RequireSuperUser()
        {
            session.GetUserOrThrow();
            if (!session.IsSuperUser()) throw new UnauthorizedAccessException();
        }

        public OrganizationSnapshot GetSnapshot(bool refresh = false)
        {
            var user = session.GetUserOrThrow();
            var fullAccess = session.IsSuperUser();
            // No client-supplied root or role can widen this scope. Non-super users
            // are limited to their active IAS posting and its package-defined descendants.
            var key = $"org:{user.SessionId}:{user.ID}:{user.UserRoleID}:{user.UserEntityID}:{user.UserContextAssignmentId}:{fullAccess}";
            CancellationToken changeToken;
            lock (HierarchyLock) changeToken = hierarchyChange.Token;
            if (refresh) cache.Remove(key);
            return cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheSeconds);
                entry.AddExpirationToken(new CancellationChangeToken(changeToken));
                int? root = fullAccess ? null : user.UserEntityID;
                var nodes = database.GetOrganizationNodes(root).GroupBy(n => n.EntityId).Select(g => g.First()).ToList();
                var permitted = nodes.Select(n => n.EntityId).ToHashSet();
                if (!fullAccess)
                {
                    foreach (var node in nodes)
                    {
                        if (!permitted.Contains(node.ParentEntityId ?? -1))
                        {
                            node.ParentEntityId = null;
                            node.ParentEntityName = null;
                        }
                        node.Reporting1 = WithinScope(node.Reporting1, permitted);
                        node.Reporting2 = WithinScope(node.Reporting2, permitted);
                        node.Reporting3 = WithinScope(node.Reporting3, permitted);
                        node.Reporting4 = WithinScope(node.Reporting4, permitted);
                        node.Reporting5 = WithinScope(node.Reporting5, permitted);
                        node.Reporting6 = WithinScope(node.Reporting6, permitted);
                    }
                }
                return new OrganizationSnapshot { Nodes = nodes, RootEntityId = root, IsRestricted = !fullAccess };
            });
        }

        public List<OrganizationOpenParaCounts> GetOpenParaCounts()
        {
            // GetSnapshot validates the active session and derives the server-only scope.
            // Only the hierarchy is cached; every request executes the counts procedure once.
            var snapshot = GetSnapshot();
            var permitted = snapshot.Nodes.Select(n => n.EntityId).ToHashSet();
            return database.GetOrganizationOpenParaCounts(snapshot.RootEntityId)
                .Where(count => permitted.Contains(count.EntityId))
                .GroupBy(count => count.EntityId).Select(group => group.First()).ToList();
        }

        public List<OrganizationPathEntry> GetPath(int entityId)
        {
            var snapshot = GetSnapshot();
            var permitted = snapshot.Nodes.Select(n => n.EntityId).ToHashSet();
            if (!permitted.Contains(entityId)) return null;
            var path = database.GetOrganizationEntityPath(entityId)
                .Where(p => permitted.Contains(p.EntityId)).GroupBy(p => p.EntityId).Select(g => g.First()).ToList();
            if (snapshot.IsRestricted)
            {
                foreach (var item in path)
                {
                    if (!permitted.Contains(item.ParentEntityId ?? -1))
                    {
                        item.ParentEntityId = null;
                        item.ParentEntityName = null;
                    }
                }
            }
            return path;
        }

        private static int? WithinScope(int? id, HashSet<int> permitted) =>
            id.HasValue && permitted.Contains(id.Value) ? id : null;
    }
}
