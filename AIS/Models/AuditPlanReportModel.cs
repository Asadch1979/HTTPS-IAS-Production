using AIS.Validation;
namespace AIS.Models
    {
    public class AuditPlanReportModel
        {
        [PlainText]
        public string AUDITEDBY { get; set; }
        [PlainText]
        public string PARRENTOFFICE { get; set; }
        [PlainText]
        public string AUDITEENAME { get; set; }
        public int? ENTITYCODE { get; set; }
        public int? ANTITYID { get; set; }
        [PlainText]
        public string LASTAUDITOPSENDATE { get; set; }
        [PlainText]
        public string ENTITYRISK { get; set; }
        [PlainText]
        public string ENTITYSIZE { get; set; }
        public int? NORMALDAYS { get; set; }
        public int? REVENUEDAYS { get; set; }
        public int? TRAVELDAY { get; set; }
        public int? DISCUSSIONDAY { get; set; }
        [PlainText]
        public string AUDITSTARTDATE { get; set; }
        [PlainText]
        public string AUDITENDDATE { get; set; }
        [PlainText]
        public string TNAME { get; set; }
        [PlainText]
        public string TEAMLEAD { get; set; }
        }
    }
