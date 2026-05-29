using AIS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models.CAU
    {
    public class CommercialAuditActionResultModel
        {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        }

    public class CommercialAuditWorkflowStage
        {
        [PlainText]
        public string Key { get; set; }
        [PlainText]
        public string Title { get; set; }
        [PlainText]
        public string Description { get; set; }
        }

    public class CommercialAuditWorkflowStep
        {
        public int StepNo { get; set; }
        [PlainText]
        public string StepKey { get; set; }
        [PlainText]
        public string StageKey { get; set; }
        [PlainText]
        public string Title { get; set; }
        [PlainText]
        public string Description { get; set; }
        [PlainText]
        public string PartialViewName { get; set; }
        }

    public class CommercialAuditWorkflowViewModel
        {
        [PlainText]
        public string CurrentStepKey { get; set; }
        public List<CommercialAuditWorkflowStep> Steps { get; set; } = new List<CommercialAuditWorkflowStep>();
        }

    public class CommercialAuditOmModel
        {
        public int? OmId { get; set; }
        [Required]
        public int? AuditYearId { get; set; }
        [PlainText]
        public string AuditYearText { get; set; }
        [Required]
        [MaxLength(100)]
        [PlainText]
        public string OmNo { get; set; }
        [Required]
        [MaxLength(1000)]
        [PlainText]
        public string GistOfOm { get; set; }
        [Required]
        
        [RichTextSanitize]
        public string BodyOfOm { get; set; }
        
        [RichTextSanitize]
        public string ManagementResponse { get; set; }
        [PlainText]
        public string IsActive { get; set; } = "Y";
        }

    public class CommercialAuditPdpModel
        {
        public int? PdpId { get; set; }
        [Required]
        public int? AuditYearId { get; set; }
        [PlainText]
        public string AuditYearText { get; set; }
        [Required]
        [MaxLength(100)]
        [PlainText]
        public string PdpNo { get; set; }
        [Required]
        [MaxLength(500)]
        [PlainText]
        public string GistOfPdp { get; set; }
        [Required]
        
        [RichTextSanitize]
        public string BodyOfPdp { get; set; }
        
        [RichTextSanitize]
        public string ManagementResponse { get; set; }
        
        [RichTextSanitize]
        public string DacRecommendations { get; set; }
        [RichTextSanitize]
        public string UpdateManagementResponse { get; set; }

        [RichTextSanitize]
        public string UpdatedStatus { get; set; }
        [PlainText]
        public string IsActive { get; set; } = "Y";
        public int LinkedOmCount { get; set; }
        [PlainText]
        public string LinkedOmNumbers { get; set; }
        }

    public class CommercialAuditPdpOmMappingModel
        {
        public int? MappingId { get; set; }
        public int? PdpId { get; set; }
        public int? OmId { get; set; }
        [PlainText]
        public string OmNo { get; set; }
        [PlainText]
        public string GistOfOm { get; set; }
        [PlainText]
        public string IsActive { get; set; } = "Y";
        }

    public class CommercialAuditPdpOmMappingSaveRequest
        {
        [Required]
        public int? PdpId { get; set; }
        public List<int> OmIds { get; set; } = new List<int>();
        [PlainText]
        public string IsActive { get; set; } = "Y";
        }

    public class CommercialAuditArpseHeaderModel
        {
        public int? ArpseId { get; set; }
        [Required]
        public int? ArpseYearId { get; set; }
        [PlainText]
        public string ArpseYearText { get; set; }
        [Required]
        [MaxLength(100)]
        [PlainText]
        public string ParaNo { get; set; }
        [Required]
        [MaxLength(500)]
        [PlainText]
        public string GistOfPara { get; set; }
        [Required]
        [RichTextSanitize]
        public string BodyOfPara { get; set; }
        
        [RichTextSanitize]
        public string ManagementResponse { get; set; }
        [PlainText]
        public string IsActive { get; set; } = "Y";
        public int LinkedPdpCount { get; set; }
        [PlainText]
        public string LinkedPdpNumbers { get; set; }
        }

    public class CommercialAuditArpsePdpMappingModel
        {
        public int? MappingId { get; set; }
        public int? ArpseId { get; set; }
        public int? PdpId { get; set; }
        [PlainText]
        public string PdpNo { get; set; }
        [PlainText]
        public string GistOfPdp { get; set; }
        [PlainText]
        public string IsActive { get; set; } = "Y";
        }

    public class CommercialAuditArpsePdpMappingSaveRequest
        {
        [Required]
        public int? ArpseId { get; set; }
        public List<int> PdpIds { get; set; } = new List<int>();
        [PlainText]
        public string IsActive { get; set; } = "Y";
        }

    public class CommercialAuditArpseDacEntryModel
        {
        public int? DacEntryId { get; set; }
        [Required]
        public int? ArpseId { get; set; }
        [Required]
        
        [RichTextSanitize]
        public string DacRecommendation { get; set; }
        public DateTime? DacDate { get; set; }
        [RichTextSanitize]
        public string UpdatedStatus { get; set; }
        [PlainText]
        public string IsActive { get; set; } = "Y";
        }

    public class CommercialAuditArpsePacEntryModel
        {
        public int? PacEntryId { get; set; }
        [Required]
        public int? ArpseId { get; set; }
        [Required]
        
        [RichTextSanitize]
        public string PacDirective { get; set; }
        public DateTime? PacDate { get; set; }
        [RichTextSanitize]
        public string UpdatedStatus { get; set; }
        [PlainText]
        public string IsActive { get; set; } = "Y";
        }
    }
