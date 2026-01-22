using System;
using System.Text;
using AIS.Models;
using AIS.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace AIS.Services
    {
    public class PasswordChangeTokenService
        {
        private const string CookieName = "IAS.PwdChange";
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(10);
        private readonly SecurityTokenService _tokenService;

        public PasswordChangeTokenService(SecurityTokenService tokenService)
            {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            }

        public void IssueToken(HttpRequest request, HttpResponse response, UserModel user)
            {
            if (response == null)
                {
                throw new ArgumentNullException(nameof(response));
                }

            if (user == null)
                {
                throw new ArgumentNullException(nameof(user));
                }

            var expiresAt = DateTimeOffset.UtcNow.Add(TokenLifetime);
            var nonce = Guid.NewGuid().ToString("N");
            var payload = string.Join("|",
                user.ID,
                user.PPNumber ?? string.Empty,
                user.UserEntityID ?? 0,
                user.UserRoleID,
                expiresAt.ToUnixTimeSeconds(),
                nonce);

            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var encodedPayload = WebEncoders.Base64UrlEncode(payloadBytes);
            var signature = _tokenService.ComputeHmacHash(encodedPayload);
            var value = $"{encodedPayload}.{signature}";

            response.Cookies.Append(CookieName, value, BuildCookieOptions(request, expiresAt));
            }

        public bool TryValidate(HttpRequest request, out PasswordChangeToken token)
            {
            token = null;
            if (request == null)
                {
                return false;
                }

            if (!request.Cookies.TryGetValue(CookieName, out var rawValue))
                {
                return false;
                }

            var parts = rawValue.Split('.', 2);
            if (parts.Length != 2)
                {
                return false;
                }

            var encodedPayload = parts[0];
            var signature = parts[1];
            var expectedSignature = _tokenService.ComputeHmacHash(encodedPayload);
            if (!string.Equals(signature, expectedSignature, StringComparison.OrdinalIgnoreCase))
                {
                return false;
                }

            string payload;
            try
                {
                var payloadBytes = WebEncoders.Base64UrlDecode(encodedPayload);
                payload = Encoding.UTF8.GetString(payloadBytes);
                }
            catch
                {
                return false;
                }

            var fields = payload.Split('|');
            if (fields.Length != 6)
                {
                return false;
                }

            if (!int.TryParse(fields[0], out var userId))
                {
                return false;
                }

            var ppNumber = fields[1];
            if (!int.TryParse(fields[2], out var entityId))
                {
                return false;
                }

            if (!int.TryParse(fields[3], out var roleId))
                {
                return false;
                }

            if (!long.TryParse(fields[4], out var expiresSeconds))
                {
                return false;
                }

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresSeconds);
            if (DateTimeOffset.UtcNow > expiresAt)
                {
                return false;
                }

            token = new PasswordChangeToken(userId, ppNumber, entityId, roleId, expiresAt);
            return true;
            }

        public void ClearToken(HttpRequest request, HttpResponse response)
            {
            if (response == null)
                {
                throw new ArgumentNullException(nameof(response));
                }

            response.Cookies.Delete(CookieName, BuildCookieOptions(request, DateTimeOffset.UtcNow.AddDays(-1)));
            }

        private static CookieOptions BuildCookieOptions(HttpRequest request, DateTimeOffset expiresAt)
            {
            var path = request?.PathBase.HasValue == true ? request.PathBase.Value : "/";
            return new CookieOptions
                {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = expiresAt.UtcDateTime,
                Path = string.IsNullOrWhiteSpace(path) ? "/" : path
                };
            }
        }

    public sealed class PasswordChangeToken
        {
        public PasswordChangeToken(int userId, string ppNumber, int entityId, int roleId, DateTimeOffset expiresAt)
            {
            UserId = userId;
            PPNumber = ppNumber;
            EntityId = entityId;
            RoleId = roleId;
            ExpiresAt = expiresAt;
            }

        public int UserId { get; }

        public string PPNumber { get; }

        public int EntityId { get; }

        public int RoleId { get; }

        public DateTimeOffset ExpiresAt { get; }
        }
    }
