using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public sealed class LegacyParaRequest
    {
        [Range(0, long.MaxValue)] public long RequestId { get; set; }
        [Range(0, int.MaxValue)] public int Version { get; set; }
        [Range(1, int.MaxValue)] public int EntityTypeId { get; set; }
        [Range(1, int.MaxValue)] public int EntityId { get; set; }
        [Range(1, int.MaxValue)] public int NatureId { get; set; }
        [Range(1900, 2025)] public int AuditYear { get; set; }
        [Required, StringLength(100)] public string ParaNo { get; set; }
        [Required, StringLength(500)] public string Heading { get; set; }
        [Required, StringLength(2000000)] public string ObservationHtml { get; set; }
        [Range(1, int.MaxValue)] public int AnnexureId { get; set; }
        [Range(1, int.MaxValue)] public int CategoryId { get; set; }
        [Range(1, int.MaxValue)] public int SubChecklistId { get; set; }
        [Range(1, int.MaxValue)] public int ChecklistId { get; set; }
        public long? ReferenceId { get; set; }
        [Range(typeof(decimal), "0", "999999999999999.99")] public decimal Amount { get; set; }
        [Range(0, int.MaxValue)] public int Instances { get; set; }
        public List<LegacyParaPerson> ResponsiblePersons { get; set; } = new();
        public List<LegacyParaAttachment> Attachments { get; set; } = new();
    }
    public sealed class LegacyParaPerson
    {
        [Required, RegularExpression(@"\d{1,12}")] public string PpNo { get; set; }
        [StringLength(100)] public string Role { get; set; }
        [StringLength(250)] public string EmployeeName { get; set; }
        [StringLength(40)] public string LoanCase { get; set; }
        [StringLength(40)] public string AccountNumber { get; set; }
        [Range(typeof(decimal), "0", "999999999999999.99")] public decimal LoanAmount { get; set; }
        [Range(typeof(decimal), "0", "999999999999999.99")] public decimal AccountAmount { get; set; }
    }
    public sealed class LegacyParaAttachment
    {
        [Required, StringLength(200)] public string Name { get; set; }
        [Required, StringLength(100)] public string ContentType { get; set; }
        [Required, StringLength(7000000)] public string Base64 { get; set; }
    }
    public sealed class LegacyParaAction
    {
        [Range(1, long.MaxValue)] public long RequestId { get; set; }
        [Range(1, int.MaxValue)] public int Version { get; set; }
        [Required, RegularExpression("^(AUTHORIZE|RETURN|REJECT)$")] public string Action { get; set; }
        [StringLength(2000)] public string Remarks { get; set; }
    }
    public sealed class LegacyParaQuery
    {
        public long RequestId { get; set; }
        public bool Queue { get; set; }
        [StringLength(200)] public string Search { get; set; }
        [Range(0, 100000)] public int Offset { get; set; }
    }
    public sealed class LegacyParaLookup
    {
        [Required, RegularExpression("^(ENTITIES|SUBCHECKLIST|CHECKLIST|REFERENCES|PERSON|LC)$")] public string Kind { get; set; }
        public long ParentId { get; set; }
        [StringLength(200)] public string Search { get; set; }
    }
}
