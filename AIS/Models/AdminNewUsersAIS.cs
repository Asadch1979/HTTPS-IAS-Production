using AIS.Validation;
namespace AIS.Models
    {
    public class AdminNewUsersAIS
        {

        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string EMP_NAME { get; set; }
        [PlainText]
        public string DESIGNATION_CODE { get; set; }
        [PlainText]
        public string DESIGNATION { get; set; }
        [PlainText]
        public string EMPLOYEE_TYPE { get; set; }
        [PlainText]
        public string POSTING_TYPE { get; set; }
        [PlainText]
        public string CODE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }



        }
    }
