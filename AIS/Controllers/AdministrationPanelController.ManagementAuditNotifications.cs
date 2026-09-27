using AIS.Models.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mail;

namespace AIS.Controllers
    {
    public partial class AdministrationPanelController
        {
        [HttpGet]
        public IActionResult management_audit_notifications()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler) || !sessionHandler.IsSuperUser())
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            var model = new ManagementAuditNotificationAdminViewModel
                {
                Divisions = dBConnection.GetManagementAuditNotificationConfigurations(),
                DivisionOptions = dBConnection.GetDivisions(false)
                    .OrderBy(item => item.NAME)
                    .Select(item => new ManagementAuditNotificationDivisionOptionModel
                        {
                        DivisionId = item.DIVISIONID,
                        DivisionName = item.NAME
                        })
                    .ToList()
                };

            return View(model);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AIS.Filters.ApplicationAudit("SAVE_MGMT_AUDIT_NOTIFY_DIVISION", "ADMINISTRATION", "ADMINISTRATION", "PKG_MGMT_AUDIT_NOTIFY_ADMIN", "P_SAVE_DIVISION", ObjectType = "MGMT_AUDIT_NOTIFY_DIVISION")]
        public IActionResult SaveManagementAuditNotificationDivision(SaveManagementAuditNotificationDivisionModel model)
            {
            if (!CanManageManagementAuditNotifications())
                {
                return Forbid();
                }

            if (model.EffectiveFrom.HasValue && model.EffectiveTo.HasValue && model.EffectiveTo.Value.Date < model.EffectiveFrom.Value.Date)
                {
                ModelState.AddModelError(nameof(model.EffectiveTo), "Effective To cannot be earlier than Effective From.");
                }

            var division = dBConnection.GetDivisions(false).FirstOrDefault(item => item.DIVISIONID == model.DivisionId);
            if (division == null)
                {
                ModelState.AddModelError(nameof(model.DivisionId), "Select a valid Division.");
                }
            else
                {
                model.DivisionName = division.NAME?.Trim() ?? string.Empty;
                }

            if (!ModelState.IsValid)
                {
                return BadRequest(new { status = false, message = "Please correct the highlighted fields.", errors = ExtractModelStateErrors() });
                }

            try
                {
                dBConnection.SaveManagementAuditNotificationDivision(model, GetManagementAuditNotificationActor());
                return Json(new { status = true, message = "Division notification configuration saved." });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unable to save Management Audit notification Division {DivisionId}.", model.DivisionId);
                return StatusCode(500, new { status = false, message = "Unable to save the Division notification configuration." });
                }
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AIS.Filters.ApplicationAudit("SAVE_MGMT_AUDIT_NOTIFY_RECIPIENT", "ADMINISTRATION", "ADMINISTRATION", "PKG_MGMT_AUDIT_NOTIFY_ADMIN", "P_SAVE_RECIPIENT", ObjectType = "MGMT_AUDIT_NOTIFY_RECIPIENT")]
        public IActionResult SaveManagementAuditNotificationRecipient(SaveManagementAuditNotificationRecipientModel model)
            {
            if (!CanManageManagementAuditNotifications())
                {
                return Forbid();
                }

            model.RecipientType = model.RecipientType?.Trim().ToUpperInvariant() ?? string.Empty;
            model.EmailAddress = model.EmailAddress?.Trim() ?? string.Empty;

            if (!IsSingleValidEmailAddress(model.EmailAddress))
                {
                ModelState.AddModelError(nameof(model.EmailAddress), "Enter one valid email address for this recipient.");
                }

            if (model.EffectiveFrom.HasValue && model.EffectiveTo.HasValue && model.EffectiveTo.Value.Date < model.EffectiveFrom.Value.Date)
                {
                ModelState.AddModelError(nameof(model.EffectiveTo), "Effective To cannot be earlier than Effective From.");
                }

            if (!dBConnection.GetManagementAuditNotificationConfigurations().Any(item => item.DivisionId == model.DivisionId))
                {
                ModelState.AddModelError(nameof(model.DivisionId), "Save the Division notification configuration before adding recipients.");
                }

            if (!ModelState.IsValid)
                {
                return BadRequest(new { status = false, message = "Please correct the highlighted fields.", errors = ExtractModelStateErrors() });
                }

            try
                {
                var recipientId = dBConnection.SaveManagementAuditNotificationRecipient(model, GetManagementAuditNotificationActor());
                return Json(new { status = true, message = "Recipient saved.", recipientId });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unable to save Management Audit notification recipient for Division {DivisionId}.", model.DivisionId);
                return StatusCode(500, new { status = false, message = "Unable to save the recipient." });
                }
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AIS.Filters.ApplicationAudit("REMOVE_MGMT_AUDIT_NOTIFY_RECIPIENT", "ADMINISTRATION", "ADMINISTRATION", "PKG_MGMT_AUDIT_NOTIFY_ADMIN", "P_REMOVE_RECIPIENT", ObjectType = "MGMT_AUDIT_NOTIFY_RECIPIENT")]
        public IActionResult RemoveManagementAuditNotificationRecipient(int recipientId)
            {
            if (!CanManageManagementAuditNotifications())
                {
                return Forbid();
                }

            if (recipientId <= 0)
                {
                return BadRequest(new { status = false, message = "Select a valid recipient." });
                }

            try
                {
                dBConnection.RemoveManagementAuditNotificationRecipient(recipientId, GetManagementAuditNotificationActor());
                return Json(new { status = true, message = "Recipient removed from active notification routing." });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unable to remove Management Audit notification recipient {RecipientId}.", recipientId);
                return StatusCode(500, new { status = false, message = "Unable to remove the recipient." });
                }
            }

        private bool CanManageManagementAuditNotifications()
            {
            return User?.Identity?.IsAuthenticated == true
                && sessionHandler.IsSuperUser()
                && this.UserHasPagePermissionForCurrentAction(sessionHandler);
            }

        private string GetManagementAuditNotificationActor()
            {
            var user = sessionHandler.GetUser();
            return string.IsNullOrWhiteSpace(user?.PPNumber) ? user?.ID.ToString() ?? "IAS" : user.PPNumber.Trim();
            }

        internal static bool IsSingleValidEmailAddress(string value)
            {
            if (string.IsNullOrWhiteSpace(value) || value.Contains(';') || value.Contains(','))
                {
                return false;
                }

            try
                {
                var trimmed = value.Trim();
                return string.Equals(new MailAddress(trimmed).Address, trimmed, StringComparison.OrdinalIgnoreCase);
                }
            catch (FormatException)
                {
                return false;
                }
            }
        }
    }
