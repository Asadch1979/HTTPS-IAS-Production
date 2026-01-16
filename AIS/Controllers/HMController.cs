using AIS.Exceptions;
using AIS.Models;
using AIS.Models.HD;
using AIS.Models.HM;
using AIS.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AIS.Services;


namespace AIS.Controllers
    {

    public class HMController : Controller
        {
        private readonly ILogger<HMController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        public HMController(ILogger<HMController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            }
        public IActionResult dashboard()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ActiveInactive"] = dBConnection.GetActiveInactiveChartData();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult old_paras_monitoring()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ZonesList"] = dBConnection.GetZonesoldparamointoring();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult old_paras_monitoring_ppno()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ZonesList"] = dBConnection.GetZones();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult SbpObservationRegister()
            {
            PrepareObservationRegisterContext();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                {
                return View("~/Views/HM/SbpObservationRegister.cshtml", CreateRegisterViewModel(false));
                }

            var observationTypes = FetchObservationTypes();
            var model = CreateRegisterViewModel(false, null, null, null, observationTypes);
            return View(model);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SbpObservationRegister([FromForm] SbpObservationPasswordPostModel passwordModel)
            {
            PrepareObservationRegisterContext();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                {
                var deniedModel = CreateRegisterViewModel(false, null, null, null, FetchObservationTypes());
                return View("~/Views/HM/SbpObservationRegister.cshtml", deniedModel);
                }

            var observationTypes = FetchObservationTypes();

            if (passwordModel == null || string.IsNullOrWhiteSpace(passwordModel.Password))
                {
                var model = CreateRegisterViewModel(false, null, "Password is required.", null, observationTypes);
                return View(model);
                }

            try
                {
                var validation = dBConnection.ValidateSbpAccessPassword(passwordModel.Password);
                if (!validation.Success)
                    {
                    var model = CreateRegisterViewModel(false, null, validation.Message ?? "Invalid password.", null, observationTypes);
                    return View(model);
                    }

                try
                    {
                    sessionHandler.GrantSbpAccess();
                    }
                catch (Exception ex)
                    {
                    _logger.LogError(ex, "Unable to persist SBP authorization state.");
                    var model = CreateRegisterViewModel(false, null, "Session could not be updated. Please try again.", null, observationTypes);
                    return View(model);
                    }

                var successMessage = string.IsNullOrWhiteSpace(validation.Message)
                    ? "Access granted."
                    : validation.Message;
                var viewModel = CreateRegisterViewModel(true, Enumerable.Empty<SBPObservationRegisterItem>(), null, successMessage, observationTypes);
                return View(viewModel);
                }
            catch (DatabaseUnavailableException ex)
                {
                _logger.LogError(ex, "Database connection is unavailable while loading SBP observation register.");
                var model = CreateRegisterViewModel(false, null, "The register is temporarily unavailable. Please try again later.", null, observationTypes);
                return View(model);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unexpected error while loading SBP observation register.");
                var model = CreateRegisterViewModel(false, null, "Unable to load the register. Please try again.", null, observationTypes);
                return View(model);
                }
            }

        private void PrepareObservationRegisterContext()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            }

        private SbpObservationRegisterViewModel CreateRegisterViewModel(
            bool isUnlocked,
            IEnumerable<SBPObservationRegisterItem> observations = null,
            string passwordError = null,
            string statusMessage = null,
            IEnumerable<SbpObservationTypeOption> observationTypes = null,
            int? selectedObservationTypeId = null)
            {
            var items = (observations ?? Enumerable.Empty<SBPObservationRegisterItem>()).ToList();
            var types = (observationTypes ?? Enumerable.Empty<SbpObservationTypeOption>())
                .Where(type => type != null && type.ObservationTypeId > 0 && type.IsActive && !string.IsNullOrWhiteSpace(type.ObservationTypeName))
                .GroupBy(type => type.ObservationTypeId)
                .Select(group => group
                    .OrderBy(option => option.SortOrder ?? int.MaxValue)
                    .ThenBy(option => option.ObservationTypeName, StringComparer.OrdinalIgnoreCase)
                    .First())
                .OrderBy(option => option.SortOrder ?? int.MaxValue)
                .ThenBy(option => option.ObservationTypeName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var model = new SbpObservationRegisterViewModel
                {
                IsUnlocked = isUnlocked,
                PasswordError = passwordError,
                ObservationTypes = types,
                Observations = items,
                SelectedObservationTypeId = selectedObservationTypeId
                };

            if (!string.IsNullOrWhiteSpace(statusMessage))
                {
                model.StatusMessage = statusMessage;
                }

            return model;
            }

        private IReadOnlyList<SbpObservationTypeOption> FetchObservationTypes()
            {
            try
                {
                return dBConnection.GetSbpObservationTypes();
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to load SBP observation types.");
                return Array.Empty<SbpObservationTypeOption>();
                }
            }

        public IActionResult SbpObservationHistory(int paraID)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return View("~/Views/HM/SbpObservationHistory.cshtml");
                    }
                else
                    return View();
                }
            }

        public IActionResult ManageSbpPassword(int paraID)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return View("~/Views/HM/ManageSbpPassword.cshtml");
                    }
                else
                    return View();
                }
            }

        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        [HttpPost]
        public IActionResult SetPageId(string page_path)
            {
            var pageId = 0;

            if (!string.IsNullOrWhiteSpace(page_path))
                {
                var resolver = HttpContext?.RequestServices.GetService<IPageIdResolver>();
                if (resolver != null)
                    {
                    pageId = resolver.ResolvePageId(page_path);
                    }
                }

            sessionHandler.SetPageId(pageId);
            return Ok(new { pageId });
            }
        }

    }
