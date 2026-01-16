using System.ComponentModel.DataAnnotations.Schema;

using AIS.Validation;
namespace AIS.Models
    {
    public class AuditPlanModel
        {
        public int? PLAN_ID { get; set; }
        public int? AUDITPERIOD_ID { get; set; }
        public int? AUDIT_CONDUCTBY_DEPT { get; set; }
        public int? NO_OF_DAYS_AUDIT { get; set; }
        public int? AUDITZONE_ID { get; set; }
        public int? BRANCH_ID { get; set; }
        public int? DIVISION_ID { get; set; }
        public int? DEPARTMENT_ID { get; set; }
        public int? RISK_LEVEL_ID { get; set; }
        public int? BRANCH_SIZE_ID { get; set; }
        public int? PLAN_STATUS_ID { get; set; }
        public int? SUB_ENTITY_ID { get; set; }

        [NotMapped]
        [PlainText]
        public string DEPARTMENT_NAME { get; set; }
        [PlainText]
        public string BRANCH_NAME { get; set; }
        [PlainText]
        public string DIVISION_NAME { get; set; }
        [PlainText]
        public string AUDITZONE_NAME { get; set; }
        }
    }
