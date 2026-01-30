using AIS.Models;
using AIS.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    return View();
                }
            }
        public IActionResult task_list()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["TaskList"] = dBConnection.GetTaskList();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            ViewData["PPNumber"] = loggedInUser.PPNumber;
            ViewData["EMP_NAME"] = loggedInUser.Name;
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
        public List<List<String>> add_audit_criteria(List<List<String>> CRITERIA_LIST)
            {
            List<List<String>> Error = new List<List<String>>();
            foreach (var criteria in CRITERIA_LIST)
                {
                AddAuditCriteriaModel cm = new AddAuditCriteriaModel();
                cm.ID = 0;
                cm.AUDITPERIODID = Convert.ToInt32(criteria[0]);
                cm.ENTITY_TYPEID = Convert.ToInt32(criteria[1]);
                cm.RISK_ID = Convert.ToInt32(criteria[2]);
                cm.FREQUENCY_ID = Convert.ToInt32(criteria[3]);
                cm.SIZE_ID = Convert.ToInt32(criteria[4]);
                cm.NO_OF_DAYS = Convert.ToInt32(criteria[5]);
                cm.AUDITPERIOD = criteria[7];
                cm.ENTITY_NAME = criteria[8];
                cm.RISK = criteria[9];
                cm.SIZE = criteria[10];
                cm.FREQUENCY = criteria[11];
                cm.ENTITY_ID = Convert.ToInt32(criteria[12]);
                if (criteria[6].ToLower() == "yes")
                    criteria[6] = "Y";
                else
                    criteria[6] = "N";

                cm.VISIT = criteria[6];
                cm.APPROVAL_STATUS = 1;
                List<string> arr = new List<string>();
                arr.Add(cm.AUDITPERIOD);
                arr.Add(cm.ENTITY_NAME);
                arr.Add(cm.RISK);
                arr.Add(cm.SIZE);
                arr.Add(cm.FREQUENCY);

                if (!dBConnection.AddAuditCriteria(cm))
                    arr.Add("Criteria already defined");
                else
                    arr.Add("Criteria successfully added");
                Error.Add(arr);
                }
            return Error;
            }
        [HttpPost]
        public AuditEntitiesModel add_auditee_entity(AuditEntitiesModel am)
            {
            am.AUTID = 0;
            return dBConnection.AddAuditEntity(am);
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
