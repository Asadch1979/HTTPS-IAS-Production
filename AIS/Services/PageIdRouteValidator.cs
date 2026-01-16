using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIS.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIS.Services
    {
    public class PageIdRouteValidator : IHostedService
        {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPageIdResolver _pageIdResolver;
        private readonly ILogger<PageIdRouteValidator> _logger;
        private readonly IHostEnvironment _environment;

        public PageIdRouteValidator(
            IServiceScopeFactory scopeFactory,
            IPageIdResolver pageIdResolver,
            IHostEnvironment environment,
            ILogger<PageIdRouteValidator> logger)
            {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _pageIdResolver = pageIdResolver ?? throw new ArgumentNullException(nameof(pageIdResolver));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        public Task StartAsync(CancellationToken cancellationToken)
            {
            if (!_environment.IsDevelopment())
                {
                return Task.CompletedTask;
                }

            try
                {
                using var scope = _scopeFactory.CreateScope();
                var menuReader = scope.ServiceProvider.GetRequiredService<IMenuPagesReader>();
                var menuPages = menuReader.GetActiveMenuPages() ?? new List<MenuPagesModel>();

                foreach (var menuPage in menuPages)
                    {
                    var normalizedPath = PageIdPathHelper.NormalizePath(menuPage.Page_Path);
                    if (string.IsNullOrWhiteSpace(normalizedPath))
                        {
                        continue;
                        }

                    if (_pageIdResolver.TryResolvePageId(normalizedPath, out _))
                        {
                        continue;
                        }

                    _logger.LogWarning(
                        "Missing PAGE_ID mapping in Page ID.xlsx for DB menu path: {Path}, PAGE_ID: {PageId}",
                        normalizedPath,
                        menuPage.PageId);
                    }
                }
            catch (Exception ex)
                {
                if (_environment.IsDevelopment())
                    {
                    _logger.LogWarning(ex, "Failed to validate PAGE_ID mappings on startup.");
                    }
                }

            return Task.CompletedTask;
            }

        public Task StopAsync(CancellationToken cancellationToken)
            {
            return Task.CompletedTask;
            }
        }
    }
