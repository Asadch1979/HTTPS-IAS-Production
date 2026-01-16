using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AIS.Validation;

namespace AIS.Models.HD
    {
    public class SbpObservationModel
        {
        public long ParaId { get; set; }
        [PlainText]
        public string Ref_No { get; set; }
        [PlainText]
        public string Function_Name { get; set; }
        [PlainText]
        public string Para_No { get; set; }
        [RichTextSanitize]
        public string SBP_Observation { get; set; }
        [RichTextSanitize]
        public string SBP_Directions { get; set; }
        [PlainText]
        public string Compliance_Quarter { get; set; }
        [PlainText]
        public string Created_By { get; set; }
        public DateTime Created_On { get; set; }
        }

    public class SBPObservationCreateModel
    {
        public long? ParaId { get; set; }

        [PlainText]
        public string RefNo { get; set; }

        [PlainText]
        public string FunctionName { get; set; }

        [PlainText]
        public string ParaNo { get; set; }

        [RichTextSanitize]
        public string SBPObservation { get; set; }

        [RichTextSanitize]
        public string SBPDirections { get; set; }

        [PlainText]
        public string ComplianceQuarter { get; set; }

        [JsonPropertyName("observation_type_id")]
        public int? ObservationTypeId { get; set; }

        [PlainText]
        public string User { get; set; }
    }

    public class SbpObservationResponseModel
        {
        public int Response_Id { get; set; }
        public long ParaId { get; set; }
        [PlainText]
        public string Ref_No { get; set; }
        [RichTextSanitize]
        public string Bank_Response { get; set; }
        public DateTime Reply_Date { get; set; }
        [PlainText]
        public string Compliance_Status { get; set; }
        [PlainText]
        public string IAD_Validation { get; set; }
        [PlainText]
        public string Entered_By { get; set; }
        public DateTime Entered_On { get; set; }
        }

    public class SBPObservationResponseCreateModel
        {
        public long ParaId { get; set; }
        [PlainText]
        public string RefNo { get; set; }

        [RichTextSanitize]
        public string BankResponse { get; set; }
        public DateTime? ReplyDate { get; set; }
        [PlainText]
        public string ComplianceStatus { get; set; }

        [PlainText]
        public string IADValidation { get; set; }

        [PlainText]
        public string User { get; set; }
        }

    public class SBPObservationResponseUpdateModel : SBPObservationResponseCreateModel
        {
        [JsonPropertyName("response_id")]
        public long ResponseId { get; set; }
        }

    public class SbpObservationRegisterModel
        {
        public long ParaId { get; set; }
        [PlainText]
        public string Ref_No { get; set; }
        [PlainText]
        public string Function_Name { get; set; }
        [PlainText]
        public string Para_No { get; set; }
        [RichTextSanitize]
        public string SBP_Observation { get; set; }
        [RichTextSanitize]
        public string SBP_Directions { get; set; }
        [PlainText]
        public string Compliance_Quarter { get; set; }
        [PlainText]
        public string Latest_Bank_Response { get; set; }
        public DateTime? Reply_Date { get; set; }
        [PlainText]
        public string Compliance_Status { get; set; }
        [PlainText]
        public string IAD_Validation { get; set; }
        [PlainText]
        public string Observation_Type { get; set; }
        }

    public class SBPObservationRegisterItem
    {
        public long ParaId { get; set; }
        [PlainText]
        public string RefNo { get; set; }
        [PlainText]
        public string FunctionName { get; set; }
        [PlainText]
        public string ParaNo { get; set; }
        [RichTextSanitize]
        public string SBPObservation { get; set; }
        [RichTextSanitize]
        public string SBPDirections { get; set; }
        [PlainText]
        public string ComplianceQuarter { get; set; }
        [PlainText]
        public string LatestBankResponse { get; set; }
        public long? LatestResponseId { get; set; }
        public DateTime? ReplyDate { get; set; }
        [PlainText]
        public string ComplianceStatus { get; set; }
        [PlainText]
        public string IADValidation { get; set; }
        [PlainText]
        public string EnteredBy { get; set; }
        public DateTime? EnteredOn { get; set; }
        public DateTime? CreatedOn { get; set; }

        public long? ObservationTypeId { get; set; }
        [PlainText]
        public string ObservationTypeName { get; set; }

        [JsonIgnore]
        [PlainText]
        public string ObservationType
        {
            get => ObservationTypeName;
            set => ObservationTypeName = value;
        }
    }

    public class SbpObservationHistoryModel
        {
        public long ParaId { get; set; }
        [PlainText]
        public string Ref_No { get; set; }
        [PlainText]
        public string Function_Name { get; set; }
        [PlainText]
        public string Para_No { get; set; }
        [RichTextSanitize]
        public string SBP_Observation { get; set; }
        [RichTextSanitize]
        public string SBP_Directions { get; set; }
        public DateTime Reply_Date { get; set; }
        [RichTextSanitize]
        public string Bank_Response { get; set; }
        [PlainText]
        public string Compliance_Status { get; set; }
        [PlainText]
        public string IAD_Validation { get; set; }
        [PlainText]
        public string Entered_By { get; set; }
        public DateTime Entered_On { get; set; }
        }

    public class SBPObservationHistoryModel
        {
        public long ParaId { get; set; }
        [PlainText]
        public string RefNo { get; set; }
        [PlainText]
        public string FunctionName { get; set; }
        [PlainText]
        public string ParaNo { get; set; }
        [RichTextSanitize]
        public string SBPObservation { get; set; }
        [RichTextSanitize]
        public string SBPDirections { get; set; }
        public List<SBPObservationHistoryItem> Responses { get; set; } = new List<SBPObservationHistoryItem>();
        }

    public class SBPObservationHistoryItem
        {
        public long ResponseId { get; set; }
        public long ParaId { get; set; }
        [RichTextSanitize]
        public string BankResponse { get; set; }
        public DateTime? ReplyDate { get; set; }
        [PlainText]
        public string ComplianceStatus { get; set; }
        [PlainText]
        public string IADValidation { get; set; }
        [PlainText]
        public string EnteredBy { get; set; }
        public DateTime? EnteredOn { get; set; }
        }

    public class SBPPasswordValidationResult
        {
        public bool Success { get; set; }
        [PlainText]
        public string Message { get; set; }
        }

    public class SbpPasswordUpdateRequest
        {
        [JsonPropertyName("newPassword")]
        [PlainText]
        public string NewPassword { get; set; }

        [JsonPropertyName("password")]
        [PlainText]
        public string LegacyPassword
            {
            get => NewPassword;
            set => NewPassword = value;
            }

        [JsonPropertyName("updatedBy")]
        [PlainText]
        public string UpdatedBy { get; set; }

        [JsonPropertyName("updated_by")]
        [PlainText]
        public string UpdatedByLegacy
            {
            get => UpdatedBy;
            set => UpdatedBy = value;
            }
        }
    }
