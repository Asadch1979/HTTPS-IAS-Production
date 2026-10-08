using AIS.Controllers;
using AIS.Models;
using Microsoft.Extensions.Caching.Memory;
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

        public OrganizationStructureService(DBConnection database, SessionHandler session, IMemoryCache cache)
        {
            this.database = database;
            this.session = session;
            this.cache = cache;
        }

        public OrganizationSnapshot GetSnapshot()
        {
            var user = session.GetUserOrThrow();
            var fullAccess = session.IsSuperUser();
            // No client-supplied root or role can widen this scope. Non-super users
            // are limited to their active IAS posting and its package-defined descendants.
            var key = $"org:{user.SessionId}:{user.ID}:{user.UserRoleID}:{user.UserEntityID}:{user.UserContextAssignmentId}:{fullAccess}";
            return cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
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
