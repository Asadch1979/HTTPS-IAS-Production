using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIS.Services
    {
    public class PageIdResolver : IPageIdResolver
        {
        private const string PageIdItemKey = "AIS.PageIdResolution";
        private readonly IReadOnlyDictionary<string, PageIdEntry> _pathLookup;
        private readonly ILogger<PageIdResolver> _logger;
        private readonly IWebHostEnvironment _environment;

        public PageIdResolver(IWebHostEnvironment environment, ILogger<PageIdResolver> logger)
            {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var filePath = ResolvePageMappingPath(environment);
            _pathLookup = AddFieldAuditReportMappings(LoadPageIndex(filePath));
            }

        public int ResolvePageId(HttpContext httpContext)
            {
            if (httpContext == null)
                {
                return 0;
                }

            var requestPath = httpContext.Request?.Path.Value;
            if (PageIdPathHelper.IsExempt(requestPath))
                {
                return 0;
                }

            if (TryResolvePageId(httpContext, out var pageId))
                {
                return pageId;
                }

            LogMissingPageId(requestPath);
            return 0;
            }

        public int ResolvePageId(string requestPath)
            {
            if (PageIdPathHelper.IsExempt(requestPath))
                {
                return 0;
                }

            if (TryResolvePageId(requestPath, out var pageId))
                {
                return pageId;
                }

            LogMissingPageId(requestPath);
            return 0;
            }

        public bool TryResolvePageId(HttpContext httpContext, out int pageId)
            {
            pageId = 0;

            if (httpContext == null)
                {
                return false;
                }

            if (httpContext.Items.TryGetValue(PageIdItemKey, out var cached) && cached is PageIdResolution resolution)
                {
                pageId = resolution.PageId;
                return resolution.Found;
                }

            if (TryResolvePageIdFromRequest(httpContext.Request, out pageId))
                {
                httpContext.Items[PageIdItemKey] = new PageIdResolution(true, pageId);
                return true;
                }

            httpContext.Items[PageIdItemKey] = new PageIdResolution(false, 0);
            return false;
            }

        public bool TryResolvePageId(string requestPath, out int pageId)
            {
            pageId = 0;
            return TryResolvePageIdFromPath(requestPath, out pageId);
            }

        private bool TryResolvePageIdFromRequest(HttpRequest request, out int pageId)
            {
            pageId = 0;

            if (request == null)
                {
                return false;
                }

            var path = request.Path.HasValue ? request.Path.Value : string.Empty;
            return TryResolvePageIdFromPath(path, out pageId);
            }

        private bool TryResolvePageIdFromPath(string requestPath, out int pageId)
            {
            pageId = 0;

            var normalizedPath = PageIdPathHelper.NormalizePath(requestPath);
            if (!string.IsNullOrWhiteSpace(normalizedPath) && _pathLookup.TryGetValue(normalizedPath, out var entry))
                {
                pageId = entry.PageId;
                return true;
                }

            return false;
            }

        private static string ResolvePageMappingPath(IWebHostEnvironment environment)
            {
            var webRoot = environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                {
                webRoot = Path.Combine(environment.ContentRootPath ?? string.Empty, "wwwroot");
                }

            return Path.Combine(webRoot, "Images", "Page ID.xlsx");
            }

        private IReadOnlyDictionary<string, PageIdEntry> LoadPageIndex(string filePath)
            {
            if (string.IsNullOrWhiteSpace(filePath))
                {
                throw new InvalidOperationException("Page ID.xlsx path could not be resolved.");
                }

            if (!File.Exists(filePath))
                {
                throw new FileNotFoundException("Page ID.xlsx was not found. Ensure wwwroot/Images/Page ID.xlsx is deployed.", filePath);
                }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                    UseHeaderRow = true
                    }
                });

            if (dataSet.Tables.Count == 0)
                {
                throw new InvalidOperationException("Page ID.xlsx does not contain any worksheets.");
                }

            var table = dataSet.Tables[0];
            var columnMap = table.Columns.Cast<DataColumn>()
                .ToDictionary(column => column.ColumnName, column => column.ColumnName, StringComparer.OrdinalIgnoreCase);

            var entries = new List<PageIdEntry>();

            foreach (DataRow row in table.Rows)
                {
                var pageIdRaw = GetColumnValue(row, columnMap, "PAGE_ID", "Page Id", "PageId","ID");
                if (!int.TryParse(pageIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pageId) || pageId <= 0)
                    {
                    continue;
                    }

                var pagePath = PageIdPathHelper.NormalizePath(GetColumnValue(row, columnMap, "PAGE_PATH", "Page Path", "PagePath"));
                entries.Add(new PageIdEntry(pageId, pagePath));
                }

            var pathLookup = new Dictionary<string, PageIdEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in entries)
                {
                if (!string.IsNullOrWhiteSpace(entry.PagePath))
                    {
                    var pathKey = PageIdPathHelper.NormalizePath(entry.PagePath);
                    if (!pathLookup.TryAdd(pathKey, entry))
                        {
                        _logger.LogWarning("Duplicate PAGE_PATH mapping detected for {Path}. Using PAGE_ID {PageId}.", pathKey, entry.PageId);
                        }
                    }
                }

            return pathLookup;
            }

        private static IReadOnlyDictionary<string, PageIdEntry> AddFieldAuditReportMappings(IReadOnlyDictionary<string, PageIdEntry> existing)
            {
            var updated = new Dictionary<string, PageIdEntry>(existing, StringComparer.OrdinalIgnoreCase);
            var fieldAuditPages = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                ["/FieldAuditReport/ReportOverview"] = 970101,
                ["/FieldAuditReport/NarrativeSections"] = 970102,
                ["/FieldAuditReport/KpiSnapshot"] = 970103,
                ["/FieldAuditReport/NplSnapshot"] = 970104,
                ["/FieldAuditReport/StaffSnapshot"] = 970105,
                ["/FieldAuditReport/FinalizeReport"] = 970106
                };

            foreach (var entry in fieldAuditPages)
                {
                var normalized = PageIdPathHelper.NormalizePath(entry.Key);
                if (!updated.ContainsKey(normalized))
                    {
                    updated[normalized] = new PageIdEntry(entry.Value, normalized);
                    }
                }

            return updated;
            }

        private static string GetColumnValue(DataRow row, IDictionary<string, string> columnMap, params string[] candidates)
            {
            foreach (var candidate in candidates)
                {
                if (columnMap.TryGetValue(candidate, out var columnName))
                    {
                    var value = row[columnName]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        {
                        return value.Trim();
                        }
                    }
                }

            return string.Empty;
            }

        private void LogMissingPageId(string requestPath)
            {
            if (!_environment.IsDevelopment())
                {
                return;
                }

            var normalized = PageIdPathHelper.NormalizePath(requestPath);
            var pathValue = string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized;
            _logger.LogWarning("Missing PAGE_ID mapping for request '{Path}'.", pathValue);
            }

        private sealed class PageIdEntry
            {
            public PageIdEntry(int pageId, string pagePath)
                {
                PageId = pageId;
                PagePath = pagePath;
                }

            public int PageId { get; }
            public string PagePath { get; }
            }

        private sealed class PageIdResolution
            {
            public PageIdResolution(bool found, int pageId)
                {
                Found = found;
                PageId = pageId;
                }

            public bool Found { get; }
            public int PageId { get; }
            }
        }
    }
