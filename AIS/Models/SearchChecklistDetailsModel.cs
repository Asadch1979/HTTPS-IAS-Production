using AIS.Validation;
namespace AIS.Models
    {
    public class SearchChecklistDetailsModel
        {

        public int ID { get; set; }
        [PlainText]
        public string PROCESS { get; set; }
        [PlainText]
        public string SUB_PROCESS { get; set; }
        [PlainText]
        public string PROCESS_DETAIL { get; set; }
        [PlainText]
        public string RISK { get; set; }

        }
    }
