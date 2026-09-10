using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIS.Services
    {
    public class LoginViewResolver
        {
        public LoginViewResolver(IConfiguration configuration, ILogger<LoginViewResolver> logger)
            {
            IsDevLoginMode = configuration.GetValue<bool>("Security:UseDevelopmentLoginView");
            ResolvedViewName = IsDevLoginMode ? "index_dev" : "index";

            logger.LogInformation(
                "Login view resolved to {ViewName}. Mode={LoginMode}.",
                ResolvedViewName,
                IsDevLoginMode ? "DEV" : "STANDARD");
            }

        public bool IsDevLoginMode { get; }

        public string ResolvedViewName { get; }
        }
    }
