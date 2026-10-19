// Executes against the already-built AIS.dll. No Oracle connection or app host.
// Missing DB configuration ensures no Oracle connection can open. Valid
// requests must reach the real CIA reader even when the legacy flag is absent
// or is explicitly false. Unauthorized requests remain denied first.
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
using Microsoft.Extensions.Logging;

public static class CiaCommandAccessChecks
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
        public bool HasViewPermission(SessionUser user,int pageId) => Page && pageId==1000303;
        public bool HasApiPermissionForPath(SessionUser user,string method,string pathBase,string path)
            => Api && method=="GET" && (path=="/api/ExecutiveDashboard/GetCiaCommandKpis" || path=="/api/ExecutiveDashboard/GetCiaIssueNature");
        public bool HasPermissionToExecuteAction(SessionUser user,string id) => false;
        public bool HasRole(SessionUser user,string code) => false;
    }
    sealed class Resolver : IPageIdResolver
    {
        public int Id=1000303;
        public int ResolvePageId(HttpContext context) => Id;
        public int ResolvePageId(string path) => Id;
        public bool TryResolvePageId(HttpContext context,out int id) { id=Id;return Id>0; }
        public bool TryResolvePageId(string path,out int id) { id=Id;return Id>0; }
    }
    sealed class CaptureLogger : ILogger<ExecutiveDashboardController>
    {
        public Exception LastException;
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level,EventId id,TState state,Exception exception,Func<TState,Exception,string> formatter)
        { if (exception!=null) LastException=exception; }
    }
    public static string Run()
    {
        var count=0;
        Action<int,int> check=(actual,expected) => {
            if (actual!=expected) throw new Exception("Check "+(count+1)+": expected HTTP "+expected+"; got "+actual);
            count++;
        };
        int request(int role,bool superFlag,bool pageGrant,bool apiGrant,int resolvedId,
            bool revoked=false,string method="GET",string path="/api/ExecutiveDashboard/GetCiaCommandKpis",int period=12,
            bool? legacyEnabled=null,bool nature=false)
        {
            var context=new DefaultHttpContext { Session=new MemorySession() };
            context.Request.Method=method;
            context.Request.PathBase="/ZTBLAIS";
            context.Request.Path=nature ? "/api/ExecutiveDashboard/GetCiaIssueNature" : path;
            context.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,"snap-test")},"test"));
            // Deliberately vary a legacy/obsolete setting. The controller
            // must no longer use it as a loading gate.
            var settings=new Dictionary<string,string>();
            if (legacyEnabled.HasValue)
                settings["RoleDashboards:CiaCommandCenterEnabled"]=legacyEnabled.Value.ToString();
            var configuration=new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            var session=new SessionHandler(new HttpContextAccessor { HttpContext=context },configuration,null);
            if (role>0)
            {
                session.SetUser(new SessionUser { ID=999999,PPNumber="snap-regression-"+Guid.NewGuid(),UserRoleID=role,
                    UserEntityID=1,IPAddress="127.0.0.1",MACAddress="test",FirstMACCardAddress="test" });
                context.Session.SetString(SessionKeys.IsSuperUser,superFlag ? "Y" : "N");
            }
            context.Session.SetString(SessionKeys.AllowedViewIds,pageGrant ? "[1000303]" : "[]");
            if (revoked) context.Session.Remove(SessionKeys.SessionStamp);
            session.CacheApiPermissions(apiGrant ? new[]{new ApiPermissionModel {
                ApiPath=nature ? "/api/ExecutiveDashboard/GetCiaIssueNature" : "/api/ExecutiveDashboard/GetCiaCommandKpis",HttpMethod="GET" }} : Array.Empty<ApiPermissionModel>());
            var database=(DBConnection)RuntimeHelpers.GetUninitializedObject(typeof(DBConnection));
            typeof(DBConnection).GetField("sessionHandler",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(database,session);
            // Normal scope resolution needs configuration for unmapped roles.
            // Test DBConnection intentionally has no actual Oracle credentials.
            typeof(DBConnection).GetField("_configuration",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(database,configuration);
            var logger=new CaptureLogger();
            var controller=new ExecutiveDashboardController(database,session,new Permissions { Page=pageGrant,Api=apiGrant },
                new Resolver { Id=resolvedId },logger,configuration);
            controller.ControllerContext=new ControllerContext { HttpContext=context };
            controller.ControllerContext.RouteData=new Microsoft.AspNetCore.Routing.RouteData();
            controller.RouteData.Values["action"]=nature ? "GetCiaIssueNature" : "GetCiaCommandKpis";
            var result=nature ? controller.GetCiaIssueNature(period) : controller.GetCiaCommandKpis(period);
            if ((role==3 || (role==1 && superFlag)) && pageGrant && apiGrant
                && resolvedId==1000303 && !revoked && method=="GET" && period>0
                && (nature || path=="/api/ExecutiveDashboard/GetCiaCommandKpis")) {
                var value=((ObjectResult)result).Value;
                var code=value.GetType().GetProperty("error")?.GetValue(value) as string;
                if (code!="database_unavailable" || logger.LastException==null ||
                    !logger.LastException.ToString().Contains("ReadCiaCommandCursor"))
                    throw new Exception("Authorized CIA request must reach the DB reader regardless of the removed loading flag.");
            }
            if (string.IsNullOrEmpty(context.Response.Headers["X-CIA-Request-Id"]))
                throw new Exception("Missing CIA log correlation header.");
            return ((ObjectResult)result).StatusCode ?? 0;
        }
        check(request(0,false,false,false,1000303),401);
        foreach (var role in new[]{38,6,7,15,2,39}) check(request(role,false,true,true,1000303),403);
        check(request(1,false,true,true,1000303),403); // role/flag mismatch
        check(request(3,false,false,true,1000303),403);
        check(request(3,false,true,false,1000303),403);
        check(request(3,false,true,true,1000304),403); // Snap page cannot replace CIA grant
        check(request(1,true,false,true,1000303),403);
        check(request(1,true,true,false,1000303),403); // no Super User API bypass
        check(request(3,false,true,true,1000303),503); // no legacy flag: real reader reached
        check(request(1,true,true,true,1000303),503);
        check(request(3,false,true,true,1000303,revoked:true),401);
        check(request(3,false,true,true,1000303,method:"POST"),403);
        check(request(3,false,true,true,1000303,path:"/api/ExecutiveDashboard/GetCiaIssueNature"),403);
        check(request(3,false,true,true,1000303,period:0),400);
        check(request(3,false,true,true,1000303,nature:true),503);
        check(request(3,false,true,true,1000303,legacyEnabled:false),503); // obsolete false ignored
        check(request(3,false,true,true,1000303,legacyEnabled:true),503); // obsolete true ignored
        check(request(3,false,true,true,1000303,legacyEnabled:true,nature:true),503);
        return count+" CIA compiled-controller access checks passed (obsolete flag ignored, no Oracle).";
    }
}
