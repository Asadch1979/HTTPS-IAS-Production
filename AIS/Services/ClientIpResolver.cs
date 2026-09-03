using Microsoft.AspNetCore.Http;
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
        private readonly LocalIPAddress _localIPAddress = new LocalIPAddress();

        public ClientIpResolver(IHttpContextAccessor httpContextAccessor)
            {
            _httpContextAccessor = httpContextAccessor;
            }

        public string GetClientIp(HttpContext context = null)
            {
            context ??= _httpContextAccessor?.HttpContext;
            var ip = context?.Connection?.RemoteIpAddress;
            if (ip == null)
                {
                return "unknown";
                }

            if (ip.IsIPv4MappedToIPv6)
                {
                ip = ip.MapToIPv4();
                }

            return ip.Equals(IPAddress.IPv6Loopback) ? IPAddress.Loopback.ToString() : ip.ToString();
            }

        public string GetServerIp()
            {
            return _localIPAddress.GetLocalIpAddress();
            }
        }
    }
