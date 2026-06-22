using System;
using System.Collections.Generic;
using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AIS.Controllers
    {
    [IgnoreAntiforgeryToken]
    [Route("Administration/ApiMaster")]
    public class ApiMasterController : Controller
        {
        private readonly ILogger<ApiMasterController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;

        public ApiMasterController(
            ILogger<ApiMasterController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            }

        [HttpGet("List")]
        public IActionResult List()
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { success = false, message = "User session is not authenticated." });
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, new { success = false, message = "You don't have access." });
                }

            if (!_sessionHandler.IsSuperUser())
                {
                return StatusCode(403, new { success = false, message = "Only Super Admins can access API Master." });
                }

            var results = _dbConnection.GetApiMasterList() ?? new List<ApiMasterModel>();
            return Json(new { success = true, data = results });
            }

        [HttpGet("ControllerOptions")]
        public IActionResult ControllerOptions()
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { success = false, message = "User session is not authenticated." });
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, new { success = false, message = "You don't have access." });
                }

            if (!_sessionHandler.IsSuperUser())
                {
                return StatusCode(403, new { success = false, message = "Only Super Admins can access API Master." });
                }

            var options = _dbConnection.GetApiMasterControllerNames() ?? new List<string>();
            return Json(new { success = true, data = options });
            }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] ApiMasterSaveRequest request)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { success = false, message = "User session is not authenticated." });
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, new { success = false, message = "You don't have access." });
                }

            if (!_sessionHandler.IsSuperUser())
                {
                return StatusCode(403, new { success = false, message = "Only Super Admins can access API Master." });
                }

            if (request == null)
                {
                return BadRequest(new { success = false, message = "No API payload was provided." });
                }

            var action = request.ActionInd?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(action))
                {
                return BadRequest(new { success = false, message = "Action indicator is required." });
                }

            if (action != "A" && request.ApiId <= 0)
                {
                return BadRequest(new { success = false, message = "API identifier is required for this action." });
                }

            if (action != "D")
                {
                if (string.IsNullOrWhiteSpace(request.ApiName) ||
                    string.IsNullOrWhiteSpace(request.ControllerName) ||
                    string.IsNullOrWhiteSpace(request.ApiPath) ||
                    string.IsNullOrWhiteSpace(request.HttpMethod) ||
                    request.PageId <= 0)
                    {
                    return BadRequest(new { success = false, message = "API name, controller name, path, method, and page are required." });
                    }

                var normalizedPath = NormalizeApiPath(request.ApiPath);
                var normalizedMethod = NormalizeHttpMethod(request.HttpMethod);
                if (string.IsNullOrWhiteSpace(normalizedMethod))
                    {
                    return BadRequest(new { success = false, message = "HTTP Method must be GET, POST, or BOTH." });
                    }

                request.ApiPath = normalizedPath;
                request.HttpMethod = normalizedMethod;
                }

            try
                {
                switch (action)
                    {
                    case "A":
                        InsertOrExpandApiMaster(request);
                        break;
                    case "U":
                        UpdateOrExpandApiMaster(request);
                        break;
                    case "D":
                        _dbConnection.MaintainApiMaster(new ApiMasterModel
                            {
                            ApiId = request.ApiId,
                            IsActive = "N"
                            }, "D");
                        break;
                    default:
                        return BadRequest(new { success = false, message = "Unsupported action indicator." });
                    }
                }
            catch (InvalidOperationException ex)
                {
                return BadRequest(new { success = false, message = ex.Message });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to save API master entry for path {Path}.", request.ApiPath);
                return StatusCode(500, new { success = false, message = "Failed to save API master entry." });
                }

            return Json(new { success = true });
            }

        private void InsertOrExpandApiMaster(ApiMasterSaveRequest request)
            {
            if (string.Equals(request.HttpMethod, "BOTH", StringComparison.OrdinalIgnoreCase))
                {
                EnsureApiPathIsUnique(request.ApiPath, "GET", request.PageId);
                EnsureApiPathIsUnique(request.ApiPath, "POST", request.PageId);

                _dbConnection.InsertApiMaster(BuildApiMasterModel(request, "GET"));
                _dbConnection.InsertApiMaster(BuildApiMasterModel(request, "POST"));
                return;
                }

            EnsureApiPathIsUnique(request.ApiPath, request.HttpMethod, request.PageId);
            _dbConnection.InsertApiMaster(BuildApiMasterModel(request, request.HttpMethod));
            }

        private void UpdateOrExpandApiMaster(ApiMasterSaveRequest request)
            {
            var existingEntries = _dbConnection.GetApiMasterList() ?? new List<ApiMasterModel>();
            var existingEntry = existingEntries.Find(item => item.ApiId == request.ApiId);
            if (existingEntry == null)
                {
                throw new InvalidOperationException("API definition not found.");
                }

            if (!string.Equals(request.HttpMethod, "BOTH", StringComparison.OrdinalIgnoreCase))
                {
                if (!MatchesPageApiKey(existingEntry, request.ApiPath, request.HttpMethod, request.PageId))
                    {
                    EnsureApiPathIsUnique(request.ApiPath, request.HttpMethod, request.PageId);
                    }

                _dbConnection.UpdateApiMaster(BuildApiMasterModel(request, request.HttpMethod, request.ApiId));
                return;
                }

            var primaryMethod = NormalizeStoredHttpMethod(existingEntry.HttpMethod);
            var secondaryMethod = string.Equals(primaryMethod, "POST", StringComparison.OrdinalIgnoreCase) ? "GET" : "POST";
            var existingPath = NormalizeApiPath(existingEntry.ApiPath);
            var counterpartEntry = existingEntries.Find(item =>
                item.ApiId != request.ApiId &&
                item.PageId == existingEntry.PageId &&
                string.Equals(NormalizeStoredHttpMethod(item.HttpMethod), secondaryMethod, StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(NormalizeApiPath(item.ApiPath), existingPath, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(NormalizeApiPath(item.ApiPath), request.ApiPath, StringComparison.OrdinalIgnoreCase)));

            if (!MatchesPageApiKey(existingEntry, request.ApiPath, primaryMethod, request.PageId))
                {
                EnsureApiPathIsUnique(request.ApiPath, primaryMethod, request.PageId);
                }

            if (counterpartEntry == null
                || !MatchesPageApiKey(counterpartEntry, request.ApiPath, secondaryMethod, request.PageId))
                {
                EnsureApiPathIsUnique(request.ApiPath, secondaryMethod, request.PageId);
                }

            _dbConnection.UpdateApiMaster(BuildApiMasterModel(request, primaryMethod, request.ApiId));

            if (counterpartEntry == null)
                {
                _dbConnection.InsertApiMaster(BuildApiMasterModel(request, secondaryMethod));
                }
            else
                {
                _dbConnection.UpdateApiMaster(BuildApiMasterModel(request, secondaryMethod, counterpartEntry.ApiId));
                }
            }

        private void EnsureApiPathIsUnique(string apiPath, string httpMethod, int pageId)
            {
            if (_dbConnection.ApiPathExists(apiPath, httpMethod, pageId))
                {
                throw new InvalidOperationException("API path and method must be unique within the selected page.");
                }
            }

        private static bool MatchesPageApiKey(ApiMasterModel entry, string apiPath, string httpMethod, int pageId)
            {
            return entry != null
                && entry.PageId == pageId
                && string.Equals(NormalizeApiPath(entry.ApiPath), NormalizeApiPath(apiPath), StringComparison.OrdinalIgnoreCase)
                && string.Equals(NormalizeStoredHttpMethod(entry.HttpMethod), NormalizeStoredHttpMethod(httpMethod), StringComparison.OrdinalIgnoreCase);
            }

        private static ApiMasterModel BuildApiMasterModel(ApiMasterSaveRequest request, string httpMethod, int apiId = 0)
            {
            return new ApiMasterModel
                {
                ApiId = apiId,
                ApiName = request.ApiName?.Trim(),
                ControllerName = request.ControllerName?.Trim(),
                ApiPath = request.ApiPath?.Trim(),
                HttpMethod = httpMethod,
                PageId = request.PageId,
                IsActive = NormalizeIsActive(request.IsActive)
                };
            }

        private static string NormalizeApiPath(string path)
            {
            if (string.IsNullOrWhiteSpace(path))
                {
                return string.Empty;
                }

            var cleaned = path.Trim();
            cleaned = cleaned.Replace("\\", "/");
            var separatorIndex = cleaned.IndexOf(";", StringComparison.Ordinal);
            if (separatorIndex >= 0)
                {
                cleaned = cleaned.Substring(0, separatorIndex);
                }

            if (!cleaned.StartsWith("/", StringComparison.Ordinal))
                {
                cleaned = "/" + cleaned;
                }

            return cleaned.Trim();
            }

        private static string NormalizeHttpMethod(string httpMethod)
            {
            if (string.IsNullOrWhiteSpace(httpMethod))
                {
                return string.Empty;
                }

            var normalized = httpMethod.Trim().ToUpperInvariant();
            if (normalized == "GET" || normalized == "POST" || normalized == "BOTH")
                {
                return normalized;
                }

            return string.Empty;
            }

        private static string NormalizeStoredHttpMethod(string httpMethod)
            {
            var normalized = NormalizeHttpMethod(httpMethod);
            return string.IsNullOrWhiteSpace(normalized) || normalized == "BOTH" ? "GET" : normalized;
            }

        private static string NormalizeIsActive(string isActive)
            {
            if (string.IsNullOrWhiteSpace(isActive))
                {
                return "Y";
                }

            var normalized = isActive.Trim().ToUpperInvariant();
            return normalized == "N" ? "N" : "Y";
            }
        }
    }
