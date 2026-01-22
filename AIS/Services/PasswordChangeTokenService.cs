using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AIS.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace AIS.Services
    {
    public sealed class PasswordChangeTokenService
        {
        public const string CookieName = "IAS.PwdChange";
        private const int DefaultTokenMinutes = 10;
        private const char TokenSeparator = '.';
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
            {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        private readonly SecurityTokenService _tokenService;

        public PasswordChangeTokenService(SecurityTokenService tokenService)
            {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            }

        public string CreateToken(int userId, string ppNumber)
            {
            var now = DateTimeOffset.UtcNow;
            var payload = new PasswordChangeTokenPayload
                {
                UserId = userId,
                PPNumber = ppNumber ?? string.Empty,
                ExpiresAt = now.AddMinutes(DefaultTokenMinutes).ToUnixTimeSeconds(),
                Nonce = Guid.NewGuid().ToString("N")
                };

            var payloadJson = JsonSerializer.Serialize(payload, SerializerOptions);
            var payloadEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
            var signature = _tokenService.ComputeHmacHash(payloadEncoded);
            return string.Concat(payloadEncoded, TokenSeparator, signature);
            }

        public bool TryValidateToken(string token, out PasswordChangeTokenInfo tokenInfo)
            {
            tokenInfo = null;

            if (string.IsNullOrWhiteSpace(token))
                {
                return false;
                }

            var parts = token.Split(TokenSeparator);
            if (parts.Length != 2)
                {
                return false;
                }

            var payloadEncoded = parts[0];
            var signature = parts[1];
            var expectedSignature = _tokenService.ComputeHmacHash(payloadEncoded);
            if (!SignaturesMatch(signature, expectedSignature))
                {
                return false;
                }

            PasswordChangeTokenPayload payload;
            try
                {
                var payloadBytes = WebEncoders.Base64UrlDecode(payloadEncoded);
                payload = JsonSerializer.Deserialize<PasswordChangeTokenPayload>(payloadBytes, SerializerOptions);
                }
            catch
                {
                return false;
                }

            if (payload == null)
                {
                return false;
                }

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAt);
            if (expiresAt <= DateTimeOffset.UtcNow)
                {
                return false;
                }

            tokenInfo = new PasswordChangeTokenInfo(payload.UserId, payload.PPNumber, expiresAt);
            return true;
            }

        public void AppendCookie(HttpResponse response, string token, PathString pathBase)
            {
            if (response == null)
                {
                throw new ArgumentNullException(nameof(response));
                }

            if (string.IsNullOrWhiteSpace(token))
                {
                return;
                }

            response.Cookies.Append(CookieName, token, new CookieOptions
                {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(DefaultTokenMinutes),
                Path = pathBase.HasValue ? pathBase.Value : "/"
                });
            }

        public void ClearCookie(HttpResponse response, PathString pathBase)
            {
            if (response == null)
                {
                throw new ArgumentNullException(nameof(response));
                }

            response.Cookies.Delete(CookieName, new CookieOptions
                {
                Path = pathBase.HasValue ? pathBase.Value : "/"
                });
            }

        public bool TryGetTokenFromRequest(HttpRequest request, out PasswordChangeTokenInfo tokenInfo)
            {
            tokenInfo = null;
            if (request == null)
                {
                return false;
                }

            if (!request.Cookies.TryGetValue(CookieName, out var token))
                {
                return false;
                }

            return TryValidateToken(token, out tokenInfo);
            }

        private static bool SignaturesMatch(string provided, string expected)
            {
            if (provided == null || expected == null)
                {
                return false;
                }

            var providedBytes = Encoding.UTF8.GetBytes(provided);
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            return CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
            }

        private sealed class PasswordChangeTokenPayload
            {
            public int UserId { get; set; }
            public string PPNumber { get; set; }
            public long ExpiresAt { get; set; }
            public string Nonce { get; set; }
            }
        }

    public sealed record PasswordChangeTokenInfo(int UserId, string PPNumber, DateTimeOffset ExpiresAt);
    }
