using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
    {
    [Route("Observation")]
    public class ObservationPdfController : BaseController
        {
        private readonly ILogger<ObservationPdfController> _logger;
        private readonly DBConnection _dbConnection;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;
        private const string ManageObservationBranchesPath = "/Execution/manage_observations_branches";

        public ObservationPdfController(
            ILogger<ObservationPdfController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            IPermissionService permissionService,
            IPageIdResolver pageIdResolver)
            : base(sessionHandler)
            {
            _logger = logger;
            _dbConnection = dbConnection;
            _permissionService = permissionService;
            _pageIdResolver = pageIdResolver;
            }

        [HttpGet("GeneratePdf")]
        public IActionResult GeneratePdf(int obsId, int engId)
            {
            try
                {
                if (!SessionHandler.TryGetUser(out var user))
                    {
                    return StatusCode(401, "Session expired. Please sign in again.");
                    }

                if (!User.Identity.IsAuthenticated)
                    {
                    return StatusCode(401, "User session is not authenticated.");
                    }

                if (!HasObservationPdfPermission(user))
                    {
                    return StatusCode(403, "User is not authorized to access observation PDFs.");
                    }

                if (obsId <= 0)
                    {
                    return BadRequest("Observation id is required.");
                    }

                if (engId <= 0)
                    {
                    return BadRequest("Engagement id is required.");
                    }

                var observation = GetAuthorizedObservation(engId, obsId);
                if (observation == null)
                    {
                    return StatusCode(403, "Not authorized. Observation is not available for this engagement.");
                    }

                _logger.LogInformation("Preparing observation print view for OBS_ID {ObsId} ENG_ID {EngId} by user {UserId}.", obsId, engId, user?.PPNumber);

                var data = _dbConnection.GetObservationPrintDetails(obsId);
                if (!HasPrintableObservationData(data))
                    {
                    _logger.LogWarning("Observation print data is missing for OBS_ID {ObsId} ENG_ID {EngId}.", obsId, engId);
                    return BadRequest("Observation print data is not available for the selected record.");
                    }

                if (!data.ReferenceId.HasValue)
                    {
                    data.ReferenceId = observation?.ReferenceId;
                    }

                if (string.IsNullOrWhiteSpace(data.MemoNumber) && observation.MEMO_NO > 0)
                    {
                    data.MemoNumber = observation.MEMO_NO.ToString();
                    }

                if (string.IsNullOrWhiteSpace(data.Title))
                    {
                    data.Title = observation.HEADING;
                    }

                if (data.ReferenceId.HasValue && data.ReferenceId.Value > 0)
                    {
                    var referenceDetail = _dbConnection.GetReferenceDetailByRefId(data.ReferenceId.Value);
                    data.ReferenceText = FormatReferenceText(referenceDetail);
                    }

                data.Responsibilities = _dbConnection.GetObservationPrintResponsibilities(obsId, engId);

                return View("~/Views/Observation/ObservationPrint.cshtml", data);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to prepare observation print view for OBS_ID {ObsId} ENG_ID {EngId}.", obsId, engId);
                return StatusCode(500, "An error occurred while preparing the print view. Please try again later.");
                }
            }

        private bool HasObservationPdfPermission(SessionUser user)
            {
            if (this.UserHasPagePermissionForCurrentAction(SessionHandler))
                {
                return true;
                }

            if (_pageIdResolver?.TryResolvePageId(ManageObservationBranchesPath, out var managePageId) == true
                && managePageId > 0
                && _permissionService.HasViewPermission(user, managePageId))
                {
                return true;
                }

            _logger.LogWarning("Observation PDF permission denied for user {UserId}. Current PAGE_ID {PageId}, fallback path {Path}.",
                user?.PPNumber,
                SessionHandler.GetPageId(),
                ManageObservationBranchesPath);
            return false;
            }

        private ManageObservations GetAuthorizedObservation(int engId, int obsId)
            {
            if (engId <= 0 || obsId <= 0)
                {
                return null;
                }

            return _dbConnection.GetManagedObservationsForBranches(engId, obsId).FirstOrDefault();
            }

        private static bool HasPrintableObservationData(ObservationPdfDataModel data)
            {
            if (data == null)
                {
                return false;
                }

            return !string.IsNullOrWhiteSpace(data.MemoNumber)
                || !string.IsNullOrWhiteSpace(data.Title)
                || !string.IsNullOrWhiteSpace(data.ParaText)
                || !string.IsNullOrWhiteSpace(data.EntityName)
                || !string.IsNullOrWhiteSpace(data.Annexure);
            }

        private static string FormatReferenceText(ReferenceMasterDetailItemModel detail)
            {
            if (detail == null)
                {
                return string.Empty;
                }

            if (string.Equals(detail.ReferenceSourceType, "MANUAL_INDEX", StringComparison.OrdinalIgnoreCase))
                {
                var parts = new List<string>
                    {
                    detail.ReferenceType,
                    detail.SectionText,
                    detail.ChapterNo,
                    detail.SubSectionNo,
                    detail.TitleOrHeading
                    };

                return string.Join(" / ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
                }

            var text = !string.IsNullOrWhiteSpace(detail.DisplayText)
                ? detail.DisplayText
                : detail.TitleOrHeading;

            if (detail.InstructionDate.HasValue)
                {
                var dateText = detail.InstructionDate.Value.ToString("yyyy-MM-dd");
                return string.IsNullOrWhiteSpace(text) ? dateText : text + " (" + dateText + ")";
                }

            return text ?? string.Empty;
            }

        }
    }
