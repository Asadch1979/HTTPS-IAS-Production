using AIS.Models;
using AIS.Models.Requests;
using AIS.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AIS.Services;
namespace AIS.Controllers
    {

    public class EngagementController : Controller
        {
        private readonly ILogger<EngagementController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        public EngagementController(ILogger<EngagementController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            }
        public IActionResult Index()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View("~/Views/Planning/audit_criteria.cshtml");
                }
            }
        public IActionResult task_list()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["TaskList"] = dBConnection.GetTaskList();
            var loggedInUser = sessionHandler.GetUser();
            ViewData["PPNumber"] = loggedInUser?.PPNumber ?? string.Empty;
            ViewData["EMP_NAME"] = loggedInUser?.Name ?? string.Empty;
            List<TaskListModel> tlist = dBConnection.GetTaskList();
            foreach (var item in tlist)
                {
                ViewData["TEAM_NAME"] = item.TEAM_NAME.ToString();
                }
            ViewData["TaskList"] = tlist;
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult create_audit_plan()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult engagement_plan()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditDepartments"] = dBConnection.GetDepartments(354);
            ViewData["DivisionsList"] = dBConnection.GetDivisions(false);
            ViewData["AuditZonesList"] = dBConnection.GetZones();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult ongoing_engagements_list()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["OngoingEngagementPlans"] = dBConnection.GetAuditOngoingEngagementPlans();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult audit_criteria()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditEntities"] = dBConnection.GetAuditEntities();
            ViewData["AuditPeriodList"] = dBConnection.GetAuditPeriods();
            ViewData["AuditFrequencies"] = dBConnection.GetAuditFrequencies();
            ViewData["BranchSizesList"] = dBConnection.GetBranchSizes();
            ViewData["RiskList"] = dBConnection.GetRisks();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        [HttpPost]
        public IActionResult add_audit_criteria(List<List<String>> CRITERIA_LIST)
            {
            var response = new AuditCriteriaSaveResponseModel();

            try
                {
                if (CRITERIA_LIST == null || CRITERIA_LIST.Count == 0)
                    {
                    response.Status = false;
                    response.Message = "No audit criteria rows were submitted.";
                    return BadRequest(response);
                    }

                _logger.LogInformation("add_audit_criteria received {CriteriaCount} row(s).", CRITERIA_LIST.Count);

                for (var index = 0; index < CRITERIA_LIST.Count; index++)
                    {
                    var criteria = CRITERIA_LIST[index];
                    _logger.LogInformation(
                        "add_audit_criteria row {RowIndex} received {ValueCount} value(s): {CriteriaValues}",
                        index + 1,
                        criteria?.Count ?? 0,
                        SerializeCriteriaValues(criteria));

                    var rowResponse = CreateAuditCriteriaRowResponse(index, criteria);
                    response.Rows.Add(rowResponse);

                    if (!TryBuildAuditCriteriaModel(criteria, rowResponse, out var model, out var validationMessage))
                        {
                        rowResponse.Success = false;
                        rowResponse.Message = validationMessage;
                        _logger.LogWarning(
                            "add_audit_criteria validation failed for row {RowIndex}: {ValidationMessage}. Values: {CriteriaValues}",
                            index + 1,
                            validationMessage,
                            SerializeCriteriaValues(criteria));
                        continue;
                        }

                    try
                        {
                        var dbResult = dBConnection.AddAuditCriteria(model);
                        rowResponse.Success = dbResult.Success;
                        rowResponse.Message = string.IsNullOrWhiteSpace(dbResult.Message)
                            ? (dbResult.Success ? "Criteria successfully added." : "Criteria already defined.")
                            : dbResult.Message;
                        }
                    catch (Exception ex)
                        {
                        rowResponse.Success = false;
                        rowResponse.Message = "Unable to add this audit criteria row right now.";
                        _logger.LogError(
                            ex,
                            "add_audit_criteria failed while saving row {RowIndex}. Values: {CriteriaValues}",
                            index + 1,
                            SerializeCriteriaValues(criteria));
                        }
                    }

                var successCount = response.Rows.Count(r => r.Success);
                var failedCount = response.Rows.Count - successCount;
                response.Status = failedCount == 0;
                response.Message = $"Processed {response.Rows.Count} audit criteria row(s): {successCount} succeeded, {failedCount} failed.";
                return Json(response);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unhandled exception in add_audit_criteria. Row count: {CriteriaCount}", CRITERIA_LIST?.Count ?? 0);
                response.Status = false;
                response.Message = "Unable to add audit criteria right now. Please try again.";
                return StatusCode(500, response);
                }
            }
        [HttpPost]
        public AuditEntitiesModel add_auditee_entity(AuditEntitiesModel am)
            {
            am.AUTID = 0;
            return dBConnection.AddAuditEntity(am);
            }

        private static AuditCriteriaRowResponseModel CreateAuditCriteriaRowResponse(int index, IReadOnlyList<string> criteria)
            {
            return new AuditCriteriaRowResponseModel
                {
                RowIndex = index + 1,
                AuditPeriod = GetCriteriaValue(criteria, 7),
                EntityName = GetCriteriaValue(criteria, 8),
                Risk = GetCriteriaValue(criteria, 9),
                Size = GetCriteriaValue(criteria, 10),
                Frequency = GetCriteriaValue(criteria, 11)
                };
            }

        private static bool TryBuildAuditCriteriaModel(
            IReadOnlyList<string> criteria,
            AuditCriteriaRowResponseModel rowResponse,
            out AddAuditCriteriaModel model,
            out string validationMessage)
            {
            model = null;
            validationMessage = string.Empty;

            if (criteria == null)
                {
                validationMessage = "Criteria row is missing.";
                return false;
                }

            if (criteria.Count < 12)
                {
                validationMessage = $"Criteria row is malformed. Expected at least 12 values but received {criteria.Count}.";
                return false;
                }

            if (!TryGetRequiredInt(criteria, 0, "Audit period ID", out var auditPeriodId, out validationMessage)
                || !TryGetRequiredInt(criteria, 1, "Entity type ID", out var entityTypeId, out validationMessage)
                || !TryGetRequiredInt(criteria, 2, "Risk ID", out var riskId, out validationMessage)
                || !TryGetRequiredInt(criteria, 3, "Frequency ID", out var frequencyId, out validationMessage)
                || !TryGetRequiredInt(criteria, 4, "Size ID", out var sizeId, out validationMessage)
                || !TryGetRequiredInt(criteria, 5, "Number of days", out var noOfDays, out validationMessage))
                {
                return false;
                }

            var entityId = 0;
            var entityIdRaw = GetCriteriaValue(criteria, 12);
            if (!string.IsNullOrWhiteSpace(entityIdRaw))
                {
                var parsedEntityId = NumericParsing.ToNullableInt(entityIdRaw);
                if (!parsedEntityId.HasValue)
                    {
                    validationMessage = $"Entity ID value '{entityIdRaw}' is not numeric.";
                    return false;
                    }

                entityId = parsedEntityId.Value;
                }

            model = new AddAuditCriteriaModel
                {
                ID = 0,
                AUDITPERIODID = auditPeriodId,
                ENTITY_TYPEID = entityTypeId,
                RISK_ID = riskId,
                FREQUENCY_ID = frequencyId,
                SIZE_ID = sizeId,
                NO_OF_DAYS = noOfDays,
                VISIT = NormalizeVisit(GetCriteriaValue(criteria, 6)),
                APPROVAL_STATUS = 1,
                AUDITPERIOD = GetCriteriaValue(criteria, 7),
                ENTITY_NAME = GetCriteriaValue(criteria, 8),
                RISK = GetCriteriaValue(criteria, 9),
                SIZE = GetCriteriaValue(criteria, 10),
                FREQUENCY = GetCriteriaValue(criteria, 11),
                ENTITY_ID = entityId
                };

            rowResponse.AuditPeriod = model.AUDITPERIOD;
            rowResponse.EntityName = model.ENTITY_NAME;
            rowResponse.Risk = model.RISK;
            rowResponse.Size = model.SIZE;
            rowResponse.Frequency = model.FREQUENCY;
            return true;
            }

        private static bool TryGetRequiredInt(IReadOnlyList<string> criteria, int index, string fieldName, out int value, out string validationMessage)
            {
            value = 0;
            validationMessage = string.Empty;

            if (criteria == null || index < 0 || index >= criteria.Count)
                {
                validationMessage = $"{fieldName} is missing.";
                return false;
                }

            var rawValue = criteria[index];
            if (string.IsNullOrWhiteSpace(rawValue))
                {
                validationMessage = $"{fieldName} is blank.";
                return false;
                }

            var parsedValue = NumericParsing.ToNullableInt(rawValue);
            if (!parsedValue.HasValue)
                {
                validationMessage = $"{fieldName} value '{rawValue}' is not numeric.";
                return false;
                }

            value = parsedValue.Value;
            return true;
            }

        private static string NormalizeVisit(string visitValue)
            {
            return string.Equals(visitValue, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(visitValue, "y", StringComparison.OrdinalIgnoreCase)
                ? "Y"
                : "N";
            }

        private static string GetCriteriaValue(IReadOnlyList<string> criteria, int index)
            {
            if (criteria == null || index < 0 || index >= criteria.Count)
                {
                return string.Empty;
                }

            return criteria[index] ?? string.Empty;
            }

        private static string SerializeCriteriaValues(IReadOnlyList<string> criteria)
            {
            if (criteria == null)
                {
                return "<null>";
                }

            if (criteria.Count == 0)
                {
                return "<empty>";
                }

            return string.Join(" | ", criteria.Select((value, index) =>
                $"[{index}]={(!string.IsNullOrWhiteSpace(value) ? value.Trim() : "<blank>")}"));
            }
        public IActionResult submission_for_review()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult Join()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public JoiningModel get_joining_details([FromForm] int? engId = null)
            {
            return dBConnection.GetJoiningDetails(engId ?? 0);
            }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult add_joining_report([FromForm] AddJoiningPostModel jm)
            {
            if (jm == null || !jm.ENG_PLAN_ID.HasValue || !jm.TEAM_MEM_PPNO.HasValue)
                {
                return Json(new
                    {
                    status = false,
                    message = "Invalid input data"
                    });
                }

            var model = new AddJoiningModel
                {
                ID = jm.ID ?? 0,
                ENG_PLAN_ID = jm.ENG_PLAN_ID ?? 0,
                TEAM_MEM_PPNO = jm.TEAM_MEM_PPNO ?? 0,
                JOINING_DATE = jm.JOINING_DATE,
                COMPLETION_DATE = jm.COMPLETION_DATE
                };

            var message = dBConnection.AddJoiningReport(model);
            return Json(new
                {
                status = true,
                message = message
                });
            }
        public IActionResult acceptance()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult change_request()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult eng_plan_approvals()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult eng_plan_list()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["EngagementPlans"] = dBConnection.GetAuditEngagementPlans();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult eng_plan_ref_list()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["EngagementPlans"] = dBConnection.GetRefferedBackAuditEngagementPlans();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult submission_for_approval()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult post_changes_approved_plan()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult post_changes_team_members()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult notifications()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult preparation_ccqs()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }


        public IActionResult ccqs()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
