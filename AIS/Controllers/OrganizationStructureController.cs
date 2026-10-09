using AIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

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
            return View();
        }

        [HttpGet]
        public IActionResult Data()
        {
            if (!CanView()) return StatusCode(403);
            try { return Json(organization.GetSnapshot()); }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to load organizational structure.");
                return StatusCode(503, new { message = "Organization structure is temporarily unavailable. Please try again." });
            }
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
