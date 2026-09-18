using AIS.Models;
using AIS.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    [EnableRateLimiting("FileTransferPolicy")]
    [IgnoreAntiforgeryToken]
    public class UploadFileController : Controller
        {
        private readonly ILogger<UploadFileController> _logger;
        private readonly IConfiguration _configuration;
        private readonly DBConnection _dbConnection;
        private readonly string _uploadPath;
        private readonly string _uploadReportPath;
        private readonly string _uploadPathAuditee;
        private readonly string _uploadPathCAU;
        private const long ComplianceEvidenceSizeLimitBytes = 10 * 1024 * 1024;
        private const int ComplianceEvidenceFileLimit = 100;
        private const string ComplianceEvidenceSizeLimitMessage = "Total evidence size cannot exceed 10 MB. Please remove unnecessary files or compress your documents.";
        private static readonly HashSet<string> ComplianceEvidenceAllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            ".pdf",
            ".zip",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docx",
            ".csv",
            ".xls",
            ".xlsx"
            };

        public UploadFileController(ILogger<UploadFileController> logger, IConfiguration configuration, DBConnection dbConnection)
            {
            _logger = logger;
            _configuration = configuration;
            _dbConnection = dbConnection;
            // Set the directory path where files will be uploaded
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PostCompliance_Evidences");
            _uploadReportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Audit_Report");
            _uploadPathAuditee = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Auditee_Evidences");
            _uploadPathCAU = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/CAU_Evidences");
            }

        private IActionResult InvalidModelStateResponse()
            {
            var endpointName = ControllerContext?.ActionDescriptor?.DisplayName ?? "Unknown";
            ValidationErrorHelper.LogValidationErrors(_logger, endpointName, ModelState);
            return BadRequest(ValidationErrorHelper.BuildInvalidRequestResponse(ModelState));
            }

        [HttpPost]
        public async Task<IActionResult> UploadFiles([FromForm] FileUploadRequest request)
            {
            if (!ModelState.IsValid || request?.Files == null)
                {
                return InvalidModelStateResponse();
                }

            try
                {
                if (!TryGetAuthorizedComplianceEvidencePath(request.Subfolder, out var uploadPath))
                    {
                    return Forbid();
                    }

                var validationResult = ValidateComplianceEvidenceUpload(request, uploadPath);
                if (validationResult != null)
                    {
                    return validationResult;
                    }

                Directory.CreateDirectory(uploadPath);

                foreach (var file in request.Files)
                    {
                    if (file.Length > 0)
                        {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                            await file.CopyToAsync(stream);
                            }
                        }
                    }

                return Json(new { success = true, message = "Files uploaded successfully" });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error uploading files");
                return Json(new { success = false, message = ex.Message });
                }
            }

        [HttpPost]
        [DisableRateLimiting]
        public IActionResult GetComplianceEvidenceUploadStatus(string subfolder)
            {
            if (!TryGetAuthorizedComplianceEvidencePath(subfolder, out var uploadPath))
                {
                return Forbid();
                }

            var existingSizeBytes = GetDirectoryFileSize(uploadPath);
            return Json(new
                {
                success = true,
                maxTotalBytes = ComplianceEvidenceSizeLimitBytes,
                existingSizeBytes
                });
            }

        [HttpPost]
        [DisableRateLimiting]
        public IActionResult GetComplianceEvidenceFiles(string subfolder)
            {
            if (!TryGetAuthorizedComplianceEvidencePath(subfolder, out var folderPath))
                {
                return Forbid();
                }

            var fileInfos = Directory.Exists(folderPath)
                ? Directory.EnumerateFiles(folderPath)
                    .Select(filePath => new FileInfo(filePath))
                    .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                : new List<FileInfo>();
            var files = fileInfos.Select(file => new { fileName = file.Name, sizeBytes = file.Length });

            return Json(new
                {
                success = true,
                files,
                totalSizeBytes = fileInfos.Sum(file => file.Length),
                maxTotalBytes = ComplianceEvidenceSizeLimitBytes
                });
            }

        [HttpPost]
        public async Task<IActionResult> UploadAuditeeEvideces([FromForm] FileUploadRequest request)
            {
            if (!ModelState.IsValid || request?.Files == null)
                {
                return InvalidModelStateResponse();
                }

            try
                {
                var uploadPath = Path.Combine(_uploadPathAuditee, request.Subfolder);
                if (!Directory.Exists(uploadPath))
                    {
                    Directory.CreateDirectory(uploadPath);
                    }

                foreach (var file in request.Files)
                    {
                    if (file.Length > 0)
                        {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                            await file.CopyToAsync(stream);
                            }
                        }
                    }

                return Json(new { success = true, message = "Files uploaded successfully" });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error uploading files");
                return Json(new { success = false, message = ex.Message });
                }
            }
        [HttpPost]
        public async Task<IActionResult> UploadFilesCAU([FromForm] FileUploadRequest request)
            {
            if (!ModelState.IsValid || request?.Files == null)
                {
                return InvalidModelStateResponse();
                }

            try
                {
                var uploadPath = Path.Combine(_uploadPathCAU, request.Subfolder);
                if (!Directory.Exists(uploadPath))
                    {
                    Directory.CreateDirectory(uploadPath);
                    }

                foreach (var file in request.Files)
                    {
                    if (file.Length > 0)
                        {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                            await file.CopyToAsync(stream);
                            }
                        }
                    }

                return Json(new { success = true, message = "Files uploaded successfully" });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error uploading files");
                return Json(new { success = false, message = ex.Message });
                }
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("UPLOADED_FILE_DELETED", "FILES_EVIDENCE", "FILE MANAGEMENT", "", "", ObjectType = "UPLOADED_FILE", ObjectId = "fileName", RequireResultMessage = true)]
        public IActionResult DeleteFile(string subFolder, string fileName)
            {
            try
                {
                if (!TryGetAuthorizedComplianceEvidencePath(subFolder, out var uploadPath))
                    {
                    return Forbid();
                    }

                var safeFileName = Path.GetFileName(fileName);
                if (string.IsNullOrWhiteSpace(safeFileName) || !string.Equals(safeFileName, fileName, StringComparison.Ordinal))
                    {
                    return BadRequest(new { success = false, message = "Invalid evidence file name." });
                    }

                var filePath = Path.Combine(uploadPath, safeFileName);
                if (System.IO.File.Exists(filePath))
                    {
                    System.IO.File.Delete(filePath);
                    return Json(new { success = true, message = "File deleted successfully." });
                    }
                else
                    {
                    return NotFound(new { success = false, message = "Evidence file was not found." });
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error deleting file");
                return Json(new { success = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult DeleteAuditeeEvidences(string subFolder, string fileName)
            {
            try
                {
                var uploadPath = Path.Combine(_uploadPathAuditee, subFolder);
                var filePath = Path.Combine(uploadPath, fileName);
                if (System.IO.File.Exists(filePath))
                    {
                    System.IO.File.Delete(filePath);

                    return Json(new { success = true, Message = "File deleted successfully." });
                    }
                else
                    {
                    return Json(new { success = false, Message = "" });
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error deleting file");
                return Json(new { success = false, message = ex.Message });
                }
            }
        [HttpPost]
        public IActionResult DeleteFileCAU(string subFolder, string fileName)
            {
            try
                {
                var uploadPath = Path.Combine(_uploadPathCAU, subFolder);
                var filePath = Path.Combine(uploadPath, fileName);
                if (System.IO.File.Exists(filePath))
                    {
                    System.IO.File.Delete(filePath);
                    return Json(new { success = true, Message = "File deleted successfully." });
                    }
                else
                    {
                    return Json(new { success = false, Message = "" });
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error deleting file");
                return Json(new { success = false, message = ex.Message });
                }
            }

        [HttpPost]
        public async Task<List<AuditeeResponseEvidenceModel>> GetFilesData(string subfolder)
            {
            try
                {
                var filesData = new List<AuditeeResponseEvidenceModel>();
                var folderPath = Path.Combine(_uploadPath, subfolder);
                if (!Directory.Exists(folderPath))
                    {
                    return filesData;
                    }


                var files = Directory.GetFiles(folderPath);

                foreach (var filePath in files)
                    {
                    var fileName = Path.GetFileName(filePath);
                    var fileType = Path.GetExtension(filePath).TrimStart('.'); // Get the file extension without the dot
                    var fileLength = new FileInfo(filePath).Length;

                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                        using (var memoryStream = new MemoryStream())
                            {
                            await fileStream.CopyToAsync(memoryStream);
                            var base64String = Convert.ToBase64String(memoryStream.ToArray());

                            filesData.Add(new AuditeeResponseEvidenceModel
                                {
                                FILE_NAME = fileName,
                                IMAGE_LENGTH = Convert.ToInt64(fileLength),
                                IMAGE_TYPE = fileType,
                                IMAGE_DATA = base64String
                                });
                            }
                        }
                    }

                return filesData;
                }
            catch (Exception ex)
                {
                var filesData = new List<AuditeeResponseEvidenceModel>();
                _logger.LogError(ex, "Error retrieving file data");
                return filesData;
                }
            }

        [HttpPost]
        public async Task<List<AuditeeResponseEvidenceModel>> GetAuditeeEvidenceData(string subfolder)
            {
            try
                {
                var filesData = new List<AuditeeResponseEvidenceModel>();
                var folderPath = Path.Combine(_uploadPathAuditee, subfolder);
                if (!Directory.Exists(folderPath))
                    {
                    return filesData;
                    }


                var files = Directory.GetFiles(folderPath);

                foreach (var filePath in files)
                    {
                    var fileName = Path.GetFileName(filePath);
                    var fileType = Path.GetExtension(filePath).TrimStart('.'); // Get the file extension without the dot
                    var fileLength = new FileInfo(filePath).Length;

                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                        using (var memoryStream = new MemoryStream())
                            {
                            await fileStream.CopyToAsync(memoryStream);
                            var base64String = Convert.ToBase64String(memoryStream.ToArray());

                            filesData.Add(new AuditeeResponseEvidenceModel
                                {
                                FILE_NAME = fileName,
                                IMAGE_LENGTH = Convert.ToInt64(fileLength),
                                IMAGE_TYPE = fileType,
                                IMAGE_DATA = base64String
                                });
                            }
                        }
                    }

                return filesData;
                }
            catch (Exception ex)
                {
                var filesData = new List<AuditeeResponseEvidenceModel>();
                _logger.LogError(ex, "Error retrieving file data");
                return filesData;
                }
            }

        public async Task<IActionResult> UploadAuditReport([FromForm] FileUploadRequest request)
            {
            if (!ModelState.IsValid || request?.Files == null)
                {
                return InvalidModelStateResponse();
                }

            try
                {
                var uploadPath = Path.Combine(_uploadReportPath, request.Subfolder);
                if (!Directory.Exists(uploadPath))
                    {
                    Directory.CreateDirectory(uploadPath);
                    }

                foreach (var file in request.Files)
                    {
                    if (file.Length > 0)
                        {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                            await file.CopyToAsync(stream);
                            }
                        }
                    }

                return Json(new { success = true, message = "Audit Report Uploaded successfully" });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error uploading audit report");
                return Json(new { success = false, message = ex.Message });
                }
            }

        [HttpPost]
        public IActionResult DeleteAuditReport(string subFolder, string fileName)
            {
            try
                {
                var uploadPath = Path.Combine(_uploadReportPath, subFolder);
                var filePath = Path.Combine(uploadPath, fileName);
                if (System.IO.File.Exists(filePath))
                    {
                    System.IO.File.Delete(filePath);
                    return Json(new { success = true, Message = "Audit Report deleted successfully." });
                    }
                else
                    {
                    return Json(new { success = false, Message = "" });
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error deleting Audit Report");
                return Json(new { success = false, message = ex.Message });
                }
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

        private IActionResult ValidateComplianceEvidenceUpload(FileUploadRequest request, string uploadPath)
            {
            if (request.Files.Count > ComplianceEvidenceFileLimit)
                {
                return BadRequest(new { success = false, message = $"A maximum of {ComplianceEvidenceFileLimit} files can be uploaded at once." });
                }

            var selectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in request.Files)
                {
                if (file == null || file.Length <= 0)
                    {
                    continue;
                    }

                var fileName = Path.GetFileName(file.FileName);
                if (string.IsNullOrWhiteSpace(fileName))
                    {
                    return BadRequest(new { success = false, message = "Invalid file name." });
                    }

                if (!selectedNames.Add(fileName))
                    {
                    return BadRequest(new { success = false, message = $"Duplicate file selected: {fileName}." });
                    }

                var extension = Path.GetExtension(fileName);
                if (!ComplianceEvidenceAllowedExtensions.Contains(extension))
                    {
                    return BadRequest(new { success = false, message = $"{fileName}: disallowed file format." });
                    }

                if (file.Length > ComplianceEvidenceSizeLimitBytes)
                    {
                    return BadRequest(new { success = false, message = ComplianceEvidenceSizeLimitMessage });
                    }

                var existingFilePath = Path.Combine(uploadPath, fileName);
                if (System.IO.File.Exists(existingFilePath))
                    {
                    return BadRequest(new { success = false, message = $"File already uploaded: {fileName}." });
                    }
                }

            var selectedSizeBytes = request.Files.Where(file => file != null && file.Length > 0).Sum(file => file.Length);
            var existingSizeBytes = GetDirectoryFileSize(uploadPath);
            if (existingSizeBytes + selectedSizeBytes > ComplianceEvidenceSizeLimitBytes)
                {
                return BadRequest(new { success = false, message = ComplianceEvidenceSizeLimitMessage });
                }

            return null;
            }

        private bool TryGetAuthorizedComplianceEvidencePath(string subfolder, out string uploadPath)
            {
            uploadPath = string.Empty;
            if (!int.TryParse(subfolder, out var complianceId) || complianceId <= 0)
                {
                return false;
                }

            var isAuthorized = _dbConnection.GetParasForComplianceByAuditee()
                .Any(compliance => string.Equals(compliance.COM_ID, complianceId.ToString(), StringComparison.Ordinal));
            if (!isAuthorized)
                {
                return false;
                }

            uploadPath = Path.Combine(_uploadPath, complianceId.ToString());
            return true;
            }

        private static long GetDirectoryFileSize(string folderPath)
            {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                return 0;
                }

            return Directory.EnumerateFiles(folderPath)
                .Select(filePath => new FileInfo(filePath).Length)
                .Sum();
            }
        }

    public class FileUploadRequest
        {
        [Required]
        public string Subfolder { get; set; } = string.Empty;

        [Required]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
        }
    }
