using AIS.Models;

namespace AIS.Services
    {
        public interface IPermissionService
        {
        void EnsurePermissionsCached(SessionUser user);
        void PreloadPermissions(SessionUser user);
        bool HasViewPermission(SessionUser user, int pageId);
        bool HasApiPermissionForPath(SessionUser user, string method, string pathBase, string path);
        bool HasPermissionToExecuteAction(SessionUser user, string actionId);
        bool HasRole(SessionUser user, string roleCode);
        }
    }
