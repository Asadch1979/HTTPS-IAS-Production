using AIS.Validation;
namespace AIS.Models
    {
    public class LoanSchemeYearlyModel
        {

        public int ENTITY_ID { get; set; }
        public int DISB_STATUSID { get; set; }

        public int GLSUBCODE { get; set; }
        [PlainText]
        public string GLSUBNAME { get; set; }


        public double DISBURSED_AMOUNT_2021 { get; set; }


        public double PRIN_OUT_2021 { get; set; }

        public double MARKUP_OUT_2021 { get; set; }
        public double DISBURSED_AMOUNT_2022 { get; set; }


        public double PRIN_OUT_2022 { get; set; }

        public double MARKUP_OUT_2022 { get; set; }


        }
    }
