using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AIS.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var isLoginPath = context.Request.Path.StartsWithSegments("/Login", StringComparison.OrdinalIgnoreCase);
            string nonce = null;

            if (isLoginPath)
            {
                nonce = GenerateNonce();
                context.Items["CSPNonce"] = nonce;
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["Referrer-Policy"] = "same-origin";
                context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=(), usb=()";

                if (isLoginPath && !string.IsNullOrEmpty(nonce))
                {
                    var cspBuilder = new StringBuilder();
                    cspBuilder.Append("default-src 'self'; ");
                    cspBuilder.Append("base-uri 'self'; ");
                    cspBuilder.Append("form-action 'self'; ");
                    cspBuilder.Append("object-src 'none'; ");
                    cspBuilder.Append($"script-src 'self' 'nonce-{nonce}'; ");
                    cspBuilder.Append($"style-src 'self' 'nonce-{nonce}'; ");
                    cspBuilder.Append("img-src 'self' data:; ");
                    cspBuilder.Append("font-src 'self' data:; ");
                    cspBuilder.Append("connect-src 'self'; ");
                    cspBuilder.Append("frame-ancestors 'self';");

                    context.Response.Headers["Content-Security-Policy"] = cspBuilder.ToString();
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }

        private static string GenerateNonce()
        {
            Span<byte> bytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
