using AIS.Validation;
namespace AIS.Models
    {
    public class AuditConcludingEntitiesModel
        {
        public int? ENG_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }

        public int? TYPE_ID { get; set; }

        }
    }
