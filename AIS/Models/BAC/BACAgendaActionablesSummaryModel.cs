using AIS.Validation;
namespace AIS.Models
    {
    public class BACAgendaActionablesSummaryModel
        {
        public int TOTAL { get; set; }
        [PlainText]
        public string MEETING_NO { get; set; }
        [PlainText]
        public string COMPLETED { get; set; }
        [PlainText]
        public string UN_COMPLETED { get; set; }
        [PlainText]
        public string REFERENCE { get; set; }
        [PlainText]
        public string RESPONSIBLES { get; set; }
        [PlainText]
        public string MANAGEMENT_RESPONSE { get; set; }
        [PlainText]
        public string CIA_REMARKS { get; set; }
        }
    }
