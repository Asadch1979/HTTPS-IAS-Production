using AIS.Validation;
namespace AIS.Models
    {
    public class RoleRespModel
        {

        public int DESIGNATIONCODE { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
