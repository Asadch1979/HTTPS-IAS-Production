using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class ParaTextModel
        {
        public int? MEMO_NO { get; set; }
        public int? TEXT_ID { get; set; }
        [RichTextSanitize]
        public string MEMO_TXT { get; set; }
        [RichTextSanitize]
        public string BRANCH_REPLY { get; set; }
        [RichTextSanitize]
        public string CAU_INSTRUCTION { get; set; }
        public int ComId { get; set; }
        public int EntityId { get; set; }
        public int OldParaId { get; set; }
        public int NewParaId { get; set; }
        [PlainText]
        public string AuditPeriod { get; set; }
        [PlainText]
        public string ParaStatus { get; set; }
        [PlainText]
        public string AuditedBy { get; set; }
        [PlainText]
        public string Risk { get; set; }
        [PlainText]
        public string IND { get; set; }
        [PlainText]
        public string ParaNo { get; set; }
        public DateTime ParaAddedOn { get; set; }
        [PlainText]
        public string GistOfParas { get; set; }
        [RichTextSanitize]
        public string Text { get; set; }
        [RichTextSanitize]
        public string ParaText { get; set; }

        }
    }
