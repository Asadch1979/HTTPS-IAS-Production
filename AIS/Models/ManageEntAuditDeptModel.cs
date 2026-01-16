using AIS.Validation;
namespace AIS.Models
    {
    public class ManageEntAuditDeptModel
        {

        [PlainText]
        public string R_ID { get; set; }
        [PlainText]
        public string D_ID { get; set; }
        [PlainText]
        public string D_CODE { get; set; }
        [PlainText]
        public string D_NAME { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string CBAS_CODE { get; set; }
        [PlainText]
        public string ENT_ID { get; set; }
        [PlainText]
        public string AUD_ID { get; set; }
        [PlainText]
        public string AUDITOR { get; set; }


        }
    }
