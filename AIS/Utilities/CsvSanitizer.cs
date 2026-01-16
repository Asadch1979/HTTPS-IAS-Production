using System.Linq;

namespace AIS.Utilities
{
    public static class CsvSanitizer
    {
        private static readonly char[] DangerousPrefixes = new[] { '=', '+', '@', ':', '\\', '|', '\'', '"' };

        public static string Sanitize(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var normalized = value.Replace("\r\n", "\n").Replace("\r", "\n");

            if (normalized.Length == 0)
            {
                return normalized;
            }

            var firstChar = normalized[0];

            if (firstChar == '-')
            {
                return normalized;
            }

            if (DangerousPrefixes.Contains(firstChar))
            {
                return "'" + normalized;
            }

            return normalized;
        }
    }
}
