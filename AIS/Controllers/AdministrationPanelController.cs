using System;
using AIS.Models;
using AIS.Filters;
using AIS.Models.Requests;
using AIS.Models.WorkflowDashboard;
using AIS.Security.PasswordPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AIS.Services;

namespace AIS.Controllers
    {

    public class AdministrationPanelController : Controller
        {
        private readonly ILogger<AdministrationPanelController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;
        private readonly DBConnection dBConnection;
        private readonly PasswordPolicyValidator _passwordPolicyValidator;
        public AdministrationPanelController(ILogger<AdministrationPanelController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService, PasswordPolicyValidator passwordPolicyValidator, IPageIdResolver pageIdResolver)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _passwordPolicyValidator = passwordPolicyValidator;
            _pageIdResolver = pageIdResolver;
            }

        [HttpGet]
        public IActionResult User_Dashboard(string stepKey = null)
            {
            PopulateDashboardChrome();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!HasPageAccess(user, "/AdministrationPanel/User_Dashboard"))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            var model = BuildUserDashboardViewModel(user, stepKey);
            if (!model.VisibleSteps.Any())
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            return View("~/Views/AdministrationPanel/User_Dashboard.cshtml", model);
            }

        [HttpGet]
        public IActionResult LoadUserDashboardStep(string stepKey)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized();
                }

            if (!TryGetRequestedStep(BuildUserDashboardViewModel(user, stepKey), stepKey, out var step, out var errorResult))
                {
                if (errorResult is ForbidResult)
                    {
                    return CreateStepAccessDeniedResult();
                    }

                return errorResult;
                }

            PopulateAdministrationDashboardStepViewData(step.StepKey, step.RequiredPermissionPageId);
            return PartialView(step.PartialViewName);
            }

        [HttpGet]
        public IActionResult Entity_Dashboard(string stepKey = null)
            {
            PopulateDashboardChrome();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!HasPageAccess(user, "/AdministrationPanel/Entity_Dashboard"))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            var model = BuildEntityDashboardViewModel(user, stepKey);
            if (!model.VisibleSteps.Any())
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            return View("~/Views/AdministrationPanel/Entity_Dashboard.cshtml", model);
            }

        [HttpGet]
        public IActionResult LoadEntityDashboardStep(string stepKey)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized();
                }

            if (!TryGetRequestedStep(BuildEntityDashboardViewModel(user, stepKey), stepKey, out var step, out var errorResult))
                {
                if (errorResult is ForbidResult)
                    {
                    return CreateStepAccessDeniedResult();
                    }

                return errorResult;
                }

            PopulateAdministrationDashboardStepViewData(step.StepKey, step.RequiredPermissionPageId);
            return PartialView(step.PartialViewName);
            }

        public IActionResult MasterAdminControlPanel()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AdminMenuList"] = dBConnection.GetAllMenusForAdminPanel();
            ViewData["MenuList"] = dBConnection.GetAllMenusForAdminPanel();
            ViewData["TopMenuList"] = dBConnection.GetAllTopMenus();
            ViewData["MenuPagesList"] = dBConnection.GetAllMenuPages();
            ViewData["RoleList"] = dBConnection.GetRolesForComplianceFlow();
            ViewData["UserList"] = dBConnection.GetAuditEmployees();
            ViewData["IsAdvancedUser"] = sessionHandler.IsSuperUser();
            ViewData["CanUploadCatalogs"] = sessionHandler.IsSuperUser();

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
                    return View("MasterAdminControlPanel");
                }
            }



        public IActionResult entity_heirarchy()
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

        public IActionResult audit_criteria()
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
        public IActionResult manage_user()
            {

            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["GroupList"] = dBConnection.GetGroups();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
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
        public IActionResult audit_period()
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

        public IActionResult setup_engagement_reversal()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            ViewData["statusList"] = dBConnection.GetObservationReversalStatus();

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

        public IActionResult manage_obs_status()
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
        public IActionResult manage_ent_audit_dept()
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

        public IActionResult manage_user_rights()
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
        public IActionResult audit_comp_management()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                ViewData["AZOfficeList"] = new List<AuditZoneModel>();
                ViewData["ComplianceUnitsList"] = new List<AuditeeEntitiesModel>();
                }
            else if (loggedInUser.UserRoleID == 1)
                {
                ViewData["AZOfficeList"] = dBConnection.GetAuditZones();
                ViewData["ComplianceUnitsList"] = dBConnection.GetComplianceUnits();
                }
            else if (loggedInUser.UserRoleID == 2)
                {
                ViewData["AZOfficeList"] = dBConnection.GetAuditZones();
                ViewData["ComplianceUnitsList"] = new List<AuditeeEntitiesModel>();
                }
            else if (loggedInUser.UserRoleID == 41)
                {
                ViewData["ComplianceUnitsList"] = dBConnection.GetComplianceUnits();
                ViewData["AZOfficeList"] = new List<AuditZoneModel>();
                }
            else
                {
                ViewData["ComplianceUnitsList"] = new List<AuditeeEntitiesModel>();
                ViewData["AZOfficeList"] = new List<AuditZoneModel>();
                }

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
        public IActionResult gm_repo_line_management()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                ViewData["GMOffList"] = new List<AuditeeEntitiesModel>();
                ViewData["ReportingOffList"] = new List<AuditeeEntitiesModel>();
                }
            else if (loggedInUser.UserRoleID == 1)
                {
                ViewData["GMOffList"] = dBConnection.GetGMOffices();
                ViewData["ReportingOffList"] = dBConnection.GetReportingOffices();
                }


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

        public IActionResult entity_gm_reporting_div_management()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                ViewData["AZOfficeList"] = new List<AuditZoneModel>();
                ViewData["ComplianceUnitsList"] = new List<AuditeeEntitiesModel>();
                }
            else if (loggedInUser.UserRoleID == 1)
                {
                ViewData["AZOfficeList"] = dBConnection.GetAuditZones();
                ViewData["ComplianceUnitsList"] = dBConnection.GetComplianceUnits();
                }
            else if (loggedInUser.UserRoleID == 2)
                {
                ViewData["AZOfficeList"] = dBConnection.GetAuditZones();
                ViewData["ComplianceUnitsList"] = new List<AuditeeEntitiesModel>();
                }
            else if (loggedInUser.UserRoleID == 41)
                {
                ViewData["ComplianceUnitsList"] = dBConnection.GetComplianceUnits();
                ViewData["AZOfficeList"] = new List<AuditZoneModel>();
                }
            else
                {
                ViewData["ComplianceUnitsList"] = new List<AuditeeEntitiesModel>();
                ViewData["AZOfficeList"] = new List<AuditZoneModel>();
                }

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
        public IActionResult entity_addition()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            var pageId = ViewData["PageId"] as int? ?? 0;
            ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
            ViewData["RelationshipList"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            ViewData["Audit_By"] = dBConnection.GetAuditBy();

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
        public IActionResult entity_shifting()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            var pageId = ViewData["PageId"] as int? ?? 0;
            ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
            ViewData["RelationshipList"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            ViewData["Audit_By"] = dBConnection.GetAuditBy();

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

        [HttpGet]
        public async Task<IActionResult> EntityShiftingHistory()
            {
            if (User.Identity?.IsAuthenticated != true || !sessionHandler.TryGetUser(out var user) || user == null)
                return RedirectToAction("Index", "Login");

            if (!HasPageAccess(user, "/AdministrationPanel/EntityShiftingHistory"))
                return RedirectToAction("Index", "PageNotFound");

            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            try
                {
                var records = await dBConnection.GetEntityShiftingHistoryAsync();
                return View(new EntityShiftingHistoryPageModel { ShiftingRecords = records });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unable to retrieve entity shifting history.");
                TempData["ErrorMessage"] = "Unable to load entity shifting history.";
                return View(new EntityShiftingHistoryPageModel());
                }
            }

        [HttpGet]
        public async Task<IActionResult> GetEntityShiftingParas(int refId)
            {
            if (User.Identity?.IsAuthenticated != true || !sessionHandler.TryGetUser(out var user) || user == null)
                return Unauthorized(new { success = false, message = "Your session has expired." });

            if (!HasPageAccess(user, "/AdministrationPanel/EntityShiftingHistory"))
                return Forbid();

            if (refId <= 0)
                return BadRequest(new { success = false, message = "A valid shifting reference is required." });

            try
                {
                var paras = await dBConnection.GetEntityShiftingParasAsync(refId);
                return Ok(new { success = true, data = paras });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unable to retrieve paras for shifting reference {RefId}.", refId);
                return StatusCode(500, new { success = false, message = "Unable to retrieve shifted paras." });
                }
            }

        public IActionResult groups()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["GroupsList"] = dBConnection.GetGroups();
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
        public IActionResult group_role_assignment()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["GroupList"] = dBConnection.GetGroups();
            ViewData["MenuList"] = dBConnection.GetAllTopMenus();
            ViewData["MenuPagesList"] = dBConnection.GetAllMenuPages();
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
        public IActionResult menu_assignment()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["GroupList"] = dBConnection.GetGroups();
            ViewData["MenuList"] = dBConnection.GetAllTopMenus();
            ViewData["MenuPagesList"] = dBConnection.GetAllMenuPages();
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
        public IActionResult setup_auditee_entities()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            var pageId = ViewData["PageId"] as int? ?? 0;
            ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
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

        public IActionResult update_auditee_entities()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            var pageId = ViewData["PageId"] as int? ?? 0;
            ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
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

        public IActionResult authorize_auditee_entities_update()
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
        public IActionResult risk_model()
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
        public IActionResult user_roles()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["UserList"] = dBConnection.GetAuditEmployees();
            ViewData["MenuList"] = dBConnection.GetAllTopMenus();
            ViewData["MenuPagesList"] = dBConnection.GetAllMenuPages();
            ViewData["GroupList"] = dBConnection.GetGroups();
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

        public IActionResult review_audit_checklist()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            // status ids required 1, 4 but 4 pass to procedure will bring 1 & 4 both processes
            ViewData["TransactionsList"] = dBConnection.GetUpdatedChecklistDetailsForReviewAndAuthorize(4);
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
        public IActionResult authorize_audit_checklist()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["TransactionsList"] = dBConnection.GetUpdatedChecklistDetailsForReviewAndAuthorize(3);
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

        public IActionResult manage_entity_type()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditedByList"] = dBConnection.GetAuditBy();
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

        public IActionResult manage_entity_relations()
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

        public IActionResult manage_entity_mapping()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            ViewData["EntitiesType"] = dBConnection.GetAuditeeEntities();
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

        public IActionResult compliance_flow()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["GroupList"] = dBConnection.GetRolesForComplianceFlow();
            ViewData["EntitiesList"] = dBConnection.GetEntityTypesForComplianceFlow();
            ViewData["StatusList"] = dBConnection.GetComplianceStatusesForComplianceFlow();
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

        public IActionResult hr_design_wise_role()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["HRDesigList"] = dBConnection.GetHRDesignation();
            ViewData["GroupList"] = dBConnection.GetRolesForComplianceFlow();
            ViewData["EntitiesList"] = dBConnection.GetEntityTypesForHRDesignationWiseRole();
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
        public IActionResult menu_management()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["MenuList"] = dBConnection.GetAllMenusForAdminPanel();

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
        public IActionResult sub_menu_management()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["MenuList"] = dBConnection.GetAllMenusForAdminPanel();

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
        public IActionResult pages_management()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["MenuList"] = dBConnection.GetAllMenusForAdminPanel();

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

        public IActionResult ManagePublicHolidays()
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
        public List<MenuPagesModel> menu_pages([FromForm] int? MENU_ID = null)
            {
            return dBConnection.GetAllMenuPages(MENU_ID ?? 0);
            }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public List<MenuPagesModel> assigned_menu_pages([FromForm] int? GROUP_ID, [FromForm] int? MENU_ID)
            {
            return dBConnection.GetAssignedMenuPages(GROUP_ID ?? 0, MENU_ID ?? 0);
            }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public GroupMenuItemMapping add_group_item_assignment([FromForm] GroupMenuItemMapping gItemMap)
            {
            if (gItemMap?.UNLINK_MENU_ITEM_IDs != null && gItemMap.UNLINK_MENU_ITEM_IDs.Count > 0)
                {
                foreach (var id in gItemMap.UNLINK_MENU_ITEM_IDs)
                    {
                    dBConnection.RemoveGroupMenuItemsAssignment(gItemMap.GROUP_ID, id);
                    }
                }

            if (gItemMap?.MENU_ITEM_IDs != null && gItemMap.MENU_ITEM_IDs.Count > 0)
                {
                foreach (var id in gItemMap.MENU_ITEM_IDs)
                    {
                    dBConnection.AddGroupMenuItemsAssignment(gItemMap.GROUP_ID, id);
                    }
                }
            return gItemMap;
            }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public GroupModel group_add([FromForm] GroupPostModel grp)
            {
            if (grp == null)
                {
                return null;
                }

            var group = new GroupModel
                {
                GROUP_ID = grp.GROUP_ID,
                GROUP_NAME = grp.GROUP_NAME,
                GROUP_DESCRIPTION = grp.GROUP_DESCRIPTION,
                ISACTIVE = grp.ISACTIVE,
                GROUP_CODE = grp.GROUP_CODE
                };

            return dBConnection.AddGroup(group);

            }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult update_user([FromForm] UpdateUserPostModel user)
            {
            var formKeys = HttpContext?.Request?.HasFormContentType == true
                ? HttpContext.Request.Form.Keys.ToArray()
                : Array.Empty<string>();

            _logger.LogInformation("update_user received form keys: {Keys}", string.Join(", ", formKeys));

            if (user == null)
                {
                _logger.LogWarning("update_user model binding failed. Form keys: {Keys}", string.Join(", ", formKeys));
                return BadRequest(new { status = false, message = "Invalid request.", errors = new[] { "No user payload was provided." } });
                }

            if (!user.USER_ID.HasValue || !user.ROLE_ID.HasValue || !user.ENTITY_ID.HasValue || string.IsNullOrWhiteSpace(user.PPNO))
                {
                _logger.LogWarning("update_user model validation failed for USER_ID {UserId}.", user.USER_ID);
                return BadRequest(new { status = false, message = "Invalid input data." });
                }

            var request = new SaveUserContextsPostModel
                {
                USER_ID = user.USER_ID,
                PPNO = user.PPNO,
                PASSWORD = user.PASSWORD,
                EMAIL_ADDRESS = user.EMAIL_ADDRESS,
                ISACTIVE = user.ISACTIVE,
                ASSIGNMENTS = new List<UserContextAssignmentPostModel>
                    {
                    new UserContextAssignmentPostModel
                        {
                        ROLE_ID = user.ROLE_ID,
                        GROUP_ID = user.ROLE_ID,
                        ENTITY_ID = user.ENTITY_ID,
                        ISDEFAULT = "Y",
                        ISACTIVE = string.Equals(user.ISACTIVE, "Y", StringComparison.OrdinalIgnoreCase) ? "Y" : "N"
                        }
                    }
                };

            return SaveUserContextsInternal(request, "update_user");
            }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Consumes("application/json")]
        public IActionResult save_user_contexts([FromBody] SaveUserContextsPostModel user)
            {
            return SaveUserContextsInternal(user, "save_user_contexts");
            }

        private IActionResult SaveUserContextsInternal(SaveUserContextsPostModel user, string source)
            {
            if (user == null)
                {
                _logger.LogWarning("{Source} received no payload.", source);
                return BadRequest(new { status = false, message = "Invalid request." });
                }

            if (string.IsNullOrWhiteSpace(user.PPNO))
                {
                _logger.LogWarning("{Source} missing PP number.", source);
                return BadRequest(new { status = false, message = "PP number is required." });
                }

            var assignments = NormalizeAssignmentsForValidation(user.ASSIGNMENTS);
            if (!TryValidateAssignments(assignments, source, user.PPNO, out var assignmentError))
                {
                return assignmentError;
                }

            if (!string.IsNullOrWhiteSpace(user.PASSWORD))
                {
                var validation = _passwordPolicyValidator.Validate(user.PASSWORD, user.PPNO);
                if (!validation.IsValid)
                    {
                    _logger.LogWarning("{Source} password policy failed for PP {PPNO}: {Message}", source, user.PPNO, validation.ErrorMessage);
                    return Json(new { status = false, message = validation.ErrorMessage });
                    }
                }

            user.ASSIGNMENTS = assignments;
            _logger.LogInformation("{Source} processing {AssignmentCount} assignments for USER_ID {UserId} / PP {PPNO}.", source, assignments.Count, user.USER_ID, user.PPNO);

            var response = dBConnection.SaveUserContextAssignments(user);
            if (!IsSuccessfulSaveResponse(response))
                {
                var failureMessage = string.IsNullOrWhiteSpace(response) ? "Unable to save user assignments." : response.Trim();
                _logger.LogWarning("{Source} failed for USER_ID {UserId} / PP {PPNO}: {Message}", source, user.USER_ID, user.PPNO, failureMessage);
                return BadRequest(new { status = false, message = failureMessage });
                }

            _logger.LogInformation("{Source} succeeded for USER_ID {UserId} / PP {PPNO}.", source, user.USER_ID, user.PPNO);
            return Json(new
                {
                status = true,
                message = string.IsNullOrWhiteSpace(response) || string.Equals(response.Trim(), "OK", StringComparison.OrdinalIgnoreCase)
                    ? "User assignments saved successfully."
                    : response.Trim(),
                user
                });
            }

        private bool TryValidateAssignments(List<UserContextAssignmentPostModel> assignments, string source, string ppNumber, out IActionResult errorResult)
            {
            errorResult = null;
            var visibleAssignments = assignments
                .Where(item => item != null && !item.ISDELETED)
                .ToList();
            var activeAssignments = visibleAssignments
                .Where(item => string.Equals(item.ISACTIVE, "Y", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (visibleAssignments.Count == 0 || activeAssignments.Count == 0)
                {
                _logger.LogWarning("{Source} missing active assignments for PP {PPNO}.", source, ppNumber);
                errorResult = BadRequest(new { status = false, message = "At least one active role and posting assignment is required." });
                return false;
                }

            var invalidAssignment = visibleAssignments
                .Select((item, index) => new { Assignment = item, RowNumber = index + 1 })
                .FirstOrDefault(item =>
                    !item.Assignment.ROLE_ID.HasValue
                    || item.Assignment.ROLE_ID.Value <= 0
                    || !item.Assignment.ENTITY_ID.HasValue
                    || item.Assignment.ENTITY_ID.Value <= 0);

            if (invalidAssignment != null)
                {
                _logger.LogWarning("{Source} received an incomplete assignment row {RowNumber} for PP {PPNO}.", source, invalidAssignment.RowNumber, ppNumber);
                errorResult = BadRequest(new { status = false, message = $"Assignment row {invalidAssignment.RowNumber} must include one role and one entity." });
                return false;
                }

            var duplicateAssignments = visibleAssignments
                .Select((item, index) => new
                    {
                    Assignment = item,
                    RowNumber = index + 1,
                    RoleId = item.ROLE_ID.GetValueOrDefault(),
                    GroupId = (item.GROUP_ID ?? item.ROLE_ID).GetValueOrDefault(),
                    EntityId = item.ENTITY_ID.GetValueOrDefault()
                    })
                .GroupBy(item => new { item.RoleId, item.GroupId, item.EntityId })
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicateAssignments != null)
                {
                var duplicateRows = string.Join(", ", duplicateAssignments.Select(item => item.RowNumber));
                _logger.LogWarning("{Source} received duplicate assignments for PP {PPNO} in rows {Rows}.", source, ppNumber, duplicateRows);
                errorResult = BadRequest(new { status = false, message = $"Duplicate role and posting assignments are not allowed. Check rows {duplicateRows}." });
                return false;
                }

            var defaultAssignments = visibleAssignments
                .Select((item, index) => new { Assignment = item, RowNumber = index + 1 })
                .Where(item => string.Equals(item.Assignment.ISDEFAULT, "Y", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (defaultAssignments.Count == 0)
                {
                _logger.LogWarning("{Source} received no default assignment for PP {PPNO}.", source, ppNumber);
                errorResult = BadRequest(new { status = false, message = "Mark one active assignment as the default before saving." });
                return false;
                }

            if (defaultAssignments.Count > 1)
                {
                var duplicateDefaultRows = string.Join(", ", defaultAssignments.Select(item => item.RowNumber));
                _logger.LogWarning("{Source} received multiple default assignments for PP {PPNO} in rows {Rows}.", source, ppNumber, duplicateDefaultRows);
                errorResult = BadRequest(new { status = false, message = $"Only one assignment can be marked as default. Check rows {duplicateDefaultRows}." });
                return false;
                }

            if (!string.Equals(defaultAssignments[0].Assignment.ISACTIVE, "Y", StringComparison.OrdinalIgnoreCase))
                {
                _logger.LogWarning("{Source} received an inactive default assignment for PP {PPNO} in row {RowNumber}.", source, ppNumber, defaultAssignments[0].RowNumber);
                errorResult = BadRequest(new { status = false, message = $"Default assignment row {defaultAssignments[0].RowNumber} must be active." });
                return false;
                }

            return true;
            }

        private static List<UserContextAssignmentPostModel> NormalizeAssignmentsForValidation(IEnumerable<UserContextAssignmentPostModel> assignments)
            {
            return (assignments ?? new List<UserContextAssignmentPostModel>())
                .Where(item => item != null)
                .Select(item => new UserContextAssignmentPostModel
                    {
                    ASSIGNMENT_ID = item.ASSIGNMENT_ID ?? item.USER_CONTEXT_ID,
                    USER_CONTEXT_ID = item.USER_CONTEXT_ID ?? item.ASSIGNMENT_ID,
                    ROLE_ID = item.ROLE_ID,
                    GROUP_ID = item.GROUP_ID ?? item.ROLE_ID,
                    ENTITY_ID = item.ENTITY_ID,
                    PARENT_ENTITY_ID = item.PARENT_ENTITY_ID,
                    RELATIONSHIP_TYPE_ID = item.RELATIONSHIP_TYPE_ID,
                    ISDEFAULT = NormalizeAssignmentFlag(item.ISDEFAULT, "N"),
                    ISACTIVE = NormalizeAssignmentFlag(item.ISACTIVE, "Y"),
                    ASSIGNMENT_TYPE = string.IsNullOrWhiteSpace(item.ASSIGNMENT_TYPE) ? "MANUAL" : item.ASSIGNMENT_TYPE.Trim(),
                    EFFECTIVE_FROM = string.IsNullOrWhiteSpace(item.EFFECTIVE_FROM) ? null : item.EFFECTIVE_FROM.Trim(),
                    EFFECTIVE_TO = string.IsNullOrWhiteSpace(item.EFFECTIVE_TO) ? null : item.EFFECTIVE_TO.Trim(),
                    REMARKS = string.IsNullOrWhiteSpace(item.REMARKS) ? null : item.REMARKS.Trim(),
                    ISDELETED = item.ISDELETED
                    })
                .ToList();
            }

        private static string NormalizeAssignmentFlag(string value, string defaultValue)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return defaultValue;
                }

            return string.Equals(value.Trim(), "Y", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
            }

        private static bool IsSuccessfulSaveResponse(string response)
            {
            if (string.IsNullOrWhiteSpace(response))
                {
                return true;
                }

            var normalized = response.Trim();
            return string.Equals(normalized, "OK", StringComparison.OrdinalIgnoreCase)
                || normalized.IndexOf("success", StringComparison.OrdinalIgnoreCase) >= 0;
            }

        private Dictionary<string, string[]> ExtractModelStateErrors()
            {
            return ModelState
                .Where(entry => entry.Value.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value.Errors.Select(error => error.ErrorMessage).ToArray());
            }

        public IActionResult ais_post_compliance()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
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

        public IActionResult ManageVersionHistory()
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
                    {
                    // Use DBConnection directly
                    List<VersionHistoryModel> versionList = dBConnection.GetAllVersionHistory();
                    return View(versionList);
                    }
                }
            }

        public IActionResult SystemLogs()
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
                    {
                    return View();
                    }
                }
            }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult GetSystemLogs(string start, string end, string logLevel, string module, string userPpno, int? engId)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler))
                {
                return Forbid();
                }

            if (!ModelState.IsValid)
                {
                return BadRequest(new
                    {
                    status = false,
                    errors = ModelState
                    });
                }

            DateTime? startTime = ParseSystemLogDateTime(start);
            DateTime? endTime = ParseSystemLogDateTime(end);

            var logs = dBConnection.GetSystemLogs(startTime, endTime, logLevel, module, userPpno, engId);
            return Json(new
                {
                status = true,
                data = logs
                });
            }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult DeleteSystemLogs(string cutoffTime)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler))
                {
                return Forbid();
                }

            DateTime? cutoff = ParseSystemLogDateTime(cutoffTime);
            if (!cutoff.HasValue)
                {
                return BadRequest(new
                    {
                    status = false,
                    message = "Cutoff time is required."
                    });
                }

            int deletedCount = dBConnection.DeleteOldSystemLogs(cutoff.Value);
            return Json(new
                {
                status = true,
                deletedCount = deletedCount
                });
            }

        public IActionResult SystemErrorMonitoring()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            return View();
            }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult GetSystemErrors(SystemErrorFilter filter)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler))
                {
                return Forbid();
                }

            NormalizeSystemErrorFilter(filter);
            return Json(new { status = true, data = dBConnection.GetSystemErrors(filter) });
            }

        [HttpGet]
        public IActionResult SystemErrorDetail(long id)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            return View(dBConnection.GetSystemErrorDetail(id));
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResolveSystemError(long errorId, string remarks)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler) || !sessionHandler.IsSuperUser())
                {
                return Forbid();
                }

            if (string.IsNullOrWhiteSpace(remarks))
                {
                return BadRequest(new { status = false, message = "Resolution remarks are required." });
                }

            var user = sessionHandler.GetUser();
            dBConnection.ResolveSystemError(errorId, user?.PPNumber, remarks.Trim());
            return Json(new { status = true, message = "Error marked as resolved." });
            }

        [HttpGet]
        public IActionResult ExportSystemErrors(SystemErrorFilter filter)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler))
                {
                return Forbid();
                }

            NormalizeSystemErrorFilter(filter);
            var bytes = Encoding.UTF8.GetBytes(BuildSystemErrorExcelXml(dBConnection.GetSystemErrors(filter)));
            return File(bytes, "application/vnd.ms-excel", $"IAS-System-Errors-{DateTime.UtcNow:yyyyMMddHHmmss}.xls");
            }

        private DateTime? ParseSystemLogDateTime(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return null;
                }

            if (DateTime.TryParseExact(value, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                {
                return parsed;
                }

            _logger.LogWarning("Failed to parse system log datetime value: {Value}", value);
            return null;
            }

        private static void NormalizeSystemErrorFilter(SystemErrorFilter filter)
            {
            if (filter == null)
                {
                return;
                }

            filter.Status = string.IsNullOrWhiteSpace(filter.Status) ? "OPEN" : filter.Status.Trim().ToUpperInvariant();
            if (!filter.FromDate.HasValue && !filter.ToDate.HasValue)
                {
                var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetBusinessTimeZone()).Date;
                ApplyBusinessDateRange(filter, today, today);
                }
            else
                {
                ApplyBusinessDateRange(filter, filter.FromDate?.Date, filter.ToDate?.Date);
                }
            }

        private static void ApplyBusinessDateRange(SystemErrorFilter filter, DateTime? fromLocalDate, DateTime? toLocalDate)
            {
            var businessTimeZone = GetBusinessTimeZone();
            filter.FromDate = fromLocalDate.HasValue
                ? TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(fromLocalDate.Value.Date, DateTimeKind.Unspecified), businessTimeZone)
                : null;
            filter.ToDate = toLocalDate.HasValue
                ? TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(toLocalDate.Value.Date.AddDays(1), DateTimeKind.Unspecified), businessTimeZone).AddTicks(-1)
                : null;
            }

        private static TimeZoneInfo GetBusinessTimeZone()
            {
            try
                {
                return TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                }
            catch (TimeZoneNotFoundException)
                {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");
                }
            catch (InvalidTimeZoneException)
                {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");
                }
            }

        private static string BuildSystemErrorExcelXml(IEnumerable<SystemErrorSummaryModel> rows)
            {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\"?>");
            builder.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            builder.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"><Worksheet ss:Name=\"System Errors\"><Table>");
            AppendExcelRow(builder, "Error Reference", "Status", "Module", "Controller / Action / API", "Error Type / Code", "Stored Procedure", "PPNO", "Role", "Entity", "Client IP", "First Occurrence", "Last Occurrence", "Occurrence Count", "Resolved By", "Resolved On", "Resolution Remarks");
            foreach (var row in rows ?? Enumerable.Empty<SystemErrorSummaryModel>())
                {
                AppendExcelRow(builder, row.ErrorReference, row.ResolutionStatus, row.Module, $"{row.Controller} / {row.Action} / {row.ApiPath}", $"{row.ErrorType} / {row.ErrorCode}", row.StoredProcedure, row.Ppno, row.Role, row.Entity, row.ClientIp, row.FirstOccurrenceUtc.ToString("u", CultureInfo.InvariantCulture), row.LastOccurrenceUtc.ToString("u", CultureInfo.InvariantCulture), row.OccurrenceCount.ToString(CultureInfo.InvariantCulture), row.ResolvedBy, row.ResolvedOnUtc?.ToString("u", CultureInfo.InvariantCulture), row.ResolutionRemarks);
                }

            builder.AppendLine("</Table></Worksheet></Workbook>");
            return builder.ToString();
            }

        private static void AppendExcelRow(StringBuilder builder, params string[] cells)
            {
            builder.AppendLine("<Row>");
            foreach (var cell in cells ?? Array.Empty<string>())
                {
                builder.Append("<Cell><Data ss:Type=\"String\">");
                builder.Append(WebUtility.HtmlEncode(cell ?? string.Empty));
                builder.AppendLine("</Data></Cell>");
                }
            builder.AppendLine("</Row>");
            }

        private void PopulateDashboardChrome()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["HideTopHeader"] = true;
            }

        private WorkflowDashboardViewModel BuildUserDashboardViewModel(SessionUser user, string requestedStepKey)
            {
            var steps = BuildUserDashboardSteps();
            return BuildDashboardViewModel(user, "USER_ACCESS_ADMIN", "User & Access Administration Workspace", requestedStepKey, steps);
            }

        private WorkflowDashboardViewModel BuildEntityDashboardViewModel(SessionUser user, string requestedStepKey)
            {
            var steps = BuildEntityDashboardSteps();
            return BuildDashboardViewModel(user, "ENTITY_GOVERNANCE", "Entity Governance Workspace", requestedStepKey, steps);
            }

        private WorkflowDashboardViewModel BuildDashboardViewModel(SessionUser user, string dashboardKey, string dashboardTitle, string requestedStepKey, List<WorkflowDashboardStepModel> steps)
            {
            foreach (var step in steps)
                {
                step.IsVisible = step.RequiredPermissionPageId > 0 && _permissionService.HasViewPermission(user, step.RequiredPermissionPageId);
                step.IsEnabled = step.IsVisible;
                step.IsCompleted = false;
                step.IsSaved = true;
                step.StatusText = "Available";
                }

            var firstVisibleStep = steps.FirstOrDefault(step => step.IsVisible);
            var currentStep = steps.FirstOrDefault(step => step.IsVisible && string.Equals(step.StepKey, requestedStepKey, StringComparison.OrdinalIgnoreCase))
                ?? firstVisibleStep;

            return new WorkflowDashboardViewModel
                {
                DashboardKey = dashboardKey,
                DashboardTitle = dashboardTitle,
                CurrentStepKey = currentStep?.StepKey,
                Steps = steps
                };
            }

        private bool TryGetRequestedStep(WorkflowDashboardViewModel model, string requestedStepKey, out WorkflowDashboardStepModel step, out IActionResult errorResult)
            {
            step = null;
            errorResult = null;

            if (model == null)
                {
                errorResult = BadRequest("Dashboard model could not be resolved.");
                return false;
                }

            if (string.IsNullOrWhiteSpace(requestedStepKey))
                {
                step = model.CurrentStep;
                if (step == null)
                    {
                    errorResult = Forbid();
                    return false;
                    }

                return true;
                }

            step = model.Steps.FirstOrDefault(item => string.Equals(item.StepKey, requestedStepKey, StringComparison.OrdinalIgnoreCase));
            if (step == null)
                {
                errorResult = BadRequest("Invalid workflow step.");
                return false;
                }

            if (!step.IsVisible)
                {
                errorResult = Forbid();
                return false;
                }

            return true;
            }

        private List<WorkflowDashboardStepModel> BuildUserDashboardSteps()
            {
            return new List<WorkflowDashboardStepModel>
                {
                CreateDashboardStep(1, "PAGES_MANAGEMENT", "Pages Management", "/AdministrationPanel/pages_management", "~/Views/AdministrationPanel/DashboardPartials/_PagesManagement.cshtml"),
                CreateDashboardStep(2, "MENU_ASSIGNMENT", "Menu Assignment", "/AdministrationPanel/menu_assignment", "~/Views/AdministrationPanel/DashboardPartials/_MenuAssignment.cshtml"),
                CreateDashboardStep(3, "SUB_MENU_MANAGEMENT", "Sub Menu Management", "/AdministrationPanel/sub_menu_management", "~/Views/AdministrationPanel/DashboardPartials/_SubMenuManagement.cshtml"),
                CreateDashboardStep(4, "GROUP_ROLE_ASSIGNMENT", "Group Role Assignment", "/AdministrationPanel/group_role_assignment", "~/Views/AdministrationPanel/DashboardPartials/_GroupRoleAssignment.cshtml"),
                CreateDashboardStep(5, "MANAGE_USER", "Manage User", "/AdministrationPanel/manage_user", "~/Views/AdministrationPanel/DashboardPartials/_ManageUser.cshtml"),
                CreateDashboardStep(6, "MANAGE_USER_RIGHTS", "Manage User Rights", "/AdministrationPanel/manage_user_rights", "~/Views/AdministrationPanel/DashboardPartials/_ManageUserRights.cshtml")
                };
            }

        private List<WorkflowDashboardStepModel> BuildEntityDashboardSteps()
            {
            return new List<WorkflowDashboardStepModel>
                {
                CreateDashboardStep(1, "ENTITY_ADDITION", "Entity Addition", "/AdministrationPanel/entity_addition", "~/Views/AdministrationPanel/DashboardPartials/_EntityAddition.cshtml"),
                CreateDashboardStep(2, "ENTITY_RELATIONSHIP", "Entity Relationship", "/AdministrationPanel/entity_relationship", "~/Views/AdministrationPanel/DashboardPartials/_EntityRelationship.cshtml"),
                CreateDashboardStep(3, "ENTITY_SHIFTING", "Entity Shifting", "/AdministrationPanel/entity_shifting", "~/Views/AdministrationPanel/DashboardPartials/_EntityShifting.cshtml"),
                CreateDashboardStep(4, "SETUP_AUDITEE_ENTITIES", "Setup Auditee Entities", "/AdministrationPanel/setup_auditee_entities", "~/Views/AdministrationPanel/DashboardPartials/_SetupAuditeeEntities.cshtml"),
                CreateDashboardStep(5, "UPDATE_AUDITEE_ENTITIES", "Update Auditee Entities", "/AdministrationPanel/update_auditee_entities", "~/Views/AdministrationPanel/DashboardPartials/_UpdateAuditeeEntities.cshtml"),
                CreateDashboardStep(6, "AUTHORIZE_AUDITEE_ENTITIES_UPDATE", "Authorize Auditee Entities Update", "/AdministrationPanel/authorize_auditee_entities_update", "~/Views/AdministrationPanel/DashboardPartials/_AuthorizeAuditeeEntitiesUpdate.cshtml"),
                CreateDashboardStep(7, "SETUP_ENGAGEMENT_REVERSAL", "Setup Engagement Reversal", "/AdministrationPanel/setup_engagement_reversal", "~/Views/AdministrationPanel/setup_engagement_reversal.cshtml"),
                CreateDashboardStep(8, "ENGAGEMENT_SHIFTING", "Engagement Shifting", "/AdministrationPanel/setup_engagement_reversal", "~/Views/AdministrationPanel/DashboardPartials/_EngagementShifting.cshtml")
                };
            }

        private WorkflowDashboardStepModel CreateDashboardStep(int stepNo, string stepKey, string stepTitle, string legacyPath, string partialViewName)
            {
            return new WorkflowDashboardStepModel
                {
                StepNo = stepNo,
                StepKey = stepKey,
                StepTitle = stepTitle,
                LegacyPath = legacyPath,
                PartialViewName = partialViewName,
                RequiredPermissionPageId = ResolveWorkflowPageId(legacyPath)
                };
            }

        private int ResolveWorkflowPageId(string legacyPath)
            {
            return _pageIdResolver?.ResolvePageId(legacyPath) ?? 0;
            }

        private bool HasPageAccess(SessionUser user, string pagePath)
            {
            var pageId = ResolveWorkflowPageId(pagePath);
            return pageId > 0 && _permissionService.HasViewPermission(user, pageId);
            }

        private PartialViewResult CreateStepAccessDeniedResult()
            {
            Response.StatusCode = 403;
            return PartialView("~/Views/Shared/_DashboardStepAccessDenied.cshtml");
            }

        private void PopulateAdministrationDashboardStepViewData(string stepKey, int pageId)
            {
            ViewData["Layout"] = null;
            ViewData["PageId"] = pageId;
            ViewData["HideTopHeader"] = true;

            switch ((stepKey ?? string.Empty).Trim().ToUpperInvariant())
                {
                case "PAGES_MANAGEMENT":
                    ViewData["MenuList"] = dBConnection.GetAllMenusForAdminPanel();
                    break;
                case "MENU_ASSIGNMENT":
                    ViewData["MenuList"] = dBConnection.GetAllTopMenus();
                    ViewData["MenuPagesList"] = dBConnection.GetAllMenuPages();
                    break;
                case "SUB_MENU_MANAGEMENT":
                    ViewData["MenuList"] = dBConnection.GetAllMenusForAdminPanel();
                    break;
                case "GROUP_ROLE_ASSIGNMENT":
                    ViewData["GroupList"] = dBConnection.GetGroups();
                    ViewData["MenuList"] = dBConnection.GetAllTopMenus();
                    break;
                case "MANAGE_USER":
                    ViewData["GroupList"] = dBConnection.GetGroups();
                    ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(pageId);
                    break;
                case "MANAGE_USER_RIGHTS":
                    break;
                case "ENTITY_ADDITION":
                    ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
                    ViewData["RelationshipList"] = dBConnection.Getrealtionshiptype(pageId);
                    ViewData["Audit_By"] = dBConnection.GetAuditBy();
                    break;
                case "ENTITY_RELATIONSHIP":
                    var entityRelationships = dBConnection.Getrealtionshiptype(pageId);
                    ViewData["Userrelationship"] = entityRelationships;
                    ViewData["RelationshipList"] = entityRelationships;
                    ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
                    break;
                case "ENTITY_SHIFTING":
                    ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
                    ViewData["RelationshipList"] = dBConnection.Getrealtionshiptype(pageId);
                    ViewData["Audit_By"] = dBConnection.GetAuditBy();
                    break;
                case "SETUP_AUDITEE_ENTITIES":
                    ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
                    ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(pageId);
                    break;
                case "UPDATE_AUDITEE_ENTITIES":
                    ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
                    ViewData["BranchSizesList"] = dBConnection.GetBranchSizes();
                    ViewData["RiskList"] = dBConnection.GetRisks();
                    break;
                case "AUTHORIZE_AUDITEE_ENTITIES_UPDATE":
                    break;
                case "SETUP_ENGAGEMENT_REVERSAL":
                    ViewData["IsDashboardPartial"] = true;
                    ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(pageId);
                    ViewData["statusList"] = dBConnection.GetObservationReversalStatus();
                    break;
                case "ENGAGEMENT_SHIFTING":
                    ViewData["IsDashboardPartial"] = true;
                    ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(pageId);
                    break;
                default:
                    break;
                }
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetShiftableEngagements(int ENTITY_ID = 0)
            {
            if (!User.Identity.IsAuthenticated || !sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized();
                }

            if (!HasPageAccess(user, "/AdministrationPanel/setup_engagement_reversal"))
                {
                return Forbid();
                }

            if (ENTITY_ID <= 0)
                {
                return Ok(new List<ObservationReversalModel>());
                }

            return Ok(dBConnection.GetShiftableEngagementDetails(ENTITY_ID));
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ApplicationAudit("Engagement Entity Shift", "Administration", "PKG_AD", "P_SHIFT_ENGAGEMENT_ENTITY", ActionCategory = "SHIFT", EngagementId = "request.EngagementId", ObjectType = "ENGAGEMENT", ObjectId = "request.EngagementId")]
        public IActionResult ShiftEngagementEntity(EngagementEntityShiftRequestModel request)
            {
            if (!User.Identity.IsAuthenticated || !sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized(new { Status = false, Message = "Your session has expired. Please sign in again." });
                }

            if (!HasPageAccess(user, "/AdministrationPanel/setup_engagement_reversal"))
                {
                return Forbid();
                }

            if (request == null || request.EngagementId <= 0 || request.NewEntityId <= 0 || string.IsNullOrWhiteSpace(request.Reason))
                {
                return BadRequest(new { Status = false, Message = "Engagement, destination entity and reason are required." });
                }

            var response = dBConnection.ShiftEngagementEntity(request.EngagementId, request.NewEntityId, request.Reason);
            return Ok(new { Status = response.Status, Message = response.Remarks });
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

    internal class GMOfficeModel
        {
        }
    }
