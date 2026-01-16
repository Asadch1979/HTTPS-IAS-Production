using System.ComponentModel.DataAnnotations.Schema;
using AIS.Validation;
namespace AIS.Models
    {
    public class AuditTeamModel
        {
        public int ID { get; set; }
        public int T_ID { get; set; }
        [PlainText]
        public string CODE { get; set; }
        [PlainText]
        public string NAME { get; set; }
        public int AUDIT_DEPARTMENT { get; set; }
        public int TEAMMEMBER_ID { get; set; }
        [PlainText]
        public string IS_TEAMLEAD { get; set; }
        [NotMapped]
        [PlainText]
        public string EMPLOYEENAME { get; set; }
        [NotMapped]
        [PlainText]
        public string PLACE_OF_POSTING { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        }
    }
