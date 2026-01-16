using System;
using AIS.Validation;
namespace AIS.Models
    {
    public class CurrentActiveUsers
        {
        [PlainText]
        public string DEPARTMENT_NAME { get; set; }
        [PlainText]
        public string NAME { get; set; }
        public int PP_NUMBER { get; set; }
        public DateTime? LOGGED_IN_DATE { get; set; }
        [PlainText]
        public string SESSION_TIME { get; set; }
        }
    }
