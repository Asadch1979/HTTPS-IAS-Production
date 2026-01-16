using AIS.Validation;
namespace AIS.Models
    {
    public class BACAgendaActionablesModel
        {
        public int ID { get; set; }
        [PlainText]
        public string MEETING_NO { get; set; }
        [PlainText]
        public string ITEM_HEADING { get; set; }
        [PlainText]
        public string BAC_DIRECTION { get; set; }
        [PlainText]
        public string ASSIGN_TO { get; set; }
        [PlainText]
        public string TIMELINE { get; set; }
        [PlainText]
        public string OPEN_TIMELINE { get; set; }
        [PlainText]
        public string DUE_DATE { get; set; }
        [PlainText]
        public string REPORT_FREQUENCY { get; set; }
        [PlainText]
        public string ENTERED_BY { get; set; }
        [PlainText]
        public string ENTERED_ON { get; set; }
        [PlainText]
        public string DELAY { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
