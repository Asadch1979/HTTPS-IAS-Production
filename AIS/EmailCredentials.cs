using System;
using AIS.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AIS.Controllers;
using AIS.Security.Cryptography;

namespace AIS
    {
    public class EmailCredentails
        {
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public EmailCredentails(Microsoft.Extensions.Configuration.IConfiguration configuration, IServiceProvider serviceProvider = null)
            {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serviceProvider = serviceProvider;
            }

        public EmailCredentailsModel GetEmailCredentails()
            {
            var databaseCredentials = GetDatabaseCredentials();
            if (databaseCredentials != null)
                {
                return databaseCredentials;
                }

            var email = Environment.GetEnvironmentVariable("Email__From") ?? _configuration["Email:From"];
            var password = Environment.GetEnvironmentVariable("Email__Password");
            var host = Environment.GetEnvironmentVariable("Email__Host") ?? _configuration["Email:Host"];
            var username = Environment.GetEnvironmentVariable("Email__Username") ?? _configuration["Email:Username"] ?? email;
            var portText = Environment.GetEnvironmentVariable("Email__Port");
            var port = int.TryParse(portText, out var environmentPort)
                ? environmentPort
                : _configuration.GetValue<int?>("Email:Port") ?? 587;
            var sslText = Environment.GetEnvironmentVariable("Email__EnableSsl");
            var enableSsl = bool.TryParse(sslText, out var environmentSsl)
                ? environmentSsl
                : _configuration.GetValue<bool?>("Email:EnableSsl") ?? true;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(host))
                {
                return new EmailCredentailsModel
                    {
                    EMAIL = string.Empty,
                    PASSWORD = string.Empty,
                    Host = string.Empty,
                    Port = port,
                    IsConfigured = false,
                    StatusMessage = "disabled"
                    };
                }

            EmailCredentailsModel em = new EmailCredentailsModel();
            em.EMAIL = email;
            em.PASSWORD = password;
            em.Host = host;
            em.Username = username;
            em.Port = port;
            em.EnableSsl = enableSsl;
            em.IsConfigured = true;
            em.StatusMessage = "enabled";
            return em;

            }

        private EmailCredentailsModel GetDatabaseCredentials()
            {
            try
                {
                var db = _serviceProvider?.GetService<DBConnection>();
                var protector = _serviceProvider?.GetService<EmailPasswordProtector>();
                if (db == null || protector == null || !protector.IsConfigured)
                    {
                    return null;
                    }

                var record = db.GetActiveEmailConfiguration();
                if (record == null)
                    {
                    return null;
                    }

                var password = protector.Decrypt(record.EncryptedPassword);
                if (string.IsNullOrWhiteSpace(record.SmtpHost)
                    || string.IsNullOrWhiteSpace(record.FromEmail)
                    || string.IsNullOrWhiteSpace(password))
                    {
                    return null;
                    }

                return new EmailCredentailsModel
                    {
                    EMAIL = record.FromEmail,
                    Username = string.IsNullOrWhiteSpace(record.SmtpUsername) ? record.FromEmail : record.SmtpUsername,
                    PASSWORD = password,
                    Host = record.SmtpHost,
                    Port = record.SmtpPort,
                    EnableSsl = record.EnableSsl,
                    IsConfigured = true,
                    StatusMessage = "enabled"
                    };
                }
            catch
                {
                // Database configuration is optional; environment fallback is evaluated by the caller.
                return null;
                }
            }

        }
    }
