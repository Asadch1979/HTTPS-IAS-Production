using AIS.Validation;
namespace AIS.Models
    {
    public class GISTWiseReportParas
        {

        [PlainText]
        public string AUDIT_ZONE { get; set; }
        [PlainText]
        public string REGION { get; set; }
        [PlainText]
        public string BRANCH { get; set; }
        [PlainText]
        public string BRANCH_CODE { get; set; }
        [PlainText]
        public string E_DATE { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string ANNEX { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        [PlainText]
        public string NO_OF_INSTANCES { get; set; }
        [PlainText]
        public string AMOUNT_INVOLVED { get; set; }



        }
    }
