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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

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
        private readonly PasswordChangeStateStore _passwordChangeStateStore;
        private readonly AIS.Security.Cryptography.SecurityTokenService _tokenService;

        public HomeController(ILogger<HomeController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService, PasswordPolicyValidator passwordPolicyValidator, PasswordChangeTokenService passwordChangeTokenService, PasswordChangeStateStore passwordChangeStateStore, AIS.Security.Cryptography.SecurityTokenService tokenService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _passwordPolicyValidator = passwordPolicyValidator;
            _passwordChangeTokenService = passwordChangeTokenService;
            _passwordChangeStateStore = passwordChangeStateStore;
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
            if (!TryGetPasswordChangeState(out var token, out _, out _))
                {
                ExpirePasswordChangeToken(token);
                return RedirectToAction("Index", "Login");
                }

            return View();
            }
        [HttpPost]
        [EnableRateLimiting("ChangePasswordPolicy")]
        [AllowAnonymous]
        public async Task<IActionResult> DoChangePassword(string Password, string NewPassword, string ConfirmPassword)
            {
            try
                {
                if (!TryGetPasswordChangeState(out var token, out var tokenInfo, out var changeState))
                    {
                    ExpirePasswordChangeToken(token);
                    return StatusCode(StatusCodes.Status401Unauthorized, new { success = false, message = "Password change session expired, login again." });
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

                var validation = _passwordPolicyValidator.Validate(decodedNewPassword, tokenInfo?.PPNumber);
                if (!validation.IsValid)
                    {
                    return Json(new { success = false, message = validation.ErrorMessage });
                    }

                var passwordChanged = dBConnection.ChangePasswordForUser(changeState.User, NewPassword);
                if (!passwordChanged)
                    {
                    return Json(new { success = false, message = "Unable to change password. Please try again." });
                    }

                ExpirePasswordChangeToken(token);
                var sessionUser = dBConnection.CreateLoginSession(changeState.User);
                IssueSessionToken(changeState.User);
                await SignInUserAsync(sessionUser);
                return Json(new { success = true, message = "Your password has been changed successfully.", redirectUrl = BuildAppPath("/Home/Index") });
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

        private bool TryGetPasswordChangeState(out string token, out PasswordChangeTokenInfo tokenInfo, out PasswordChangeState changeState)
            {
            token = null;
            tokenInfo = null;
            changeState = null;

            if (!Request.Cookies.TryGetValue(PasswordChangeTokenService.CookieName, out token))
                {
                return false;
                }

            if (!_passwordChangeTokenService.TryValidateToken(token, out tokenInfo))
                {
                return false;
                }

            return _passwordChangeStateStore.TryGet(token, out changeState);
            }

        private void ExpirePasswordChangeToken(string token)
            {
            if (!string.IsNullOrWhiteSpace(token))
                {
                _passwordChangeStateStore.Remove(token);
                }

            _passwordChangeTokenService.ClearCookie(Response, Request.PathBase);
            }

        private void IssueSessionToken(UserModel user)
            {
            if (user == null)
                {
                return;
                }

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
            }

        private string BuildAppPath(string relativePath)
            {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            return string.Concat(pathBase, relativePath);
            }

        private async Task SignInUserAsync(SessionUser sessionUser)
            {
            if (sessionUser == null)
                {
                return;
                }

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

        }
    }
