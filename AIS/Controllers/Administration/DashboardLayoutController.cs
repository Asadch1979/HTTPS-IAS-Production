using System;
using System.Collections.Generic;
using System.Linq;
using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AIS.Controllers
    {
    [IgnoreAntiforgeryToken]
    [Route("Administration/DashboardLayout")]
    public class DashboardLayoutController : Controller
        {
        private readonly ILogger<DashboardLayoutController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;

        public DashboardLayoutController(
            ILogger<DashboardLayoutController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            }

        [HttpGet("GetByRole")]
        public IActionResult GetByRole(int roleId)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { success = false, message = "User session is not authenticated." });
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, new { success = false, message = "You don't have access." });
                }

            if (roleId <= 0)
                {
                return BadRequest(new { success = false, message = "Role is required." });
                }

            var results = _dbConnection.GetRoleDashboardConfig(roleId) ?? new List<DashboardLayoutPageModel>();
            return Json(new { success = true, data = results });
            }

        [HttpGet("GetAvailablePages")]
        public IActionResult GetAvailablePages(int roleId)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { success = false, message = "User session is not authenticated." });
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, new { success = false, message = "You don't have access." });
                }

            if (roleId <= 0)
                {
                return BadRequest(new { success = false, message = "Role is required." });
                }

            var assignedPages = _dbConnection.GetRoleDashboardPages(roleId) ?? new List<DashboardLayoutPageModel>();
            var existingPages = _dbConnection.GetRoleDashboardConfig(roleId) ?? new List<DashboardLayoutPageModel>();
            var existingIds = new HashSet<int>(existingPages.Select(page => page.PageId));
            var available = assignedPages.Where(page => !existingIds.Contains(page.PageId)).ToList();

            return Json(new { success = true, data = available });
            }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] List<DashboardLayoutSaveRequest> items)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { success = false, message = "User session is not authenticated." });
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, new { success = false, message = "You don't have access." });
                }

            if (items == null || items.Count == 0)
                {
                return BadRequest(new { success = false, message = "Invalid dashboard payload." });
                }

            var failures = new List<string>();
            var existingPagesByRole = new Dictionary<int, HashSet<int>>();

            foreach (var item in items)
                {
                if (item == null || item.RoleId <= 0 || item.PageId <= 0 || string.IsNullOrWhiteSpace(item.ActionInd))
                    {
                    failures.Add("Invalid dashboard payload detected.");
                    continue;
                    }

                item.IsActive = string.IsNullOrWhiteSpace(item.IsActive) ? "Y" : item.IsActive;
                item.ActionInd = item.ActionInd.Trim().ToUpperInvariant();

                if (item.ActionInd == "A")
                    {
                    if (!existingPagesByRole.TryGetValue(item.RoleId, out var existingPages))
                        {
                        existingPages = new HashSet<int>(
                            (_dbConnection.GetRoleDashboardConfig(item.RoleId) ?? new List<DashboardLayoutPageModel>())
                            .Select(page => page.PageId));
                        existingPagesByRole[item.RoleId] = existingPages;
                        }

                    if (existingPages.Contains(item.PageId))
                        {
                        failures.Add($"Page {item.PageId} is already configured for the dashboard.");
                        continue;
                        }
                    }

                try
                    {
                    _dbConnection.MaintainRoleDashboardPage(item);
                    }
                catch (Exception ex)
                    {
                    _logger.LogError(ex, "Failed to save dashboard layout for role {RoleId} page {PageId}.", item.RoleId, item.PageId);
                    failures.Add($"Failed to save page {item.PageId}.");
                    }
                }

            if (failures.Any())
                {
                return StatusCode(500, new { success = false, message = string.Join(" ", failures) });
                }

            return Json(new { success = true });
            }
        }
    }
