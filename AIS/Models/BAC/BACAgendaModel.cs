using AIS.Validation;
namespace AIS.Models
    {
    public class BACAgendaModel
        {
        public int ID { get; set; }
        [PlainText]
        public string MEMO_NO { get; set; }
        [PlainText]
        public string MEETING_NO { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string SUBJECT { get; set; }

        }
    }
