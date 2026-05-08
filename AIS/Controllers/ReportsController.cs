using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AIS.Controllers
    {
    public class ReportsController : Controller
        {
        private readonly ILogger<ReportsController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;

        public ReportsController(ILogger<ReportsController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            }

        public IActionResult Para_Position()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.GetrealtionshiptypeForParaPositionReport();
            return ReportView("para_position");
            }

        public IActionResult FAD_Monthly_Review()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["EntitiesList"] = dBConnection.GetEntityTypesForEntityWiseOutstandingObsPosition();
            return ReportView();
            }

        public IActionResult gist_wise_report()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            return ReportView();
            }

        public IActionResult search_para_text_report()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            return ReportView();
            }

        public IActionResult join_comp_report()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["DepartmentList"] = dBConnection.GetDepartments(112215);
            return ReportView();
            }

        public IActionResult compliance_progress_report()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            return ReportView();
            }

        public IActionResult settled_paras_report()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["EntityTypesList"] = dBConnection.GetEntityTypesForSettlementReport();
            return ReportView();
            }

        public IActionResult eng_plan_delay_analysis_report()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            return ReportView();
            }

        public IActionResult consolidated_outstanding_paras_pdf()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditDepartments"] = dBConnection.GetAuditDepartments();
            ViewData["RiskList"] = dBConnection.GetRisks();
            return ReportView();
            }

        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

        private IActionResult ReportView(string viewName = null)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            return string.IsNullOrWhiteSpace(viewName) ? View() : View(viewName);
            }
        }
    }
