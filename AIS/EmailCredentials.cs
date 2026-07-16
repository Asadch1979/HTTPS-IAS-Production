using System;
using AIS.Models;
using Microsoft.Extensions.Configuration;

namespace AIS
    {
    public class EmailCredentails
        {
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public EmailCredentails(Microsoft.Extensions.Configuration.IConfiguration configuration)
            {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            }

        public EmailCredentailsModel GetEmailCredentails()
            {
            var email = _configuration["Email:From"];
            var password = _configuration["Email:Password"];
            var host = _configuration["Email:Host"];
            var port = _configuration.GetValue<int?>("Email:Port") ?? 587;

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
            em.Port = port;
            em.IsConfigured = true;
            em.StatusMessage = "enabled";
            return em;

            }

        }
    }
