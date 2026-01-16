using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AIS.Security.PasswordPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIS.Security.PasswordPolicy
{
    public class PasswordPolicyValidator
    {
        private const string GenericErrorMessage = "Password does not meet security requirements.";

        private readonly ILogger<PasswordPolicyValidator> _logger;
        private readonly int _minimumLength;
        private readonly string _blocklistPath;
        private readonly Lazy<HashSet<string>> _blockedPasswords;
        private readonly bool _rejectIdentifiers;

        public PasswordPolicyValidator(IConfiguration configuration, ILogger<PasswordPolicyValidator> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _minimumLength = Math.Max(configuration.GetValue<int?>("PasswordPolicy:MinimumLength") ?? 12, 8);
            _blocklistPath = ResolveBlocklistPath(configuration["PasswordPolicy:BlocklistPath"], environment.ContentRootPath);
            _blockedPasswords = new Lazy<HashSet<string>>(LoadBlocklist);
            _rejectIdentifiers = configuration.GetValue<bool?>("PasswordPolicy:RejectIdentifiers") ?? true;
        }

        public PasswordPolicyResult Validate(string password, params string[] identifiers)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return new PasswordPolicyResult(false, GenericErrorMessage);
            }

            var trimmed = password.Trim();
            if (trimmed.Length < _minimumLength)
            {
                return new PasswordPolicyResult(false, GenericErrorMessage);
            }

            if (!HasRequiredComplexity(trimmed))
            {
                return new PasswordPolicyResult(false, GenericErrorMessage);
            }

            if (_blockedPasswords.Value.Contains(trimmed.ToLowerInvariant()))
            {
                _logger.LogWarning("Password rejected because it appears in the blocklist.");
                return new PasswordPolicyResult(false, GenericErrorMessage);
            }

            if (_rejectIdentifiers && ContainsIdentifier(trimmed, identifiers))
            {
                _logger.LogWarning("Password rejected because it contains an identifier value.");
                return new PasswordPolicyResult(false, GenericErrorMessage);
            }

            return new PasswordPolicyResult(true, null);
        }

        public string GenerateCompliantPassword(params string[] identifiers)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var candidate = BuildCandidatePassword();
                if (Validate(candidate, identifiers).IsValid)
                {
                    return candidate;
                }
            }

            var fallback = BuildCandidatePassword(_minimumLength + 4);
            return Validate(fallback, identifiers).IsValid ? fallback : fallback + "A1!";
        }

        private string BuildCandidatePassword(int? overrideLength = null)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()-_=+[]{}<>?";
            var length = Math.Max(overrideLength ?? _minimumLength, _minimumLength);
            var characterSets = new[] { upper, lower, digits, special };
            var allCharacters = string.Concat(characterSets);

            var passwordChars = new List<char>(length);
            var randomBytes = new byte[length];
            RandomNumberGenerator.Fill(randomBytes);

            for (int i = 0; i < characterSets.Length; i++)
            {
                passwordChars.Add(characterSets[i][randomBytes[i] % characterSets[i].Length]);
            }

            for (int i = characterSets.Length; i < length; i++)
            {
                passwordChars.Add(allCharacters[randomBytes[i] % allCharacters.Length]);
            }

            Shuffle(passwordChars);
            return new string(passwordChars.ToArray());
        }

        private bool HasRequiredComplexity(string password)
        {
            var hasUpper = password.Any(char.IsUpper);
            var hasLower = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        private bool ContainsIdentifier(string password, params string[] identifiers)
        {
            if (identifiers == null)
            {
                return false;
            }

            foreach (var identifier in identifiers)
            {
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    continue;
                }

                var trimmedIdentifier = identifier.Trim();
                if (trimmedIdentifier.Length < 3)
                {
                    continue;
                }

                if (password.IndexOf(trimmedIdentifier, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private string ResolveBlocklistPath(string configuredPath, string contentRootPath)
        {
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                var combined = Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.Combine(contentRootPath, configuredPath);
                return combined;
            }

            return Path.Combine(contentRootPath, "Security", "PasswordPolicy", "password_blocklist.txt");
        }

        private HashSet<string> LoadBlocklist()
        {
            try
            {
                if (File.Exists(_blocklistPath))
                {
                    var entries = File.ReadAllLines(_blocklistPath)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim().ToLowerInvariant());
                    return new HashSet<string>(entries);
                }

                _logger.LogWarning("Password blocklist not found at path {Path}.", _blocklistPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load password blocklist from {Path}.", _blocklistPath);
            }

            return new HashSet<string>();
        }

        private void Shuffle(IList<char> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
