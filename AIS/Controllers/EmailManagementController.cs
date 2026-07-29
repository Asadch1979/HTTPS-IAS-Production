using AIS.Models.EmailManagement;
using AIS.Services.EmailManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    [Authorize]
    [Route("EmailManagement")]
    public class EmailManagementController : Controller
        {
        private readonly DBConnection _db;
        private readonly SessionHandler _session;
        private readonly IStandaloneEmailManagementService _service;

        public EmailManagementController(
            DBConnection db,
            SessionHandler session,
            IStandaloneEmailManagementService service)
            {
            _db = db;
            _session = session;
            _service = service;
            }

        [HttpGet("")]
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            var events = _db.GetManagedEmailEvents();
            var templates = _db.GetManagedEmailTemplates();
            var logs = _db.GetManagedEmailLogs(new EmailManagementLogFilter
                {
                FromDate = DateTime.UtcNow.AddDays(-30),
                Status = EmailManagementStatuses.SendFailed
                });
            return View(new EmailManagementDashboard
                {
                EventCount = events.Count,
                EnabledEventCount = events.Count(item => item.IsEnabled),
                TemplateCount = templates.Count,
                FailedAttemptCount = logs.Count,
                RecentLogs = _db.GetManagedEmailLogs().Take(10).ToList()
                });
            }

        [HttpGet("Events")]
        public IActionResult Events(long? id = null)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            ViewBag.Events = _db.GetManagedEmailEvents();
            return View(id.HasValue ? _db.GetManagedEmailEvents(id).SingleOrDefault() ?? new EmailManagementEvent() : new EmailManagementEvent());
            }

        [HttpPost("Events")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEvent(EmailManagementEvent model)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            if (!ModelState.IsValid)
                {
                ViewBag.Events = _db.GetManagedEmailEvents();
                return View("Events", model);
                }
            _db.SaveManagedEmailEvent(model, CurrentUser());
            TempData["EmailManagementMessage"] = "Email event saved.";
            return RedirectToAction(nameof(Events));
            }

        [HttpPost("Events/Toggle")]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleEvent(long id, bool enabled)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            _db.SetManagedEmailEventEnabled(id, enabled, CurrentUser());
            return RedirectToAction(nameof(Events));
            }

        [HttpGet("Templates")]
        public IActionResult Templates(long? id = null, long? eventId = null)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            PopulateEvents();
            ViewBag.Templates = _db.GetManagedEmailTemplates(eventId);
            return View(id.HasValue ? _db.GetManagedEmailTemplates(null, id).SingleOrDefault() ?? new EmailManagementTemplate() : new EmailManagementTemplate { EventId = eventId ?? 0 });
            }

        [HttpPost("Templates")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveTemplate(EmailManagementTemplate model)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            if (!ModelState.IsValid)
                {
                PopulateEvents();
                ViewBag.Templates = _db.GetManagedEmailTemplates();
                return View("Templates", model);
                }
            _db.SaveManagedEmailTemplate(model, CurrentUser());
            TempData["EmailManagementMessage"] = "Email template saved.";
            return RedirectToAction(nameof(Templates), new { eventId = model.EventId });
            }

        [HttpPost("Templates/Activate")]
        [ValidateAntiForgeryToken]
        public IActionResult ActivateTemplate(long eventId, long templateId)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            _db.SetManagedEmailActiveTemplate(eventId, templateId, CurrentUser());
            return RedirectToAction(nameof(Templates), new { eventId });
            }

        [HttpGet("Rules")]
        public IActionResult Rules(long? eventId = null)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            PopulateEvents();
            ViewBag.Rules = _db.GetManagedEmailRules();
            return View(eventId.HasValue
                ? _db.GetManagedEmailRules(eventId).SingleOrDefault() ?? new EmailManagementRule { EventId = eventId.Value }
                : new EmailManagementRule());
            }

        [HttpPost("Rules")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveRule(EmailManagementRule model)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            if (!ModelState.IsValid)
                {
                PopulateEvents();
                ViewBag.Rules = _db.GetManagedEmailRules();
                return View("Rules", model);
                }
            _db.SaveManagedEmailRule(model, CurrentUser());
            TempData["EmailManagementMessage"] = "Recipient rule saved. It remains configuration-only.";
            return RedirectToAction(nameof(Rules), new { eventId = model.EventId });
            }

        [HttpGet("Placeholders")]
        public IActionResult Placeholders(long? eventId = null)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            PopulateEvents();
            ViewBag.Placeholders = _db.GetManagedEmailPlaceholders();
            return View(new EmailManagementPlaceholder { EventId = eventId ?? 0 });
            }

        [HttpPost("Placeholders")]
        [ValidateAntiForgeryToken]
        public IActionResult SavePlaceholder(EmailManagementPlaceholder model)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            if (!ModelState.IsValid)
                {
                PopulateEvents();
                ViewBag.Placeholders = _db.GetManagedEmailPlaceholders();
                return View("Placeholders", model);
                }
            _db.SaveManagedEmailPlaceholder(model, CurrentUser());
            TempData["EmailManagementMessage"] = "Placeholder saved.";
            return RedirectToAction(nameof(Placeholders), new { eventId = model.EventId });
            }

        [HttpGet("Preview")]
        public IActionResult Preview(long? eventId = null, long? templateId = null)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            PopulateEvents();
            ViewBag.Templates = _db.GetManagedEmailTemplates(eventId);
            if (!eventId.HasValue) return View(new EmailManagementPreview());
            try
                {
                return View(_service.BuildPreview(eventId.Value, templateId));
                }
            catch (Exception ex)
                {
                ViewBag.Error = ex.Message;
                return View(new EmailManagementPreview { EventId = eventId.Value });
                }
            }

        [HttpGet("Test")]
        public IActionResult Test(long? eventId = null)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            PopulateEvents();
            return View(new EmailManagementTestRequest { EventId = eventId ?? 0 });
            }

        [HttpPost("Test")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Test(EmailManagementTestRequest model)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            PopulateEvents();
            if (!ModelState.IsValid) return View(model);
            ViewBag.SendResult = await _service.SendTestAsync(model, CurrentUser(), nameof(EmailManagementController) + "." + nameof(Test));
            return View(model);
            }

        [HttpGet("Logs")]
        public IActionResult Logs([FromQuery] EmailManagementLogFilter filter)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            ViewBag.Filter = filter ?? new EmailManagementLogFilter();
            ViewBag.Events = _db.GetManagedEmailEvents();
            ViewBag.Statuses = EmailManagementStatuses.All;
            return View(_db.GetManagedEmailLogs(filter));
            }

        [HttpGet("Logs/{id:long}")]
        public IActionResult LogDetail(long id)
            {
            var denied = EnsureAdministrator();
            if (denied != null) return denied;
            var log = _db.GetManagedEmailLogs(logId: id).SingleOrDefault();
            return log == null ? NotFound() : View(log);
            }

        private IActionResult EnsureAdministrator()
            {
            if (!_session.TryGetUser(out var user))
                {
                return RedirectToAction("Index", "Login");
                }
            return user.UserRoleID == 1 ? null : Forbid();
            }

        private string CurrentUser() =>
            _session.TryGetUser(out var user) ? user.PPNumber ?? user.Name ?? "unknown" : "unknown";

        private void PopulateEvents()
            {
            ViewBag.EventOptions = _db.GetManagedEmailEvents()
                .Select(item => new SelectListItem(item.DisplayName + " (" + item.EventKey + ")", item.EventId.ToString()))
                .ToList();
            }
        }
    }
