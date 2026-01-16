using AIS.Validation;
namespace AIS.Models
    {
    public class AuditEmployeeModel
        {
        public int PPNO { get; set; }
        public int DEPARTMENTCODE { get; set; }
        [PlainText]
        public string DEPTARMENT { get; set; }
        [PlainText]
        public string EMPLOYEEFIRSTNAME { get; set; }
        [PlainText]
        public string EMPLOYEELASTNAME { get; set; }
        public int RANKCODE { get; set; }
        [PlainText]
        public string CURRENT_RANK { get; set; }
        public int DESIGNATIONCODE { get; set; }
        [PlainText]
        public string FUN_DESIGNATION { get; set; }
        [PlainText]
        public string TYPE { get; set; }
        [PlainText]
        public string TASK_ALLOCATED { get; set; }
        }
    }
