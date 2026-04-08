using System.Collections.Generic;

namespace AIS.Models
    {
    public class AuditCriteriaPersistResult
        {
        public bool Success { get; set; }
        public string Message { get; set; }
        }

    public class AuditCriteriaRowResponseModel
        {
        public int RowIndex { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AuditPeriod { get; set; }
        public string EntityName { get; set; }
        public string Risk { get; set; }
        public string Size { get; set; }
        public string Frequency { get; set; }
        }

    public class AuditCriteriaSaveResponseModel
        {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<AuditCriteriaRowResponseModel> Rows { get; set; } = new List<AuditCriteriaRowResponseModel>();
        }
    }
