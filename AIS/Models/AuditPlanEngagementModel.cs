using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class AuditPlanEngagementModel
        {

        [PlainText]
        public string AUDITPERIOD { get; set; }
        [PlainText]
        public string PARENT_OFFICE { get; set; }
        [PlainText]
        public string ENITIY_NAME { get; set; }
        public DateTime? AUDIT_STARTDATE { get; set; }
        public DateTime? AUDIT_ENDDATE { get; set; }
        public int? TRAVEL_DAY { get; set; }
        public int? REVENUE_RECORD_DAY { get; set; }
        public int? DISCUSSION_DAY { get; set; }
        [PlainText]
        public string TEAM_NAME { get; set; }
        [PlainText]
        public string STATUS { get; set; }


        }
    }
