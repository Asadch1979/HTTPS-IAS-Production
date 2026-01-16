using AIS.Validation;
namespace AIS.Models
    {
    public class AddOldParasComplianceReply
        {

        public int? PPNO { get; set; }
        public int? PID { get; set; }
        [RichTextSanitize]
        public string REPLY { get; set; }



        }
    }
