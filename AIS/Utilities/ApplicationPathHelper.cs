using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

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

        public static string RemoveQueryParameter(string localUrl, string parameterName)
            {
            if (string.IsNullOrWhiteSpace(localUrl) || string.IsNullOrWhiteSpace(parameterName))
                {
                return localUrl;
                }

            var fragmentIndex = localUrl.IndexOf('#');
            var valueWithoutFragment = fragmentIndex >= 0 ? localUrl.Substring(0, fragmentIndex) : localUrl;
            var fragment = fragmentIndex >= 0 ? localUrl.Substring(fragmentIndex) : string.Empty;
            var queryIndex = valueWithoutFragment.IndexOf('?');
            if (queryIndex < 0)
                {
                return localUrl;
                }

            var path = valueWithoutFragment.Substring(0, queryIndex);
            var query = valueWithoutFragment.Substring(queryIndex);
            var preservedPairs = QueryHelpers.ParseQuery(query)
                .Where(queryItem => !string.Equals(queryItem.Key, parameterName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(
                    queryItem => queryItem.Value,
                    (queryItem, value) => new KeyValuePair<string, string>(queryItem.Key, value));

            return path + QueryString.Create(preservedPairs) + fragment;
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
