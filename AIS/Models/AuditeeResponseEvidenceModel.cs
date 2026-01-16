using AIS.Validation;
namespace AIS.Models
    {
    public class AuditeeResponseEvidenceModel
        {
        public int? TEXT_ID { get; set; }
        public int? COM_ID { get; set; }
        public int? SEQUENCE { get; set; }
        public int? LENGTH { get; set; }
        public long? IMAGE_LENGTH { get; set; }
        [PlainText]
        public string IMAGE_TYPE { get; set; }
        [PlainText]
        public string IMAGE_DATA { get; set; }
        [PlainText]
        public string IMAGE_NAME { get; set; }
        [PlainText]
        public string FILE_NAME { get; set; }
        public bool? COVER_IMAGE { get; set; }
        [PlainText]
        public string RESPONSE_ID { get; set; }
        [PlainText]
        public string FILE_ID { get; set; }


        }
    }
