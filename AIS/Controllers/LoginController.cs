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
using System.Linq;
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
        private readonly PasswordChangeTokenService _passwordChangeTokenService;
        private readonly PasswordChangeStateStore _passwordChangeStateStore;
        private readonly IClientIpResolver _clientIpResolver;

        public LoginController(ILogger<LoginController> logger, SessionHandler sessionHandler, DBConnection dbConnection, IConfiguration configuration, LoginAttemptTracker loginAttemptTracker, PasswordPolicyValidator passwordPolicyValidator, SecurityTokenService tokenService, IPermissionService permissionService, LoginViewResolver loginViewResolver, PasswordChangeTokenService passwordChangeTokenService, PasswordChangeStateStore passwordChangeStateStore, IClientIpResolver clientIpResolver)
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
            _passwordChangeTokenService = passwordChangeTokenService;
            _passwordChangeStateStore = passwordChangeStateStore;
            _clientIpResolver = clientIpResolver;
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
            LogClientIpTrace("DoLogin");
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

                var user = dBConnection.AutheticateLogin(loginModel);
                _logger.LogDebug("Authentication outcome for PP {PPNumber}: Authenticated={IsAuthenticated}, AlreadyLoggedIn={AlreadyLoggedIn}, ErrorCode={ErrorCode}.", login.PPNumber, user.isAuthenticate, user.isAlreadyLoggedIn, user.ErrorCode);

                if (user.isAuthenticate)
                {
                    ResetRateLimit(loginModel);
                    RefreshAuthenticatedContexts(user);
                }

                if (user.isAuthenticate && RequiresPasswordChange(user))
                {
                    IssuePasswordChangeToken(user);
                    ClearAuthCookies();
                    return Json(BuildLoginResponse(user, forcePwdChange: true, redirectUrl: BuildChangePasswordRedirectUrl()));
                }

                if (user.isAuthenticate && user.isAlreadyLoggedIn)
                {
                    user.ErrorTitle ??= "Session Details";
                    user.ErrorMsg ??= "You are already logged in System";
                    return Json(BuildLoginResponse(user));
                }

                if (user.ID != 0 && user.isAuthenticate)
                {
                    var contextCount = user.AvailableContexts?.Count ?? 0;
                    if (contextCount > 1)
                    {
                        PersistPendingContextState(user);
                        return Json(BuildLoginResponse(user, redirectUrl: BuildContextSelectionRedirectUrl(), requiresContextSelection: true));
                    }

                    if (contextCount == 1 && !user.UserContextAssignmentId.HasValue)
                    {
                        dBConnection.ApplyLoginContext(user, user.AvailableContexts[0]);
                    }

                    await CompleteAuthenticatedLoginAsync(user);
                    return Json(BuildLoginResponse(user));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(user?.ErrorCode))
                    {
                        RegisterFailedAttempt(loginModel);
                        user.isAuthenticate = false;
                        user.ErrorCode = "INVALID_CREDENTIALS";
                        user.ErrorTitle = "Sign in failed";
                        user.ErrorMsg = "Invalid user ID or password.";
                    }
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SelectContext()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(BuildHomeRedirectUrl());
            }

            if (!TryGetPendingLoginContextState(out var pendingState))
            {
                return RedirectToAction(nameof(Index));
            }

            if (pendingState.Contexts.Count == 1)
            {
                return await CompleteContextSelectionAsync(pendingState, pendingState.Contexts[0].AssignmentId);
            }

            return View("SelectContext", BuildContextSelectionViewModel(pendingState));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectContext([FromForm] ContextSelectionPostModel model)
        {
            if (!TryGetPendingLoginContextState(out var pendingState))
            {
                return RedirectToAction(nameof(Index));
            }

            var selectedAssignmentId = model?.AssignmentId ?? 0;
            var selectedContext = pendingState.Contexts
                .FirstOrDefault(context => context != null && context.AssignmentId == selectedAssignmentId);

            if (selectedContext == null)
            {
                var viewModel = BuildContextSelectionViewModel(pendingState);
                viewModel.ErrorMessage = "Select one role and posting context to continue.";
                return View("SelectContext", viewModel);
            }

            return await CompleteContextSelectionAsync(pendingState, selectedAssignmentId);
        }

        [HttpGet]
        [AllowAnonymous]
        public Task<IActionResult> ConfirmContext()
        {
            return SelectContext();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> ConfirmContext([FromForm] ContextSelectionPostModel model)
        {
            return SelectContext(model);
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
            LogClientIpTrace("ResetPassword");
            _logger.LogInformation("Password reset request received for PP {MaskedIdentifier} from {RemoteIp}.", maskedIdentifier, GetRemoteIpAddress());

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

            var displayName = !string.IsNullOrWhiteSpace(sessionUser?.Name)
                ? sessionUser.Name.Trim()
                : sessionUser?.PPNumber ?? string.Empty;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, sessionUser.ID.ToString()),
                new Claim(ClaimTypes.Name, displayName),
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

            if (sessionUser.UserContextAssignmentId.HasValue)
            {
                claims.Add(new Claim("contextId", sessionUser.UserContextAssignmentId.Value.ToString()));
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
            return _clientIpResolver?.GetClientIp(HttpContext) ?? HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private void LogClientIpTrace(string operation)
            {
            var request = HttpContext?.Request;
            var connection = HttpContext?.Connection;
            _logger.LogInformation(
                "IAS client IP trace for {Operation}: RemoteIpAddress={RemoteIpAddress}; LocalIpAddress={LocalIpAddress}; XForwardedFor={XForwardedFor}; XRealIP={XRealIP}; Forwarded={Forwarded}; Host={Host}; Scheme={Scheme}; ResolvedClientIp={ResolvedClientIp}.",
                operation,
                connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                connection?.LocalIpAddress?.ToString() ?? string.Empty,
                request?.Headers["X-Forwarded-For"].ToString() ?? string.Empty,
                request?.Headers["X-Real-IP"].ToString() ?? string.Empty,
                request?.Headers["Forwarded"].ToString() ?? string.Empty,
                request?.Host.ToString() ?? string.Empty,
                request?.Scheme ?? string.Empty,
                GetRemoteIpAddress());
            }

        private object BuildLoginResponse(UserModel user, bool forcePwdChange = false, string redirectUrl = null, bool? requiresContextSelection = null)
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
                changePassword = user?.changePassword,
                forcePwdChange,
                requiresContextSelection = requiresContextSelection ?? user?.RequiresContextSelection ?? false,
                redirectUrl
            };
        }

        private static bool RequiresPasswordChange(UserModel user)
        {
            if (user == null)
            {
                return false;
            }

            return user.passwordChangeRequired ||
                   string.Equals(user.changePassword, "Y", StringComparison.OrdinalIgnoreCase);
        }

        private void RefreshAuthenticatedContexts(UserModel user)
        {
            if (user == null || !user.isAuthenticate || string.IsNullOrWhiteSpace(user.PPNumber))
            {
                return;
            }

            var contexts = dBConnection.GetLoginContexts(user.PPNumber);
            user.AvailableContexts = contexts;
            user.AssignmentCount = contexts.Count;
            user.RequiresContextSelection = contexts.Count > 1;

            if (contexts.Count == 0)
            {
                ClearResolvedLoginContext(user);
                user.isAuthenticate = false;
                user.ErrorCode ??= "NO_LOGIN_CONTEXT";
                user.ErrorTitle ??= "No login context assigned";
                user.ErrorMsg ??= "Your account is active, but no valid role and entity assignment is available. Please contact IAS administration.";
                return;
            }

            if (contexts.Count == 1)
            {
                dBConnection.ApplyLoginContext(user, contexts[0]);
                return;
            }

            ClearResolvedLoginContext(user);
        }

        private static void ClearResolvedLoginContext(UserModel user)
        {
            if (user == null)
            {
                return;
            }

            user.UserContextAssignmentId = null;
            user.UserGroupID = null;
            user.UserRoleID = null;
            user.UserGroup = null;
            user.UserRole = null;
            user.UserRoleName = null;
            user.UserEntityID = null;
            user.UserEntityName = null;
            user.UserParentEntityID = null;
            user.UserParentEntityName = null;
            user.RelationshipId = null;
            user.UserEntityTypeID = null;
            user.UserParentEntityTypeID = null;
            user.UserEntityCode = null;
            user.UserParentEntityCode = null;
        }

        private void IssuePasswordChangeToken(UserModel user)
        {
            var token = _passwordChangeTokenService.CreateToken(user.ID, user.PPNumber);
            _passwordChangeTokenService.AppendCookie(Response, token, Request.PathBase);
            _passwordChangeStateStore.Store(token, user);
        }

        private void PersistPendingContextState(UserModel user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var pendingState = new PendingLoginContextState
            {
                UserId = user.ID,
                PPNumber = user.PPNumber,
                Name = user.Name,
                Email = user.Email,
                IsActive = user.IsActive,
                UserLocationType = user.UserLocationType,
                ChangePassword = user.changePassword,
                PasswordChangeRequired = user.passwordChangeRequired,
                CreatedAt = DateTimeOffset.UtcNow,
                Contexts = user.AvailableContexts
                    ?.Where(context => context != null)
                    .Select(CloneUserContext)
                    .ToList()
                    ?? new List<UserContextAssignmentModel>()
            };

            sessionHandler.SetPendingLoginContextState(pendingState);
        }

        private bool TryGetPendingLoginContextState(out PendingLoginContextState pendingState)
        {
            pendingState = null;
            if (!sessionHandler.TryGetPendingLoginContextState(out pendingState) || pendingState == null)
            {
                return false;
            }

            if (pendingState.CreatedAt != default &&
                pendingState.CreatedAt.AddMinutes(15) < DateTimeOffset.UtcNow)
            {
                sessionHandler.ClearPendingLoginContextState();
                pendingState = null;
                return false;
            }

            if (pendingState.Contexts == null || pendingState.Contexts.Count == 0)
            {
                sessionHandler.ClearPendingLoginContextState();
                pendingState = null;
                return false;
            }

            return true;
        }

        private ContextSelectionViewModel BuildContextSelectionViewModel(PendingLoginContextState pendingState)
        {
            return new ContextSelectionViewModel
            {
                PPNumber = pendingState?.PPNumber,
                UserName = pendingState?.Name,
                Contexts = (pendingState?.Contexts ?? new List<UserContextAssignmentModel>())
                    .Where(context => context != null)
                    .OrderByDescending(context => string.Equals(context.IsDefault, "Y", StringComparison.OrdinalIgnoreCase))
                    .ThenBy(context => context.RoleName)
                    .ThenBy(context => context.EntityName)
                    .ToList()
            };
        }

        private async Task<IActionResult> CompleteContextSelectionAsync(PendingLoginContextState pendingState, int assignmentId)
        {
            var validatedContext = dBConnection.GetValidatedLoginContext(pendingState?.PPNumber, assignmentId, pendingState?.UserId);
            if (validatedContext == null)
            {
                sessionHandler.ClearPendingLoginContextState();
                return RenderLoginView(errorMessage: "The selected role and posting context is no longer available. Please sign in again.");
            }

            var user = BuildUserFromPendingState(pendingState);
            dBConnection.ApplyLoginContext(user, validatedContext);
            await CompleteAuthenticatedLoginAsync(user);
            return LocalRedirect(BuildHomeRedirectUrl());
        }

        private UserModel BuildUserFromPendingState(PendingLoginContextState pendingState)
        {
            return new UserModel
            {
                ID = pendingState?.UserId ?? 0,
                PPNumber = pendingState?.PPNumber,
                Name = pendingState?.Name,
                Email = pendingState?.Email,
                IsActive = pendingState?.IsActive,
                UserLocationType = pendingState?.UserLocationType,
                changePassword = pendingState?.ChangePassword,
                passwordChangeRequired = pendingState?.PasswordChangeRequired ?? false,
                isAuthenticate = true,
                isAlreadyLoggedIn = false
            };
        }

        private async Task CompleteAuthenticatedLoginAsync(UserModel user)
        {
            var sessionUser = dBConnection.CreateLoginSession(user);
            IssueApplicationSession(user);
            SetMustChangePasswordFlag(user);
            await SignInUserAsync(sessionUser);
        }

        private void IssueApplicationSession(UserModel user)
        {
            if (user == null || !int.TryParse(user.PPNumber, out var ppNumber))
            {
                return;
            }

            dBConnection.KillSessions(ppNumber);
            var sessionToken = _tokenService.GenerateSessionToken();
            dBConnection.CreateSession(
                sessionToken,
                ppNumber,
                _clientIpResolver.GetClientIp(HttpContext),
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

        private string BuildContextSelectionRedirectUrl()
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            return string.Concat(pathBase, "/Login/SelectContext");
        }

        private string BuildHomeRedirectUrl()
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            return string.Concat(pathBase, "/Home/Index");
        }

        private static UserContextAssignmentModel CloneUserContext(UserContextAssignmentModel context)
        {
            if (context == null)
            {
                return null;
            }

            return new UserContextAssignmentModel
            {
                AssignmentId = context.AssignmentId,
                UserId = context.UserId,
                PPNumber = context.PPNumber,
                GroupId = context.GroupId,
                RoleId = context.RoleId,
                RoleName = context.RoleName,
                EntityId = context.EntityId,
                EntityName = context.EntityName,
                ParentEntityId = context.ParentEntityId,
                ParentEntityName = context.ParentEntityName,
                RelationshipTypeId = context.RelationshipTypeId,
                RelationshipTypeName = context.RelationshipTypeName,
                EntityTypeId = context.EntityTypeId,
                ParentEntityTypeId = context.ParentEntityTypeId,
                EntityCode = context.EntityCode,
                ParentEntityCode = context.ParentEntityCode,
                UserLocationType = context.UserLocationType,
                UserPostingAuditZone = context.UserPostingAuditZone,
                UserPostingDiv = context.UserPostingDiv,
                UserPostingDept = context.UserPostingDept,
                UserPostingBranch = context.UserPostingBranch,
                UserPostingZone = context.UserPostingZone,
                IsDefault = context.IsDefault,
                IsActive = context.IsActive
            };
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("IAS.Auth");
            Response.Cookies.Delete("IAS.Session");
            Response.Cookies.Delete("IAS_SESSION");
        }

        private string BuildChangePasswordRedirectUrl()
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            return string.Concat(pathBase, "/Home/Change_Password");
        }

    }
}
