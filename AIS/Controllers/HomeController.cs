using AIS.Models;
using AIS.Security.PasswordPolicy;
using AIS.Services;
using AIS.Session;
using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AIS.Controllers
    {

    public class HomeController : Controller
        {
        private readonly ILogger<HomeController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly DBConnection dBConnection;
        private readonly IPermissionService _permissionService;
        private readonly PasswordPolicyValidator _passwordPolicyValidator;

        public HomeController(ILogger<HomeController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService, PasswordPolicyValidator passwordPolicyValidator)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _passwordPolicyValidator = passwordPolicyValidator;
            }
        public IActionResult Index()
            {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    {
                    ViewData["TopMenu"] = tm.GetTopMenus();
                    ViewData["TopMenuPages"] = tm.GetTopMenusPages();
                    var loggedInUser = sessionHandler.GetUserOrThrow();
                    ViewData["QuickLinks"] = dBConnection.GetDashboardQuickLinks(loggedInUser.UserRoleID);
                    return View();
                    }

                }
            }
        public IActionResult Change_Password()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            TempData["Message"] = "";
            TempData["SessionKill"] = "";
            return View();
            }
        [HttpPost]
        [EnableRateLimiting("ChangePasswordPolicy")]
        public IActionResult DoChangePassword(string Password, string NewPassword)
            {
            try
                {
                var loggedInUser = sessionHandler.GetUserOrThrow();
                var decodedNewPassword = DecodePassword(NewPassword);
                var validation = _passwordPolicyValidator.Validate(decodedNewPassword, loggedInUser?.PPNumber);
                if (!validation.IsValid)
                    {
                    return Json(new { success = false, message = validation.ErrorMessage });
                    }

                var passwordChanged = dBConnection.ChangePassword(Password, NewPassword);
                if (!passwordChanged)
                    {
                    return Json(new { success = false, message = "Unable to change password. Please try again." });
                    }

                HttpContext.Session.Remove(SessionKeys.MustChangePassword);
                return Json(new { success = true, message = "Your password has been changed successfully." });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error while changing password for current user.");
                return Json(new { success = false, message = "Unable to change password. Please try again." });
                }
            }

        public IActionResult Privacy()
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
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature?.Error != null)
                {
                _logger.LogError(exceptionFeature.Error, "Unhandled exception encountered while processing request {Path}.", exceptionFeature.Path);
                }
            else
                {
                _logger.LogError("Unhandled error encountered without exception details.");
                }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

        [Route("Home/StatusCode")]
        public IActionResult StatusCode(int code)
            {
            var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            _logger.LogWarning("HTTP {StatusCode} returned for path {OriginalPath}.", code, statusFeature?.OriginalPath);
            ViewData["StatusCode"] = code;
            return View("StatusCode", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

        private string DecodePassword(string encrypted)
            {
            try
                {
                var bytes = Convert.FromBase64String(encrypted ?? string.Empty);
                return Encoding.UTF8.GetString(bytes);
                }
            catch
                {
                return string.Empty;
                }
            }

        }
    }
