using AIS.Validation;
namespace AIS.Models
    {
    public class MergeDuplicateChecklistModel
        {
        public int ID { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string NEW_COUNT { get; set; }
        [PlainText]
        public string OLD_COUNT { get; set; }

        }
    }
