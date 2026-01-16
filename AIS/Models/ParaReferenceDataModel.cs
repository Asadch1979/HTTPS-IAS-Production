using System.Collections.Generic;

using AIS.Validation;
namespace AIS.Models
    {
    public class ParaReferenceDataModel
        {
        [PlainText]
        public string ParaText { get; set; }
        public List<int> References { get; set; }
        public List<AuditChecklistAnnexureCircularModel> ReferenceDetails { get; set; }
        /// <summary>
        /// Raw link records for the para.
        /// </summary>
        public List<ParaReferenceLinkModel> ReferenceLinks { get; set; }
        }
    }
