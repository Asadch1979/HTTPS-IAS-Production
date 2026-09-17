using System;
using System.Collections.Generic;

using AIS.Validation;
namespace AIS.Models
    {
    public class ObservationResponseModel
        {
        public int? ID { get; set; }
        public int? AU_OBS_ID { get; set; }
        [RichTextSanitize]
        public string REPLY { get; set; }
        public int? REPLIEDBY { get; set; }
        public DateTime? REPLIEDDATE { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? LASTUPDATEDDATE { get; set; }
        public int? OBS_TEXT_ID { get; set; }
        public int? REPLY_ROLE { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string SUBMITTED { get; set; }
        public List<AuditeeResponseEvidenceModel> EVIDENCE_LIST { get; set; }

        }

    public class AuditeeReplySaveResult
        {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ObservationId { get; set; }
        public int EngagementId { get; set; }
        public int PreviousStatus { get; set; }
        public int NewStatus { get; set; }
        public int ResponseId { get; set; }
        public bool ReplyPersisted { get; set; }
        public bool StatusPersisted { get; set; }
        }
    }
