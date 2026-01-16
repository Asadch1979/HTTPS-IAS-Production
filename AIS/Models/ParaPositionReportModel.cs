using AIS.Validation;
namespace AIS.Models
    {
    public class ParaPositionReportModel
        {
        [PlainText]
        public string P_NAME { get; set; }
        [PlainText]
        public string C_NAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string Total_Paras { get; set; }
        [PlainText]
        public string Setteled_Para { get; set; }
        [PlainText]
        public string Unsetteled_Para { get; set; }
        }
    }
