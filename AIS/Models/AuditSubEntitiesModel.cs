using AIS.Validation;
namespace AIS.Models
    {
    public class AuditSubEntitiesModel
        {
        public int E_ID { get; set; }
        public int ID { get; set; }
        [PlainText]
        public string NAME { get; set; }
        public int DIV_ID { get; set; }
        public int DEP_ID { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
