// Executes against the already-built AIS.dll. No Oracle connection or app host.
// A session-only DBConnection shell prevents directory/procedure calls: each
// check must terminate at the real controller's session/role/page/API/date gate.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AIS;
using AIS.Controllers;
using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

public static class AuditParaSnapAccessChecks
{
    sealed class MemorySession : ISession
    {
        readonly Dictionary<string,byte[]> items=new Dictionary<string,byte[]>();
        public bool IsAvailable => true;
        public string Id => "snap-test";
        public IEnumerable<string> Keys => items.Keys;
        public void Clear() => items.Clear();
        public void Remove(string key) => items.Remove(key);
        public void Set(string key,byte[] value) => items[key]=value;
        public bool TryGetValue(string key,out byte[] value) => items.TryGetValue(key,out value);
        public Task LoadAsync(CancellationToken token=default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken token=default) => Task.CompletedTask;
    }
    sealed class Permissions : IPermissionService
    {
        public bool Page,Api;
        public void EnsurePermissionsCached(SessionUser user) {}
        public void PreloadPermissions(SessionUser user) {}
        public bool HasViewPermission(SessionUser user,int pageId) => Page && pageId==1000304;
        public bool HasApiPermissionForPath(SessionUser user,string method,string pathBase,string path)
            => Api && method=="GET" && path=="/api/AuditParaSnap/GetKpis";
        public bool HasPermissionToExecuteAction(SessionUser user,string id) => false;
        public bool HasRole(SessionUser user,string code) => false;
    }
    sealed class Resolver : IPageIdResolver
    {
        public int Id=1000304;
        public int ResolvePageId(HttpContext context) => Id;
        public int ResolvePageId(string path) => Id;
        public bool TryResolvePageId(HttpContext context,out int id) { id=Id;return Id>0; }
        public bool TryResolvePageId(string path,out int id) { id=Id;return Id>0; }
    }
    public static string Run()
    {
        var count=0;
        Action<int,int> check=(actual,expected) => {
            if (actual!=expected) throw new Exception("Expected HTTP "+expected+"; got "+actual);
            count++;
        };
        int request(int role,bool superFlag,bool pageGrant,bool apiGrant,int resolvedId,
            bool revoked=false,string method="GET",string path="/api/AuditParaSnap/GetKpis")
        {
            var context=new DefaultHttpContext { Session=new MemorySession() };
            context.Request.Method=method;
            context.Request.PathBase="/ZTBLAIS";
            context.Request.Path=path;
            context.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,"snap-test")},"test"));
            var configuration=new ConfigurationBuilder().Build();
            var session=new SessionHandler(new HttpContextAccessor { HttpContext=context },configuration,null);
            if (role>0)
            {
                session.SetUser(new SessionUser { ID=999999,PPNumber="snap-regression-"+Guid.NewGuid(),UserRoleID=role,
                    UserEntityID=1,IPAddress="127.0.0.1",MACAddress="test",FirstMACCardAddress="test" });
                context.Session.SetString(SessionKeys.IsSuperUser,superFlag ? "Y" : "N");
            }
            context.Session.SetString(SessionKeys.AllowedViewIds,pageGrant ? "[1000304]" : "[]");
            if (revoked) context.Session.Remove(SessionKeys.SessionStamp);
            session.CacheApiPermissions(apiGrant ? new[]{new ApiPermissionModel {
                ApiPath="/api/AuditParaSnap/GetKpis",HttpMethod="GET" }} : Array.Empty<ApiPermissionModel>());
            var database=(DBConnection)RuntimeHelpers.GetUninitializedObject(typeof(DBConnection));
            typeof(DBConnection).GetField("sessionHandler",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(database,session);
            var controller=new AuditParaSnapController(database,session,new Permissions { Page=pageGrant,Api=apiGrant },
                new Resolver { Id=resolvedId },configuration,NullLogger<AuditParaSnapController>.Instance,null);
            controller.ControllerContext=new ControllerContext { HttpContext=context };
            // Missing dates must be rejected before all directory/Oracle access.
            var result=path.EndsWith("/GetPerformance",StringComparison.Ordinal)
                ? controller.GetPerformance(new ExecutiveDashboardFilterRequest { View="position" })
                : controller.GetKpis(new ExecutiveDashboardFilterRequest { View="glance" });
            return ((ObjectResult)result).StatusCode ?? 0;
        }
        check(request(0,false,false,false,1000304),401);
        foreach (var role in new[]{38,6,7,15,2,39}) check(request(role,false,true,true,1000304),403);
        check(request(1,false,true,true,1000304),403); // role/flag mismatch
        check(request(3,false,false,true,1000304),403);
        check(request(3,false,true,false,1000304),403);
        check(request(3,false,true,true,1000303),403); // CIA page cannot replace Snap grant
        check(request(1,true,false,true,1000304),403);
        check(request(1,true,true,false,1000304),403); // no Super User API bypass
        check(request(3,false,true,true,1000304),400); // passed auth, dates rejected
        check(request(1,true,true,true,1000304),400);
        check(request(3,false,true,true,1000304,revoked:true),401);
        check(request(3,false,true,true,1000304,method:"POST"),403);
        check(request(3,false,true,true,1000304,path:"/api/AuditParaSnap/GetCriticalItems"),403);
        check(request(3,false,true,true,1000304,path:"/api/AuditParaSnap/GetPerformance"),403);
        check(request(1,true,true,true,1000304,path:"/api/AuditParaSnap/GetPerformance"),403);
        return count+" compiled-controller access checks passed (no Oracle).";
    }
}
