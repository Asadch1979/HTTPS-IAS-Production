using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Models
{
    // This contract is separate from executive parents and Audit Para Snap.
    public sealed class CcoOrganizationFilter
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string GroupBy { get; set; } = "GROUP";
        public int? GroupId { get; set; }
        public int? DivisionId { get; set; }
        public int? GmOfficeId { get; set; }
        public int? RegionId { get; set; }
        public int? EntityId { get; set; }
        public int MaxRows { get; set; } = 500;
        public void Validate()
        {
            ComplianceDashboardPolicy.Validate(new ExecutiveDashboardFilterRequest { StartDate=StartDate, EndDate=EndDate });
            if (!new[]{"GROUP","DIVISION","GM"}.Contains(GroupBy) || MaxRows<1 || MaxRows>1000 ||
                GroupId<=0 || DivisionId<=0 || GmOfficeId<=0 || RegionId<=0 || EntityId<=0 ||
                (DivisionId.HasValue && !GroupId.HasValue) ||
                (GmOfficeId.HasValue && (!GroupId.HasValue || !DivisionId.HasValue)))
                throw new ArgumentException("Select valid dates, aggregation and organizational parents.");
        }
        public void ValidatePath(IEnumerable<CcoOrganizationPath> authorizedPaths)
        {
            Validate();
            if (!GroupId.HasValue && !DivisionId.HasValue && !GmOfficeId.HasValue && !RegionId.HasValue && !EntityId.HasValue) return;
            // The provider must already intersect these paths with live CCO scope.
            // Validate all supplied IDs against ONE path, never independently.
            if (authorizedPaths == null || !authorizedPaths.Any(p => p.IsVerified &&
                (!GroupId.HasValue || p.GroupId==GroupId) && (!DivisionId.HasValue || p.DivisionId==DivisionId) &&
                (!GmOfficeId.HasValue || p.GmOfficeId==GmOfficeId) && (!RegionId.HasValue || p.RegionId==RegionId) &&
                (!EntityId.HasValue || p.EntityId==EntityId)))
                throw new ArgumentException("The selected organizational path is missing, conflicting or outside the authorized scope.");
        }
    }
    public sealed class CcoOrganizationPath
    {
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int? GmOfficeId { get; set; }
        public string GmOfficeName { get; set; }
        public int? RegionId { get; set; }
        public int EntityId { get; set; }
        public bool IsVerified { get; set; }
    }
    public sealed class CcoOrganizationRow
    {
        public string RowKind { get; set; }
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int? GmOfficeId { get; set; }
        public string GmOfficeName { get; set; }
        public long? OpeningCases { get; set; }
        public long? ParaAdded { get; set; }
        public long? TotalCases { get; set; }
        public long? ParasSettled { get; set; }
        public long? Outstanding { get; set; }
        public decimal? SettlementRate { get; set; }
        public long? NetBacklogChange { get; set; }
        public long? HighRiskOpen { get; set; }
        public long? ZeroCycleOpen { get; set; }
        public long? Over3CycleOpen { get; set; }
        public long? AgedOver365Open { get; set; }
        public long? ReconciliationDifference { get; set; }
        public long? MappingExceptions { get; set; }
        public long? QualityExceptions { get; set; }
        public string HistoricalQuality { get; set; }
        public string MappingQuality { get; set; }
        public string PeriodStart { get; set; }
        public string PeriodEnd { get; set; }
    }
    public sealed class CcoOrganizationUnavailableException : Exception
    {
        public CcoOrganizationUnavailableException() : base("CCO organizational reporting awaits approved staging mappings, database scope and permissions.") { }
    }
}
