using AIS.Models;
using AIS.Security.PasswordPolicy;
using AIS.Security.Cryptography;
using AIS.Services;
using AIS.Session;
using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Claims;

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
        private readonly PasswordChangeTokenService _passwordChangeTokenService;
        private readonly SecurityTokenService _tokenService;

        public HomeController(ILogger<HomeController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService, PasswordPolicyValidator passwordPolicyValidator, PasswordChangeTokenService passwordChangeTokenService, SecurityTokenService tokenService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _passwordPolicyValidator = passwordPolicyValidator;
            _passwordChangeTokenService = passwordChangeTokenService;
            _tokenService = tokenService;
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
        [AllowAnonymous]
        public IActionResult Change_Password()
            {
            if (!TryValidatePasswordChangeToken(out _))
                {
                return RedirectToAction("Index", "Login");
                }

            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            TempData["Message"] = "";
            TempData["SessionKill"] = "";
            return View();
            }
        [HttpPost]
        [EnableRateLimiting("ChangePasswordPolicy")]
        [AllowAnonymous]
        public async Task<IActionResult> DoChangePassword(string Password, string NewPassword, string ConfirmPassword)
            {
            try
                {
                if (!TryValidatePasswordChangeToken(out var token))
                    {
                    return Unauthorized(new { success = false, message = "Password change session expired, login again." });
                    }

                var decodedNewPassword = DecodePassword(NewPassword);
                var decodedConfirmPassword = DecodePassword(ConfirmPassword);
                if (string.IsNullOrWhiteSpace(decodedNewPassword))
                    {
                    return Json(new { success = false, message = "New password is required." });
                    }

                if (!string.Equals(decodedNewPassword, decodedConfirmPassword, StringComparison.Ordinal))
                    {
                    return Json(new { success = false, message = "New password and confirm password do not match." });
                    }

                var validation = _passwordPolicyValidator.Validate(decodedNewPassword, token.PPNumber);
                if (!validation.IsValid)
                    {
                    return Json(new { success = false, message = validation.ErrorMessage });
                    }

                var passwordChanged = dBConnection.ChangePasswordForUser(token.PPNumber, token.EntityId, token.RoleId, NewPassword);
                if (!passwordChanged)
                    {
                    return Json(new { success = false, message = "Unable to change password. Please try again." });
                    }

                _passwordChangeTokenService.ClearToken(Request, Response);
                var loginUser = dBConnection.AutheticateLogin(new LoginModel
                    {
                    PPNumber = token.PPNumber,
                    Password = NewPassword
                    });

                if (!loginUser.isAuthenticate || loginUser.isAlreadyLoggedIn)
                    {
                    return Json(new { success = false, message = "Unable to create a login session. Please log in again." });
                    }

                if (!sessionHandler.TryGetUser(out var sessionUser))
                    {
                    return Json(new { success = false, message = "Unable to create a login session. Please log in again." });
                    }

                await CreateIasSessionAsync(loginUser, sessionUser);
                return Json(new { success = true, message = "Your password has been changed successfully.", redirectUrl = BuildHomeRedirect() });
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

        private bool TryValidatePasswordChangeToken(out PasswordChangeToken token)
            {
            return _passwordChangeTokenService.TryValidate(Request, out token);
            }

        private async Task CreateIasSessionAsync(UserModel user, SessionUser sessionUser)
            {
            if (int.TryParse(user.PPNumber, out var ppNumber))
                {
                dBConnection.KillSessions(ppNumber);
                var sessionToken = _tokenService.GenerateSessionToken();
                dBConnection.CreateSession(
                    sessionToken,
                    ppNumber,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString());

                Response.Cookies.Append("IAS_SESSION", sessionToken, new CookieOptions
                    {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    Path = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : "/"
                    });
                }

            await SignInUserAsync(sessionUser);
            }

        private async Task SignInUserAsync(SessionUser sessionUser)
            {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, sessionUser.ID.ToString()),
                new Claim(ClaimTypes.Name, sessionUser.Name ?? sessionUser.PPNumber ?? string.Empty),
                new Claim(ClaimTypes.SerialNumber, sessionUser.PPNumber ?? string.Empty),
                new Claim("sessionId", sessionUser.SessionId ?? string.Empty)
            };

            if (!string.IsNullOrWhiteSpace(sessionUser.UserRoleName))
                {
                claims.Add(new Claim(ClaimTypes.Role, sessionUser.UserRoleName));
                }

            claims.Add(new Claim("roleId", sessionUser.UserRoleID.ToString()));

            if (sessionUser.UserEntityID.HasValue)
                {
                claims.Add(new Claim("entityId", sessionUser.UserEntityID.Value.ToString()));
                }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

        private string BuildHomeRedirect()
            {
            var pathBase = HttpContext?.Request?.PathBase.HasValue == true
                ? HttpContext.Request.PathBase.Value
                : string.Empty;
            return string.Concat(pathBase, "/Home/Index");
            }

        }
    }
