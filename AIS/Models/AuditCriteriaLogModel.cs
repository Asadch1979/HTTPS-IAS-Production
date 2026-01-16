using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class AuditCriteriaLogModel
        {
        public int ID { get; set; }
        public int C_ID { get; set; }
        public int STATUS_ID { get; set; }
        public int CREATEDBY_ID { get; set; }
        public DateTime CREATED_ON { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        public int UPDATED_BY { get; set; }
        public DateTime LAST_UPDATED_ON { get; set; }
        }
    }
