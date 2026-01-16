using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIS.Services
    {
    public class LoginViewResolver
        {
        private const string DevDataSourceMarker = "10.1.100.112:1521/qadb18c.ztbl.com.pk";


        public LoginViewResolver(IConfiguration configuration, ILogger<LoginViewResolver> logger)
            {
            var dataSource = configuration.GetConnectionString("DBDataSource");
            logger.LogInformation("LoginViewResolver detected DBDataSource: {DataSource}",dataSource);

            if (string.IsNullOrWhiteSpace(dataSource))
                {
                throw new InvalidOperationException("ConnectionStrings:DBDataSource must be configured to resolve the login view.");
                }

            var trimmedDataSource = dataSource.Trim();
            IsDevLoginMode = trimmedDataSource.IndexOf(DevDataSourceMarker, StringComparison.OrdinalIgnoreCase) >= 0;
            ResolvedViewName = IsDevLoginMode ? "index_dev" : "index";

            logger.LogInformation(
                "Login view resolved to {ViewName} based on DBDataSource configuration. Mode={LoginMode}.",
                ResolvedViewName,
                IsDevLoginMode ? "DEV" : "STANDARD");
            }

        public bool IsDevLoginMode { get; }

        public string ResolvedViewName { get; }
        }
    }
