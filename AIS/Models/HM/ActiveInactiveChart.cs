using AIS.Validation;
namespace AIS.Models
    {
    public class ActiveInactiveChart
        {
        [PlainText]
        public string Active_Count { get; set; }
        [PlainText]
        public string Inactive_Count { get; set; }

        }
    }
