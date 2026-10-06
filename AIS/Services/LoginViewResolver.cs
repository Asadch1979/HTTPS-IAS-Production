using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIS.Services
    {
    public class LoginViewResolver
        {
        private const string DevDataSource = "10.1.100.112:1521/qadb18c.ztbl.com.pk";

        public LoginViewResolver(
            IConfiguration configuration,
            ILogger<LoginViewResolver> logger)
            {
            var dataSource =
                configuration.GetConnectionString("DBDataSource");
            var dbUser = configuration.GetConnectionString("DBUserName");

            if (string.IsNullOrWhiteSpace(dbUser))
                {
                throw new InvalidOperationException(
                    "ConnectionStrings:DBUserName must be configured to resolve the login view.");
                }

            if (string.IsNullOrWhiteSpace(dataSource))
                {
                throw new InvalidOperationException(
                    "ConnectionStrings:DBDataSource must be configured to resolve the login view.");
                }

            var trimmedDataSource = dataSource.Trim();

            IsDevLoginMode =
                string.Equals(trimmedDataSource, DevDataSource,
                    StringComparison.OrdinalIgnoreCase);

            ResolvedViewName =
                IsDevLoginMode ? "index_dev" : "index";

            logger.LogInformation(
                "DBUserName={DBUserName}; LoginView={ViewName}; Mode={LoginMode}",
                dbUser.Trim(),
                ResolvedViewName,
                IsDevLoginMode ? "DEV_LOGIN" : "STANDARD_LOGIN");
            }

        public bool IsDevLoginMode { get; }

        public string ResolvedViewName { get; }
        }
    }
