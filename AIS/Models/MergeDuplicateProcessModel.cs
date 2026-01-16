using AIS.Validation;
namespace AIS.Models
    {
    public class MergeDuplicateProcessModel
        {
        public int ID { get; set; }
        public int M_ID { get; set; }
        [PlainText]
        public string HEADING { get; set; }

        }
    }
