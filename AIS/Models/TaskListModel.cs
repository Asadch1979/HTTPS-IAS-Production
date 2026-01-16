using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class TaskListModel
        {
        public int ID { get; set; }
        public int ENG_PLAN_ID { get; set; }
        public int TEAM_ID { get; set; }
        public int SEQUENCE_NO { get; set; }
        public int TEAMMEMBER_PPNO { get; set; }
        public int ENTITY_ID { get; set; }
        public int ENTITY_CODE { get; set; }
        public int ENTITY_TYPE { get; set; }
        [PlainText]
        public string ENTITY_TYPE_DESC { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string AUDIT_START_DATE { get; set; }
        [PlainText]
        public string AUDIT_END_DATE { get; set; }
        public int STATUS_ID { get; set; }
        [PlainText]
        public string ENG_STATUS { get; set; }
        [PlainText]
        public string ENG_NEXT_STATUS { get; set; }
        [PlainText]
        public string ISACTIVE { get; set; }
        [PlainText]
        public string TEAM_NAME { get; set; }
        [PlainText]
        public string EMP_NAME { get; set; }
        [PlainText]
        public string WORKING_PAPER { get; set; }
        [PlainText]
        public string PRE_INFO { get; set; }
        [PlainText]
        public string AUDIT_YEAR { get; set; }
        [PlainText]
        public string ISCLOSE { get; set; }
        public DateTime OPERATION_STARTDATE { get; set; }
        public DateTime OPERATION_ENDDATE { get; set; }
        [PlainText]
        public string REPORTING { get; set; }

        }
    }
