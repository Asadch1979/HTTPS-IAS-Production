using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using AIS.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;

namespace AIS.Controllers
    {
    [IgnoreAntiforgeryToken]
    [Route("Administration/Catalog")]
    public class CatalogController : Controller
        {
        private const string CatalogSessionPrefix = "CatalogUpload:";
        private readonly ILogger<CatalogController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly IWebHostEnvironment _environment;

        public CatalogController(
            ILogger<CatalogController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            IWebHostEnvironment environment)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _environment = environment;
            }

        [HttpPost("Validate")]
        public IActionResult Validate([FromForm] IFormFile file, [FromForm] string catalogType)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { success = false, message = "User session is not authenticated." });
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, new { success = false, message = "You don't have access." });
                }

            if (!_sessionHandler.IsSuperUser())
                {
                return StatusCode(403, new { success = false, message = "Only Super Admins can access Catalog Uploads." });
                }

            if (file == null || file.Length == 0)
                {
                return BadRequest(new { success = false, message = "No file was provided." });
                }

            var normalizedType = NormalizeCatalogType(catalogType);
            if (normalizedType == null)
                {
                return BadRequest(new { success = false, message = "Unsupported catalog type." });
                }

            var token = Guid.NewGuid().ToString("N");
            var tempPath = SaveTempFile(file, normalizedType, token);
            var preview = BuildPreview(normalizedType, tempPath);
            preview.Token = token;

            StoreCatalogToken(token, tempPath);

            return Json(new { success = true, preview });
            }

        [HttpPost("Apply")]
        public IActionResult Apply([FromForm] string catalogType, [FromForm] string token)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { success = false, message = "User session is not authenticated." });
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, new { success = false, message = "You don't have access." });
                }

            if (!_sessionHandler.IsSuperUser())
                {
                return StatusCode(403, new { success = false, message = "Only Super Admins can access Catalog Uploads." });
                }

            var normalizedType = NormalizeCatalogType(catalogType);
            if (normalizedType == null)
                {
                return BadRequest(new { success = false, message = "Unsupported catalog type." });
                }

            if (string.IsNullOrWhiteSpace(token))
                {
                return BadRequest(new { success = false, message = "Validation token is required." });
                }

            if (!TryGetCatalogPath(token, out var tempPath))
                {
                return BadRequest(new { success = false, message = "Validation token is invalid or expired." });
                }

            try
                {
                ApplyCatalog(normalizedType, tempPath);
                ReplaceCatalogFile(normalizedType, tempPath);
                TryDeleteTempFile(tempPath);
                RemoveCatalogToken(token);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to apply catalog upload for {CatalogType}.", normalizedType);
                return StatusCode(500, new { success = false, message = "Failed to apply catalog upload." });
                }

            return Json(new { success = true });
            }

        private static string NormalizeCatalogType(string catalogType)
            {
            if (string.IsNullOrWhiteSpace(catalogType))
                {
                return null;
                }

            var normalized = catalogType.Trim().ToLowerInvariant();
            if (normalized is "api" or "page")
                {
                return normalized;
                }

            return null;
            }

        private string SaveTempFile(IFormFile file, string catalogType, string token)
            {
            var uploadRoot = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath ?? string.Empty, "Uploads", "Catalog");
            Directory.CreateDirectory(uploadRoot);
            var extension = Path.GetExtension(file.FileName);
            var safeName = catalogType == "api" ? "All Appicalls" : "Page ID";
            var fileName = $"{safeName}-{token}{extension}";
            var tempPath = Path.Combine(uploadRoot, fileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                file.CopyTo(stream);
                }

            return tempPath;
            }

        private CatalogUploadPreview BuildPreview(string catalogType, string filePath)
            {
            return catalogType == "api"
                ? BuildApiPreview(filePath)
                : BuildPagePreview(filePath);
            }

        private CatalogUploadPreview BuildApiPreview(string filePath)
            {
            var apiEntries = ParseApiCsv(filePath);
            var existing = _dbConnection.GetApiMasterList() ?? new List<ApiMasterModel>();
            var existingLookup = existing
                .Where(item => !string.IsNullOrWhiteSpace(item.ApiPath) && !string.IsNullOrWhiteSpace(item.HttpMethod))
                .ToDictionary(
                    item => BuildApiKey(item.ApiPath, item.HttpMethod),
                    item => item,
                    StringComparer.OrdinalIgnoreCase);

            var preview = new CatalogUploadPreview { CatalogType = "api" };

            foreach (var entry in apiEntries)
                {
                var key = BuildApiKey(entry.ApiPath, entry.HttpMethod);
                if (!existingLookup.TryGetValue(key, out var existingItem))
                    {
                    preview.NewApiRecords.Add(entry);
                    continue;
                    }

                if (!string.Equals(existingItem.ApiName, entry.ApiName, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existingItem.IsActive, "Y", StringComparison.OrdinalIgnoreCase))
                    {
                    preview.UpdatedApiRecords.Add(entry);
                    }
                }

            foreach (var existingItem in existing)
                {
                var key = BuildApiKey(existingItem.ApiPath, existingItem.HttpMethod);
                if (apiEntries.All(entry => !string.Equals(BuildApiKey(entry.ApiPath, entry.HttpMethod), key, StringComparison.OrdinalIgnoreCase)))
                    {
                    preview.InactiveApiRecords.Add(new CatalogApiRecord
                        {
                        ApiName = existingItem.ApiName,
                        ApiPath = existingItem.ApiPath,
                        HttpMethod = existingItem.HttpMethod,
                        IsActive = existingItem.IsActive
                        });
                    }
                }

            preview.NewCount = preview.NewApiRecords.Count;
            preview.UpdatedCount = preview.UpdatedApiRecords.Count;
            preview.InactiveCount = preview.InactiveApiRecords.Count;
            return preview;
            }

        private CatalogUploadPreview BuildPagePreview(string filePath)
            {
            var pageEntries = ParsePageCatalog(filePath);
            var existing = _dbConnection.GetMenuPagesForAdminPanel(0, 0) ?? new List<MenuPagesAssignmentModel>();
            var existingLookup = existing
                .Where(item => int.TryParse(item.P_ID, out _))
                .ToDictionary(item => int.Parse(item.P_ID, CultureInfo.InvariantCulture), item => item);

            var preview = new CatalogUploadPreview { CatalogType = "page" };

            foreach (var entry in pageEntries)
                {
                if (!existingLookup.TryGetValue(entry.PageId, out var existingItem))
                    {
                    preview.NewPageRecords.Add(entry);
                    continue;
                    }

                if (!string.Equals(existingItem.P_PATH, entry.PagePath, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existingItem.P_STATUS, "Y", StringComparison.OrdinalIgnoreCase))
                    {
                    preview.UpdatedPageRecords.Add(entry);
                    }
                }

            foreach (var existingItem in existing)
                {
                if (!int.TryParse(existingItem.P_ID, out var pageId))
                    {
                    continue;
                    }

                if (pageEntries.All(entry => entry.PageId != pageId))
                    {
                    preview.InactivePageRecords.Add(new CatalogPageRecord
                        {
                        PageId = pageId,
                        PageName = existingItem.P_NAME,
                        PagePath = existingItem.P_PATH,
                        IsActive = existingItem.P_STATUS
                        });
                    }
                }

            preview.NewCount = preview.NewPageRecords.Count;
            preview.UpdatedCount = preview.UpdatedPageRecords.Count;
            preview.InactiveCount = preview.InactivePageRecords.Count;
            return preview;
            }

        private void ApplyCatalog(string catalogType, string filePath)
            {
            if (catalogType == "api")
                {
                ApplyApiCatalog(filePath);
                return;
                }

            ApplyPageCatalog(filePath);
            }

        private void ApplyApiCatalog(string filePath)
            {
            var apiEntries = ParseApiCsv(filePath);
            var existing = _dbConnection.GetApiMasterList() ?? new List<ApiMasterModel>();
            var existingLookup = existing
                .Where(item => !string.IsNullOrWhiteSpace(item.ApiPath) && !string.IsNullOrWhiteSpace(item.HttpMethod))
                .ToDictionary(
                    item => BuildApiKey(item.ApiPath, item.HttpMethod),
                    item => item,
                    StringComparer.OrdinalIgnoreCase);

            foreach (var entry in apiEntries)
                {
                var key = BuildApiKey(entry.ApiPath, entry.HttpMethod);
                if (!existingLookup.TryGetValue(key, out var existingItem))
                    {
                    _dbConnection.InsertApiMaster(new ApiMasterModel
                        {
                        ApiName = entry.ApiName,
                        ApiPath = entry.ApiPath,
                        HttpMethod = entry.HttpMethod,
                        IsActive = "Y"
                        });
                    continue;
                    }

                if (!string.Equals(existingItem.ApiName, entry.ApiName, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existingItem.IsActive, "Y", StringComparison.OrdinalIgnoreCase))
                    {
                    _dbConnection.UpdateApiMaster(new ApiMasterModel
                        {
                        ApiId = existingItem.ApiId,
                        ApiName = entry.ApiName,
                        ApiPath = entry.ApiPath,
                        HttpMethod = entry.HttpMethod,
                        IsActive = "Y"
                        });
                    }
                }

            foreach (var existingItem in existing)
                {
                var key = BuildApiKey(existingItem.ApiPath, existingItem.HttpMethod);
                if (apiEntries.All(entry => !string.Equals(BuildApiKey(entry.ApiPath, entry.HttpMethod), key, StringComparison.OrdinalIgnoreCase)))
                    {
                    _dbConnection.UpdateApiMaster(new ApiMasterModel
                        {
                        ApiId = existingItem.ApiId,
                        ApiName = existingItem.ApiName,
                        ApiPath = existingItem.ApiPath,
                        HttpMethod = existingItem.HttpMethod,
                        IsActive = "N"
                        });
                    }
                }
            }

        private void ApplyPageCatalog(string filePath)
            {
            var pageEntries = ParsePageCatalog(filePath);
            var existing = _dbConnection.GetMenuPagesForAdminPanel(0, 0) ?? new List<MenuPagesAssignmentModel>();
            var existingLookup = existing
                .Where(item => int.TryParse(item.P_ID, out _))
                .ToDictionary(item => int.Parse(item.P_ID, CultureInfo.InvariantCulture), item => item);

            foreach (var entry in pageEntries)
                {
                if (!existingLookup.TryGetValue(entry.PageId, out var existingItem))
                    {
                    continue;
                    }

                if (!string.Equals(existingItem.P_PATH, entry.PagePath, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existingItem.P_STATUS, "Y", StringComparison.OrdinalIgnoreCase))
                    {
                    existingItem.P_PATH = entry.PagePath;
                    existingItem.P_STATUS = "Y";
                    _dbConnection.UpdateMenuPageForAdminPanel(existingItem);
                    }
                }

            foreach (var existingItem in existing)
                {
                if (!int.TryParse(existingItem.P_ID, out var pageId))
                    {
                    continue;
                    }

                if (pageEntries.All(entry => entry.PageId != pageId))
                    {
                    existingItem.P_STATUS = "N";
                    _dbConnection.UpdateMenuPageForAdminPanel(existingItem);
                    }
                }
            }

        private void ReplaceCatalogFile(string catalogType, string tempPath)
            {
            if (catalogType == "api")
                {
                var targetPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath ?? string.Empty, "All Appicalls.csv");
                System.IO.File.Copy(tempPath, targetPath, true);
                return;
                }

            var imagesPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath ?? string.Empty, "Images");
            Directory.CreateDirectory(imagesPath);
            var targetFile = Path.Combine(imagesPath, "Page ID.xlsx");
            System.IO.File.Copy(tempPath, targetFile, true);
            }

        private void TryDeleteTempFile(string tempPath)
            {
            if (string.IsNullOrWhiteSpace(tempPath))
                {
                return;
                }

            if (!System.IO.File.Exists(tempPath))
                {
                return;
                }

            System.IO.File.Delete(tempPath);
            }

        private List<CatalogApiRecord> ParseApiCsv(string filePath)
            {
            var results = new List<CatalogApiRecord>();

            using (var parser = new TextFieldParser(filePath))
                {
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                if (parser.EndOfData)
                    {
                    return results;
                    }

                var headers = parser.ReadFields() ?? Array.Empty<string>();
                var headerLookup = headers
                    .Select((name, index) => new { name = name ?? string.Empty, index })
                    .ToDictionary(item => item.name.Trim(), item => item.index, StringComparer.OrdinalIgnoreCase);

                var methodIndex = GetHeaderIndex(headerLookup, "http_method");
                var controllerIndex = GetHeaderIndex(headerLookup, "controller_name");
                var actionIndex = GetHeaderIndex(headerLookup, "action_name");
                var apiPathIndex = GetHeaderIndex(headerLookup, "api_path");

                while (!parser.EndOfData)
                    {
                    var fields = parser.ReadFields();
                    if (fields == null || fields.Length == 0)
                        {
                        continue;
                        }

                    var apiPath = apiPathIndex >= 0 && apiPathIndex < fields.Length
                        ? fields[apiPathIndex]
                        : fields.Length > 9
                            ? fields[9]
                            : string.Empty;

                    apiPath = NormalizeApiPath(apiPath);

                    var method = methodIndex >= 0 && methodIndex < fields.Length
                        ? fields[methodIndex]
                        : string.Empty;

                    method = NormalizeHttpMethod(method);

                    if (string.IsNullOrWhiteSpace(apiPath) || string.IsNullOrWhiteSpace(method))
                        {
                        continue;
                        }

                    var controller = controllerIndex >= 0 && controllerIndex < fields.Length
                        ? fields[controllerIndex]
                        : string.Empty;
                    var action = actionIndex >= 0 && actionIndex < fields.Length
                        ? fields[actionIndex]
                        : string.Empty;

                    var apiName = string.Join("/", new[] { controller, action }.Where(value => !string.IsNullOrWhiteSpace(value)));

                    results.Add(new CatalogApiRecord
                        {
                        ApiName = string.IsNullOrWhiteSpace(apiName) ? apiPath : apiName,
                        ApiPath = apiPath,
                        HttpMethod = method,
                        IsActive = "Y"
                        });
                    }
                }

            return results;
            }

        private List<CatalogPageRecord> ParsePageCatalog(string filePath)
            {
            var results = new List<CatalogPageRecord>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
                return results;
                }

            var table = dataSet.Tables[0];
            var columnMap = table.Columns.Cast<System.Data.DataColumn>()
                .ToDictionary(column => column.ColumnName, column => column.ColumnName, StringComparer.OrdinalIgnoreCase);

            foreach (System.Data.DataRow row in table.Rows)
                {
                var pageIdRaw = GetColumnValue(row, columnMap, "PAGE_ID", "Page Id", "PageId", "ID");
                if (!int.TryParse(pageIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pageId) || pageId <= 0)
                    {
                    continue;
                    }

                var pagePath = GetColumnValue(row, columnMap, "PAGE_PATH", "Page Path", "PagePath");
                var pageName = GetColumnValue(row, columnMap, "PAGE_NAME", "Page Name", "PageName", "NAME");

                results.Add(new CatalogPageRecord
                    {
                    PageId = pageId,
                    PageName = pageName,
                    PagePath = pagePath,
                    IsActive = "Y"
                    });
                }

            return results;
            }

        private static string GetColumnValue(System.Data.DataRow row, IDictionary<string, string> columnMap, params string[] candidates)
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

        private static int GetHeaderIndex(IDictionary<string, int> headerLookup, params string[] candidates)
            {
            foreach (var candidate in candidates)
                {
                if (headerLookup.TryGetValue(candidate, out var index))
                    {
                    return index;
                    }
                }

            return -1;
            }

        private static string BuildApiKey(string route, string method)
            {
            return $"{NormalizeApiPath(route)}::{NormalizeHttpMethod(method)}";
            }

        private static string NormalizeApiPath(string path)
            {
            if (string.IsNullOrWhiteSpace(path))
                {
                return string.Empty;
                }

            var cleaned = path.Trim();
            var separatorIndex = cleaned.IndexOf(";", StringComparison.Ordinal);
            if (separatorIndex >= 0)
                {
                cleaned = cleaned.Substring(0, separatorIndex);
                }

            return cleaned.Trim();
            }

        private static string NormalizeHttpMethod(string method)
            {
            return string.IsNullOrWhiteSpace(method) ? string.Empty : method.Trim().ToUpperInvariant();
            }

        private void StoreCatalogToken(string token, string filePath)
            {
            HttpContext.Session.SetString(CatalogSessionPrefix + token, JsonSerializer.Serialize(new CatalogSessionData
                {
                FilePath = filePath
                }));
            }

        private bool TryGetCatalogPath(string token, out string filePath)
            {
            filePath = null;
            var payload = HttpContext.Session.GetString(CatalogSessionPrefix + token);
            if (string.IsNullOrWhiteSpace(payload))
                {
                return false;
                }

            var data = JsonSerializer.Deserialize<CatalogSessionData>(payload);
            if (data == null || string.IsNullOrWhiteSpace(data.FilePath) || !System.IO.File.Exists(data.FilePath))
                {
                return false;
                }

            filePath = data.FilePath;
            return true;
            }

        private void RemoveCatalogToken(string token)
            {
            HttpContext.Session.Remove(CatalogSessionPrefix + token);
            }

        private class CatalogSessionData
            {
            public string FilePath { get; set; }
            }
        }
    }
