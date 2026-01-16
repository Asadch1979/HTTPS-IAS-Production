using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class AuditEngagementPlanModel
        {
        public int? PLAN_ID { get; set; }
        public int? ENG_ID { get; set; }
        public int? PERIOD_ID { get; set; }
        public int? ENTITY_TYPE { get; set; }
        public int? AUDIT_BY_ID { get; set; }
        public DateTime? AUDIT_STARTDATE { get; set; }
        public DateTime? AUDIT_ENDDATE { get; set; }
        public DateTime? OP_STARTDATE { get; set; }
        public DateTime? OP_ENDDATE { get; set; }
        public int? CREATEDBY { get; set; }
        public DateTime? CREATED_ON { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? LASTUPDATEDDATE { get; set; }
        [PlainText]
        public string TEAM_NAME { get; set; }
        public int? STATUS { get; set; }
        public int? TEAM_ID { get; set; }
        public int? ENTITY_ID { get; set; }
        public int? ENTITY_CODE { get; set; }
        public int? TRAVELDAY { get; set; }
        public int? RRDAY { get; set; }
        public int? D_Day { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string REMARKS_OUT { get; set; }
        [PlainText]
        public string IS_SUCCESS { get; set; }
        [PlainText]
        public string COMMENTS { get; set; }


        }
    }
