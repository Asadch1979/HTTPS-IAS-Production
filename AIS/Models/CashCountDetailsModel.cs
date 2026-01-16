using AIS.Validation;
namespace AIS.Models
    {
    public class CashCountDetailsModel
        {
        [PlainText]
        public string ID { get; set; }
        [PlainText]
        public string DENOMINATION_VAULT { get; set; }
        [PlainText]
        public string NO_CURRENCY_NOTES_VAULT { get; set; }
        [PlainText]
        public string TOTAL_AMOUNT_VAULT { get; set; }
        [PlainText]
        public string DENOMINATION_SAFE_REGISTER { get; set; }
        [PlainText]
        public string NO_CURRENCY_NOTES_SAFE_REGISTER { get; set; }
        [PlainText]
        public string TOTAL_AMOUNT_SAFE_REGISTER { get; set; }
        [PlainText]
        public string DIFFERENCE { get; set; }

        }
    }
