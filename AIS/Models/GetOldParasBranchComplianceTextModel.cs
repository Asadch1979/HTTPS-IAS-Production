using System.Collections.Generic;

using AIS.Validation;
namespace AIS.Models
    {
    public class GetOldParasBranchComplianceTextModel
        {

        [RichTextSanitize]
        public string PARA_TEXT { get; set; }
        [RichTextSanitize]
        public string OBS_TEXT { get; set; }
        [PlainText]
        public string PARA_TEXT_ID { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARA { get; set; }
        public List<ObservationResponsiblePPNOModel> RESPONSIBLE_PPs { get; set; }
        public List<ObservationResponsiblePPNOModel> UPDATED_RESPONSIBLE_PPs_BY_IMP { get; set; }
        public List<AuditeeResponseEvidenceModel> EVIDENCES { get; set; }

        }
    }
