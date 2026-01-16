using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class StaffPositionModel
        {
        [PlainText]
        public string EMPLOYEE_NAME { get; set; }
        [PlainText]
        public string QUALIFICATION { get; set; }

        [PlainText]
        public string DESIGNATION { get; set; }

        [PlainText]
        public string RANK_DESC { get; set; }

        [PlainText]
        public string PLACE_OF_POSTING { get; set; }

        public int PPNO { get; set; }

        public DateTime? DATE_OF_POSTING { get; set; }

        }
    }
