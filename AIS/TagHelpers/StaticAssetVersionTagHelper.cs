using AIS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIS.TagHelpers
    {
    [HtmlTargetElement("script", Attributes = "src")]
    [HtmlTargetElement("img", Attributes = "src")]
    [HtmlTargetElement("link", Attributes = "href")]
    public class StaticAssetVersionTagHelper : TagHelper
        {
        private const string AdminVersionKey = "iasv";
        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            ".js",
            ".css",
            ".png",
            ".jpg",
            ".jpeg",
            ".gif",
            ".svg",
            ".ico",
            ".webp",
            ".bmp",
            ".map",
            ".woff",
            ".woff2",
            ".ttf",
            ".eot",
            ".otf",
            ".avif"
            };
        private static readonly HashSet<string> VersionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            "v",
            "ver",
            "version",
            AdminVersionKey
            };

        private readonly IFileVersionProvider _fileVersionProvider;
        private readonly IStaticAssetVersionTokenProvider _tokenProvider;

        public StaticAssetVersionTagHelper(
            IFileVersionProvider fileVersionProvider,
            IStaticAssetVersionTokenProvider tokenProvider)
            {
            _fileVersionProvider = fileVersionProvider ?? throw new ArgumentNullException(nameof(fileVersionProvider));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override int Order => int.MaxValue;

        public override void Process(TagHelperContext context, TagHelperOutput output)
            {
            if (ViewContext?.HttpContext?.Request == null)
                {
                return;
                }

            var attributeName = string.Equals(output.TagName, "link", StringComparison.OrdinalIgnoreCase) ? "href" : "src";
            var originalValue = output.Attributes[attributeName]?.Value?.ToString();
            if (!ShouldProcess(originalValue))
                {
                return;
                }

            var fragment = ExtractFragment(originalValue);
            var prepared = PrepareLocalAssetPath(originalValue);
            if (string.IsNullOrWhiteSpace(prepared))
                {
                return;
                }

            var versionedPath = _fileVersionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, prepared);
            var token = _tokenProvider.GetToken();
            if (!string.IsNullOrWhiteSpace(token))
                {
                versionedPath = QueryHelpers.AddQueryString(versionedPath, AdminVersionKey, token);
                }

            output.Attributes.SetAttribute(attributeName, versionedPath + fragment);
            }

        private static bool ShouldProcess(string path)
            {
            if (string.IsNullOrWhiteSpace(path))
                {
                return false;
                }

            if (IsExternal(path))
                {
                return false;
                }

            var barePath = ExtractBarePath(path);
            if (!(barePath.StartsWith("~/", StringComparison.Ordinal) || barePath.StartsWith("/", StringComparison.Ordinal)))
                {
                return false;
                }

            var extension = Path.GetExtension(barePath);
            return !string.IsNullOrWhiteSpace(extension) && SupportedExtensions.Contains(extension);
            }

        private static string PrepareLocalAssetPath(string originalValue)
            {
            var valueWithoutFragment = RemoveFragment(originalValue);

            var pathOnly = valueWithoutFragment;
            var queryText = string.Empty;
            var queryIndex = valueWithoutFragment.IndexOf('?');
            if (queryIndex >= 0)
                {
                pathOnly = valueWithoutFragment.Substring(0, queryIndex);
                queryText = valueWithoutFragment.Substring(queryIndex);
                }

            if (string.IsNullOrWhiteSpace(pathOnly))
                {
                return string.Empty;
                }

            var queryValues = QueryHelpers.ParseQuery(queryText);
            var preservedPairs = queryValues
                .Where(kvp => !VersionKeys.Contains(kvp.Key))
                .SelectMany(kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value))
                .ToList();

            var rebuilt = pathOnly;
            if (preservedPairs.Count > 0)
                {
                rebuilt += QueryString.Create(preservedPairs).ToString();
                }

            return rebuilt;
            }

        private static string ExtractBarePath(string originalValue)
            {
            var value = RemoveFragment(originalValue);
            var queryIndex = value.IndexOf('?');
            if (queryIndex >= 0)
                {
                value = value.Substring(0, queryIndex);
                }

            return value;
            }

        private static bool IsExternal(string path)
            {
            return path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("//", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("blob:", StringComparison.OrdinalIgnoreCase);
            }

        private static string RemoveFragment(string value)
            {
            var fragmentIndex = value.IndexOf('#');
            return fragmentIndex >= 0 ? value.Substring(0, fragmentIndex) : value;
            }

        private static string ExtractFragment(string value)
            {
            var fragmentIndex = value.IndexOf('#');
            return fragmentIndex >= 0 ? value.Substring(fragmentIndex) : string.Empty;
            }
        }
    }
