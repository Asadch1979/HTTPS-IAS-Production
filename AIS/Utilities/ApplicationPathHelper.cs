using System;

namespace AIS.Utilities
    {
    public static class ApplicationPathHelper
        {
        public static string EnsurePathBase(string localUrl, string pathBase)
            {
            if (string.IsNullOrWhiteSpace(localUrl))
                {
                return localUrl;
                }

            var suffixIndex = FindSuffixIndex(localUrl);
            var path = suffixIndex >= 0 ? localUrl.Substring(0, suffixIndex) : localUrl;
            var suffix = suffixIndex >= 0 ? localUrl.Substring(suffixIndex) : string.Empty;

            if (path.StartsWith("~/", StringComparison.Ordinal))
                {
                path = path.Substring(1);
                }
            else if (string.Equals(path, "~", StringComparison.Ordinal))
                {
                path = "/";
                }

            var normalizedPathBase = NormalizePathBase(pathBase);
            if (string.IsNullOrEmpty(normalizedPathBase) || !path.StartsWith("/", StringComparison.Ordinal))
                {
                return path + suffix;
                }

            while (HasPathBase(path, normalizedPathBase))
                {
                path = path.Substring(normalizedPathBase.Length);
                if (string.IsNullOrEmpty(path))
                    {
                    path = "/";
                    }
                }

            return normalizedPathBase + path + suffix;
            }

        private static int FindSuffixIndex(string url)
            {
            var queryIndex = url.IndexOf('?');
            var fragmentIndex = url.IndexOf('#');

            if (queryIndex < 0)
                {
                return fragmentIndex;
                }

            if (fragmentIndex < 0)
                {
                return queryIndex;
                }

            return Math.Min(queryIndex, fragmentIndex);
            }

        private static string NormalizePathBase(string pathBase)
            {
            if (string.IsNullOrWhiteSpace(pathBase) || string.Equals(pathBase, "/", StringComparison.Ordinal))
                {
                return string.Empty;
                }

            var normalized = pathBase.Trim();
            if (!normalized.StartsWith("/", StringComparison.Ordinal))
                {
                normalized = "/" + normalized;
                }

            return normalized.TrimEnd('/');
            }

        private static bool HasPathBase(string path, string pathBase)
            {
            return string.Equals(path, pathBase, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(pathBase + "/", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
