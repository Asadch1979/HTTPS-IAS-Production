using AIS.Validation;
namespace AIS.Models
    {
    namespace AIS.Models
        {
        public class AuditeeOldParasPpnoModel
            {
            public int? ComId { get; set; }
            public int? OldParaId { get; set; }
            public int? NewParaId { get; set; }
        [PlainText]
            public string EntityName { get; set; }
        [PlainText]
            public string AuditPeriod { get; set; }
        [PlainText]
            public string Amount { get; set; }
        [PlainText]
            public string Annex { get; set; }
        [PlainText]
            public string ParaStatus { get; set; }    // Always 'Un-Settled' per query
        [PlainText]
            public string Ind { get; set; }
        [PlainText]
            public string ParaNo { get; set; }
        [PlainText]
            public string GistOfParas { get; set; }
            }
        }

    }
