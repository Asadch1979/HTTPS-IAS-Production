using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class DuplicateDeleteManageParaModel
        {
        public int DId { get; set; }


        public int OldParaId { get; set; }


        public int NewParaId { get; set; }


        [PlainText]
        public string EntityId { get; set; }
        [PlainText]
        public string EntityName { get; set; }


        [PlainText]
        public string EntityCode { get; set; }
        [PlainText]
        public string ParaGist { get; set; }
        [PlainText]
        public string AuditPeriod { get; set; }


        [PlainText]
        public string AuditedBy { get; set; }


        [PlainText]
        public string ParaNo { get; set; }


        [PlainText]
        public string ParaStatus { get; set; }


        [PlainText]
        public string Ind { get; set; }


        [PlainText]
        public string Risk { get; set; }


        [PlainText]
        public string Instances { get; set; }


        [PlainText]
        public string Amount { get; set; }


        [PlainText]
        public string Annex { get; set; }


        [PlainText]
        public string AddedBy { get; set; }


        public DateTime AddedOn { get; set; }


        public bool AuthorizedStatus { get; set; }


        [PlainText]
        public string AuthorizedBy { get; set; }


        public DateTime? AuthorizedOn { get; set; }


        [PlainText]
        public string Remarks { get; set; }

        }
    }
