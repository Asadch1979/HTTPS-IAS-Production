using System;
using System.Collections.Generic;

using AIS.Validation;
namespace AIS.Models
    {
    public class AssignedObservations
        {
        public int ID { get; set; }
        public int? RESP_ID { get; set; }
        public int OBS_ID { get; set; }
        public int OBS_TEXT_ID { get; set; }
        public int EDITABLE { get; set; }
        [PlainText]
        public string RESPONSE_ID { get; set; }
        [RichTextSanitize]
        public string GIST { get; set; }
        public int ASSIGNEDTO_ROLE { get; set; }
        public int ASSIGNEDBY { get; set; }
        public DateTime? ASSIGNED_DATE { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public DateTime? LASTUPDATEDDATE { get; set; }
        [PlainText]
        public string IS_ACTIVE { get; set; }
        [PlainText]
        public string REPLIED { get; set; }
        [RichTextSanitize]
        public string OBSERVATION_TEXT { get; set; }
        [PlainText]
        public string PROCESS { get; set; }
        [PlainText]
        public string SUB_PROCESS { get; set; }
        [PlainText]
        public string CHECKLIST_DETAIL { get; set; }
        [PlainText]
        public string VIOLATION { get; set; }
        [PlainText]
        public string NATURE { get; set; }
        [RichTextSanitize]
        public string REPLY_TEXT { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string STATUS_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        public int? CAN_REPLY { get; set; }
        [PlainText]
        public string MEMO_DATE { get; set; }
        [PlainText]
        public string MEMO_NUMBER { get; set; }
        [PlainText]
        public string MEMO_REPLY_DATE { get; set; }
        [PlainText]
        public string AUDIT_YEAR { get; set; }
        [PlainText]
        public string OPERATION_STARTDATE { get; set; }
        [PlainText]
        public string OPERATION_ENDDATE { get; set; }
        public List<ObservationResponsiblePPNOModel> RESPONSIBLE_PPNOs { get; set; }
        }
    }
