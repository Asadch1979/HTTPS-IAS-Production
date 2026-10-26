using AIS.Filters;
using AIS.Models.Planning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;

namespace AIS.Controllers
{
    public partial class PlanningController
    {
        private bool CanRequestAuditPlanDeletion()
        {
            if (!sessionHandler.TryGetUser(out var user) || user == null)
                return false;
            return HasPageAccess(user, "/Planning/tentative_audit_plan")
                || HasPageAccess(user, "/Planning/Planning");
        }

        private bool CanAuthorizeAuditPlanDeletion()
        {
            return sessionHandler.TryGetUser(out var user) && user != null
                && HasPageAccess(user, "/Planning/audit_plan_deletion_authorization");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ApplicationAudit("AUDIT_PLAN_DELETE_REQUEST", "AUDIT_PLANNING",
            "PLAN_DELETION", "PKG_HD", "P_REQUEST_PLAN_DELETION",
            ObjectType = "AUDIT_PLAN", ObjectId = "PlanId")]
        public IActionResult RequestAuditPlanDeletion(PlanDeletionRequestInputModel input)
        {
            if (!CanRequestAuditPlanDeletion()) return Forbid();
            if (input == null || input.PlanId <= 0 || string.IsNullOrWhiteSpace(input.Reason)
                || input.Reason.Trim().Length < 10 || input.Reason.Length > 2000)
            {
                TempData["PlanDeletionError"] = "Select a plan and provide a reason of 10 to 2000 characters.";
                return RedirectToAction(nameof(tentative_audit_plan));
            }
            try
            {
                if (dBConnection.RequestAuditPlanDeletion(input.PlanId, input.Reason) != "REQUESTED")
                    throw new InvalidOperationException("Unexpected request result.");
                TempData["PlanDeletionMessage"] = "Deletion request submitted for authorization.";
            }
            catch (OracleException ex)
            {
                _logger.LogWarning(ex, "Audit plan deletion request failed for plan {PlanId}.", input.PlanId);
                TempData["PlanDeletionError"] = PlanDeletionFriendlyError(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit plan deletion request failed.");
                TempData["PlanDeletionError"] = "Unable to submit deletion request.";
            }
            return RedirectToAction(nameof(tentative_audit_plan));
        }

        [Authorize]
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult audit_plan_deletion_authorization()
        {
            if (!CanAuthorizeAuditPlanDeletion()) return Forbid();
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            try
            {
                List<AuditPlanDeletionRequestModel> requests = dBConnection.GetAuditPlanDeletionRequests();
                return View(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load pending audit plan deletion requests.");
                return StatusCode(503, "Pending deletion requests are temporarily unavailable.");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ApplicationAudit("AUDIT_PLAN_DELETE_AUTHORIZED", "AUDIT_PLANNING",
            "PLAN_DELETION", "PKG_HD", "P_AUTHORIZE_PLAN_DELETION",
            ObjectType = "AUDIT_PLAN_DELETE_REQUEST", ObjectId = "RequestId")]
        public IActionResult AuthorizeAuditPlanDeletion(PlanDeletionAuthorizationInputModel input)
        {
            if (!CanAuthorizeAuditPlanDeletion()) return Forbid();
            if (input == null || input.RequestId <= 0) return BadRequest();
            try
            {
                if (dBConnection.AuthorizeAuditPlanDeletion(input.RequestId) != "AUTHORIZED")
                    throw new InvalidOperationException("Unexpected authorization result.");
                TempData["PlanDeletionMessage"] = "Audit plan deletion authorized.";
            }
            catch (OracleException ex)
            {
                _logger.LogWarning(ex, "Audit plan deletion authorization failed for request {RequestId}.", input.RequestId);
                TempData["PlanDeletionError"] = PlanDeletionFriendlyError(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit plan deletion authorization failed.");
                TempData["PlanDeletionError"] = "Unable to authorize audit plan deletion.";
            }
            return RedirectToAction(nameof(audit_plan_deletion_authorization));
        }

        private static string PlanDeletionFriendlyError(OracleException ex)
        {
            switch (ex.Number)
            {
                case 1: case 20214:
                    return "A deletion request already exists for this plan.";
                case 20212:
                    return "An audit engagement already exists; deletion is not permitted.";
                case 20215: case 20216: case 20217: case 20218:
                    return "The request or plan has changed. Refresh and review again.";
                case 20213:
                    return "The deletion reason must contain 10 to 2000 characters.";
                case 20210: case 20211:
                    return "You are not authorized for this operation.";
                default:
                    return "Unable to complete the requested operation.";
            }
        }
    }
}
