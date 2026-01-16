using AIS.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
    {
    public class AuditCCQModel
        {
        public int ID { get; set; }
        public int ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        [StringLength(1000)]
        public string QUESTIONS { get; set; }
        public int CONTROL_VIOLATION_ID { get; set; }
        [PlainText]
        public string CONTROL_VIOLATION { get; set; }
        public int RISK_ID { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        public int CREATED_BY { get; set; }
        public int UPDATED_BY { get; set; }
        public DateTime? CREATED_DATETIME { get; set; }
        public DateTime? UPDATED_DATETIME { get; set; }

        }
    }
