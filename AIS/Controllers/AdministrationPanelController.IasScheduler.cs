using AIS.Models.Scheduler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
{
    public partial class AdministrationPanelController
    {
        [HttpGet]
        public IActionResult ias_scheduler()
        {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!CanManageIasScheduler())
            {
                return RedirectToAction("Index", "PageNotFound");
            }

            var model = new IasSchedulerAdminViewModel
            {
                Schedules = dBConnection.GetIasSchedulerSchedules(),
                JobDefinitions = dBConnection.GetIasSchedulerJobDefinitions(),
                ExecutionHistory = dBConnection.GetIasSchedulerExecutionHistory(),
                RunRequests = dBConnection.GetIasSchedulerRunRequests(),
                ReconciliationRequired = dBConnection.GetIasSchedulerReconciliationRequired()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AIS.Filters.ApplicationAudit("SAVE_IAS_SCHEDULER_JOB", "ADMINISTRATION", "IAS SCHEDULER", "PKG_IAS_SCHEDULER", "P_SAVE_JOB_DEFINITION", ObjectType = "IAS_SCHEDULER_JOB")]
        public IActionResult SaveIasSchedulerJobDefinition(SaveIasSchedulerJobDefinitionRequest model)
        {
            if (!CanManageIasScheduler())
            {
                return Forbid();
            }

            NormalizeJobDefinition(model, GetIasSchedulerActor());
            ValidateJobDefinition(model);

            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = false, message = "Please correct the highlighted fields.", errors = ExtractModelStateErrors() });
            }

            try
            {
                var jobId = dBConnection.SaveIasSchedulerJobDefinition(model);
                return Json(new { status = true, message = "Job definition saved.", jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save IAS scheduler job definition {JobId}.", model?.JobId);
                return StatusCode(500, new { status = false, message = "Unable to save the job definition." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AIS.Filters.ApplicationAudit("SAVE_IAS_SCHEDULER_SCHEDULE", "ADMINISTRATION", "IAS SCHEDULER", "PKG_IAS_SCHEDULER", "P_SAVE_SCHEDULE", ObjectType = "IAS_SCHEDULER_SCHEDULE")]
        public IActionResult SaveIasSchedulerSchedule(SaveIasSchedulerScheduleRequest model)
        {
            if (!CanManageIasScheduler())
            {
                return Forbid();
            }

            NormalizeSchedule(model, GetIasSchedulerActor());
            ValidateSchedule(model);

            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = false, message = "Please correct the highlighted fields.", errors = ExtractModelStateErrors() });
            }

            try
            {
                var scheduleId = dBConnection.SaveIasSchedulerSchedule(model);
                return Json(new { status = true, message = "Schedule saved.", scheduleId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save IAS scheduler schedule {ScheduleId}.", model?.ScheduleId);
                return StatusCode(500, new { status = false, message = "Unable to save the schedule." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AIS.Filters.ApplicationAudit("SET_IAS_SCHEDULER_ACTIVE", "ADMINISTRATION", "IAS SCHEDULER", "PKG_IAS_SCHEDULER", "P_SET_ACTIVE", ObjectType = "IAS_SCHEDULER_SCHEDULE")]
        public IActionResult SetIasSchedulerScheduleActive(long scheduleId, bool isActive)
        {
            if (!CanManageIasScheduler())
            {
                return Forbid();
            }

            if (scheduleId <= 0)
            {
                return BadRequest(new { status = false, message = "Select a valid schedule." });
            }

            try
            {
                dBConnection.SetIasSchedulerScheduleActive(scheduleId, isActive, GetIasSchedulerActor());
                return Json(new { status = true, message = isActive ? "Schedule activated." : "Schedule deactivated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update IAS scheduler active status for {ScheduleId}.", scheduleId);
                return StatusCode(500, new { status = false, message = "Unable to update the schedule status." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AIS.Filters.ApplicationAudit("REQUEST_IAS_SCHEDULER_RUN_NOW", "ADMINISTRATION", "IAS SCHEDULER", "PKG_IAS_SCHEDULER", "P_REQUEST_RUN_NOW", ObjectType = "IAS_SCHEDULER_RUN_REQUEST")]
        public IActionResult RequestIasSchedulerRunNow(IasSchedulerRunNowRequest model)
        {
            if (!CanManageIasScheduler())
            {
                return Forbid();
            }

            var errors = BuildRunNowPeriod(model, out var periodFrom, out var periodToExclusive);
            foreach (var error in errors)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            if (model == null || model.ScheduleId <= 0)
            {
                ModelState.AddModelError(nameof(IasSchedulerRunNowRequest.ScheduleId), "Select a valid schedule.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = false, message = "Please correct the highlighted fields.", errors = ExtractModelStateErrors() });
            }

            try
            {
                var requestId = dBConnection.RequestIasSchedulerRunNow(
                    model.ScheduleId,
                    periodFrom,
                    periodToExclusive,
                    GetIasSchedulerActor(),
                    model.Remarks);
                return Json(new { status = true, message = "Run Now request queued. Normal Next Due is unchanged.", requestId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to request IAS scheduler Run Now for {ScheduleId}.", model?.ScheduleId);
                return StatusCode(500, new { status = false, message = "Unable to queue the Run Now request." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AIS.Filters.ApplicationAudit("CANCEL_IAS_SCHEDULER_RUN_NOW", "ADMINISTRATION", "IAS SCHEDULER", "PKG_IAS_SCHEDULER", "P_CANCEL_RUN_REQUEST", ObjectType = "IAS_SCHEDULER_RUN_REQUEST")]
        public IActionResult CancelIasSchedulerRunRequest(long requestId)
        {
            if (!CanManageIasScheduler())
            {
                return Forbid();
            }

            if (requestId <= 0)
            {
                return BadRequest(new { status = false, message = "Select a valid Run Now request." });
            }

            try
            {
                dBConnection.CancelIasSchedulerRunRequest(requestId, GetIasSchedulerActor());
                return Json(new { status = true, message = "Pending Run Now request cancelled." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to cancel IAS scheduler run request {RequestId}.", requestId);
                return StatusCode(500, new { status = false, message = "Unable to cancel the Run Now request." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AIS.Filters.ApplicationAudit("RESOLVE_IAS_SCHEDULER_EXECUTION", "ADMINISTRATION", "IAS SCHEDULER", "PKG_IAS_SCHEDULER", "P_RESOLVE_EXECUTION", ObjectType = "IAS_SCHEDULER_EXECUTION")]
        public IActionResult ResolveIasSchedulerExecution(IasSchedulerResolveExecutionRequest model)
        {
            if (!CanManageIasScheduler())
            {
                return Forbid();
            }

            if (model == null || model.ExecutionId <= 0)
            {
                ModelState.AddModelError(nameof(IasSchedulerResolveExecutionRequest.ExecutionId), "Select a valid execution.");
            }

            if (!IsAllowedIasSchedulerResolution(model?.Resolution))
            {
                ModelState.AddModelError(nameof(IasSchedulerResolveExecutionRequest.Resolution), "Resolution must be SUCCESS, FAILED or SKIPPED.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = false, message = "Please correct the highlighted fields.", errors = ExtractModelStateErrors() });
            }

            try
            {
                dBConnection.ResolveIasSchedulerExecution(
                    model.ExecutionId,
                    model.Resolution.Trim().ToUpperInvariant(),
                    model.RecordsProcessed,
                    model.Remarks,
                    GetIasSchedulerActor());
                return Json(new { status = true, message = "Execution resolved." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to resolve IAS scheduler execution {ExecutionId}.", model?.ExecutionId);
                return StatusCode(500, new { status = false, message = "Unable to resolve the execution." });
            }
        }

        private bool CanManageIasScheduler()
        {
            return User?.Identity?.IsAuthenticated == true
                && sessionHandler.IsSuperUser()
                && this.UserHasPagePermissionForCurrentAction(sessionHandler);
        }

        private string GetIasSchedulerActor()
        {
            var user = sessionHandler.GetUser();
            return string.IsNullOrWhiteSpace(user?.PPNumber) ? user?.ID.ToString() ?? "IAS" : user.PPNumber.Trim();
        }

        private void ValidateJobDefinition(SaveIasSchedulerJobDefinitionRequest model)
        {
            if (model == null)
            {
                ModelState.AddModelError(string.Empty, "Job definition is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(model.JobCode)) ModelState.AddModelError(nameof(model.JobCode), "Job Code is required.");
            if (string.IsNullOrWhiteSpace(model.JobName)) ModelState.AddModelError(nameof(model.JobName), "Job Name is required.");
            if (!IsKnownExecutionType(model.ExecutionType)) ModelState.AddModelError(nameof(model.ExecutionType), "Execution Type must be APPLICATION or DATABASE.");
            if (string.Equals(model.ExecutionType, IasSchedulerExecutionTypes.Application, StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(model.ApplicationHandler))
            {
                ModelState.AddModelError(nameof(model.ApplicationHandler), "Application Handler is required for application jobs.");
            }
            if (string.Equals(model.ExecutionType, IasSchedulerExecutionTypes.Database, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(model.PackageName) || string.IsNullOrWhiteSpace(model.ProcedureName)))
            {
                ModelState.AddModelError(nameof(model.PackageName), "Package Name and Procedure Name are required for database jobs.");
            }
        }

        private void ValidateSchedule(SaveIasSchedulerScheduleRequest model)
        {
            if (model == null)
            {
                ModelState.AddModelError(string.Empty, "Schedule is required.");
                return;
            }

            if (model.JobId <= 0) ModelState.AddModelError(nameof(model.JobId), "Select a job definition.");
            if (model.RunHour < 0 || model.RunHour > 23) ModelState.AddModelError(nameof(model.RunHour), "Run Hour must be between 0 and 23.");
            if (model.RunMinute < 0 || model.RunMinute > 59) ModelState.AddModelError(nameof(model.RunMinute), "Run Minute must be between 0 and 59.");
            if (model.DayOfWeek.HasValue && (model.DayOfWeek.Value < 1 || model.DayOfWeek.Value > 7)) ModelState.AddModelError(nameof(model.DayOfWeek), "Day of Week must be between 1 and 7.");
            if (model.DayOfMonth.HasValue && (model.DayOfMonth.Value < 1 || model.DayOfMonth.Value > 31)) ModelState.AddModelError(nameof(model.DayOfMonth), "Day of Month must be between 1 and 31.");
            if (model.MaxRetry < 0) ModelState.AddModelError(nameof(model.MaxRetry), "Max Retry cannot be negative.");
            if (model.RetryIntervalMinutes < 0) ModelState.AddModelError(nameof(model.RetryIntervalMinutes), "Retry Interval cannot be negative.");
            if (model.EffectiveTo.HasValue && model.EffectiveTo.Value.Date < model.EffectiveFrom.Date) ModelState.AddModelError(nameof(model.EffectiveTo), "Effective To cannot be earlier than Effective From.");
        }

        private static void NormalizeJobDefinition(SaveIasSchedulerJobDefinitionRequest model, string actor)
        {
            if (model == null) return;
            model.JobCode = NormalizeUpper(model.JobCode);
            model.JobName = NormalizeText(model.JobName);
            model.ExecutionType = NormalizeUpper(model.ExecutionType);
            model.PackageName = NormalizeUpper(model.PackageName);
            model.ProcedureName = NormalizeUpper(model.ProcedureName);
            model.ApplicationHandler = NormalizeUpper(model.ApplicationHandler);
            model.Description = NormalizeText(model.Description);
            model.UpdatedBy = actor;
        }

        private static void NormalizeSchedule(SaveIasSchedulerScheduleRequest model, string actor)
        {
            if (model == null) return;
            model.FrequencyType = NormalizeUpper(model.FrequencyType);
            model.PeriodType = NormalizeUpper(model.PeriodType);
            model.Remarks = NormalizeText(model.Remarks);
            model.UpdatedBy = actor;
        }

        public static IReadOnlyDictionary<string, string> BuildRunNowPeriod(
            IasSchedulerRunNowRequest model,
            out DateTime? periodFrom,
            out DateTime? periodToExclusive)
        {
            periodFrom = null;
            periodToExclusive = null;
            var errors = new Dictionary<string, string>();
            if (model == null)
            {
                errors[string.Empty] = "Run Now request is required.";
                return errors;
            }

            var mode = NormalizeUpper(model.Mode);
            if (string.IsNullOrWhiteSpace(mode) || mode == "AUTO" || mode == "AUTOMATIC")
            {
                return errors;
            }

            if (mode != "CUSTOM")
            {
                errors[nameof(IasSchedulerRunNowRequest.Mode)] = "Run Now mode must be Automatic or Custom.";
                return errors;
            }

            if (!model.PeriodFrom.HasValue)
            {
                errors[nameof(IasSchedulerRunNowRequest.PeriodFrom)] = "Period From is required for Custom Run Now.";
            }

            if (!model.PeriodToInclusive.HasValue)
            {
                errors[nameof(IasSchedulerRunNowRequest.PeriodToInclusive)] = "Period To is required for Custom Run Now.";
            }

            if (model.PeriodFrom.HasValue && model.PeriodToInclusive.HasValue)
            {
                var from = model.PeriodFrom.Value.Date;
                var toInclusive = model.PeriodToInclusive.Value.Date;
                if (toInclusive < from)
                {
                    errors[nameof(IasSchedulerRunNowRequest.PeriodToInclusive)] = "Period To cannot be earlier than Period From.";
                }
                else
                {
                    periodFrom = from;
                    periodToExclusive = toInclusive.AddDays(1);
                }
            }

            return errors;
        }

        public static bool IsAllowedIasSchedulerResolution(string resolution)
        {
            var value = NormalizeUpper(resolution);
            return value == "SUCCESS" || value == "FAILED" || value == "SKIPPED";
        }

        private static bool IsKnownExecutionType(string value)
        {
            return string.Equals(value, IasSchedulerExecutionTypes.Application, StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, IasSchedulerExecutionTypes.Database, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string NormalizeUpper(string value)
        {
            return NormalizeText(value).ToUpperInvariant();
        }
    }
}
