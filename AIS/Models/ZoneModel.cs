using AIS.Validation;
namespace AIS.Models
    {
    public class ZoneModel
        {
        public int ZONEID { get; set; }
        public int ENTITYID { get; set; }
        public int ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ZONECODE { get; set; }
        [PlainText]
        public string ZONENAME { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string ISACTIVE { get; set; }
        }
    }
