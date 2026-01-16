using System.Collections.Generic;

namespace AIS.Models
    {
    public class CatalogUploadPreview
        {
        public string CatalogType { get; set; }
        public string Token { get; set; }
        public int NewCount { get; set; }
        public int UpdatedCount { get; set; }
        public int InactiveCount { get; set; }
        public List<CatalogApiRecord> NewApiRecords { get; set; } = new List<CatalogApiRecord>();
        public List<CatalogApiRecord> UpdatedApiRecords { get; set; } = new List<CatalogApiRecord>();
        public List<CatalogApiRecord> InactiveApiRecords { get; set; } = new List<CatalogApiRecord>();
        public List<CatalogPageRecord> NewPageRecords { get; set; } = new List<CatalogPageRecord>();
        public List<CatalogPageRecord> UpdatedPageRecords { get; set; } = new List<CatalogPageRecord>();
        public List<CatalogPageRecord> InactivePageRecords { get; set; } = new List<CatalogPageRecord>();
        }

    public class CatalogApiRecord
        {
        public string ApiName { get; set; }
        public string ApiPath { get; set; }
        public string HttpMethod { get; set; }
        public string IsActive { get; set; }
        }

    public class CatalogPageRecord
        {
        public int PageId { get; set; }
        public string PageName { get; set; }
        public string PagePath { get; set; }
        public string IsActive { get; set; }
        }
    }
