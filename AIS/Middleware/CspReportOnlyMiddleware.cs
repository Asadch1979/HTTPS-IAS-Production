using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Middleware
{
    public class CspReportOnlyMiddleware
    {
        private const string ReportOnlyHeader = "Content-Security-Policy-Report-Only";
        private const string EnforcedHeader = "Content-Security-Policy";

        private readonly RequestDelegate _next;
        private readonly bool _enabled;
        private readonly bool _reportOnly;
        private readonly string _policy;
        private readonly string _reportUri;

        public CspReportOnlyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _enabled = configuration.GetValue("SecurityHeaders:CspEnabled", true);
            _reportOnly = configuration.GetValue("SecurityHeaders:CspReportOnly", true);
            _reportUri = configuration["SecurityHeaders:CspReportUriPath"] ?? "/api/security/csp-report";

            // Library discovery summary (Views + wwwroot scan):
            // - Local scripts/styles/fonts/images are served from ~/lib, ~/js, ~/css, ~/Images (covered by 'self').
            // - External domains found in Razor views:
            //   script-src/connect-src: https://cdnjs.cloudflare.com, https://cdn.jsdelivr.net, https://html2canvas.hertzen.com
            //   style-src/font-src: https://cdn.jsdelivr.net
            // No other CDN sources were found in application Razor files.
            var scriptSources = ReadList(configuration, "SecurityHeaders:CspExtraScriptSrc");
            var styleSources = ReadList(configuration, "SecurityHeaders:CspExtraStyleSrc");
            var fontSources = ReadList(configuration, "SecurityHeaders:CspExtraFontSrc");
            var imageSources = ReadList(configuration, "SecurityHeaders:CspExtraImgSrc");
            var connectSources = ReadList(configuration, "SecurityHeaders:CspExtraConnectSrc");
            var frameSources = ReadList(configuration, "SecurityHeaders:CspExtraFrameSrc");

            var directives = new List<string>
            {
                "default-src 'self'",
                "base-uri 'self'",
                "object-src 'none'",
                "form-action 'self'",
                "frame-ancestors 'none'",
                BuildDirective("script-src", scriptSources),
                BuildDirective("style-src", styleSources, "'unsafe-inline'"),
                BuildDirective("font-src", fontSources, "data:"),
                BuildDirective("img-src", imageSources, "data:", "blob:"),
                BuildDirective("connect-src", connectSources),
                BuildDirective("frame-src", frameSources),
                "upgrade-insecure-requests",
                $"report-uri {_reportUri}"
            };

            _policy = string.Join("; ", directives.Where(d => !string.IsNullOrWhiteSpace(d))) + ";";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_enabled)
            {
                await _next(context);
                return;
            }

            context.Response.OnStarting(() =>
            {
                if (!IsHtmlResponse(context.Response.ContentType))
                {
                    return Task.CompletedTask;
                }

                var headerName = _reportOnly ? ReportOnlyHeader : EnforcedHeader;
                context.Response.Headers[headerName] = _policy;
                return Task.CompletedTask;
            });

            await _next(context);
        }

        private static bool IsHtmlResponse(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }

            return contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase)
                || contentType.Contains("application/xhtml+xml", StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> ReadList(IConfiguration configuration, string sectionPath)
        {
            var values = configuration.GetSection(sectionPath).Get<string[]>()
                ?? Array.Empty<string>();

            return values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string BuildDirective(string directive, IEnumerable<string> extras, params string[] defaults)
        {
            var values = new List<string> { "'self'" };
            if (defaults != null)
            {
                values.AddRange(defaults.Where(v => !string.IsNullOrWhiteSpace(v)));
            }

            if (extras != null)
            {
                values.AddRange(extras.Where(v => !string.IsNullOrWhiteSpace(v)));
            }

            return $"{directive} {string.Join(" ", values.Distinct(StringComparer.OrdinalIgnoreCase))}";
        }
    }
}
