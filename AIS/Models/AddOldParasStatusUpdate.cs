using AIS.Validation;
namespace AIS.Models
    {
    public class AddOldParasStatusUpdate
        {
        public int? PPNO { get; set; }
        public int PID { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string R_STATUS { get; set; }



        }
    }
