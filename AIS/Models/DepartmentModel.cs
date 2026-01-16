using System.ComponentModel.DataAnnotations.Schema;

using AIS.Validation;
namespace AIS.Models
    {
    public class DepartmentModel
        {
        public int ID { get; set; }
        public int DIV_ID { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string CODE { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [NotMapped]
        [PlainText]
        public string DIV_NAME { get; set; }
        [NotMapped]
        [PlainText]
        public string HO_UNIT_NAME { get; set; }
        [NotMapped]
        [PlainText]
        public string AUDITED_BY_NAME { get; set; }
        public int AUDITED_BY_DEPID { get; set; }

        }
    }
