using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AIS.Models
{
    // Shared MVC/API contract. Dates are ISO calendar dates, never audit-period text.
    public sealed class ExecutiveDashboardFilterRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string SelectBy { get; set; } = "entityType";
        public int? DimensionId { get; set; }
        public int? ReportingOfficeId { get; set; }
        public int? EntityId { get; set; }
        public string View { get; set; }
        // Audit Para Snap-specific audit-portfolio selectors; not business hierarchy IDs.
        public string SnapDivision { get; set; }
        public int? SnapDepartmentId { get; set; }

        // Parent pages outside CIA accept dates only. Scoped API drilldowns retain
        // their existing independently validated organizational contract.
        public void ValidateParentPerspective(string perspective)
        {
            if (perspective != "CIA" && (SelectBy != "entityType" || DimensionId.HasValue ||
                ReportingOfficeId.HasValue || EntityId.HasValue))
                throw new ArgumentException("This dashboard accepts dates only; its hierarchy comes from the authenticated IAS scope.");
        }

        public ExecutiveDashboardDateRange Validate()
        {
            if (SelectBy != "entityType" && SelectBy != "auditedBy") throw new ArgumentException("Select Entity Type or Audited By.");
            if (DimensionId <= 0 || ReportingOfficeId <= 0 || EntityId <= 0) throw new ArgumentException("Filter IDs must be positive.");
            if (string.IsNullOrEmpty(StartDate) && string.IsNullOrEmpty(EndDate)) return null;
            if (!DateTime.TryParseExact(StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) ||
                !DateTime.TryParseExact(EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end) || start > end || end == DateTime.MaxValue.Date)
                throw new ArgumentException("Provide both dates in yyyy-MM-dd format, with Start Date no later than End Date.");
            return new ExecutiveDashboardDateRange { StartDate = start, EndDate = end };
        }

        public Dictionary<string, object> RouteValues(string view, int? groupRootId)
            => new Dictionary<string, object> { ["view"] = view, ["groupRootId"] = groupRootId,
                ["startDate"] = StartDate, ["endDate"] = EndDate, ["selectBy"] = SelectBy,
                ["dimensionId"] = DimensionId, ["reportingOfficeId"] = ReportingOfficeId, ["entityId"] = EntityId,
                ["snapDivision"] = SnapDivision, ["snapDepartmentId"] = SnapDepartmentId };
    }

    public sealed class ExecutiveDashboardDateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime EndExclusive => EndDate.AddDays(1);
    }

    public sealed class ExecutiveFilterOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public sealed class ExecutiveFilterEntity
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public int? EntityTypeId { get; set; }
        public string EntityTypeName { get; set; }
        public int? AuditedById { get; set; }
        public string AuditedByName { get; set; }
        public int? ReportingOfficeId { get; set; }
        public string ReportingOfficeName { get; set; }
    }

    public sealed class ExecutiveDashboardFilterOptions
    {
        public List<ExecutiveFilterOption> Dimensions { get; set; } = new();
        public List<ExecutiveFilterOption> ReportingOffices { get; set; } = new();
        public List<ExecutiveFilterOption> Entities { get; set; } = new();

        // Input is already intersected with the authenticated organizational scope.
        public static ExecutiveDashboardFilterOptions FromScopedEntities(IEnumerable<ExecutiveFilterEntity> entities, ExecutiveDashboardFilterRequest filters, Func<int, int, bool> withinReportingOffice = null)
        {
            filters.Validate();
            var rows = entities.ToList();
            var result = new ExecutiveDashboardFilterOptions();
            result.Dimensions = Options(rows.Select(e => filters.SelectBy == "entityType"
                ? new ExecutiveFilterOption { Id = e.EntityTypeId ?? 0, Name = e.EntityTypeName }
                : new ExecutiveFilterOption { Id = e.AuditedById ?? 0, Name = e.AuditedByName }));
            RequireOption(result.Dimensions, filters.DimensionId);
            if (filters.DimensionId.HasValue) rows = rows.Where(e => (filters.SelectBy == "entityType" ? e.EntityTypeId : e.AuditedById) == filters.DimensionId).ToList();
            result.ReportingOffices = Options(rows.Select(e => new ExecutiveFilterOption { Id = e.ReportingOfficeId ?? 0, Name = e.ReportingOfficeName }));
            RequireOption(result.ReportingOffices, filters.ReportingOfficeId);
            if (filters.ReportingOfficeId.HasValue) rows = rows.Where(e => withinReportingOffice == null ? e.ReportingOfficeId == filters.ReportingOfficeId : withinReportingOffice(e.EntityId, filters.ReportingOfficeId.Value)).ToList();
            result.Entities = Options(rows.Select(e => new ExecutiveFilterOption { Id = e.EntityId, Name = e.EntityName }));
            RequireOption(result.Entities, filters.EntityId);
            return result;
        }

        private static List<ExecutiveFilterOption> Options(IEnumerable<ExecutiveFilterOption> options)
            => options.Where(o => o.Id > 0).GroupBy(o => o.Id).Select(g => new ExecutiveFilterOption { Id = g.Key,
                Name = g.Select(o => o.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? g.Key.ToString(CultureInfo.InvariantCulture) }).OrderBy(o => o.Name).ThenBy(o => o.Id).ToList();
        private static void RequireOption(List<ExecutiveFilterOption> options, int? id)
        {
            if (id.HasValue && !options.Any(o => o.Id == id)) throw new ExecutiveDashboardAccessException("EntityScope", "A filter is outside the authorized scope or incompatible with its parent selection.");
        }
    }
}
