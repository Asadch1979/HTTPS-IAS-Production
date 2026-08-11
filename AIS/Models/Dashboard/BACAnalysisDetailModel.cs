using AIS.Validation;
using System;

namespace AIS.Models
    {
    public class BACAnalysisDetailModel
        {
        public int OBSERVATION_ID { get; set; }
        public int ENTITY_ID { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string ENTITY { get; set; }
        [PlainText]
        public string GIST { get; set; }
        public decimal NO_OF_INSTANCES { get; set; }
        public decimal AMOUNT { get; set; }
        [PlainText]
        public string PARA_STATUS { get; set; }
        public DateTime? ENTEREDDATE { get; set; }
        public DateTime? STELLED_ON { get; set; }
        public int STATUS { get; set; }
        public decimal DSA { get; set; }
        }
    }
