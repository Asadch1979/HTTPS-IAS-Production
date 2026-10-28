using AIS;
using AIS.Controllers;
using AIS.Models;
using AIS.Services;
using AIS.Session;
using AIS.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

var checks=0;
void Check(bool condition, string message) { checks++; if(!condition) throw new Exception(message); }
var definitions=ExecutiveDashboardViews.ForPerspective("CCO");
var visible=definitions.Where(v=>v.Id!="reports").ToArray();
Check(string.Join(",",visible.Select(v=>v.Id))=="overview,org-position,field-office,gm-region,regional-rankings,performance,effectiveness","Seven ordered CCO tabs");
Check(string.Join(",",definitions.Single(v=>v.Id=="reports").Reports)=="B,C,D","CCO report allowlist");
Check(visible[0].UseLifecycleKpis && new[]{"GetKpiLifecycle","GetKpiSummary","GetRiskSummary","GetAgingSummary","GetCycleSummary","GetSettlementTrend","GetCriticalItems"}.All(visible[0].Datasets.Contains),"Overview preserves all existing datasets");
foreach(var tab in new[]{"org-position","performance"}) Check(definitions.Single(v=>v.Id==tab).Datasets.Length==0,"Pending tabs must not fetch invented data");
foreach(var role in new[]{"TOP","GROUP","RLM","CIA"}) Check(!ExecutiveDashboardViews.ForPerspective(role).Any(v=>v.Id=="overview"||v.Id=="field-office"),"CCO navigation isolation");
foreach(var report in new[]{"A","E"}) { try { CcoDashboardPresentation.ResolveLegacy("reports",report); throw new Exception("Unapproved report accepted"); } catch(ArgumentException) { checks++; } }
var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>{{"Security:CauKey","cco-test-only"},{"SecretKey","cco-test-only-not-a-production-secret"}}).Build();
foreach(var pair in new[]{("glance","overview"),("position","overview"),("risk","overview"),("aging","overview"),("settlement","effectiveness"),("reports","field-office")}) {
 var (context,session,db)=Context(38);
 context.Request.QueryString=new QueryString("?report=B");
 var controller=new ExecutiveOverviewController(db,session,new Permissions(),new Pages(),null,NullLogger<ExecutiveOverviewController>.Instance,config){ControllerContext=new ControllerContext{HttpContext=context}};
 var result=controller.CCO(view:pair.Item1,filters:Dates()) as RedirectToActionResult;
 Check(result?.ActionName=="CCO" && (string)result.RouteValues["view"]==pair.Item2,"Authorized legacy redirect "+pair.Item1);
 Check((string)result.RouteValues["startDate"]=="2024-01-01" && (string)result.RouteValues["endDate"]=="2024-02-29","Redirect date preservation");
}
foreach(var report in new[]{"B","C","D","A","E"}) foreach(var role in new[]{38,3,5}) {
 var (context,session,db)=Context(role);
 context.Request.Path="/api/ExecutiveReports/GetReport"+report;
 var controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context}};
 // Missing API grant denies every report before any database query.
 var result=Report(controller,report,Dates());
 Check((result as ObjectResult)?.StatusCode==403,"Missing API or wrong role denied");
 if(role==38 && (report=="A"||report=="E")) {
  controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context}};
  // Role 1 uses the permission-service API path; role 38 uses session grant cache.
  context.Session.SetString(SessionKeys.AllowedApiPaths,"[]");
  Check(!definitions.Single(v=>v.Id=="reports").Reports.Contains(report),"A/E absent from CCO allowlist");
 }
}
foreach(var report in new[]{"A","E","B"}) {
 var (context,session,db)=Context(38);
 context.Request.Path="/api/ExecutiveReports/GetReport"+report;
 context.Session.SetString(SessionKeys.AllowedApiPaths,System.Text.Json.JsonSerializer.Serialize(new[]{"GET:"+context.Request.Path.Value.ToLowerInvariant()}));
 if(report=="B") context.Request.QueryString=new QueryString("?format=pdf");
 var controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context}};
 Check((Report(controller,report,Dates()) as ObjectResult)?.StatusCode==403,"Granted route still denies A/E and CCO PDF");
}
foreach(var denied in new[]{"page","future","root","snap"}) {
 var (context,session,db)=Context(38);
 if(denied=="page") context.Session.SetString(SessionKeys.AllowedViewIds,"[]");
 var f=Dates();if(denied=="future") f.EndDate="2099-01-01";if(denied=="snap") f.SnapDivision="1";
 var controller=new ExecutiveOverviewController(db,session,new Permissions(),new Pages(),null,NullLogger<ExecutiveOverviewController>.Instance,config){ControllerContext=new ControllerContext{HttpContext=context}};
 var result=controller.CCO(groupRootId:denied=="root"?9:null,view:"risk",filters:f);
 Check(((result as StatusCodeResult)?.StatusCode ?? (result as ObjectResult)?.StatusCode)==(denied=="page"||denied=="root"?403:400),"Legacy redirect cannot bypass "+denied);
}
foreach(var invalid in new[]{"field-office","gm-region","regional-rankings"}) {
 var (context,session,db)=Context(38);
 var controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context}};
 var f=Dates();f.View=invalid;
 Check((Report(controller,"B",f) as ObjectResult)?.StatusCode==400,"Presentation view rejected by report API");
}
foreach(var role in new[]{38,1}) {
 var (context,session,db)=Context(role);
 context.Session.Remove(SessionKeys.SessionStamp);
 var controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context}};
 Check((Report(controller,"B",Dates()) as ObjectResult)?.StatusCode==401,"Revoked session denied");
}
Console.WriteLine($"PASS: {checks} compiled CCO navigation, datasets, legacy dates, API contract and session/role/API-denial checks. No Oracle reads.");
ExecutiveDashboardFilterRequest Dates()=>new(){StartDate="2024-01-01",EndDate="2024-02-29",View="reports"};
IActionResult Report(ExecutiveReportsController c,string r,ExecutiveDashboardFilterRequest f)=>r switch {"A"=>c.GetReportA(f,"CCO"),"B"=>c.GetReportB(f,"CCO"),"C"=>c.GetReportC(f,"CCO"),"D"=>c.GetReportD(f,"CCO"),_=>c.GetReportE(f,"CCO")};
(DefaultHttpContext,SessionHandler,DBConnection) Context(int role) {
 var context=new DefaultHttpContext{Session=new MemorySession(),User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,"cco-test")},"test"))};
 context.Request.Method="GET";
 var accessor=new HttpContextAccessor{HttpContext=context};
 var session=new SessionHandler(accessor,config,null);
 session.SetUser(new SessionUser{ID=999999,PPNumber="cco-"+Guid.NewGuid(),UserRoleID=role,UserEntityID=9,IPAddress="127.0.0.1",MACAddress="test",FirstMACCardAddress="test"});
 context.Session.SetString(SessionKeys.AllowedViewIds,"[1000302]");
 return(context,session,new DBConnection(accessor,null,config,session,new SecurityTokenService(config),null,null));
}
sealed class MemorySession:ISession {
 readonly Dictionary<string,byte[]> items=new();public bool IsAvailable=>true;public string Id=>"cco-test";public IEnumerable<string> Keys=>items.Keys;
 public void Clear()=>items.Clear();public void Remove(string k)=>items.Remove(k);public void Set(string k,byte[] v)=>items[k]=v;public bool TryGetValue(string k,out byte[] v)=>items.TryGetValue(k,out v);
 public Task LoadAsync(CancellationToken t=default)=>Task.CompletedTask;public Task CommitAsync(CancellationToken t=default)=>Task.CompletedTask;
}
sealed class Permissions:IPermissionService {
 public void EnsurePermissionsCached(SessionUser u){}public void PreloadPermissions(SessionUser u){}public bool HasViewPermission(SessionUser u,int id)=>id==1000302;
 public bool HasApiPermissionForPath(SessionUser u,string m,string b,string p)=>false;public bool HasPermissionToExecuteAction(SessionUser u,string id)=>false;public bool HasRole(SessionUser u,string c)=>false;
}
sealed class Pages:IPageIdResolver {
 public int ResolvePageId(HttpContext c)=>1000302;public int ResolvePageId(string p)=>1000302;
 public bool TryResolvePageId(HttpContext c,out int id){id=1000302;return true;}public bool TryResolvePageId(string p,out int id){id=1000302;return true;}
}
