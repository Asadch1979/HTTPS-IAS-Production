using AIS.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
    {
    public class ManageObservations
        {
        public int OBS_ID { get; set; }
        public int ENG_ID { get; set; }
        [PlainText]
        public string ANNEXURE_ID { get; set; }
        [PlainText]
        public string ANNEXURE_CODE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string PROCESS { get; set; }
        [PlainText]
        public string PROCESS_ID { get; set; }
        [PlainText]
        public string SUB_PROCESS { get; set; }
        [PlainText]
        public string SUB_PROCESS_ID { get; set; }
        [PlainText]
        public string Checklist_Details { get; set; }
        [PlainText]
        public string Checklist_Details_Id { get; set; }
        [PlainText]
        public string VIOLATION { get; set; }
        [PlainText]
        public string NATURE { get; set; }
        public int MEMO_NO { get; set; }
        public int DRAFT_PARA_NO { get; set; }
        public int FINAL_PARA_NO { get; set; }
        [RichTextSanitize]
        public string OBS_TEXT { get; set; }
        [RichTextSanitize]
        public string OBS_REPLY { get; set; }
        [PlainText]
        [StringLength(500)]
        public string HEADING { get; set; }
        [RichTextSanitize]
        public string AUD_REPLY { get; set; }
        [RichTextSanitize]
        public string HEAD_REPLY { get; set; }
        public int OBS_RISK_ID { get; set; }
        [PlainText]
        public string OBS_RISK { get; set; }
        public int OBS_STATUS_ID { get; set; }
        [PlainText]
        public string OBS_STATUS { get; set; }
        [PlainText]
        public string PERIOD { get; set; }
        public int? NO_OF_INSTANCES { get; set; }
        [PlainText]
        public string AMOUNT { get; set; }
        [PlainText]
        public string PPNO_TEST { get; set; }
        [PlainText]
        public string INDICATOR { get; set; }
        [PlainText]
        public string TYPE_INDICATOR { get; set; }
        [RichTextSanitize]
        public string DSA { get; set; }
        public List<ObservationResponsiblePPNOModel> RESPONSIBLE_PPs { get; set; }
        public List<AuditeeResponseEvidenceModel> ATTACHED_EVIDENCES { get; set; }
        }
    }
