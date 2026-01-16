using System;

using AIS.Validation;
namespace AIS.Models
    {
    /// <summary>
    /// Represents a link between a para and its reference.
    /// Maps to TBL_PARA_REFERENCE_LINKS.
    /// </summary>
    public class ParaReferenceLinkModel
        {
        public int? LinkId { get; set; }
        public int EntityId { get; set; }
        public int OldParaId { get; set; }
        public int NewParaId { get; set; }
        public int ParaId { get; set; }
        public int ReferenceId { get; set; }
        [PlainText]
        public string ReferenceTitle { get; set; }
        public int? CreditManualId { get; set; }
        public int? OpManualId { get; set; }
        [PlainText]
        public string ManualType { get; set; }
        [PlainText]
        public string Chapter { get; set; }
        [PlainText]
        public string MatchedText { get; set; }
        [PlainText]
        public string LinkType { get; set; }
        /// <summary>
        /// Date on which the reference instructions were issued. This value is
        /// required when creating new manual or policy references so that the
        /// backend can persist the issuance date along with other metadata.
        /// </summary>
        public DateTime? InstructionsDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        }
    }
