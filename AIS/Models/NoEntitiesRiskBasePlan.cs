using AIS.Validation;
namespace AIS.Models
    {
    public class NoEntitiesRiskBasePlan
        {


        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ENTITY_DESC { get; set; }
        [PlainText]
        public string ENTITY_RISK { get; set; }
        [PlainText]
        public string ENTITY_SIZE { get; set; }
        public int ENTITY_NO { get; set; }

        }
    }
