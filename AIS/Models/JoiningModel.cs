using System;
using System.Collections.Generic;

using AIS.Validation;
namespace AIS.Models
    {
    public class JoiningModel
        {
        public int ENG_PLAN_ID { get; set; }
        public int ENTITY_ID { get; set; }
        public int ENTITY_CODE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string TEAM_NAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string SIZE { get; set; }
        [RichTextSanitize]
        public string AUDITEE_EMAIL { get; set; }
        [PlainText]
        public string AUDITEE_PHONE { get; set; }
        public DateTime? START_DATE { get; set; }
        public DateTime? END_DATE { get; set; }
        public List<JoiningTeamModel> TEAM_DETAILS { get; set; }

        }
    }
