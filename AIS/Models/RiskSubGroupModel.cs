using System.ComponentModel.DataAnnotations.Schema;

using AIS.Validation;
namespace AIS.Models
    {
    public class RiskSubGroupModel
        {
        public int GR_ID { get; set; }
        public int S_GR_ID { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [NotMapped]
        [PlainText]
        public string GROUP_DESC { get; set; }


        }
    }
