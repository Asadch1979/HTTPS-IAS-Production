using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;

namespace AIS.Services
    {
    public interface IClientIpResolver
        {
        string GetClientIp(HttpContext context = null);
        string GetServerIp();
        }

    public class ClientIpResolver : IClientIpResolver
        {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly LocalIPAddress _localIPAddress = new LocalIPAddress();

        public ClientIpResolver(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
            {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            }

        public string GetClientIp(HttpContext context = null)
            {
            context ??= _httpContextAccessor?.HttpContext;
            var ip = context?.Connection?.RemoteIpAddress;
            if (ip == null)
                {
                return "unknown";
                }

            ip = NormalizeIpAddress(ip);

            if (IsTrustedProxy(ip))
                {
                var forwardedIp = ResolveForwardedClientIp(context);
                if (!string.IsNullOrWhiteSpace(forwardedIp))
                    {
                    return forwardedIp;
                    }
                }

            return FormatIpAddress(ip);
            }

        public string GetServerIp()
            {
            return _localIPAddress.GetLocalIpAddress();
            }

        private string ResolveForwardedClientIp(HttpContext context)
            {
            var request = context?.Request;
            if (request == null)
                {
                return null;
                }

            var xForwardedFor = request.Headers["X-Forwarded-For"].ToString();
            foreach (var forwardedValue in xForwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                var parsed = ParseForwardedIp(forwardedValue);
                if (parsed != null)
                    {
                    return FormatIpAddress(parsed);
                    }
                }

            var xRealIp = ParseForwardedIp(request.Headers["X-Real-IP"].ToString());
            if (xRealIp != null)
                {
                return FormatIpAddress(xRealIp);
                }

            var forwarded = request.Headers["Forwarded"].ToString();
            foreach (var forwardedElement in forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                foreach (var part in forwardedElement.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                    var pair = part.Split('=', 2, StringSplitOptions.TrimEntries);
                    if (pair.Length == 2 && string.Equals(pair[0], "for", StringComparison.OrdinalIgnoreCase))
                        {
                        var parsed = ParseForwardedIp(pair[1]);
                        if (parsed != null)
                            {
                            return FormatIpAddress(parsed);
                            }
                        }
                    }
                }

            return null;
            }

        private bool IsTrustedProxy(IPAddress ip)
            {
            foreach (var proxy in _configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>())
                {
                if (IPAddress.TryParse(proxy, out var knownProxy) && NormalizeIpAddress(knownProxy).Equals(ip))
                    {
                    return true;
                    }
                }

            foreach (var network in _configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? Array.Empty<string>())
                {
                if (IsInConfiguredNetwork(ip, network))
                    {
                    return true;
                    }
                }

            return false;
            }

        private static bool IsInConfiguredNetwork(IPAddress ip, string network)
            {
            var parts = (network ?? string.Empty).Split('/', 2);
            if (parts.Length != 2
                || !IPAddress.TryParse(parts[0], out var prefix)
                || !int.TryParse(parts[1], out var prefixLength))
                {
                return false;
                }

            ip = NormalizeIpAddress(ip);
            prefix = NormalizeIpAddress(prefix);
            if (ip.AddressFamily != prefix.AddressFamily)
                {
                return false;
                }

            var ipBytes = ip.GetAddressBytes();
            var prefixBytes = prefix.GetAddressBytes();
            if (prefixLength < 0 || prefixLength > ipBytes.Length * 8)
                {
                return false;
                }

            var fullBytes = prefixLength / 8;
            var remainingBits = prefixLength % 8;

            for (var i = 0; i < fullBytes; i++)
                {
                if (ipBytes[i] != prefixBytes[i])
                    {
                    return false;
                    }
                }

            if (remainingBits == 0)
                {
                return true;
                }

            var mask = (byte)(0xFF << (8 - remainingBits));
            return (ipBytes[fullBytes] & mask) == (prefixBytes[fullBytes] & mask);
            }

        private static IPAddress ParseForwardedIp(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return null;
                }

            var candidate = value.Trim().Trim('"');
            if (candidate.StartsWith("[", StringComparison.Ordinal) && candidate.Contains(']'))
                {
                candidate = candidate[1..candidate.IndexOf(']')];
                }
            else if (candidate.CountCharacter(':') == 1 && candidate.Contains(':'))
                {
                candidate = candidate[..candidate.LastIndexOf(':')];
                }

            return IPAddress.TryParse(candidate, out var parsed) ? NormalizeIpAddress(parsed) : null;
            }

        private static IPAddress NormalizeIpAddress(IPAddress ip)
            {
            return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
            }

        private static string FormatIpAddress(IPAddress ip)
            {
            return ip.Equals(IPAddress.IPv6Loopback) ? IPAddress.Loopback.ToString() : ip.ToString();
            }
        }

    internal static class ClientIpResolverStringExtensions
        {
        public static int CountCharacter(this string value, char character)
            {
            var count = 0;
            foreach (var current in value)
                {
                if (current == character)
                    {
                    count++;
                    }
                }

            return count;
            }
        }
    }
