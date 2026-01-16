using System.ComponentModel.DataAnnotations.Schema;

using AIS.Validation;
namespace AIS.Models
    {
    public class RiskActivityModel
        {
        public int ACTIVITY_ID { get; set; }
        public int S_GR_ID { get; set; }
        public int MAX_NUMBER { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [NotMapped]
        [PlainText]
        public string SUB_GROUP_DESC { get; set; }

        }
    }
