using System.Collections.Generic;
using System.Text.Json.Serialization;

using AIS.Validation;
namespace AIS.Models.Reports
    {
    public class AuditZoneItem
        {
        [JsonPropertyName("zone_id")]
        public int ZoneId { get; set; }

        [JsonPropertyName("zone_name")]
        [PlainText]
        public string ZoneName { get; set; } = string.Empty;
        }

    public class DepartmentPerformanceRequest
        {
        [JsonPropertyName("ent_id")]
        public int? EntId { get; set; }

        [JsonPropertyName("start_date")]
        [PlainText]
        public string StartDate { get; set; } = string.Empty;

        [JsonPropertyName("end_date")]
        [PlainText]
        public string EndDate { get; set; } = string.Empty;
        }

    public class DepartmentPerformanceByZoneRequest : DepartmentPerformanceRequest
        {
        [JsonPropertyName("zone_id")]
        public int? ZoneId { get; set; }
        }

    public class AuditorPerformanceRequest : DepartmentPerformanceRequest
        {
        [JsonPropertyName("zone_id")]
        public int? ZoneId { get; set; }
        }

    public class DeptPerfSummaryRow
        {
        [JsonPropertyName("opening_balance")]
        public decimal OpeningBalance { get; set; }

        [JsonPropertyName("added")]
        public decimal Added { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("settled")]
        public decimal Settled { get; set; }

        [JsonPropertyName("outstanding")]
        public decimal Outstanding { get; set; }

        [JsonPropertyName("high")]
        public decimal High { get; set; }

        [JsonPropertyName("low")]
        public decimal Low { get; set; }

        [JsonPropertyName("medium")]
        public decimal Medium { get; set; }
        }

    public class DeptPerfDetailRow
        {
        [JsonPropertyName("entity_audited")]
        [PlainText]
        public string EntityAudited { get; set; } = string.Empty;

        [JsonPropertyName("coso")]
        [PlainText]
        public string Coso { get; set; } = string.Empty;

        [JsonPropertyName("high")]
        public decimal High { get; set; }

        [JsonPropertyName("medium")]
        public decimal Medium { get; set; }

        [JsonPropertyName("low")]
        public decimal Low { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("settled")]
        public decimal Settled { get; set; }

        [JsonPropertyName("final")]
        public decimal Final { get; set; }

        [JsonPropertyName("days_delay")]
        public decimal DaysDelay { get; set; }
        }

    public class DeptPerfByZoneRow
        {
        [JsonPropertyName("entity_audited")]
        [PlainText]
        public string EntityAudited { get; set; } = string.Empty;

        [JsonPropertyName("anexx")]
        [PlainText]
        public string Anexx { get; set; } = string.Empty;

        [JsonPropertyName("high")]
        public decimal High { get; set; }

        [JsonPropertyName("medium")]
        public decimal Medium { get; set; }

        [JsonPropertyName("low")]
        public decimal Low { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("settled")]
        public decimal Settled { get; set; }

        [JsonPropertyName("delayed_start")]
        public decimal DelayedStart { get; set; }

        [JsonPropertyName("delayed_exit")]
        public decimal DelayedExit { get; set; }

        [JsonPropertyName("delay_final_report")]
        public decimal DelayFinalReport { get; set; }
        }

    public class DepartmentPerformanceSummaryDetailResponse
        {
        [JsonPropertyName("summary")]
        public DeptPerfSummaryRow Summary { get; set; } = new DeptPerfSummaryRow();

        [JsonPropertyName("detail")]
        public List<DeptPerfDetailRow> Detail { get; set; } = new List<DeptPerfDetailRow>();
        }

    public class AuditorPerformanceRow
        {
        [JsonPropertyName("ppno")]
        [PlainText]
        public string Ppno { get; set; } = string.Empty;

        [JsonPropertyName("auditor_name")]
        [PlainText]
        public string AuditorName { get; set; } = string.Empty;

        [JsonPropertyName("entity_audited")]
        [PlainText]
        public string EntityAudited { get; set; } = string.Empty;

        [JsonPropertyName("audited_as")]
        [PlainText]
        public string AuditedAs { get; set; } = string.Empty;

        [JsonPropertyName("annexure_coso")]
        [PlainText]
        public string AnnexureCoso { get; set; } = string.Empty;

        [JsonPropertyName("high")]
        public decimal High { get; set; }

        [JsonPropertyName("medium")]
        public decimal Medium { get; set; }

        [JsonPropertyName("low")]
        public decimal Low { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("settled")]
        public decimal Settled { get; set; }

        [JsonPropertyName("outstanding")]
        public decimal Outstanding { get; set; }

        [JsonPropertyName("performance")]
        [PlainText]
        public string Performance { get; set; } = string.Empty;
        }
    }
