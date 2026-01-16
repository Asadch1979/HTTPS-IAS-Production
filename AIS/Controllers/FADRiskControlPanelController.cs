using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using AIS.Services;

namespace AIS.Controllers
    {
    public class FADRiskControlPanelController : BaseController
        {
        private readonly ILogger<FADRiskControlPanelController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly AIS.Security.Cryptography.SecurityTokenService _tokenService;

        public FADRiskControlPanelController(
     ILogger<FADRiskControlPanelController> logger,
     SessionHandler _sessionHandler,
     DBConnection _dbCon,
     TopMenus _tpMenu,
     IHttpContextAccessor httpContextAccessor,
     IConfiguration configuration,
     IPermissionService permissionService,
     AIS.Security.Cryptography.SecurityTokenService tokenService)
     : base(_sessionHandler)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _tokenService = tokenService;
            }


        private DBConnection CreateDbConnection()
            {
            if (_httpContextAccessor?.HttpContext == null)
                throw new InvalidOperationException("HTTP context accessor is not available.");

            if (_configuration == null)
                throw new InvalidOperationException("Configuration dependency is not available.");

            return DBConnection.CreateFromHttpContext(
                _httpContextAccessor,
                _configuration,
                sessionHandler,
                _tokenService
            );
            }

        /* =========================
           MAIN SCREEN
           ========================= */

        public IActionResult Index()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler))
                return RedirectToAction("Index", "PageNotFound");

            return View("~/Views/FAD/FADRiskControlPanel/Index.cshtml");
            }

        /* =========================
           GET – MAIN KPI
           ========================= */

        [HttpGet]
        public JsonResult GetMainKpi()
            {
            var db = CreateDbConnection();
            DataTable dt = db.P_GET_KPI_MAIN();
            return Json(ConvertDataTable(dt));
            }

        /* =========================
           GET – SUB KPI (BY MAIN)
           ========================= */

        [HttpGet]
        public JsonResult GetSubKpiByMain(int kpiMainId)
            {
            var db = CreateDbConnection();
            DataTable dt = db.P_GET_KPI_SUB(kpiMainId);
            return Json(ConvertDataTable(dt));
            }

        /* =========================
           GET – PROCESS (BY SUB KPI)
           ========================= */

        [HttpGet]
        public JsonResult GetProcessBySubKpi(int kpiSubId)
            {
            var db = CreateDbConnection();
            DataTable dt = db.P_GET_PROCESS(kpiSubId);
            return Json(ConvertDataTable(dt));
            }

        /* =========================
           GET – SUB PROCESS (BY PROCESS)
           ========================= */

        [HttpGet]
        public JsonResult GetSubProcessByProcess(int processId)
            {
            var db = CreateDbConnection();
            DataTable dt = db.P_GET_SUB_PROCESS(processId);
            return Json(ConvertDataTable(dt));
            }

        /* =========================
           GET – ANNEXURE (BY SUB PROCESS)
           ========================= */

        [HttpGet]
        public JsonResult GetAnnexureBySubProcess(int subProcessId)
            {
            var db = CreateDbConnection();
            DataTable dt = db.P_GET_SUBPROC_ANNEX(subProcessId);
            return Json(ConvertDataTable(dt));
            }

        /* =========================
           GET – GRAVITY MASTER
           ========================= */

        [HttpGet]
        public JsonResult GetGravity()
            {
            var db = CreateDbConnection();
            DataTable dt = db.P_GET_GRAVITY();
            return Json(ConvertDataTable(dt));
            }

        /* =========================
           SAVE – MAIN KPI
           ========================= */

        [HttpPost]
        public JsonResult SaveMainKpi(
            int? kpiMainId,
            string code,
            string name,
            int displayOrder,
            string isActive)
            {
            var db = CreateDbConnection();
            int id = kpiMainId ?? 0;

            db.P_SAVE_KPI_MAIN(ref id, code, name, displayOrder, isActive);

            return Json(new { success = true, kpiMainId = id });
            }

        /* =========================
           SAVE – SUB KPI
           ========================= */

        [HttpPost]
        public JsonResult SaveSubKpi(
            int kpiMainId,
            int? kpiSubId,
            string code,
            string name,
            decimal weightage,
            int displayOrder,
            string isActive)
            {
            if (kpiMainId <= 0)
                {
                return Json(new { success = false, message = "Main KPI is required." });
                }

            var db = CreateDbConnection();
            int id = kpiSubId ?? 0;

            db.P_SAVE_KPI_SUB(ref id, kpiMainId, code, name, weightage, displayOrder, isActive);

            return Json(new { success = true, kpiSubId = id });
            }

        /* =========================
           SAVE – PROCESS
           ========================= */

        [HttpPost]
        public JsonResult SaveProcess(
            int kpiSubId,
            int? processId,
            string code,
            string name,
            decimal weightage,
            int displayOrder,
            string isActive)
            {
            if (kpiSubId <= 0)
                {
                return Json(new { success = false, message = "Sub KPI is required." });
                }

            var db = CreateDbConnection();
            int id = processId ?? 0;

            db.P_SAVE_PROCESS(ref id, kpiSubId, code, name, weightage, displayOrder, isActive);

            return Json(new { success = true, processId = id });
            }

        /* =========================
           SAVE – SUB PROCESS
           ========================= */

        [HttpPost]
        public JsonResult SaveSubProcess(
            int processId,
            int? subProcessId,
            string code,
            string name,
            int gravityId,
            string isActive)
            {
            if (processId <= 0)
                {
                return Json(new { success = false, message = "Process is required." });
                }

            var db = CreateDbConnection();
            int id = subProcessId ?? 0;

            db.P_SAVE_SUB_PROCESS(ref id, processId, code, name, gravityId, isActive);

            return Json(new { success = true, subProcessId = id });
            }

        /* =========================
           SAVE – ANNEXURE + MAP
           ========================= */

        [HttpPost]
        public JsonResult SaveAnnexure(
            int subProcessId,
            int? annexureId,
            string code,
            string name,
            string isActive)
            {
            if (subProcessId <= 0)
                {
                return Json(new { success = false, message = "Sub Process is required." });
                }

            var db = CreateDbConnection();
            int id = annexureId ?? 0;

            db.P_SAVE_ANNEXURE(ref id, code, name, isActive);
            db.P_SAVE_SUBPROC_ANNEX(subProcessId, id, "Y");

            return Json(new { success = true, annexureId = id });
            }
        }
    }
