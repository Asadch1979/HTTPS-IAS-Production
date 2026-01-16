using AIS.Validation;
namespace AIS.Models
    {
    public class ParaPositionDetailsModel
        {

        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string PARA_STATUS { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string HEADING { get; set; }

        }
    }
