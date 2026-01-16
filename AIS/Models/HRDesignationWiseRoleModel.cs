using AIS.Validation;
namespace AIS.Models
    {
    public class HRDesignationWiseRoleModel
        {
        public int ID { get; set; }
        [PlainText]
        public string DESIGNATION_CODE { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string ROLE_ID { get; set; }
        [PlainText]
        public string ROLE { get; set; }
        [PlainText]
        public string ENTITY_TYPE { get; set; }

        }
    }
