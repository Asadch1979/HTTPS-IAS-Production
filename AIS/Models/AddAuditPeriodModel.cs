using System.Collections.Generic;
using AIS.Validation;
namespace AIS.Models
    {
    public class AddAuditPeriodModel
        {
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string STARTDATE { get; set; }
        [PlainText]
        public string ENDDATE { get; set; }
        public List<int> DEPARTMENT_IDS { get; set; }
        }
    }
