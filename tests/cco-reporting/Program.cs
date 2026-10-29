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
using System.Reflection;
using System.Linq.Expressions;
using System.Text.Json;

var checks=0;
void Check(bool condition, string message) { checks++; if(!condition) throw new Exception(message); }
var definitions=ExecutiveDashboardViews.ForPerspective("CCO");
var visible=definitions.Where(v=>v.Id!="reports").ToArray();
Check(string.Join(",",visible.Select(v=>v.Id))=="overview,critical-observations,org-position,field-office,gm-region,regional-rankings,performance,effectiveness","Eight ordered CCO tabs");
Check(string.Join(",",definitions.Single(v=>v.Id=="reports").Reports)=="B,C,D","CCO report allowlist");
Check(!visible[0].UseLifecycleKpis && !visible[0].Datasets.Contains("GetKpiLifecycle") && new[]{"GetKpiSummary","GetRiskSummary","GetAgingSummary","GetCycleSummary","GetSettlementTrend"}.All(visible[0].Datasets.Contains),"Overview requests only authorized CCO summary/distribution datasets");
Check(!visible[0].Datasets.Contains("GetCriticalItems") && definitions.Single(v=>v.Id=="critical-observations").Datasets.Contains("GetCriticalItems"),"Critical reader belongs to dedicated tab");
foreach(var tab in new[]{"org-position","performance"}) Check(definitions.Single(v=>v.Id==tab).Datasets.Length==0,"Pending tabs must not fetch invented data");
foreach(var role in new[]{"TOP","GROUP","RLM","CIA"}) Check(!ExecutiveDashboardViews.ForPerspective(role).Any(v=>v.Id=="overview"||v.Id=="field-office"),"CCO navigation isolation");
foreach(var report in new[]{"A","E"}) { try { CcoDashboardPresentation.ResolveLegacy("reports",report); throw new Exception("Unapproved report accepted"); } catch(ArgumentException) { checks++; } }
var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>{{"Security:CauKey","cco-test-only"},{"SecretKey","cco-test-only-not-a-production-secret"}}).Build();
foreach(var pair in new[]{("glance","overview"),("position","overview"),("risk","overview"),("aging","overview"),("settlement","effectiveness"),("reports","field-office")}) {
 var (context,session,db)=Context(38);
 context.Request.QueryString=new QueryString("?report=B");
 var controller=new ExecutiveOverviewController(db,session,new Permissions(),new Pages(),null,NullLogger<ExecutiveOverviewController>.Instance,config){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 var result=controller.CCO(view:pair.Item1,filters:Dates()) as RedirectToActionResult;
 Check(result?.ActionName=="CCO" && (string)result.RouteValues["view"]==pair.Item2,"Authorized legacy redirect "+pair.Item1);
 Check((string)result.RouteValues["startDate"]=="2024-01-01" && (string)result.RouteValues["endDate"]=="2024-02-29","Redirect date preservation");
}
foreach(var report in new[]{"B","C","D","A","E"}) foreach(var role in new[]{38,3,5}) {
 var (context,session,db)=Context(role);
 context.Request.Path="/api/ExecutiveReports/GetReport"+report;
 var controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 // Missing API grant denies every report before any database query.
 var result=Report(controller,report,Dates());
 Check((result as ObjectResult)?.StatusCode==403,"Missing API or wrong role denied");
 if(role==38 && (report=="A"||report=="E")) {
  controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
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
 var controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 Check((Report(controller,report,Dates()) as ObjectResult)?.StatusCode==403,"Granted route still denies A/E and CCO PDF");
}
foreach(var denied in new[]{"page","future","root","snap"}) {
 var (context,session,db)=Context(38);
 if(denied=="page") context.Session.SetString(SessionKeys.AllowedViewIds,"[]");
 var f=Dates();if(denied=="future") f.EndDate="2099-01-01";if(denied=="snap") f.SnapDivision="1";
 var controller=new ExecutiveOverviewController(db,session,new Permissions(),new Pages(),null,NullLogger<ExecutiveOverviewController>.Instance,config){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 var result=controller.CCO(groupRootId:denied=="root"?9:null,view:"risk",filters:f);
 Check(((result as StatusCodeResult)?.StatusCode ?? (result as ObjectResult)?.StatusCode)==(denied=="page"||denied=="root"?403:400),"Legacy redirect cannot bypass "+denied);
}
foreach(var invalid in new[]{"field-office","gm-region","regional-rankings"}) {
 var (context,session,db)=Context(38);
 var controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 var f=Dates();f.View=invalid;
 Check((Report(controller,"B",f) as ObjectResult)?.StatusCode==400,"Presentation view rejected by report API");
}
foreach(var role in new[]{38,1}) {
 var (context,session,db)=Context(role);
 context.Session.Remove(SessionKeys.SessionStamp);
 var controller=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 Check((Report(controller,"B",Dates()) as ObjectResult)?.StatusCode==401,"Revoked session denied");
}
// Exercise the production authorization pipeline with only the final Oracle
// delegate replaced. A permitted probe proves eligibility, not database success.
var eligibility=new Dictionary<string,int>();
foreach(var action in visible[0].Datasets.Concat(new[]{"GetKpiLifecycle"}).Distinct()) {
 var (context,session,db)=Context(38);
 context.Request.Path="/api/ExecutiveDashboard/"+action;
 Grant(context);
 var controller=new ExecutiveDashboardController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveDashboardController>.Instance,config){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 controller.RouteData.Values["action"]=action;
 var f=Dates();f.View="overview";
 IActionResult result;
 if(action=="GetKpiLifecycle") result=controller.GetKpiLifecycle(dashboardType:"CCO",filters:f);
 else {
  var execute=typeof(ExecutiveDashboardController).GetMethod("ExecuteSummary",BindingFlags.NonPublic|BindingFlags.Instance).MakeGenericMethod(typeof(object));
  var query=Sentinel(execute.GetParameters()[5].ParameterType,new object());
  result=(IActionResult)execute.Invoke(controller,new object[]{null,null,"CCO",null,f,query,false,false,null});
 }
 eligibility[action]=Status(result);
 Check(Status(result)==(action=="GetKpiLifecycle"?403:200),"Actual endpoint eligibility "+action);
}
foreach(var role in new[]{1,3,5}) {
 var (context,session,db)=Context(role);context.Request.Path="/api/ExecutiveDashboard/GetKpiLifecycle";Grant(context);
 var c=new ExecutiveDashboardController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveDashboardController>.Instance,config){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 Check(Status(c.GetKpiLifecycle(dashboardType:"CCO",filters:Dates()))==403,"Lifecycle CCO denied for role "+role);
}
var reportEligibility=new Dictionary<string,int>();
foreach(var report in new[]{"B","C","D"}) {
 var (context,session,db)=Context(38);context.Request.Path="/api/ExecutiveReports/GetReport"+report;Grant(context);
 var c=new ExecutiveReportsController(db,session,new Permissions(),new Pages(),NullLogger<ExecutiveReportsController>.Instance,null){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 var execute=typeof(ExecutiveReportsController).GetMethod("Execute",BindingFlags.NonPublic|BindingFlags.Instance);
 var query=Sentinel(execute.GetParameters()[4].ParameterType,new ExecutiveReportResult());
 var result=(IActionResult)execute.Invoke(c,new object[]{report,Dates(),"CCO",null,query});
 reportEligibility[report]=Status(result);Check(Status(result)==200,"Actual canonical report eligibility "+report);
}
foreach(var report in new[]{"C","D"}) {
 var (context,session,db)=Context(38);context.Request.QueryString=new QueryString("?report="+report);
 var c=new ExecutiveOverviewController(db,session,new Permissions(),new Pages(),null,NullLogger<ExecutiveOverviewController>.Instance,config){ControllerContext=new ControllerContext{HttpContext=context,RouteData=new Microsoft.AspNetCore.Routing.RouteData()}};
 var result=c.CCO(view:"reports",filters:Dates()) as RedirectToActionResult;
 Check((string)result?.RouteValues["view"]==(report=="C"?"gm-region":"regional-rankings"),"Legacy report redirect "+report);
 Check((string)result?.RouteValues["endDate"]=="2024-02-29","Legacy report retains dates");
}
// Phase 2 is deliberately unavailable even after all IAS gates pass.
foreach(var action in new[]{"GetOrgFilters","GetOrgPosition","GetOrgPerformance","GetOrgSettlement","GetOrgExceptions"}) {
 foreach(var role in new[]{38,1,3,5}) {
  var (context,session,db)=Context(role);context.Request.Path="/api/CcoCompliance/"+action;Grant(context);
  var c=new CcoComplianceController(db,session,new Permissions(true),new Pages(),NullLogger<CcoComplianceController>.Instance){ControllerContext=new ControllerContext{HttpContext=context}};
  var result=(IActionResult)typeof(CcoComplianceController).GetMethod(action).Invoke(c,new object[]{new CcoOrganizationFilter{StartDate="2024-01-01",EndDate="2024-02-29"}});
  Check(Status(result)==(role==38||role==1?503:403),"New CCO action role/gate "+action+"/"+role);
 }
}
foreach(var denied in new[]{"page","api","revoked","root","snap","duplicate","invalidPath","invalidDate","unauthenticated"}) {
 var (context,session,db)=Context(38);context.Request.Path="/api/CcoCompliance/GetOrgPosition";Grant(context);
 if(denied=="page")context.Session.SetString(SessionKeys.AllowedViewIds,"[]");
 if(denied=="api")context.Session.SetString(SessionKeys.AllowedApiPaths,"[]");
 if(denied=="revoked")context.Session.Remove(SessionKeys.SessionStamp);
 if(denied=="unauthenticated")context.User=new ClaimsPrincipal(new ClaimsIdentity());
 if(denied=="root")context.Request.QueryString=new QueryString("?groupRootId=9");
 if(denied=="snap")context.Request.QueryString=new QueryString("?snapDivision=FAD");
 if(denied=="duplicate")context.Request.QueryString=new QueryString("?groupId=1&groupId=2");
 var f=new CcoOrganizationFilter{StartDate="2024-01-01",EndDate="2024-02-29"};
 if(denied=="invalidPath")f.DivisionId=2;if(denied=="invalidDate")f.EndDate="2099-01-01";
 var c=new CcoComplianceController(db,session,new Permissions(),new Pages(),NullLogger<CcoComplianceController>.Instance){ControllerContext=new ControllerContext{HttpContext=context}};
 Check(Status(c.GetOrgPosition(f))==(denied=="page"||denied=="api"?403:denied=="revoked"||denied=="unauthenticated"?401:400),"New API rejection "+denied);
}
var pathSet=new[]{new CcoOrganizationPath{GroupId=1,DivisionId=2,GmOfficeId=3,EntityId=4,IsVerified=true},new CcoOrganizationPath{GroupId=5,DivisionId=6,GmOfficeId=7,EntityId=8,IsVerified=true}};
var selection=new CcoOrganizationFilter{StartDate="2024-01-01",EndDate="2024-02-29",GroupId=1,DivisionId=2,GmOfficeId=3,EntityId=4};
selection.ValidatePath(pathSet);checks++;
selection.GmOfficeId=7;try{selection.ValidatePath(pathSet);throw new Exception("Cross-parent path accepted");}catch(ArgumentException){checks++;}
selection.GmOfficeId=3;pathSet[0].IsVerified=false;try{selection.ValidatePath(pathSet);throw new Exception("Unverified path accepted");}catch(ArgumentException){checks++;}
foreach(var level in new[]{"ALL","gm","",null}) {var f=new CcoOrganizationFilter{StartDate="2024-01-01",EndDate="2024-02-29",GroupBy=level};try{f.Validate();throw new Exception("Invalid aggregation accepted");}catch(ArgumentException){checks++;}}
Check(new ExecutiveKpiSummary().OpeningCases==null && new ExecutiveKpiSummary().ParaAdded==null,"New optional fields default unknown");
Check(string.Join(",",visible[0].Kpis.Take(3))=="openingCases,paraAdded,totalCases","KPI definition order");
if(args.Length==2 && args[0]=="--eligibility-output") File.WriteAllText(args[1],JsonSerializer.Serialize(new { datasets=visible[0].Datasets,kpis=visible[0].Kpis,eligible=eligibility,reports=reportEligibility }));
int Status(IActionResult r)=>(r as ObjectResult)?.StatusCode ?? (r as StatusCodeResult)?.StatusCode ?? -1;
void Grant(DefaultHttpContext c)=>c.Session.SetString(SessionKeys.AllowedApiPaths,JsonSerializer.Serialize(new[]{"GET:"+c.Request.Path.Value.ToLowerInvariant()}));
Delegate Sentinel(Type type,object value) {
 var invoke=type.GetMethod("Invoke");
 var parameters=invoke.GetParameters().Select(p=>Expression.Parameter(p.ParameterType,p.Name)).ToArray();
 return Expression.Lambda(type,Expression.Constant(value,invoke.ReturnType),parameters).Compile();
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
 readonly bool allowApi;public Permissions(bool allowApi=false){this.allowApi=allowApi;}
 public void EnsurePermissionsCached(SessionUser u){}public void PreloadPermissions(SessionUser u){}public bool HasViewPermission(SessionUser u,int id)=>id==1000302;
 public bool HasApiPermissionForPath(SessionUser u,string m,string b,string p)=>allowApi;public bool HasPermissionToExecuteAction(SessionUser u,string id)=>false;public bool HasRole(SessionUser u,string c)=>false;
}
sealed class Pages:IPageIdResolver {
 public int ResolvePageId(HttpContext c)=>1000302;public int ResolvePageId(string p)=>1000302;
 public bool TryResolvePageId(HttpContext c,out int id){id=1000302;return true;}public bool TryResolvePageId(string p,out int id){id=1000302;return true;}
}
