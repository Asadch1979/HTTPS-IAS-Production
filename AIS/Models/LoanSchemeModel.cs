using AIS.Validation;
namespace AIS.Models
    {
    public class LoanSchemeModel
        {

        public int ENTITY_ID { get; set; }
        public int DISB_STATUSID { get; set; }

        public int GLSUBCODE { get; set; }
        [PlainText]
        public string GLSUBNAME { get; set; }


        public double DISBURSED_AMOUNT { get; set; }


        public double PRIN_OUT { get; set; }

        public double MARKUP_OUT { get; set; }


        }
    }
