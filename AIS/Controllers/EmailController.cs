using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using AIS.Models.Notifications;

namespace AIS.Controllers
    {
    public class EmailController : Controller
        {
        private readonly IWebHostEnvironment _env;
        private readonly EmailConfiguration _emailConfig;

        public EmailController(IWebHostEnvironment env, Microsoft.Extensions.Configuration.IConfiguration configuration, IServiceProvider serviceProvider)
            {
            _env = env;
            _emailConfig = new EmailConfiguration(configuration, serviceProvider);
            }

        public IActionResult Edit()
            {
            string templatePath = Path.Combine(_env.WebRootPath, "Templates", "GeneralNotification.html");
            ViewBag.TemplateBody = System.IO.File.Exists(templatePath) ? System.IO.File.ReadAllText(templatePath) : string.Empty;
            return View();
            }

        [HttpPost]
        public IActionResult Edit(string body)
            {
            string templatePath = Path.Combine(_env.WebRootPath, "Templates", "GeneralNotification.html");
            Directory.CreateDirectory(Path.GetDirectoryName(templatePath)!);
            System.IO.File.WriteAllText(templatePath, body ?? string.Empty);
            ViewBag.TemplateBody = body;
            ViewBag.Message = "Template updated successfully.";
            return View();
            }

        public IActionResult Send()
            {
            string templatePath = Path.Combine(_env.WebRootPath, "Templates", "GeneralNotification.html");
            ViewBag.TemplateBody = System.IO.File.Exists(templatePath) ? System.IO.File.ReadAllText(templatePath) : string.Empty;
            return View();
            }

        [HttpPost]
        public IActionResult Send(string toEmail, string ccEmail, string subject, string body)
            {
            var result = _emailConfig.Send(new EmailMessageRequest
                {
                Module = "Administration",
                TriggerPoint = "ManualEmailScreen",
                ReferenceId = HttpContext.TraceIdentifier,
                ToRecipients = new[] { toEmail },
                CcRecipients = new[] { ccEmail },
                Subject = subject,
                Body = body,
                IsBodyHtml = true
                });

            ViewBag.Message = result.IsSuccess
                ? "Email sent successfully."
                : $"Email could not be sent ({result.Status}). {result.ErrorMessage}";
            ViewBag.Success = result.IsSuccess;
            ViewBag.TemplateBody = body;
            return View();
            }
        }
    }
