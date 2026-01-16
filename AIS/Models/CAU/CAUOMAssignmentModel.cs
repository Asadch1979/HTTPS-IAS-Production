using AIS.Validation;
namespace AIS.Models
    {
    public class CAUOMAssignmentModel
        {
        public int ID { get; set; }
        [PlainText]
        public string OM_NO { get; set; }
        public int INS_YEAR { get; set; }
        [PlainText]
        public string CONTENTS_OF_OM { get; set; }
        [PlainText]
        public string OM_REPLY { get; set; }
        public int DIV_ID { get; set; }
        public int STATUS { get; set; }
        [PlainText]
        public string STATUS_DES { get; set; }
        [PlainText]
        public string KEY { get; set; }
        }
    }
