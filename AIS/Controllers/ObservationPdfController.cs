using AIS.Models;
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

        public ObservationPdfController(
            ILogger<ObservationPdfController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection)
            : base(sessionHandler)
            {
            _logger = logger;
            _dbConnection = dbConnection;
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

                if (!this.UserHasPagePermissionForCurrentAction(SessionHandler))
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

                if (!IsObservationAuthorized(engId, obsId))
                    {
                    return StatusCode(403, "Not authorized. Observation is not available for this engagement.");
                    }

                _logger.LogInformation("Preparing observation print view for OBS_ID {ObsId} ENG_ID {EngId} by user {UserId}.", obsId, engId, user?.PPNumber);

                var data = _dbConnection.GetObservationPrintDetails(obsId);
                if (data == null || string.IsNullOrWhiteSpace(data.MemoNumber))
                    {
                    return BadRequest("Observation data is not available for the selected record.");
                    }

                if (!data.ReferenceId.HasValue)
                    {
                    var observation = _dbConnection.GetManagedObservationsForBranches(engId, obsId).FirstOrDefault();
                    data.ReferenceId = observation?.ReferenceId;
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
                _logger.LogError(ex, "Failed to prepare observation print view for OBS_ID {ObsId}.", obsId);
                return StatusCode(500, "An error occurred while preparing the print view. Please try again later.");
                }
            }

        private bool IsObservationAuthorized(int engId, int obsId)
            {
            if (engId <= 0 || obsId <= 0)
                {
                return false;
                }

            return _dbConnection.GetManagedObservationsForBranches(engId, obsId).Any();
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
