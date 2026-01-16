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
                if (string.IsNullOrWhiteSpace(request.ApiName) || string.IsNullOrWhiteSpace(request.ApiPath) || string.IsNullOrWhiteSpace(request.HttpMethod))
                    {
                    return BadRequest(new { success = false, message = "API name, path, and method are required." });
                    }

                var normalizedPath = NormalizeApiPath(request.ApiPath);
                var normalizedMethod = request.HttpMethod.Trim().ToUpperInvariant();

                if (_dbConnection.ApiPathExists(normalizedPath, normalizedMethod, request.ApiId))
                    {
                    return BadRequest(new { success = false, message = "API path and method must be unique." });
                    }

                request.ApiPath = normalizedPath;
                request.HttpMethod = normalizedMethod;
                }

            try
                {
                switch (action)
                    {
                    case "A":
                        _dbConnection.InsertApiMaster(new ApiMasterModel
                            {
                            ApiName = request.ApiName?.Trim(),
                            ApiPath = request.ApiPath?.Trim(),
                            HttpMethod = request.HttpMethod?.Trim().ToUpperInvariant(),
                            IsActive = NormalizeIsActive(request.IsActive)
                            });
                        break;
                    case "U":
                        _dbConnection.UpdateApiMaster(new ApiMasterModel
                            {
                            ApiId = request.ApiId,
                            ApiName = request.ApiName?.Trim(),
                            ApiPath = request.ApiPath?.Trim(),
                            HttpMethod = request.HttpMethod?.Trim().ToUpperInvariant(),
                            IsActive = NormalizeIsActive(request.IsActive)
                            });
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
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to save API master entry for path {Path}.", request.ApiPath);
                return StatusCode(500, new { success = false, message = "Failed to save API master entry." });
                }

            return Json(new { success = true });
            }

        private static string NormalizeApiPath(string path)
            {
            if (string.IsNullOrWhiteSpace(path))
                {
                return string.Empty;
                }

            var cleaned = path.Trim();
            var separatorIndex = cleaned.IndexOf(";", StringComparison.Ordinal);
            if (separatorIndex >= 0)
                {
                cleaned = cleaned.Substring(0, separatorIndex);
                }

            return cleaned.Trim();
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
