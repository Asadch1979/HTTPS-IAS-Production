using AIS.Exceptions;
using AIS.Models;
using AIS.Security.Cryptography;
using AIS.Security.PasswordPolicy;
using AIS.Models.Requests;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AIS.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly SessionHandler sessionHandler;
        private readonly DBConnection dBConnection;
        private readonly IConfiguration _configuration;
        private readonly LoginAttemptTracker _loginAttemptTracker;
        private readonly PasswordPolicyValidator _passwordPolicyValidator;
        private readonly SecurityTokenService _tokenService;
        private readonly IPermissionService _permissionService;
        private readonly LoginViewResolver _loginViewResolver;

        public LoginController(ILogger<LoginController> logger, SessionHandler sessionHandler, DBConnection dbConnection, IConfiguration configuration, LoginAttemptTracker loginAttemptTracker, PasswordPolicyValidator passwordPolicyValidator, SecurityTokenService tokenService, IPermissionService permissionService, LoginViewResolver loginViewResolver)
            {
            _logger = logger;
            this.sessionHandler = sessionHandler;
            dBConnection = dbConnection;
            _configuration = configuration;
            _loginAttemptTracker = loginAttemptTracker;
            _passwordPolicyValidator = passwordPolicyValidator;
            _tokenService = tokenService;
            _permissionService = permissionService;
            _loginViewResolver = loginViewResolver;
            }

        public IActionResult Index()
        {
            return RenderLoginView();
        }

        public async Task<IActionResult> Logout()
        {
            ResetLoginAttemptsForCurrentUser();
            var token = Request.Cookies["IAS_SESSION"];
            if (!string.IsNullOrWhiteSpace(token))
            {
                dBConnection.InvalidateSession(token);
            }

            try
            {
                dBConnection.DisposeLoginSession();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while disposing login session during logout.");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            try
            {
                await HttpContext.Session.LoadAsync();
                HttpContext.Session.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session store is unavailable while logging out.");
                Response.Cookies.Delete("IAS.Session");
                return RedirectToAction(nameof(Maintenance));
            }

            Response.Cookies.Delete("IAS_SESSION");
            Response.Cookies.Delete("IAS.Session");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [EnableRateLimiting("LoginPolicy")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DoLogin([FromForm, Bind(Prefix = "login")] LoginPostModel login)
        {
            var loginModel = BuildLoginModel(login);
            _logger.LogDebug("DoLogin received for PPNumber {PPNumber}.", login?.PPNumber);
            _logger.LogDebug("Encrypted password length for PPNumber {PPNumber}: {PasswordLength}.", login?.PPNumber, login?.Password?.Length ?? 0);

            if (login == null)
            {
                return Json(BuildLoginResponse(new UserModel
                {
                    isAuthenticate = false,
                    isAlreadyLoggedIn = false,
                    ErrorCode = "INVALID_CREDENTIALS",
                    ErrorTitle = "Sign in failed",
                    ErrorMsg = "Invalid login request."
                }));
            }

            try
            {
                var throttleResult = EvaluateRateLimit(loginModel);
                if (throttleResult != null)
                {
                    return Json(BuildLoginResponse(throttleResult));
                }

                EnsureSessionInitialized();
                var user = dBConnection.AutheticateLogin(loginModel);
                _logger.LogDebug("Authentication outcome for PP {PPNumber}: Authenticated={IsAuthenticated}, AlreadyLoggedIn={AlreadyLoggedIn}, ErrorCode={ErrorCode}.", login.PPNumber, user.isAuthenticate, user.isAlreadyLoggedIn, user.ErrorCode);

                if (user.isAuthenticate)
                {
                    ResetRateLimit(loginModel);
                }

                if (user.ID != 0 && !user.isAlreadyLoggedIn && user.isAuthenticate)
                {
                    if (!sessionHandler.TryGetUser(out var sessionUser))
                    {
                        _logger.LogWarning("Session user could not be loaded after successful authentication for PP {PPNumber}.", login.PPNumber);
                        user.isAuthenticate = false;
                        user.isAlreadyLoggedIn = false;
                        user.ErrorCode = "INVALID_CREDENTIALS";
                        user.ErrorTitle = "Sign in failed";
                        user.ErrorMsg = "Unable to create a login session. Please try again.";
                        return Json(BuildLoginResponse(user));
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
                            Secure = ShouldSecureCookies(),
                            SameSite = SameSiteMode.Lax,
                            IsEssential = true,
                            Path = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : "/"
                        });
                    }

                    SetMustChangePasswordFlag(user);
                    await SignInUserAsync(sessionUser);
                    return Json(BuildLoginResponse(user));
                }

                if (user.isAuthenticate && user.isAlreadyLoggedIn)
                {
                    user.ErrorTitle ??= "Session Details";
                    user.ErrorMsg ??= "You are already logged in System";
                }
                else
                {
                    RegisterFailedAttempt(loginModel);
                    user.isAuthenticate = false;
                    user.ErrorCode = "INVALID_CREDENTIALS";
                    user.ErrorTitle = "Sign in failed";
                    user.ErrorMsg = "Invalid user ID or password.";
                }

                return Json(BuildLoginResponse(user));
            }
            catch (SessionMissingException ex)
            {
                _logger.LogError(ex, "Session store is unavailable during login for PP {PPNumber}.", login?.PPNumber);
                _logger.LogDebug(ex, "Exception captured in DoLogin for PP {PPNumber}.", login?.PPNumber);
                var user = new UserModel
                {
                    isAuthenticate = false,
                    isAlreadyLoggedIn = false,
                    ErrorCode = "INVALID_CREDENTIALS",
                    ErrorTitle = "Sign in failed",
                    ErrorMsg = "Unable to create a login session. Please try again."
                };
                return Json(BuildLoginResponse(user));
            }
            catch (DatabaseUnavailableException ex)
            {
                _logger.LogError(ex, "Database connection is unavailable during login.");
                _logger.LogDebug(ex, "Exception captured in DoLogin for PP {PPNumber}.", login?.PPNumber);
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login.");
                _logger.LogDebug(ex, "Exception captured in DoLogin for PP {PPNumber}.", login?.PPNumber);
                var user = new UserModel
                {
                    isAuthenticate = false,
                    isAlreadyLoggedIn = false,
                    ErrorCode = "INVALID_CREDENTIALS",
                    ErrorTitle = "Sign in failed",
                    ErrorMsg = "An unexpected error occurred. Please try again."
                };
                return Json(BuildLoginResponse(user));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KillSession([FromForm, Bind(Prefix = "")] LoginPostModel login)
        {
            string statusMessage = null;
            string errorMessage = null;
            var loginModel = BuildLoginModel(login);

            try
            {
                if (login == null || string.IsNullOrWhiteSpace(login.PPNumber) || string.IsNullOrWhiteSpace(login.Password))
                {
                    errorMessage = "PP Number and Password are required to kill the existing session.";
                }
                else
                {
                    bool isKilled = false;
                    try
                    {
                        isKilled = dBConnection.KillExistSession(loginModel);
                    }
                    catch (DatabaseUnavailableException ex)
                    {
                        _logger.LogError(ex, "Database connection is unavailable during kill session.");
                        errorMessage = "The system is currently unavailable. Please try again later.";
                    }

                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        if (isKilled)
                        {
                            if (int.TryParse(login.PPNumber, out var ppNumber))
                            {
                                dBConnection.KillSessions(ppNumber);
                            }

                            ResetRateLimit(loginModel);

                            statusMessage = "All sessions have been terminated.";
                        }
                        else
                        {
                            errorMessage = "We could not find an active session for the provided credentials.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during kill session.");
                errorMessage = "Unable to clear the session. Please try again.";
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            try
            {
                await HttpContext.Session.LoadAsync();
                HttpContext.Session.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session store is unavailable while clearing the login session.");
                Response.Cookies.Delete("IAS.Session");
                return RedirectToAction(nameof(Maintenance));
            }

            Response.Cookies.Delete("IAS_SESSION");
            Response.Cookies.Delete("IAS.Session");

            return RenderLoginView(statusMessage, errorMessage);
        }

        public IActionResult Maintenance()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [EnableRateLimiting("ForgotPasswordPolicy")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordPostModel model)
        {
            var maskedIdentifier = MaskIdentifier(model?.PPNumber);
            _logger.LogInformation("Password reset request received for PP {MaskedIdentifier} from {RemoteIp}.", maskedIdentifier, HttpContext.Connection.RemoteIpAddress?.ToString());

            var genericResponse = BuildResetPasswordResponse();

            try
            {
                var generatedPassword = _passwordPolicyValidator.GenerateCompliantPassword(model?.PPNumber, model?.CNICNumber);
                var resetResult = dBConnection.ResetUserPassword(model?.PPNumber, model?.CNICNumber, generatedPassword);
                _logger.LogInformation(
                    "Password reset attempt completed for PP {MaskedIdentifier}. AccountFound={AccountFound}, EmailSent={EmailSent}.",
                    maskedIdentifier,
                    resetResult?.AccountFound ?? false,
                    resetResult?.EmailSent ?? false);
            }
            catch (DatabaseUnavailableException ex)
            {
                _logger.LogError(ex, "Database connection is unavailable during password reset for PP {MaskedIdentifier}.", maskedIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password reset for PP {MaskedIdentifier}.", maskedIdentifier);
            }

            await ApplyUniformResetDelayAsync();
            return Json(genericResponse);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IActionResult RenderLoginView(string statusMessage = null, string errorMessage = null)
        {
            try
            {
                EnsureSessionInitialized();
            }
            catch (SessionMissingException ex)
            {
                _logger.LogError(ex, "Session store is unavailable while rendering the login page.");
                return RedirectToAction(nameof(Maintenance));
            }

            var viewName = ResolveLoginViewName();
            if (string.IsNullOrWhiteSpace(viewName))
            {
                return RedirectToAction(nameof(Maintenance));
            }

            PopulateLoginViewData(statusMessage, errorMessage);
            return View(viewName);
        }

        private void PopulateLoginViewData(string statusMessage, string errorMessage)
        {
            var requestBasePath = HttpContext?.Request?.PathBase.Value ?? string.Empty;
            var configuredBaseUrl = _configuration["BaseURL"] ?? string.Empty;
            var baseUrl = !string.IsNullOrWhiteSpace(requestBasePath)
                ? requestBasePath
                : configuredBaseUrl;

            if (!string.IsNullOrWhiteSpace(baseUrl) && !baseUrl.StartsWith("/"))
            {
                baseUrl = "/" + baseUrl;
            }

            baseUrl = baseUrl?.TrimEnd('/') ?? string.Empty;
            ViewBag.BaseURL = baseUrl;
            ViewData["StatusMessage"] = statusMessage;
            ViewData["ErrorMessage"] = errorMessage;
        }

        private string ResolveLoginViewName()
            {
            return _loginViewResolver.ResolvedViewName;
            }

        private bool ShouldSecureCookies()
            {
            return true;
            }

        private void EnsureSessionInitialized()
        {
            var session = HttpContext?.Session;
            if (session == null)
            {
                throw new SessionMissingException("Session is not available in the current context.");
            }

            try
            {
                var marker = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                session.SetString(SessionKeys.Bootstrapped, marker);
            }
            catch (Exception ex)
            {
                throw new SessionMissingException("Unable to access the session store.", ex);
            }
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

        private void SetMustChangePasswordFlag(UserModel user)
        {
            var mustChangePassword = user?.passwordChangeRequired == true ||
                                     string.Equals(user?.changePassword, "Y", StringComparison.OrdinalIgnoreCase);

            try
            {
                HttpContext.Session.SetString(SessionKeys.MustChangePassword, mustChangePassword ? "1" : "0");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to set must-change-password flag for current session.");
            }
        }

        private UserModel EvaluateRateLimit(LoginModel login)
        {
            return _loginAttemptTracker.EvaluateRateLimit(login, GetRemoteIpAddress());
        }

        private void RegisterFailedAttempt(LoginModel login)
        {
            _loginAttemptTracker.RegisterFailedAttempt(login, GetRemoteIpAddress());
        }

        private void ResetRateLimit(LoginModel login)
        {
            _loginAttemptTracker.ResetAttempts(login, GetRemoteIpAddress());
        }

        private static string MaskIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return "unknown";
            }

            var trimmed = identifier.Trim();
            if (trimmed.Length <= 2)
            {
                return new string('*', trimmed.Length);
            }

            return new string('*', Math.Max(trimmed.Length - 2, 0)) + trimmed[^2..];
        }

        private static object BuildResetPasswordResponse()
        {
            return new
            {
                status = true,
                message = "If the account exists, an email will be sent to the registered address."
            };
        }

        private static LoginModel BuildLoginModel(LoginPostModel login)
        {
            if (login == null)
            {
                return null;
            }

            return new LoginModel
            {
                PPNumber = login.PPNumber,
                Password = login.Password
            };
        }

        private static Task ApplyUniformResetDelayAsync()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(400));
        }

        private void ResetLoginAttemptsForCurrentUser()
        {
            if (sessionHandler.TryGetUser(out var user) && !string.IsNullOrWhiteSpace(user.PPNumber))
            {
                _loginAttemptTracker.ResetAttempts(user.PPNumber, GetRemoteIpAddress());
                return;
            }

            var claim = User?.FindFirst(ClaimTypes.SerialNumber);
            if (claim != null && !string.IsNullOrWhiteSpace(claim.Value))
            {
                _loginAttemptTracker.ResetAttempts(claim.Value, GetRemoteIpAddress());
            }
        }

        private string GetRemoteIpAddress()
        {
            return HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private object BuildLoginResponse(UserModel user)
        {
            return new
            {
                isAuthenticate = user?.isAuthenticate ?? false,
                isAlreadyLoggedIn = user?.isAlreadyLoggedIn ?? false,
                errorCode = user?.ErrorCode,
                errorTitle = user?.ErrorTitle,
                errorMsg = user?.ErrorMsg,
                retryAfterSeconds = user?.RetryAfterSeconds,
                passwordChangeRequired = user?.passwordChangeRequired ?? false,
                changePassword = user?.changePassword
            };
        }

    }
}
