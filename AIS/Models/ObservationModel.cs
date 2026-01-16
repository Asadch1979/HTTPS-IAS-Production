using AIS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
    {
    public class ObservationModel
        {
        public int ID { get; set; }

        public int ENG_ID { get; set; }
        public int FINAL_PARA_NO { get; set; }
        public int? BRANCH_ID { get; set; }
        [RichTextSanitize]
        public string OBSERVATION_TEXT { get; set; }
        [PlainText]
        [StringLength(500)]
        public string HEADING { get; set; }
        [PlainText]
        public string OBSERVATION_TEXT_PLAIN { get; set; }
        public int ENGPLANID { get; set; }
        [PlainText]
        public string ANNEXURE_ID { get; set; }
        public int STATUS { get; set; }
        [PlainText]
        public string STATUS_NAME { get; set; }
        public int ENTEREDBY { get; set; }
        public DateTime ENTEREDDATE { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public DateTime LASTUPDATEDDATE { get; set; }
        [PlainText]
        public string AMOUNT_INVOLVED { get; set; }
        public int REPLYBY { get; set; }
        public DateTime REPLYDATE { get; set; }
        public int LASTREPLYBY { get; set; }
        public DateTime LASTREPLYDATE { get; set; }
        public DateTime MEMO_DATE { get; set; }
        public int SEVERITY { get; set; }
        public int MEMO_NUMBER { get; set; }
        [PlainText]
        public string RESPONSIBILITY_ASSIGNED { get; set; }
        [PlainText]
        public string DSA_ISSUED { get; set; }
        public int TRANSACTION_ID { get; set; }
        public int RISKMODEL_ID { get; set; }
        public int? PROCESS_ID { get; set; }
        public int? SUBCHECKLIST_ID { get; set; }
        public int? CHECKLISTDETAIL_ID { get; set; }
        public int V_CAT_ID { get; set; }
        [PlainText]
        public string NO_OF_INSTANCES { get; set; }
        public int V_CAT_NATURE_ID { get; set; }
        public int? OTHER_ENTITY_ID { get; set; }
        [PlainText]
        public string TEAM_LEAD { get; set; }
        [RichTextSanitize]
        public string AUDITEE_REPLY { get; set; }
        [RichTextSanitize]
        public string AUDITOR_RECOM { get; set; }
        [RichTextSanitize]
        public string HEAD_RECOM { get; set; }
        [RichTextSanitize]
        public string QA_RECOM { get; set; }
        [RichTextSanitize]
        [StringLength(4000)]
        public string QA_GIST { get; set; }
        public List<ObservationResponsiblePPNOModel> RESPONSIBLE_PPNO { get; set; }

        }
    }
