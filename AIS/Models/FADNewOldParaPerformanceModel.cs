using AIS.Validation;
namespace AIS.Models
    {
    public class FADNewOldParaPerformanceModel
        {
        [PlainText]
        public string D_ID { get; set; }
        [PlainText]
        public string Audit_Zone { get; set; }
        [PlainText]
        public string Checklist { get; set; }
        [PlainText]
        public string Process { get; set; }
        [PlainText]
        public string Sub_Checklist { get; set; }
        [PlainText]
        public string Checklist_Details { get; set; }
        [PlainText]
        public string Total_Paras { get; set; }
        [PlainText]
        public string Setteled_Para { get; set; }
        [PlainText]
        public string Unsetteled_Para { get; set; }
        [PlainText]
        public string Ratio { get; set; }
        [PlainText]
        public string R1 { get; set; }
        [PlainText]
        public string R2 { get; set; }
        [PlainText]
        public string R3 { get; set; }

        }
    }
