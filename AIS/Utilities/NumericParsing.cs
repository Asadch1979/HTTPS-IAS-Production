using System.Globalization;

namespace AIS.Utilities
    {
    public static class NumericParsing
        {
        public static int? ToNullableInt(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return null;
                }

            var sanitized = value.Replace(",", string.Empty).Trim();
            return int.TryParse(sanitized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : (int?)null;
            }

        public static long? ToNullableLong(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return null;
                }

            var sanitized = value.Replace(",", string.Empty).Trim();
            return long.TryParse(sanitized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : (long?)null;
            }

        public static decimal? ToNullableDecimal(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return null;
                }

            var sanitized = value.Replace(",", string.Empty).Trim();
            return decimal.TryParse(sanitized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : (decimal?)null;
            }

        public static int ToIntOrDefault(string value, int defaultValue = 0)
            {
            return ToNullableInt(value) ?? defaultValue;
            }

        public static long ToLongOrDefault(string value, long defaultValue = 0)
            {
            return ToNullableLong(value) ?? defaultValue;
            }

        public static decimal ToDecimalOrDefault(string value, decimal defaultValue = 0)
            {
            return ToNullableDecimal(value) ?? defaultValue;
            }

        public static int ToIntOrDefault(int? value, int defaultValue = 0)
            {
            return value ?? defaultValue;
            }

        public static long ToLongOrDefault(long? value, long defaultValue = 0)
            {
            return value ?? defaultValue;
            }

        public static decimal ToDecimalOrDefault(decimal? value, decimal defaultValue = 0)
            {
            return value ?? defaultValue;
            }
        }
    }
