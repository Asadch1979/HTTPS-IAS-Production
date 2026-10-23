using AIS;
using AIS.Controllers;
using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

static class HeadAuditAccessChecks
{
    sealed class MemorySession : ISession
    {
        readonly Dictionary<string, byte[]> items = new();
        public bool IsAvailable => true;
        public string Id => "head-audit-test";
        public IEnumerable<string> Keys => items.Keys;
        public void Clear() => items.Clear();
        public void Remove(string key) => items.Remove(key);
        public void Set(string key, byte[] value) => items[key] = value;
        public bool TryGetValue(string key, out byte[] value) => items.TryGetValue(key, out value);
        public Task LoadAsync(CancellationToken token = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken token = default) => Task.CompletedTask;
    }
    sealed class Permissions : IPermissionService
    {
        public void EnsurePermissionsCached(SessionUser user) { }
        public void PreloadPermissions(SessionUser user) { }
        public bool HasViewPermission(SessionUser user, int id) => id == HeadAuditMonitoringPolicy.PageId;
        public bool HasApiPermissionForPath(SessionUser user, string method, string pathBase, string path) => false;
        public bool HasPermissionToExecuteAction(SessionUser user, string id) => false;
        public bool HasRole(SessionUser user, string code) => false;
    }
    public static void Run()
    {
        int Request(bool signedIn = true, bool page = true, bool revoked = false, int? entity = 9, bool api = false, string tab = "progress")
        {
            var context = new DefaultHttpContext { Session = new MemorySession() };
            context.Request.Method = "GET";
            context.Request.Path = api ? "/api/HeadAuditDepartment/Engagements" : "/HeadAuditDepartment/AuditTeamsMonitoring";
            var session = new SessionHandler(new HttpContextAccessor { HttpContext = context }, new ConfigurationBuilder().Build(), null);
            if (signedIn) session.SetUser(new SessionUser { ID=999999, PPNumber="head-test-"+Guid.NewGuid(), UserRoleID=5, UserEntityID=entity,
                IPAddress="127.0.0.1", MACAddress="test", FirstMACCardAddress="test" });
            context.Session.SetString(SessionKeys.AllowedViewIds, page ? "[1000401]" : "[]");
            if (revoked) context.Session.Remove(SessionKeys.SessionStamp);
            // Null DB and menus prove a successful cached-tab authorization probe does no data access.
            var controller = new HeadAuditDepartmentController(null, session, new Permissions(), null, NullLogger<HeadAuditDepartmentController>.Instance) {
                ControllerContext = new ControllerContext { HttpContext = context }
            };
            var result = api ? controller.Engagements() : controller.AuditTeamsMonitoring(tab:tab, checkOnly:true);
            return result is StatusCodeResult status ? status.StatusCode : throw new Exception("Unexpected result");
        }
        if (Request(signedIn:false)!=401 || Request(page:false)!=403 || Request(revoked:true)!=401 || Request(entity:null)!=403 || Request(api:true)!=403 || Request(tab:"review")!=400)
            throw new Exception("Head Audit controller gates regressed.");
        foreach (var tab in new[] { "progress", "details", "exceptions", "register" }) if (Request(tab:tab)!=204) throw new Exception("Authorization probe queried Oracle or rejected an allowed tab.");
        var user = new SessionUser { ID=1, UserRoleID=5, UserEntityID=9 };
        var first = HeadAuditDisplayContext.Key(user);
        user.UserEntityID=10;
        if (HeadAuditDisplayContext.Key(user)==first) throw new Exception("Assignment changes must invalidate cached display context.");
        Console.WriteLine("PASS: 10 compiled Head Audit controller/session/page/API/tab gates, Oracle-free cache probes and context invalidation.");
    }
}
