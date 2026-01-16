using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class PublicHolidayModel
        {
        public int? ID { get; set; }
        public DateTime? HOLIDAY_DATE { get; set; }
        public int? HOLIDAY_YEAR { get; set; }
        [PlainText]
        public string IS_WEEKEND { get; set; }  // "Y"/"N"
        [PlainText]
        public string IS_HOLIDAY { get; set; }  // "Y"/"N"
        [PlainText]
        public string HOLIDAY_NAME { get; set; }
        [PlainText]
        public string DAT { get; set; }  // "N" - Normal, "F" - Festival, "O" - Other
        }

    }
