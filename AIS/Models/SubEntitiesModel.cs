using System.ComponentModel.DataAnnotations.Schema;

using AIS.Validation;
namespace AIS.Models
    {
    public class SubEntitiesModel
        {
        public int ID { get; set; }
        [PlainText]
        public string NAME { get; set; }
        public int DIV_ID { get; set; }
        public int DEP_ID { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [NotMapped]
        [PlainText]
        public string Division_Name { get; set; }
        [NotMapped]
        [PlainText]
        public string Department_Name { get; set; }

        }
    }
