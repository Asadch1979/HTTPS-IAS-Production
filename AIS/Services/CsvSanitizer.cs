using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIS.Services
{
    public static class CsvSanitizer
    {
        private static readonly char[] DangerousPrefixes = new[] { '=', '+', '@', ':', '\\', '|', '\'', '"' };

        public static string Sanitize(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            var normalized = Regex.Replace(input, "[\\r\\n]+", " ").Trim();
            if (string.IsNullOrEmpty(normalized))
            {
                return string.Empty;
            }

            var firstChar = normalized[0];
            if (DangerousPrefixes.Contains(firstChar))
            {
                return "'" + normalized;
            }

            return normalized;
        }
    }
}
