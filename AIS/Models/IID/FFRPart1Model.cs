using AIS.Validation;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AIS.Models.IID
    {
    public class FFRPart1Model
        {
        public int ComplaintId { get; set; }
        public int? FfrId { get; set; }
        [PlainText]
        public string PertainsTo { get; set; }
        [PlainText]
        public string FieldType { get; set; }
        public int? HOUnitTypeId { get; set; }
        public int? HOUnitId { get; set; }
        public int? RegionId { get; set; }
        public int? BranchId { get; set; }
        [PlainText]
        public string Source { get; set; }
        [PlainText]
        public string SourceOtherText { get; set; }
        [PlainText]
        public string Nature { get; set; }
        [PlainText]
        public string ReferenceNo { get; set; }
        [PlainText]
        public string FFRDate { get; set; }
        [PlainText]
        public string IncidentDate { get; set; }
        [PlainText]
        public string IncidentVenue { get; set; }
        [PlainText]
        public string IncidentNarrative { get; set; }
        [PlainText]
        public string ComplainantName { get; set; }
        [PlainText]
        public string ComplainantCNIC { get; set; }
        [PlainText]
        public string AccountNo { get; set; }
        [PlainText]
        public string ComplainantMobile { get; set; }
        [PlainText]
        public string ComplainantAddress { get; set; }
        [PlainText]
        public string MainAccused { get; set; }
        [PlainText]
        public string CoAccused { get; set; }
        [PlainText]
        public string Accusations { get; set; }
        [PlainText]
        public string ApproachAdopted { get; set; }
        [PlainText]
        public string RecordScrutinized { get; set; }
        [PlainText]
        public string RootCause { get; set; }
        [PlainText]
        public string KeyFindings { get; set; }
        [PlainText]
        public string ClearRecommendations { get; set; }
        [PlainText]
        public string AttachmentsPath { get; set; }
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
        }
    }
