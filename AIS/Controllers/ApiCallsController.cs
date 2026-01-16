using AIS.Exceptions;
using AIS.Models;
using AIS.Models.AIS.Models;
using AIS.Models.AIS.Models.Execution;
using AIS.Models.HD;
using AIS.Models.Reports;
using AIS.Models.SM;
using AIS.Models.Requests;
using AIS.Security.Cryptography;
using AIS.Security.PasswordPolicy;
using AIS.Services;
using AIS.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServicesCsvSanitizer = AIS.Services.CsvSanitizer;


namespace AIS.Controllers
    {
    [IgnoreAntiforgeryToken]
    [EnableRateLimiting("GeneralApiPolicy")]
    public class ApiCallsController : Controller
        {
        private readonly ILogger<ApiCallsController> _logger;

        private readonly SessionHandler sessionHandler;
        private readonly DBConnection dBConnection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly SecurityTokenService _tokenService;
        private readonly PasswordPolicyValidator _passwordPolicyValidator;
        private static readonly Regex AlphaNumericWithSpacesRegex = new Regex("^[A-Za-z0-9 &]+$", RegexOptions.Compiled);

        public ApiCallsController(
            ILogger<ApiCallsController> logger,
            SessionHandler _sessionHandler,
            DBConnection _dbCon,
            IWebHostEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            SecurityTokenService tokenService,
            PasswordPolicyValidator passwordPolicyValidator)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _tokenService = tokenService;
            _passwordPolicyValidator = passwordPolicyValidator;
            }

        public override void OnActionExecuting(ActionExecutingContext context)
            {
            base.OnActionExecuting(context);
            }

        private IActionResult EnsureAuthenticatedSession()
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { error = "unauthorized", message = "User session is not authenticated." });
                }

            return null;
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

        private IActionResult EnsureSbpAccess()
            {
            if (!sessionHandler.HasSbpAccess())
                {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "forbidden", message = "SBP access is not authorized for this session." });
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

        private IActionResult InvalidModelStateResponse()
            {
            var endpointName = ControllerContext?.ActionDescriptor?.DisplayName ?? "Unknown";
            var errors = ValidationErrorHelper.BuildModelErrors(ModelState);
            ValidationErrorHelper.LogValidationErrors(_logger, endpointName, ModelState);
            HttpContext.Items["AjaxModelErrors"] = errors;
            return BadRequest(ValidationErrorHelper.BuildInvalidRequestResponse(ModelState));
            }

        private IActionResult InvalidRequestResponse(string field, string error)
            {
            var endpointName = ControllerContext?.ActionDescriptor?.DisplayName ?? "Unknown";
            var response = ValidationErrorHelper.BuildInvalidRequestResponse(field, error);
            _logger.LogWarning("Validation failed for {Endpoint}. Errors: {@Errors}", endpointName, response.Errors);
            HttpContext.Items["AjaxModelErrors"] = response.Errors;
            return BadRequest(response);
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

            dBConnection.UpdateAuditCriteria(model, CRITERIA_LIST[8]);
            return Ok(new { Status = true });
            }

        [HttpPost]
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

            dBConnection.UpdateAuditCriteria(model, CRITERIA_LIST[8]);
            return Ok(new { Status = true });
            }

        [HttpPost]
        public IActionResult ReferredBackAuditCriteria([FromForm] List<CriteriaIDComment> DATALIST)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            if (DATALIST != null && DATALIST.Count > 0)
                {
                foreach (var criteria in DATALIST)
                    {
                    dBConnection.SetAuditCriteriaStatusReferredBack(criteria.ID.GetValueOrDefault(), criteria.COMMENT);
                    }
                }

            return Ok(true);
            }

        [HttpPost]
        public IActionResult AuthorizeAuditCriteria([FromForm] List<CriteriaIDComment> DATALIST)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            if (DATALIST != null && DATALIST.Count > 0)
                {
                foreach (var criteria in DATALIST)
                    {
                    dBConnection.SetAuditCriteriaStatusApprove(criteria.ID.GetValueOrDefault(), criteria.COMMENT);
                    }
                }

            return Ok(true);
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
        public IActionResult AddEngagementPlan([FromForm] AuditEngagementPlanModel eng)
            {
            var unauthorized = EnsureAuthenticatedSession();
            if (unauthorized != null)
                {
                return unauthorized;
                }

            return Ok(dBConnection.AddAuditEngagementPlan(eng));
            }

        [HttpPost]
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

            return Ok(new { Status = true });
            }

        [HttpPost]
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
        public List<AuditPlanEngagementModel> getauditplanengagement(int b_id)
            {
            return dBConnection.GetAuditPlanEngagement(b_id);
            }

        [HttpPost]
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
        public RiskProcessDefinition process_add(RiskProcessDefinition proc)
            {
            return dBConnection.AddRiskProcess(proc);
            }
        [HttpPost]
        public RiskProcessDetails sub_process_add(RiskProcessDetails subProc)
            {
            return dBConnection.AddRiskSubProcess(subProc);
            }
        [HttpPost]
        public RiskProcessTransactions sub_process_transaction_add(RiskProcessTransactions tran)
            {
            return dBConnection.AddRiskSubProcessTransaction(tran);
            }

        [HttpPost]
        public string authorize_sub_process_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeSubProcessByAuthorizer(T_ID, COMMENTS) + "\"}";

            }
        [HttpPost]
        public string reffered_back_sub_process_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RefferedBackSubProcessByAuthorizer(T_ID, COMMENTS) + "\"}";

            }

        [HttpPost]
        public string recommend_process_transaction_by_reviewer(int T_ID, string COMMENTS, int PROCESS_DETAIL_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int V_ID = 0, int CONTROL_ID = 0, int ROLE_ID = 0, int RISK_ID = 0, string ANNEX_CODE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RecommendProcessTransactionByReviewer(T_ID, COMMENTS, PROCESS_DETAIL_ID, SUB_PROCESS_ID, HEADING, V_ID, CONTROL_ID, ROLE_ID, RISK_ID, ANNEX_CODE) + "\"}";

            }

        [HttpPost]
        public string reffered_back_process_transaction_by_reviewer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RefferedBackProcessTransactionByReviewer(T_ID, COMMENTS) + "\"}";
            }
        [HttpPost]
        public string authorize_process_transaction_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeProcessTransactionByAuthorizer(T_ID, COMMENTS) + "\"}";

            }
        [HttpPost]
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
            var db = CreateDbConnection();
            var result = db.ValidateSbpAccessPassword(password);

            if (result.Success)
                {
                try
                    {
                    sessionHandler.GrantSbpAccess();
                    }
                catch (Exception ex)
                    {
                    _logger.LogError(ex, "Unable to persist SBP authentication state.");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Session could not be updated." });
                    }
                }

            return Ok(result);
            }

        [HttpPost]
        public IActionResult UpdateSbpObservationPassword([FromBody] SbpPasswordUpdateRequest request)
            {
            if (request == null)
                {
                return BadRequest(new { success = false, message = "Request payload is required." });
                }

            var newPassword = request.NewPassword?.Trim();
            if (string.IsNullOrWhiteSpace(newPassword))
                {
                return BadRequest(new { success = false, message = "New password is required." });
                }

            var validation = _passwordPolicyValidator.Validate(newPassword, sessionHandler.TryGetUser(out var user) ? user?.PPNumber : null);
            if (!validation.IsValid)
                {
                return BadRequest(new { success = false, message = validation.ErrorMessage });
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
                    return StatusCode(StatusCodes.Status400BadRequest, new { success = false, message = errorMessage });
                    }

                var successMessage = string.IsNullOrWhiteSpace(result.Message)
                    ? "Password updated successfully."
                    : result.Message;

                return Ok(new { success = true, message = successMessage });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error updating SBP observation password.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Password update failed." });
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
        public IActionResult get_department_performance_summary_and_detail([FromBody] DepartmentPerformanceRequest request)
            {
            return GetDepartmentPerformanceSummaryAndDetail(request?.EntId ?? 0, request?.StartDate, request?.EndDate);
            }

        [HttpGet]
        public IActionResult get_department_performance_summary_and_detail(int ent_id, string start_date, string end_date)
            {
            return GetDepartmentPerformanceSummaryAndDetail(ent_id, start_date, end_date);
            }

        [HttpPost]
        public IActionResult get_department_performance_by_zone([FromBody] DepartmentPerformanceByZoneRequest request)
            {
            return GetDepartmentPerformanceByZone(request?.EntId ?? 0, request?.ZoneId ?? 0, request?.StartDate, request?.EndDate);
            }

        [HttpGet]
        public IActionResult get_department_performance_by_zone(int ent_id, int zone_id, string start_date, string end_date)
            {
            return GetDepartmentPerformanceByZone(ent_id, zone_id, start_date, end_date);
            }

        [HttpPost]
        public IActionResult get_auditor_performance([FromBody] AuditorPerformanceRequest request)
            {
            return GetAuditorPerformance(request?.EntId ?? 0, request?.ZoneId, request?.StartDate, request?.EndDate);
            }

        [HttpGet]
        public IActionResult get_auditor_performance(int ent_id, int? zone_id, string start_date, string end_date)
            {
            return GetAuditorPerformance(ent_id, zone_id, start_date, end_date);
            }

        private IActionResult GetDepartmentPerformanceSummaryAndDetail(int entId, string startDateValue, string endDateValue)
            {
            if (entId <= 0)
                {
                return BadRequest("Invalid request payload.");
                }

            if (!TryParseIsoDate(startDateValue, out var startDate))
                {
                return BadRequest("Invalid start_date.");
                }

            if (!TryParseIsoDate(endDateValue, out var endDate))
                {
                return BadRequest("Invalid end_date.");
                }

            if (startDate > endDate)
                {
                return BadRequest("start_date cannot be later than end_date.");
                }

            try
                {
                var response = dBConnection.GetDepartmentPerformanceSummaryAndDetail(entId, startDate, endDate);
                return Ok(response ?? new DepartmentPerformanceSummaryDetailResponse());
                }
            catch (OracleException ex)
                {
                _logger.LogError(ex, "Error retrieving department performance summary and detail.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch department performance data.");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unexpected error retrieving department performance summary and detail.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch department performance data.");
                }
            }

        private IActionResult GetDepartmentPerformanceByZone(int entId, int zoneId, string startDateValue, string endDateValue)
            {
            if (entId <= 0)
                {
                return BadRequest("Invalid request payload.");
                }

            if (!TryParseIsoDate(startDateValue, out var startDate))
                {
                return BadRequest("Invalid start_date.");
                }

            if (!TryParseIsoDate(endDateValue, out var endDate))
                {
                return BadRequest("Invalid end_date.");
                }

            if (startDate > endDate)
                {
                return BadRequest("start_date cannot be later than end_date.");
                }

            try
                {
                var rows = dBConnection.GetDepartmentPerformanceByZone(entId, zoneId, startDate, endDate);
                return Ok(rows ?? new List<DeptPerfByZoneRow>());
                }
            catch (OracleException ex)
                {
                _logger.LogError(ex, "Error retrieving department performance by zone.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch department performance data.");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unexpected error retrieving department performance by zone.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch department performance data.");
                }
            }

        private IActionResult GetAuditorPerformance(int entId, int? zoneId, string startDateValue, string endDateValue)
            {
            if (entId <= 0)
                {
                return BadRequest("Invalid request payload.");
                }

            if (!TryParseIsoDate(startDateValue, out var startDate))
                {
                return BadRequest("Invalid start_date.");
                }

            if (!TryParseIsoDate(endDateValue, out var endDate))
                {
                return BadRequest("Invalid end_date.");
                }

            if (startDate > endDate)
                {
                return BadRequest("start_date cannot be later than end_date.");
                }

            try
                {
                var rows = dBConnection.GetAuditorPerformance(entId, zoneId, startDate, endDate);
                return Ok(rows ?? new List<AuditorPerformanceRow>());
                }
            catch (OracleException ex)
                {
                _logger.LogError(ex, "Error retrieving auditor performance.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch auditor performance data.");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unexpected error retrieving auditor performance.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch auditor performance data.");
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
        public IActionResult save_observations([FromBody] SaveObservationRequest request)
            {
            if (!ModelState.IsValid)
                {
                return InvalidModelStateResponse();
                }

            var isFinalSubmission = request?.IS_FINAL == true;
            var loggedInUser = sessionHandler.GetUserOrThrow();
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
                    HEADING = m.HEADING,
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
                    STATUS = 1
                    };

                responses += dBConnection.SaveAuditObservation(ob);
                }

            return Ok(new
                {
                Status = true,
                Message = responses
                });
            }




        [HttpPost]
        [Consumes("application/json")]
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

                var ob = new ObservationModel
                    {
                    SUBCHECKLIST_ID = request.SUB_CHECKLISTID.GetValueOrDefault(),
                    CHECKLISTDETAIL_ID = checklistDetailId,
                    ANNEXURE_ID = request.ANNEXURE_ID,
                    ENGPLANID = request.ENG_ID.GetValueOrDefault(),
                    REPLYDATE = DateTime.Today.AddDays(m.DAYS.GetValueOrDefault()),
                    OBSERVATION_TEXT = m.MEMO,
                    HEADING = m.HEADING,
                    SEVERITY = m.RISK.GetValueOrDefault(),
                    BRANCH_ID = request.BRANCH_ID.GetValueOrDefault(),
                    AMOUNT_INVOLVED = m.AMOUNT_INVOLVED,
                    NO_OF_INSTANCES = m.NO_OF_INSTANCES,
                    RESPONSIBLE_PPNO = m.RESPONSIBLE_PPNO,
                    STATUS = 1
                    };

                responses += dBConnection.SaveAuditObservationCAU(ob);
                }

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
        public string update_observation_text(int OBS_ID, string OBS_TEXT, int PROCESS_ID = 0, int SUBPROCESS_ID = 0, int CHECKLIST_ID = 0, string OBS_TITLE = "", int RISK_ID = 0, int ANNEXURE_ID = 0)
            {
            string response = "";
            response = dBConnection.UpdateAuditObservationText(OBS_ID, OBS_TEXT, PROCESS_ID, SUBPROCESS_ID, CHECKLIST_ID, OBS_TITLE, RISK_ID, ANNEXURE_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        public string update_observation_status(int OBS_ID, int NEW_STATUS_ID, string DRAFT_PARA_NO, int RISK_ID, string AUDITOR_COMMENT)
            {
            string response = "";

            if (NEW_STATUS_ID == 4)
                if (RISK_ID != 3)
                    return "{\"Status\":false,\"Message\":\"Only Low Risk para can be settled by Team Lead\"}";

            response = dBConnection.UpdateAuditObservationStatus(OBS_ID, NEW_STATUS_ID, DRAFT_PARA_NO, AUDITOR_COMMENT);

            return "{\"Status\":true,\"Message\":\"" + response + "\"}";

            }

        [HttpPost]
        public string drop_observation(int OBS_ID)
            {
            string response = "";
            response = dBConnection.DropAuditObservation(OBS_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";

            }
        [HttpPost]
        public string submit_observation_to_auditee(int OBS_ID)
            {
            string response = "";
            response = dBConnection.SubmitAuditObservationToAuditee(OBS_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";

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
        public string referredback_para_for_manage_audit_paras(ManageAuditParasModel pm)
            {
            string response = "";
            response = dBConnection.ReferredBackAuditObservationStatus(pm);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
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
        [HttpPost]
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
        public List<FadOldParaReportModel> get_fad_paras(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, int PROCESS_DETAIL_ID = 0)
            {
            return dBConnection.GetFadBranchesParas(PROCESS_ID, SUB_PROCESS_ID, PROCESS_DETAIL_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<ClosingDraftTeamDetailsModel> get_team_details(int ENG_ID = 0)
            {
            return dBConnection.GetClosingDraftTeamDetails(ENG_ID);
            }
        [HttpPost]
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
            return dBConnection.GetDepositAccountSubdetails(b_name);
            }
        [HttpPost]
        public List<DepositAccountCatDetailsModel> GetDepositAccountcatdetails(int catid)
            {
            return dBConnection.GetDepositAccountcatdetails(catid);
            }


        [HttpPost]
        public List<LoanCaseModel> GetBranchDesbursementaccountdetails(int b_id)
            {
            return dBConnection.GetBranchDesbursementAccountdetails(b_id);
            }

        [HttpPost]
        public List<GlHeadDetailsModel> GetIncomeExpenceDetails(int b_id, int ENG_ID)
            {
            return dBConnection.GetIncomeExpenceDetails(b_id, ENG_ID);
            }
        [HttpPost]
        public int GetAuditEntitiesCount(int CRITERIA_ID)
            {
            return dBConnection.GetExpectedCountOfAuditEntitiesOnCriteria(CRITERIA_ID);
            }
        [HttpPost]
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
        public string UpdateAuditeeEntity(AuditeeEntityUpdateModel ENTITY_MODEL, string IND)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditeeEntity(ENTITY_MODEL, IND) + "\"}";
            }
        [HttpPost]
        public List<AuditeeEntitiesModel> GetAISEntities(string ENTITY_ID, string TYPE_ID)
            {
            return dBConnection.GetAISEntities(ENTITY_ID, TYPE_ID);
            }
        [HttpPost]
        public List<AuditeeEntitiesModel> GetCBASEntities(string E_CODE, string E_NAME)
            {
            return dBConnection.GetCBASEntities(E_CODE, E_NAME);
            }
        [HttpPost]
        public List<AuditeeEntitiesModel> GetERPEntities(string E_CODE, string E_NAME)
            {
            return dBConnection.GetERPEntities(E_CODE, E_NAME);
            }
        [HttpPost]
        public List<AuditeeEntitiesModel> GetHREntities(string E_CODE, string E_NAME)
            {
            return dBConnection.GetHREntities(E_CODE, E_NAME);
            }
        [HttpPost]
        public bool submit_audit_criterias(int PERIOD_ID)
            {
            return dBConnection.SubmitAuditCriteriaForApproval(PERIOD_ID);
            }
        [HttpPost]
        public List<COSORiskModel> GetCOSORiskForDepartment(int PERIOD_ID = 0)
            {
            return dBConnection.GetCOSORiskForDepartment(PERIOD_ID);
            }
        [HttpPost]
        public CAUOMAssignmentResponseModel CAU_OM_assignment(CAUOMAssignmentModel caumodel)
            {
            return dBConnection.CAUOMAssignment(caumodel);
            }
        [HttpPost]
        public CAUOMAssignmentResponseModel CAU_OM_assignmentAIR(CAUOMAssignmentAIRModel caumodel)
            {
            return dBConnection.CAUOMAssignmentAIR(caumodel);
            }
        [HttpPost]
        public CAUOMAssignmentResponseModel CAU_OM_assignmentPDP(List<CAUOMAssignmentPDPModel> DAC_LIST)
            {
            CAUOMAssignmentResponseModel resp = new CAUOMAssignmentResponseModel();
            foreach (CAUOMAssignmentPDPModel pdp in DAC_LIST)
                {
                resp = dBConnection.CAUOMAssignmentPDP(pdp);
                }

            return resp;

            }
        [HttpPost]
        public CAUOMAssignmentResponseModel CAU_OM_assignmentARPSE(List<CAUOMAssignmentARPSEModel> PAC_LIST)
            {
            CAUOMAssignmentResponseModel resp = new CAUOMAssignmentResponseModel();
            foreach (CAUOMAssignmentARPSEModel pdp in PAC_LIST)
                {
                resp = dBConnection.CAUOMAssignmentARPSE(pdp);
                }
            return resp;
            }

        [HttpPost]
        public CAUOMAssignmentModel CAU_get_Pre_Added_OM(string OM_NO, string INS_YEAR)
            {
            return dBConnection.CAUGetPreAddedOM(OM_NO, INS_YEAR);

            }

        [HttpPost]
        public List<CAUOMAssignmentModel> CAU_Get_OMs()
            {
            return dBConnection.CAUGetAssignedOMs();
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
            return dBConnection.GetOldParasManagement();
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
            return dBConnection.GetParasForStatusChange(ENTITY_ID);
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
            return dBConnection.GetCurrentParasForStatusChangeRequestAuthorize();
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
        public bool add_legacy_para_observation_text(OldParasModel ob)
            {
            return dBConnection.AddOldParas(ob);
            }
        [HttpPost]
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
        public string add_legacy_para_cad_reply(int ID, int V_CAT_ID, int V_CAT_NATURE_ID, int RISK_ID, string REPLY)
            {
            string response = "";
            response = dBConnection.AddOldParasCADReply(ID, V_CAT_ID, V_CAT_NATURE_ID, RISK_ID, REPLY);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";

            }
        [HttpPost]
        public string add_legacy_para_cad_compliance(List<OldParaComplianceModel> COMPLIANCE_LIST)
            {
            string response = "";
            foreach (OldParaComplianceModel opc in COMPLIANCE_LIST)
                {
                response += dBConnection.AddOldParasCADCompliance(opc) + "\n";
                }

            return "{\"Status\":true,\"Message\":\"" + response + "\"}";

            }
        [HttpGet]
        [HttpPost]
        public ActiveInactiveChart get_pie_chart_data()
            {
            return dBConnection.GetActiveInactiveChartData();
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
        public List<AuditPlanCompletionReportModel> get_auditplan_completion(int DEPT_ID)
            {
            return dBConnection.GetauditplanCompletion(DEPT_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<CurrentAuditProgress> get_current_audit_progress(int ENTITY_ID)
            {
            return dBConnection.GetCurrentAuditProgress(ENTITY_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<CurrentActiveUsers> get_active_users()
            {
            return dBConnection.GetCurrentActiveUsers();

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
            return dBConnection.GetAuditeeOldParas(ENTITY_ID);

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


        [HttpPost]
        public async Task<string> submit_post_audit_compliance(string OLD_PARA_ID, int NEW_PARA_ID, string INDICATOR, string COMPLIANCE, string COMMENTS, List<AuditeeResponseEvidenceModel> EVIDENCE_LIST, string SUBFOLDER)
            {
            string response = await dBConnection.SubmitPostAuditCompliance(OLD_PARA_ID, NEW_PARA_ID, INDICATOR, COMPLIANCE, COMMENTS, EVIDENCE_LIST, SUBFOLDER);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        public string submit_post_audit_compliance_review(string OLD_PARA_ID, int NEW_PARA_ID, string INDICATOR, string COMPLIANCE, string COMMENTS, List<AuditeeResponseEvidenceModel> EVIDENCE_LIST)
            {
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
        public string submit_old_para_br_compliance_status(string OBS_ID, string REFID, string REMARKS, int NEW_STATUS, string PARA_CAT, string SETTLE_INDICATOR, string SEQUENCE, string AUDITED_BY)
            {
            string response = "";
            response = dBConnection.AddOldParasStatusUpdate(OBS_ID, REFID, REMARKS, NEW_STATUS, PARA_CAT, SETTLE_INDICATOR, SEQUENCE, AUDITED_BY);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
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
        [HttpGet]
        [HttpPost]
        public string get_user_name(string PPNUMBER)
            {
            string response = "";
            response = dBConnection.GetUserName(PPNUMBER);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        public string Add_Old_Para_Change_status(string REFID, string OBS_ID, string INDICATOR, int NEW_STATUS, string REMARKS)
            {
            string response = "";
            response = dBConnection.AddChangeStatusRequestForSettledPara(REFID, OBS_ID, INDICATOR, NEW_STATUS, REMARKS);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        public string Add_Old_Para_Change_status_Review(string REFID, string IND, string REMARKS, string Action_IND)
            {
            string response = "";
            response = dBConnection.ReviewerAddChangeStatusRequestForSettledPara(REFID, IND, REMARKS, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        public string Add_Old_Para_Change_status_Authorize(string REFID, string IND, int NEW_STATUS, string REMARKS, string Action_IND)
            {
            string response = "";
            response = dBConnection.AuthorizerAddChangeStatusRequestForSettledPara(REFID, IND, NEW_STATUS, REMARKS, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        public string Add_New_Para_Change_status_Request(string REFID, int NEW_STATUS, string REMARKS)
            {
            string response = "";
            response = dBConnection.AddChangeStatusRequestForCurrentPara(REFID, NEW_STATUS, REMARKS);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }


        [HttpPost]

        public string Add_Para_Change_status_Request(string COM_ID, int NEW_STATUS, string REMARKS, string IND, string Action_IND)
            {
            string response = "";
            response = dBConnection.AddChangeStatusRequestForPara(COM_ID, NEW_STATUS, REMARKS, IND, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }


        [HttpGet]
        [HttpPost]
        public List<ZoneBranchParaStatusModel> get_zone_brach_para_position(int ENTITY_ID)
            {
            return dBConnection.GetZoneBranchParaPositionStatus(ENTITY_ID);
            }
        [HttpPost]
        public string Add_Authorization_Old_Para_Change_status(string REFID, string OBS_ID, string IND, string Action_IND)
            {
            string response = "";
            response = dBConnection.AddAuthorizeChangeStatusRequestForSettledPara(REFID, OBS_ID, IND, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<OldParasAuthorizeModel> get_legacy_settled_paras_autorize()
            {
            return dBConnection.GetOldSettledParasForResponseAuthorize();
            }

        [HttpGet]
        [HttpPost]
        public List<GetOldParasBranchComplianceModel> get_old_para_br_compliance_text_update()
            {
            return dBConnection.GetOldParasBranchComplianceTextupdate();
            }

        [HttpGet]
        [HttpPost]
        public List<ParaStatusChangeModel> get_paras_for_status_change_authorize()
            {
            return dBConnection.GetParasForStatusChangeToAuthorize();
            }

        [HttpPost]
        public string authorize_para_change_status(string COM_ID, int NEW_PARA_ID, int OLD_PARA_ID, string REMARKS, string IND, string Action_IND)
            {
            string response = "";
            response = dBConnection.AuthorizeChangeStatusRequestForPara(COM_ID, NEW_PARA_ID, OLD_PARA_ID, REMARKS, IND, Action_IND);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
        public List<GetTeamDetailsModel> GetTeamDetails(int ENG_ID)
            {
            return dBConnection.GetTeamDetails(ENG_ID);
            }




        [HttpGet]
        [HttpPost]
        public List<GetAuditeeParasModel> get_report_status(int ENG_ID)
            {
            return dBConnection.GetAuditeReportStatus(ENG_ID);
            }
        [HttpPost]
        public string submit_pre_concluding(int ENG_ID)
            {
            string response = "";
            response = dBConnection.SubmitPreConcluding(ENG_ID);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }
        [HttpPost]
        public string update_audit_para_for_finalization(int OBS_ID, string ANNEX_ID, string PROCESS_ID, int SUB_PROCESS_ID, int PROCESS_DETAIL_ID, int RISK_ID, int FINAL_PARA_NO, string GIST_OF_PARA, string TEXT_PARA, string AMOUNT_INV, string NO_INST)
            {
            string response = "";
            response = dBConnection.UpdateAuditParaForFinalization(OBS_ID, ANNEX_ID, PROCESS_ID, SUB_PROCESS_ID, PROCESS_DETAIL_ID, RISK_ID, FINAL_PARA_NO, GIST_OF_PARA, TEXT_PARA, AMOUNT_INV, NO_INST);
            return "{\"Status\":true,\"Message\":\"" + response + "\"}";
            }

        [HttpPost]
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
        public string update_legacy_para_with_responsibilities(AddLegacyParaModel LEGACY_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateLegacyParasWithResponsibility(LEGACY_PARA) + "\"}";

            }

        [HttpPost]
        public string update_legacy_para_gist_paraNo(string PARA_REF, string PARA_NO, string GIST_OF_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateLegacyParaGistParaNo(PARA_REF, PARA_NO, GIST_OF_PARA) + "\"}";

            }
        //
        [HttpPost]
        public string delete_responsibility_of_legacy_para(string REF_P, int P_ID, int PP_NO)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.DeleteResponsibilityOfLegacyParas(REF_P, P_ID, PP_NO) + "\"}";

            }

        [HttpPost]
        public string add_responsibility_to_legacy_para(ObservationResponsiblePPNOModel RESP_PP, string REF_P, int P_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddResponsibilityToLegacyParas(RESP_PP, REF_P, P_ID) + "\"}";

            }

        [HttpPost]
        public string add_responsibility_to_legacy_para_fad(ObservationResponsiblePPNOModel RESP_PP, string REF_P, int P_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddResponsibilityToLegacyParasFAD(RESP_PP, REF_P, P_ID) + "\"}";

            }


        [HttpPost]
        public string update_legacy_para_with_responsibilities_no_changes_AZ(AddLegacyParaModel LEGACY_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateLegacyParasWithResponsibilityNoChangesAZ(LEGACY_PARA) + "\"}";

            }

        [HttpPost]
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
        public string update_legacy_para_with_responsibilities_FAD(AddLegacyParaModel LEGACY_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateLegacyParasWithResponsibilityFAD(LEGACY_PARA) + "\"}";

            }

        [HttpPost]
        public List<AuditPlanReportModel> GetFADAuditPlan(int ENT_ID, int Z_ID, int RISK, int SIZE)
            {
            return dBConnection.GetFadAuditPlanReport(ENT_ID, Z_ID, RISK, SIZE);


            }
        [HttpGet]
        [HttpPost]
        public List<FADNewOldParaPerformanceModel> get_fad_new_old_para_performance(int AUDIT_ZONE_ID)
            {
            return dBConnection.GetFADNewOldParaPerformance(AUDIT_ZONE_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<LegacyZoneWiseOldParasPerformanceModel> get_legacy_zone_wise_performance(DateTime? FILTER_DATE)
            {
            return dBConnection.GetLegacyZoneWiseOldParasPerformance(FILTER_DATE);
            }
        [HttpGet]
        [HttpPost]
        public List<LegacyUserWiseOldParasPerformanceModel> get_legacy_user_wise_performance(DateTime? FILTER_DATE)
            {
            return dBConnection.GetLegacyUserWiseOldParasPerformance(FILTER_DATE);
            }
        [HttpGet]
        [HttpPost]
        public List<FADHOUserLegacyParaUserWiseParasPerformanceModel> get_fad_ho_user_legacy_para_user_wise_performance(DateTime? FILTER_DATE)
            {
            return dBConnection.GetFADHOUserLegacyParaUserWiseOldParasPerformance(FILTER_DATE);
            }

        [HttpPost]
        public string delete_legacy_para_responsibility(string PARA_REF, int PARA_ID, int PP_NO)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.DeleteLegacyParaResponsibility(PARA_REF, PARA_ID, PP_NO) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<AuditEntitiesModel> get_auditee_entities_by_entity_type_id(int ENTITY_TYPE_ID)
            {
            return dBConnection.GetAuditEntitiesByTypeId(ENTITY_TYPE_ID);
            }

        [HttpPost]
        public string add_new_legacy_para(AddNewLegacyParaModel LEGACY_PARA)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddNewLegacyPara(LEGACY_PARA) + "\"}";
            }

        [HttpPost]
        public string refer_back_legacy_para_to_az(string PARA_REF, int PARA_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.ReferBackLegacyPara(PARA_REF, PARA_ID) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<AddNewLegacyParaModel> get_add_legacy_paras_autorize()
            {
            return dBConnection.GetAddedLegacyParaForAuthorize();
            }

        //
        [HttpGet]
        [HttpPost]
        public List<AddNewLegacyParaModel> get_update_gist_paraNo_legacy_paras_autorize()
            {
            return dBConnection.GetUpdatedGistParaOfLegacyParaForAuthorize();
            }

        [HttpPost]
        public string Authorize_Legacy_Para_addition(string PARA_REF)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeLegacyParaAddition(PARA_REF) + "\"}";
            }
        [HttpPost]
        public string Authorize_Legacy_Para_Gist_ParaNo(string PARA_REF, string GIST_OF_PARA, string PARA_NO)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeLegacyParaGistParaNoUpdate(PARA_REF, GIST_OF_PARA, PARA_NO) + "\"}";
            }

        [HttpPost]
        public string Delete_Legacy_Para_addition_request(string PARA_REF)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.DeleteLegacyParaAdditionRequest(PARA_REF) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<AuditeeOldParasModel> get_legacy_report_dropdown_contents(int ENTITY_ID)
            {
            return dBConnection.GetLegacyParasEntitiesReport(ENTITY_ID);
            }

        [HttpPost]
        public string settle_legacy_para_HO(int NEW_STATUS, string PARA_REF, string SETTLEMENT_NOTES)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SettleLegacyParaHO(NEW_STATUS, PARA_REF, SETTLEMENT_NOTES) + "\"}";
            }
        [HttpPost]
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
            return dBConnection.GetFunctionalResponsibilityWiseParaForDashboard(FUNCTIONAL_ENTITY_ID);
            }
        [HttpGet]
        [HttpPost]
        public List<FADNewOldParaPerformanceModel> get_functional_responsibility_wise_paras_for_dashboard_ho(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, int PROCESS_DETAIL_ID = 0, int FUNCTIONAL_ENTITY_ID = 0, int DEPT_ID = 0)
            {
            return dBConnection.GetHOFunctionalResponsibilityWiseParaForDashboard(PROCESS_ID, SUB_PROCESS_ID, PROCESS_DETAIL_ID, FUNCTIONAL_ENTITY_ID, DEPT_ID);
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
            return dBConnection.GetEntitiesRiskBasePlanForDashboard();
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
        public string add_audit_sub_checklist(int PROCESS_ID = 0, int ENTITY_TYPE_ID = 0, string HEADING = "", string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAuditSubChecklist(PROCESS_ID, ENTITY_TYPE_ID, HEADING, RISK_SEQUENCE, RISK_WEIGHTAGE) + "\"}";
            }
        [HttpPost]
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
        public string add_audit_checklist_detail(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int V_ID = 0, int CONTROL_ID = 0, int ROLE_ID = 0, int RISK_ID = 0, string ANNEX_CODE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAuditChecklistDetail(PROCESS_ID, SUB_PROCESS_ID, HEADING, V_ID, CONTROL_ID, ROLE_ID, RISK_ID, ANNEX_CODE) + "\"}";
            }
        [HttpPost]
        public string update_audit_checklist_detail(int PROCESS_DETAIL_ID = 0, int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int V_ID = 0, int CONTROL_ID = 0, int ROLE_ID = 0, int RISK_ID = 0, string ANNEX_CODE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAuditChecklistDetail(PROCESS_DETAIL_ID, PROCESS_ID, SUB_PROCESS_ID, HEADING, V_ID, CONTROL_ID, ROLE_ID, RISK_ID, ANNEX_CODE) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<ParaPositionReportModel> get_para_position_report(int P_ID = 0, int C_ID = 0)
            {
            return dBConnection.GetParaPositionReport(P_ID, C_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<RepetativeParaModel> get_repetative_paras_for_dashboard(int P_ID = 0, int SP_ID = 0, int PD_ID = 0)
            {
            return dBConnection.GetRepetativeParaForDashboard(P_ID, SP_ID, PD_ID);
            }

        [HttpPost]
        public string add_audit_checklist(string HEADING = "", int ENTITY_TYPE_ID = 0, string RISK_SEQUENCE = "", string RISK_WEIGHTAGE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAuditChecklist(HEADING, ENTITY_TYPE_ID, RISK_SEQUENCE, RISK_WEIGHTAGE) + "\"}";
            }

        [HttpPost]
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
        public List<ParaPositionDetailsModel> get_para_position_details(int ENTITY_ID = 0, int AUDIT_PERIOD = 0)
            {
            return dBConnection.GetParaPositionParaDetails(ENTITY_ID, AUDIT_PERIOD);
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
        public List<ComplianceHistoryModel> get_settled_para_compliance_history(string REF_P, string OBS_ID)
            {
            return dBConnection.GetSettledParaComplianceHistory(REF_P, OBS_ID);

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
            return dBConnection.GetHOViolationListForDashboard(FUNCTIONAL_ENTITY_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<RiskProcessDefinition> get_sub_violation_area_for_functional_responsibility_wise_paras_ho(int FUNCTIONAL_ENTITY_ID = 0, int PROCESS_ID = 0)
            {
            return dBConnection.GetHOSubViolationListForDashboard(FUNCTIONAL_ENTITY_ID, PROCESS_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<FADNewOldParaPerformanceModel> get_total_para_details_ho(int ENTITY_ID = 0)
            {
            return dBConnection.GetTotalParasDetailsHO(ENTITY_ID);

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
        public string add_new_user(FindUserModel user)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddNewUser(user) + "\"}";

            }

        [HttpGet]
        [HttpPost]
        public List<RoleActivityLogModel> get_role_activity_log(int ROLE_ID, int DEPT_ID, int AZ_ID)
            {
            return dBConnection.GetRoleActivityLog(ROLE_ID, DEPT_ID, AZ_ID);

            }
        [HttpGet]
        [HttpPost]
        public List<RoleActivityLogModel> get_user_activity_log(int PP_NO)
            {
            return dBConnection.GetUserActivityLog(PP_NO);

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
        #endregion
        [HttpGet]
        [HttpPost]
        public List<EntityWiseObservationModel> get_reporting_wise_observations()
            {
            return dBConnection.GetReportingOfficeWiseObservations();

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
            return dBConnection.GetFunctionalObservations(ANNEX_ID, ENTITY_ID);

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
        public string authorize_merge_duplicate_checklists(int PROCESS_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeMergeDuplicateChecklists(PROCESS_ID) + "\"}";
            }

        [HttpPost]
        public string update_observation_status_for_reversal(List<int> OBS_IDS, int NEW_STATUS_ID, int ENG_ID)
            {
            string resp = "";
            foreach (int ID in OBS_IDS)
                {
                resp += dBConnection.UpdateObservationStatusForReversal(ID, NEW_STATUS_ID, ENG_ID) + "<br />";
                }
            return "{\"Status\":true,\"Message\":\"" + resp + "\"}";
            }

        [HttpPost]

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
        public string submit_settled_para_compliance_comments(string REF_P, string OBS_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SaveSettledParaCompliacne(REF_P, OBS_ID, COMMENTS) + "\"}";

            }

        [HttpGet]
        [HttpPost]
        public List<StatusWiseComplianceModel> get_status_wise_compliance(string AUDITEE_ID, string START_DATE, string END_DATE, string RELATION_CHECK)
            {
            return dBConnection.GetStatusWiseCompliance(AUDITEE_ID, START_DATE, END_DATE, RELATION_CHECK);
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
        public string update_ais_entity_for_admin_panel_entity_addition(string ENTITY_ID, string ENTITY_NAME, string ENTITY_CODE, string AUDITABLE, string AUDIT_BY_ID, string ENTITY_TYPE_ID, string ENT_DESC, string STATUS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAISEntityForAdminPanelEntityAddition(ENTITY_ID, ENTITY_NAME, ENTITY_CODE, AUDITABLE, AUDIT_BY_ID, ENTITY_TYPE_ID, ENT_DESC, STATUS) + "\"}";

            }

        [HttpPost]
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
        public string add_ais_entity_mapping_for_admin_panel_entity_addition(string P_ENTITY_ID, string ENTITY_ID, string RELATION_TYPE_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddAISEntityMappingForAdminPanelEntityAddition(P_ENTITY_ID, ENTITY_ID, RELATION_TYPE_ID) + "\"}";

            }


        [HttpPost]
        public string update_ais_entity_mapping_for_admin_panel_entity_addition(string P_ENTITY_ID, string ENTITY_ID, string RELATION_TYPE_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateAISEntityMappingForAdminPanelEntityAddition(P_ENTITY_ID, ENTITY_ID, RELATION_TYPE_ID) + "\"}";

            }



        [HttpGet]
        [HttpPost]
        public List<AuditPlanEngDetailReport> get_audit_plan_engagement_detailed_report(string AUDITED_BY, string PERIOD_ID)
            {
            return dBConnection.GetAuditPlanEngagementDetailedReport(AUDITED_BY, PERIOD_ID);

            }

        [HttpGet]
        [HttpPost]
        public List<LoanCaseFileDetailsModel> Get_Working_Paper_Loan_Cases(string ENGID)
            {
            return dBConnection.GetWorkingPaperLoanCases(ENGID);

            }

        [HttpPost]
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
        public string Add_Working_Paper_Cash_Counter(string ENGID, string DVAULT, string NOVAULT, string TOTVAULT, string DSR, string NOSR, string TOTSR, string DIFF)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddWorkingCashCounter(ENGID, DVAULT, NOVAULT, TOTVAULT, DSR, NOSR, TOTSR, DIFF) + "\"}";

            }
        [HttpGet]
        [HttpPost]
        public List<AnnexureExerciseStatus> Get_Annexure_Exercise_Status()
            {
            return dBConnection.GetAnnexureExerciseStatus();

            }

        [HttpPost]
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
        public string submit_entity_shifting_from_admin_panel(string FROM_ENT_ID, string TO_ENT_ID, string CIR_REF, DateTime CIR_DATE, string CIR)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitEntityShiftingFromAdminPanel(FROM_ENT_ID, TO_ENT_ID, CIR_REF, CIR_DATE, CIR) + "\"}";
            }
        [HttpPost]
        public string submit_entity_conv_to_islamic_from_admin_panel(string FROM_ENT_ID, string TO_ENT_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitEntityConvToIslamicFromAdminPanel(FROM_ENT_ID, TO_ENT_ID) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<GroupWiseUsersCountModel> get_group_wise_users_count()
            {
            return dBConnection.GetGroupWiseUsersCount();
            }
        [HttpGet]
        [HttpPost]
        public List<GroupWisePagesModel> get_group_wise_pages(string GROUP_ID)
            {
            return dBConnection.GetGroupWisePages(GROUP_ID);
            }

        [HttpPost]
        public string add_compliance_flow(string ID, string ENTITY_TYPE_ID, string GROUP_ID, string PREV_GROUP_ID, string NEXT_GROUP_ID, string COMP_UP_STATUS, string COMP_DOWN_STATUS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddComplianceFlow(ID, ENTITY_TYPE_ID, GROUP_ID, PREV_GROUP_ID, NEXT_GROUP_ID, COMP_UP_STATUS, COMP_DOWN_STATUS) + "\"}";
            }

        [HttpPost]
        public string update_compliance_flow(string ID, string ENTITY_TYPE_ID, string GROUP_ID, string PREV_GROUP_ID, string NEXT_GROUP_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateComplianceFlow(ID, ENTITY_TYPE_ID, GROUP_ID, PREV_GROUP_ID, NEXT_GROUP_ID) + "\"}";
            }
        [HttpGet]
        [HttpPost]
        public List<ComplianceFlowModel> get_compliance_flow_by_entity_type(int ENTITY_TYPE_ID = 0, int GROUP_ID = 0)
            {
            return dBConnection.GetComplianceFlowByEntityType(ENTITY_TYPE_ID, GROUP_ID);
            }


        [HttpGet]
        [HttpPost]
        public List<DepttWiseOutstandingParasModel> get_outstanding_paras_for_entity_type_id(string ENTITY_TYPE_ID, string P_REF_DATE, int P_USE_TRUNC = 0)
            {
            DateTime refDateValue = DateTime.Today;

            if (!string.IsNullOrWhiteSpace(P_REF_DATE))
                {
                if (!DateTime.TryParseExact(P_REF_DATE, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out refDateValue))
                    {
                    refDateValue = DateTime.Today;
                    }
                }

            return dBConnection.GetOutstandingParasForEntityTypeId(ENTITY_TYPE_ID, refDateValue, P_USE_TRUNC);
            }

        [HttpGet]
        [HttpPost]
        public List<AuditTeamModel> get_team_memeber_details_for_post_changes_team_eng_reversal(int AUDITED_BY_DEPT)
            {
            return dBConnection.GetAuditTeams(0, AUDITED_BY_DEPT);
            }

        [HttpPost]
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
        public string update_observation_numbers_for_status_reversal(ObservationNumbersModel onum)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateObservationNumbersForStatusReversal(onum) + "\"}";
            }
        [HttpPost]
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
        public string add_hr_designation_wise_role_assignment(int ASSIGNMENT_ID, int DESIGNATION_ID, int GROUP_ID, string SUB_ENTITY_NAME)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddHRDesignationWiseRoleAssignment(ASSIGNMENT_ID, DESIGNATION_ID, GROUP_ID, SUB_ENTITY_NAME) + "\"}";
            }

        [HttpPost]
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
        public string add_manage_observatiton_status(ManageObservationModel OBS_STATUS_MODEL)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddManageObservationStatus(OBS_STATUS_MODEL) + "\"}";
            }
        [HttpPost]
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
        public string add_manage_entities_audit_department(ManageEntAuditDeptModel ENT_AUD_DEPT_MODEL)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddManageEntityAuditDepartment(ENT_AUD_DEPT_MODEL) + "\"}";
            }
        [HttpPost]
        public string update_manage_entities_audit_department(ManageEntAuditDeptModel ENT_AUD_DEPT_MODEL)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateEntityAuditDepartment(ENT_AUD_DEPT_MODEL) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<LoanDetailReportModel> get_loan_detail_report(int ENT_ID, int GLSUBID, int STATUSID, DateTime START_DATE, DateTime END_DATE)
            {
            return dBConnection.GetLoanDetailsReport(ENT_ID, GLSUBID, STATUSID, START_DATE, END_DATE);
            }

        [HttpGet]
        [HttpPost]
        public List<LoanDetailReportModel> get_cnic_loan_detail_report(string CNIC)
            {
            return dBConnection.GetCNICLoanDetailsReport(CNIC);
            }
        [HttpGet]
        [HttpPost]
        public List<DefaultHisotryLoanDetailReportModel> get_default_cnic_loan_detail_report(string CNIC, string LOAN_DISB_ID)
            {
            return dBConnection.GetDefaultCNICLoanDetailsReport(CNIC, LOAN_DISB_ID);
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
        public string add_sub_menu_for_admin_panel(SubMenuModel sm)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddSubMenuForAdminPanel(sm) + "\"}";
            }
        [HttpPost]
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
        public string add_menu_page_for_admin_panel(MenuPagesAssignmentModel mPage)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddMenuPageForAdminPanel(mPage) + "\"}";
            }
        [HttpPost]
        public string update_menu_page_for_admin_panel(MenuPagesAssignmentModel mPage)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateMenuPageForAdminPanel(mPage) + "\"}";
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
        public List<ComplianceProgressReportDetailModel> get_compliance_progress_report_details(string ROLE_TYPE, string PP_NO)
            {
            return dBConnection.GetComplianceProgressReportDetails(ROLE_TYPE, PP_NO);
            }
        [HttpPost]
        public string add_compliance_hierarchy(int ENTITY_ID, string REVIEWER_PP, string AUTHORIZER_PP)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddComplianceHierarchy(ENTITY_ID, REVIEWER_PP, AUTHORIZER_PP) + "\"}";
            }
        [HttpPost]
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
        public List<ComplianceOSParasModel> get_paras_for_compliance_summary_report()
            {
            return dBConnection.GetParasForComplianceSummaryReport();
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
            return dBConnection.GetFADMonthlyReviewParasForEntityTypeId(ENT_TYPE_ID, S_DATE, E_DATE);
            }
        [HttpGet]
        [HttpPost]
        public List<SpecialAuditPlanModel> get_saved_special_audit_plans()
            {
            return dBConnection.GetSaveSpecialAuditPlan();
            }
        [HttpPost]
        public string add_special_audit_plan(string NATURE, string PERIOD, string ENTITY_ID, string NO_DAYS, string PLAN_ID, string INDICATOR)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AddSpecialAuditPlan(NATURE, PERIOD, ENTITY_ID, NO_DAYS, PLAN_ID, INDICATOR) + "\"}";
            }
        [HttpPost]
        public string delete_special_audit_plan(string PLAN_ID, string INDICATOR)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.DeleteSpecialAuditPlan(PLAN_ID, INDICATOR) + "\"}";
            }
        [HttpPost]
        public string submit_special_audit_plan(string PLAN_ID, string INDICATOR)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitSpecialAuditPlan(PLAN_ID, INDICATOR) + "\"}";
            }
        [HttpPost]
        public string referred_back_special_audit_plan(string PLAN_ID, string INDICATOR)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitSpecialAuditPlan(PLAN_ID, INDICATOR) + "\"}";
            }
        [HttpPost]
        public string approve_special_audit_plan(string PLAN_ID, string INDICATOR)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitSpecialAuditPlan(PLAN_ID, INDICATOR) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<DuplicateDeleteManageParaModel> get_duplicate_paras_for_authorize()
            {
            return dBConnection.GetDuplicateParasForAuthorization();
            }
        [HttpPost]
        public string request_delete_duplicate_para(int NEW_PARA_ID = 0, int OLD_PARA_ID = 0, string INDICATOR = "", string REMARKS = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RequestDeleteDuplicatePara(NEW_PARA_ID, OLD_PARA_ID, INDICATOR, REMARKS) + "\"}";
            }
        [HttpPost]
        public string reject_delete_duplicate_para(int D_ID = 0)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RejectDeleteDuplicatePara(D_ID) + "\"}";
            }
        [HttpPost]
        public string authorize_delete_duplicate_para(int D_ID = 0)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthDeleteDuplicatePara(D_ID) + "\"}";
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
        public IActionResult add_responsible_for_old_paras([FromQuery] string IND_Action, [FromBody] ObservationResponsiblePPNOModel model)
            {
            var result = dBConnection.AddResponsibilityforoldparas(model.COM_ID.GetValueOrDefault(), model, IND_Action);
            return Json(new { Message = result });
            }
        [HttpPost]
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
        public string submit_dsa_to_head_fad(int DSA_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.SubmitDSAToHeadFAD(DSA_ID) + "\"}";

            }
        [HttpPost]

        //SVP AZ ACTION
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

        [HttpGet]
        [HttpPost]
        public List<LoanCaseDetailModel> get_lc_details(int LC_NO, int BR_CODE)
            {
            return dBConnection.GetLoanCaseDetailsWithBRCode(LC_NO, BR_CODE);
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
        public string update_gm_office(int GM_OFF_ID, int ENTITY_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateGMOffice(GM_OFF_ID, ENTITY_ID) + "\"}";

            }
        [HttpPost]
        public string update_reporting_line(int REP_OFF_ID, int ENTITY_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateReportingLine(REP_OFF_ID, ENTITY_ID) + "\"}";

            }

        [HttpPost]
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
        public string create_engagement_sample_data(int ENG_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.CreateSampleDataAfterEngagementApproval(ENG_ID) + "\"}";
            }

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
        public List<YearWiseOutstandingObservationsModel> get_year_wise_outstanding_observations(int ENTITY_ID)
            {
            return dBConnection.GetYearWiseOutstandingParas(ENTITY_ID);
            }

        [HttpGet]
        [HttpPost]
        public List<AuditeeOldParasModel> get_year_wise_outstanding_observations_detials(int ENTITY_ID, int AUDIT_PERIOD)
            {
            return dBConnection.GetYearWiseOutstandingParasDetails(ENTITY_ID, AUDIT_PERIOD);
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
        public IActionResult add_exception_account_report(string IND = "", int REPORT_ID = 0, string REPORT_TITLE = "", string DESCRIPTION = "", string TYPE = "", int LOAN_STATUS_ID = 0)
            {
            if (string.IsNullOrWhiteSpace(REPORT_TITLE) || !AlphaNumericWithSpacesRegex.IsMatch(REPORT_TITLE))
                {
                return BadRequest(new { Status = false, Message = "Report title must contain only letters, numbers, and spaces." });
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
        public List<ParaTextSearchModel> get_para_text_in_audit_report(string SEARCH_KEYWORD)
            {
            return dBConnection.GetAuditParasByText(SEARCH_KEYWORD);
            }
        [HttpPost]
        public string regenerate_sample_of_loans(int ENG_ID, int LOAN_SAMPLE_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RegenerateSampleofLoan(ENG_ID, LOAN_SAMPLE_ID) + "\"}";
            }

        [HttpGet]
        [HttpPost]
        public List<YearWiseAllParasModel> get_year_wise_all_audit_paras(string AUDIT_PERIOD)
            {
            return dBConnection.GetYearWiseAllParas(AUDIT_PERIOD);
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
        public IActionResult SubmitComplaint([FromForm] AIS.Models.IID.ComplaintModel model)
            {
            if (string.IsNullOrWhiteSpace(model.PertainsTo))
                {
                return BadRequest(new { message = "PertainsTo is required." });
                }

            if (string.Equals(model.Source, "Other", StringComparison.OrdinalIgnoreCase))
                {
                if (string.IsNullOrWhiteSpace(model.SourceOtherText))
                    {
                    return BadRequest(new { message = "Source other text is required." });
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
                        return BadRequest(new { message = "HO Unit Type and HO Unit are required." });
                        }
                    model.RegionId = null;
                    model.BranchId = null;
                    }
                else if (string.Equals(model.FieldType, "BRANCH", StringComparison.OrdinalIgnoreCase))
                    {
                    if (!model.RegionId.HasValue || !model.BranchId.HasValue)
                        {
                        return BadRequest(new { message = "Region and Branch are required." });
                        }
                    model.HOUnitTypeId = null;
                    model.HOUnitId = null;
                    }
                else
                    {
                    return BadRequest(new { message = "FieldType is required for Field complaints." });
                    }
                }
            else
                {
                return BadRequest(new { message = "PertainsTo must be HO or FIELD." });
                }

            var complaintFile = Request.Form.Files.GetFile("UploadedComplaint");
            var ffrFile = Request.Form.Files.GetFile("UploadedFFR");
            var evidenceFiles = Request.Form.Files.GetFiles("UploadedEvidence");
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
            return Ok(new { ComplaintId = id });
            }

        [HttpPost]
        public IActionResult AddAssessment([FromBody] AIS.Models.IID.InitialAssessmentModel model)
            {
            var id = dBConnection.AddAssessment(model);
            return Ok(new { AssessmentId = id });
            }

        [HttpPost]
        public IActionResult AddHeadReview([FromBody] AIS.Models.IID.HeadReviewModel model)
            {
            var reviewId = dBConnection.AddHeadReview(model);

            var emailMap = new Dictionary<int, string>
            {
                {1, "sukkur@iid.com"},
                {2, "multan@iid.com"},
                {3, "lahore@iid.com"},
                {4, "ho@iid.com"}
            };
            if (emailMap.ContainsKey((int)model.AssignedToUnit))
                {
                EmailConfiguration email = new EmailConfiguration(_configuration);
                string body = $"Complaint {model.ComplaintId} assigned to your unit.";
                email.ConfigEmail(emailMap[model.AssignedToUnit], "", "IAS Assignment", body);
                }
            return Ok(new { ReviewId = reviewId });
            }

        [HttpPost]
        public IActionResult AddInvestigationPlan([FromBody] AIS.Models.IID.InvestigationPlanModel model)
            {
            var id = dBConnection.AddInvestigationPlan(model);
            return Ok(new { PlanId = id });
            }

        [HttpPost]
        public IActionResult AddPlanApproval([FromBody] AIS.Models.IID.PlanApprovalModel model)
            {
            var id = dBConnection.AddPlanApproval(model);
            return Ok(new { ApprovalId = id });
            }

        [HttpPost]
        public IActionResult AddInquiryReport([FromForm] AIS.Models.IID.InquiryReportModel model)
            {
            var reportFile = Request.Form.Files.GetFile("UploadedReport");
            var evidenceFiles = Request.Form.Files.GetFiles("UploadedEvidence");
            var dsaFile = Request.Form.Files.GetFile("UploadedDsa");
            model.UploadedReport = SaveUploadFile(reportFile);
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
            model.UploadedDsa = SaveUploadFile(dsaFile);
            var id = dBConnection.AddInquiryReport(model);
            return Ok(new { ReportId = id });
            }

        [HttpPost]
        public IActionResult AddAnalysis([FromBody] AIS.Models.IID.AnalysisModel model)
            {
            var id = dBConnection.AddAnalysis(model);
            return Ok(new { AnalysisId = id });
            }

        [HttpPost]
        public IActionResult AddFinalApproval([FromBody] AIS.Models.IID.FinalApprovalModel model)
            {
            var id = dBConnection.AddFinalApproval(model);
            return Ok(new { FinalApprovalId = id });
            }

        [HttpPost]
        public IActionResult AddCaseStudy([FromBody] AIS.Models.IID.CaseStudyModel model)
            {
            var id = dBConnection.AddCaseStudy(model);
            return Ok(new { CaseStudyId = id });
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
            var list = dBConnection.GetReports(filter);
            return Ok(list);
            }


        [HttpPost]
        public IActionResult GetComplaintsByUser()
            {
            var list = dBConnection.GetComplaintsByUser();
            return Ok(list);
            }

        [HttpPost]
        public IActionResult GetComplaintsWithoutAssessment()
            {
            var list = dBConnection.Get_Complaints_Without_Assessment();
            return Ok(list);
            }

        [HttpPost]
        public IActionResult GetComplaint(int complaintId)
            {
            var complaint = dBConnection.GetComplaint(complaintId);
            return Ok(complaint);
            }

        [HttpGet]
        [HttpPost]
        public List<string> get_fad_desk_officer_audit_periods()
            {
            return dBConnection.GetDistinctFadDeskOfficerAuditPeriods();
            }

        [HttpPost]
        public PublicHolidayModel add_public_holiday([FromBody] PublicHolidayModel model)
            {
            return dBConnection.AddPublicHoliday(model);
            }

        [HttpGet]
        [HttpPost]
        public List<PublicHolidayModel> get_all_public_holidays([FromBody] HolidayYearModel input)
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

        [HttpPost]
        public List<IdNameModel> GetRelationTypes()
            {
            return dBConnection.GetRelationTypes();
            }

        [HttpPost]
        public List<IdNameModel> GetReportingOffices(int relationTypeId)
            {
            return dBConnection.GetReportingOffices(relationTypeId);
            }

        [HttpPost]
        public List<EntityModel> GetEntitiesForOffice(int reportingOfficeId)
            {
            return dBConnection.Get_Entities_For_Office(reportingOfficeId);
            }

        [HttpPost]
        [EnableRateLimiting("FileTransferPolicy")]
        public IActionResult UploadCircularFile(int circularId, IFormFile file)
            {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadedBy = User.Identity?.Name ?? "anonymous";
            using (var ms = new MemoryStream())
                {
                file.CopyTo(ms);
                var model = new CircularDocumentModel
                    {
                    CircularId = circularId,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    FileBlob = ms.ToArray(),
                    UploadedBy = uploadedBy
                    };
                var db = CreateDbConnection();
                var status = db.SaveCircularDocument(model);
                }
            return Ok("File uploaded successfully!");
            }

        [HttpPost]
        [EnableRateLimiting("FileTransferPolicy")]
        public IActionResult UploadCircularFiles(int circularId, List<IFormFile> files)
            {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");
            var db = CreateDbConnection();
            var uploadedBy = User.Identity?.Name ?? "anonymous";
            int successCount = 0;
            foreach (var file in files)
                {
                using (var ms = new MemoryStream())
                    {
                    file.CopyTo(ms);
                    string status;
                    db.InsertCircularDoc(
                        circularId,
                        file.FileName,
                        file.ContentType,
                        file.Length,
                        ms.ToArray(),
                        uploadedBy,
                        out status
                    );
                    if (status != null && status.StartsWith("Success"))
                        successCount++;
                    }
                }
            return Ok($"{successCount} file(s) uploaded successfully.");
            }

        [HttpGet]
        [EnableRateLimiting("FileTransferPolicy")]
        public IActionResult DownloadCircularFileFromDb(int docId)
            {
            var db = CreateDbConnection();
            var doc = db.GetCircularDocument(docId);
            if (doc == null || doc.FileBlob == null) return NotFound();
            return File(doc.FileBlob, doc.FileType ?? "application/octet-stream", doc.FileName);
            }


        [HttpPost]
        public string AllocateEntitiesToAuditor(int azId, int entId, int auditorPPNO)
            {
            _ = sessionHandler.GetUserOrThrow();
            return dBConnection.AllocateEntityToAuditor(azId, entId, auditorPPNO);
            }

        [HttpPost]
        public List<ObservationReferenceModel> GetObservationsForReferenceUpdate(int? entId, int? assignedAuditorId, int? referenceId)
            {
            return dBConnection.GetObservationsForReferenceUpdate(entId, assignedAuditorId, referenceId);
            }

        [HttpPost]
        public string UpdateParaReference(int comId, int? linkId, int newRef)
            {
            _ = sessionHandler.GetUserOrThrow();
            return dBConnection.UpdateParaReference(comId, linkId, newRef);
            }

        [HttpGet]
        public IActionResult GetPendingReferenceParas()
            {
            var list = dBConnection.GetPendingReferenceParas();
            return Json(list);
            }

        [HttpGet]
        public IActionResult GetParaReferenceData(int comId)
            {
            var data = dBConnection.GetParaReferenceData(comId);
            return Json(data);
            }

        [HttpPost]
        public string SaveParaReferences([FromBody] SaveParaReferencesRequestModel model)
            {
            return dBConnection.SaveParaReferences(model.ComId.GetValueOrDefault(), model.References);
            }

        [HttpPost]
        public List<UpdateLogModel> GetUpdateLog(int comId)
            {
            return dBConnection.GetUpdateLog(comId);
            }

        [HttpPost]
        public List<ReferenceSearchResultModel> SearchReferences(string referenceType, string keyword)
            {
            _ = sessionHandler.GetUserOrThrow();
            return dBConnection.SearchReferences(referenceType, keyword);
            }

        [HttpGet]
        public IActionResult GetReferenceDetail(int refId)
            {
            var detail = dBConnection.GetReferenceDetail(refId);
            return Json(detail);
            }

        [HttpGet]
        public JsonResult GetPendingParas(int entityId, int auditYear)
            {
            var list = dBConnection.GetPendingParas(entityId, auditYear);
            return Json(list);
            }

        [HttpGet]
        public JsonResult GetReferenceEntitySummary()
            {
            _ = sessionHandler.GetUserOrThrow();
            var list = dBConnection.GetReferenceEntitySummary();
            return Json(list);
            }

        [HttpGet]
        public JsonResult GetEntityTaskSummary()
            {
            _ = sessionHandler.GetUserOrThrow();
            var list = dBConnection.GetEntityTaskSummary();
            return Json(list);
            }

        [HttpGet]
        public List<VersionHistoryModel> GetAllVersionHistory()
            {
            return dBConnection.GetAllVersionHistory();
            }

        [HttpPost]
        public string AddVersionHistory([FromBody] VersionHistoryModel model)
            {
            return dBConnection.AddVersionHistory(model);
            }

        [HttpPost]
        public string UpdateVersionHistory([FromBody] VersionHistoryModel model)
            {
            return dBConnection.UpdateVersionHistory(model);
            }

        [HttpGet]
        [HttpPost]
        public IActionResult GET_PARA_STATUS_CHANGE_REQUEST(int entityId, int status)
            {
            var data = dBConnection.GETPARASTATUSCHANGEREQUEST(entityId, status);
            return PartialView("~/Views/FAD/_ParaStatusTable.cshtml", data);
            }

        [HttpPost]
        public IActionResult ADD_PARA_STATUS_CHANGE_REQUEST(int comId, int newStatus, string makerRemarks)
            {
            var user = sessionHandler.GetUserOrThrow();
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
        public IActionResult AUTHORIZE_PARA_STATUS_CHANGE_REQUEST(int logId, string action, string authRemarks)
            {
            var user = sessionHandler.GetUserOrThrow();
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

            if (!AlphaNumericWithSpacesRegex.IsMatch(model.ColumnHeader ?? string.Empty))
                {
                return InvalidRequestResponse("ColumnHeader", "Column header must contain only letters, numbers, and spaces.");
                }

            return null;
            }

        private static bool TryParseIsoDate(string value, out DateTime date)
            {
            return DateTime.TryParseExact(value ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
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
