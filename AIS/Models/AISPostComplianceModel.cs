using AIS.Validation;

namespace AIS.Models
    {
    public class AISPostComplianceModel
        {
        [PlainText]
        public string PNAME { get; set; }
        [PlainText]
        public string CNAME { get; set; }
        [PlainText]
        public string COMID { get; set; }
        [PlainText]
        public string OLDPARAID { get; set; }
        [PlainText]
        public string NEWPARAID { get; set; }
        [PlainText]
        public string AUDITPERIOD { get; set; }
        [PlainText]
        public string ENTITYID { get; set; }
        [PlainText]
        public string ENTITYCODE { get; set; }
        [PlainText]
        public string AUDITEDBY { get; set; }
        [PlainText]
        public string ENTITYTYPEID { get; set; }
        [PlainText]
        public string COMCYCLE { get; set; }
        [PlainText]
        public string COMSTATUS { get; set; }
        [PlainText]
        public string COMSTAGE { get; set; }
        [PlainText]
        public string PARASTATUS { get; set; }
        [PlainText]
        public string PARANO { get; set; }
        [RichTextSanitize]
        public string GISTOFPARAS { get; set; }
        [PlainText]
        public string SETTELEDON { get; set; }
        [PlainText]
        public string SETTELEDBY { get; set; }
        [PlainText]
        public string IND { get; set; }
        [PlainText]
        public string PARAADDEDON { get; set; }
        [PlainText]
        public string CAUSTATUS { get; set; }
        [PlainText]
        public string CAUASSIGNEDENTID { get; set; }
        [PlainText]
        public string BRRESPONSEBY { get; set; }
        [PlainText]
        public string BRRESPONSEON { get; set; }
        [PlainText]
        public string CAUASSIGNEDBY { get; set; }
        [PlainText]
        public string CAUASSIGNEDON { get; set; }
        [PlainText]
        public string SETTLEMENTCOMREVIEWEDBY { get; set; }
        [PlainText]
        public string SETTLEMENTCOMREVIEWEDON { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string ANNEX { get; set; }
        }
    }
