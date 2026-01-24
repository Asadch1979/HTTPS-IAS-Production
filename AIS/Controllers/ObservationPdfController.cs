using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

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
            var totalStopwatch = Stopwatch.StartNew();
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

                _logger.LogInformation("Generating observation print view for OBS_ID {ObsId} ENG_ID {EngId} by user {UserId}.", obsId, engId, user?.USER_ID);

                var dataStopwatch = Stopwatch.StartNew();
                var data = _dbConnection.GetObservationPdfData(obsId);
                dataStopwatch.Stop();
                _logger.LogInformation("Observation print data retrieval completed in {ElapsedMs} ms for OBS_ID {ObsId}.", dataStopwatch.ElapsedMilliseconds, obsId);
                if (data == null || string.IsNullOrWhiteSpace(data.MemoNumber))
                    {
                    return BadRequest("Observation data is not available for the selected record.");
                    }

                totalStopwatch.Stop();
                _logger.LogInformation("Observation print view request completed in {ElapsedMs} ms for OBS_ID {ObsId}.", totalStopwatch.ElapsedMilliseconds, obsId);
                return View("ObservationPrint", data);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate observation print view for OBS_ID {ObsId}.", obsId);
                return StatusCode(500, "An error occurred while generating the print view. Please try again later.");
                }
            }
        }
    }
