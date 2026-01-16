using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading;

namespace AIS.Controllers
    {
    public class EmailController : Controller
        {
        private readonly IWebHostEnvironment _env;
        private static Timer? _reminderTimer;
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
        public IActionResult Send(string toEmail, string ccEmail, string subject, string body, int reminderMinutes = 0)
            {
            _emailConfig.ConfigEmail(toEmail, ccEmail, subject, body);

            _reminderTimer?.Dispose();
            if (reminderMinutes > 0)
                {
                _reminderTimer = new Timer(_ =>
                {
                    _emailConfig.ConfigEmail(toEmail, ccEmail, subject, body);
                }, null, reminderMinutes * 60000, reminderMinutes * 60000);
                }

            ViewBag.Message = "Email Sent";
            ViewBag.TemplateBody = body;
            return View();
            }
        }
    }
