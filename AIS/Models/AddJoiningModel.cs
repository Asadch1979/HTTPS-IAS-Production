using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class AddJoiningModel
        {
        public int ID { get; set; }
        public int ENG_PLAN_ID { get; set; }
        public int TEAM_MEM_PPNO { get; set; }
        public DateTime? JOINING_DATE { get; set; }
        public DateTime? COMPLETION_DATE { get; set; }
        public DateTime? ENTEREDDATE { get; set; }
        public DateTime? LASTUPDATEDDATE { get; set; }
        public int ENTEREDBY { get; set; }
        public int LASTUPDATEDBY { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
