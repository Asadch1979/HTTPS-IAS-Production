using AIS.Validation;
namespace AIS.Models
    {
    public class FindUserModel
        {

        public int? PPNUMBER { get; set; }

        [PlainText]
        public string LOGINNAME { get; set; }

        [RichTextSanitize]
        public string EMAIL { get; set; }
        [PasswordText]
        public string PASSWORD { get; set; }
        public int? DIVISIONID { get; set; }
        public int? DEPARTMENTID { get; set; }
        public int? ZONEID { get; set; }
        public int? BRANCHID { get; set; }
        public int? GROUPID { get; set; }
        public int? ENTITYID { get; set; }

        }
    }
