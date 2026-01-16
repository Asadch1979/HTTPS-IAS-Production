using AIS.Validation;
namespace AIS.Models
    {
    public class CAUOMResponseModel
        {
        public int ID { get; set; }
        [PlainText]
        public string OM_NO { get; set; }
        [PlainText]
        public string CONTENTS_OF_OM { get; set; }
        [PlainText]
        public string RESPONSE_OF_OM { get; set; }
        public int DIV_ID { get; set; }
        public int DEPT_ID { get; set; }
        public int STATUS { get; set; }
        }
    }
