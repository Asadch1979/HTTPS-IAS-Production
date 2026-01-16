using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using AIS.Models.HD;

using AIS.Validation;
namespace AIS.Models.HM
{
    public class SbpObservationPasswordModel
    {
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [PlainText]
        public string Password { get; set; }
    }

    public class SbpObservationTypeOption
    {
        [JsonPropertyName("observation_type_id")]
        public int ObservationTypeId { get; set; }

        [JsonPropertyName("observation_type_code")]
        [PlainText]
        public string ObservationTypeCode { get; set; }

        [JsonPropertyName("observation_type_name")]
        [PlainText]
        public string ObservationTypeName { get; set; }

        [JsonPropertyName("active_flag")]
        [PlainText]
        public string ActiveFlag { get; set; }

        [JsonPropertyName("sort_order")]
        public int? SortOrder { get; set; }

        [JsonIgnore]
        public bool IsActive => string.Equals(ActiveFlag, "Y", StringComparison.OrdinalIgnoreCase);
    }

    public class SbpObservationRegisterViewModel
    {
        public bool IsUnlocked { get; set; }

        [PlainText]
        public string StatusMessage { get; set; }

        [PlainText]
        public string PasswordError { get; set; }

        public IReadOnlyList<SBPObservationRegisterItem> Observations { get; set; }
            = new List<SBPObservationRegisterItem>();

        public IReadOnlyList<SbpObservationTypeOption> ObservationTypes { get; set; }
            = new List<SbpObservationTypeOption>();

        public int? SelectedObservationTypeId { get; set; }

        public bool HasRecords => Observations != null && Observations.Count > 0;

        public SbpObservationTypeOption GetSelectedObservationType()
        {
            if (ObservationTypes == null || !SelectedObservationTypeId.HasValue)
            {
                return null;
            }

            return ObservationTypes.FirstOrDefault(option => option?.ObservationTypeId == SelectedObservationTypeId);
        }
    }
}
