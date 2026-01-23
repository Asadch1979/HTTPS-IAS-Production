# Package Cleanup Report

## Before Inventory (Package References)

### Core Web App (AIS)
- HtmlSanitizer 9.0.889 (usage found in validation helpers).
- ExcelDataReader 3.6.0 (usage found in import workflows).
- ExcelDataReader.DataSet 3.6.0 (usage found alongside ExcelDataReader).
- itext7 9.5.0 (usage found in PDF controllers).
- itext7.pdfhtml 9.5.0 (usage found in PDF controllers for HtmlConverter).
- Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation 8.0.2 (usage found in Startup configuration).
- Microsoft.EntityFrameworkCore.SqlServer 9.0.0-rc.1.24451.1 (no usage found in code search; preview package for net8 app).
- Microsoft.EntityFrameworkCore.Tools 9.0.0-rc.1.24451.1 (no usage found in code search; tooling-only package).
- Newtonsoft.Json 13.0.3 (direct reference; usage not found in code search).
- Oracle.ManagedDataAccess.Core 23.5.1 (usage found in DB connection classes).
- Portable.itextsharp.xmlworker 5.5.13.5 (no usage found in code search; legacy iText 5 XMLWorker).
- WebEssentials.AspNetCore.PWA 1.0.85 (direct reference; usage not found in code search).

### Doc Generator (AIS.DocGen)
- ExcelDataReader 3.6.0 (usage found in doc generator tool).
- ExcelDataReader.DataSet 3.6.0 (usage found alongside ExcelDataReader).
- Oracle.ManagedDataAccess.Core 23.5.1 (usage found in doc generator tool).

## Removed Packages
- Portable.itextsharp.xmlworker 5.5.13.5 (legacy iText 5 XMLWorker package unused in code; replaced by iText 7 HTML-to-PDF workflow).
- Microsoft.EntityFrameworkCore.SqlServer 9.0.0-rc.1.24451.1 (unused in code search and preview package for net8 app).
- Microsoft.EntityFrameworkCore.Tools 9.0.0-rc.1.24451.1 (unused tooling-only package and tied to preview EF Core version).

## Final Package Versions (After Cleanup)

### Core Web App (AIS)
- HtmlSanitizer 9.0.889
- ExcelDataReader 3.6.0
- ExcelDataReader.DataSet 3.6.0
- itext7 9.5.0
- itext7.pdfhtml 9.5.0
- Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation 8.0.2
- Newtonsoft.Json 13.0.3
- Oracle.ManagedDataAccess.Core 23.5.1
- WebEssentials.AspNetCore.PWA 1.0.85

### Doc Generator (AIS.DocGen)
- ExcelDataReader 3.6.0
- ExcelDataReader.DataSet 3.6.0
- Oracle.ManagedDataAccess.Core 23.5.1

## Updated Packages
- itext7.pdfhtml 9.5.0 (upgraded from 5.0.5 to align with iText 9 and keep HtmlConverter support).

## Supporting Dependencies Added
- itext7.pdfhtml 9.5.0 (required by the iText Html2Pdf API used for PDF generation).

## Updated Files
- AIS/AIS.csproj (removed unused EF Core + XMLWorker packages, added itext7.pdfhtml).

## Validation
- `dotnet clean AIS.sln` (failed: .NET SDK not available in this environment).
- `dotnet build AIS.sln` (not run; .NET SDK not available in this environment).
- Login flow, impacted pages, and PDF generation flow not executed (runtime environment not available).
