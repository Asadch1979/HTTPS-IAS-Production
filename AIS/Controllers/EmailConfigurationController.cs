using AIS.Models;
using AIS.Models.Notifications;
using AIS.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace AIS.Controllers
    {
    [Authorize]
    public class EmailConfigurationController : Controller
        {
        private readonly DBConnection _db;
        private readonly SessionHandler _sessionHandler;
        private readonly EmailPasswordProtector _protector;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailConfigurationController> _logger;

        public EmailConfigurationController(
            DBConnection db,
            SessionHandler sessionHandler,
            EmailPasswordProtector protector,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<EmailConfigurationController> logger)
            {
            _db = db;
            _sessionHandler = sessionHandler;
            _protector = protector;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            }

        [HttpGet]
        public IActionResult Index(long? id = null)
            {
            if (!TryGetAdministrator(out _))
                {
                return Forbid();
                }

            var record = id.HasValue ? _db.GetEmailConfiguration(id.Value) : _db.GetActiveEmailConfiguration();
            var model = record == null ? new EmailConfigurationAdminModel() : Map(record);
            ApplyMessages();
            ViewBag.EncryptionKeyConfigured = _protector.IsConfigured;
            return View(model);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(EmailConfigurationAdminModel model)
            {
            if (!TryGetAdministrator(out var changedBy))
                {
                return Forbid();
                }

            if (!string.IsNullOrWhiteSpace(model.NewPassword) && !_protector.IsConfigured)
                {
                ModelState.AddModelError(nameof(model.NewPassword), "The IIS email encryption key is not configured.");
                }

            if (!ModelState.IsValid)
                {
                model.NewPassword = string.Empty;
                ViewBag.EncryptionKeyConfigured = _protector.IsConfigured;
                return View("Index", model);
                }

            try
                {
                var encryptedPassword = string.IsNullOrWhiteSpace(model.NewPassword)
                    ? Array.Empty<byte>()
                    : _protector.Encrypt(model.NewPassword);
                var id = _db.SaveEmailConfiguration(new EmailConfigurationRecord
                    {
                    ConfigId = model.ConfigId,
                    SmtpHost = model.SmtpHost?.Trim(),
                    SmtpPort = model.SmtpPort,
                    FromEmail = model.FromEmail?.Trim(),
                    SmtpUsername = model.SmtpUsername?.Trim(),
                    EncryptedPassword = encryptedPassword,
                    EnableSsl = model.EnableSsl,
                    IsActive = model.IsActive
                    }, changedBy, ClientIp());

                TempData["EmailConfigurationMessage"] = "Email configuration saved successfully.";
                return RedirectToAction(nameof(Index), new { id });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Email configuration save failed for ConfigId={ConfigId}.", model.ConfigId);
                model.NewPassword = string.Empty;
                ModelState.AddModelError(string.Empty, "The email configuration could not be saved.");
                ViewBag.EncryptionKeyConfigured = _protector.IsConfigured;
                return View("Index", model);
                }
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendTestEmail(string testRecipient)
            {
            if (!TryGetAdministrator(out _))
                {
                return Forbid();
                }

            var email = new EmailConfiguration(_configuration, _serviceProvider);
            var result = email.Send(new EmailMessageRequest
                {
                Module = "Administration",
                TriggerPoint = "EmailConfigurationTest",
                ReferenceId = HttpContext.TraceIdentifier,
                ToRecipients = new[] { testRecipient },
                Subject = "IAS SMTP Configuration Test",
                Body = "This is a test message from the IAS Email Configuration administration screen."
                });

            TempData["EmailConfigurationMessage"] = result.IsSuccess
                ? "Test email sent successfully."
                : $"Test email failed ({result.Status}). {result.ErrorMessage}";
            TempData["EmailConfigurationSuccess"] = result.IsSuccess;
            return RedirectToAction(nameof(Index));
            }

        private bool TryGetAdministrator(out string ppNumber)
            {
            ppNumber = string.Empty;
            if (!_sessionHandler.TryGetUser(out var user) || user == null || user.UserRoleID != 1)
                {
                return false;
                }

            ppNumber = user.PPNumber ?? string.Empty;
            return true;
            }

        private string ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        private void ApplyMessages()
            {
            ViewBag.Message = TempData["EmailConfigurationMessage"];
            ViewBag.Success = TempData["EmailConfigurationSuccess"] as bool?;
            }

        private static EmailConfigurationAdminModel Map(EmailConfigurationRecord record) => new EmailConfigurationAdminModel
            {
            ConfigId = record.ConfigId,
            SmtpHost = record.SmtpHost,
            SmtpPort = record.SmtpPort,
            FromEmail = record.FromEmail,
            SmtpUsername = record.SmtpUsername,
            NewPassword = string.Empty,
            EnableSsl = record.EnableSsl,
            IsActive = record.IsActive,
            HasSavedPassword = record.EncryptedPassword?.Length > 0
            };
        }
    }
