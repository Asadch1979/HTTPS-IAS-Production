using AIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using AIS.Models;
using Oracle.ManagedDataAccess.Client;

namespace AIS.Controllers
{
    [Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class OrganizationStructureController : Controller
    {
        private readonly SessionHandler session;
        private readonly IPermissionService permissions;
        private readonly IPageIdResolver pageIds;
        private readonly OrganizationStructureService organization;
        private readonly TopMenus menus;
        private readonly ILogger<OrganizationStructureController> logger;

        public OrganizationStructureController(SessionHandler session, IPermissionService permissions,
            IPageIdResolver pageIds, OrganizationStructureService organization, TopMenus menus,
            ILogger<OrganizationStructureController> logger)
        {
            this.session = session;
            this.permissions = permissions;
            this.pageIds = pageIds;
            this.organization = organization;
            this.menus = menus;
            this.logger = logger;
        }

        private bool CanView() => session.GetUser() is { } user &&
            permissions.HasViewPermission(user, pageIds.ResolvePageId("/Dashboard/entity_wise_obs"));

        [HttpGet]
        public IActionResult Index()
        {
            if (!CanView()) return StatusCode(403);
            ViewData["TopMenu"] = menus.GetTopMenus();
            ViewData["TopMenuPages"] = menus.GetTopMenusPages();
            ViewData["CanMoveEntity"] = session.IsSuperUser();
            return View();
        }

        [HttpGet]
        public IActionResult Data(bool refresh = false)
        {
            if (!CanView()) return StatusCode(403);
            try { return Json(organization.GetSnapshot(refresh)); }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to load organizational structure.");
                return StatusCode(503, new { message = "Organization structure is temporarily unavailable. Please try again." });
            }
        }

        [HttpGet]
        public IActionResult PreviewMove(int entityId, int newParentId)
        {
            if (!CanView() || !session.IsSuperUser()) return StatusCode(403);
            if (entityId <= 0 || newParentId <= 0) return BadRequest(new { message = "Select a valid entity and destination." });
            try { return Json(organization.PreviewMove(entityId, newParentId)); }
            catch (Exception exception) { return MoveError(exception); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Move([FromForm] OrganizationMoveRequest move)
        {
            if (!CanView() || !session.IsSuperUser()) return StatusCode(403);
            if (!ModelState.IsValid || move == null || move.EntityId <= 0 || move.NewParentId <= 0 ||
                move.ExpectedParentId <= 0 || string.IsNullOrWhiteSpace(move.Reason) || move.Reason.Length > 1000)
                return BadRequest(new { message = "Valid entity IDs and a change reason of up to 1000 bytes are required." });
            try { return Json(new { moveId = organization.Move(move) }); }
            catch (Exception exception) { return MoveError(exception); }
        }

        private IActionResult MoveError(Exception exception)
        {
            if (exception is OracleException oracle)
            {
                if (oracle.Number == 54 || oracle.Number == 30006)
                    return StatusCode(409, new { message = "The mapping table is busy. Please retry shortly." });
                if (oracle.Number >= 20001 && oracle.Number <= 20022)
                {
                    var message = oracle.Message.Split('\n')[0];
                    var separator = message.IndexOf(':');
                    return BadRequest(new { message = separator >= 0 ? message.Substring(separator + 1).Trim() : message });
                }
            }
            logger.LogError(exception, "Unable to preview or move organizational entity.");
            return StatusCode(503, new { message = "The entity move could not be completed. Refresh the structure before retrying." });
        }

        [HttpGet]
        public IActionResult OpenParaCounts()
        {
            if (!CanView()) return StatusCode(403);
            try { return Json(organization.GetOpenParaCounts()); }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to load organizational open para counts.");
                return StatusCode(503, new { message = "Open para counts are temporarily unavailable. Please retry." });
            }
        }

        [HttpGet]
        public IActionResult EntityPath(int entityId)
        {
            if (!CanView()) return StatusCode(403);
            if (entityId <= 0) return BadRequest();
            try
            {
                var path = organization.GetPath(entityId);
                return path == null ? StatusCode(403) : Json(path);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to load organizational reporting path.");
                return StatusCode(503, new { message = "Reporting path is temporarily unavailable." });
            }
        }
    }
}
