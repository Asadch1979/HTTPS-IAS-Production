using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AIS.Controllers
{
    // Always-run result filter also handles anti-forgery authorization short circuits.
    // Scoped to the two new POST actions; no global filter behavior changes.
    public sealed class FadRiskJsonFailureAttribute : Attribute, IAlwaysRunResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is not JsonResult && context.Result is IStatusCodeActionResult status && status.StatusCode == 400)
                context.Result=new JsonResult(new { success=false,message="Invalid request or verification token. Reload the page and try again." }) { StatusCode=400 };
        }
        public void OnResultExecuted(ResultExecutedContext context) { }
    }
    public partial class FADRiskControlPanelController
    {
        [HttpGet]
        public IActionResult Assessment()
        {
            if (User.Identity?.IsAuthenticated != true || !sessionHandler.TryGetUser(out var user) || sessionHandler.IsSessionRevoked(user))
                return RedirectToAction("Index","Login");
            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) return RedirectToAction("Index","PageNotFound");
            ViewData["TopMenu"]=tm.GetTopMenus(); ViewData["TopMenuPages"]=tm.GetTopMenusPages();
            return View("~/Views/FAD/FADRiskControlPanel/Assessment.cshtml");
        }
        private IActionResult RiskAction(Func<SessionUser,object> action)
        {
            Response.Headers["Cache-Control"]="no-store";
            if (User.Identity?.IsAuthenticated != true || !sessionHandler.TryGetUser(out var user) || sessionHandler.IsSessionRevoked(user))
                return StatusCode(401,new { success=false,message="Your session has expired." });
            // Explicit check also covers direct non-AJAX API calls; global PageKeyPermissionGuard remains in force.
            _permissionService.EnsurePermissionsCached(user);
            if (!_permissionService.HasApiPermissionForPath(user,Request.Method,Request.PathBase.Value,Request.Path.Value))
                return StatusCode(403,new { success=false,message="You do not have access to this operation." });
            if (!ModelState.IsValid) return BadRequest(new { success=false,message="Check the required fields, input values and evidence references." });
            try { return Json(new { success=true,data=action(user) }); }
            catch (UnauthenticatedException) { return StatusCode(401,new { success=false,message="Your session has expired." }); }
            catch (UnauthorizedAccessException) { return StatusCode(403,new { success=false,message="Access denied." }); }
            catch (OracleException ex)
            {
                var status=ex.Number==20043 ? 403 : ex.Number==20044 || ex.Number==20045 || ex.Number==1 ? 409 :
                    ex.Number==20040 || ex.Number==20041 ? 422 : 500;
                _logger.LogWarning("FAD assessment Oracle failure {Code}",ex.Number);
                return StatusCode(status,new { success=false,message=status==403 ? "Assessment access or evidence ownership denied." :
                    status==409 ? "The assessment changed, already exists, or is locked. Reload before continuing." :
                    status==422 ? "Check the complete matrix, evidence IDs and nonzero financial denominators." : "The assessment operation could not be completed." });
            }
            catch (Exception ex)
            {
                _logger.LogError("FAD assessment failure of type {Type}",ex.GetType().Name);
                return StatusCode(500,new { success=false,message="The assessment operation could not be completed." });
            }
        }
        [HttpGet]
        public IActionResult AssessmentCatalog() => RiskAction(user => {
            var catalog=dBConnection.RiskAssessmentCatalog(); // Database authorization precedes advisory UI capabilities.
            var rights=dBConnection.RiskAssessmentCapabilities();
            bool Has(string action) => _permissionService.HasApiPermissionForPath(user,"POST",Request.PathBase.Value,"/FADRiskControlPanel/"+action);
            return new { catalog,
                engagements=dBConnection.GetReportEntities().Select(e => new { engagementId=e.EngagementId,branch=e.EntityName,period=e.AuditPeriod }),
                currentUserId=user.ID,canSave=rights.Prepare && Has("SaveAssessment"),
                canSubmit=rights.Prepare && Has("TransitionAssessment"),canReview=rights.Review && Has("TransitionAssessment") };
        });
        [HttpGet]
        public IActionResult ReadAssessment(int engagementId,int assessmentId=0,int versionId=0)
        {
            if (engagementId<=0 || assessmentId<0 || (assessmentId==0 && versionId<=0)) ModelState.AddModelError("id","Invalid selection.");
            return RiskAction(user => dBConnection.RiskAssessmentRead(engagementId,assessmentId,versionId));
        }
        [HttpPost, ValidateAntiForgeryToken, FadRiskJsonFailure, RequestSizeLimit(1048576)]
        public IActionResult SaveAssessment([FromBody] FadRiskAssessmentRequest request)
        {
            if (request==null) ModelState.AddModelError("request","Request required.");
            return RiskAction(user => new { assessmentId=dBConnection.RiskAssessmentSave(request) });
        }
        [HttpPost, ValidateAntiForgeryToken, FadRiskJsonFailure]
        public IActionResult TransitionAssessment([FromBody] FadRiskTransitionRequest request)
        {
            if (request==null) ModelState.AddModelError("request","Request required.");
            return RiskAction(user => { dBConnection.RiskAssessmentTransition(request); return new { assessmentId=request.AssessmentId }; });
        }
    }
}
