using AIS.Validation;
namespace AIS.Models
    {
    public class AnnexureExerciseStatus
        {
        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string AUDIT_ZONE { get; set; }
        [PlainText]
        public string TOTAL { get; set; }
        [PlainText]
        public string PENDING { get; set; }
        [PlainText]
        public string COMPLETED { get; set; }
        }
    }
