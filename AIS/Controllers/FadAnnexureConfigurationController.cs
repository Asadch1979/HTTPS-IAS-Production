using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    public sealed class FadAnnexureConfigurationController : Controller
        {
        private readonly DBConnection _dbConnection;
        private readonly ILogger<FadAnnexureConfigurationController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly TopMenus _topMenus;

        public FadAnnexureConfigurationController(
            DBConnection dbConnection,
            ILogger<FadAnnexureConfigurationController> logger,
            SessionHandler sessionHandler,
            TopMenus topMenus,
            IHttpContextAccessor httpContextAccessor)
            {
            _dbConnection = dbConnection;
            _logger = logger;
            _sessionHandler = sessionHandler;
            _topMenus = topMenus;
            }

        [HttpGet]
        public async Task<IActionResult> AnnexureConfiguration()
            {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Index", "Login");

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                return RedirectToAction("Index", "PageNotFound");

            ViewData["TopMenu"] = _topMenus.GetTopMenus();
            ViewData["TopMenuPages"] = _topMenus.GetTopMenusPages();

            try
                {
                return View(await _dbConnection.GetFadAnnexureConfigurationsAsync());
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unable to retrieve Field Audit Annexure shifting configuration.");
                TempData["ErrorMessage"] = "Unable to load Annexure configuration. Please try again.";
                return View(Array.Empty<FadAnnexureConfiguration>());
                }
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAnnexureStatus([FromBody] UpdateFadAnnexureStatusRequest request)
            {
            if (User.Identity?.IsAuthenticated != true)
                return Unauthorized(new { success = false, message = "Your session has expired." });

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Annexure ID and a status of Y or N are required." });

            var user = _sessionHandler.GetUser();
            if (user == null || !int.TryParse(user.PPNumber, out var ppNumber) || ppNumber <= 0)
                return Unauthorized(new { success = false, message = "Your session has expired." });

            try
                {
                var result = await _dbConnection.UpdateFadAnnexureStatusAsync(request, ppNumber);
                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                return Ok(new
                    {
                    success = true,
                    message = result.Message,
                    updatedOn = result.UpdatedOn?.ToString("dd-MMM-yyyy HH:mm")
                    });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unable to update Annexure {AnnexureId} shifting status.", request?.AnnexureId);
                return StatusCode(500, new { success = false, message = "Unable to update the Annexure status." });
                }
            }
        }
    }
