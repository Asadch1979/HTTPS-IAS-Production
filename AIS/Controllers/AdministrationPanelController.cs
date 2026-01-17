using System;
using AIS.Models;
using AIS.Models.Requests;
using AIS.Security.PasswordPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using AIS.Services;

namespace AIS.Controllers
    {

    public class AdministrationPanelController : Controller
        {
        private readonly ILogger<AdministrationPanelController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        private readonly PasswordPolicyValidator _passwordPolicyValidator;
        public AdministrationPanelController(ILogger<AdministrationPanelController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService, PasswordPolicyValidator passwordPolicyValidator)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _passwordPolicyValidator = passwordPolicyValidator;
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
        public IActionResult audit_observation_text()
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

        public IActionResult audit_template()
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

        public IActionResult status_reversal_audit_entities()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            ViewData["EntTypeList"] = dBConnection.GetEntityTypeList();

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
        public IActionResult setup_observation_reversal()
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
            var loggedInUser = sessionHandler.GetUserOrThrow();
            if (loggedInUser.UserRoleID == 1)
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
            var loggedInUser = sessionHandler.GetUserOrThrow();
            if (loggedInUser.UserRoleID == 1)
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
            var loggedInUser = sessionHandler.GetUserOrThrow();
            if (loggedInUser.UserRoleID == 1)
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
        public IActionResult entity_relationship()
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
        public IActionResult memo_status()
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

            if (!string.IsNullOrWhiteSpace(user.PASSWORD))
                {
                var validation = _passwordPolicyValidator.Validate(user.PASSWORD, user.PPNO);
                if (!validation.IsValid)
                    {
                    _logger.LogWarning("update_user password policy failed for USER_ID {UserId}: {Message}", user.USER_ID, validation.ErrorMessage);
                    return Json(new { status = false, message = validation.ErrorMessage });
                    }
                }

            var updatedUser = dBConnection.UpdateUser(new UpdateUserModel
                {
                USER_ID = user.USER_ID ?? 0,
                ROLE_ID = user.ROLE_ID ?? 0,
                ENTITY_ID = user.ENTITY_ID ?? 0,
                PASSWORD = user.PASSWORD,
                EMAIL_ADDRESS = user.EMAIL_ADDRESS,
                PPNO = user.PPNO,
                ISACTIVE = user.ISACTIVE
                });
            _logger.LogInformation("update_user succeeded for USER_ID {UserId}.", user.USER_ID);
            return Json(new { status = true, user = updatedUser });
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

        private static DateTime? ParseSystemLogDateTime(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return null;
                }

            if (DateTime.TryParseExact(value, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                {
                return parsed;
                }

            return null;
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
