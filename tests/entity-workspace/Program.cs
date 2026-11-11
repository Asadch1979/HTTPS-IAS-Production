using AIS;
using AIS.Controllers;
using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

void Check(bool condition, string label) {
 if (!condition) throw new Exception(label);
 Console.WriteLine("PASS: " + label);
}
AdministrationPanelController Controller(bool authenticated, bool sessionPresent, params int[] allowed) {
 var context = new DefaultHttpContext();
 context.User = new ClaimsPrincipal(new ClaimsIdentity(authenticated ? new[]{new Claim(ClaimTypes.Name,"test")} : Array.Empty<Claim>(), authenticated ? "Test" : null));
 context.Session = new MemorySession();
 if(sessionPresent) context.Session.Set(SessionKeys.User, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new SessionUser{ID=5,UserRoleID=9,SessionId="test",PPNumber="test"})));
 var session = new SessionHandler(new HttpContextAccessor{HttpContext=context}, new ConfigurationBuilder().Build(), null);
 var controller = new AdministrationPanelController(NullLogger<AdministrationPanelController>.Instance,session,null,null,new Permissions(allowed),null,new Pages());
 controller.ControllerContext = new ControllerContext{HttpContext=context};
 return controller;
}
Check(Controller(false,true,1,2).LoadEntityDashboardStep("AUTHORIZE_AUDITEE_ENTITIES_UPDATE") is UnauthorizedResult,"unauthenticated partial rejected");
Check(Controller(true,false,1,2).LoadEntityDashboardStep("AUTHORIZE_AUDITEE_ENTITIES_UPDATE") is UnauthorizedResult,"missing session rejected");
var dashboardOnly=Controller(true,true,1);
Check(dashboardOnly.LoadEntityDashboardStep("AUTHORIZE_AUDITEE_ENTITIES_UPDATE") is PartialViewResult && dashboardOnly.Response.StatusCode==403,"dashboard permission cannot grant step access");
var stepOnly=Controller(true,true,2);
Check(stepOnly.LoadEntityDashboardStep("AUTHORIZE_AUDITEE_ENTITIES_UPDATE") is PartialViewResult && stepOnly.Response.StatusCode==403,"step permission cannot bypass dashboard");
Check(Controller(true,true,1,2).LoadEntityDashboardStep("NOT_A_STEP") is BadRequestObjectResult,"unknown step rejected");
var allowed=Controller(true,true,1,2).LoadEntityDashboardStep("AUTHORIZE_AUDITEE_ENTITIES_UPDATE") as PartialViewResult;
Check(allowed?.ViewName.EndsWith("_AuthorizeAuditeeEntitiesUpdate.cshtml")==true,"authorized step returns exact intended partial");
var unmapped=Controller(true,true,1,2);
Check(unmapped.LoadEntityDashboardStep("ENTITY_ADDITION") is PartialViewResult && unmapped.Response.StatusCode==403,"unmapped permission fails closed");
Check(Controller(false,false).Entity_Dashboard() is RedirectToActionResult,"dashboard rejects missing identity before chrome readers");
Check(Controller(true,true).Entity_Dashboard() is RedirectToActionResult,"dashboard rejects missing page permission before chrome readers");
Check(!EntityDashboardAccess.CanViewStep(true,0,_=>true),"zero page never authorized");
Check(!EntityDashboardAccess.CanViewStep(false,2,_=>throw new Exception()),"dashboard rejection short circuits step permission");

var modelMethod = typeof(AdministrationPanelController).GetMethod("BuildEntityDashboardViewModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
var permittedModel = (AIS.Models.WorkflowDashboard.WorkflowDashboardViewModel)modelMethod.Invoke(Controller(true,true,1,3), new object[]{new SessionUser{ID=5}, "ENGAGEMENT_SHIFTING"});
var shifting = permittedModel.VisibleSteps.Single(s=>s.StepKey=="ENGAGEMENT_SHIFTING");
Check(shifting.LegacyPath.EndsWith("?stepKey=ENGAGEMENT_SHIFTING") && shifting.RequiredPermissionPageId==3,"shifting link targets its own partial with established API permission");
Check(permittedModel.VisibleSteps.All(s=>s.RequiredPermissionPageId==3),"unrelated step permissions stay hidden");
var linksMethod=typeof(AdministrationPanelController).GetMethod("BuildEntityAdministrationLinks",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
var links=(List<AIS.Models.WorkflowDashboard.WorkflowDashboardStepModel>)linksMethod.Invoke(Controller(true,true,1,2),new object[]{new SessionUser{ID=5}});
Check(links.Count==1 && links[0].StepKey=="AUTHORIZE_AUDITEE_ENTITIES_UPDATE" && links[0].LegacyPath.EndsWith("stepKey=AUTHORIZE_AUDITEE_ENTITIES_UPDATE"),"legacy function directory filters each permission and routes to working host");
var workspaceModelMethod = typeof(AdministrationPanelController).GetMethod("BuildEntityWorkspaceViewModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
var independentModel = (AIS.Models.WorkflowDashboard.WorkflowDashboardViewModel)workspaceModelMethod.Invoke(Controller(true,true,467), new object[]{new SessionUser{ID=5}, null});
Check(independentModel.DashboardKey=="ENTITY_WORKSPACE" && !independentModel.VisibleSteps.Any(), "workspace model exists without dashboard or operation permissions");
Check(Controller(true,true).Entity_Workspace(data:"search") is ObjectResult denial && denial.StatusCode==403, "workspace AJAX denied with 403 rather than HTML redirect");
var reached=false;
var revokedContext=new DefaultHttpContext();
revokedContext.Request.Path="/AdministrationPanel/LoadEntityDashboardStep";
revokedContext.Request.Headers.Cookie="IAS_SESSION=revoked";
revokedContext.Request.Headers["X-Requested-With"]="XMLHttpRequest";
await new AIS.Middleware.SessionValidationMiddleware(_=>{reached=true;return Task.CompletedTask;}).Invoke(revokedContext,new RevokedDatabase());
Check(!reached && revokedContext.Response.StatusCode==401,"revoked IAS session is rejected before partial endpoint");

var root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../AIS"));
var realPages=new PageIdResolver(new TestEnvironment(root),NullLogger<PageIdResolver>.Instance);
var existingLegacyPageId = realPages.ResolvePageId("/AdministrationPanel/Entity_Dashboard");
Check(existingLegacyPageId > 0 && existingLegacyPageId != 467,
    "unchanged active Page ID.xlsx retains the legacy dashboard mapping distinct from workspace 467");
Console.WriteLine("Effective original dashboard PAGE_ID from unchanged catalog: " + existingLegacyPageId);
Check(realPages.ResolvePageId("/AdministrationPanel/Entity_Workspace")==467,"workspace registration resolves to approved 467");
var catalogController=Controller(true,true);
var resolverField=typeof(AdministrationPanelController).GetField("_pageIdResolver",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
resolverField.SetValue(catalogController,realPages);
var stepsMethod=typeof(AdministrationPanelController).GetMethod("BuildEntityDashboardSteps",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
var actualSteps=(List<AIS.Models.WorkflowDashboard.WorkflowDashboardStepModel>)stepsMethod.Invoke(catalogController,null);
var expectedStepIds=new[]{217,216,218,41,323,324,225,225};
Check(actualSteps.Select(s=>s.RequiredPermissionPageId).SequenceEqual(expectedStepIds),"all eight step IDs match effective active/archive configuration");
foreach(var step in actualSteps){Console.WriteLine($"MAPPING: {step.StepKey} = {step.RequiredPermissionPageId}");Check(step.RequiredPermissionPageId>0,"configured original step "+step.StepKey);}
foreach(var action in new[]{"entity_heirarchy","manage_entity_type","manage_entity_relations","manage_entity_mapping","audit_comp_management","gm_repo_line_management","entity_gm_reporting_div_management","EntityShiftingHistory"}){var id=realPages.ResolvePageId("/AdministrationPanel/"+action);Console.WriteLine($"MAPPING: {action} = {id}");var expected=action switch {"audit_comp_management"=>265,"EntityShiftingHistory"=>448,_=>0};Check(id==expected,"additional link mapping snapshot "+action);}
var configuredController=Controller(true,true,existingLegacyPageId,217,216,218,41,323,324,225,265,448);
resolverField.SetValue(configuredController,realPages);
var configuredLinks=(List<AIS.Models.WorkflowDashboard.WorkflowDashboardStepModel>)linksMethod.Invoke(configuredController,new object[]{new SessionUser{ID=5}});
Check(configuredLinks.Count==10,"all eight mapped steps and two mapped additional functions visible with permission; six unmapped links excluded");
var deniedController=Controller(true,true,existingLegacyPageId);
resolverField.SetValue(deniedController,realPages);
Check(((List<AIS.Models.WorkflowDashboard.WorkflowDashboardStepModel>)linksMethod.Invoke(deniedController,new object[]{new SessionUser{ID=5}})).Count==0,"dashboard-only access cannot expose any configured legacy function");
Check(Controller(true,true,1).GetShiftableEngagements(5) is ForbidResult,"direct shifting reader requires shifting policy");
Check(Controller(true,true,1).ShiftEngagementEntity(null) is ForbidResult,"direct shifting mutation requires shifting policy before validation or DB");
Check(Controller(false,true,1,3).GetShiftableEngagements(5) is UnauthorizedResult,"expired identity cannot retrieve shifting records");
var expiredContext=new DefaultHttpContext();expiredContext.Request.Path="/AdministrationPanel/LoadEntityDashboardStep";expiredContext.Request.Headers["X-Requested-With"]="XMLHttpRequest";
var expiredReached=false;await new AIS.Middleware.SessionValidationMiddleware(_=>{expiredReached=true;return Task.CompletedTask;}).Invoke(expiredContext,new RevokedDatabase());
Check(!expiredReached && expiredContext.Response.StatusCode==401,"missing IAS cookie is rejected before operational partial");
Check(actualSteps.Single(s=>s.StepKey=="SETUP_ENGAGEMENT_REVERSAL").PartialViewName.EndsWith("setup_engagement_reversal.cshtml") && actualSteps.Single(s=>s.StepKey=="ENGAGEMENT_SHIFTING").PartialViewName.EndsWith("_EngagementShifting.cshtml"),"reversal and shifting have distinct correct partial destinations");
sealed class Permissions(int[] allowed) : IPermissionService {
 public void EnsurePermissionsCached(SessionUser u){} public void PreloadPermissions(SessionUser u){}
 public bool HasViewPermission(SessionUser u,int id)=>allowed.Contains(id);
 public bool HasApiPermissionForPath(SessionUser u,string method,string pathBase,string path)=>false;
 public bool HasPermissionToExecuteAction(SessionUser u,string id)=>false;
 public bool HasRole(SessionUser u,string role)=>false;
}
sealed class Pages : IPageIdResolver {
 public int ResolvePageId(string path)=>path switch {"/AdministrationPanel/Entity_Dashboard"=>1,"/AdministrationPanel/authorize_auditee_entities_update"=>2,"/AdministrationPanel/setup_engagement_reversal"=>3,_=>0};
 public int ResolvePageId(HttpContext c)=>ResolvePageId(c.Request.Path);
 public bool TryResolvePageId(string p,out int id){id=ResolvePageId(p);return id>0;}
 public bool TryResolvePageId(HttpContext c,out int id)=>TryResolvePageId(c.Request.Path,out id);
}
sealed class MemorySession : ISession {
 readonly Dictionary<string,byte[]> data=new();public bool IsAvailable=>true;public string Id=>"test";public IEnumerable<string> Keys=>data.Keys;
 public void Clear()=>data.Clear();public Task CommitAsync(CancellationToken t=default)=>Task.CompletedTask;public Task LoadAsync(CancellationToken t=default)=>Task.CompletedTask;
 public void Remove(string k)=>data.Remove(k);public void Set(string k,byte[] v)=>data[k]=v;public bool TryGetValue(string k,out byte[] v)=>data.TryGetValue(k,out v);
}
sealed class RevokedDatabase : IDBConnection {
 public bool IsSessionValid(string token)=>false;
 public void CreateSession(string token,int ppno,string ip,string agent)=>throw new Exception();
 public void KillSessions(int ppno)=>throw new Exception();
 public void InvalidateSession(string token)=>throw new Exception();
}
sealed class TestEnvironment(string root) : Microsoft.AspNetCore.Hosting.IWebHostEnvironment {
 public string ApplicationName{get;set;}="AIS"; public string EnvironmentName{get;set;}="Development";
 public string ContentRootPath{get;set;}=root;public string WebRootPath{get;set;}=Path.Combine(root,"wwwroot");
 public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider{get;set;}=new Microsoft.Extensions.FileProviders.NullFileProvider();
 public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider{get;set;}=new Microsoft.Extensions.FileProviders.NullFileProvider();
}
