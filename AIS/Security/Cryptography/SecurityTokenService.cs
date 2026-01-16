using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace AIS.Security.Cryptography
    {
    public class SecurityTokenService
        {
        private readonly byte[] _secretKeyBytes;

        public SecurityTokenService(IConfiguration configuration)
            {
            var secretKey = configuration?["SecretKey"] ?? configuration?["Security:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                {
                throw new InvalidOperationException("SecretKey configuration is required for cryptographic operations.");
                }

            _secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            }

        public string GenerateSessionToken()
            {
            Span<byte> tokenBytes = stackalloc byte[48];
            RandomNumberGenerator.Fill(tokenBytes);
            return WebEncoders.Base64UrlEncode(tokenBytes);
            }

        public string ComputeHash(string input)
            {
            if (input == null)
                {
                throw new ArgumentNullException(nameof(input));
                }

            using var sha256 = SHA256.Create();
            var data = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(data).ToLowerInvariant();
            }

        public string ComputeHmacHash(string input)
            {
            if (input == null)
                {
                throw new ArgumentNullException(nameof(input));
                }

            using var hmac = new HMACSHA256(_secretKeyBytes);
            var data = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(data).ToLowerInvariant();
            }
        }
    }
