using AIS.Exceptions;
using AIS.Models;
using AIS.Filters;
using AIS.Models.AIS.Models;
using AIS.Models.AIS.Models.Execution;
using AIS.Models.CAU;
using AIS.Models.HD;
using AIS.Models.IID;
using AIS.Models.Reports;
using AIS.Models.Requests;
using AIS.Models.SM;
using AIS.Security.Cryptography;
using AIS.Security.PasswordPolicy;
using AIS.Services;
using AIS.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServicesCsvSanitizer = AIS.Services.CsvSanitizer;

namespace AIS.Controllers
    {
    [IgnoreAntiforgeryToken]
    [EnableRateLimiting("GeneralApiPolicy")]
    public partial class ApiCallsController : Controller
        {
        private readonly ILogger<ApiCallsController> _logger;

        private readonly SessionHandler sessionHandler;
        private readonly DBConnection dBConnection;
        private readonly DBConnectionArchive archiveDbConnection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly SecurityTokenService _tokenService;
        private readonly PasswordPolicyValidator _passwordPolicyValidator;
        private readonly IStaticAssetVersionTokenProvider _staticAssetVersionTokenProvider;
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> PostAuditComplianceSuspiciousFailures = new ConcurrentDictionary<string, Queue<DateTime>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Regex AlphaNumericWithSpacesRegex = new Regex("^[A-Za-z0-9 &]+$", RegexOptions.Compiled);
        private static readonly Regex ExceptionReportTextRegex = new Regex("^[A-Za-z0-9 &,?]+$", RegexOptions.Compiled);
        private static readonly Regex ObservationHeadingRegex = new Regex("^[A-Za-z0-9 &,?]+$", RegexOptions.Compiled);
        private static readonly Regex RichTextTagRegex = new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.Singleline);
        private const string ObservationHeadingValidationMessage = "Observation Heading/Title can contain only alphabets, numbers, space, &, ?, and comma.";
        private const string PostAuditComplianceEndpoint = "/ApiCalls/submit_post_audit_compliance";

        public ApiCallsController(
            ILogger<ApiCallsController> logger,
            SessionHandler _sessionHandler,
            DBConnection _dbCon,
            DBConnectionArchive archiveDbCon,
            IWebHostEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            SecurityTokenService tokenService,
            PasswordPolicyValidator passwordPolicyValidator,
            IStaticAssetVersionTokenProvider staticAssetVersionTokenProvider)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            archiveDbConnection = archiveDbCon;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _tokenService = tokenService;
            _passwordPolicyValidator = passwordPolicyValidator;
            _staticAssetVersionTokenProvider = staticAssetVersionTokenProvider;
            }

        public override void OnActionExecuting(ActionExecutingContext context)
            {
            base.OnActionExecuting(context);
            }

        private static int? SafeOracleNullableInt(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return null;
                }

            if (value is OracleDecimal oracleDecimal)
                {
                return oracleDecimal.IsNull ? (int?)null : oracleDecimal.ToInt32();
                }

            try
                {
                return Convert.ToInt32(value);
                }
            catch
                {
                return int.TryParse(value.ToString(), out var parsed) ? parsed : (int?)null;
                }
            }

        private static int SafeOracleInt(object value, int defaultValue = 0)
            {
            return SafeOracleNullableInt(value) ?? defaultValue;
            }

        private IActionResult EnsureAuthenticatedSession()
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { error = "unauthorized", message = "User session is not authenticated." });
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized(new { error = "unauthorized", message = "User session is not available. Please sign in again." });
                }

            return null;
            }

        private IActionResult ExecuteCommercialAuditRequest(Func<IActionResult> action, string failureMessage)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            try
                {
                return action();
                }
            catch (DatabaseUnavailableException ex)
                {
                _logger.LogError(ex, "Commercial Audit request failed because the database is unavailable.");
                var detail = ResolveCommercialAuditFailureMessage(ex, failureMessage);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                    {
                    ok = false,
                    message = detail,
                    detail
                    });
                }
            catch (Exception ex)
                {
                var detail = ResolveCommercialAuditFailureMessage(ex, failureMessage);
                _logger.LogError(ex, "Commercial Audit request failed. Detail: {Detail}", detail);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                    ok = false,
                    message = detail,
                    detail
                    });
                }
            }

        private static string ResolveCommercialAuditFailureMessage(Exception ex, string fallbackMessage)
            {
            var detail = ex?.GetBaseException()?.Message;
            if (string.IsNullOrWhiteSpace(detail))
                {
                detail = ex?.Message;
                }

            return string.IsNullOrWhiteSpace(detail) ? fallbackMessage : detail.Trim();
            }

        private string SaveUploadFile(IFormFile file)
            {
            if (file == null || file.Length == 0)
                {
                return string.Empty;
                }

            var uploadsPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
            Directory.CreateDirectory(uploadsPath);
            var safeFileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid():N}_{safeFileName}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                {
                file.CopyTo(stream);
                }

            return uniqueFileName;
            }

        private string BuildUploadsFileUrl(string fileName)
            {
            if (string.IsNullOrWhiteSpace(fileName))
                {
                return string.Empty;
                }

            return Url.Content("~/Uploads/" + Uri.EscapeDataString(fileName.Trim())) ?? string.Empty;
            }

        private IActionResult EnsureSbpAccess()
            {
            if (!sessionHandler.HasSbpAccess())
                {
                return Unauthorized(new { success = false, message = "SBP access not authorized. Please authenticate first." });
                }

            return null;
            }

        private DBConnection CreateDbConnection()
            {
            if (_httpContextAccessor?.HttpContext == null)
                throw new InvalidOperationException("HTTP context accessor is not available for database operations.");
            if (_configuration == null)
                throw new InvalidOperationException("Configuration dependency is not available for database operations.");

            return DBConnection.CreateFromHttpContext(_httpContextAccessor, _configuration, sessionHandler, _tokenService);
            }

        private IActionResult InvalidModelStateResponse(string message = "Invalid request")
            {
            var endpointName = ControllerContext?.ActionDescriptor?.DisplayName ?? "Unknown";
            var errors = ValidationErrorHelper.BuildModelErrors(ModelState);
            ValidationErrorHelper.LogValidationErrors(_logger, endpointName, ModelState);
            HttpContext.Items["AjaxModelErrors"] = errors;
            return BadRequest(ValidationErrorHelper.BuildInvalidRequestResponse(ModelState, message));
            }

        private IActionResult InvalidRequestResponse(string field, string error, string message = "Invalid request")
            {
            var endpointName = ControllerContext?.ActionDescriptor?.DisplayName ?? "Unknown";
            var response = ValidationErrorHelper.BuildInvalidRequestResponse(field, error, message);
            _logger.LogWarning("Validation failed for {Endpoint}. Errors: {@Errors}", endpointName, response.Errors);
            HttpContext.Items["AjaxModelErrors"] = response.Errors;
            return BadRequest(response);
            }

        private static bool HasMeaningfulRichTextContent(string html)
            {
            if (string.IsNullOrWhiteSpace(html))
                {
                return false;
                }

            var withoutTags = RichTextTagRegex.Replace(html, " ");
            var decoded = System.Net.WebUtility.HtmlDecode(withoutTags) ?? string.Empty;
            var normalized = decoded.Replace('\u00A0', ' ').Trim();
            return !string.IsNullOrWhiteSpace(normalized);
            }

        private static bool IsValidObservationHeading(string heading)
            {
            return !string.IsNullOrWhiteSpace(heading)
                && ObservationHeadingRegex.IsMatch(heading.Trim());
            }

        private IActionResult InvalidObservationHeadingResponse()
            {
            _logger.LogWarning("Observation heading validation failed for {Endpoint}.", ControllerContext?.ActionDescriptor?.DisplayName ?? "Unknown");
            return Ok(new
                {
                Status = false,
                Message = ObservationHeadingValidationMessage
                });
            }

        private JsonResult LegacyMessageResponse(string message, string fallbackMessage)
            {
            var normalizedMessage = string.IsNullOrWhiteSpace(message) ? fallbackMessage : message.Trim();
            return Json(new { Status = true, Message = normalizedMessage });
            }

        private static SBPObservationCreateModel MapSbpObservationRequest(SbpObservationRequest request)
            {
            return new SBPObservationCreateModel
                {
                ParaId = request.ParaId,
                RefNo = request.RefNo?.Trim(),
                FunctionName = request.FunctionName?.Trim(),
                ParaNo = request.ParaNo?.Trim(),
                SBPObservation = request.SBPObservation,
                SBPDirections = request.SBPDirections,
                ComplianceQuarter = request.ComplianceQuarter?.Trim(),
                ObservationTypeId = request.ObservationTypeId,
                User = request.User?.Trim()
                };
            }

        private static SBPObservationResponseCreateModel MapSbpObservationResponseRequest(SbpObservationResponseRequest request)
            {
            return new SBPObservationResponseCreateModel
                {
                ParaId = NumericParsing.ToLongOrDefault(request.ParaId),
                RefNo = request.RefNo?.Trim(),
                BankResponse = request.BankResponse,
                ReplyDate = request.ReplyDate,
                ComplianceStatus = request.ComplianceStatus?.Trim(),
                IADValidation = request.IADValidation?.Trim(),
                User = request.User?.Trim()
                };
            }

        private static SBPObservationResponseUpdateModel MapSbpObservationResponseUpdateRequest(SbpObservationResponseUpdateRequest request)
            {
            var model = MapSbpObservationResponseRequest(request);
            return new SBPObservationResponseUpdateModel
                {
                ParaId = model.ParaId,
                RefNo = model.RefNo,
                BankResponse = model.BankResponse,
                ReplyDate = model.ReplyDate,
                ComplianceStatus = model.ComplianceStatus,
                IADValidation = model.IADValidation,
                User = model.User,
                ResponseId = NumericParsing.ToLongOrDefault(request.ResponseId)
                };
            }

        [HttpPost]
        public async Task<IActionResult> upload_post_compliance_evidences(List<IFormFile> files)
            {
            // Directory path where files will be stored
            var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "Audit_Evidences");

            // Ensure the directory exists
            if (!Directory.Exists(uploadPath))
                {
                Directory.CreateDirectory(uploadPath);
                }

            foreach (var file in files)
                {
                if (file.Length > 0)
                    {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Save the file to the specified directory
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                        await file.CopyToAsync(stream);
                        }
                    }
                }

            return Ok(new { Message = "Files uploaded successfully!" });
            }

        [HttpPost]
        public bool kill_session(LoginModel user)
            {
            try
                {
                return dBConnection.KillExistSession(user);
                }
            catch (DatabaseUnavailableException ex)
                {
                _logger.LogError(ex, "Database connection is unavailable while attempting to kill a session via API.");
                return false;
                }

            }
        [HttpPost]
        public bool terminate_idle_session()
            {
            dBConnection.TerminateIdleSession();
            return true;
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_CRITERIA_POST_CHANGE", "AUDIT_EXECUTION", "AUDIT_CRITERIA", "PKG_PG", "P_UPDATEAUDITCRITERIA", ObjectType = "AUDIT_CRITERIA")]
        public IActionResult PostChangesAuditCriteria([FromForm] List<string> CRITERIA_LIST)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            if (CRITERIA_LIST == null || CRITERIA_LIST.Count < 9)
                {
                return BadRequest(new { error = "invalid_request", message = "Criteria list is missing required values." });
                }

            var model = new AddAuditCriteriaModel
                {
                    ID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[0]),
                    AUDITPERIODID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[1]),
                    ENTITY_TYPEID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[2]),
                    RISK_ID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[3]),
                    FREQUENCY_ID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[4]),
                    SIZE_ID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[5]),
                    NO_OF_DAYS = NumericParsing.ToIntOrDefault(CRITERIA_LIST[6]),
                    VISIT = (CRITERIA_LIST[7].ToLower() == "y") ? "Y" : "N",
                    APPROVAL_STATUS = 6
                };

            var updated = dBConnection.UpdateAuditCriteria(model, CRITERIA_LIST[8]);
            return updated ? Ok(new { Status = true }) : BadRequest(new { Status = false, Message = "Oracle did not confirm the criteria update." });
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_CRITERIA_UPDATED", "AUDIT_EXECUTION", "AUDIT_CRITERIA", "PKG_PG", "P_UPDATEAUDITCRITERIA", ObjectType = "AUDIT_CRITERIA")]
        public IActionResult UpdateAuditCriteria([FromForm] List<string> CRITERIA_LIST)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            if (CRITERIA_LIST == null || CRITERIA_LIST.Count < 9)
                {
                return BadRequest(new { error = "invalid_request", message = "Criteria list is missing required values." });
                }

            var model = new AddAuditCriteriaModel
                {
                    ID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[0]),
                    AUDITPERIODID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[1]),
                    ENTITY_TYPEID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[2]),
                    RISK_ID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[3]),
                    FREQUENCY_ID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[4]),
                    SIZE_ID = NumericParsing.ToIntOrDefault(CRITERIA_LIST[5]),
                    NO_OF_DAYS = NumericParsing.ToIntOrDefault(CRITERIA_LIST[6]),
                    VISIT = (CRITERIA_LIST[7].ToLower() == "y") ? "Y" : "N",
                    APPROVAL_STATUS = 3
                };

            var updated = dBConnection.UpdateAuditCriteria(model, CRITERIA_LIST[8]);
            return updated ? Ok(new { Status = true }) : BadRequest(new { Status = false, Message = "Oracle did not confirm the criteria update." });
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_CRITERIA_REFERRED_BACK", "AUDIT_EXECUTION", "AUDIT_CRITERIA", "PKG_PG", "P_SETAUDITCRITERIASTATUSREFERREDBACK", ObjectType = "AUDIT_CRITERIA", RequireNonEmpty = "DATALIST")]
        public IActionResult ReferredBackAuditCriteria([FromForm] List<CriteriaIDComment> DATALIST)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            if (DATALIST == null || DATALIST.Count == 0)
                return BadRequest(new { Status = false, Message = "No criteria received." });
            var allUpdated = true;
            if (DATALIST.Count > 0)
                {
                foreach (var criteria in DATALIST)
                    {
                    allUpdated &= dBConnection.SetAuditCriteriaStatusReferredBack(criteria.ID.GetValueOrDefault(), criteria.COMMENT);
                    }
                }

            return allUpdated ? Ok(new { Status = true }) : BadRequest(new { Status = false, Message = "Oracle did not confirm every referral." });
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_CRITERIA_APPROVED", "AUDIT_EXECUTION", "AUDIT_CRITERIA", "PKG_PG", "P_SETAUDITCRITERIASTATUSAPPROVE", ObjectType = "AUDIT_CRITERIA", RequireNonEmpty = "datalist")]
        public IActionResult AuthorizeAuditCriteria([FromBody] List<CriteriaIDComment> datalist)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null) return unauthorized;

            if (datalist == null || datalist.Count == 0)
                return BadRequest(new { status = false, message = "No criteria received." });

            foreach (var criteria in datalist)
                {
                var id = criteria?.ID.GetValueOrDefault() ?? 0;
                var comment = criteria?.COMMENT;

                if (id <= 0)
                    return BadRequest(new { status = false, message = "Invalid Criteria ID received." });

                var response = dBConnection.SetAuditCriteriaStatusApprove(id, comment);
                if (string.IsNullOrWhiteSpace(response))
                    return BadRequest(new { status = false, message = "Oracle did not confirm every approval." });
                }

            return Ok(new { status = true });
            }

        [HttpPost]
        public IActionResult GetAuditTeam([FromForm] int dept_code)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            return Ok(dBConnection.GetAuditTeams(dept_code));
            }

        [HttpPost]
        public IActionResult GeneratePlanAuditCriteria([FromForm] int CRITERIA_ID)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            var message = dBConnection.GeneratePlanForAuditCriteria(CRITERIA_ID);
            return Ok(new { Message = message });
            }

        [HttpPost]
        public IActionResult AddAuditPlan([FromForm] AuditPlanModel plan)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            return Ok(plan);
            }

        [HttpPost]
        public IActionResult GetZoneBranches([FromForm] int zone_code, [FromForm] bool session_check = true)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            return Ok(dBConnection.GetBranches(zone_code, session_check));
            }

        [HttpPost]
        public IActionResult GetDivDepartments([FromForm] int div_code)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            return Ok(dBConnection.GetDepartments(div_code, false));
            }

        [HttpPost]
        public IActionResult GetAuditTeams([FromForm] int dept_code)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            return Ok(dBConnection.GetAuditTeams(dept_code));
            }

        [HttpPost]
        public IActionResult GetOperationalStartDate([FromForm] int periodId, [FromForm] int entityCode)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            return Ok(dBConnection.GetAuditOperationalStartDate(periodId, entityCode));
            }

        [HttpPost]
        [ApplicationAudit("ENGAGEMENT_PLAN_CREATED", "AUDIT_PLANNING", "ENGAGEMENT_PLANNING", "PKG_PG", "P_ADDAUDITENGAGEMENTPLAN", EngagementId = "eng.ENG_ID", ObjectType = "ENGAGEMENT_PLAN", ObjectId = "eng.ENG_ID")]
        public async Task<IActionResult> AddEngagementPlan([FromForm] AuditEngagementPlanModel eng)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            if ((eng?.TEAM_ID).GetValueOrDefault() > 0 && string.IsNullOrWhiteSpace(eng.TEAM_NAME))
                {
                eng.TEAM_NAME = dBConnection.GetAuditTeams()
                    .FirstOrDefault(item => item.T_ID == eng.TEAM_ID.Value)
                    ?.NAME;
                }

            var result = dBConnection.AddAuditEngagementPlan(eng);
            if (string.Equals(result?.IS_SUCCESS, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                var notificationData = dBConnection.GetAuditTaskAssignedNotificationData(result);
                await EmailNotification.SendAuditTaskAssignedAsync(_configuration, notificationData, HttpContext?.RequestServices);
                }

            return string.Equals(result?.IS_SUCCESS, "Yes", StringComparison.OrdinalIgnoreCase)
                ? Ok(result)
                : BadRequest(result);
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_TEAM_CREATED", "AUDIT_PLANNING", "AUDIT_TEAM", "PKG_PG", "P_ADDAUDITTEAM", ObjectType = "AUDIT_TEAM")]
        public IActionResult AddAuditTeam([FromForm] List<AddAuditTeamModel> AUDIT_TEAM)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            if (AUDIT_TEAM == null || AUDIT_TEAM.Count == 0)
                {
                return BadRequest(new { Status = false, Message = "No team members supplied." });
                }

            var newTeamId = dBConnection.GetLatestTeamID();
            foreach (var item in AUDIT_TEAM.Select((model, index) => (model, index)))
                {
                if (!TryValidateModel(item.model, prefix: $"[{item.index}]") || !ModelState.IsValid)
                    {
                    return InvalidModelStateResponse();
                    }

                if (ContainsHtmlEncodedPayload(item.model?.T_NAME))
                    {
                    ModelState.AddModelError($"[{item.index}].T_NAME", "HTML content is not allowed in team name.");
                    return InvalidModelStateResponse();
                    }

                if (ContainsHtmlEncodedPayload(item.model?.NAME))
                    {
                    ModelState.AddModelError($"[{item.index}].NAME", "HTML content is not allowed in team member name.");
                    return InvalidModelStateResponse();
                    }
                }

            var responses = new List<string>();
            foreach (var item in AUDIT_TEAM)
                {
                var ateam = new AuditTeamModel
                    {
                        T_ID = newTeamId,
                        CODE = newTeamId.ToString(),
                        NAME = item.T_NAME,
                        EMPLOYEENAME = item.NAME,
                        TEAMMEMBER_ID = item.PPNO.GetValueOrDefault(),
                        IS_TEAMLEAD = item.ISTEAMLEAD,
                        PLACE_OF_POSTING = item.PLACEOFPOSTING,
                        STATUS = "Y"
                    };

                responses.Add(dBConnection.AddAuditTeam(ateam));
                }

            var saved = responses.Count == AUDIT_TEAM.Count && responses.All(response => !string.IsNullOrWhiteSpace(response));
            return saved
                ? Ok(new { Status = true, Message = "Audit team saved successfully." })
                : BadRequest(new { Status = false, Message = "Oracle did not confirm every audit-team member save." });
            }

        private static bool ContainsHtmlEncodedPayload(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return false;
                }

            var input = value.Trim();

            return input.Contains("&lt;", StringComparison.OrdinalIgnoreCase)
                || input.Contains("&gt;", StringComparison.OrdinalIgnoreCase)
                || input.Contains("&#60;", StringComparison.OrdinalIgnoreCase)
                || input.Contains("&#62;", StringComparison.OrdinalIgnoreCase)
                || input.Contains("&#x3c;", StringComparison.OrdinalIgnoreCase)
                || input.Contains("&#x3e;", StringComparison.OrdinalIgnoreCase)
                || input.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
                || Regex.IsMatch(input, @"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_TEAM_DELETED", "AUDIT_PLANNING", "AUDIT_TEAM", "PKG_PG", "P_DELETEAUDITTEAM", ObjectType = "AUDIT_TEAM", ObjectId = "T_CODE")]
        public IActionResult DeleteAuditTeam([FromForm] string T_CODE)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            var deleted = dBConnection.DeleteAuditTeam(T_CODE);
            return Ok(new { Status = deleted });
            }

        [HttpPost]
        public IActionResult GetAuditEmployees([FromForm] int dept_code = 0)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            return Ok(dBConnection.GetAuditEmployees(dept_code));
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("BRANCH_ADD", "ADMINISTRATION", "ADMINISTRATION", "", "", ObjectType = "BRANCH_ADD")]
        public BranchModel branch_add(BranchModel br)
            {
            if (br.ISACTIVE == "Active")
                br.ISACTIVE = "Y";
            else if (br.ISACTIVE == "InActive")
                br.ISACTIVE = "N";

            if (br.BRANCHID == 0)
                br = dBConnection.AddBranch(br);
            else
                br = dBConnection.UpdateBranch(br);
            return br;
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_CONTROL_VIOLATION", "ADMINISTRATION", "ADMINISTRATION", "", "", ObjectType = "ADD_CONTROL_VIOLATION")]
        public ControlViolationsModel add_control_violation(ControlViolationsModel cv)
            {
            return dBConnection.AddControlViolation(cv);
            }

        [HttpGet]
        [HttpPost]
        public List<DepartmentModel> get_departments(int div_id)
            {
            return dBConnection.GetDepartments(div_id, false);
            }

        [HttpPost]
        public IActionResult get_ho_unit_types()
            {
            var list = dBConnection.GetDivisions(false);
            return Ok(list);
            }

        [HttpPost]
        public IActionResult get_ho_units(int divisionId)
            {
            var list = dBConnection.GetDepartments(divisionId, false);
            return Ok(list);
            }
        [HttpGet]
        [HttpPost]
        public List<SubEntitiesModel> get_sub_entities(int div_id = 0, int dept_id = 0)
            {
            return dBConnection.GetSubEntities(div_id, dept_id);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_SUB_ENTITY", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_ADDSUBENTITY", ObjectType = "ADD_SUB_ENTITY")]
        public SubEntitiesModel add_sub_entity(SubEntitiesModel entity)
            {
            if (entity.STATUS == "Active")
                entity.STATUS = "Y";
            else
                entity.STATUS = "N";
            if (entity.ID == 0)
                return dBConnection.AddSubEntity(entity);
            else
                return dBConnection.UpdateSubEntity(entity);
            }
        [HttpPost]
        public List<RiskProcessDetails> process_details(int ProcessId)
            {
            return dBConnection.GetRiskProcessDetails(ProcessId);
            }
        [HttpPost]
        public List<RiskProcessTransactions> process_transactions(int ProcessDetailId = 0, int transactionId = 0)
            {
            return dBConnection.GetRiskProcessTransactions(ProcessDetailId, transactionId);
            }
        [HttpGet]
        [HttpPost]
        public List<ChecklistDetailComparisonModel> get_checklist_detail_comparison_by_Id(int CHECKLIST_DETAIL_ID = 0)
            {
            return dBConnection.GetChecklistComparisonDetailById(CHECKLIST_DETAIL_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<ChecklistDetailComparisonModel> get_checklist_detail_comparison_by_Id_for_referredBack(int CHECKLIST_DETAIL_ID = 0)
            {
            return dBConnection.GetChecklistComparisonDetailByIdForRefferedBack(CHECKLIST_DETAIL_ID);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("PROCESS_ADD", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_AUDIT_CHECKLIST", ObjectType = "PROCESS_ADD")]
        public RiskProcessDefinition process_add(RiskProcessDefinition proc)
            {
            return dBConnection.AddRiskProcess(proc);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("SUB_PROCESS_ADD", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_AUDIT_CHECKLIST_SUB", ObjectType = "SUB_PROCESS_ADD")]
        public RiskProcessDetails sub_process_add(RiskProcessDetails subProc)
            {
            return dBConnection.AddRiskSubProcess(subProc);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("SUB_PROCESS_TRANSACTION_ADD", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_AUDIT_CHECKLIST_DETAIL", ObjectType = "SUB_PROCESS_TRANSACTION_ADD")]
        public RiskProcessTransactions sub_process_transaction_add(RiskProcessTransactions tran)
            {
            return dBConnection.AddRiskSubProcessTransaction(tran);
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("AUTHORIZE_SUB_PROCESS_BY_AUTHORIZER", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_APPROVED_SUB_PROCESS_BY_AUTHORIZER", ObjectType = "AUTHORIZE_SUB_PROCESS_BY_AUTHORIZER", ObjectId = "T_ID", RequireResultMessage = true)]
        public string authorize_sub_process_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeSubProcessByAuthorizer(T_ID, COMMENTS) + "\"}";

            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("REFFERED_BACK_SUB_PROCESS_BY_AUTHORIZER", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_REFFEREDBACK_SUB_CHECKLIST_BY_REVIEWER", ObjectType = "REFFERED_BACK_SUB_PROCESS_BY_AUTHORIZER", ObjectId = "T_ID", RequireResultMessage = true)]
        public string reffered_back_sub_process_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RefferedBackSubProcessByAuthorizer(T_ID, COMMENTS) + "\"}";

            }

        [HttpPost]
        public string recommend_process_transaction_by_reviewer(int T_ID, string COMMENTS, int? PROCESS_DETAIL_ID = null, int? SUB_PROCESS_ID = null, string HEADING = "", int? V_ID = null, int? CONTROL_ID = null, int? ROLE_ID = null, int? RISK_ID = null, string ANNEX_CODE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RecommendProcessTransactionByReviewer(T_ID, COMMENTS, PROCESS_DETAIL_ID.GetValueOrDefault(), SUB_PROCESS_ID.GetValueOrDefault(), HEADING, V_ID.GetValueOrDefault(), CONTROL_ID.GetValueOrDefault(), ROLE_ID.GetValueOrDefault(), RISK_ID.GetValueOrDefault(), ANNEX_CODE) + "\"}";

            }

        [HttpPost]
        public string reffered_back_process_transaction_by_reviewer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RefferedBackProcessTransactionByReviewer(T_ID, COMMENTS) + "\"}";
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("AUTHORIZE_PROCESS_TRANSACTION_BY_AUTHORIZER", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_APPROVE_CHECKLIST_BY_AUTHORIZER", ObjectType = "AUTHORIZE_PROCESS_TRANSACTION_BY_AUTHORIZER", ObjectId = "T_ID", RequireResultMessage = true)]
        public string authorize_process_transaction_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeProcessTransactionByAuthorizer(T_ID, COMMENTS) + "\"}";

            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("REFFERED_BACK_PROCESS_TRANSACTION_BY_AUTHORIZER", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_REFFEREDBACK_CHECKLIST_BY_AUTHORIZER", ObjectType = "REFFERED_BACK_PROCESS_TRANSACTION_BY_AUTHORIZER", ObjectId = "T_ID", RequireResultMessage = true)]
        public string reffered_back_process_transaction_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RefferedBackProcessTransactionByAuthorizer(T_ID, COMMENTS) + "\"}";

            }

        [HttpPost]
        public IActionResult InsertSbpObservation([FromBody] SbpObservationRequest request)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (request == null)
                {
                return InvalidRequestResponse("request", "Observation payload is required.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            if (request.ObservationTypeId == null || request.ObservationTypeId <= 0)
                {
                return InvalidRequestResponse("observationTypeId", "observationTypeId is required.");
                }

            var model = MapSbpObservationRequest(request);
            var db = CreateDbConnection();
            var paraId = db.InsertSbpObservation(model);
            if (paraId <= 0)
                {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unable to create observation." });
                }

            return Ok(new { success = true, paraId });
            }

        [HttpPost]
        public IActionResult UpdateSbpObservation([FromBody] SbpObservationRequest request)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (request == null || request.ParaId == null || request.ParaId <= 0)
                {
                return InvalidRequestResponse("paraId", "paraId is required for update.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            if (request.ObservationTypeId == null || request.ObservationTypeId <= 0)
                {
                return InvalidRequestResponse("observationTypeId", "observationTypeId is required.");
                }

            var model = MapSbpObservationRequest(request);
            var db = CreateDbConnection();
            db.UpdateSbpObservation(model);
            return Ok(new { success = true });
            }

        [HttpPost]
        public IActionResult InsertSbpObservationResponse([FromBody] SbpObservationResponseRequest request)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (request == null || request.ParaId == null || request.ParaId <= 0)
                {
                return InvalidRequestResponse("paraId", "paraId must be provided.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            var model = MapSbpObservationResponseRequest(request);
            var db = CreateDbConnection();
            db.InsertSbpObservationResponse(model);
            return Ok(new { success = true });
            }

        [HttpGet]
        public IActionResult GetSbpObservationResponse(long responseId)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (responseId <= 0)
                {
                return BadRequest(new { message = "responseId must be greater than zero." });
                }

            var db = CreateDbConnection();
            var result = db.GetSbpObservationResponse(responseId);
            if (result == null)
                {
                return NotFound(new { message = "Response not found." });
                }

            return Ok(result);
            }

        [HttpPost]
        public IActionResult UpdateSbpObservationResponse([FromBody] SbpObservationResponseUpdateRequest request)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (request == null || request.ResponseId == null || request.ResponseId <= 0)
                {
                return InvalidRequestResponse("responseId", "response_id is required.");
                }

            if (request.ParaId == null || request.ParaId <= 0)
                {
                return InvalidRequestResponse("paraId", "paraId must be provided.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            var model = MapSbpObservationResponseUpdateRequest(request);
            var db = CreateDbConnection();
            db.UpdateSbpObservationResponse(model);
            return Ok(new { success = true });
            }

        [HttpPost]
        public IActionResult RequestDeleteObservation([FromBody] ObsDeleteRequestDto dto)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (dto == null || dto.ParaId == null || dto.ParaId <= 0)
                {
                return BadRequest("Invalid ParaId");
                }

            var db = CreateDbConnection();
            var result = db.RequestDeleteObservation(dto.ParaId.GetValueOrDefault(), dto.Reason);
            return Ok(new { success = result.Success, message = result.Message, requestId = result.RequestId });
            }

        [HttpPost]
        public IActionResult RequestDeleteResponse([FromBody] RespDeleteRequestDto dto)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (dto == null || dto.ResponseId == null || dto.ResponseId <= 0)
                {
                return BadRequest("Invalid ResponseId");
                }

            var db = CreateDbConnection();
            var result = db.RequestDeleteResponse(dto.ResponseId.GetValueOrDefault(), dto.Reason);
            return Ok(new { success = result.Success, message = result.Message, requestId = result.RequestId });
            }

        [HttpPost]
        public IActionResult RequestReverse([FromBody] ReverseRequestDto dto)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (dto == null || dto.RequestIdToReverse == null || dto.RequestIdToReverse <= 0)
                {
                return BadRequest("Invalid RequestId");
                }

            var db = CreateDbConnection();
            var result = db.RequestReverse(dto.RequestIdToReverse.GetValueOrDefault(), dto.Reason);
            return Ok(new { success = result.Success, message = result.Message, requestId = result.RequestId });
            }

        [HttpPost]
        public IActionResult ApproveRequest([FromBody] ApproveRejectDto dto)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (dto == null || dto.RequestId == null || dto.RequestId <= 0)
                {
                return BadRequest("Invalid RequestId");
                }

            var db = CreateDbConnection();
            var result = db.ApproveRequest(dto.RequestId.GetValueOrDefault());
            return Ok(new { success = result.Success, message = result.Message });
            }

        [HttpPost]
        public IActionResult RejectRequest([FromBody] ApproveRejectDto dto)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (dto == null || dto.RequestId == null || dto.RequestId <= 0)
                {
                return BadRequest("Invalid RequestId");
                }

            var db = CreateDbConnection();
            var result = db.RejectRequest(dto.RequestId.GetValueOrDefault(), dto.Reason ?? "Rejected");
            return Ok(new { success = result.Success, message = result.Message });
            }

        [HttpGet]
        public IActionResult GetRequests([FromQuery] string status)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            var db = CreateDbConnection();
            var rows = db.GetRequests(status);
            return Ok(rows ?? new List<Dictionary<string, object>>());
            }

        [HttpGet]
        public IActionResult GetRequestHistory(long id)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (id <= 0)
                {
                return BadRequest("Invalid RequestId");
                }

            var db = CreateDbConnection();
            var rows = db.GetRequestHistory(id);
            return Ok(rows ?? new List<Dictionary<string, object>>());
            }

        [HttpGet]
        public IActionResult GetSbpObservationRegister([FromQuery] int observationTypeId)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            if (observationTypeId <= 0)
                {
                return BadRequest(new { message = "observationTypeId is required." });
                }

            var db = CreateDbConnection();
            var result = db.GetSbpObservationRegister(observationTypeId);
            return Ok(result);
            }

        [HttpGet]
        public IActionResult GetSbpObservationTypes()
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            var db = CreateDbConnection();
            var result = db.GetSbpObservationTypes();
            return Ok(result);
            }

        [HttpGet]
        public IActionResult GetSbpObservationHistory(int paraId)
            {
            var accessResult = EnsureSbpAccess();
            if (accessResult != null)
                {
                return accessResult;
                }

            var db = CreateDbConnection();
            var result = db.GetSbpObservationHistory(paraId);
            return Ok(result);
            }

        [HttpPost]
        public IActionResult Authenticate([FromBody] string password)
            {
            try
                {
                if (string.IsNullOrWhiteSpace(password))
                    {
                    return BadRequest(new { success = false, message = "Password is required." });
                    }

                var db = CreateDbConnection();
                var result = db.ValidateSbpAccessPassword(password);

                if (result == null || !result.Success)
                    {
                    return Unauthorized(new
                        {
                        success = false,
                        message = string.IsNullOrWhiteSpace(result?.Message)
                            ? "Invalid password. Access denied."
                            : result.Message
                        });
                    }

                try
                    {
                    sessionHandler.GrantSbpAccess();
                    }
                catch (Exception ex)
                    {
                    _logger.LogError(ex, "Unable to persist SBP authentication state.");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Session could not be updated." });
                    }

                return Ok(new { success = true, message = "Authentication successful." });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error while authenticating SBP access password.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while validating the password." });
                }
            }

        [HttpPost]
        public IActionResult UpdateSbpObservationPassword([FromBody] SbpPasswordUpdateRequest request)
            {
            if (request == null)
                {
                return BadRequest(new { Status = false, Message = "Request payload is required." });
                }

            var newPassword = request.NewPassword?.Trim();
            if (string.IsNullOrWhiteSpace(newPassword))
                {
                return BadRequest(new { Status = false, Message = "New password is required." });
                }

            var validation = _passwordPolicyValidator.Validate(newPassword, sessionHandler.TryGetUser(out var user) ? user?.PPNumber : null);
            if (!validation.IsValid)
                {
                return BadRequest(new { Status = false, Message = validation.ErrorMessage });
                }

            try
                {
                var db = CreateDbConnection();
                var updatedBy = string.IsNullOrWhiteSpace(request.UpdatedBy)
                    ? User?.Identity?.Name
                    : request.UpdatedBy;
                var result = db.UpdateSbpPassword(newPassword, updatedBy);

                if (!result.Success)
                    {
                    var errorMessage = string.IsNullOrWhiteSpace(result.Message)
                        ? "Password update failed."
                        : result.Message;
                    return StatusCode(StatusCodes.Status400BadRequest, new { Status = false, Message = errorMessage });
                    }

                var successMessage = string.IsNullOrWhiteSpace(result.Message)
                    ? "Password updated successfully."
                    : result.Message;

                return Ok(new { Status = true, Message = successMessage });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error updating SBP observation password.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = false, Message = "Password update failed." });
                }
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_audit_zones()
            {
            try
                {
                var zones = dBConnection.GetAuditZones();
                return Ok(zones ?? new List<AuditZoneItem>());
                }
            catch (OracleException ex)
                {
                _logger.LogError(ex, "Error retrieving audit zones.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch audit zones.");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unexpected error retrieving audit zones.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch audit zones.");
                }
            }










        [HttpPost]
        public List<AuditChecklistSubModel> sub_checklist(int T_ID, int ENG_ID)
            {
            return dBConnection.GetAuditChecklistSub(T_ID, ENG_ID);
            }
        [HttpPost]
        public List<AuditChecklistDetailsModel> checklist_details(int S_ID)
            {
            return dBConnection.GetAuditChecklistDetails(S_ID);
            }

        [HttpPost]
        [Consumes("application/json")]
        [ApplicationAudit("OBSERVATION_CREATED", "AUDIT_EXECUTION", "Execution", "pkg_ar", "P_SaveAuditObservation", EngagementId = "request.ENG_ID", ObjectType = "OBSERVATION", ObjectIdItem = "ApplicationAudit.ObjectIds", RequireNonEmpty = "request.LIST_OBS")]
        public IActionResult save_observations([FromBody] SaveObservationRequest request)
            {
            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            var isFinalSubmission = request?.IS_FINAL == true;
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Ok(new
                    {
                    Status = false,
                    Message = "Invalid session."
                    });
                }
            var isSpecialEntity = loggedInUser.UserEntityID == 112242 || loggedInUser.UserEntityID == 112248;
            var vCatId = request?.V_CAT_ID;
            var vCatNatureId = request?.V_CAT_NATURE_ID;

            if (!isSpecialEntity)
                {
                vCatId ??= 0;
                vCatNatureId ??= 0;
                }

            if (request?.LIST_OBS == null || request.LIST_OBS.Count == 0)
                {
                if (isFinalSubmission)
                    {
                    return BadRequest(new
                        {
                        Status = false,
                        Message = "No observations supplied."
                        });
                    }

                return Ok(new
                    {
                    Status = true,
                    Message = "Draft saved."
                    });
                }

            if (isFinalSubmission && (!request.ENG_ID.HasValue || request.ENG_ID <= 0))
                {
                return BadRequest(new { Status = false, Message = "Engagement is required for final submission." });
                }

            string responses = "";
            var allSucceeded = true;
            var createdObservationIds = new List<int>();

            foreach (var m in request.LIST_OBS)
                {
                if (isFinalSubmission && (string.IsNullOrWhiteSpace(m.ID) || m.DAYS == null || m.RISK == null))
                    {
                    return BadRequest(new { Status = false, Message = "Observation entry is missing required fields." });
                    }

                if (string.IsNullOrWhiteSpace(m.ID))
                    {
                    continue;
                    }

                var checklistDetailId = 0;
                var checklistParts = m.ID.Split("obs_");
                if (checklistParts.Length > 1)
                    {
                    int.TryParse(checklistParts[1], out checklistDetailId);
                    }

                if (isFinalSubmission && checklistDetailId <= 0)
                    {
                    return BadRequest(new { Status = false, Message = "Observation checklist detail is required." });
                    }

                if (!IsValidObservationHeading(m.HEADING))
                    {
                    return InvalidObservationHeadingResponse();
                    }

                var observationHeading = m.HEADING.Trim();

                var subChecklistId = request.S_ID;
                var annexureId = m.ANNEXURE_ID;
                var noOfInstances = m.NO_OF_INSTANCES;
                var responsiblePpno = m.RESPONSIBLE_PPNO;
                var amountInvolved = m.AMOUNT_INVOLVED;

                if (isSpecialEntity)
                    {
                    subChecklistId ??= 0;
                    annexureId = string.IsNullOrWhiteSpace(annexureId) ? "0" : annexureId;
                    checklistDetailId = checklistDetailId <= 0 ? 0 : checklistDetailId;
                    noOfInstances = string.IsNullOrWhiteSpace(noOfInstances) ? "0" : noOfInstances;
                    amountInvolved = string.IsNullOrWhiteSpace(amountInvolved) ? "0" : amountInvolved;
                    responsiblePpno ??= new List<ObservationResponsiblePPNOModel>();
                    }

                var ob = new ObservationModel
                    {
                    HEADING = observationHeading,
                    SUBCHECKLIST_ID = subChecklistId.GetValueOrDefault(),
                    ANNEXURE_ID = annexureId,
                    CHECKLISTDETAIL_ID = checklistDetailId,
                    V_CAT_ID = vCatId.GetValueOrDefault(),
                    V_CAT_NATURE_ID = vCatNatureId.GetValueOrDefault(),
                    ENGPLANID = request.ENG_ID.GetValueOrDefault(),
                    REPLYDATE = DateTime.Today.AddDays(m.DAYS.GetValueOrDefault()),
                    OBSERVATION_TEXT = m.MEMO,
                    SEVERITY = m.RISK.GetValueOrDefault(),
                    NO_OF_INSTANCES = noOfInstances,
                    OTHER_ENTITY_ID = request.OTHER_ENTITY_ID,
                    RESPONSIBLE_PPNO = responsiblePpno,
                    AMOUNT_INVOLVED = amountInvolved,
                    REFERENCE_ID = m.REFERENCE_ID ?? request.REFERENCE_ID,
                    STATUS = 1
                    };

                var saveResponse = dBConnection.SaveAuditObservation(ob, out var createdObservationId);
                allSucceeded &= !string.IsNullOrWhiteSpace(saveResponse) && createdObservationId > 0;
                if (createdObservationId > 0) createdObservationIds.Add(createdObservationId);
                responses += saveResponse;
                }

            if (!allSucceeded)
                return BadRequest(new { Status = false, Message = "One or more observations could not be saved." });
            HttpContext.Items["ApplicationAudit.ObjectIds"] = string.Join(',', createdObservationIds);
            return Ok(new
                {
                Status = true,
                Message = responses
                });
            }

        [HttpPost]
        [Consumes("application/json")]
        [ApplicationAudit("OBSERVATION_CREATED", "AUDIT_EXECUTION", "Execution", "pkg_ar", "P_SaveAuditObservationCAD", EngagementId = "request.ENG_ID", ObjectType = "OBSERVATION", ObjectIdItem = "ApplicationAudit.ObjectIds", RequireNonEmpty = "request.LIST_OBS")]
        public IActionResult save_observations_cau(
     [FromBody] SaveObservationCauRequest request)
            {
            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            var isFinalSubmission = request?.IS_FINAL == true;

            if (request?.LIST_OBS == null || request.LIST_OBS.Count == 0)
                {
                if (isFinalSubmission)
                    {
                    return BadRequest(new
                        {
                        Status = false,
                        Message = "No observations supplied."
                        });
                    }

                return Ok(new
                    {
                    Status = true,
                    Message = "Draft saved."
                    });
                }

            string responses = "";
            var allSucceeded = true;
            var createdObservationIds = new List<int>();

            foreach (var m in request.LIST_OBS)
                {
                if (isFinalSubmission && (m.DAYS == null || m.RISK == null))
                    {
                    return BadRequest(new { Status = false, Message = "Observation entry is missing required fields." });
                    }

                var checklistDetailId = request.CHECKLIST_ID.GetValueOrDefault();
                if (isFinalSubmission && checklistDetailId <= 0)
                    {
                    return BadRequest(new { Status = false, Message = "Checklist is required for final submission." });
                    }

                if (!IsValidObservationHeading(m.HEADING))
                    {
                    return InvalidObservationHeadingResponse();
                    }

                var observationHeading = m.HEADING.Trim();

                var ob = new ObservationModel
                    {
                    SUBCHECKLIST_ID = request.SUB_CHECKLISTID.GetValueOrDefault(),
                    CHECKLISTDETAIL_ID = checklistDetailId,
                    ANNEXURE_ID = request.ANNEXURE_ID,
                    ENGPLANID = request.ENG_ID.GetValueOrDefault(),
                    REPLYDATE = DateTime.Today.AddDays(m.DAYS.GetValueOrDefault()),
                    OBSERVATION_TEXT = m.MEMO,
                    HEADING = observationHeading,
                    SEVERITY = m.RISK.GetValueOrDefault(),
                    BRANCH_ID = request.BRANCH_ID.GetValueOrDefault(),
                    AMOUNT_INVOLVED = m.AMOUNT_INVOLVED,
                    NO_OF_INSTANCES = m.NO_OF_INSTANCES,
                    RESPONSIBLE_PPNO = m.RESPONSIBLE_PPNO,
                    REFERENCE_ID = m.REFERENCE_ID ?? request.REFERENCE_ID,
                    STATUS = 1
                    };

                var saveResponse = dBConnection.SaveAuditObservationCAU(ob, out var createdObservationId);
                allSucceeded &= !string.IsNullOrWhiteSpace(saveResponse) && createdObservationId > 0;
                if (createdObservationId > 0) createdObservationIds.Add(createdObservationId);
                responses += saveResponse;
                }

            if (!allSucceeded)
                return BadRequest(new { Status = false, Message = "One or more observations could not be saved." });
            HttpContext.Items["ApplicationAudit.ObjectIds"] = string.Join(',', createdObservationIds);
            return Ok(new
                {
                Status = true,
                Message = responses
                });
            }

        [HttpPost]
        public async Task<bool> reply_observation([FromForm] ObservationResponseModel or, [FromForm] string SUBFOLDER)
            {
            return await dBConnection.ResponseAuditObservation(or, SUBFOLDER);
            }
        [HttpPost]
        [ApplicationAudit("OBSERVATION_UPDATED", "AUDIT_EXECUTION", "Execution", "pkg_ar", "P_UpdateObservation", ObjectType = "OBSERVATION", ObjectId = "OBS_ID", RequireResultMessage = true)]
        public string update_observation_text(int OBS_ID, string OBS_TEXT, int PROCESS_ID = 0, int SUBPROCESS_ID = 0, int CHECKLIST_ID = 0, string OBS_TITLE = "", int RISK_ID = 0, int ANNEXURE_ID = 0, long? REFERENCE_ID = null)
            {
            if (!IsValidObservationHeading(OBS_TITLE))
                {
                _logger.LogWarning("Observation heading validation failed for update_observation_text. OBS_ID: {ObsId}", OBS_ID);
                return System.Text.Json.JsonSerializer.Serialize(new
                    {
                    Status = false,
                    Message = ObservationHeadingValidationMessage
                    });
                }

            string response = "";
            response = dBConnection.UpdateAuditObservationText(OBS_ID, OBS_TEXT, PROCESS_ID, SUBPROCESS_ID, CHECKLIST_ID, OBS_TITLE.Trim(), RISK_ID, ANNEXURE_ID, REFERENCE_ID);
            return System.Text.Json.JsonSerializer.Serialize(new { Status = !string.IsNullOrWhiteSpace(response), Message = response });
            }
        [HttpPost]
        [ApplicationAudit("OBSERVATION_STATUS_CHANGED", "AUDIT_EXECUTION", "Execution", "pkg_ar", "P_UpdateAuditObservationStatus", ObjectType = "OBSERVATION", ObjectId = "request.OBS_ID")]
        public IActionResult update_observation_status(UpdateObservationStatusRequest request)
            {
            if (request.NEW_STATUS_ID != 4)
                return BadRequest(new { Status = false, Message = "This legacy endpoint only accepts status 4." });
            if (request.RISK_ID != 3)
                {
                return Ok(new { Status = false, Message = "Only Low Risk para can be settled by Team Lead" });
                }

            var response = dBConnection.UpdateAuditObservationStatus(request.OBS_ID, 4, null, request.AUDITOR_COMMENT);
            return Ok(new { Status = !string.IsNullOrWhiteSpace(response), Message = response ?? string.Empty });

            }        [HttpPost]
        [ApplicationAudit("OBSERVATION_ADDED_TO_DRAFT_REPORT", "AUDIT_REPORT", "Reporting", "pkg_ar", "P_Add_Observation_To_Draft", ObjectType = "OBSERVATION", ObjectId = "request.ObservationId")]
        public IActionResult AddObservationToDraft(AddObservationToDraftRequest request)
            {
            if (request.ObservationId <= 0)
                return BadRequest(new { Status = false, Message = "Observation is required." });
            if (string.IsNullOrWhiteSpace(request.DraftParaNumber))
                return BadRequest(new { Status = false, Message = "Draft Para Number is required." });

            var result = dBConnection.AddObservationToDraft(
                request.ObservationId,
                request.DraftParaNumber.Trim(),
                request.Remarks);
            return result.Success
                ? Ok(new { Status = true, Message = result.Remarks })
                : BadRequest(new { Status = false, Message = result.Remarks });
            }

        [HttpPost]
        [ApplicationAudit("OBSERVATION_FINALIZED_OR_SETTLED", "AUDIT_EXECUTION", "Execution", "pkg_ar", "P_Finalize_Or_Settle_Observation", ObjectType = "OBSERVATION", ObjectId = "request.ObservationId")]
        public IActionResult FinalizeOrSettleObservation(FinalizeOrSettleObservationRequest request)
            {
            if (request.ObservationId <= 0)
                return BadRequest(new { Status = false, Message = "Observation is required." });
            if (request.NewStatusId != 8 && request.NewStatusId != 9)
                return BadRequest(new { Status = false, Message = "Only status 8 or 9 is supported." });
            if (request.NewStatusId == 8
                && (!request.FinalParaNumber.HasValue || request.FinalParaNumber.Value <= 0))
                return BadRequest(new { Status = false, Message = "Final Para Number is required and must be greater than zero." });

            var user = sessionHandler.GetUser();
            if (user == null || (user.UserRoleID != 6 && user.UserRoleID != 7 && user.UserRoleID != 15))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { Status = false, Message = "Only Departmental Head is authorized to update this observation status." });

            var finalParaNumber = request.NewStatusId == 9 ? null : request.FinalParaNumber;
            var result = dBConnection.FinalizeOrSettleObservation(
                request.ObservationId,
                request.NewStatusId,
                finalParaNumber,
                request.Remarks);
            return result.Success
                ? Ok(new { Status = true, Message = result.Remarks })
                : BadRequest(new { Status = false, Message = result.Remarks });
            }

        [HttpPost]
        public IActionResult get_memo_draft_para_update_observations(int ENG_ID)
            {
            if (ENG_ID <= 0)
                return BadRequest(new { Status = false, Message = "Engagement is required." });

            var user = sessionHandler.GetUser();
            if (user == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { Status = false, Message = "User session is not available." });

            var engagement = dBConnection.GetArDashboardDropdownOptions(ENG_ID)
                .FirstOrDefault(item => item.EngagementId == ENG_ID);
            if (engagement == null)
                return StatusCode(StatusCodes.Status403Forbidden, new { Status = false, Message = "Selected engagement is not accessible." });
            if (!string.Equals((engagement.IsTeamLead ?? string.Empty).Trim(), "Y", StringComparison.OrdinalIgnoreCase))
                return StatusCode(StatusCodes.Status403Forbidden, new { Status = false, Message = "Only the assigned Team Lead can update Memo and Draft Para numbers." });

            return Ok(dBConnection.GetMemoDraftParaUpdateObservations(ENG_ID));
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ApplicationAudit("DRAFT_PARA_NUMBER_CHANGED", "AUDIT_REPORT", "Reporting", "pkg_ar", "P_Update_Memo_Draft_Para_No", EngagementId = "request.EngagementId", ObjectType = "OBSERVATION", ObjectId = "request.ObservationId")]
        public IActionResult update_memo_draft_para_no(UpdateMemoDraftParaRequest request)
            {
            if (!ModelState.IsValid)
                return BadRequest(new { Status = false, Message = "Memo Number and Draft Para Number are required and must contain digits only." });
            if (request.EngagementId <= 0 || request.ObservationId <= 0)
                return BadRequest(new { Status = false, Message = "Engagement and observation are required." });

            var result = dBConnection.UpdateMemoDraftParaNo(
                request.EngagementId,
                request.ObservationId,
                request.MemoNumber,
                request.DraftParaNumber);

            return result.Success
                ? Ok(new { Status = true, Message = result.Remarks })
                : BadRequest(new { Status = false, Message = result.Remarks });
            }

        [HttpPost]
        [ApplicationAudit("OBSERVATION_DROPPED", "AUDIT_EXECUTION", "Execution", "pkg_ar", "P_DropAuditObservation", ObjectType = "OBSERVATION", ObjectId = "OBS_ID")]
        public IActionResult drop_observation(int OBS_ID)
            {
            var response = dBConnection.DropAuditObservation(OBS_ID);
            return !string.IsNullOrWhiteSpace(response)
                ? Ok(new { Status = true, Message = response })
                : BadRequest(new { Status = false, Message = "Observation could not be dropped." });

            }
        [HttpPost]
        [ApplicationAudit("OBSERVATION_SUBMITTED_TO_AUDITEE", "AUDIT_EXECUTION", "Execution", "pkg_ar", "P_SubmitAuditObservationToAuditee", ObjectType = "OBSERVATION", ObjectId = "OBS_ID")]
        public async Task<IActionResult> submit_observation_to_auditee(int OBS_ID)
            {
            string response = "";
            response = dBConnection.SubmitAuditObservationToAuditee(OBS_ID);
            if (dBConnection.IsObservationSubmittedToAuditee(OBS_ID))
                {
                var notificationData = dBConnection.GetObservationSubmittedNotificationData(OBS_ID);
                await EmailNotification.SendObservationSubmittedToAuditeeAsync(_configuration, notificationData, HttpContext?.RequestServices);
                }

            return !string.IsNullOrWhiteSpace(response)
                ? Ok(new { Status = true, Message = response })
                : BadRequest(new { Status = false, Message = "Observation was not submitted." });

            }
        [HttpGet]
        [HttpPost]
        public List<ManageAuditParasModel> get_observations_for_manage_paras(int ENTITY_ID = 0, int OBS_ID = 0)
            {
            return dBConnection.GetObservationsForManageAuditParas(ENTITY_ID, OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public viewMemoModel get_observations_details_for_manage_paras(int COM_ID)
            {
            return dBConnection.GetObservationDetailsForManageAuditParas(COM_ID);
            }
        [HttpGet]
        public List<ObservationResponsiblePPNOModel> GetResponsiblePPNOforoldPara(int COM_ID)
            {
            return dBConnection.GetResponsiblePPNOforoldPara(COM_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<ObservationResponsiblePPNOModel> get_responsibility_for_authorize(int COM_ID)
            {
            return dBConnection.GetResponsibilityForAuthorize(COM_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<ManageAuditParasModel> get_observations_for_manage_paras_auth()
            {
            return dBConnection.GetObservationsForMangeAuditParasForAuthorization();
            }
        [HttpGet]
        [HttpPost]
        public List<ManageAuditParasModel> get_proposed_changes_in_manage_paras_auth(int COM_ID)
            {
            return dBConnection.GetProposedChangesInManageParasAuth(COM_ID);
            }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [ApplicationAudit("MANAGED_AUDIT_PARA_UPDATED", "COMPLIANCE", "PARA MANAGEMENT", "PKG_AR", "P_UPDATEAUDITOBSERVATIONSTATUS", ParaId = "req.NEW_PARA_ID", OldParaId = "req.OLD_PARA_ID", NewParaId = "req.NEW_PARA_ID", ComId = "req.COM_ID", ObjectType = "PARA", ObjectId = "req.COM_ID", RequireResultMessage = true)]
        public IActionResult update_para_for_manage_audit_paras(
            [FromBody] UpdateAuditParaRequest req)
            {
            if (!ModelState.IsValid)
                return InvalidModelStateResponse();
            var model = new ManageAuditParasModel
                {
                COM_ID = NumericParsing.ToIntOrDefault(req.COM_ID),
                OLD_PARA_ID = NumericParsing.ToIntOrDefault(req.OLD_PARA_ID),
                NEW_PARA_ID = NumericParsing.ToIntOrDefault(req.NEW_PARA_ID),
                PARA_NO = req.PARA_NO,
                PARA_TEXT = req.PARA_TEXT,
                OBS_GIST = req.OBS_GIST,
                INDICATOR = req.INDICATOR,
                AUDIT_PERIOD = req.AUDIT_PERIOD,
                OBS_RISK_ID = req.OBS_RISK_ID ?? 0,
                ANNEX_ID = req.ANNEX_ID ?? 0,
                AMOUNT_INV = req.AMOUNT_INV,
                NO_INSTANCES = req.NO_INSTANCES,
                UPDATED_BY = User.Identity?.Name
                };

            var response = dBConnection.UpdateAuditObservationStatus(model);
            return Ok(new { Status = true, Message = response });
            }

        [HttpGet]
        [HttpPost]
        [ApplicationAudit("MANAGED_AUDIT_PARA_REFERRED_BACK", "COMPLIANCE", "PARA MANAGEMENT", "PKG_AR", "P_AUTHORIZE_UPDATE_AUDIT_PARAS", ParaId = "pm.NEW_PARA_ID", OldParaId = "pm.OLD_PARA_ID", NewParaId = "pm.NEW_PARA_ID", ComId = "pm.COM_ID", ObjectType = "PARA", ObjectId = "pm.COM_ID", RequireResultMessage = true)]
        public string referredback_para_for_manage_audit_paras(ManageAuditParasModel pm)
            {
            string response = "";
            response = dBConnection.ReferredBackAuditObservationStatus(pm);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("MANAGED_AUDIT_PARA_AUTHORIZED", "COMPLIANCE", "PARA MANAGEMENT", "PKG_AR", "P_AUTHORIZE_UPDATE_AUDIT_PARAS", ParaId = "pm.NEW_PARA_ID", OldParaId = "pm.OLD_PARA_ID", NewParaId = "pm.NEW_PARA_ID", ComId = "pm.COM_ID", ObjectType = "PARA", ObjectId = "pm.COM_ID", RequireResultMessage = true)]
        public string authorize_para_for_manage_audit_paras(ManageAuditParasModel pm)
            {
            string response = "";
            response = dBConnection.AuthorizedAuditObservationStatus(pm);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_observation(int ENG_ID = 0, int OBS_ID = 0)
            {
            return dBConnection.GetManagedObservations(ENG_ID, OBS_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_dept_observation_text(int ENG_ID = 0, int OBS_ID = 0)
            {
            return dBConnection.GetManagedObservationText(ENG_ID, OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<AuditeeResponseEvidenceModel> get_responded_obs_evidences(int OBS_ID = 0)
            {
            return dBConnection.GetRespondedObservationEvidences(OBS_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<ObservationTextModel> get_details_for_manage_observations_text(int OBS_ID = 0, string INDICATOR = "")
            {
            return dBConnection.GetManagedAllObservationsText(OBS_ID, INDICATOR);
            }

        [HttpGet]
        [HttpPost]
        public List<SubCheckListStatus> get_subchecklist_status(int ENG_ID = 0, int S_ID = 0)
            {
            return dBConnection.GetSubChecklistStatus(ENG_ID, S_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_observation_branches(int ENG_ID = 0, int OBS_ID = 0)
            {
            return dBConnection.GetManagedObservationsForBranches(ENG_ID, OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public ObservationPdfDataModel get_observation_pdf_data(int OBS_ID = 0)
            {
            return dBConnection.GetObservationPdfData(OBS_ID);
            }
        [HttpPost]
        [ApplicationAudit("OBSERVATION_GIST_RECOMMENDATION_UPDATED", "AUDIT_EXECUTION", "OBSERVATION", "PKG_HD", "P_ADD_OBSERVATION_GIST_RECOMMENDATION", ObjectType = "OBSERVATION", ObjectId = "model.OBS_ID", RequireResultMessage = true)]
        public string add_observation_gist_and_recommendation(ObservationGistRecommendationModel model)
            {
            if (!ModelState.IsValid)
                {
                return "{\"Status\":false,\"Message\":\"Please correct highlighted fields and try again.\"}";
                }

            string response = "";
            response = dBConnection.AddObservationGistAndRecommendation(model.OBS_ID, model.GIST_OF_PARA, model.AUDITOR_RECOMMENDATION);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_observation_text_branches(int OBS_ID = 0)
            {
            return dBConnection.GetManagedObservationTextForBranches(OBS_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<ObservationResponsiblePPNOModel> get_observation_responsible_ppnos(int OBS_ID, int ENG_ID)
            {
            return dBConnection.GetObservationResponsiblePPNOs(OBS_ID, ENG_ID);
            }
        [HttpPost]
        public DraftReportSummaryModel draft_report_summary(int ENG_ID)
            {
            DraftReportSummaryModel resp = new DraftReportSummaryModel();
            string filename = "";
            // filename = dBConnection.CreateAuditReport(ENG_ID);
            resp = dBConnection.GetDraftReportSummary(ENG_ID);
            resp.ReportName = filename;
            return resp;
            }
        [HttpPost]
        public List<ClosingDraftTeamDetailsModel> closing_draft_report_status(int ENG_ID = 0)
            {
            return dBConnection.GetClosingDraftObservations(ENG_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<ClosingDraftTeamDetailsModel> get_team_details(int ENG_ID = 0)
            {
            return dBConnection.GetClosingDraftTeamDetails(ENG_ID);
            }
        [HttpPost]
        [ApplicationAudit("DRAFT_AUDIT_CLOSED", "AUDIT_EXECUTION", "DRAFT_AUDIT", "PKG_AR", "P_CLOSEAUDIT", EngagementId = "ENG_ID", ObjectType = "ENGAGEMENT", ObjectId = "ENG_ID", RequireResultMessage = true)]
        public object close_draft_audit(int ENG_ID)
            {
            string response = "";
            response = dBConnection.CloseDraftAuditReport(ENG_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        public object conclude_draft_audit(int ENG_ID)
            {
            string response = "";
            response = dBConnection.ConcludeDraftAuditReport(ENG_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        public List<LoanCaseModel> Loan_Case_Details(int Loan_case, string LOAN_TYPE = "", int ENG_ID = 0)
            {
            return dBConnection.GetLoanCaseDetails(Loan_case, LOAN_TYPE, ENG_ID);
            }
        [HttpPost]
        public GlHeadSubDetailsModel Glhead_Sub_Details(int GLTYPEID)
            {
            return dBConnection.GetGlheadSubDetails(GLTYPEID);
            }
        [HttpPost]
        public List<DepositAccountModel> GetDepositAccountSubdetails(string b_name)
            {
            return archiveDbConnection.GetDepositAccountSubdetails(b_name);
            }
        [HttpPost]
        public List<DepositAccountCatDetailsModel> GetDepositAccountcatdetails(int catid)
            {
            return archiveDbConnection.GetDepositAccountcatdetails(catid);
            }

        [HttpPost]
        public List<LoanCaseModel> GetBranchDesbursementaccountdetails(int b_id)
            {
            return dBConnection.GetBranchDesbursementAccountdetails(b_id);
            }

        [HttpPost]
        public List<GlHeadDetailsModel> GetIncomeExpenceDetails(int b_id, int ENG_ID)
            {
            return archiveDbConnection.GetIncomeExpenceDetails(b_id, ENG_ID);
            }
        [HttpPost]
        public int GetAuditEntitiesCount(int CRITERIA_ID)
            {
            return dBConnection.GetExpectedCountOfAuditEntitiesOnCriteria(CRITERIA_ID);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("DELETE_PENDING_CRITERIA", "ADMINISTRATION", "ADMINISTRATION", "PKG_PG", "P_DELETEPENDINGCRITERIA", ObjectType = "DELETEPENDINGCRITERIA")]
        public bool DeletePendingCriteria(int CID = 0)
            {
            return dBConnection.DeletePendingCriteria(CID);
            }
        [HttpPost]
        public List<AuditeeEntitiesModel> GetAuditeeEntitiesByTypeId(int ENTITY_TYPE_ID = 0)
            {
            return dBConnection.GetAuditeeEntitiesForUpdate(ENTITY_TYPE_ID);
            }
        [HttpPost]
        public AuditeeEntitiesModel GetAuditeeEntityById(int ENTITY_ID = 0)
            {
            return dBConnection.GetAuditeeEntitiesForUpdate(0, ENTITY_ID).FirstOrDefault();
            }
        [HttpPost]

        public AuditeeEntityUpdateModel GetAuditeeEntityByIdforAuthorization(int ENTITY_ID = 0)
            {
            return dBConnection.GetAuditeeEntitiesForUpdateForAuthorization(0, ENTITY_ID).FirstOrDefault();
            }
        [HttpPost]
        public List<AuditeeEntityUpdateModel> GetAuditeeEntitiesPendingAuthorization()
            {
            return dBConnection.GetAuditeeEntitiesForAuthorization();
            }

        [HttpPost]
        [ApplicationAudit("AUDITEE_ENTITY_UPDATED", "AUDIT_EXECUTION", "AUDIT_UNIVERSE", "PKG_AD", "P_UPDATE_ENTITIES", ObjectType = "AUDITEE_ENTITY", RequireResultMessage = true)]
        public string UpdateAuditeeEntity(AuditeeEntityUpdateModel ENTITY_MODEL, string IND)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditeeEntity(ENTITY_MODEL, IND) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("AUDIT_CRITERIA_SUBMITTED", "AUDIT_EXECUTION", "AUDIT_CRITERIA", "PKG_PG", "P_SUBMITAUDITCRITERIAFORAPPROVAL", ObjectType = "AUDIT_PERIOD", ObjectId = "PERIOD_ID")]
        public bool submit_audit_criterias(int PERIOD_ID)
            {
            return dBConnection.SubmitAuditCriteriaForApproval(PERIOD_ID);
            }
        [HttpPost]
        public List<COSORiskModel> GetCOSORiskForDepartment(int PERIOD_ID = 0)
            {
            return dBConnection.GetCOSORiskForDepartment(PERIOD_ID);
            }
        [HttpGet]
        [HttpPost]
        public IActionResult get_commercial_audit_oms()
            {
            return ExecuteCommercialAuditRequest(
                () => Json(dBConnection.GetCommercialAuditOms()),
                "Unable to load OM records right now.");
            }

        [HttpPost]
        [ApplicationAudit("COMMERCIAL_AUDIT_OM_SAVED", "AUDIT_EXECUTION", "COMMERCIAL_AUDIT", "PKG_COMMERCIAL_AUDIT", "P_SAVE_OM", ObjectType = "COMMERCIAL_AUDIT_OM", ObjectId = "model.OmId")]
        public IActionResult save_commercial_audit_om([FromBody] CommercialAuditOmModel model)
            {
            return ExecuteCommercialAuditRequest(() =>
                {
                if (model == null)
                    {
                    if (!ModelState.IsValid)
                        {
                        return InvalidModelStateResponse("Model binding failed");
                        }

                    return InvalidRequestResponse(
                        "request",
                        "Model binding failed. Ensure the request body is valid JSON matching CommercialAuditOmModel.",
                        "Model binding failed");
                    }

                var validationResult = ValidateCommercialAuditOm(model);
                if (validationResult != null)
                    {
                    return validationResult;
                    }

                return Json(dBConnection.SaveCommercialAuditOm(model));
                },
                "Unable to save OM right now.");
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_commercial_audit_pdps()
            {
            return ExecuteCommercialAuditRequest(
                () => Json(dBConnection.GetCommercialAuditPdps()),
                "Unable to load PDP records right now.");
            }

        [HttpPost]
        [ApplicationAudit("COMMERCIAL_AUDIT_PDP_SAVED", "AUDIT_EXECUTION", "COMMERCIAL_AUDIT", "PKG_COMMERCIAL_AUDIT", "P_SAVE_PDP", ObjectType = "COMMERCIAL_AUDIT_PDP", ObjectId = "model.PdpId")]
        public IActionResult save_commercial_audit_pdp([FromBody] CommercialAuditPdpModel model)
            {
            return ExecuteCommercialAuditRequest(() =>
                {
                var validationResult = ValidateCommercialAuditPdp(model);
                if (validationResult != null)
                    {
                    return validationResult;
                    }

                return Json(dBConnection.SaveCommercialAuditPdp(model));
                },
                "Unable to save PDP right now.");
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_commercial_audit_pdp_om_mappings(int pdp_id)
            {
            return ExecuteCommercialAuditRequest(
                () => Json(dBConnection.GetCommercialAuditPdpMappings(pdp_id)),
                "Unable to load PDP to OM mappings right now.");
            }

        [HttpPost]
        [ApplicationAudit("COMMERCIAL_AUDIT_PDP_OM_MAPPING_SAVED", "AUDIT_EXECUTION", "COMMERCIAL_AUDIT", "PKG_COMMERCIAL_AUDIT", "P_SAVE_PDP_OM_MAP", ObjectType = "COMMERCIAL_AUDIT_PDP", ObjectId = "model.PdpId")]
        public IActionResult save_commercial_audit_pdp_om_mapping([FromBody] CommercialAuditPdpOmMappingSaveRequest model)
            {
            return ExecuteCommercialAuditRequest(() =>
                {
                var validationResult = ValidateCommercialAuditPdpMapping(model);
                if (validationResult != null)
                    {
                    return validationResult;
                    }

                return Json(dBConnection.SaveCommercialAuditPdpMappings(model));
                },
                "Unable to save PDP mappings right now.");
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_commercial_audit_arpse_headers()
            {
            return ExecuteCommercialAuditRequest(
                () => Json(dBConnection.GetCommercialAuditArpseHeaders()),
                "Unable to load ARPSE headers right now.");
            }

        [HttpPost]
        [ApplicationAudit("COMMERCIAL_AUDIT_ARPSE_SAVED", "AUDIT_EXECUTION", "COMMERCIAL_AUDIT", "PKG_COMMERCIAL_AUDIT", "P_SAVE_ARPSE", ObjectType = "COMMERCIAL_AUDIT_ARPSE", ObjectId = "model.ArpseId")]
        public IActionResult save_commercial_audit_arpse_header([FromBody] CommercialAuditArpseHeaderModel model)
            {
            return ExecuteCommercialAuditRequest(() =>
                {
                var validationResult = ValidateCommercialAuditArpseHeader(model);
                if (validationResult != null)
                    {
                    return validationResult;
                    }

                return Json(dBConnection.SaveCommercialAuditArpseHeader(model));
                },
                "Unable to save ARPSE header right now.");
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_commercial_audit_arpse_pdp_mappings(int arpse_id)
            {
            return ExecuteCommercialAuditRequest(
                () => Json(dBConnection.GetCommercialAuditArpsePdpMappings(arpse_id)),
                "Unable to load ARPSE to PDP mappings right now.");
            }

        [HttpPost]
        [ApplicationAudit("COMMERCIAL_AUDIT_ARPSE_PDP_MAPPING_SAVED", "AUDIT_EXECUTION", "COMMERCIAL_AUDIT", "PKG_COMMERCIAL_AUDIT", "P_SAVE_ARPSE_PDP_MAP", ObjectType = "COMMERCIAL_AUDIT_ARPSE", ObjectId = "model.ArpseId")]
        public IActionResult save_commercial_audit_arpse_pdp_mapping([FromBody] CommercialAuditArpsePdpMappingSaveRequest model)
            {
            return ExecuteCommercialAuditRequest(() =>
                {
                var validationResult = ValidateCommercialAuditArpsePdpMapping(model);
                if (validationResult != null)
                    {
                    return validationResult;
                    }

                return Json(dBConnection.SaveCommercialAuditArpsePdpMappings(model));
                },
                "Unable to save ARPSE PDP mappings right now.");
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_commercial_audit_arpse_dac_entries(int arpse_id)
            {
            return ExecuteCommercialAuditRequest(
                () => Json(dBConnection.GetCommercialAuditArpseDacEntries(arpse_id)),
                "Unable to load ARPSE DAC entries right now.");
            }

        [HttpPost]
        [ApplicationAudit("COMMERCIAL_AUDIT_ARPSE_DAC_SAVED", "AUDIT_EXECUTION", "COMMERCIAL_AUDIT", "PKG_COMMERCIAL_AUDIT", "P_SAVE_ARPSE_DAC", ObjectType = "COMMERCIAL_AUDIT_ARPSE", ObjectId = "model.ArpseId")]
        public IActionResult save_commercial_audit_arpse_dac_entry([FromBody] CommercialAuditArpseDacEntryModel model)
            {
            return ExecuteCommercialAuditRequest(() =>
                {
                var validationResult = ValidateCommercialAuditArpseDacEntry(model);
                if (validationResult != null)
                    {
                    return validationResult;
                    }

                return Json(dBConnection.SaveCommercialAuditArpseDacEntry(model));
                },
                "Unable to save ARPSE DAC entry right now.");
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_commercial_audit_arpse_pac_entries(int arpse_id)
            {
            return ExecuteCommercialAuditRequest(
                () => Json(dBConnection.GetCommercialAuditArpsePacEntries(arpse_id)),
                "Unable to load ARPSE PAC entries right now.");
            }

        [HttpPost]
        [ApplicationAudit("COMMERCIAL_AUDIT_ARPSE_PAC_SAVED", "AUDIT_EXECUTION", "COMMERCIAL_AUDIT", "PKG_COMMERCIAL_AUDIT", "P_SAVE_ARPSE_PAC", ObjectType = "COMMERCIAL_AUDIT_ARPSE", ObjectId = "model.ArpseId")]
        public IActionResult save_commercial_audit_arpse_pac_entry([FromBody] CommercialAuditArpsePacEntryModel model)
            {
            return ExecuteCommercialAuditRequest(() =>
                {
                var validationResult = ValidateCommercialAuditArpsePacEntry(model);
                if (validationResult != null)
                    {
                    return validationResult;
                    }

                return Json(dBConnection.SaveCommercialAuditArpsePacEntry(model));
                },
                "Unable to save ARPSE PAC entry right now.");
            }
        [HttpGet]
        [HttpPost]
        public List<ObservationSummaryModel> get_observations_summary_for_selected_entity(int ENG_ID)
            {
            return dBConnection.GetManagedObservationsSummaryForSelectedEntity(ENG_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<ObservationRevisedModel> get_observations_for_selected_entity(int ENG_ID)
            {
            return dBConnection.GetManagedObservationsForSelectedEntity(ENG_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_observations(int ENG_ID, int OBS_ID = 0)
            {
            return dBConnection.GetManagedObservations(ENG_ID, OBS_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_observations_draft(int ENG_ID, int OBS_ID = 0)
            {
            return dBConnection.GetManagedDraftObservations(ENG_ID, OBS_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_finalized_observations_draft(int ENG_ID, int OBS_ID = 0)
            {
            return dBConnection.GetFinalizedDraftObservations(ENG_ID, OBS_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_finalized_observations_draft_branch(int ENG_ID, int OBS_ID = 0)
            {
            return dBConnection.GetFinalizedDraftObservationsBranch(ENG_ID, OBS_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_observations_draft_branch(int ENG_ID, int OBS_ID = 0)
            {
            return dBConnection.GetManagedDraftObservationsBranch(ENG_ID, OBS_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_observations_draft_text(int ENG_ID, int OBS_ID = 0)
            {
            return dBConnection.GetManagedDraftObservationsText(ENG_ID, OBS_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_observations_draft_auditee_reply(int ENG_ID, int OBS_ID = 0)
            {
            List<ManageObservations> resp = new List<ManageObservations>();
            ManageObservations m = new ManageObservations();
            m.OBS_ID = OBS_ID;
            m.OBS_REPLY = dBConnection.GetLatestAuditeeResponse(OBS_ID);
            resp.Add(m);
            return resp;
            }

        [HttpGet]
        [HttpPost]
        public List<AssignedObservations> get_assigned_observation(int ENG_ID)
            {
            return dBConnection.GetAssignedObservations(ENG_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<AuditCCQModel> get_ccqs(int ENTITY_ID)
            {
            return dBConnection.GetCCQ(ENTITY_ID);

            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_CCQ", "ADMINISTRATION", "ADMINISTRATION", "PKG_PG", "P_UPDATECCQ", ObjectType = "UPDATE_CCQ")]
        public bool update_ccq(AuditCCQModel ccq)
            {
            return dBConnection.UpdateCCQ(ccq);

            }
        [HttpGet]
        [HttpPost]
        public List<object> get_observation_text(int OBS_ID, int RESP_ID)
            {
            return dBConnection.GetObservationText(OBS_ID, RESP_ID);

            }

        [HttpPost]
        public bool old_para_response(AuditeeOldParasResponseModel ob)
            {
            return dBConnection.AuditeeOldParaResponse(ob);
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_legacy_para(string AUDITED_BY, string AUDIT_YEAR)
            {
            return dBConnection.GetOldParas(AUDITED_BY, AUDIT_YEAR);
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModelCAD> get_old_para_management()
            {
            return archiveDbConnection.GetOldParasManagement();
            }

        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_legacy_settled_paras(int ENTITY_ID = 0)
            {
            return dBConnection.GetOldSettledParasForResponse(ENTITY_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<ParaStatusChangeModel> get_paras_for_status_change(int ENTITY_ID = 0)
            {
            return archiveDbConnection.GetParasForStatusChange(ENTITY_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_current_paras_for_status_change_request(int ENTITY_ID = 0)
            {
            return dBConnection.GetCurrentParasForStatusChangeRequest(ENTITY_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_current_paras_for_status_change_request_review()
            {
            return dBConnection.GetCurrentParasForStatusChangeRequestReview();
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_current_paras_for_status_change_request_authorize()
            {
            return archiveDbConnection.GetCurrentParasForStatusChangeRequestAuthorize();
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_manage_legacy_para()
            {
            return dBConnection.GetManageLegacyParas();
            }
        [HttpGet]
        [HttpPost]
        public List<AuditeeOldParasModel> get_outstanding_para(string ENTITY_ID)
            {
            return dBConnection.GetOutstandingParas(ENTITY_ID);
            }
        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_OBSERVATION_ADDED", "COMPLIANCE", "LEGACY PARA", "PKG_HD", "P_ADDOLDPARAS", ParaId = "ob.ID", ObjectType = "LEGACY_PARA", ObjectId = "ob.ID")]
        public bool add_legacy_para_observation_text(OldParasModel ob)
            {
            return dBConnection.AddOldParas(ob);
            }
        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_REPLY_ADDED", "COMPLIANCE", "LEGACY PARA", "PKG_AE", "P_ADDOLDPARASREPLY", ParaId = "ID", ObjectType = "LEGACY_PARA", ObjectId = "ID")]
        public bool add_legacy_para_reply(int ID, string REPLY)
            {
            return dBConnection.AddOldParasReply(ID, REPLY);
            }
        [HttpPost]
        public bool set_manage_legacy_para_status(int ID, int NEW_STATUS)
            {
            return dBConnection.UpdateOldParasStatus(ID, NEW_STATUS);
            }
        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_CAD_REPLY_ADDED", "COMPLIANCE", "LEGACY PARA", "PKG_AE", "P_UPDATEOLDPARAMANAGEMENT", ParaId = "ID", ObjectType = "LEGACY_PARA", ObjectId = "ID", RequireResultMessage = true)]
        public string add_legacy_para_cad_reply(int ID, int V_CAT_ID, int V_CAT_NATURE_ID, int RISK_ID, string REPLY)
            {
            string response = "";
            response = archiveDbConnection.AddOldParasCADReply(ID, V_CAT_ID, V_CAT_NATURE_ID, RISK_ID, REPLY);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";

            }
        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_CAD_COMPLIANCE_ADDED", "COMPLIANCE", "LEGACY PARA", "PKG_AE", "P_UPDATEAUDITEEOLDPARASRESPONSE", ObjectType = "LEGACY_PARA_COMPLIANCE", RequireNonEmpty = "COMPLIANCE_LIST", RequireResultMessage = true)]
        public string add_legacy_para_cad_compliance(List<OldParaComplianceModel> COMPLIANCE_LIST)
            {
            string response = "";
            foreach (OldParaComplianceModel opc in COMPLIANCE_LIST)
                {
                response += archiveDbConnection.AddOldParasCADCompliance(opc) + "\n";
                }

            return "{\"Status\":true,\"Message\":\"" + response + "\"}";

            }

        [HttpPost]
        public List<UserRelationshipModel> getparentrel(int ENTITY_REALTION_ID)
            {
            return dBConnection.Getparentrepoffice(ENTITY_REALTION_ID);
            }

        [HttpPost]
        public List<UserRelationshipModel> getparentrelForDashboardPanel(int ENTITY_REALTION_ID)
            {
            return dBConnection.GetparentrepofficeForDashboardPanel(ENTITY_REALTION_ID);
            }
        [HttpPost]
        public List<UserRelationshipModel> getparentrelForParaPositionReport(int ENTITY_REALTION_ID)
            {
            return dBConnection.GetparentrepofficeForParaPositionReport(ENTITY_REALTION_ID);
            }

        [HttpPost]
        public List<UserRelationshipModel> getpostplace(int E_R_ID)
            {
            return dBConnection.Getchildposting(E_R_ID);
            }
        [HttpPost]
        public List<UserRelationshipModel> getpostplaceForDashboardPanel(int E_R_ID)
            {
            return dBConnection.GetchildpostingForDashboardPanel(E_R_ID);
            }
        [HttpPost]
        public List<UserRelationshipModel> getpostplaceForParaPositionReport(int E_R_ID)
            {
            return dBConnection.GetchildpostingForParaPositionReport(E_R_ID);
            }
        [HttpPost]
        [ApplicationAudit("ENGAGEMENT_PLAN_APPROVED", "AUDIT_PLANNING", "ENGAGEMENT_PLANNING", "PKG_PG", "P_APPROVEAUDITENGAGEMENTPLAN", EngagementId = "ENG_ID", ObjectType = "ENGAGEMENT", ObjectId = "ENG_ID")]
        public bool approve_engagement_plan(int ENG_ID)
            {
            return dBConnection.ApproveAuditEngagementPlan(ENG_ID);
            }

        [HttpGet]
        [HttpPost]
        public UserModel get_matched_pp_numbers(string PPNO)
            {
            return dBConnection.GetMatchedPPNumbers(PPNO);
            }

        [HttpPost]
        [ApplicationAudit("ENGAGEMENT_PLAN_REFERRED_BACK", "AUDIT_PLANNING", "ENGAGEMENT_PLANNING", "PKG_PG", "P_REFFEREDBACKAUDITENGAGEMENTPLAN", EngagementId = "ENG_ID", ObjectType = "ENGAGEMENT", ObjectId = "ENG_ID")]
        public bool reject_engagement_plan(int ENG_ID, string COMMENTS)
            {
            return dBConnection.RefferedBackAuditEngagementPlan(ENG_ID, COMMENTS);
            }

        [HttpPost]
        public string rerecommend_engagement_plan(int ENG_ID, int PLAN_ID, int ENTITY_ID, DateTime OP_START_DATE, DateTime OP_END_DATE, DateTime START_DATE, DateTime END_DATE, int TEAM_ID, string COMMENTS)
            {
            string response = "";
            response = dBConnection.RerecommendAuditEngagementPlan(ENG_ID, PLAN_ID, ENTITY_ID, OP_START_DATE, OP_END_DATE, START_DATE, END_DATE, TEAM_ID, COMMENTS);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        public List<LoanCasedocModel> Getloancasedocuments(int ENG_ID)
            {
            return dBConnection.GetLoanCaseDocuments(ENG_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<FunctionalResponsibilityWiseParas> get_functional_responsibility_wise_paras(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, int PROCESS_DETAIL_ID = 0)
            {
            return dBConnection.GetFunctionalResponsibilityWisePara(PROCESS_ID, SUB_PROCESS_ID, PROCESS_DETAIL_ID);
            }
        [HttpPost]
        public bool divisional_head_remarks_on_functional_legacy_para(int CONCERNED_DEPT_ID = 0, string COMMENTS = "", int REF_PARA_ID = 0)
            {
            return dBConnection.AddDivisionalHeadRemarksOnFunctionalLegacyPara(CONCERNED_DEPT_ID, COMMENTS, REF_PARA_ID);
            }
        [HttpPost]
        public bool menu_pages_updation(int MENU_ID = 0, int[] PAGE_IDS = null)
            {
            if (PAGE_IDS != null)
                {
                foreach (var PAGE_ID in PAGE_IDS)
                    {
                    dBConnection.UpdateMenuPagesAssignment(MENU_ID, PAGE_ID);
                    }
                return true;
                }
            else
                return false;

            }

        [HttpPost]
        public bool addinpectioncriteria(string fquat = "", string squat = "", string tquat = "", string frquat = "")
            {
            return true;// dBConnection.AddInspectionCriteria(fquat, squat, tquat, frquat);
            }

        [HttpPost]

        public bool add_inspection_team(int teamid = 0, string tname = "", int pop = 0)
            {
            return true;// dBConnection.AddInspectionTeam(teamid, tname, pop);
            }

        [HttpPost]

        public bool Join_inspection_team(int e_id = 0, int t_m_ppno = 0, int e_b = 0)
            {
            return true;// dBConnection.InspectionTeamJoining(e_id, t_m_ppno, e_b);
            }

        [HttpGet]
        [HttpPost]
        public List<JoiningCompletionReportModel> get_joining_completion(int DEPT_ID, DateTime AUDIT_STARTDATE, DateTime AUDIT_ENDDATE)
            {
            return dBConnection.GetJoiningCompletion(DEPT_ID, AUDIT_STARTDATE, AUDIT_ENDDATE);

            }




        [HttpGet]
        [HttpPost]
        public List<ManageObservations> get_entity_report_paras_branch(int ENG_ID)
            {
            return dBConnection.GetEntityReportParasForBranch(ENG_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<AuditeeOldParasModel> get_assigned_observation_old_paras(int ENTITY_ID = 0)
            {
            return archiveDbConnection.GetAuditeeOldParas(ENTITY_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<AuditeeAddressModel> get_address(int ENT_ID)
            {
            return dBConnection.GetAddress(ENT_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<GetFinalReportModel> get_report_paras(int ENG_ID)
            {
            return dBConnection.GetAuditeeParas(ENG_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<PreConcludingModel> get_obs_for_pre_concluding(int ENG_ID)
            {
            return dBConnection.GetEntityObservationDetails(ENG_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<GetOldParasBranchComplianceModel> get_paras_for_compliance_by_auditee()
            {
            return dBConnection.GetParasForComplianceByAuditee();
            }
        [HttpGet]
        [HttpPost]
        public List<GetOldParasBranchComplianceModel> get_paras_for_review_compliance_by_auditee()
            {
            return dBConnection.GetParasForReviewComplianceByAuditee();
            }

        [HttpGet]
        [HttpPost]
        public List<SettledPostCompliancesModel> get_settled_post_compliances_for_monitoring(string MONTH_NAME, string YEAR)
            {
            return dBConnection.GetSettledPostCompliancesForMonitoring(MONTH_NAME, YEAR);
            }
        [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel get_para_compliance_text(int OLD_PARA_ID = 0, int NEW_PARA_ID = 0, string INDICATOR = "")
            {
            return dBConnection.GetParaComplianceText(OLD_PARA_ID, NEW_PARA_ID, INDICATOR);
            }
        [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel get_old_para_compliance_cycle_text(string COM_ID, string C_CYCLE)
            {
            return dBConnection.GetOldParasComplianceCycleText(COM_ID, C_CYCLE);
            }

         [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel GetComplianceCycleText(string COM_ID, string C_CYCLE)
            {
            return dBConnection.GetComplianceCycleText(COM_ID, C_CYCLE);
            }
        [HttpGet]
        [HttpPost]
        public AuditeeResponseEvidenceModel get_post_compliance_evidence_data(string FILE_ID)
            {
            return dBConnection.GetPostComplianceEvidenceData(FILE_ID);
            }
        [HttpGet]
        [HttpPost]
        public AuditeeResponseEvidenceModel get_cau_paras_post_compliance_evidence_data(string FILE_ID)
            {
            return dBConnection.GetCAUParasPostComplianceEvidenceData(FILE_ID);
            }

        [HttpGet]
        [HttpPost]
        public AuditeeResponseEvidenceModel get_auditee_evidence_data(string FILE_ID)
            {
            return dBConnection.GetAuditeeEvidenceData(FILE_ID);
            }
        [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel get_old_para_br_compliance_text_ref(string REF_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            return dBConnection.GetOldParasBranchComplianceTextRef(REF_P, PARA_CATEGORY, REPLY_DATE, OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel get_old_para_zone_compliance_text(string REF_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            return dBConnection.GetOldParasBranchComplianceTextForZone(REF_P, PARA_CATEGORY, REPLY_DATE, OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel get_old_para_zone_compliance_text_ref(string REF_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            return dBConnection.GetOldParasBranchComplianceTextForZoneRef(REF_P, PARA_CATEGORY, REPLY_DATE, OBS_ID);
            }

        [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel get_old_para_imp_text(int PID, string REF_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            return dBConnection.GetOldParasBranchComplianceTextForImpIncharge(PID, REF_P, PARA_CATEGORY, REPLY_DATE, OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel get_compliance_text(string COM_ID, string C_CYCLE)
            {
            return dBConnection.GetComplianceCycleText(COM_ID, C_CYCLE);
            }
        [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel get_old_para_imp_text_ref(int PID, string REF_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            return dBConnection.GetOldParasReferredBackBranchComplianceTextForImpIncharge(PID, REF_P, PARA_CATEGORY, REPLY_DATE, OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public GetOldParasBranchComplianceTextModel get_old_para_head_az_text(int PID, string REF_P, string OBS_ID, string PARA_CATEGORY, string REPLY_DATE)
            {
            return dBConnection.GetOldParasBranchComplianceTextForHeadAZ(PID, REF_P, OBS_ID, PARA_CATEGORY, REPLY_DATE);
            }

        private string BuildPostAuditComplianceSecurityEvent(
            string eventType,
            string result,
            string validationFailureReason,
            SessionUser user,
            PostAuditComplianceSecuritySnapshot snapshot,
            int submittedComId,
            string submittedOldParaId,
            int submittedNewParaId,
            string submittedIndicator,
            string dbMessage,
            string traceId)
            {
            var request = HttpContext?.Request;
            var payload = new
                {
                EventId = Guid.NewGuid().ToString("N"),
                CreatedOn = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                TraceId = traceId,
                Event = eventType,
                Api = PostAuditComplianceEndpoint,
                M = request?.Method,
                Ip = HttpContext?.RequestServices?.GetService<IClientIpResolver>()?.GetClientIp(HttpContext) ?? HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                Ua = LimitForLog(request?.Headers["User-Agent"].ToString(), 120),
                User = new
                    {
                    Id = user?.ID,
                    PPNO = user?.PPNumber,
                    Role = user?.UserRoleID,
                    Ent = user?.UserEntityID,
                    Ctx = user?.UserContextAssignmentId
                    },
                Sub = new
                    {
                    Com = submittedComId,
                    Old = submittedOldParaId,
                    New = submittedNewParaId,
                    Ind = submittedIndicator,
                    R_ID = user?.UserRoleID,
                    Ent = user?.UserEntityID
                    },
                Exp = snapshot == null ? null : new
                    {
                    Com = snapshot.COM_ID,
                    Old = snapshot.OLD_PARA_ID,
                    New = snapshot.NEW_PARA_ID,
                    snapshot.IND,
                    Ent = snapshot.ENTITY_ID,
                    Stage = snapshot.COM_STAGE,
                    Status = snapshot.COM_STATUS,
                    Cycle = snapshot.COM_CYCLE,
                    Next = snapshot.NEXT_R_ID,
                    Prev = snapshot.PER_R_ID,
                    Para = snapshot.PARA_NO
                    },
                Result = result,
                Db = LimitForLog(dbMessage, 120),
                Reason = LimitForLog(validationFailureReason, 180),
                Hash = ComputePostAuditComplianceRequestHash(user, submittedComId, submittedOldParaId, submittedNewParaId, submittedIndicator, traceId)
                };

            return LimitForLog(JsonSerializer.Serialize(payload), 1000);
            }

        private static string LimitForLog(string value, int maxLength)
            {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                {
                return value ?? string.Empty;
                }

            return value.Substring(0, maxLength);
            }

        private static string ComputePostAuditComplianceRequestHash(SessionUser user, int submittedComId, string oldParaId, int newParaId, string indicator, string traceId)
            {
            var source = string.Join("|",
                user?.PPNumber ?? string.Empty,
                user?.UserRoleID.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                user?.UserEntityID?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                submittedComId.ToString(CultureInfo.InvariantCulture),
                oldParaId ?? string.Empty,
                newParaId.ToString(CultureInfo.InvariantCulture),
                indicator ?? string.Empty,
                traceId ?? string.Empty);
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(source)));
            }

        private void LogPostAuditComplianceSecurityEvent(
            string eventType,
            string result,
            string validationFailureReason,
            SessionUser user,
            PostAuditComplianceSecuritySnapshot snapshot,
            int submittedComId,
            string submittedOldParaId,
            int submittedNewParaId,
            string submittedIndicator,
            string dbMessage,
            string traceId)
            {
            try
                {
                var payload = BuildPostAuditComplianceSecurityEvent(eventType, result, validationFailureReason, user, snapshot, submittedComId, submittedOldParaId, submittedNewParaId, submittedIndicator, dbMessage, traceId);
                var entityId = user?.UserEntityID.GetValueOrDefault() ?? 0;
                var roleId = user?.UserRoleID ?? 0;
                var ppNo = int.TryParse(user?.PPNumber, out var parsedPpNo) ? parsedPpNo : 0;
                dBConnection.AddActivityLog(entityId, roleId, ppNo, 255, payload, "Y");
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Failed to write Post Audit Compliance security event {EventType} for trace {TraceId}.", eventType, traceId);
                }
            }

        private async Task MaybeSendPostAuditComplianceAlertAsync(
            string eventType,
            string validationFailureReason,
            SessionUser user,
            PostAuditComplianceSecuritySnapshot snapshot,
            int submittedComId,
            string submittedOldParaId,
            int submittedNewParaId,
            string traceId)
            {
            var highRisk = string.Equals(eventType, "ENTITY_MISMATCH", StringComparison.OrdinalIgnoreCase)
                || string.Equals(eventType, "PARA_ENTITY_MISMATCH", StringComparison.OrdinalIgnoreCase)
                || string.Equals(eventType, "PPNO_CONTEXT_MISMATCH", StringComparison.OrdinalIgnoreCase)
                || string.Equals(eventType, "ROLE_39_ATTEMPT", StringComparison.OrdinalIgnoreCase)
                || string.Equals(eventType, "REQUEST_PARAMETER_TAMPERING", StringComparison.OrdinalIgnoreCase);

            var thresholdReached = TrackPostAuditComplianceSuspiciousFailure(user?.PPNumber ?? "anonymous");
            if (!highRisk && !thresholdReached)
                {
                return;
                }

            await EmailNotification.SendSystemErrorAlertAsync(
                _configuration,
                "PostAuditComplianceSecurityAlert",
                traceId,
                $"Post Audit Compliance {eventType}",
                validationFailureReason,
                new[]
                    {
                    new KeyValuePair<string, string>("User / PPNO", $"{user?.Name} / {user?.PPNumber}"),
                    new KeyValuePair<string, string>("Role", user?.UserRoleID.ToString()),
                    new KeyValuePair<string, string>("Entity", user?.UserEntityID.ToString()),
                    new KeyValuePair<string, string>("COM_ID / Para", $"{submittedComId} / {snapshot?.PARA_NO}"),
                    new KeyValuePair<string, string>("Event Type", eventType),
                    new KeyValuePair<string, string>("Time UTC", DateTime.UtcNow.ToString("O")),
                    new KeyValuePair<string, string>("IP", HttpContext?.RequestServices?.GetService<IClientIpResolver>()?.GetClientIp(HttpContext) ?? HttpContext?.Connection?.RemoteIpAddress?.ToString()),
                    new KeyValuePair<string, string>("Expected Context", $"Role={snapshot?.COM_STAGE}; Entity={snapshot?.ENTITY_ID}; OldPara={snapshot?.OLD_PARA_ID}; NewPara={snapshot?.NEW_PARA_ID}"),
                    new KeyValuePair<string, string>("Actual Context", $"Role={user?.UserRoleID}; Entity={user?.UserEntityID}; OldPara={submittedOldParaId}; NewPara={submittedNewParaId}"),
                    new KeyValuePair<string, string>("Trace ID", traceId),
                    new KeyValuePair<string, string>("Result", "Blocked"),
                    new KeyValuePair<string, string>("Reason", validationFailureReason)
                    },
                HttpContext?.RequestServices);
            }

        private static bool TrackPostAuditComplianceSuspiciousFailure(string ppNo)
            {
            var now = DateTime.UtcNow;
            var queue = PostAuditComplianceSuspiciousFailures.GetOrAdd(ppNo, _ => new Queue<DateTime>());
            lock (queue)
                {
                while (queue.Count > 0 && now - queue.Peek() > TimeSpan.FromMinutes(10))
                    {
                    queue.Dequeue();
                    }

                queue.Enqueue(now);
                return queue.Count >= 3;
                }
            }

        private string ValidatePostAuditComplianceSubmission(
            SessionUser user,
            PostAuditComplianceSecuritySnapshot snapshot,
            int submittedComId,
            string submittedOldParaId,
            int submittedNewParaId,
            string submittedIndicator,
            out string eventType)
            {
            eventType = "POST_AUDIT_COMPLIANCE_SUBMISSION";
            if (user == null)
                {
                eventType = "UNAUTHORIZED_API_CALL";
                return "Authenticated session is missing.";
                }

            if (submittedComId <= 0 || snapshot == null)
                {
                eventType = "INVALID_COM_ID";
                return "COM_ID does not resolve to an active Post Audit Compliance row.";
                }

            if (snapshot.NEW_PARA_ID.GetValueOrDefault() != submittedNewParaId)
                {
                eventType = "REQUEST_PARAMETER_TAMPERING";
                return $"Submitted NEW_PARA_ID {submittedNewParaId} does not match COM_ID {submittedComId} NEW_PARA_ID {snapshot.NEW_PARA_ID.GetValueOrDefault()}.";
                }

            if (snapshot.OLD_PARA_ID.HasValue && !string.IsNullOrWhiteSpace(submittedOldParaId) && snapshot.OLD_PARA_ID.Value.ToString(CultureInfo.InvariantCulture) != submittedOldParaId.Trim())
                {
                eventType = "REQUEST_PARAMETER_TAMPERING";
                return $"Submitted OLD_PARA_ID {submittedOldParaId} does not match COM_ID {submittedComId} OLD_PARA_ID {snapshot.OLD_PARA_ID.Value}.";
                }

            if (user.UserRoleID != snapshot.COM_STAGE.GetValueOrDefault() && user.UserRoleID != 39)
                {
                eventType = "ROLE_STAGE_MISMATCH";
                return $"Session role {user.UserRoleID} does not match current COM_STAGE {snapshot.COM_STAGE.GetValueOrDefault()}.";
                }

            return string.Empty;
            }

        [HttpPost]
        [ApplicationAudit("POST_AUDIT_COMPLIANCE_SUBMITTED", "COMPLIANCE", "Post Audit Compliance", "pkg_ae", "P_SubmitPostAuditCompliance", OldParaId = "OLD_PARA_ID", NewParaId = "NEW_PARA_ID", ComId = "SUBFOLDER", ObjectType = "COMPLIANCE", ObjectId = "SUBFOLDER", SuccessMessageContains = "Submitted")]
        public async Task<string> submit_post_audit_compliance(string OLD_PARA_ID, int NEW_PARA_ID, string INDICATOR, string COMPLIANCE, string COMMENTS, List<AuditeeResponseEvidenceModel> EVIDENCE_LIST, string SUBFOLDER)
            {
            var traceId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            var user = sessionHandler.GetUser();
            var submittedComId = int.TryParse(SUBFOLDER, out var parsedComId) ? parsedComId : 0;
            var snapshot = dBConnection.GetPostAuditComplianceSecuritySnapshot(submittedComId);
            var validationFailure = ValidatePostAuditComplianceSubmission(user, snapshot, submittedComId, OLD_PARA_ID, NEW_PARA_ID, INDICATOR, out var eventType);
            if (!string.IsNullOrWhiteSpace(validationFailure))
                {
                LogPostAuditComplianceSecurityEvent(eventType, "Blocked", validationFailure, user, snapshot, submittedComId, OLD_PARA_ID, NEW_PARA_ID, INDICATOR, string.Empty, traceId);
                await MaybeSendPostAuditComplianceAlertAsync(eventType, validationFailure, user, snapshot, submittedComId, OLD_PARA_ID, NEW_PARA_ID, traceId);
                return "{\"Status\":false,\"Message\":\"Your login context is not authorized for this compliance. Please refresh, select the correct role/entity, and try again.\"}";
                }

            LogPostAuditComplianceSecurityEvent("POST_AUDIT_COMPLIANCE_SUBMISSION", "Allowed", string.Empty, user, snapshot, submittedComId, OLD_PARA_ID, NEW_PARA_ID, INDICATOR, string.Empty, traceId);
            string response = await dBConnection.SubmitPostAuditCompliance(OLD_PARA_ID, NEW_PARA_ID, INDICATOR, COMPLIANCE, COMMENTS, EVIDENCE_LIST, SUBFOLDER);
            var resultEvent = response?.IndexOf("Submitted", StringComparison.OrdinalIgnoreCase) >= 0
                ? "POST_AUDIT_COMPLIANCE_SUBMITTED"
                : "DUPLICATE_SUBMISSION_ATTEMPT";
            LogPostAuditComplianceSecurityEvent(resultEvent, "Completed", string.Empty, user, snapshot, submittedComId, OLD_PARA_ID, NEW_PARA_ID, INDICATOR, response, traceId);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("POST_AUDIT_COMPLIANCE_REVIEWED", "COMPLIANCE", "Post Audit Compliance", "pkg_ae", "P_SubmitPostAuditCompliance_Review", OldParaId = "OLD_PARA_ID", NewParaId = "NEW_PARA_ID", ObjectType = "COMPLIANCE", SuccessMessageContains = "settled|Forwarded|Rejected/Referred Back")]
        public string submit_post_audit_compliance_review(string OLD_PARA_ID, int NEW_PARA_ID, string INDICATOR, string COMPLIANCE, string COMMENTS, List<AuditeeResponseEvidenceModel> EVIDENCE_LIST)
            {
            var remarksWithoutTags = RichTextTagRegex.Replace(COMMENTS ?? string.Empty, string.Empty);
            var remarksText = (System.Net.WebUtility.HtmlDecode(remarksWithoutTags) ?? string.Empty).Replace('\u00A0', ' ');
            if (remarksText.Length > 1000)
                {
                return "{\"Status\":false,\"Message\":\"Only 1000 characters are allowed in Remarks.\"}";
                }

            string response = "";
            response = dBConnection.SubmitPostAuditComplianceReview(OLD_PARA_ID, NEW_PARA_ID, INDICATOR, COMPLIANCE, COMMENTS, EVIDENCE_LIST);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<GetOldParasForComplianceReviewer> get_branch_comp_review()
            {
            return dBConnection.GetOldParasForReviewer();
            }
        [HttpGet]
        [HttpPost]
        public List<GetOldParasForComplianceReviewer> get_branch_comp_review_ref()
            {
            return dBConnection.GetOldParasForReviewerRef();
            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_COMPLIANCE_REVIEWED", "COMPLIANCE", "POST AUDIT COMPLIANCE", "PKG_AE", "P_ADDOLDPARASREVIEWER", ParaId = "Para_ID", ObjectType = "LEGACY_PARA", ObjectId = "Para_ID", RequireResultMessage = true)]
        public string AddOldParasComplianceReviewer(string Para_ID, string PARA_CAT, string REPLY, string r_status, string OBS_ID, int PARENT_ID, string SEQUENCE, string AUDITED_BY)
            {
            string response = "";
            response = dBConnection.AddOldParasComplianceReviewer(Para_ID, PARA_CAT, REPLY, r_status, OBS_ID, PARENT_ID, SEQUENCE, AUDITED_BY);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<GetOldParasforComplianceSettlement> get_old_para_br_compliance_submission()
            {
            return dBConnection.GetOldParasBranchComplianceSubmission();
            }

        [HttpGet]
        [HttpPost]
        public List<GetOldParasforComplianceSettlement> get_old_para_br_compliance_recommendation()
            {
            return dBConnection.GetComplianceForImpZone();
            }
        [HttpGet]
        [HttpPost]
        public List<GetOldParasforComplianceSettlement> get_old_para_br_compliance_recommendation_ref()
            {
            return dBConnection.GetReferredBackParasComplianceForImpZone();
            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_COMPLIANCE_SUBMITTED", "COMPLIANCE", "POST AUDIT COMPLIANCE", "PKG_HD", "P_ADDOLDPARASIMPREMARKS", ParaId = "OBS_ID", ObjectType = "LEGACY_PARA", ObjectId = "OBS_ID", RequireResultMessage = true)]
        public string submit_old_para_br_compliance_status(string OBS_ID, string REFID, string REMARKS, int NEW_STATUS, string PARA_CAT, string SETTLE_INDICATOR, string SEQUENCE, string AUDITED_BY)
            {
            string response = "";
            response = dBConnection.AddOldParasStatusUpdate(OBS_ID, REFID, REMARKS, NEW_STATUS, PARA_CAT, SETTLE_INDICATOR, SEQUENCE, AUDITED_BY);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_PARTIAL_SETTLEMENT_SUBMITTED", "COMPLIANCE", "POST AUDIT COMPLIANCE", "PKG_HD", "P_ADDOLDPARASIMPREMARKS_PARTIAL_COMP", ParaId = "OBS_ID", ObjectType = "LEGACY_PARA", ObjectId = "OBS_ID", RequireResultMessage = true)]
        public string submit_old_para_br_compliance_status_partially_settle(string OBS_ID, string REFID, string REMARKS, int NEW_STATUS, string PARA_CAT, string SETTLE_INDICATOR, List<ObservationResponsiblePPNOModel> RESPONSIBLES_ARR, string SEQUENCE, string AUDITED_BY, string PARA_TEXT)
            {
            string response = "";
            response = dBConnection.AddOldParasStatusPartiallySettle(OBS_ID, REFID, REMARKS, NEW_STATUS, PARA_CAT, SETTLE_INDICATOR, RESPONSIBLES_ARR, SEQUENCE, AUDITED_BY, PARA_TEXT);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<GetOldParasForFinalSettlement> get_old_para_br_compliance_head()
            {
            return dBConnection.GetOldParasForFinalSettlement();
            }

        [HttpPost]
        [ApplicationAudit("PARA_SETTLEMENT_SUBMITTED", "COMPLIANCE", "Post Audit Compliance", "pkg_hd", "P_AddFinalsettlement", ParaId = "PARA_ID", ObjectType = "PARA", ObjectId = "PARA_ID", RequireResultMessage = true)]
        public string submit_old_para_compliance_head_status(int PARA_ID, string REMARKS, int NEW_STATUS, string PARA_REF, string PARA_INDICATOR, string PARA_CATEGORY, int AU_OBS_ID, string SEQUENCE, string AUDITED_BY, string ENTITY_ID)
            {
            string response = "";
            response = dBConnection.AddOldParasheadStatusUpdate(PARA_ID, REMARKS, NEW_STATUS, PARA_REF, PARA_INDICATOR, PARA_CATEGORY, AU_OBS_ID, SEQUENCE, AUDITED_BY, ENTITY_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<BranchModel> get_zone_Branches(int ZONEID)
            {
            return dBConnection.GetZoneBranches(ZONEID, false);

            }
        [HttpGet]
        [HttpPost]
        public List<AuditeeOldParasModel> get_old_paras_for_monitoring(int ENTITY_ID)
            {
            return dBConnection.GetOldParasForMonitoring(ENTITY_ID);
            }

        [HttpGet]
        [HttpPost]
        public string get_para_text(string ref_p)
            {
            return dBConnection.GetParaText(ref_p);
            }

        [HttpGet]
        [HttpPost]
        public string get_all_para_text(int COM_ID)
            {
            return dBConnection.GetAllParaText(COM_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<AuditeeOldParasPpnoModel> get_old_paras_for_monitoring_ppno(int ppno)
            {
            return dBConnection.GetOldParasForMonitoringPpno(ppno);
            }

        [HttpPost]
        public IActionResult FindUsers([FromBody] FindUserSearchModel user)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { error = "unauthorized", message = "User session is not authenticated." });
                }

            if (user == null)
                {
                return InvalidRequestResponse("request", "User search payload is required.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            try
                {
                var searchCriteria = new FindUserModel
                    {
                    PPNUMBER = user.PPNUMBER,
                    LOGINNAME = user.LOGINNAME?.Trim(),
                    EMAIL = user.EMAIL?.Trim(),
                    GROUPID = user.GROUPID,
                    ENTITYID = user.ENTITYID
                    };

                var users = dBConnection.GetAllUsers(searchCriteria);
                return Ok(users);
                }
            catch (DatabaseUnavailableException ex)
                {
                _logger.LogError(ex, "Database connection is unavailable while searching for users via API.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "database_unavailable", message = "The database is currently unavailable. Please try again later." });
                }
            }

        [HttpPost]
        public IActionResult GetUserContexts([FromForm] int? userId, [FromForm] string ppNumber)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            if (!userId.HasValue && string.IsNullOrWhiteSpace(ppNumber))
                {
                return BadRequest(new { error = "invalid_request", message = "A user id or PP number is required." });
                }

            return Ok(dBConnection.GetUserContextAssignments(userId, ppNumber));
            }

        [HttpGet]
        [HttpPost]
        public string get_user_name(string PPNUMBER)
            {
            string response = "";
            response = dBConnection.GetUserName(PPNUMBER);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("OLD_PARA_STATUS_CHANGE_REQUESTED", "COMPLIANCE", "PARA STATUS", "PKG_HD", "P_CHANGESTATUSREQUESTFORSETTLEDPARA", ParaId = "OBS_ID", ObjectType = "OLD_PARA", ObjectId = "OBS_ID", RequireResultMessage = true)]
        public string Add_Old_Para_Change_status(string REFID, string OBS_ID, string INDICATOR, int NEW_STATUS, string REMARKS)
            {
            string response = "";
            response = dBConnection.AddChangeStatusRequestForSettledPara(REFID, OBS_ID, INDICATOR, NEW_STATUS, REMARKS);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("OLD_PARA_STATUS_CHANGE_REVIEWED", "COMPLIANCE", "PARA STATUS", "PKG_HD", "P_CHANGESTATUSREQUESTFORSETTLEDPARA_NEW_REVIEWER", ObjectType = "OLD_PARA_STATUS_REQUEST", ObjectId = "REFID", RequireResultMessage = true)]
        public string Add_Old_Para_Change_status_Review(string REFID, string IND, string REMARKS, string Action_IND)
            {
            string response = "";
            response = dBConnection.ReviewerAddChangeStatusRequestForSettledPara(REFID, IND, REMARKS, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("OLD_PARA_STATUS_CHANGE_AUTHORIZED", "COMPLIANCE", "PARA STATUS", "PKG_FAD", "P_AUTHORIZECHANGESTATUSREQUESTFORSETTLEDPARA_NEW", ObjectType = "OLD_PARA_STATUS_REQUEST", ObjectId = "REFID", RequireResultMessage = true)]
        public string Add_Old_Para_Change_status_Authorize(string REFID, string IND, int NEW_STATUS, string REMARKS, string Action_IND)
            {
            string response = "";
            response = dBConnection.AuthorizerAddChangeStatusRequestForSettledPara(REFID, IND, NEW_STATUS, REMARKS, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("NEW_PARA_STATUS_CHANGE_REQUESTED", "COMPLIANCE", "PARA STATUS", "PKG_HD", "P_CHANGESTATUSREQUESTFORSETTLEDPARA_NEW", ObjectType = "PARA_STATUS_REQUEST", ObjectId = "REFID", RequireResultMessage = true)]
        public string Add_New_Para_Change_status_Request(string REFID, int NEW_STATUS, string REMARKS)
            {
            string response = "";
            response = archiveDbConnection.AddChangeStatusRequestForCurrentPara(REFID, NEW_STATUS, REMARKS);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]

        [ApplicationAudit("PARA_STATUS_CHANGE_REQUESTED", "COMPLIANCE", "PARA STATUS", "PKG_HD", "P_ADD_PARAS_FOR_STATUS_CHANGE", ComId = "COM_ID", ObjectType = "PARA", ObjectId = "COM_ID", RequireResultMessage = true)]
        public string Add_Para_Change_status_Request(string COM_ID, int NEW_STATUS, string REMARKS, string IND, string Action_IND)
            {
            string response = "";
            response = archiveDbConnection.AddChangeStatusRequestForPara(COM_ID, NEW_STATUS, REMARKS, IND, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("OLD_PARA_STATUS_CHANGE_AUTHORIZATION_RECORDED", "COMPLIANCE", "PARA STATUS", "PKG_FAD", "P_AUTHORIZECHANGESTATUSREQUESTFORSETTLEDPARA", ParaId = "OBS_ID", ObjectType = "OLD_PARA", ObjectId = "OBS_ID", RequireResultMessage = true)]
        public string Add_Authorization_Old_Para_Change_status(string REFID, string OBS_ID, string IND, string Action_IND)
            {
            string response = "";
            response = archiveDbConnection.AddAuthorizeChangeStatusRequestForSettledPara(REFID, OBS_ID, IND, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasAuthorizeModel> get_legacy_settled_paras_autorize()
            {
            return archiveDbConnection.GetOldSettledParasForResponseAuthorize();
            }

        [HttpGet]
        [HttpPost]
        public List<GetOldParasBranchComplianceModel> get_old_para_br_compliance_text_update()
            {
            return archiveDbConnection.GetOldParasBranchComplianceTextupdate();
            }

        [HttpGet]
        [HttpPost]
        public List<ParaStatusChangeModel> get_paras_for_status_change_authorize()
            {
            return archiveDbConnection.GetParasForStatusChangeToAuthorize();
            }

        [HttpPost]
        [ApplicationAudit("PARA_STATUS_CHANGE_AUTHORIZED", "COMPLIANCE", "PARA STATUS", "PKG_HD", "P_AUTHORIZE_PARAS_FOR_STATUS", ParaId = "NEW_PARA_ID", OldParaId = "OLD_PARA_ID", NewParaId = "NEW_PARA_ID", ComId = "COM_ID", ObjectType = "PARA", ObjectId = "COM_ID", RequireResultMessage = true)]
        public string authorize_para_change_status(string COM_ID, int NEW_PARA_ID, int OLD_PARA_ID, string REMARKS, string IND, string Action_IND)
            {
            string response = "";
            response = archiveDbConnection.AuthorizeChangeStatusRequestForPara(COM_ID, NEW_PARA_ID, OLD_PARA_ID, REMARKS, IND, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        public List<GetTeamDetailsModel> GetTeamDetails(int ENG_ID)
            {
            return dBConnection.GetTeamDetails(ENG_ID);
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("SUBMIT_PRE_CONCLUDING", "ADMINISTRATION", "ADMINISTRATION", "PKG_HD", "P_AUDIT_PRE_SUBMISSION", EngagementId = "ENG_ID", ObjectType = "SUBMIT_PRE_CONCLUDING", RequireResultMessage = true)]
        public string submit_pre_concluding(int ENG_ID)
            {
            string response = "";
            response = dBConnection.SubmitPreConcluding(ENG_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("AUDIT_PARA_FINALIZATION_UPDATED", "COMPLIANCE", "PARA FINALIZATION", "PKG_HD", "P_AUDIT_PARA_UPDATE_SVZ_AZ", ParaId = "OBS_ID", ObjectType = "PARA", ObjectId = "OBS_ID", RequireResultMessage = true)]
        public string update_audit_para_for_finalization(int OBS_ID, string ANNEX_ID, string PROCESS_ID, int SUB_PROCESS_ID, int PROCESS_DETAIL_ID, int RISK_ID, int FINAL_PARA_NO, string GIST_OF_PARA, string TEXT_PARA, string AMOUNT_INV, string NO_INST, long? REFERENCE_ID = null)
            {
            string response = "";
            response = dBConnection.UpdateAuditParaForFinalization(OBS_ID, ANNEX_ID, PROCESS_ID, SUB_PROCESS_ID, PROCESS_DETAIL_ID, RISK_ID, FINAL_PARA_NO, GIST_OF_PARA, TEXT_PARA, AMOUNT_INV, NO_INST, REFERENCE_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_PARA_FINALIZATION_HO_UPDATED", "COMPLIANCE", "PARA FINALIZATION", "PKG_HD", "P_AUDIT_PARA_UPDATE_HEAD_DEPT", ParaId = "OBS_ID", ObjectType = "PARA", ObjectId = "OBS_ID", RequireResultMessage = true)]
        public string update_audit_para_for_finalization_ho(int OBS_ID, string VIOLATION_ID, int VIOLATION_NATURE_ID, int RISK_ID, string GIST_OF_PARA, string TEXT_PARA)
            {
            string response = "";
            response = dBConnection.UpdateAuditParaForFinalizationHO(OBS_ID, VIOLATION_ID, VIOLATION_NATURE_ID, RISK_ID, GIST_OF_PARA, TEXT_PARA);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_legacy_paras_for_update(int ENTITY_ID, string PARA_REF, int PARA_ID = 0)
            {
            return dBConnection.GetLegacyParasForUpdate(ENTITY_ID, PARA_REF, PARA_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_legacy_paras_for_update_ho(string ENTITY_NAME, string PARA_REF, int PARA_ID = 0)
            {
            return dBConnection.GetLegacyParasForUpdateHO(ENTITY_NAME, PARA_REF, PARA_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_legacy_paras_for_gist_update(int ENTITY_ID, string PARA_REF, int PARA_ID = 0)
            {
            return dBConnection.GetLegacyParasForGistUpdate(ENTITY_ID, PARA_REF, PARA_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasModel> get_legacy_paras_for_update_FAD(int ENTITY_ID, string PARA_REF, int PARA_ID = 0)
            {
            return dBConnection.GetLegacyParasForUpdateFAD(ENTITY_ID, PARA_REF, PARA_ID);
            }
        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_UPDATED", "COMPLIANCE", "LEGACY PARA", "PKG_AR", "P_UPDATE_LEGACY_PARA_TEXT", ParaId = "LEGACY_PARA.NEW_PARA_ID", OldParaId = "LEGACY_PARA.OLD_PARA_ID", NewParaId = "LEGACY_PARA.NEW_PARA_ID", ComId = "LEGACY_PARA.COM_ID", ObjectType = "LEGACY_PARA", RequireResultMessage = true)]
        public string update_legacy_para_with_responsibilities(AddLegacyParaModel LEGACY_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateLegacyParasWithResponsibility(LEGACY_PARA) + "\"}";

            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_GIST_NUMBER_UPDATED", "COMPLIANCE", "LEGACY PARA", "PKG_AR", "P_UPDATE_LEGACY_PARA_GIST", ObjectType = "LEGACY_PARA", ObjectId = "PARA_REF", RequireResultMessage = true)]
        public string update_legacy_para_gist_paraNo(string PARA_REF, string PARA_NO, string GIST_OF_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateLegacyParaGistParaNo(PARA_REF, PARA_NO, GIST_OF_PARA) + "\"}";

            }
        //
        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_RESPONSIBILITY_DELETED", "COMPLIANCE", "LEGACY PARA", "PKG_AR", "P_DELETE_PARA_RESPONSIBILITY", ParaId = "P_ID", ObjectType = "LEGACY_PARA", ObjectId = "P_ID", RequireResultMessage = true)]
        public string delete_responsibility_of_legacy_para(string REF_P, int P_ID, int PP_NO)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.DeleteResponsibilityOfLegacyParas(REF_P, P_ID, PP_NO) + "\"}";

            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_RESPONSIBILITY_ADDED", "COMPLIANCE", "LEGACY PARA", "PKG_AR", "P_ADD_PARA_RESPONSIBILITY", ParaId = "P_ID", ObjectType = "LEGACY_PARA", ObjectId = "P_ID", RequireResultMessage = true)]
        public string add_responsibility_to_legacy_para(ObservationResponsiblePPNOModel RESP_PP, string REF_P, int P_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddResponsibilityToLegacyParas(RESP_PP, REF_P, P_ID) + "\"}";

            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_RESPONSIBILITY_FAD_ADDED", "COMPLIANCE", "LEGACY PARA", "PKG_FAD", "P_ADD_PARA_RESPONSIBILITY", ParaId = "P_ID", ObjectType = "LEGACY_PARA", ObjectId = "P_ID", RequireResultMessage = true)]
        public string add_responsibility_to_legacy_para_fad(ObservationResponsiblePPNOModel RESP_PP, string REF_P, int P_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddResponsibilityToLegacyParasFAD(RESP_PP, REF_P, P_ID) + "\"}";

            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_NO_CHANGE_AZ_RECORDED", "COMPLIANCE", "LEGACY PARA", "PKG_AR", "P_NO_UPDATE_LEGACY_PARA_TEXT", ParaId = "LEGACY_PARA.NEW_PARA_ID", ComId = "LEGACY_PARA.COM_ID", ObjectType = "LEGACY_PARA", RequireResultMessage = true)]
        public string update_legacy_para_with_responsibilities_no_changes_AZ(AddLegacyParaModel LEGACY_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateLegacyParasWithResponsibilityNoChangesAZ(LEGACY_PARA) + "\"}";

            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_NO_CHANGE_REVIEWED", "COMPLIANCE", "LEGACY PARA", "PKG_FAD", "P_REVIEWED_LEGACY_PARA", ParaId = "LEGACY_PARA.NEW_PARA_ID", ComId = "LEGACY_PARA.COM_ID", ObjectType = "LEGACY_PARA", RequireResultMessage = true)]
        public string update_legacy_para_with_responsibilities_no_changes(AddLegacyParaModel LEGACY_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateLegacyParasWithResponsibilityNoChanges(LEGACY_PARA) + "\"}";

            }

        [HttpGet]
        [HttpPost]
        public UserModel get_employee_name_from_pp(int PP_NO)
            {
            return dBConnection.GetEmployeeNameFromPPNO(PP_NO);

            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_FAD_UPDATED", "COMPLIANCE", "LEGACY PARA", "PKG_FAD", "P_UPDATE_LEGACY_PARA_TEXT", ParaId = "LEGACY_PARA.NEW_PARA_ID", OldParaId = "LEGACY_PARA.OLD_PARA_ID", NewParaId = "LEGACY_PARA.NEW_PARA_ID", ComId = "LEGACY_PARA.COM_ID", ObjectType = "LEGACY_PARA", RequireResultMessage = true)]
        public string update_legacy_para_with_responsibilities_FAD(AddLegacyParaModel LEGACY_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateLegacyParasWithResponsibilityFAD(LEGACY_PARA) + "\"}";

            }


        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_RESPONSIBILITY_FAD_DELETED", "COMPLIANCE", "LEGACY PARA", "PKG_FAD", "P_DELETE_PARA_RESPONSIBILITY", ParaId = "PARA_ID", ObjectType = "LEGACY_PARA", ObjectId = "PARA_ID", RequireResultMessage = true)]
        public string delete_legacy_para_responsibility(string PARA_REF, int PARA_ID, int PP_NO)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.DeleteLegacyParaResponsibility(PARA_REF, PARA_ID, PP_NO) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<AuditEntitiesModel> get_auditee_entities_by_entity_type_id(int ENTITY_TYPE_ID)
            {
            return archiveDbConnection.GetAuditEntitiesByTypeId(ENTITY_TYPE_ID);
            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_ADDED", "COMPLIANCE", "LEGACY PARA", "PKG_HD", "P_ADD_LEGACY_PARA", ObjectType = "LEGACY_PARA", RequireResultMessage = true)]
        public string add_new_legacy_para(AddNewLegacyParaModel LEGACY_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + archiveDbConnection.AddNewLegacyPara(LEGACY_PARA) + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_REFERRED_BACK", "COMPLIANCE", "LEGACY PARA", "PKG_FAD", "P_REFERBACK_LEGACY_PARA", ParaId = "PARA_ID", ObjectType = "LEGACY_PARA", ObjectId = "PARA_ID", RequireResultMessage = true)]
        public string refer_back_legacy_para_to_az(string PARA_REF, int PARA_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.ReferBackLegacyPara(PARA_REF, PARA_ID) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<AddNewLegacyParaModel> get_add_legacy_paras_autorize()
            {
            return archiveDbConnection.GetAddedLegacyParaForAuthorize();
            }

        //
        [HttpGet]
        [HttpPost]
        public List<AddNewLegacyParaModel> get_update_gist_paraNo_legacy_paras_autorize()
            {
            return archiveDbConnection.GetUpdatedGistParaOfLegacyParaForAuthorize();
            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_ADDITION_AUTHORIZED", "COMPLIANCE", "LEGACY PARA", "PKG_HD", "P_AUTHORIZE_LEGACY_PARA_ADDITION", ObjectType = "LEGACY_PARA", ObjectId = "PARA_REF", RequireResultMessage = true)]
        public string Authorize_Legacy_Para_addition(string PARA_REF)
            {
            return "{\"Status\":true,\"Message\":\"" + archiveDbConnection.AuthorizeLegacyParaAddition(PARA_REF) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_GIST_NUMBER_AUTHORIZED", "COMPLIANCE", "LEGACY PARA", "PKG_AR", "P_AUTHORIZE_PARA_GIST", ObjectType = "LEGACY_PARA", ObjectId = "PARA_REF", RequireResultMessage = true)]
        public string Authorize_Legacy_Para_Gist_ParaNo(string PARA_REF, string GIST_OF_PARA, string PARA_NO)
            {
            return "{\"Status\":true,\"Message\":\"" + archiveDbConnection.AuthorizeLegacyParaGistParaNoUpdate(PARA_REF, GIST_OF_PARA, PARA_NO) + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_ADDITION_REQUEST_DELETED", "COMPLIANCE", "LEGACY PARA", "PKG_HD", "P_REFERREDBACK_DEL_PARA", ObjectType = "LEGACY_PARA", ObjectId = "PARA_REF", RequireResultMessage = true)]
        public string Delete_Legacy_Para_addition_request(string PARA_REF)
            {
            return "{\"Status\":true,\"Message\":\"" + archiveDbConnection.DeleteLegacyParaAdditionRequest(PARA_REF) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<AuditeeOldParasModel> get_legacy_report_dropdown_contents(int ENTITY_ID)
            {
            return dBConnection.GetLegacyParasEntitiesReport(ENTITY_ID);
            }

        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_SETTLED_HO", "COMPLIANCE", "PARA SETTLEMENT", "PKG_AR", "P_SETTEL_LEGACY_PARA_HO", ObjectType = "LEGACY_PARA", ObjectId = "PARA_REF", RequireResultMessage = true)]
        public string settle_legacy_para_HO(int NEW_STATUS, string PARA_REF, string SETTLEMENT_NOTES)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SettleLegacyParaHO(NEW_STATUS, PARA_REF, SETTLEMENT_NOTES) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_DELETED_HO", "COMPLIANCE", "LEGACY PARA", "PKG_AR", "P_DELETE_LEGACY_PARA_HO", ObjectType = "LEGACY_PARA", ObjectId = "PARA_REF", RequireResultMessage = true)]
        public string delete_legacy_para_HO(string PARA_REF)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.DeleteLegacyParaHO(PARA_REF) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<FADNewOldParaPerformanceModel> get_relation_legacy_observation_for_dashboard(int ENTITY_ID = 0)
            {
            return dBConnection.GetRelationLegacyObservationForDashboard(ENTITY_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<FADNewOldParaPerformanceModel> get_relation_ais_observation_for_dashboard(int ENTITY_ID = 0)
            {
            return dBConnection.GetRelationAISObservationForDashboard(ENTITY_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<FADNewOldParaPerformanceModel> get_relation_observation_for_dashboard(int ENTITY_ID = 0)
            {
            return dBConnection.GetRelationObservationForDashboard(ENTITY_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<FunctionalResponsibilitiesWiseParasModel> get_functional_responsibility_wise_paras_for_dashboard(int FUNCTIONAL_ENTITY_ID = 0)
            {
            return archiveDbConnection.GetFunctionalResponsibilityWiseParaForDashboard(FUNCTIONAL_ENTITY_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<FADNewOldParaPerformanceModel> get_functional_responsibility_wise_paras_for_dashboard_ho(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, int PROCESS_DETAIL_ID = 0, int FUNCTIONAL_ENTITY_ID = 0, int DEPT_ID = 0)
            {
            return archiveDbConnection.GetHOFunctionalResponsibilityWiseParaForDashboard(PROCESS_ID, SUB_PROCESS_ID, PROCESS_DETAIL_ID, FUNCTIONAL_ENTITY_ID, DEPT_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<FADNewOldParaPerformanceModel> get_violation_wise_paras_for_dashboard(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, int PROCESS_DETAIL_ID = 0)
            {
            return dBConnection.GetViolationWiseParaForDashboard(PROCESS_ID, SUB_PROCESS_ID, PROCESS_DETAIL_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<NoEntitiesRiskBasePlan> get_risk_base_plan_for_dashboard()
            {
            return archiveDbConnection.GetEntitiesRiskBasePlanForDashboard();
            }
        [HttpGet]
        [HttpPost]
        public List<AuditPerformanceChartDashboardModel> get_audit_performance_chart_for_dashboard()
            {
            return dBConnection.GetAuditPerformanceChartForDashboard();
            }
        [HttpGet]
        [HttpPost]
        public List<FADAuditPerformanceModel> get_audit_performance_for_dashboard()
            {
            return dBConnection.GetAuditPerformanceForDashboard();
            }

        [HttpGet]
        [HttpPost]
        public List<SubCheckListStatus> get_audit_sub_checklist(int PROCESS_ID = 0)
            {
            return dBConnection.GetAuditSubChecklist(PROCESS_ID);
            }
        [HttpPost]
        [ApplicationAudit("AUDIT_SUB_CHECKLIST_CREATED", "CHECKLIST", "CHECKLIST_SETUP", "PKG_AD", "P_AUDIT_CHECKLIST_SUB", ObjectType = "AUDIT_CHECKLIST", ObjectId = "PROCESS_ID", RequireResultMessage = true)]
        public string add_audit_sub_checklist(int PROCESS_ID = 0, int ENTITY_TYPE_ID = 0, string HEADING = "", string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAuditSubChecklist(PROCESS_ID, ENTITY_TYPE_ID, HEADING, RISK_SEQUENCE, RISK_WEIGHTAGE) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("AUDIT_SUB_CHECKLIST_UPDATED", "CHECKLIST", "CHECKLIST_SETUP", "PKG_AD", "P_AUDIT_CHECKLIST_SUB_UPDATE", ObjectType = "AUDIT_SUB_CHECKLIST", ObjectId = "SUB_PROCESS_ID", RequireResultMessage = true)]
        public string update_audit_sub_checklist(int PROCESS_ID = 0, int OLD_PROCESS_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int ENTITY_TYPE_ID = 0, string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditSubChecklist(PROCESS_ID, OLD_PROCESS_ID, SUB_PROCESS_ID, HEADING, ENTITY_TYPE_ID, RISK_SEQUENCE, RISK_WEIGHTAGE) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<SubProcessUpdateModelForReviewAndAuthorizeModel> get_sub_checklist_comparison_by_Id(int SUB_PROCESS_ID = 0)
            {
            return dBConnection.GetSubChecklistComparisonDetailById(SUB_PROCESS_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<AuditChecklistDetailsModel> get_audit_checklist_detail(int SUB_PROCESS_ID = 0)
            {
            return dBConnection.GetAuditChecklistDetail(SUB_PROCESS_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<AuditChecklistDetailsModel> get_audit_checklist_detail_for_remove_duplicate(int SUB_PROCESS_ID = 0)
            {
            return dBConnection.GetAuditChecklistDetailForRemoveDuplicate(SUB_PROCESS_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<AuditChecklistDetailsModel> get_checklist_details_for_sub_process(int SUB_PROCESS_ID = 0)
            {
            return dBConnection.GetChecklistDetailForSubProcess(SUB_PROCESS_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<AuditChecklistDetailsModel> get_ref_audit_checklist_detail()
            {
            return dBConnection.GetReferredBackAuditChecklistDetail();
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_CHECKLIST_DETAIL_CREATED", "CHECKLIST", "CHECKLIST_SETUP", "PKG_AD", "P_AUDIT_CHECKLIST_DETAIL", ObjectType = "AUDIT_SUB_CHECKLIST", ObjectId = "SUB_PROCESS_ID", RequireResultMessage = true)]
        public string add_audit_checklist_detail(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int V_ID = 0, int CONTROL_ID = 0, int ROLE_ID = 0, int RISK_ID = 0, string ANNEX_CODE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAuditChecklistDetail(PROCESS_ID, SUB_PROCESS_ID, HEADING, V_ID, CONTROL_ID, ROLE_ID, RISK_ID, ANNEX_CODE) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("AUDIT_CHECKLIST_DETAIL_UPDATED", "CHECKLIST", "CHECKLIST_SETUP", "PKG_AD", "AUDIT_CHECKLIST_DETAIL_UPDATE", ObjectType = "AUDIT_CHECKLIST_DETAIL", ObjectId = "PROCESS_DETAIL_ID", RequireResultMessage = true)]
        public string update_audit_checklist_detail(int PROCESS_DETAIL_ID = 0, int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int V_ID = 0, int CONTROL_ID = 0, int ROLE_ID = 0, int RISK_ID = 0, string ANNEX_CODE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditChecklistDetail(PROCESS_DETAIL_ID, PROCESS_ID, SUB_PROCESS_ID, HEADING, V_ID, CONTROL_ID, ROLE_ID, RISK_ID, ANNEX_CODE) + "\"}";
            }


        [HttpGet]
        [HttpPost]
        public List<RepetativeParaModel> get_repetative_paras_for_dashboard(int P_ID = 0, int SP_ID = 0, int PD_ID = 0)
            {
            return dBConnection.GetRepetativeParaForDashboard(P_ID, SP_ID, PD_ID);
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_CHECKLIST_CREATED", "CHECKLIST", "CHECKLIST_SETUP", "PKG_AD", "P_AUDIT_CHECKLIST", ObjectType = "AUDIT_CHECKLIST", RequireResultMessage = true)]
        public string add_audit_checklist(string HEADING = "", int ENTITY_TYPE_ID = 0, string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAuditChecklist(HEADING, ENTITY_TYPE_ID, RISK_SEQUENCE, RISK_WEIGHTAGE) + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_CHECKLIST_UPDATED", "CHECKLIST", "CHECKLIST_SETUP", "PKG_AD", "P_AUDIT_CHECKLIST_UPDATE", ObjectType = "AUDIT_CHECKLIST", ObjectId = "PROCESS_ID", RequireResultMessage = true)]
        public string update_audit_checklist(int PROCESS_ID = 0, string HEADING = "", string ACTIVE = "", string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditChecklist(PROCESS_ID, HEADING, ACTIVE, RISK_SEQUENCE, RISK_WEIGHTAGE) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<AuditeeEntitiesModel> get_entities_parent_ent_type_id(int ENTITY_TYPE_ID = 0)
            {
            return dBConnection.GetEntitiesByParentEntityTypeId(ENTITY_TYPE_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<ObservationStatusReversalModel> get_engagement_status_for_reversal(int ENG_ID = 0)
            {
            return dBConnection.GetEngagementReversalStatus(ENG_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<ObservationReversalModel> get_engagements_details_for_status_reversal(int ENTITY_ID = 0)
            {
            return dBConnection.GetEngagementDetailsForStatusReversal(ENTITY_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<AuditReportModel> get_audit_report_for_fad_review(int RPT_ID = 0, int ENG_ID = 0)
            {
            return dBConnection.GetAuditReportForFadReview(RPT_ID, ENG_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<ObservationReversalModel> get_engagements_details_for_fad_review(int ENTITY_ID = 0)
            {
            return dBConnection.GetEngagementDetailsForFadReview(ENTITY_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<EngagementObservationsForStatusReversalModel> get_observation_details_for_status_reversal(int ENG_ID = 0)
            {
            return dBConnection.GetObservationDetailsForStatusReversal(ENG_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<EngagementObservationsForStatusReversalModel> get_observation_details_for_report(int ENG_ID = 0)
            {
            return dBConnection.GetAuditDetailsFAD(ENG_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<FADAuditParasReviewModel> get_observation_details_for_fad(int OBS_ID = 0)
            {
            return dBConnection.GetObservationDetailsForReport(OBS_ID);

            }
        [HttpGet]
        [HttpPost]
        public string get_compliance_text_auditee(int COMPLIANCE_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.GetComplianceTextAuditee(COMPLIANCE_ID) + "\"}";

            }

        [HttpGet]
        [HttpPost]
        public List<PostComplianceHistoryModel> get_compliance_history(string COM_ID)
            {
            return dBConnection.GetComplianceHistory(COM_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<PostComplianceHistoryModel> get_settled_para_compliance_history(string COM_ID)
            {
            return dBConnection.GetSettledParaComplianceHistory(COM_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<RiskProcessDefinition> get_violation_area_for_functional_responsibility_wise_paras(int FUNCTIONAL_ENTITY_ID = 0)
            {
            return dBConnection.GetViolationListForDashboard(FUNCTIONAL_ENTITY_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<RiskProcessDefinition> get_sub_violation_area_for_functional_responsibility_wise_paras(int FUNCTIONAL_ENTITY_ID = 0, int PROCESS_ID = 0)
            {
            return dBConnection.GetSubViolationListForDashboard(FUNCTIONAL_ENTITY_ID, PROCESS_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<RiskProcessDefinition> get_functional_owner_area_for_functional_responsibility_wise_paras_ho(int ENTITY_ID = 0)
            {
            return dBConnection.GetHOFunctionalListForDashboard(ENTITY_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<RiskProcessDefinition> get_violation_area_for_functional_responsibility_wise_paras_ho(int FUNCTIONAL_ENTITY_ID = 0)
            {
            return archiveDbConnection.GetHOViolationListForDashboard(FUNCTIONAL_ENTITY_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<RiskProcessDefinition> get_sub_violation_area_for_functional_responsibility_wise_paras_ho(int FUNCTIONAL_ENTITY_ID = 0, int PROCESS_ID = 0)
            {
            return archiveDbConnection.GetHOSubViolationListForDashboard(FUNCTIONAL_ENTITY_ID, PROCESS_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<ObservationReversalModel> get_auditee_engagement_plan(int ENTITY_ID, int PERIOD)
            {
            return dBConnection.GetAuditeeEngagements(ENTITY_ID, PERIOD);

            }
        [HttpGet]
        [HttpPost]
        public List<AuditeeRiskModel> get_auditee_risk(int ENG_ID)
            {
            return dBConnection.GetAuditeeRisk(ENG_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<RiskAssessmentEntTypeModel> get_auditee_risk_for_entity_types(int ENT_TYPE_ID = 0, int PERIOD = 0)
            {
            return dBConnection.GetAuditeeRiskForEntTypes(ENT_TYPE_ID, PERIOD);

            }

        [HttpGet]
        [HttpPost]
        public List<AuditeeRiskModeldetails> get_auditee_risk_details(int ENG_ID)
            {
            return dBConnection.GetAuditeeRiskDetails(ENG_ID);

            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_NEW_USER", "ADMINISTRATION", "ADMINISTRATION", "", "", ObjectType = "ADD_NEW_USER", RequireResultMessage = true)]
        public string add_new_user(FindUserModel user)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddNewUser(user) + "\"}";

            }


        #region BAC API CALLS

        [HttpGet]
        [HttpPost]
        public List<BACAgendaModel> get_bac_agenda(int MEETING_NO)
            {
            return dBConnection.GetBACAgenda(MEETING_NO);

            }
        [HttpGet]
        [HttpPost]
        public List<BACAgendaModel> get_bac_meeting_summary(int MEETING_NO)
            {
            return dBConnection.GetBACAMeetingSummary(MEETING_NO);

            }

        [HttpGet]
        [HttpPost]
        public List<BACAgendaActionablesSummaryModel> get_bac_agenda_actionables_consolidate_summary()
            {
            return dBConnection.GetBACAgendaActionablesConsolidatedSummary();
            }
        [HttpGet]
        [HttpPost]
        public List<BACAgendaActionablesSummaryModel> get_bac_agenda_actionables_summary()
            {
            return dBConnection.GetBACAgendaActionablesSummary();
            }

        [HttpGet]
        [HttpPost]
        public List<BACAgendaActionablesModel> get_bac_agenda_actionables(string STATUS)
            {
            return dBConnection.GetBACAgendaActionables(STATUS);

            }
        [HttpGet]
        [HttpPost]
        public List<BACAgendaActionablesModel> get_bac_agenda_actionables_meeting_no(string STATUS, string MEETING_NO)
            {
            return dBConnection.GetBACAgendaActionablesWithMeetingNo(STATUS, MEETING_NO);

            }
        [HttpGet]
        [HttpPost]
        public List<BACCIAAnalysisModel> get_bac_analysis(int PROCESS_ID)
            {
            return dBConnection.GetBACCIAAnalysis(PROCESS_ID);

            }

        [HttpPost]
        public IActionResult get_bac_analysis_report(string FROM_DATE, string TO_DATE, int RISK_ID = 0)
            {
            if (!DateTime.TryParseExact(FROM_DATE, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
                return BadRequest(new { message = "From Date is mandatory." });
            if (!DateTime.TryParseExact(TO_DATE, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                return BadRequest(new { message = "To Date is mandatory." });
            if (fromDate > toDate)
                return BadRequest(new { message = "From Date cannot be greater than To Date." });

            return Ok(dBConnection.GetBACAnalysis(fromDate, toDate, RISK_ID));
            }

        [HttpPost]
        public IActionResult get_bac_analysis_detail(string FROM_DATE, string TO_DATE, int ANNEX_ID, int RISK_ID = 0)
            {
            if (!DateTime.TryParseExact(FROM_DATE, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
                return BadRequest(new { message = "From Date is mandatory." });
            if (!DateTime.TryParseExact(TO_DATE, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                return BadRequest(new { message = "To Date is mandatory." });
            if (fromDate > toDate)
                return BadRequest(new { message = "From Date cannot be greater than To Date." });
            if (ANNEX_ID <= 0)
                return BadRequest(new { message = "Annexure is required." });

            return Ok(dBConnection.GetBACAnalysisDetail(fromDate, toDate, ANNEX_ID, RISK_ID));
            }

        [HttpPost]
        public IActionResult get_bac_para_text(int OBSERVATION_ID)
            {
            if (OBSERVATION_ID <= 0)
                return BadRequest(new { message = "Observation is required." });

            var para = dBConnection.GetBACParaText(OBSERVATION_ID);
            if (para == null)
                return NotFound(new { message = "Para text was not found." });

            var validationContext = new ValidationContext(para) { MemberName = nameof(BACParaTextModel.PARA_TEXT) };
            Validator.TryValidateProperty(para.PARA_TEXT, validationContext, new List<ValidationResult>());
            return Ok(para);
            }

        [HttpPost]
        public IActionResult get_bac_dsa_details(int OBSERVATION_ID)
            {
            if (OBSERVATION_ID <= 0)
                return BadRequest(new { message = "Observation is required." });

            var details = dBConnection.GetBACDSADetails(OBSERVATION_ID);
            foreach (var detail in details)
                {
                var validationContext = new ValidationContext(detail) { MemberName = nameof(BACDSADetailModel.DSA_TEXT) };
                Validator.TryValidateProperty(detail.DSA_TEXT, validationContext, new List<ValidationResult>());
                }
            return Ok(details);
            }
        #endregion
        [HttpGet]
        [HttpPost]
        public List<EntityWiseObservationModel> get_reporting_wise_observations()
            {
            return archiveDbConnection.GetReportingOfficeWiseObservations();

            }

        [HttpGet]
        [HttpPost]
        public List<EntityWiseObservationModel> get_entity_wise_observations()
            {
            return dBConnection.GetEntityWiseObservations();
            }

        [HttpGet]
        [HttpPost]
        public List<AnnexWiseObservationModel> get_annex_wise_observations(string P_REF_DATE)
            {
            return dBConnection.GetAnnexureWiseObservations(P_REF_DATE);

            }
        [HttpGet]
        [HttpPost]
        public List<FunctionalAnnexureWiseObservationModel> get_entity_wise_observation_detail(int ENTITY_ID)
            {
            return dBConnection.GetEntityWiseObservationDetail(ENTITY_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<FunctionalAnnexureWiseObservationModel> get_functional_observations(int ANNEX_ID, int ENTITY_ID)
            {
            return archiveDbConnection.GetFunctionalObservations(ANNEX_ID, ENTITY_ID);

            }
        [HttpGet]
        [HttpPost]
        public string get_functional_observation_text(int PARA_ID, string PARA_CATEGORY)
            {
            return dBConnection.GetFunctionalObservationText(PARA_ID, PARA_CATEGORY);

            }

        [HttpGet]
        [HttpPost]
        public List<FunctionalAnnexureWiseObservationModel> get_analysis_detail_paras(int PROCESS_ID)
            {
            return dBConnection.GetAnalysisDetailPara(PROCESS_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<FunctionalAnnexureWiseObservationModel> get_functional_resp_detail_paras(int PROCESS_ID)
            {
            return dBConnection.GetFunctionalRespDetailPara(PROCESS_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<FunctionalAnnexureWiseObservationModel> get_analysis_summary_paras(int PROCESS_ID)
            {
            return dBConnection.GetAnalysisSummaryPara(PROCESS_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<FunctionalAnnexureWiseObservationModel> get_functional_resp_summary_paras(int PROCESS_ID)
            {
            return dBConnection.GetFunctionalRespSummaryPara(PROCESS_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<BranchModel> get_zone_Branches_for_Annexure_Assignment(int ENTITY_ID)
            {
            return dBConnection.GetZoneBranchesForAnnexureAssignment(ENTITY_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<AllParaForAnnexureAssignmentModel> get_all_paras_for_annexure_assignment(int ENTITY_ID)
            {
            return dBConnection.GetAllParasForAnnexureAssignment(ENTITY_ID);
            }
        [HttpPost]
        [ApplicationAudit("ANNEXURE_ASSIGNED_TO_PARA", "COMPLIANCE", "PARA MANAGEMENT", "PKG_FAD", "P_UPDATE_PARAS_ANNEX_FAD", ParaId = "OBS_ID", ObjectType = "PARA", ObjectId = "OBS_ID", RequireResultMessage = true)]
        public string assign_annexure_with_para(string OBS_ID, string REF_P, string ANNEX_ID, string PARA_CATEGORY)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AssignAnnexureWithPara(OBS_ID, REF_P, ANNEX_ID, PARA_CATEGORY) + "\"}";
            }

        [HttpPost]
        public string merge_duplicate_process(string MAIN_PROCESS_ID, List<string> MERGE_PROCESS_IDs)
            {
            string resp = "";
            foreach (string ID in MERGE_PROCESS_IDs)
                {
                resp += dBConnection.MergeDuplicateProcesses(MAIN_PROCESS_ID, ID) + "</br>";
                }
            return "{\"Status\":true,\"Message\":\"" + resp + "\"}";

            }

        [HttpPost]
        public string merge_duplicate_sub_process(string MAIN_PROCESS_ID, string MAIN_SUB_PROCESS_ID, List<string> MERGE_SUB_PROCESS_IDs)
            {
            string resp = "";
            foreach (string ID in MERGE_SUB_PROCESS_IDs)
                {
                resp += dBConnection.MergeDuplicateSubProcesses(MAIN_PROCESS_ID, MAIN_SUB_PROCESS_ID, ID) + "</br>";
                }
            return "{\"Status\":true,\"Message\":\"" + resp + "\"}";

            }
        [HttpGet]
        [HttpPost]
        public List<AISPostComplianceModel> get_ais_post_compliance_details(int ENT)
            {
            return dBConnection.GetAisPostComplianceDetails(ENT);
            }

        [HttpPost]
        [ApplicationAudit("AIS_POST_COMPLIANCE_UPDATED", "COMPLIANCE", "POST AUDIT COMPLIANCE", "PKG_AD", "P_UPDATE_PARA_AIS_POST_COMPLIANCE", ParaId = "model.PARA_ID", ComId = "model.COM_ID", ObjectType = "PARA", RequireResultMessage = true)]
        public string update_ais_post_compliance(AISPostComplianceModel model)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAisPostCompliance(model) + "\"}";
            }

        [HttpPost]
        public string merge_duplicate_checklists(string MAIN_CHECKLIST_ID, List<string> MERGE_CHECKLIST_IDs)
            {
            foreach (string ID in MERGE_CHECKLIST_IDs)
                {
                dBConnection.MergeDuplicateChecklists(MAIN_CHECKLIST_ID, ID);
                }
            return "{\"Status\":true,\"Message\":\"Duplicates merged successfully\"}";

            }

        [HttpGet]
        [HttpPost]
        public List<MergeDuplicateProcessModel> get_duplicate_Processes(int PROCESS_ID)
            {
            return dBConnection.GetDuplicateProcesses(PROCESS_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<MergeDuplicateProcessModel> get_duplicate_Sub_Processes(int SUB_PROCESS_ID)
            {
            return dBConnection.GetDuplicateSubProcesses(SUB_PROCESS_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<MergeDuplicateChecklistModel> get_duplicate_checklists(int PROCESS_ID)
            {
            return dBConnection.GetDuplicateChecklists(PROCESS_ID);
            }
        [HttpGet]
        [HttpPost]
        public MergeDuplicateChecklistModel get_duplicate_checklists_count(int PROCESS_ID)
            {
            return dBConnection.GetDuplicateChecklistsCount(PROCESS_ID);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("AUTHORIZE_MERGE_DUPLICATE_PROCESS", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_AUTHORIZE_MERGER_CHECKLIST", ObjectType = "AUTHORIZE_MERGE_DUPLICATE_PROCESS", RequireResultMessage = true, RequireNonEmpty = "AUTH_P_IDS")]
        public string authorize_merge_duplicate_process(int PROCESS_ID, List<int> AUTH_P_IDS)
            {
            string resp = "";
            foreach (int ID in AUTH_P_IDS)
                {
                resp += dBConnection.AuthorizeMergeDuplicateProcesses(PROCESS_ID, ID) + "</br>";
                }
            return "{\"Status\":true,\"Message\":\"" + resp + "\"}";

            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("AUTHORIZE_MERGE_DUPLICATE_SUB_PROCESS", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_AUTHORIZE_MERGER_CHECKLIST_SUB", ObjectType = "AUTHORIZE_MERGE_DUPLICATE_SUB_PROCESS", RequireResultMessage = true, RequireNonEmpty = "AUTH_S_P_IDS")]
        public string authorize_merge_duplicate_sub_process(int SUB_PROCESS_ID, List<int> AUTH_S_P_IDS)
            {
            string resp = "";
            foreach (int ID in AUTH_S_P_IDS)
                {
                resp += dBConnection.AuthorizeMergeDuplicateSubProcesses(SUB_PROCESS_ID, ID) + "</br>";
                }
            return "{\"Status\":true,\"Message\":\"" + resp + "\"}";

            }
        [HttpPost]
        [ApplicationAudit("DUPLICATE_CHECKLIST_MERGE_AUTHORIZED", "CHECKLIST", "CHECKLIST_ADMINISTRATION", "PKG_AD", "P_AUTHORIZE_DUPLICATE_CHECKLIST_DETAILS", ObjectType = "AUDIT_CHECKLIST", ObjectId = "PROCESS_ID", RequireResultMessage = true)]
        public string authorize_merge_duplicate_checklists(int PROCESS_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeMergeDuplicateChecklists(PROCESS_ID) + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("PARAS_SHIFTED_TO_ENTITY", "COMPLIANCE", "PARA SHIFTING", "PKG_FAD", "P_PARA_SHIFTING", ObjectType = "ENTITY", ObjectId = "NEW_ENT_ID", RequireNonEmpty = "OBS_IDS", RequireResultMessage = true)]
        public string Para_Shifted_To(List<int> OBS_IDS, int NEW_ENT_ID, int OLD_ENT_ID, List<string> P_INDS)
            {
            string resp = "";
            for (int i = 0; i < OBS_IDS.Count; i++)
                {
                string pInd = P_INDS.Count > i ? P_INDS[i] : string.Empty;
                resp += dBConnection.ParaShiftedTo(OBS_IDS[i], NEW_ENT_ID, OLD_ENT_ID, pInd) + "<br />";
                }
            return "{\"Status\":true,\"Message\":\"" + resp + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<SettledParasMonitoringModel> get_settled_paras_for_monitoring(int ENTITY_ID)
            {
            return dBConnection.GetSettledParasForMonitoring(ENTITY_ID);
            }
        [HttpPost]
        public List<AdminNewUsersAIS> admin_get_new_users()
            {
            return dBConnection.AdminNewUsersInAIS();
            }

        [HttpGet]
        [HttpPost]
        public List<AuditParaReconsillation> get_audit_para_reconsillation()
            {
            return dBConnection.GetAuditParaRensillation();
            }
        [HttpGet]
        [HttpPost]
        public List<HREntitiesModel> get_hr_entities_for_admin_panel_entity_addition(string ENTITY_NAME, string ENTITY_CODE)
            {
            return dBConnection.GetHREntitiesForAdminPanelEntityAddition(ENTITY_NAME, ENTITY_CODE);
            }

        [HttpGet]
        [HttpPost]
        public List<AISEntitiesModel> get_ais_entities_for_admin_panel_entity_addition(string ENTITY_NAME, string ENTITY_CODE, int ENT_TYPE_ID = 0)
            {
            return dBConnection.GetAISEntitiesForAdminPanelEntityAddition(ENTITY_NAME, ENTITY_CODE, ENT_TYPE_ID);
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_AIS_ENTITY_FOR_ADMIN_PANEL_ENTITY_ADDITION", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATEENTITIEES", ObjectType = "UPDATE_AIS_ENTITY_FOR_ADMIN_PANEL_ENTITY_ADDITION", ObjectId = "ENTITY_ID", RequireResultMessage = true)]
        public string update_ais_entity_for_admin_panel_entity_addition(string ENTITY_ID, string ENTITY_NAME, string ENTITY_CODE, string AUDITABLE, string AUDIT_BY_ID, string ENTITY_TYPE_ID, string ENT_DESC, string STATUS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAISEntityForAdminPanelEntityAddition(ENTITY_ID, ENTITY_NAME, ENTITY_CODE, AUDITABLE, AUDIT_BY_ID, ENTITY_TYPE_ID, ENT_DESC, STATUS) + "\"}";

            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_AIS_ENTITY_FOR_ADMIN_PANEL_ENTITY_ADDITION", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_INSERTENTITIEES", ObjectType = "ADD_AIS_ENTITY_FOR_ADMIN_PANEL_ENTITY_ADDITION", RequireResultMessage = true)]
        public string add_ais_entity_for_admin_panel_entity_addition(string ENTITY_NAME, string ENTITY_CODE, string AUDITABLE, string AUDIT_BY_ID, string ENTITY_TYPE_ID, string ENT_DESC, string STATUS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAISEntityForAdminPanelEntityAddition(ENTITY_NAME, ENTITY_CODE, AUDITABLE, AUDIT_BY_ID, ENTITY_TYPE_ID, ENT_DESC, STATUS) + "\"}";

            }

        [HttpGet]
        [HttpPost]
        public List<EntityMappingForEntityAddition> get_ais_entity_existing_mapping_for_admin_panel_entity_addition(string ENTITY_ID)
            {
            return dBConnection.GetAISEntityMappingForAdminPanelEntityAddition(ENTITY_ID);

            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_AIS_ENTITY_MAPPING_FOR_ADMIN_PANEL_ENTITY_ADDITION", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_ADD_ENTITIES_MAPPING", ObjectType = "ADD_AIS_ENTITY_MAPPING_FOR_ADMIN_PANEL_ENTITY_ADDITION", ObjectId = "ENTITY_ID", RequireResultMessage = true)]
        public string add_ais_entity_mapping_for_admin_panel_entity_addition(string P_ENTITY_ID, string ENTITY_ID, string RELATION_TYPE_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAISEntityMappingForAdminPanelEntityAddition(P_ENTITY_ID, ENTITY_ID, RELATION_TYPE_ID) + "\"}";

            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_AIS_ENTITY_MAPPING_FOR_ADMIN_PANEL_ENTITY_ADDITION", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATE_ENTITIES_MAPPING", ObjectType = "UPDATE_AIS_ENTITY_MAPPING_FOR_ADMIN_PANEL_ENTITY_ADDITION", ObjectId = "ENTITY_ID", RequireResultMessage = true)]
        public string update_ais_entity_mapping_for_admin_panel_entity_addition(string P_ENTITY_ID, string ENTITY_ID, string RELATION_TYPE_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAISEntityMappingForAdminPanelEntityAddition(P_ENTITY_ID, ENTITY_ID, RELATION_TYPE_ID) + "\"}";

            }


        [HttpGet]
        [HttpPost]
        public List<LoanCaseFileDetailsModel> Get_Working_Paper_Loan_Cases(string ENGID)
            {
            return dBConnection.GetWorkingPaperLoanCases(ENGID);

            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_WORKING_PAPER_LOAN_CASES", "ADMINISTRATION", "ADMINISTRATION", "PKG_AR", "P_ADDLOANCASEFILE", EngagementId = "ENGID", ObjectType = "ADD_WORKING_PAPER_LOAN_CASES", RequireResultMessage = true)]
        public string Add_Working_Paper_Loan_Cases(string ENGID, string LCNUMBER, string LCAMOUNT, DateTime DISBDATE, string LCAT, string OBS, string PARA_NO)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddWorkingPaperLoanCases(ENGID, LCNUMBER, LCAMOUNT, DISBDATE, LCAT, OBS, PARA_NO) + "\"}";

            }

        [HttpGet]
        [HttpPost]
        public List<VoucherCheckingDetailsModel> Get_Working_Paper_Voucher_Checking(string ENGID)
            {
            return dBConnection.GetWorkingPaperVoucherChecking(ENGID);

            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_WORKING_PAPER_VOUCHER_CHECKING", "ADMINISTRATION", "ADMINISTRATION", "PKG_AR", "P_ADDVOUCHERCHECKING", EngagementId = "ENGID", ObjectType = "ADD_WORKING_PAPER_VOUCHER_CHECKING", RequireResultMessage = true)]
        public string Add_Working_Paper_Voucher_Checking(string ENGID, string VNUMBER, string OBS, string PARA_NO)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddWorkingVoucherChecking(ENGID, VNUMBER, OBS, PARA_NO) + "\"}";

            }
        [HttpGet]
        [HttpPost]
        public List<AccountOpeningDetailsModel> Get_Working_Paper_Account_Opening(string ENGID)
            {
            return dBConnection.GetWorkingPaperAccountOpening(ENGID);

            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_WORKING_PAPER_ACCOUNT_OPENING", "ADMINISTRATION", "ADMINISTRATION", "PKG_AR", "P_ADDACCOUNTOPENINGDETAILS", EngagementId = "ENGID", ObjectType = "ADD_WORKING_PAPER_ACCOUNT_OPENING", RequireResultMessage = true)]
        public string Add_Working_Paper_Account_Opening(string ENGID, string VNUMBER, string ANATURE, string OBS, string PARA_NO)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddWorkingAccountOpening(ENGID, VNUMBER, ANATURE, OBS, PARA_NO) + "\"}";

            }

        [HttpGet]
        [HttpPost]
        public List<FixedAssetsDetailsModel> Get_Working_Paper_Fixed_Assets(string ENGID)
            {
            return dBConnection.GetWorkingPaperFixedAssets(ENGID);

            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_WORKING_PAPER_FIXED_ASSETS", "ADMINISTRATION", "ADMINISTRATION", "PKG_AR", "P_ADDFIXEDASSETSDETAILS", EngagementId = "ENGID", ObjectType = "ADD_WORKING_PAPER_FIXED_ASSETS", RequireResultMessage = true)]
        public string Add_Working_Paper_Fixed_Assets(string ENGID, string A_NAME, string PHY_EX, string FAR, string DIFF, string REM)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddWorkingFixedAssets(ENGID, A_NAME, PHY_EX, FAR, DIFF, REM) + "\"}";

            }

        [HttpGet]
        [HttpPost]
        public List<CashCountDetailsModel> Get_Working_Paper_Cash_Counter(string ENGID)
            {
            return dBConnection.GetWorkingPaperCashCounter(ENGID);

            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_WORKING_PAPER_CASH_COUNTER", "ADMINISTRATION", "ADMINISTRATION", "PKG_AR", "P_ADDCASHCOUNTERDETAILS", EngagementId = "ENGID", ObjectType = "ADD_WORKING_PAPER_CASH_COUNTER", RequireResultMessage = true)]
        public string Add_Working_Paper_Cash_Counter(string ENGID, string DVAULT, string NOVAULT, string TOTVAULT, string DSR, string NOSR, string TOTSR, string DIFF)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddWorkingCashCounter(ENGID, DVAULT, NOVAULT, TOTVAULT, DSR, NOSR, TOTSR, DIFF) + "\"}";

            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_NEW_USER_ADMIN_PANEL", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATE_NEW_USER", ObjectType = "UPDATE_NEW_USER_ADMIN_PANEL", RequireResultMessage = true, RequireNonEmpty = "PPNOArr")]
        public string update_new_user_admin_panel(List<int> PPNOArr)
            {
            string resp = "";
            foreach (int ppno in PPNOArr)
                {
                resp = dBConnection.UpdateNewUsersAdminPanel(ppno);
                }

            return "{\"Status\":true,\"Message\":\"" + resp + "\"}";
            }

        [HttpPost]
        public List<UserRoleDetailAdminPanelModel> admin_get_user_details(string DESIGNATION_CODE)
            {
            return dBConnection.GetUserDetailAdminPanel(DESIGNATION_CODE);
            }
        [HttpGet]
        [HttpPost]
        public List<ComplianceSummaryModel> get_compliance_summary(int ENTITY_ID)
            {
            return dBConnection.GetComplianceSummary(ENTITY_ID);
            }

        [HttpPost]
        public IActionResult get_head_observation_risk_summary(string cycleBucket)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (!IsAuthorizedHeadRole(loggedInUser))
                return Forbid();

            if (!string.Equals(cycleBucket, "OVER_THREE", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(cycleBucket, "ZERO", StringComparison.OrdinalIgnoreCase))
                {
                return BadRequest("Invalid compliance cycle filter.");
                }

            return Ok(dBConnection.GetHeadObservationRiskSummary(cycleBucket));
            }

        [HttpPost]
        public IActionResult get_head_observation_risk_details(int departmentId, string cycleBucket)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (!IsAuthorizedHeadRole(loggedInUser))
                return Forbid();

            if (departmentId <= 0
                || (!string.Equals(cycleBucket, "OVER_THREE", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(cycleBucket, "ZERO", StringComparison.OrdinalIgnoreCase)))
                {
                return BadRequest("Invalid department para detail request.");
                }

            return Ok(dBConnection.GetHeadObservationRiskDetails(departmentId, cycleBucket));
            }

        private static bool IsAuthorizedHeadRole(SessionUser user)
            {
            if (user == null || user.UserEntityID.GetValueOrDefault() <= 0)
                return false;

            return user.UserRoleID == 1
                || user.UserRoleID == 3
                || user.UserRoleID == 14;
            }

        [HttpGet]
        [HttpPost]
        public List<EntitiesShiftingDetailsModel> get_entity_shifting_details(string ENTITY_ID = "")
            {
            return dBConnection.GetEntityShiftingDetails(ENTITY_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<AuditEntitiesModel> get_entity_types()
            {
            return dBConnection.GetEntityTypes();
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_ENTITY_TYPES", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATE_ENTITIES_TYPES", ObjectType = "UPDATE_ENTITY_TYPES", RequireResultMessage = true)]
        public string update_entity_types(AuditEntitiesModel ENTITY_MODEL)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateEntityTypes(ENTITY_MODEL) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<AuditEntityRelationsModel> get_entity_relations()
            {
            return dBConnection.GetEntityRelations();
            }
        [HttpGet]
        [HttpPost]
        public List<EntitiesMappingModel> get_entities_mapping(string ENT_ID, string P_TYPE, string C_TYPE, string RELATION_TYPE, string IND)
            {
            return dBConnection.GetEntitiesMapping(ENT_ID, P_TYPE, C_TYPE, RELATION_TYPE, IND);
            }
        [HttpGet]
        [HttpPost]
        public List<EntitiesMappingModel> get_entities_mapping_reporting(string ENT_ID, string P_TYPE, string C_TYPE, string RELATION_TYPE, string IND)
            {
            return dBConnection.GetEntitiesMappingReporting(ENT_ID, P_TYPE, C_TYPE, RELATION_TYPE, IND);
            }
        [HttpGet]
        [HttpPost]
        public List<EntitiesMappingModel> get_entities_of_parent_child(string P_TYPE_ID, string C_TYPE_ID)
            {
            return dBConnection.GetParentChildEntities(P_TYPE_ID, C_TYPE_ID);
            }
        [HttpPost]
        [ApplicationAudit("ENTITY_SHIFTED", "ADMINISTRATION", "Administration", "pkg_ad", "P_Add_Entity_shifting", ObjectType = "ENTITY", ObjectId = "FROM_ENT_ID", RequireResultMessage = true)]
        public string submit_entity_shifting_from_admin_panel(string FROM_ENT_ID, string TO_ENT_ID, string CIR_REF, DateTime CIR_DATE, string CIR)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitEntityShiftingFromAdminPanel(FROM_ENT_ID, TO_ENT_ID, CIR_REF, CIR_DATE, CIR) + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("DEPARTMENT_ENTITY_SHIFTED", "ADMINISTRATION", "Administration", "pkg_ad", "P_Add_Department_Entity_Shifting", ObjectType = "ENTITY", ObjectId = "FROM_ENT_ID", RequireResultMessage = true, FailureMessageContains = "Invalid")]
        public string submit_department_entity_shifting_from_admin_panel(string FROM_ENT_ID, string TO_ENT_ID, string CIR_REF, DateTime CIR_DATE, string CIR)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitDepartmentEntityShiftingFromAdminPanel(FROM_ENT_ID, TO_ENT_ID, CIR_REF, CIR_DATE, CIR) + "\"}";
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("SUBMIT_ENTITY_CONV_TO_ISLAMIC_FROM_ADMIN_PANEL", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_SHIFT_BR_TO_ISLAMIC", ObjectType = "SUBMIT_ENTITY_CONV_TO_ISLAMIC_FROM_ADMIN_PANEL", RequireResultMessage = true)]
        public string submit_entity_conv_to_islamic_from_admin_panel(string FROM_ENT_ID, string TO_ENT_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitEntityConvToIslamicFromAdminPanel(FROM_ENT_ID, TO_ENT_ID) + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("COMPLIANCE_FLOW_SAVED", "COMPLIANCE", "COMPLIANCE SETUP", "PKG_AD", "P_ADD_UPDATE_COMPLIANCE_FLOW", ObjectType = "COMPLIANCE_FLOW", ObjectId = "ID", RequireResultMessage = true)]
        public string add_compliance_flow(string ID, string ENTITY_TYPE_ID, string GROUP_ID, string PREV_GROUP_ID, string NEXT_GROUP_ID, string COMP_UP_STATUS, string COMP_DOWN_STATUS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddComplianceFlow(ID, ENTITY_TYPE_ID, GROUP_ID, PREV_GROUP_ID, NEXT_GROUP_ID, COMP_UP_STATUS, COMP_DOWN_STATUS) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<ComplianceFlowModel> get_compliance_flow_by_entity_type(int ENTITY_TYPE_ID = 0, int GROUP_ID = 0)
            {
            return dBConnection.GetComplianceFlowByEntityType(ENTITY_TYPE_ID, GROUP_ID);
            }


        [HttpGet]
        [HttpPost]
        public List<AuditTeamModel> get_team_memeber_details_for_post_changes_team_eng_reversal(int AUDITED_BY_DEPT, int CURRENT_TEAM_ID)
            {
            return dBConnection.GetAuditTeamsForEngagementReversal(AUDITED_BY_DEPT, CURRENT_TEAM_ID);
            }

        [HttpPost]
        [ApplicationAudit("ENGAGEMENT_TEAM_CHANGED", "AUDIT_PLANNING", "ENGAGEMENT_PLANNING", "PKG_AD", "P_AUDIT_TEAM_POSTCHANGES", EngagementId = "ENG_ID", ObjectType = "AUDIT_TEAM", ObjectId = "TEAM_ID", RequireResultMessage = true)]
        public string submit_new_team_id_for_post_changes_team_eng_reversal(int TEAM_ID, int ENG_ID, int AUDITED_BY_ID, string TEAM_NAME)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitNewTeamIdForPostChangesTeamEngReversal(TEAM_ID, ENG_ID, AUDITED_BY_ID, TEAM_NAME) + "\"}";
            }

        [HttpPost]
        public string audit_engagement_status_reversal(int ENG_ID, int NEW_STATUS_ID, int PLAN_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuditEngagementStatusReversal(ENG_ID, NEW_STATUS_ID, PLAN_ID, COMMENTS) + "\"}";
            }
        [HttpPost]
        public string audit_engagement_obs_status_reversal(int ENG_ID, int NEW_STATUS_ID, List<int> OBS_IDS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuditEngagementObsStatusReversal(ENG_ID, NEW_STATUS_ID, OBS_IDS) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<ObservationNumbersModel> get_observation_numbers_for_status_reversal(int OBS_ID)
            {
            return dBConnection.GetObservationNumbersForStatusReversal(OBS_ID);
            }

        [HttpPost]
        [ApplicationAudit("OBSERVATION_NUMBER_UPDATED", "AUDIT_EXECUTION", "OBSERVATION", "PKG_AD", "P_UPDATE_OBSERVATION_NO", ObjectType = "OBSERVATION", RequireResultMessage = true)]
        public string update_observation_numbers_for_status_reversal(ObservationNumbersModel onum)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateObservationNumbersForStatusReversal(onum) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("ENGAGEMENT_DATES_CHANGED", "AUDIT_PLANNING", "ENGAGEMENT_PLANNING", "PKG_AD", "P_UPDATE_ENG_DATE", EngagementId = "ENG_ID", ObjectType = "ENGAGEMENT", ObjectId = "ENG_ID", RequireResultMessage = true)]
        public string update_engagement_dates_for_status_reversal(int ENG_ID, DateTime START_DATE, DateTime END_DATE)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateEngagementDatesForStatusReversal(ENG_ID, START_DATE, END_DATE) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<HRDesignationWiseRoleModel> get_hr_designation_wise_roles()
            {
            return dBConnection.GetHRDesignationWiseRoles();
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_HR_DESIGNATION_WISE_ROLE_ASSIGNMENT", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_ADD_HR_DESIGNATION_RIGHT", ObjectType = "ADD_HR_DESIGNATION_WISE_ROLE_ASSIGNMENT", ObjectId = "ASSIGNMENT_ID", RequireResultMessage = true)]
        public string add_hr_designation_wise_role_assignment(int ASSIGNMENT_ID, int DESIGNATION_ID, int GROUP_ID, string SUB_ENTITY_NAME)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddHRDesignationWiseRoleAssignment(ASSIGNMENT_ID, DESIGNATION_ID, GROUP_ID, SUB_ENTITY_NAME) + "\"}";
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_HR_DESIGNATION_WISE_ROLE_ASSIGNMENT", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATE_HR_DESIGNATION_RIGHT", ObjectType = "UPDATE_HR_DESIGNATION_WISE_ROLE_ASSIGNMENT", ObjectId = "ASSIGNMENT_ID", RequireResultMessage = true)]
        public string update_hr_designation_wise_role_assignment(int ASSIGNMENT_ID, int DESIGNATION_ID, int GROUP_ID, string SUB_ENTITY_NAME)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateHRDesignationWiseRoleAssignment(ASSIGNMENT_ID, DESIGNATION_ID, GROUP_ID, SUB_ENTITY_NAME) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<ManageObservationModel> get_maange_obs_status()
            {
            return dBConnection.GetManageObservationStatus();
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_MANAGE_OBSERVATITON_STATUS", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_ADD_OBS_STATUS", ObjectType = "ADD_MANAGE_OBSERVATITON_STATUS", RequireResultMessage = true)]
        public string add_manage_observatiton_status(ManageObservationModel OBS_STATUS_MODEL)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddManageObservationStatus(OBS_STATUS_MODEL) + "\"}";
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_MANAGE_OBSERVATITON_STATUS", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATE_OBS_STATUS", ObjectType = "UPDATE_MANAGE_OBSERVATITON_STATUS", RequireResultMessage = true)]
        public string update_manage_observatiton_status(ManageObservationModel OBS_STATUS_MODEL)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateManageObservationStatus(OBS_STATUS_MODEL) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<ManageEntAuditDeptModel> get_manage_ent_audit_dept()
            {
            return dBConnection.GetManageEntityAuditDept();
            }
        [HttpPost]
        [ApplicationAudit("ENTITY_AUDIT_DEPARTMENT_ADDED", "AUDIT_EXECUTION", "AUDIT_UNIVERSE", "PKG_AD", "P_ADD_ENTITIES_AUDIT_DEPARTMENT", ObjectType = "ENTITY_AUDIT_DEPARTMENT", ObjectId = "ENT_AUD_DEPT_MODEL.ENT_ID", RequireResultMessage = true)]
        public string add_manage_entities_audit_department(ManageEntAuditDeptModel ENT_AUD_DEPT_MODEL)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddManageEntityAuditDepartment(ENT_AUD_DEPT_MODEL) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("ENTITY_AUDIT_DEPARTMENT_UPDATED", "AUDIT_EXECUTION", "AUDIT_UNIVERSE", "PKG_AD", "P_UPDATE_ENTITIES_AUDIT_DEPARTMENT", ObjectType = "ENTITY_AUDIT_DEPARTMENT", ObjectId = "ENT_AUD_DEPT_MODEL.ENT_ID", RequireResultMessage = true)]
        public string update_manage_entities_audit_department(ManageEntAuditDeptModel ENT_AUD_DEPT_MODEL)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateEntityAuditDepartment(ENT_AUD_DEPT_MODEL) + "\"}";
            }



        [HttpGet]
        [HttpPost]
        public List<AuditeeEntitiesModel> get_region_zone_office(int RGM_ID)
            {
            return dBConnection.GetRBHList(RGM_ID);
            }

        [HttpPost]
        public List<AuditPeriodModel> audit_periods(int dept_code = 0, int AUDIT_PERIOD_ID = 0)
            {
            return dBConnection.GetAuditPeriods(dept_code, AUDIT_PERIOD_ID);
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_PERIOD_CREATED", "AUDIT_EXECUTION", "AUDIT_PERIOD", "PKG_PG", "P_ADDAUDITPERIOD", ObjectType = "AUDIT_PERIOD", RequireResultMessage = true)]
        public string add_audit_period(AddAuditPeriodModel auditPeriod)
            {
            if (!ModelState.IsValid)
                {
                return "{\"Status\":false,\"Message\":\"VALIDATION_ERROR\"}";
                }
            AuditPeriodModel apm = new AuditPeriodModel();
            apm.STATUS_ID = 1;
            apm.DESCRIPTION = auditPeriod.DESCRIPTION;
            apm.START_DATE = DateTime.ParseExact(auditPeriod.STARTDATE, "MM/dd/yyyy", null);
            apm.END_DATE = DateTime.ParseExact(auditPeriod.ENDDATE, "MM/dd/yyyy", null);
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAuditPeriod(apm) + "\"}";

            }

        [HttpPost]
        [ApplicationAudit("AUDIT_PERIOD_UPDATED", "AUDIT_EXECUTION", "AUDIT_PERIOD", "PKG_PG", "P_UPDATE_AUDITPERIOD", ObjectType = "AUDIT_PERIOD", ObjectId = "auditPeriod.ID", RequireResultMessage = true)]
        public string update_audit_period(AuditPeriodModel auditPeriod)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditPeriod(auditPeriod) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<SubMenuModel> get_sub_menu_for_admin_panel(int M_ID)
            {
            return dBConnection.GetSubMenusForAdminPanel(M_ID);
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_SUB_MENU_FOR_ADMIN_PANEL", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_ADD_NEW_SUB_MENU", ObjectType = "ADD_SUB_MENU_FOR_ADMIN_PANEL", RequireResultMessage = true)]
        public string add_sub_menu_for_admin_panel(SubMenuModel sm)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddSubMenuForAdminPanel(sm) + "\"}";
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_SUB_MENU_FOR_ADMIN_PANEL", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATE_SUB_MENU", ObjectType = "UPDATE_SUB_MENU_FOR_ADMIN_PANEL", RequireResultMessage = true)]
        public string update_sub_menu_for_admin_panel(SubMenuModel sm)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateSubMenuForAdminPanel(sm) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<MenuPagesAssignmentModel> get_menu_pages_for_admin_panel(int M_ID, int SM_ID)
            {
            return dBConnection.GetMenuPagesForAdminPanel(M_ID, SM_ID);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_MENU_PAGE_FOR_ADMIN_PANEL", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_ADD_NEW_PAGE", ObjectType = "ADD_MENU_PAGE_FOR_ADMIN_PANEL", RequireResultMessage = true)]
        public string add_menu_page_for_admin_panel(
            int M_ID = 0,
            int SM_ID = 0,
            string P_NAME = "",
            string P_KEY = "",
            string P_URL = "",
            string P_PATH = "",
            int P_ORDER = 0,
            string P_STATUS = "",
            int P_HIDE_MENU = 0)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddMenuPageForAdminPanel(M_ID, SM_ID, P_NAME, P_KEY, P_URL, P_PATH, P_ORDER, P_STATUS, P_HIDE_MENU) + "\"}";
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_MENU_PAGE_FOR_ADMIN_PANEL", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATE_PAGE", ObjectType = "UPDATE_MENU_PAGE_FOR_ADMIN_PANEL", RequireResultMessage = true)]
        public string update_menu_page_for_admin_panel(
            int P_ID = 0,
            int M_ID = 0,
            int SM_ID = 0,
            string P_NAME = "",
            string P_KEY = "",
            string P_URL = "",
            string P_PATH = "",
            int P_ORDER = 0,
            string P_STATUS = "",
            int P_HIDE_MENU = 0)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateMenuPageForAdminPanel(P_ID, M_ID, SM_ID, P_NAME, P_KEY, P_URL, P_PATH, P_ORDER, P_STATUS, P_HIDE_MENU) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<DraftDSAGuidelines> get_draft_dsa_guidelines()
            {
            return dBConnection.GetDraftDSAGuidelines();
            }
        [HttpPost]
        public string draft_dsa(int OBS_ID, List<string> RESP_LIST, List<string> GID_LIST, string DSA_CONTENT)
            {
            string resp = "";
            foreach (string PPNO in RESP_LIST)
                {
                List<Object> outResp = new List<object>();
                outResp = dBConnection.DraftDSA(OBS_ID, PPNO, DSA_CONTENT);
                resp += "<p>" + outResp[0].ToString() + "</p>";
                foreach (string GID in GID_LIST)
                    {
                    dBConnection.AddDraftDSAGuideline(outResp[1].ToString(), GID);
                    }

                }
            return "{\"Status\":true,\"Message\":\"" + resp + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("COMPLIANCE_OFFICE_UPDATED", "COMPLIANCE", "COMPLIANCE SETUP", "PKG_AD", "P_UPDATE_ENTITY_COMP", ComId = "COMP_ID", ObjectType = "AUDIT", ObjectId = "AUD_ID", RequireNonEmpty = "ENT_ID_ARR", RequireResultMessage = true)]
        public string update_compliance_office(List<int> ENT_ID_ARR, int AUD_ID, string COMP_ID)
            {
            string res = "";
            if (ENT_ID_ARR.Count > 0)
                {
                foreach (int ENT_ID in ENT_ID_ARR)
                    {
                    res = dBConnection.UpdateComplianceUnit(ENT_ID, AUD_ID, COMP_ID);
                    }
                }

            return "{\"Status\":true,\"Message\":\"" + res + "\"}";

            }
        [HttpGet]
        [HttpPost]
        public List<GISTWiseReportParas> get_report_para_by_gist_keyword(string GIST)
            {
            return dBConnection.GetAuditReportParaByGistKeyword(GIST);
            }
        [HttpGet]
        [HttpPost]
        public List<AnnexureModel> get_annexures()
            {
            return dBConnection.GetAnnexuresForChecklistDetail();
            }

        [HttpPost]
        [ApplicationAudit("ANNEXURE_ADDED", "ANNEXURE", "ANNEXURE SETUP", "PKG_AD", "P_ADD_ANNEXURE", ObjectType = "ANNEXURE", RequireResultMessage = true)]
        public string add_annexure(string ANNEX_CODE = "", int PROCESS_ID = 0, int FUNCTION_OWNER_ID = 0, int FUNCTION_ID_1 = 0, int FUNCTION_ID_2 = 0, string HEADING = "", int RISK_ID = 0, string MAX_NUMBER = "", string GRAVITY = "", string WEIGHTAGE = "")
            {
            if (!System.Text.RegularExpressions.Regex.IsMatch(ANNEX_CODE ?? string.Empty, @"^[A-Za-z0-9&]+$") ||
                !System.Text.RegularExpressions.Regex.IsMatch(HEADING ?? string.Empty, @"^[A-Za-z0-9 &]+$"))
                {
                return "{\"Status\":false,\"Message\":\"VALIDATION_ERROR\"}";
                }
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAnnexure(ANNEX_CODE, HEADING, PROCESS_ID, FUNCTION_OWNER_ID, FUNCTION_ID_1, FUNCTION_ID_2, RISK_ID, MAX_NUMBER, GRAVITY, WEIGHTAGE) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("ANNEXURE_UPDATED", "ANNEXURE", "ANNEXURE SETUP", "PKG_AD", "P_UPDATE_ANNEXURE", ObjectType = "ANNEXURE", ObjectId = "ANNEX_ID", RequireResultMessage = true)]
        public string update_annexure(int ANNEX_ID = 0, int PROCESS_ID = 0, int FUNCTION_OWNER_ID = 0, int FUNCTION_ID_1 = 0, int FUNCTION_ID_2 = 0, string HEADING = "", int RISK_ID = 0, string MAX_NUMBER = "", string GRAVITY = "", string WEIGHTAGE = "")
            {
            if (!System.Text.RegularExpressions.Regex.IsMatch(HEADING ?? string.Empty, @"^[A-Za-z0-9 &]+$"))
                {
                return "{\"Status\":false,\"Message\":\"VALIDATION_ERROR\"}";
                }
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAnnexure(ANNEX_ID, HEADING, PROCESS_ID, FUNCTION_OWNER_ID, FUNCTION_ID_1, FUNCTION_ID_2, RISK_ID, MAX_NUMBER, GRAVITY, WEIGHTAGE) + "\"}";
            }
        [HttpPost]
        public string generate_traditional_risk_rating_of_engagement(int ENG_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.GenerateTraditionalRiskRatingofEngagement(ENG_ID) + "\"}";
            }

        [HttpPost]
        public List<TraditionalRiskRatingModel> view_traditional_risk_rating_of_engagement(int ENG_ID)
            {
            return dBConnection.ViewTraditionalRiskRatingofEngagement(ENG_ID);
            }
        [HttpPost]
        public string generate_annexure_risk_rating_of_engagement(int ENG_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.GenerateAnnexureRiskRatingofEngagement(ENG_ID) + "\"}";
            }

        [HttpPost]
        public List<TraditionalRiskRatingModel> view_annexure_risk_rating_of_engagement(int ENG_ID)
            {
            return dBConnection.ViewAnnexureRiskRatingofEngagement(ENG_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<RiskRatingModelForBranchesWorking> get_risk_rating_model_for_branches_working(int ENG_ID)
            {
            return dBConnection.GetRiskRatingModelForBranchesWorking(ENG_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<ComplianceHierarchyModel> get_compliance_hierarchy()
            {
            return dBConnection.GetComplianceHierarchies();
            }
        [HttpGet]
        [HttpPost]
        public List<ComplianceProgressReportModel> get_compliance_progress_report(string ROLE_TYPE)
            {
            return dBConnection.GetComplianceProgressReport(ROLE_TYPE);
            }
        [HttpGet]
        [HttpPost]
        public List<ComplianceProgressReportDetailModel> get_compliance_progress_report_details(string ROLE_TYPE, int? PP_NO)
            {
            return dBConnection.GetComplianceProgressReportDetails(ROLE_TYPE, PP_NO);
            }
        [HttpPost]
        [ApplicationAudit("COMPLIANCE_HIERARCHY_ADDED", "COMPLIANCE", "COMPLIANCE SETUP", "PKG_AD", "P_ADD_COM_OFFICER", ObjectType = "ENTITY", ObjectId = "ENTITY_ID", RequireResultMessage = true)]
        public string add_compliance_hierarchy(int ENTITY_ID, string REVIEWER_PP, string AUTHORIZER_PP)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddComplianceHierarchy(ENTITY_ID, REVIEWER_PP, AUTHORIZER_PP) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("COMPLIANCE_HIERARCHY_UPDATED", "COMPLIANCE", "COMPLIANCE SETUP", "PKG_AD", "P_UPDATE_COM_OFFICER", ObjectType = "ENTITY", ObjectId = "ENTITY_ID", RequireResultMessage = true)]
        public string update_compliance_hierarchy(int ENTITY_ID, string REVIEWER_PP, string AUTHORIZER_PP, string COMPLIANCE_KEY)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateComplianceHierarchy(ENTITY_ID, REVIEWER_PP, AUTHORIZER_PP, COMPLIANCE_KEY) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<SettledParasModel> get_settled_paras_for_compliance_report(int ENTITY_TYPE_ID, DateTime? DATE_FROM, DateTime? DATE_TO)
            {
            return dBConnection.GetSettledParasForComplianceReport(ENTITY_TYPE_ID, DATE_FROM, DATE_TO);
            }
        [HttpGet]
        [HttpPost]
        public List<SettledParasModel> get_post_compliance_settlement_report()
            {
            return new List<SettledParasModel>(); //dBConnection.GetSettledParasForComplianceReport();
            }


        [HttpGet]
        [HttpPost]
        public List<EngPlanDelayAnalysisReportModel> get_engagement_plan_delay_analysis_report()
            {
            return dBConnection.GetEngagementPlanDelayAnalysisReport();
            }
        [HttpGet]
        [HttpPost]
        public List<CAUParaForComplianceModel> get_cau_paras_for_compliance()
            {
            return dBConnection.GetCAUParasForPostCompliance();
            }
        [HttpGet]
        [HttpPost]
        public List<UserRelationshipModel> get_parent_relationship_for_CAU(int ENTITY_REALTION_ID)
            {
            return dBConnection.GetParentRelationshipForCAU(ENTITY_REALTION_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<UserRelationshipModel> get_child_relationship_for_CAU(int E_R_ID)
            {
            return dBConnection.GetChildRelationshipForCAU(E_R_ID);
            }
        [HttpPost]
        [ApplicationAudit("CAU_PARA_SUBMITTED_TO_BRANCH", "COMPLIANCE", "POST AUDIT COMPLIANCE", "PKG_AE", "P_FORWARD_CAU_PARA_TO_BRANCH", ComId = "COM_ID", ObjectType = "PARA", ObjectId = "COM_ID", RequireResultMessage = true)]
        public string submit_cau_para_to_branch(string COM_ID, string BR_ENT_ID, string CAU_COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitCAUParaToBranch(COM_ID, BR_ENT_ID, CAU_COMMENTS) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public ParaTextModel get_cau_para_to_branch_para_text(string COM_ID, string INDICATOR)
            {
            return dBConnection.GetCAUParaToBranchParaText(COM_ID, INDICATOR);
            }
        [HttpGet]
        [HttpPost]
        public List<CAUParaForComplianceModel> get_cau_paras_for_compliance_submitted_to_branch()
            {
            return dBConnection.GetCAUParasForPostComplianceSubmittedToBranch();
            }
        [HttpPost]
        [ApplicationAudit("CAU_PARA_SUBMITTED_BY_BRANCH", "COMPLIANCE", "POST AUDIT COMPLIANCE", "PKG_AE", "P_SUBMITPOSTAUDITCOMPLIANCE_BY_BRANCH", ComId = "COM_ID", ObjectType = "PARA", ObjectId = "COM_ID", RequireResultMessage = true)]
        public async Task<string> submit_cau_para_by_branch(string COM_ID, string TEXT_ID, string BR_COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + await dBConnection.SubmitCAUParaByBranch(COM_ID, TEXT_ID, BR_COMMENTS) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<CAUParaForComplianceModel> get_cau_paras_for_compliance_for_review()
            {
            return dBConnection.GetCAUParasForPostComplianceForReview();
            }
        [HttpGet]
        [HttpPost]
        public List<AuditeeResponseEvidenceModel> get_cau_paras_evidences_for_compliance_for_review(string TEXT_ID)
            {
            return dBConnection.GetCAUAllComplianceEvidence(TEXT_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<FADMonthlyReviewParasModel> get_fad_monthly_review_paras_for_entity_type_id(string ENT_TYPE_ID, DateTime? S_DATE, DateTime? E_DATE)
            {
            var rows = dBConnection.GetFADMonthlyReviewParasForEntityTypeId(ENT_TYPE_ID, S_DATE, E_DATE);
            var canViewReportingOffice = new[] { 1, 3, 5, 6, 7,15,16,40 }.Contains(sessionHandler.GetCurrentUserRoleId() ?? 0);

            if (!canViewReportingOffice)
                {
                rows.ForEach(row => row.REPORTING_OFFICE = string.Empty);
                }

            return rows;
            }
        [HttpGet]
        [HttpPost]
        public List<SpecialAuditPlanModel> get_saved_special_audit_plans()
            {
            return dBConnection.GetSaveSpecialAuditPlan();
            }
        [HttpPost]
        [ApplicationAudit("SPECIAL_AUDIT_PLAN_SAVED", "AUDIT_PLANNING", "SPECIAL_AUDIT", "PKG_PG", "P_ADD_SPECIAL_AUDIT_PLAN", ObjectType = "SPECIAL_AUDIT_PLAN", ObjectId = "PLAN_ID")]
        public IActionResult add_special_audit_plan(string NATURE, string PERIOD, string ENTITY_ID, string NO_DAYS, string PLAN_ID, string INDICATOR)
            {
            var response = dBConnection.AddSpecialAuditPlan(NATURE, PERIOD, ENTITY_ID, NO_DAYS, PLAN_ID, INDICATOR);
            return LegacyMessageResponse(response, "Special audit plan saved successfully.");
            }
        [HttpPost]
        [ApplicationAudit("SPECIAL_AUDIT_PLAN_DELETED", "AUDIT_PLANNING", "SPECIAL_AUDIT", "PKG_PG", "P_UPDATE_SPECIAL_AUDIT", ObjectType = "SPECIAL_AUDIT_PLAN", ObjectId = "PLAN_ID")]
        public IActionResult delete_special_audit_plan(string PLAN_ID, string INDICATOR)
            {
            var response = dBConnection.DeleteSpecialAuditPlan(PLAN_ID, INDICATOR);
            return LegacyMessageResponse(response, "Special audit plan deleted successfully.");
            }
        [HttpPost]
        [ApplicationAudit("SPECIAL_AUDIT_PLAN_SUBMITTED", "AUDIT_PLANNING", "SPECIAL_AUDIT", "PKG_PG", "P_UPDATE_SPECIAL_AUDIT", ObjectType = "SPECIAL_AUDIT_PLAN", ObjectId = "PLAN_ID")]
        public IActionResult submit_special_audit_plan(string PLAN_ID, string INDICATOR)
            {
            var response = dBConnection.SubmitSpecialAuditPlan(PLAN_ID, INDICATOR);
            return LegacyMessageResponse(response, "Special audit plan submitted successfully.");
            }
        [HttpPost]
        [ApplicationAudit("SPECIAL_AUDIT_PLAN_REFERRED_BACK", "AUDIT_PLANNING", "SPECIAL_AUDIT", "PKG_PG", "P_UPDATE_SPECIAL_AUDIT", ObjectType = "SPECIAL_AUDIT_PLAN", ObjectId = "PLAN_ID")]
        public IActionResult referred_back_special_audit_plan(string PLAN_ID, string INDICATOR)
            {
            var response = dBConnection.SubmitSpecialAuditPlan(PLAN_ID, INDICATOR);
            return LegacyMessageResponse(response, "Special audit plan referred back successfully.");
            }
        [HttpPost]
        [ApplicationAudit("SPECIAL_AUDIT_PLAN_APPROVED", "AUDIT_PLANNING", "SPECIAL_AUDIT", "PKG_PG", "P_UPDATE_SPECIAL_AUDIT", ObjectType = "SPECIAL_AUDIT_PLAN", ObjectId = "PLAN_ID")]
        public IActionResult approve_special_audit_plan(string PLAN_ID, string INDICATOR)
            {
            var response = dBConnection.SubmitSpecialAuditPlan(PLAN_ID, INDICATOR);
            return LegacyMessageResponse(response, "Special audit plan approved successfully.");
            }

        [HttpGet]
        [HttpPost]
        public List<DuplicateDeleteManageParaModel> get_duplicate_paras_for_authorize()
            {
            return archiveDbConnection.GetDuplicateParasForAuthorization();
            }
        [HttpPost]
        [ApplicationAudit("DUPLICATE_PARA_DELETE_REQUESTED", "COMPLIANCE", "PARA MANAGEMENT", "PKG_HD", "P_ADD_DUPLICATE_PARAS", ParaId = "NEW_PARA_ID", OldParaId = "OLD_PARA_ID", NewParaId = "NEW_PARA_ID", ObjectType = "PARA", RequireResultMessage = true)]
        public IActionResult request_delete_duplicate_para(int NEW_PARA_ID = 0, int OLD_PARA_ID = 0, string INDICATOR = "", string REMARKS = "")
            {
            if (string.IsNullOrWhiteSpace(INDICATOR) || (NEW_PARA_ID <= 0 && OLD_PARA_ID <= 0))
                {
                return BadRequest(new { Status = false, Message = "Duplicate para details are missing. Please refresh the list and try again." });
                }

            return Json(new
                {
                Status = true,
                Message = dBConnection.RequestDeleteDuplicatePara(NEW_PARA_ID, OLD_PARA_ID, INDICATOR, REMARKS)
                });
            }
        [HttpPost]
        [ApplicationAudit("DUPLICATE_PARA_DELETE_REJECTED", "COMPLIANCE", "PARA MANAGEMENT", "PKG_HD", "P_REJECT_DUPLICATE_PARAS", ObjectType = "DUPLICATE_PARA_REQUEST", ObjectId = "D_ID", RequireResultMessage = true)]
        public string reject_delete_duplicate_para(int D_ID = 0)
            {
            return "{\"Status\":true,\"Message\":\"" + archiveDbConnection.RejectDeleteDuplicatePara(D_ID) + "\"}";
            }
        [HttpPost]
        [ApplicationAudit("DUPLICATE_PARA_DELETE_AUTHORIZED", "COMPLIANCE", "PARA MANAGEMENT", "PKG_HD", "P_AUTH_DUPLICATE_PARAS", ObjectType = "DUPLICATE_PARA_REQUEST", ObjectId = "D_ID", RequireResultMessage = true)]
        public string authorize_delete_duplicate_para(int D_ID = 0)
            {
            return "{\"Status\":true,\"Message\":\"" + archiveDbConnection.AuthDeleteDuplicatePara(D_ID) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<ObservationResponsiblePPNOModel> get_responsible_person_list(int PARA_ID, string INDICATOR)
            {
            return dBConnection.GetResponsiblePersonsList(PARA_ID, INDICATOR);
            }

        [HttpGet]
        [HttpPost]
        public List<SeriousFraudulentObsGMDetails> get_serious_entities_details(string INDICATOR, int PARENT_ENT_ID, string ANNEX_IND)
            {
            return dBConnection.GetSeriousFraudulentObsGMDetails(INDICATOR, PARENT_ENT_ID, ANNEX_IND);
            }
        [HttpPost]
        [ApplicationAudit("OBSERVATION_RESPONSIBLE_ADDED", "AUDIT_EXECUTION", "OBSERVATION", "PKG_AR", "P_ADD_RESPONSIBILITY", EngagementId = "model.ENG_ID", ParaId = "model.NEW_PARA_ID", ComId = "model.COM_ID", ObjectType = "OBSERVATION_RESPONSIBILITY", ObjectId = "model.NEW_PARA_ID", RequireResultMessage = true)]
        public IActionResult add_responsible_to_observation(ObservationResponsiblePPNOModel model)
            {
            var result = dBConnection.AddResponsiblePersonsToObservation(
                model.NEW_PARA_ID.GetValueOrDefault(),
                model.ENG_ID.GetValueOrDefault(),
                model.COM_ID.GetValueOrDefault(),
                model.INDICATOR,
                model,
                model.PARA_STATUS.GetValueOrDefault());
            return Json(new { Message = result });

            }

        [HttpPost]
        [ApplicationAudit("OBSERVATION_RESPONSIBLE_DELETED", "AUDIT_EXECUTION", "OBSERVATION", "PKG_AR", "P_DELETE_RESPONSIBILITY", EngagementId = "model.ENG_ID", ParaId = "model.NEW_PARA_ID", OldParaId = "model.OLD_PARA_ID", ObjectType = "OBSERVATION_RESPONSIBILITY", RequireResultMessage = true)]
        public IActionResult delete_responsible_from_observation(ObservationResponsiblePPNOModel model)
            {
            var paraId = model.NEW_PARA_ID ?? model.OLD_PARA_ID ?? 0;
            var result = dBConnection.DeleteResponsibilityFromObservation(
                paraId,
                model.ENG_ID.GetValueOrDefault(),
                model);
            return Json(new { Message = result });
            }

        [HttpPost]
        [ApplicationAudit("OLD_PARA_RESPONSIBILITY_ADDED", "COMPLIANCE", "LEGACY PARA", "PKG_AR", "P_RESPONIBILITYFOROLDPARA", ParaId = "model.NEW_PARA_ID", OldParaId = "model.OLD_PARA_ID", ComId = "model.COM_ID", ObjectType = "PARA", RequireResultMessage = true)]
        public IActionResult add_responsible_for_old_paras([FromQuery] string IND_Action, [FromBody] ObservationResponsiblePPNOModel model)
            {
            var result = dBConnection.AddResponsibilityforoldparas(model.COM_ID.GetValueOrDefault(), model, IND_Action);
            return Json(new { Message = result });
            }
        [HttpPost]
        [ApplicationAudit("DSA_SUBMITTED_TO_AUDITEE", "AUDIT_EXECUTION", "DRAFT_AUDIT", "PKG_AR", "P_DRAFT_DSA", EngagementId = "ENG_ID", ObjectType = "OBSERVATION", ObjectId = "OBS_ID", RequireResultMessage = true)]
        public string submit_dsa_to_auditee(int ENTITY_ID, int OBS_ID, int ENG_ID, List<RespDSAModel> RespDSAModel)
            {
            string out_resp = "";
            foreach (RespDSAModel rm in RespDSAModel)
                {
                out_resp += dBConnection.SubmitDSAToAuditee(ENTITY_ID, OBS_ID, ENG_ID, rm.RESP_PP_NO, rm.RESP_ROW_ID) + "<br/>";
                }
            return "{\"Status\":true,\"Message\":\"" + out_resp + "\"}";

            }
        [HttpGet]
        [HttpPost]
        public List<DraftDSAList> get_draft_dsa_list()
            {
            return dBConnection.GetDraftDSAList();
            }
        [HttpGet]
        [HttpPost]
        public DSAContentModel get_dsa_content(int DSA_ID)
            {
            return dBConnection.GetDraftDSAContent(DSA_ID);
            }
        [HttpPost]

        //SVP AZ ACTION
        [AIS.Filters.ApplicationAudit("SUBMIT_DSA_TO_HEAD_FAD", "ADMINISTRATION", "ADMINISTRATION", "PKG_AR", "P_SUBMIT_DSA_TO_HEAD_FAD", ObjectType = "SUBMIT_DSA_TO_HEAD_FAD", ObjectId = "DSA_ID", RequireResultMessage = true)]
        public string submit_dsa_to_head_fad(int DSA_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitDSAToHeadFAD(DSA_ID) + "\"}";

            }
        [HttpPost]

        //SVP AZ ACTION
        [AIS.Filters.ApplicationAudit("UPDATE_DSA_HEADING", "ADMINISTRATION", "ADMINISTRATION", "PKG_AR", "P_UPDATE_DSA_HEADING", ObjectType = "UPDATE_DSA_HEADING", ObjectId = "DSA_ID", RequireResultMessage = true)]
        public string update_dsa_heading(int DSA_ID, string DSA_HEADING)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateDSAHeading(DSA_ID, DSA_HEADING) + "\"}";

            }
        [HttpPost]

        //HEAD FAD ACTION
        public string reffered_back_by_head_fad(int DSA_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.ReferredBackDSAByHeadFad(DSA_ID) + "\"}";

            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("SUBMIT_DSA_TO_DPD", "ADMINISTRATION", "ADMINISTRATION", "PKG_AR", "P_SUBMIT_DSA_BY_HEAD_FAD_TO_DPD", ObjectType = "SUBMIT_DSA_TO_DPD", ObjectId = "DSA_ID", RequireResultMessage = true)]
        public string submit_dsa_to_dpd(int DSA_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitDSAToDPD(DSA_ID) + "\"}";

            }
        [HttpPost]

        //SVP DPD ACTION
        public string reffered_back_by_dpd(int DSA_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.ReferredBackDSAByDPD(DSA_ID) + "\"}";

            }
        [HttpPost]
        public string acknowledge_dsa_by_dpd(int DSA_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AcknowledgeDSA(DSA_ID) + "\"}";

            }

        [HttpPost]
        public List<LoanCaseDetailModel> get_lc_details(int LC_NO, int BR_CODE)
            {
            return dBConnection.GetLoanCaseDetailsWithBRCode(LC_NO, BR_CODE);
            }

        [HttpPost]
        public List<ResponsibilitySearchResultModel> get_responsible_by_lc(int LC_NO, int BR_CODE)
            {
            var details = dBConnection.GetLoanCaseDetailsWithBRCode(LC_NO, BR_CODE);
            var results = new List<ResponsibilitySearchResultModel>();
            foreach (var detail in details)
                {
                var loanCase = detail.LoanCaseNo;
                var lcAmount = detail.OutstandingAmount.ToString();
                void AddIfPresent(string role, string ppNo, string name)
                    {
                    if (string.IsNullOrWhiteSpace(ppNo))
                        {
                        return;
                        }
                    results.Add(new ResponsibilitySearchResultModel
                        {
                        Role = role,
                        PPNo = ppNo,
                        EmpName = name,
                        LoanCase = loanCase,
                        LCAmount = lcAmount,
                        AccountNumber = string.Empty,
                        AccAmount = string.Empty
                        });
                    }

                AddIfPresent("MCO", detail.McoPPNo, detail.McoName);
                AddIfPresent("Manager", detail.ManagerPPNo, detail.ManagerName);
                AddIfPresent("RGM", detail.RgmPPNo, detail.RgmName);
                AddIfPresent("CAD Reviewer", detail.CadReviewerPPNo, detail.CadReviewerName);
                AddIfPresent("CAD Authorizer", detail.CadAuthorizerPPNo, detail.CadAuthorizerName);
                }
            return results;
            }

        [HttpPost]
        public List<ResponsibilitySearchResultModel> get_responsible_by_pp(int PP_NO)
            {
            var user = dBConnection.GetEmployeeNameFromPPNO(PP_NO);
            if (user == null || string.IsNullOrWhiteSpace(user.PPNumber))
                {
                return new List<ResponsibilitySearchResultModel>();
                }
            return new List<ResponsibilitySearchResultModel>
                {
                new ResponsibilitySearchResultModel
                    {
                    Role = string.Empty,
                    PPNo = user.PPNumber,
                    EmpName = user.Name,
                    LoanCase = string.Empty,
                    LCAmount = string.Empty,
                    AccountNumber = string.Empty,
                    AccAmount = string.Empty
                    }
                };
            }

        [HttpGet]
        [HttpPost]
        public ObservationModel get_obs_details_by_id(int OBS_ID)
            {
            return dBConnection.GetObservationDetailsById(OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public ObservationModel get_obs_details_by_id_pre_con(int OBS_ID)
            {
            return dBConnection.GetObservationDetailsByIdForPreConcluding(OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public ObservationModel get_obs_details_by_id_pre_con_ho(int OBS_ID)
            {
            return dBConnection.GetObservationDetailsByIdForPreConcludingHO(OBS_ID);
            }
        [HttpGet]
        [HttpPost]
        public ObservationModel get_obs_details_by_id_ho(int OBS_ID)
            {
            return dBConnection.GetObservationDetailsByIdHO(OBS_ID);
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_GM_OFFICE", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATE_GM_OFFICE_RELATIONSHIP", ObjectType = "UPDATE_GM_OFFICE", ObjectId = "ENTITY_ID", RequireResultMessage = true)]
        public string update_gm_office(int GM_OFF_ID, int ENTITY_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateGMOffice(GM_OFF_ID, ENTITY_ID) + "\"}";

            }
        [HttpPost]
        [ApplicationAudit("REPORTING_LINE_UPDATED", "AUDIT_REPORT", "REPORTING SETUP", "PKG_AD", "P_UPDATE_RPT_OFFICE_RELATIONSHIP", ObjectType = "ENTITY", ObjectId = "ENTITY_ID", RequireResultMessage = true)]
        public string update_reporting_line(int REP_OFF_ID, int ENTITY_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateReportingLine(REP_OFF_ID, ENTITY_ID) + "\"}";

            }

        [HttpPost]
        [ApplicationAudit("GM_REPORTING_LINE_OFFICES_UPDATED", "AUDIT_REPORT", "REPORTING SETUP", "PKG_AD", "P_UPDATE_GM_REPORTING_LINE", ObjectType = "REPORTING_LINE", ObjectId = "REP_OFF_ID", RequireNonEmpty = "ENT_ID_ARR", RequireResultMessage = true)]
        public string update_gm_reporting_line_office(List<int> ENT_ID_ARR, int GM_OFF_ID, int REP_OFF_ID)
            {
            string res = "";
            if (ENT_ID_ARR.Count > 0)
                {
                foreach (int ENT_ID in ENT_ID_ARR)
                    {
                    res = dBConnection.UpdateGMAndReportingLineOffice(ENT_ID, GM_OFF_ID, REP_OFF_ID);
                    }
                }
            return "{\"Status\":true,\"Message\":\"GM Office and Reporting Line Updated Successfully\"}";

            }

        [HttpPost]
        [ApplicationAudit("AUDIT_REPORT_UPLOADED", "AUDIT_REPORT", "AUDIT REPORT", "PKG_HD", "P_UPLOAD_AUDIT_REPORT", EngagementId = "ENG_ID", ObjectType = "ENGAGEMENT", ObjectId = "ENG_ID", RequireResultMessage = true)]
        public async Task<string> upload_audit_report(int ENG_ID)
            {
            string response = await dBConnection.UploadAuditReport(ENG_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpGet]
        public IActionResult ExportAllAppicallsCsv()
            {
            var csvPath = Path.Combine(_hostingEnvironment.WebRootPath ?? _hostingEnvironment.ContentRootPath, "All Appicalls.csv");

            if (!System.IO.File.Exists(csvPath))
                {
                return NotFound();
                }

            var sanitizedLines = System.IO.File
                .ReadLines(csvPath)
                .Select(line =>
                    {
                        var columns = line.Split(',');
                        for (var i = 0; i < columns.Length; i++)
                            {
                            columns[i] = ServicesCsvSanitizer.Sanitize(columns[i]);
                            }

                        return string.Join(",", columns);
                    });

            var csvContent = string.Join("\n", sanitizedLines);
            var contentBytes = Encoding.UTF8.GetBytes(csvContent);
            return File(contentBytes, "text/csv", "All Appicalls.csv");
            }

        [HttpGet]
        [HttpPost]
        public List<FinalAuditReportModel> get_audit_reports(int ENG_ID)
            {
            return dBConnection.GetAuditReports(ENG_ID);
            }

        [HttpGet]
        [HttpPost]
        public AuditeeResponseEvidenceModel get_audit_report_content(string FILE_ID)
            {
            return dBConnection.GetAuditReportContent(FILE_ID);
            }

        //
        [HttpGet]
        [HttpPost]
        public FinalAuditReportModel get_check_report_exist_for_engId(int ENG_ID)
            {
            return dBConnection.GetCheckAuditReportExisits(ENG_ID);
            }

        [HttpPost]
        [ApplicationAudit("ENGAGEMENT_SAMPLE_CREATED", "AUDIT_PLANNING", "SAMPLING", "PKG_SM", "P_ADD_SAMPLE_DATA", EngagementId = "ENG_ID", ObjectType = "ENGAGEMENT", ObjectId = "ENG_ID", RequireResultMessage = true)]
        public string create_engagement_sample_data(int ENG_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.CreateSampleDataAfterEngagementApproval(ENG_ID) + "\"}";
            }

        [HttpPost]
        [ApplicationAudit("ENGAGEMENT_EXCEPTION_DATA_CREATED", "AUDIT_PLANNING", "EXCEPTION_MONITORING", "PKG_SM", "P_ADD_EXCEPTION_DATA", EngagementId = "ENG_ID", ObjectType = "ENGAGEMENT", ObjectId = "ENG_ID", RequireResultMessage = true)]
        public string create_engagement_Exception_data(int ENG_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.CreateExceptionDataAfterEngagementApproval(ENG_ID) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public JsonResult get_exception_monitor_entities()
            {
            var entities = dBConnection.GetExceptionMonitorEntities();
            return Json(entities);
            }

        [HttpGet]
        [HttpPost]
        public JsonResult get_exception_monitor_details(int eng_id)
            {
            var details = dBConnection.GetExceptionMonitorDetails(eng_id);
            return Json(details);
            }

        [HttpPost]
        public JsonResult regenerate_exception(int eng_id, int er_id)
            {
            try
                {
                dBConnection.RegenerateException(eng_id, er_id);
                return Json(new { success = true });
                }
            catch (Exception ex)
                {
                return Json(new { success = false, message = ex.Message });
                }
            }

        [HttpGet]
        [HttpPost]
        public List<BiometSamplingModel> get_biomet_sampling_details(int ENG_ID)
            {
            return dBConnection.GetBiometSamplingDetails(ENG_ID);
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_exception_account_report(long ENG_ID, long RPT_ID)
            {
            var data = dBConnection.GetExceptionReportData(RPT_ID, ENG_ID);

            return Json(new
                {
                columns = data.Columns,
                rows = data.Rows
                });
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_exception_report_format(long report_id)
            {
            var result = dBConnection.GetExceptionReportFormat(report_id);
            return Json(result);
            }

        [HttpPost]
        [ApplicationAudit("EXCEPTION_REPORT_FORMAT_SAVED", "AUDIT_REPORT", "EXCEPTION REPORT", "PKG_SM", "P_INSERT_EXCEPTION_REPORT_FORMAT", ObjectType = "EXCEPTION_REPORT_FORMAT")]
        public IActionResult save_exception_report_format([FromBody] ExceptionReportFormatModel model)
            {
            var validationResult = ValidateExceptionReportFormat(model);
            if (validationResult != null)
                {
                return validationResult;
                }

            var result = dBConnection.InsertExceptionReportFormat(model);
            return Json(new { status = result });
            }

        [HttpPost]
        [ApplicationAudit("EXCEPTION_REPORT_FORMAT_UPDATED", "AUDIT_REPORT", "EXCEPTION REPORT", "PKG_SM", "P_UPDATE_EXCEPTION_REPORT_FORMAT", ObjectType = "EXCEPTION_REPORT_FORMAT")]
        public IActionResult update_exception_report_format([FromBody] ExceptionReportFormatModel model)
            {
            var validationResult = ValidateExceptionReportFormat(model);
            if (validationResult != null)
                {
                return validationResult;
                }

            var result = dBConnection.UpdateExceptionReportFormat(model);
            return Json(new { status = result });
            }

        [HttpGet]
        [HttpPost]
        public List<AccountTransactionSampleModel> get_biomet_account_transaction_sampling_details(int ENG_ID, string AC_NO)
            {
            return dBConnection.GetBiometAccountTransactionSamplingDetails(ENG_ID, AC_NO);
            }
        [HttpGet]
        [HttpPost]
        public List<AccountDocumentBiometSamplingModel> get_biomet_account_documents_sampling_details(string AC_NO)
            {
            return dBConnection.GetBiometAccountDocumentsSamplingDetails(AC_NO);
            }


        [HttpGet]
        [HttpPost]
        public List<ListOfSamplesModel> get_list_of_samples(int ENG_ID)
            {
            return dBConnection.GetListOfSamples(ENG_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<ListOfReportsModel> get_list_of_reports(int ENG_ID)
            {
            return dBConnection.GetListOfreports(ENG_ID);
            }

        [HttpPost]
        [ApplicationAudit("EXCEPTION_ACCOUNT_REPORT_ADDED", "AUDIT_REPORT", "EXCEPTION REPORT", "PKG_SM", "P_ADD_NEW_EXP_REPORT", ObjectType = "REPORT", ObjectId = "REPORT_ID", RequireResultMessage = true)]
        public IActionResult add_exception_account_report(string IND = "", int REPORT_ID = 0, string REPORT_TITLE = "", string DESCRIPTION = "", string TYPE = "", int LOAN_STATUS_ID = 0)
            {
            REPORT_TITLE = REPORT_TITLE?.Trim();
            if (!IsValidExceptionReportText(REPORT_TITLE))
                {
                return BadRequest(new { Status = false, Message = "Report title must contain only letters, numbers, spaces, ampersand (&), comma (,), and question mark (?)." });
                }

            var response = dBConnection.AddExceptionAccountReport(IND, REPORT_ID, REPORT_TITLE, DESCRIPTION, TYPE, LOAN_STATUS_ID);
            return Json(new { Status = true, Message = response });
            }

        [HttpGet]
        [HttpPost]
        public List<LoanCaseSampleModel> get_loan_samples(string INDICATOR, int STATUS_ID, int ENG_ID, int SAMPLE_ID)
            {
            return dBConnection.GetLoanSamples(INDICATOR, STATUS_ID, ENG_ID, SAMPLE_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<LoanCaseSampleModel> get_loan_Exceptions(string INDICATOR, int STATUS_ID, int ENG_ID)
            {
            return dBConnection.GetLoanExceptions(INDICATOR, STATUS_ID, ENG_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<LoanCaseSampleDocumentsModel> get_loan_documents(int ENG_ID, string LOAN_DISB_ID)
            {
            return dBConnection.GetLoanSamplesDocuments(ENG_ID, LOAN_DISB_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<LoanCaseSampleDocumentsModel> get_loan_document_data(int IMAGE_ID)
            {
            return dBConnection.GetLoanSamplesDocumentData(IMAGE_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<LoanCaseSampleTransactionsModel> get_sample_loan_transactions(int ENG_ID, string LOAN_DISB_ID)
            {
            return dBConnection.GetLoanSamplesTransactions(ENG_ID, LOAN_DISB_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<ParaTextSearchModel> get_para_text_in_audit_report(string SEARCH_KEYWORD, string SEARCH_TYPE = "TEXT")
            {
            var searchType = string.Equals(SEARCH_TYPE, "TITLE", StringComparison.OrdinalIgnoreCase) ? "TITLE" : "TEXT";
            return dBConnection.GetAuditParasByText(SEARCH_KEYWORD, searchType);
            }
        [HttpPost]
        public string regenerate_sample_of_loans(int ENG_ID, int LOAN_SAMPLE_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RegenerateSampleofLoan(ENG_ID, LOAN_SAMPLE_ID) + "\"}";
            }


        [HttpGet]
        [HttpPost]
        public List<CDMSMasterTransactionModel> get_CDMS_master_transactions(string ENTITY_ID, DateTime START_DATE, DateTime END_DATE, string CNIC_NO, string ACC_NO)
            {
            return dBConnection.GetCDMSMasterTransactions(ENTITY_ID, START_DATE, END_DATE, CNIC_NO, ACC_NO);
            }

        [HttpGet]
        [HttpPost]
        public List<AuditEmployeeModel> get_audit_emp()
            {
            return dBConnection.GetFadAuditEmployees();
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_EMPLOYEE_UPDATED", "AUDIT_EXECUTION", "FAD_RESOURCES", "PKG_AD", "P_UPDATE_AUDIT_EMP", ObjectType = "AUDIT_EMPLOYEE", ObjectId = "model.ID", RequireResultMessage = true)]
        public string update_audit_emp(FADAuditEmpModel model)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditEmployee(model) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<FADAuditManpowerModel> get_audit_manpower()
            {
            return dBConnection.GetAuditManpower();
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_MANPOWER_UPDATED", "AUDIT_EXECUTION", "FAD_RESOURCES", "PKG_AD", "P_UPDATE_AUDIT_MANPOWER", ObjectType = "AUDIT_MANPOWER", ObjectId = "model.ID", RequireResultMessage = true)]
        public string update_audit_manpower(FADAuditManpowerModel model)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditManpower(model) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<FADAuditBudgetModel> get_audit_budget()
            {
            return dBConnection.GetAuditBudget();
            }

        [HttpPost]
        [ApplicationAudit("AUDIT_BUDGET_UPDATED", "AUDIT_EXECUTION", "FAD_RESOURCES", "PKG_AD", "P_UPDATE_AUDIT_BUDGET", ObjectType = "AUDIT_BUDGET", ObjectId = "model.ID", RequireResultMessage = true)]
        public string update_audit_budget(FADAuditBudgetModel model)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditBudget(model) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<DropDownModel> get_hr_rank()
            {
            return dBConnection.GetHrRanks();
            }

        [HttpGet]
        [HttpPost]
        public List<DropDownModel> get_hr_designation()
            {
            return dBConnection.GetHrDesignations();
            }

        [HttpGet]
        [HttpPost]
        public List<DropDownModel> get_hr_posting()
            {
            return dBConnection.GetHrPosting();
            }

        [HttpGet]
        [HttpPost]
        public List<DropDownModel> get_qualification()
            {
            return dBConnection.GetQualifications();
            }

        [HttpGet]
        [HttpPost]
        public List<DropDownModel> get_qualification_specialization()
            {
            return dBConnection.GetQualificationSpecialization();
            }

        [HttpGet]
        [HttpPost]
        public List<DropDownModel> get_certification()
            {
            return dBConnection.GetCertifications();
            }

        [HttpGet]
        [HttpPost]
        public List<DropDownModel> get_gl_heads()
            {
            return dBConnection.GetGLHeads();
            }

        // ----- I&ID Inquiry API endpoints -----

        [HttpPost]
        [ApplicationAudit("SUBMIT_COMPLAINT", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_RPT", "R_GET_RBH_LIST", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult SubmitComplaint([FromForm] AIS.Models.IID.ComplaintModel model)
            {
            try
                {
                if (string.IsNullOrWhiteSpace(model.PertainsTo))
                    {
                    return Json(new { ok = false, message = "PertainsTo is required." });
                    }

                if (string.Equals(model.Source, "Other", StringComparison.OrdinalIgnoreCase))
                    {
                    if (string.IsNullOrWhiteSpace(model.SourceOtherText))
                        {
                        return Json(new { ok = false, message = "Source other text is required." });
                        }
                    }
                else
                    {
                    model.SourceOtherText = null;
                    }

                if (string.Equals(model.PertainsTo, "HO", StringComparison.OrdinalIgnoreCase))
                    {
                    model.FieldType = null;
                    model.HOUnitTypeId = null;
                    model.HOUnitId = null;
                    model.RegionId = null;
                    model.BranchId = null;
                    }
                else if (string.Equals(model.PertainsTo, "FIELD", StringComparison.OrdinalIgnoreCase))
                    {
                    if (string.Equals(model.FieldType, "HO_UNIT", StringComparison.OrdinalIgnoreCase))
                        {
                        if (!model.HOUnitTypeId.HasValue || !model.HOUnitId.HasValue)
                            {
                            return Json(new { ok = false, message = "HO Unit Type and HO Unit are required." });
                            }
                        model.RegionId = null;
                        model.BranchId = null;
                        }
                    else if (string.Equals(model.FieldType, "BRANCH", StringComparison.OrdinalIgnoreCase))
                        {
                        if (!model.RegionId.HasValue || !model.BranchId.HasValue)
                            {
                            return Json(new { ok = false, message = "Region and Branch are required." });
                            }
                        model.HOUnitTypeId = null;
                        model.HOUnitId = null;
                        }
                    else
                        {
                        return Json(new { ok = false, message = "FieldType is required for Field complaints." });
                        }
                    }
                else
                    {
                    return Json(new { ok = false, message = "PertainsTo must be HO or FIELD." });
                    }

                var complaintFile = Request.Form.Files.GetFile("ComplaintFile");
                var ffrFile = Request.Form.Files.GetFile("FfrFile");
                var evidenceFiles = Request.Form.Files.GetFiles("OtherEvidence");
                model.UploadedComplaint = SaveUploadFile(complaintFile);
                model.UploadedFFR = SaveUploadFile(ffrFile);
                var evidenceNames = new List<string>();
                foreach (var file in evidenceFiles)
                    {
                    var savedFile = SaveUploadFile(file);
                    if (!string.IsNullOrEmpty(savedFile))
                        {
                        evidenceNames.Add(savedFile);
                        }
                    }
                model.UploadedEvidence = string.Join(";", evidenceNames);
                var id = dBConnection.SubmitComplaint(model);
                return Json(new { ok = id > 0, complaintId = id });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_ASSESSMENT", "ADMINISTRATION", "ADMINISTRATION", "PKG_INQ", "ADD_ASSESSMENT", ObjectType = "ADDASSESSMENT", ObjectId = "model.ComplaintId")]
        public IActionResult AddAssessment([FromBody] AIS.Models.IID.InitialAssessmentModel model)
            {
            try
                {
                if (model == null)
                    {
                    return Json(new { ok = false, message = "Assessment payload is required." });
                    }
                if (!model.ComplaintId.HasValue || model.ComplaintId.Value <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required." });
                    }
                if (model.AssignedUnitId <= 0)
                    {
                    return Json(new { ok = false, message = "Assigned unit is required." });
                    }
                var id = dBConnection.AddAssessment(model);
                return Json(new { ok = id > 0, id });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
      //  [Consumes("application/x-www-form-urlencoded")]
        [AIS.Filters.ApplicationAudit("ADD_HEAD_REVIEW", "ADMINISTRATION", "ADMINISTRATION", "PKG_INQ", "ADD_HEAD_REVIEW", ObjectType = "ADDHEADREVIEW", ObjectId = "model.ComplaintId")]
        public async Task<IActionResult> AddHeadReview([FromForm] HeadReviewModel model)
            {
            try
                {
                if (model == null)
                    {
                    return Json(new { ok = false, message = "Head review payload is required." });
                    }
                if (!model.ComplaintId.HasValue || model.ComplaintId.Value <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required." });
                    }
                if (!model.AssessmentId.HasValue || model.AssessmentId.Value <= 0)
                    {
                    return Json(new { ok = false, message = "AssessmentId is required." });
                    }
                if (string.Equals(model.Action, "APPROVE", StringComparison.OrdinalIgnoreCase) && model.AssignedToUnit <= 0)
                    {
                    return Json(new { ok = false, message = "Assigned unit is required." });
                    }
                var id = dBConnection.AddHeadReview(model);
                if (id > 0
                    && string.Equals(model.Action, "APPROVE", StringComparison.OrdinalIgnoreCase)
                    && model.AssignedToUnit > 0)
                    {
                    var notificationData = dBConnection.GetInquiryAssignedNotificationData(model);
                    await EmailNotification.SendInquiryAssignedToUnitAsync(_configuration, notificationData, HttpContext?.RequestServices);
                    }

                return Json(new { ok = id > 0, id });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("INVESTIGATION_PLAN_CREATED", "AUDIT_PLANNING", "INQUIRY_INVESTIGATION", "PKG_INQ", "ADD_INV_PLAN", ObjectType = "COMPLAINT", ObjectId = "model.ComplaintId")]
        public IActionResult AddInvestigationPlan([FromBody] AIS.Models.IID.InvestigationPlanModel model)
            {
            try
                {
                if (model == null || model.ComplaintId <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required." });
                    }
                var id = dBConnection.AddInvestigationPlan(model);
                return Json(new { ok = id > 0, id });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("INVESTIGATION_PLAN_DECIDED", "AUDIT_PLANNING", "INQUIRY_INVESTIGATION", "PKG_INQ", "ADD_PLAN_APPROVAL", ObjectType = "INVESTIGATION_PLAN", ObjectId = "model.PlanId")]
        public IActionResult AddPlanApproval([FromForm] AIS.Models.IID.PlanApprovalModel model)
            {
            try
                {
                if (model == null || !model.PlanId.HasValue || model.PlanId.Value <= 0)
                    {
                    return Json(new { ok = false, message = "PlanId is required." });
                    }
                var id = dBConnection.AddPlanApproval(model);
                if (id > 0 && string.Equals(model?.IsApproved, "Y", StringComparison.OrdinalIgnoreCase))
                    {
                    var complaintId = dBConnection.GetComplaintIdByPlanId(model?.PlanId);
                    if (complaintId.HasValue)
                        {
                        dBConnection.EnqueueEmail(
                            "IID_PLAN_APPROVED",
                            complaintId,
                            model?.PlanId,
                            string.Empty,
                            string.Empty,
                            "IID Plan Approved",
                            $"Investigation plan approved for complaint ID {complaintId.Value}.");
                        }
                    }
                return Json(new { ok = id > 0, id });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("INQUIRY_REPORT_ADDED", "AUDIT_REPORT", "INQUIRY REPORT", "PKG_INQ", "ADD_INQUIRY_REPORT", ObjectType = "COMPLAINT", ObjectId = "model.ComplaintId")]
        public IActionResult AddInquiryReport([FromForm] AIS.Models.IID.InquiryReportModel model)
            {
            try
                {
                if (model == null)
                    {
                    return Json(new { ok = false, message = "Inquiry report payload is required." });
                    }
                if (!model.ComplaintId.HasValue || model.ComplaintId.Value <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required." });
                    }

                if (Request.HasFormContentType)
                    {
                    var existingUploadedReport = model.UploadedReport;
                    var existingUploadedEvidence = model.UploadedEvidence;
                    var existingUploadedDsa = model.UploadedDsa;
                    var reportFile = Request.Form.Files.GetFile("UploadedReport");
                    var evidenceFiles = Request.Form.Files.GetFiles("UploadedEvidence");
                    var dsaFile = Request.Form.Files.GetFile("UploadedDsa");

                    if (reportFile != null)
                        {
                        model.UploadedReport = SaveUploadFile(reportFile);
                        }
                    else
                        {
                        model.UploadedReport = existingUploadedReport;
                        }

                    if (evidenceFiles?.Count > 0)
                        {
                        var evidenceNames = new List<string>();
                        foreach (var file in evidenceFiles)
                            {
                            var savedFile = SaveUploadFile(file);
                            if (!string.IsNullOrEmpty(savedFile))
                                {
                                evidenceNames.Add(savedFile);
                                }
                            }
                        model.UploadedEvidence = string.Join(";", evidenceNames);
                        }
                    else
                        {
                        model.UploadedEvidence = existingUploadedEvidence;
                        }

                    if (dsaFile != null)
                        {
                        model.UploadedDsa = SaveUploadFile(dsaFile);
                        }
                    else
                        {
                        model.UploadedDsa = existingUploadedDsa;
                        }
                    }

                var id = dBConnection.AddInquiryReport(model);
                return Json(new { ok = id > 0, id });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_ANALYSIS", "ADMINISTRATION", "ADMINISTRATION", "PKG_INQ", "ADD_ANALYSIS", ObjectType = "ADDANALYSIS", ObjectId = "model.ReportId")]
        public IActionResult AddAnalysis([FromBody] AIS.Models.IID.AnalysisModel model)
            {
            try
                {
                if (model == null || !model.ReportId.HasValue || model.ReportId.Value <= 0)
                    {
                    return Json(new { ok = false, message = "ReportId is required." });
                    }
                var id = dBConnection.AddAnalysis(model);
                return Json(new { ok = id > 0, id });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_FINAL_APPROVAL", "ADMINISTRATION", "ADMINISTRATION", "PKG_INQ", "P_ENQUEUE_EMAIL", ObjectType = "ADDFINALAPPROVAL", ObjectId = "model.ReportId")]
        public IActionResult AddFinalApproval([FromBody] AIS.Models.IID.FinalApprovalModel model)
            {
            try
                {
                if (model == null || !model.ReportId.HasValue || model.ReportId.Value <= 0)
                    {
                    return Json(new { ok = false, message = "ReportId is required." });
                    }
                var id = dBConnection.AddFinalApproval(model);
                if (id > 0 && string.Equals(model?.Decision, "APPROVE", StringComparison.OrdinalIgnoreCase))
                    {
                    var complaintId = dBConnection.GetComplaintIdByReportId(model?.ReportId);
                    if (complaintId.HasValue)
                        {
                        dBConnection.EnqueueEmail(
                            "IID_INQUIRY_APPROVED",
                            complaintId,
                            model?.ReportId,
                            string.Empty,
                            string.Empty,
                            "IID Inquiry Approved",
                            $"Inquiry approved for complaint ID {complaintId.Value}.");
                        }
                    }
                return Json(new { ok = id > 0, id });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("IID_REPORT_FINALIZED", "AUDIT_REPORT", "INQUIRY REPORT", "PKG_INQ", "P_FINALIZE_IID_REPORT", ObjectType = "REPORT", ObjectId = "model.ReportId")]
        public IActionResult FinalizeIidReport([FromBody] AIS.Models.IID.FinalApprovalModel model)
            {
            try
                {
                var unauthorized = EnsureAuthenticatedSession();
                if (unauthorized != null)
                    {
                    return unauthorized;
                    }

                if (model == null)
                    {
                    return Json(new { ok = false, message = "Final report payload is required." });
                    }

                var reportId = model.ReportId.GetValueOrDefault();
                if (reportId <= 0 && model.ComplaintId.GetValueOrDefault() > 0)
                    {
                    reportId = dBConnection.GetLatestInquiryReportByComplaintId(model.ComplaintId.Value)?.ReportId ?? 0;
                    model.ReportId = reportId;
                    }

                if (reportId <= 0)
                    {
                    return Json(new { ok = false, message = "A submitted inquiry report is required before finalization." });
                    }

                var id = dBConnection.FinalizeIidReport(model);
                if (id > 0)
                    {
                    var complaintId = model.ComplaintId;
                    if ((!complaintId.HasValue || complaintId.Value <= 0) && model.ReportId.HasValue)
                        {
                        complaintId = dBConnection.GetComplaintIdByReportId(model.ReportId);
                        }

                    if (complaintId.HasValue)
                        {
                        dBConnection.FinalizeIidInquiryReport(complaintId.Value, sessionHandler.GetUser()?.UserEntityID);

                        dBConnection.EnqueueEmail(
                            "IID_INQUIRY_APPROVED",
                            complaintId,
                            model.ReportId,
                            string.Empty,
                            string.Empty,
                            "IID Inquiry Approved",
                            $"Inquiry approved for complaint ID {complaintId.Value}.");
                        }
                    }

                return Json(new { ok = id > 0, id, message = id > 0 ? "Inquiry report finalized successfully." : "Unable to finalize inquiry report." });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_CASE_STUDY", "ADMINISTRATION", "ADMINISTRATION", "PKG_INQ", "ADD_CASE_STUDY", ObjectType = "ADDCASESTUDY", ObjectId = "model.ComplaintId")]
        public IActionResult AddCaseStudy([FromBody] AIS.Models.IID.CaseStudyModel model)
            {
            try
                {
                if (model == null || !model.ComplaintId.HasValue || model.ComplaintId.Value <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required." });
                    }
                var id = dBConnection.AddCaseStudy(model);
                return Json(new { ok = id > 0, id });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetInquiryReportFiles([FromBody] AIS.Models.IID.ReportIdRequestModel model)
            {
            var report = dBConnection.GetInquiryReportFiles(model.ReportId.GetValueOrDefault());
            return Ok(report);
            }

        [HttpPost]
        public IActionResult GetReports([FromBody] AIS.Models.IID.ReportFilterModel filter)
            {
            try
                {
                var list = dBConnection.GetReports(filter);
                return Json(list);
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpGet]
        [HttpPost]
        public List<ListOfReportsModel> get_iid_list_of_reports(int ENG_ID)
            {
            return dBConnection.GetIidListOfReports(ENG_ID);
            }

        [HttpGet]
        public IActionResult get_iid_exception_report_format(long report_id)
            {
            var result = dBConnection.GetIidExceptionReportFormat(report_id);
            return Json(result);
            }

        [HttpPost]
        [ApplicationAudit("IID_EXCEPTION_REPORT_FORMAT_SAVED", "AUDIT_REPORT", "IID EXCEPTION REPORT", "PKG_ISM", "P_INSERT_EXCEPTION_REPORT_FORMAT", ObjectType = "EXCEPTION_REPORT_FORMAT")]
        public IActionResult save_iid_exception_report_format([FromBody] ExceptionReportFormatModel model)
            {
            var validationResult = ValidateExceptionReportFormat(model);
            if (validationResult != null)
                {
                return validationResult;
                }

            var result = dBConnection.InsertIidExceptionReportFormat(model);
            return Json(new { status = result });
            }

        [HttpPost]
        [ApplicationAudit("IID_EXCEPTION_REPORT_FORMAT_UPDATED", "AUDIT_REPORT", "IID EXCEPTION REPORT", "PKG_ISM", "P_UPDATE_EXCEPTION_REPORT_FORMAT", ObjectType = "EXCEPTION_REPORT_FORMAT")]
        public IActionResult update_iid_exception_report_format([FromBody] ExceptionReportFormatModel model)
            {
            var validationResult = ValidateExceptionReportFormat(model);
            if (validationResult != null)
                {
                return validationResult;
                }

            var result = dBConnection.UpdateIidExceptionReportFormat(model);
            return Json(new { status = result });
            }

        [HttpPost]
        [ApplicationAudit("IID_EXCEPTION_ACCOUNT_REPORT_ADDED", "AUDIT_REPORT", "IID EXCEPTION REPORT", "PKG_ISM", "P_ADD_NEW_EXP_REPORT", ObjectType = "REPORT", ObjectId = "REPORT_ID", RequireResultMessage = true)]
        public IActionResult add_iid_exception_account_report(string IND = "", int REPORT_ID = 0, string REPORT_TITLE = "", string DESCRIPTION = "", string TYPE = "", int LOAN_STATUS_ID = 0)
            {
            REPORT_TITLE = REPORT_TITLE?.Trim();
            if (!IsValidExceptionReportText(REPORT_TITLE))
                {
                return BadRequest(new { Status = false, Message = "Report title must contain only letters, numbers, spaces, ampersand (&), comma (,), and question mark (?)." });
                }

            var response = dBConnection.AddIidExceptionAccountReport(IND, REPORT_ID, REPORT_TITLE, DESCRIPTION, TYPE, LOAN_STATUS_ID);
            return Json(new { Status = true, Message = response });
            }

        [HttpGet]
        [HttpPost]
        public IActionResult get_iid_exception_account_report(long ENG_ID, long RPT_ID)
            {
            var data = dBConnection.GetIidExceptionReportData(RPT_ID, ENG_ID);
            return Json(new
                {
                columns = data.Columns,
                rows = data.Rows
                });
            }

        [HttpGet]
        [HttpPost]
        public List<AccountTransactionSampleModel> get_iid_biomet_account_transaction_sampling_details(int ENG_ID, string AC_NO)
            {
            return dBConnection.GetIidAccountTransactions(ENG_ID, AC_NO);
            }

        [HttpGet]
        [HttpPost]
        public List<AccountDocumentBiometSamplingModel> get_iid_biomet_account_documents_sampling_details(string AC_NO)
            {
            return dBConnection.GetIidAccountDocuments(AC_NO);
            }

        [HttpGet]
        [HttpPost]
        public List<LoanCaseSampleModel> get_iid_loan_Exceptions(string INDICATOR, int STATUS_ID, int ENG_ID)
            {
            return dBConnection.GetIidLoanExceptions(INDICATOR, STATUS_ID, ENG_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<LoanCaseSampleDocumentsModel> get_iid_loan_documents(int ENG_ID, string LOAN_DISB_ID)
            {
            return dBConnection.GetIidLoanDocuments(ENG_ID, LOAN_DISB_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<LoanCaseSampleDocumentsModel> get_iid_loan_document_data(int IMAGE_ID)
            {
            return dBConnection.GetIidLoanDocumentData(IMAGE_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<LoanCaseSampleTransactionsModel> get_iid_sample_loan_transactions(int ENG_ID, string LOAN_DISB_ID)
            {
            return dBConnection.GetIidLoanTransactions(ENG_ID, LOAN_DISB_ID);
            }

        [HttpPost]
        public IActionResult GetComplaintsDropdown(int pageId)
            {
            try
                {
                var list = dBConnection.GetComplaintsDropdown(pageId);
                var data = list.Select(x => new
                    {
                    complaintId = x.ComplaintId,
                    nature = x.Nature,
                    status = x.Status,
                    displayText = $"{x.ComplaintId} | {x.Nature} | {x.Status}"
                    }).ToList();
                return Json(data);
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetLatestPlanByComplaintId(int complaintId)
            {
            try
                {
                var dt = dBConnection.GetLatestPlanByComplaintId(complaintId);
                if (dt == null || dt.Rows.Count == 0)
                    {
                    return Json(new { ok = false, message = "No investigation plan found for this complaint." });
                    }

                var row = dt.Rows[0];
                var planId = dt.Columns.Contains("PLAN_ID") ? SafeOracleInt(row["PLAN_ID"]) : 0;
                var planDetails = dt.Columns.Contains("PLAN_DETAILS") ? row["PLAN_DETAILS"]?.ToString() : string.Empty;
                var complaintNo = dt.Columns.Contains("COMPLAINT_NO") ? row["COMPLAINT_NO"]?.ToString() : string.Empty;
                var complainantName = dt.Columns.Contains("COMPLAINANT_NAME") ? row["COMPLAINANT_NAME"]?.ToString() : string.Empty;
                var assessment = dt.Columns.Contains("ASSESSMENT") ? row["ASSESSMENT"]?.ToString() : string.Empty;

                return Json(new
                    {
                    ok = true,
                    planId,
                    planDetails,
                    complaintNo,
                    complainantName,
                    assessment
                    });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetLatestInquiryReportByComplaintId(int complaintId)
            {
            try
                {
                var report = dBConnection.GetLatestInquiryReportByComplaintId(complaintId);
                if (report == null || report.ReportId <= 0)
                    {
                    return Json(new { ok = false, message = "No inquiry report found for this complaint." });
                    }

                return Json(new
                    {
                    ok = true,
                    reportId = report.ReportId,
                    nameComplainant = report.NameComplainant,
                    nameAccused = report.NameAccused,
                    gist = report.Gist,
                    proceedings = report.Proceedings,
                    findings = report.Findings,
                    recommendation = report.Recommendation,
                    conclusion = report.Conclusion,
                    reportedInAuditReport = report.ReportedInAuditReport,
                    auditReportReferenceDetail = report.AuditReportReferenceDetail,
                    uploadedReport = report.UploadedReport,
                    uploadedEvidence = report.UploadedEvidence,
                    uploadedDsa = report.UploadedDsa,
                    submittedOn = report.SubmittedOn
                    });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetComplaintsByUser()
            {
            try
                {
                var rows = dBConnection.GetComplaintsByUser();
                return Json(new { ok = true, rows });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetComplaintsWithoutAssessment()
            {
            try
                {
                var list = dBConnection.Get_Complaints_Without_Assessment();
                var data = list.Select(x => new
                    {
                    complaintId = x.ComplaintId,
                    nature = x.Nature,
                    submittedOn = x.SubmittedOn
                    }).ToList();
                return Json(data);
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpGet]
        public IActionResult GetIiUnits()
            {
            var units = dBConnection.GetInspectionUnits()
                .Select(unit => new
                    {
                    unitId = unit.I_ID,
                    unitName = unit.UNIT_NAME
                    })
                .ToList();
            return Ok(units);
            }

        [HttpGet]
        public IActionResult GetIidTaskList()
            {
            var sessionCheck = EnsureAuthenticatedSession();
            if (sessionCheck != null)
                {
                return sessionCheck;
                }

            var user = sessionHandler.GetUser();
            var unitId = user?.UserEntityID ?? 0;
            var dt = unitId > 0 ? dBConnection.GetIidTaskList(unitId) : new DataTable();
            var rows = dt.AsEnumerable().Select(row => new AIS.Models.IID.TaskListRowModel
                {
                ComplaintId = SafeOracleInt(row["COMPLAINT_ID"]),
                ComplaintNo = row["COMPLAINT_NO"]?.ToString(),
                ComplainantName = row["COMPLAINANT_NAME"]?.ToString(),
                AssignedOn = row["ASSIGNED_ON"]?.ToString(),
                Status = row["STATUS"]?.ToString(),
                AssignedUnitId = SafeOracleInt(row["ASSIGNED_UNIT_ID"]),
                PlanId = SafeOracleNullableInt(row["PLAN_ID"])
                }).ToList();

            return Ok(rows);
            }

        [HttpGet]
        public IActionResult GetIidPlanDetails(int complaintId)
            {
            var planDetails = dBConnection.GetIidPlanDetails(complaintId);
            return Ok(planDetails);
            }

        [HttpPost]
        public IActionResult GetComplaint(int complaintId)
            {
            try
                {
                var c = dBConnection.GetComplaint(complaintId);
                if (c == null)
                    {
                    return Json(new { ok = false, message = "Complaint not found" });
                    }

                return Json(new
                    {
                    ok = true,
                    complaintId = c.ComplaintId,
                    complaintNo = c.ComplaintNo,
                    nature = c.Nature,
                    category = c.Category,
                    complainantName = c.ComplainantName,
                    cnic = c.CNIC,
                    cellularNumber = c.CellularNumber,
                    mailingAddress = c.MailingAddress,
                    gender = c.Gender,
                    receivedFrom = c.ReceivedFrom,
                    locationTypeId = c.LocationTypeId,
                    locationTypeText = c.LocationTypeText,
                    gmOfficeId = c.GMOfficeId,
                    regionId = c.RegionId,
                    branchId = c.BranchId,
                    gmOffice = c.GMOffice,
                    region = c.Region,
                    branch = c.Branch,
                    contents = c.Contents,
                    uploadedComplaint = c.UploadedComplaint,
                    uploadedFFR = c.UploadedFFR,
                    uploadedEvidence = c.UploadedEvidence,
                    actionRequired = c.ActionRequired,
                    submittedOn = c.SubmittedOn,
                    status = c.Status,
                    assessment = c.Assessment,
                    assessmentId = c.AssessmentId,
                    recommendation = c.Recommendation,
                    assignedUnitId = c.AssignedUnitId,
                    assignedUnit = c.AssignedUnit
                    });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        private object BuildIidSaveResponse(AIS.Models.IID.InquiryReport.IidInqProcResult rows, string fallbackMessage)
            {
            return new
                {
                ok = rows?.Ok ?? false,
                message = string.IsNullOrWhiteSpace(rows?.Message) ? fallbackMessage : rows.Message,
                id = rows?.Id,
                data = rows
                };
            }

        [HttpPost]
        public IActionResult GetIidInqAccusations([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqAccusationsByComplaintId(request?.ComplaintId ?? 0);
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("ADD_IID_INQ_ACCUSATION", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_ADD_INQ_ACCUSATION", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult AddIidInqAccusation([FromBody] AIS.Models.IID.InquiryReport.IidInqAccusationRow model)
            {
            try
                {
                var rows = dBConnection.AddIidInqAccusation(model);
                return Json(BuildIidSaveResponse(rows, "Accusation saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("UPDATE_IID_INQ_ACCUSATION", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_UPDATE_INQ_ACCUSATION", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult UpdateIidInqAccusation([FromBody] AIS.Models.IID.InquiryReport.IidInqAccusationRow model)
            {
            try
                {
                var rows = dBConnection.UpdateIidInqAccusation(model);
                return Json(BuildIidSaveResponse(rows, "Accusation updated."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("DELETE_IID_INQ_ACCUSATION", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_DELETE_INQ_ACCUSATION", ObjectType = "IID_INQUIRY", ObjectId = "request.Id", RequireResultMessage = true)]
        public IActionResult DeleteIidInqAccusation([FromBody] AIS.Models.IID.InquiryReport.IidInqDeleteRequest request)
            {
            try
                {
                var rows = dBConnection.DeleteIidInqAccusation(request?.Id ?? 0, request?.UserId ?? 0);
                return Json(BuildIidSaveResponse(rows, "Accusation deleted."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidEmployeeInfo([FromBody] AIS.Models.IID.InquiryReport.IidEmployeeInfoRequest request)
            {
            try
                {
                var data = dBConnection.GetIidEmployeeInfo(request?.PpNo ?? 0);
                if (data == null)
                    {
                    return Json(new { ok = false, message = "No employee found for this PPNO", data = (object)null });
                    }
                return Json(new
                    {
                    ok = true,
                    message = "Employee found.",
                    data = new { ppno = data.Ppno, name = data.Name, fatherName = data.FatherName, cnic = data.Cnic }
                    });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message, data = (object)null });
                }
            }

        [HttpPost]
        public IActionResult GetIidInqAccusedList([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqAccusedListByComplaintId(request?.ComplaintId ?? 0);
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("ADD_IID_INQ_ACCUSED", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_ADD_INQ_ACCUSED", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult AddIidInqAccused([FromBody] AIS.Models.IID.InquiryReport.IidInqAccusedRow model)
            {
            try
                {
                var rows = dBConnection.AddIidInqAccused(model);
                return Json(BuildIidSaveResponse(rows, "Accused row saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("UPDATE_IID_INQ_ACCUSED", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_UPDATE_INQ_ACCUSED", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult UpdateIidInqAccused([FromBody] AIS.Models.IID.InquiryReport.IidInqAccusedRow model)
            {
            try
                {
                var rows = dBConnection.UpdateIidInqAccused(model);
                return Json(BuildIidSaveResponse(rows, "Accused row updated."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("DELETE_IID_INQ_ACCUSED", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_DELETE_INQ_ACCUSED", ObjectType = "IID_INQUIRY", ObjectId = "request.Id", RequireResultMessage = true)]
        public IActionResult DeleteIidInqAccused([FromBody] AIS.Models.IID.InquiryReport.IidInqDeleteRequest request)
            {
            try
                {
                var rows = dBConnection.DeleteIidInqAccused(request?.Id ?? 0, request?.UserId ?? 0);
                return Json(BuildIidSaveResponse(rows, "Accused row deleted."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidInqRecords([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqRecordsByComplaintId(request?.ComplaintId ?? 0);
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("ADD_IID_INQ_RECORD", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_ADD_INQ_RECORD", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult AddIidInqRecord([FromBody] AIS.Models.IID.InquiryReport.IidInqRecordRow model)
            {
            try
                {
                var rows = dBConnection.AddIidInqRecord(model);
                return Json(BuildIidSaveResponse(rows, "Record scrutinized row saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("UPDATE_IID_INQ_RECORD", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_UPDATE_INQ_RECORD", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult UpdateIidInqRecord([FromBody] AIS.Models.IID.InquiryReport.IidInqRecordRow model)
            {
            try
                {
                var rows = dBConnection.UpdateIidInqRecord(model);
                return Json(BuildIidSaveResponse(rows, "Record scrutinized row updated."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("DELETE_IID_INQ_RECORD", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_DELETE_INQ_RECORD", ObjectType = "IID_INQUIRY", ObjectId = "request.Id", RequireResultMessage = true)]
        public IActionResult DeleteIidInqRecord([FromBody] AIS.Models.IID.InquiryReport.IidInqDeleteRequest request)
            {
            try
                {
                var rows = dBConnection.DeleteIidInqRecord(request?.Id ?? 0, request?.UserId ?? 0);
                return Json(BuildIidSaveResponse(rows, "Record scrutinized row deleted."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidInqProceedings([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqProceedingsByComplaintId(request?.ComplaintId ?? 0);
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("ADD_IID_INQ_PROCEEDING", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_SAVE_INQ_PROCEEDING", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult AddIidInqProceeding([FromBody] AIS.Models.IID.InquiryReport.IidInqProceedingRow model)
            {
            try
                {
                var rows = dBConnection.SaveIidInqProceeding(model);
                return Json(BuildIidSaveResponse(rows, "Inquiry proceeding row saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("UPDATE_IID_INQ_PROCEEDING", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_SAVE_INQ_PROCEEDING", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult UpdateIidInqProceeding([FromBody] AIS.Models.IID.InquiryReport.IidInqProceedingRow model)
            {
            try
                {
                if (model == null)
                    {
                    return Json(new { ok = false, message = "Inquiry proceeding payload is required." });
                    }

                var rows = dBConnection.SaveIidInqProceeding(model);
                return Json(BuildIidSaveResponse(rows, "Inquiry proceeding row saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("DELETE_IID_INQ_PROCEEDING", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_DELETE_INQ_PROCEEDING", ObjectType = "IID_INQUIRY", ObjectId = "request.Id", RequireResultMessage = true)]
        public IActionResult DeleteIidInqProceeding([FromBody] AIS.Models.IID.InquiryReport.IidInqDeleteRequest request)
            {
            try
                {
                var rows = dBConnection.DeleteIidInqProceeding(request?.Id ?? 0, request?.UserId ?? 0);
                return Json(BuildIidSaveResponse(rows, "Inquiry proceeding row deleted."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidInqStatements([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqStatementsByComplaintId(request?.ComplaintId ?? 0);
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("ADD_IID_INQ_STATEMENT", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_ADD_INQ_STATEMENT", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult AddIidInqStatement([FromBody] AIS.Models.IID.InquiryReport.IidInqStatementRow model)
            {
            try
                {
                var rows = dBConnection.SaveIidInqStatement(model);
                return Json(BuildIidSaveResponse(rows, "Statement row saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("UPDATE_IID_INQ_STATEMENT", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_ADD_INQ_STATEMENT", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult UpdateIidInqStatement([FromBody] AIS.Models.IID.InquiryReport.IidInqStatementRow model)
            {
            try
                {
                if (model == null)
                    {
                    return Json(new { ok = false, message = "Statement payload is required." });
                    }

                var rows = dBConnection.SaveIidInqStatement(model);
                return Json(BuildIidSaveResponse(rows, "Statement row saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult UploadIidInqStatementFile()
            {
            try
                {
                var unauthorized = EnsureAuthenticatedSession();
                if (unauthorized != null)
                    {
                    return unauthorized;
                    }

                if (!Request.HasFormContentType)
                    {
                    return Json(new { ok = false, message = "Form data is required." });
                    }

                var statementFile = Request.Form.Files.GetFile("file");
                if (statementFile == null || statementFile.Length == 0)
                    {
                    return Json(new { ok = false, message = "Statement file is required." });
                    }

                var savedFile = SaveUploadFile(statementFile);
                return Json(new
                    {
                    ok = true,
                    message = "Statement file uploaded.",
                    fileName = savedFile,
                    fileUrl = BuildUploadsFileUrl(savedFile),
                    originalFileName = statementFile.FileName
                    });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("DELETE_IID_INQ_STATEMENT", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_DELETE_INQ_STATEMENT", ObjectType = "IID_INQUIRY", ObjectId = "request.Id", RequireResultMessage = true)]
        public IActionResult DeleteIidInqStatement([FromBody] AIS.Models.IID.InquiryReport.IidInqDeleteRequest request)
            {
            try
                {
                var rows = dBConnection.DeleteIidInqStatement(request?.Id ?? 0, request?.UserId ?? 0);
                return Json(BuildIidSaveResponse(rows, "Statement row deleted."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidInqEvidenceFiles([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqEvidenceFilesByComplaintId(request?.ComplaintId ?? 0);
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidInqEvidenceStep([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqEvidenceStepByComplaintId(request?.ComplaintId ?? 0);
                return Json(new
                    {
                    ok = true,
                    complaintId = data?.ComplaintId ?? 0,
                    materialEvidenceDetail = data?.MaterialEvidenceDetail ?? string.Empty,
                    circumstantialEvidenceDetail = data?.CircumstantialEvidenceDetail ?? string.Empty,
                    evidenceFiles = data?.EvidenceFiles ?? new List<AIS.Models.IID.InquiryReport.IidInqEvidenceFileRow>()
                    });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("IID_INQUIRY_EVIDENCE_STEP_SAVED", "FILES_EVIDENCE", "IID EVIDENCE", "PKG_INQ", "P_SAVE_INQ_EVIDENCE_STEP", ObjectType = "COMPLAINT", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult SaveIidInqEvidenceStep([FromBody] AIS.Models.IID.InquiryReport.IidInqEvidenceStepModel model)
            {
            try
                {
                if (model == null || model.ComplaintId <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required for evidence details." });
                    }

                var rows = dBConnection.SaveIidInqEvidenceStep(model);
                return Json(BuildIidSaveResponse(rows, "Evidence details saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("IID_INQUIRY_EVIDENCE_FILE_ADDED", "FILES_EVIDENCE", "IID EVIDENCE", "PKG_INQ", "P_ADD_INQ_EVIDENCE_FILE", ObjectType = "COMPLAINT", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult AddIidInqEvidenceFile([FromForm] AIS.Models.IID.InquiryReport.IidInqEvidenceFileRow model)
            {
            try
                {
                var unauthorized = EnsureAuthenticatedSession();
                if (unauthorized != null)
                    {
                    return unauthorized;
                    }

                if (Request.HasFormContentType)
                    {
                    var evidenceFile = Request.Form.Files.GetFile("file");
                    if (evidenceFile != null)
                        {
                        var savedFile = SaveUploadFile(evidenceFile);
                        model.FileName = evidenceFile.FileName;
                        model.FilePath = savedFile;
                        model.FileExt = Path.GetExtension(evidenceFile.FileName);
                        model.FileSizeKb = Convert.ToInt32(Math.Ceiling(evidenceFile.Length / 1024d));
                        }
                    }

                var rows = dBConnection.AddIidInqEvidenceFile(model);
                return Json(BuildIidSaveResponse(rows, "Evidence file uploaded."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("IID_INQUIRY_EVIDENCE_FILE_DELETED", "FILES_EVIDENCE", "IID EVIDENCE", "PKG_INQ", "P_DELETE_INQ_EVIDENCE_FILE", ObjectType = "EVIDENCE_FILE", ObjectId = "request.Id", RequireResultMessage = true)]
        public IActionResult DeleteIidInqEvidenceFile([FromBody] AIS.Models.IID.InquiryReport.IidInqDeleteRequest request)
            {
            try
                {
                var unauthorized = EnsureAuthenticatedSession();
                if (unauthorized != null)
                    {
                    return unauthorized;
                    }

                var rows = dBConnection.DeleteIidInqEvidenceFile(request?.Id ?? 0, request?.UserId ?? 0);
                return Json(BuildIidSaveResponse(rows, "Evidence file deleted."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }


        [HttpPost]
        public IActionResult GetIidInqFindingsRecomm([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqFindingsRecommByComplaintId(request?.ComplaintId ?? 0);
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("SAVE_IID_INQ_FINDINGS_RECOMM", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_SUBMIT_COMPLAINT", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult SaveIidInqFindingsRecomm([FromBody] AIS.Models.IID.InquiryReport.IidInqFindingsRecommRow model)
            {
            try
                {
                var accusationId = model?.AccusationId ?? 0;
                if (accusationId <= 0)
                    {
                    return Json(new { ok = false, message = "Please select an accusation saved in Section 2 before saving findings/recommendation." });
                    }

                var rows = dBConnection.SaveIidInqFindingsRecomm(
                    model?.ComplaintId ?? 0,
                    accusationId,
                    model?.FindingText,
                    model?.RecommendationText,
                    model?.Outcome);
                return Json(BuildIidSaveResponse(rows, "Findings and recommendations saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidAccusationsForFindings([FromBody] AIS.Models.IID.InquiryReport.ComplaintIdRequest request)
            {
            try
                {
                var rows = dBConnection.GetIidAccusationsForFindings(request?.ComplaintId ?? 0);
                return Json(new { ok = true, rows });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message, rows = new object[0] });
                }
            }

        [HttpPost]
        public IActionResult GetIidFindingsRecommByAccusation([FromBody] AIS.Models.IID.InquiryReport.FindingsRecommGetRequest request)
            {
            try
                {
                var model = dBConnection.GetIidFindingsRecommByAccusation(request?.ComplaintId ?? 0, request?.AccusationId ?? 0);
                return Json(new
                    {
                    ok = true,
                    complaintId = model?.ComplaintId ?? (request?.ComplaintId ?? 0),
                    accusationId = model?.AccusationId ?? (request?.AccusationId ?? 0),
                    findingText = model?.FindingText ?? string.Empty,
                    recomText = model?.RecommendationText ?? string.Empty,
                    outcome = model?.Outcome ?? string.Empty,
                    ppno = model?.Ppno ?? string.Empty,
                    savedOn = model?.UpdatedOn
                    });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("SAVE_IID_FINDINGS_RECOMM_BY_ACCUSATION", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "SAVE_INQ_FINDINGS_RECOMM", ObjectType = "IID_INQUIRY", ObjectId = "request.AccusationId", RequireResultMessage = true)]
        public IActionResult SaveIidFindingsRecommByAccusation([FromBody] AIS.Models.IID.InquiryReport.FindingsRecommRequest request)
            {
            try
                {
                var accusationId = request?.AccusationId ?? 0;
                if (accusationId <= 0)
                    {
                    return Json(new { ok = false, message = "Please select an accusation saved in Section 2 before saving findings/recommendation." });
                    }

                var ppno = sessionHandler?.GetUser()?.PPNumber ?? string.Empty;
                var rows = dBConnection.SaveIidFindingsRecommByAccusation(
                    request?.ComplaintId ?? 0,
                    accusationId,
                    request?.FindingText,
                    request?.RecomText,
                    request?.Outcome,
                    ppno);
                var savedModel = dBConnection.GetIidFindingsRecommByAccusation(request?.ComplaintId ?? 0, accusationId);
                return Json(new
                    {
                    ok = rows?.Ok ?? false,
                    message = string.IsNullOrWhiteSpace(rows?.Message) ? "Findings and recommendation saved." : rows.Message,
                    outcome = savedModel?.Outcome ?? request?.Outcome ?? string.Empty,
                    savedOn = savedModel?.UpdatedOn ?? DateTime.Now
                    });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidFindingsRecommStatus([FromBody] AIS.Models.IID.InquiryReport.ComplaintIdRequest request)
            {
            try
                {
                var rows = dBConnection.GetIidFindingsRecommStatus(request?.ComplaintId ?? 0);
                return Json(new { ok = true, rows });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message, rows = new object[0] });
                }
            }

        [HttpPost]
        public IActionResult GetIidInqViolations([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqViolationsByComplaintId(request?.ComplaintId ?? 0);
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("ADD_IID_INQ_VIOLATION", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_ADD_INQ_VIOLATION", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult AddIidInqViolation([FromBody] AIS.Models.IID.InquiryReport.IidInqViolationRow model)
            {
            try
                {
                var rows = dBConnection.AddIidInqViolation(model);
                return Json(BuildIidSaveResponse(rows, "Violation row saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("UPDATE_IID_INQ_VIOLATION", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_UPDATE_INQ_VIOLATION", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult UpdateIidInqViolation([FromBody] AIS.Models.IID.InquiryReport.IidInqViolationRow model)
            {
            try
                {
                var rows = dBConnection.UpdateIidInqViolation(model);
                return Json(BuildIidSaveResponse(rows, "Violation row updated."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("DELETE_IID_INQ_VIOLATION", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_DELETE_INQ_VIOLATION", ObjectType = "IID_INQUIRY", ObjectId = "request.Id", RequireResultMessage = true)]
        public IActionResult DeleteIidInqViolation([FromBody] AIS.Models.IID.InquiryReport.IidInqDeleteRequest request)
            {
            try
                {
                var rows = dBConnection.DeleteIidInqViolation(request?.Id ?? 0, request?.UserId ?? 0);
                return Json(BuildIidSaveResponse(rows, "Violation row deleted."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidInqDsa([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var data = dBConnection.GetIidInqDsaByComplaintId(request?.ComplaintId ?? 0);
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("ADD_IID_INQ_DSA", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_ADD_INQ_DSA", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult AddIidInqDsa([FromBody] AIS.Models.IID.InquiryReport.IidInqDsaRow model)
            {
            try
                {
                var rows = dBConnection.AddIidInqDsa(model);
                return Json(BuildIidSaveResponse(rows, "DSA row saved."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("UPDATE_IID_INQ_DSA", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_UPDATE_INQ_DSA", ObjectType = "IID_INQUIRY", ObjectId = "model.ComplaintId", RequireResultMessage = true)]
        public IActionResult UpdateIidInqDsa([FromBody] AIS.Models.IID.InquiryReport.IidInqDsaRow model)
            {
            try
                {
                var rows = dBConnection.UpdateIidInqDsa(model);
                return Json(BuildIidSaveResponse(rows, "DSA row updated."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("DELETE_IID_INQ_DSA", "INQUIRY_COMPLAINT", "IID INQUIRY", "PKG_INQ", "P_DELETE_INQ_DSA", ObjectType = "IID_INQUIRY", ObjectId = "request.Id", RequireResultMessage = true)]
        public IActionResult DeleteIidInqDsa([FromBody] AIS.Models.IID.InquiryReport.IidInqDeleteRequest request)
            {
            try
                {
                var rows = dBConnection.DeleteIidInqDsa(request?.Id ?? 0, request?.UserId ?? 0);
                return Json(BuildIidSaveResponse(rows, "DSA row deleted."));
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidInqWizardData([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var complaintId = request?.ComplaintId ?? 0;
                var latestReport = complaintId > 0
                    ? dBConnection.GetLatestInquiryReportByComplaintId((int)complaintId)
                    : null;
                var data = new
                    {
                    accusations = dBConnection.GetIidInqAccusationsByComplaintId(complaintId),
                    accused = dBConnection.GetIidInqAccusedListByComplaintId(complaintId),
                    recordsScrutinized = dBConnection.GetIidInqRecordsByComplaintId(complaintId),
                    inquiryProceedings = dBConnection.GetIidInqProceedingsByComplaintId(complaintId),
                    statementsRegister = dBConnection.GetIidInqStatementsByComplaintId(complaintId),
                    evidenceStep = dBConnection.GetIidInqEvidenceStepByComplaintId(complaintId),
                    evidenceFiles = dBConnection.GetIidInqEvidenceFilesByComplaintId(complaintId),
                    findingsRecommendations = dBConnection.GetIidInqFindingsRecommByComplaintId(complaintId),
                    violations = dBConnection.GetIidInqViolationsByComplaintId(complaintId),
                    dsa = dBConnection.GetIidInqDsaByComplaintId(complaintId),
                    inquiryNarrative = latestReport
                    };
                return Json(new { ok = true, data });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("IID_INQUIRY_REPORT_FINALIZED", "AUDIT_REPORT", "INQUIRY REPORT", "PKG_INQ", "P_FINALIZE_IID_INQUIRY_REPORT", ObjectType = "COMPLAINT", ObjectId = "request.ComplaintId", RequireResultMessage = true)]
        public IActionResult FinalizeIidInquiryReport([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var unauthorized = EnsureAuthenticatedSession();
                if (unauthorized != null)
                    {
                    return unauthorized;
                    }

                var complaintId = request?.ComplaintId ?? 0;
                if (complaintId <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required." });
                    }

                var rows = dBConnection.FinalizeIidInquiryReport(complaintId, sessionHandler.GetUser()?.UserEntityID);
                return Json(new { ok = rows > 0, message = rows > 0 ? "Inquiry report finalized successfully." : "Finalization failed." });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        [ApplicationAudit("IID_INQUIRY_REPORT_SUBMITTED_FOR_ANALYSIS", "AUDIT_REPORT", "INQUIRY REPORT", "PKG_INQ", "P_SUBMIT_IID_INQUIRY_REPORT", ObjectType = "COMPLAINT", ObjectId = "request.ComplaintId", RequireResultMessage = true)]
        public IActionResult SubmitIidInquiryReportForAnalysis([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var unauthorized = EnsureAuthenticatedSession();
                if (unauthorized != null)
                    {
                    return unauthorized;
                    }

                var complaintId = request?.ComplaintId ?? 0;
                if (complaintId <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required." });
                    }

                var evidence = dBConnection.GetIidInqEvidenceFilesByComplaintId(complaintId);
                if (evidence == null || evidence.Count == 0)
                    {
                    return Json(new { ok = false, message = "At least one evidence file is required before submitting for analysis." });
                    }

                var rows = dBConnection.SubmitIidInquiryReportForAnalysis(complaintId, sessionHandler.GetUser()?.UserEntityID);
                return Json(new { ok = rows > 0, message = rows > 0 ? "Inquiry report submitted for analysis successfully." : "Submission for analysis failed." });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult GetIidInquiryReportReadOnlyData([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            try
                {
                var complaintId = request?.ComplaintId ?? 0;
                if (complaintId <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required." });
                    }

                var complaint = dBConnection.GetComplaint((int)complaintId);
                if (complaint == null)
                    {
                    return Json(new { ok = false, message = "Complaint not found." });
                    }

                var findings = dBConnection.GetIidInqFindingsRecommByComplaintId(complaintId).FirstOrDefault();
                var evidenceStep = dBConnection.GetIidInqEvidenceStepByComplaintId(complaintId);
                var proceedings = dBConnection.GetIidInqProceedingsByComplaintId(complaintId);
                var latestReport = dBConnection.GetLatestInquiryReportByComplaintId((int)complaintId);
                var proceedingsNarrative = string.Join(Environment.NewLine, (proceedings ?? new List<AIS.Models.IID.InquiryReport.IidInqProceedingRow>()).Select(x => string.Join(" | ", new[]
                    {
                    x.NoticeReference,
                    x.VisitDate?.ToString("yyyy-MM-dd"),
                    x.PlaceVisited,
                    x.ParticipantsDetail,
                    x.MissingParticipantsReason
                    }.Where(v => !string.IsNullOrWhiteSpace(v)))));

                var payload = new
                    {
                    complaintHeader = new
                        {
                        complaintId = complaint.ComplaintId,
                        complaintNo = complaint.ComplaintNo,
                        nature = complaint.Nature,
                        source = complaint.ReceivedFrom,
                        unitName = complaint.AssignedUnit,
                        status = complaint.Status,
                        complainantName = complaint.ComplainantName,
                        finalizeState = dBConnection.GetIidComplaintFinalizeState(complaintId),
                        isFinalized = dBConnection.IsIidComplaintFinalized(complaintId)
                        },
                    accusations = dBConnection.GetIidInqAccusationsByComplaintId(complaintId),
                    accused = dBConnection.GetIidInqAccusedListByComplaintId(complaintId),
                    recordsScrutinized = dBConnection.GetIidInqRecordsByComplaintId(complaintId),
                    inquiryProceedings = proceedings,
                    statementRegister = dBConnection.GetIidInqStatementsByComplaintId(complaintId),
                    evidenceFiles = evidenceStep?.EvidenceFiles ?? dBConnection.GetIidInqEvidenceFilesByComplaintId(complaintId),
                    evidenceStep = evidenceStep,
                    violations = dBConnection.GetIidInqViolationsByComplaintId(complaintId),
                    dsa = dBConnection.GetIidInqDsaByComplaintId(complaintId),
                    finalApprovals = new object[0],
                    inquiryReport = new
                        {
                        reportId = latestReport?.ReportId ?? 0,
                        proceedings = string.IsNullOrWhiteSpace(proceedingsNarrative) ? latestReport?.Proceedings ?? string.Empty : proceedingsNarrative,
                        findings = findings?.FindingText ?? latestReport?.Findings ?? string.Empty,
                        recommendation = findings?.RecommendationText ?? latestReport?.Recommendation ?? string.Empty,
                        conclusion = latestReport?.Conclusion ?? string.Empty,
                        reportedInAuditReport = latestReport?.ReportedInAuditReport ?? string.Empty,
                        auditReportReferenceDetail = latestReport?.AuditReportReferenceDetail ?? string.Empty
                        }
                    };

                return Json(payload);
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }


        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_PUBLIC_HOLIDAY", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_INSERT_PUBLIC_HOLIDAY", ObjectType = "ADD_PUBLIC_HOLIDAY")]
        public PublicHolidayModel add_public_holiday([FromBody] PublicHolidayModel model)
            {
            return dBConnection.AddPublicHoliday(model);
            }

        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public List<PublicHolidayModel> get_all_public_holidays(HolidayYearModel input)
            {
            int year = input?.year ?? 0;
            return dBConnection.GetAllPublicHolidays(year);
            }
        [HttpPost]
        public string check_public_holiday_day(String dat)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.CheckIfHolidayOrWeekend(dat) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<FadDeskOfficerRptModel> get_fad_desk_officer_rpt_by_date_range(string startDate, string endDate)
            {
            DateTime sDate = DateTime.Parse(startDate);
            DateTime eDate = DateTime.Parse(endDate);
            return dBConnection.GetFadDeskOfficerRptByDateRange(sDate, eDate);
            }

        [HttpGet]
        public List<ParaTextModel> GetAllParaText(int comId)
            {
            return dBConnection.Get_All_Para_Text(comId);

            }

        //[HttpPost]
        //public List<AuditEmployeeModel> GetAuditEmployees(int entityId)
        //    {
        //    return dBConnection.GetAuditEmployees(entityId);
        //    }

        [HttpGet]
        public IActionResult GetParaReferenceData(int comId)
            {
            var data = dBConnection.GetParaReferenceData(comId);
            return Json(data);
            }

        [HttpPost]
        [ApplicationAudit("PARA_REFERENCES_SAVED", "COMPLIANCE", "PARA MANAGEMENT", "PKG_AR", "P_SAVE_PARA_REFERENCES", ComId = "model.ComId", ObjectType = "PARA", ObjectId = "model.ComId")]
        public string SaveParaReferences([FromBody] SaveParaReferencesRequestModel model)
            {
            return dBConnection.SaveParaReferences(model.ComId.GetValueOrDefault(), model.References);
            }

        [HttpPost]
        public List<ReferenceSearchResultModel> SearchReferences(string referenceType, string keyword)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<ReferenceSearchResultModel>();
                }
            return dBConnection.SearchReferences(referenceType, keyword);
            }

        [HttpGet]
        public IActionResult GetReferenceDetail(int refId)
            {
            var detail = dBConnection.GetReferenceDetail(refId);
            return Json(detail);
            }

        [HttpGet]
        public IActionResult GetManualMaster()
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<ManualMasterItemModel>());
                }
            return Json(dBConnection.GetManualMaster());
            }

        [HttpGet]
        public IActionResult GetManualSections(long manualId)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<ManualSectionItemModel>());
                }
            return Json(dBConnection.GetManualSections(manualId));
            }

        [HttpGet]
        public IActionResult GetManualChapters(long manualId, string sectionName)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<ManualChapterItemModel>());
                }
            return Json(dBConnection.GetManualChapters(manualId, sectionName));
            }

        [HttpGet]
        public IActionResult GetManualIndexByChapter(long manualId, string sectionName, string chapterNo)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<ManualIndexItemModel>());
                }
            return Json(dBConnection.GetManualIndexByChapter(manualId, sectionName, chapterNo));
            }

        [HttpGet]
        public IActionResult GetReferenceMasterDetail(string searchText, string sourceType, long? refId)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<ReferenceMasterDetailItemModel>());
                }

            return Json(dBConnection.GetReferenceMasterDetail(searchText, sourceType, refId));
            }

        [HttpGet]
        public IActionResult GetObservationManualMaster()
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<ManualMasterItemModel>());
                }

            return Json(dBConnection.GetObservationManualMaster());
            }

        [HttpGet]
        public IActionResult GetObservationManualSections(long manualId)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<ManualSectionItemModel>());
                }

            return Json(dBConnection.GetObservationManualSections(manualId));
            }

        [HttpGet]
        public IActionResult GetObservationManualChapters(long manualId, string sectionText)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<ManualChapterItemModel>());
                }

            return Json(dBConnection.GetObservationManualChapters(manualId, sectionText));
            }

        [HttpGet]
        public IActionResult GetObservationManualReferenceGrid(long manualId, string sectionText, string chapterNo, string sourceType = null)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<ManualIndexItemModel>());
                }

            return Json(dBConnection.GetObservationManualReferenceGrid(manualId, sectionText, chapterNo));
            }

        [HttpGet]
        public IActionResult GetReferenceDetailByRefId(long refId)
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(null);
                }

            return Json(dBConnection.GetReferenceDetailByRefId(refId));
            }


        [HttpGet]
        public JsonResult GetEntityTaskSummary()
            {
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return Json(Array.Empty<object>());
                }
            var list = dBConnection.GetEntityTaskSummary();
            return Json(list);
            }

        [HttpGet]
        public List<VersionHistoryModel> GetAllVersionHistory()
            {
            return dBConnection.GetAllVersionHistory();
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_VERSION_HISTORY", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_ADD_VERSION_HISTORY", ObjectType = "ADDVERSIONHISTORY", RequireResultMessage = true)]
        public string AddVersionHistory([FromBody] VersionHistoryModel model)
            {
            var result = dBConnection.AddVersionHistory(model);
            _staticAssetVersionTokenProvider.Invalidate();
            return result;
            }

        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPDATE_VERSION_HISTORY", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_UPDATE_VERSION_HISTORY", ObjectType = "UPDATEVERSIONHISTORY", RequireResultMessage = true)]
        public string UpdateVersionHistory([FromBody] VersionHistoryModel model)
            {
            var result = dBConnection.UpdateVersionHistory(model);
            _staticAssetVersionTokenProvider.Invalidate();
            return result;
            }

        [HttpGet]
        [HttpPost]
        public IActionResult GET_PARA_STATUS_CHANGE_REQUEST(int entityId, int status)
            {
            var data = dBConnection.GETPARASTATUSCHANGEREQUEST(entityId, status);
            return PartialView("~/Views/FAD/_ParaStatusTable.cshtml", data);
            }

        [HttpPost]
        [ApplicationAudit("PARA_STATUS_CHANGE_REQUEST_ADDED", "COMPLIANCE", "PARA STATUS", "PKG_FAD", "P_ADD_PARA_STATUS_CHANGE", ComId = "comId", ObjectType = "PARA", ObjectId = "comId", RequireResultMessage = true)]
        public IActionResult ADD_PARA_STATUS_CHANGE_REQUEST(int comId, int newStatus, string makerRemarks)
            {
            var user = sessionHandler.GetUser();
            if (user == null
                || user.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(user.PPNumber)
                || user.UserRoleID <= 0)
                {
                return Json(new { message = string.Empty });
                }
            var resp = dBConnection.ADDPARASTATUSCHANGEREQUEST(comId, newStatus, makerRemarks, user.ID);
            return Json(new { message = resp });
            }
        [HttpGet]
        [HttpPost]
        public IActionResult GET_PARA_STATUS_AUTHORIZATION()
            {
            var data = dBConnection.GETPARASTATUSAUTHORIZATION();
            return PartialView("~/Views/FAD/_AuthorizeParaStatusTable.cshtml", data);
            }

        [HttpPost]
        [ApplicationAudit("PARA_STATUS_CHANGE_REQUEST_AUTHORIZED", "COMPLIANCE", "PARA STATUS", "PKG_FAD", "P_AUTHORIZE_PARA_STATUS_CHANGE", ObjectType = "PARA_STATUS_LOG", ObjectId = "logId", RequireResultMessage = true)]
        public IActionResult AUTHORIZE_PARA_STATUS_CHANGE_REQUEST(int logId, string action, string authRemarks)
            {
            var user = sessionHandler.GetUser();
            if (user == null
                || user.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(user.PPNumber)
                || user.UserRoleID <= 0)
                {
                return Json(new { message = string.Empty });
                }
            var resp = dBConnection.AUTHORIZEPARASTATUSCHANGEREQUEST(logId, action, authRemarks, user.ID);
            return Json(new { message = resp });
            }

        [HttpGet]
        public string GetIASPARATEXT(int comId)
            {
            return dBConnection.GetIASParaText(comId);
            }

        private IActionResult ValidateExceptionReportFormat(ExceptionReportFormatModel model)
            {
            if (model == null)
                {
                return InvalidRequestResponse("request", "Invalid data supplied.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            model.ColumnHeader = model.ColumnHeader?.Trim();
            model.IsActive = string.IsNullOrWhiteSpace(model.IsActive) ? "Y" : model.IsActive.Trim().ToUpperInvariant();

            if (!IsValidExceptionReportText(model.ColumnHeader))
                {
                return InvalidRequestResponse("ColumnHeader", "Column header must contain only letters, numbers, spaces, ampersand (&), comma (,), and question mark (?).");
                }

            return null;
            }

        private static bool IsValidExceptionReportText(string value)
            {
            return !string.IsNullOrWhiteSpace(value) && ExceptionReportTextRegex.IsMatch(value.Trim());
            }

        private IActionResult ValidateCommercialAuditOm(CommercialAuditOmModel model)
            {
            if (model == null)
                {
                if (!ModelState.IsValid)
                    {
                    return InvalidModelStateResponse("Model binding failed");
                    }

                return InvalidRequestResponse(
                    "request",
                    "Model binding failed. Ensure the request body is valid JSON matching CommercialAuditOmModel.",
                    "Model binding failed");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            model.OmNo = model.OmNo?.Trim();
            model.GistOfOm = model.GistOfOm?.Trim();
            model.IsActive = string.IsNullOrWhiteSpace(model.IsActive) ? "Y" : model.IsActive.Trim();

            if (!model.AuditYearId.HasValue || model.AuditYearId <= 0)
                {
                return InvalidRequestResponse("AuditYearId", "Audit Year is required.");
                }

            if (string.IsNullOrWhiteSpace(model.OmNo))
                {
                return InvalidRequestResponse("OmNo", "OM No is required.");
                }

            if (string.IsNullOrWhiteSpace(model.GistOfOm))
                {
                return InvalidRequestResponse("GistOfOm", "Gist of OM is required.");
                }

            if (!HasMeaningfulRichTextContent(model.BodyOfOm))
                {
                return InvalidRequestResponse("BodyOfOm", "Body of OM is required.");
                }

            return null;
            }

        private IActionResult ValidateCommercialAuditPdp(CommercialAuditPdpModel model)
            {
            if (model == null)
                {
                return InvalidRequestResponse("request", "Invalid data supplied.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            if (!model.AuditYearId.HasValue || model.AuditYearId <= 0)
                {
                return InvalidRequestResponse("AuditYearId", "Audit Year is required.");
                }

            return null;
            }

        private IActionResult ValidateCommercialAuditPdpMapping(CommercialAuditPdpOmMappingSaveRequest model)
            {
            if (model == null)
                {
                return InvalidRequestResponse("request", "Invalid data supplied.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            if (!model.PdpId.HasValue || model.PdpId <= 0)
                {
                return InvalidRequestResponse("PdpId", "PDP is required.");
                }

            return null;
            }

        private IActionResult ValidateCommercialAuditArpsePdpMapping(CommercialAuditArpsePdpMappingSaveRequest model)
            {
            if (model == null)
                {
                return InvalidRequestResponse("request", "Invalid data supplied.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            if (!model.ArpseId.HasValue || model.ArpseId <= 0)
                {
                return InvalidRequestResponse("ArpseId", "ARPSE is required.");
                }

            return null;
            }

        private IActionResult ValidateCommercialAuditArpseHeader(CommercialAuditArpseHeaderModel model)
            {
            if (model == null)
                {
                return InvalidRequestResponse("request", "Invalid data supplied.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            if (!model.ArpseYearId.HasValue || model.ArpseYearId <= 0)
                {
                return InvalidRequestResponse("ArpseYearId", "ARPSE Year is required.");
                }

            if (!HasMeaningfulRichTextContent(model.BodyOfPara))
                {
                return InvalidRequestResponse("BodyOfPara", "Body of Para is required.");
                }

            return null;
            }

        private IActionResult ValidateCommercialAuditArpseDacEntry(CommercialAuditArpseDacEntryModel model)
            {
            if (model == null)
                {
                return InvalidRequestResponse("request", "Invalid data supplied.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            if (!model.ArpseId.HasValue || model.ArpseId <= 0)
                {
                return InvalidRequestResponse("ArpseId", "Save the ARPSE header first.");
                }

            return null;
            }

        private IActionResult ValidateCommercialAuditArpsePacEntry(CommercialAuditArpsePacEntryModel model)
            {
            if (model == null)
                {
                return InvalidRequestResponse("request", "Invalid data supplied.");
                }

            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            if (!model.ArpseId.HasValue || model.ArpseId <= 0)
                {
                return InvalidRequestResponse("ArpseId", "Save the ARPSE header first.");
                }

            return null;
            }

        private static bool TryParseIsoDate(string value, out DateTime date)
            {
            return DateTime.TryParseExact(value ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            }


        [HttpGet]
        [HttpPost]
        public IActionResult GetBackOfficeDashboardEngagements()
            {
            var list = dBConnection.GetBackOfficeDashboardEngagements()
                .Where(item => item.ENG_ID > 0)
                .GroupBy(item => item.ENG_ID)
                .Select(group => group.First())
                .OrderBy(item => item.ENTITY_NAME)
                .Select(item => new
                    {
                    ENG_ID = item.ENG_ID,
                    ENGAGEMENT_NAME = item.ENTITY_NAME,
                    DISPLAY_TEXT = string.IsNullOrWhiteSpace(item.DISPLAY_TEXT) ? item.ENTITY_NAME : item.DISPLAY_TEXT,
                    STATUS_ID = item.STATUS_ID,
                    STATUS_NAME = item.STATUS_NAME,
                    engagementId = item.ENG_ID,
                    entityName = item.ENTITY_NAME,
                    displayText = string.IsNullOrWhiteSpace(item.DISPLAY_TEXT) ? item.ENTITY_NAME : item.DISPLAY_TEXT,
                    statusId = item.STATUS_ID,
                    statusName = item.STATUS_NAME,
                    label = string.IsNullOrWhiteSpace(item.DISPLAY_TEXT) ? item.ENTITY_NAME : item.DISPLAY_TEXT
                    })
                .ToList();

            return Json(list);
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

    public class ObsDeleteRequestDto
        {
        public long? ParaId { get; set; }
        public string Reason { get; set; }
        }

    public class RespDeleteRequestDto
        {
        public long? ResponseId { get; set; }
        public string Reason { get; set; }
        }

    public class ReverseRequestDto
        {
        public long? RequestIdToReverse { get; set; }
        public string Reason { get; set; }
        }

    public class ApproveRejectDto
        {
        public long? RequestId { get; set; }
        public string Reason { get; set; }


        }
    }
