using System;
using System.Text.Json.Serialization;
using AIS.Validation;

namespace AIS.Models.Requests
    {
    public class SbpObservationRequest
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

    public class SbpObservationResponseRequest
        {
        public long? ParaId { get; set; }

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

    public class SbpObservationResponseUpdateRequest : SbpObservationResponseRequest
        {
        [JsonPropertyName("response_id")]
        public long? ResponseId { get; set; }
        }
    }
