using AIS.Models;
using System;
using System.Linq;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        internal AuditParaSnapPerformance GetAuditParaSnapPerformance(
            ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            ValidateSnapScope(scope);
            AuditParaSnapPolicy.Validate(filters);
            var result = new AuditParaSnapPerformance();
            // Reuse the existing lifecycle procedure rather than duplicating its
            // status-9, same-period settlement or Issue Rectified definitions.
            // Bounded to the verified direct children of the approved parents.
            var divisions = filters.SnapDivision == "1" ? new[] { "FAD", "MAD" } : new[] { filters.SnapDivision };
            foreach (var division in divisions)
            {
                var departments = GetAuditParaSnapDepartments(scope, division,
                    filters.SnapDivision == "1" ? null : filters.SnapDepartmentId);
                if (departments.Count > 100)
                    throw new InvalidOperationException("Audit-office comparison exceeds its approved bound.");
                foreach (var department in departments.Where(d => !filters.SnapDepartmentId.HasValue || d.Id == filters.SnapDepartmentId))
                    Add("department", division, department.Id, department.Name);
                Add("subtotal", division, filters.SnapDepartmentId, "Selected portfolio subtotal");
            }
            Add("total", filters.SnapDivision, filters.SnapDepartmentId, "Overall selected portfolio");
            ValidateSnapScope(scope);
            return result;

            void Add(string kind, string division, int? department, string name)
            {
                var selected = new ExecutiveDashboardFilterRequest
                {
                    StartDate=filters.StartDate, EndDate=filters.EndDate, View=filters.View,
                    SelectBy=filters.SelectBy, DimensionId=filters.DimensionId,
                    ReportingOfficeId=filters.ReportingOfficeId, EntityId=filters.EntityId,
                    SnapDivision=division, SnapDepartmentId=department
                };
                result.Rows.Add(new AuditParaSnapPerformanceRow { RowKind=kind, Division=division,
                    DepartmentId=department, Department=name, Position=GetAuditParaSnapKpis(scope,selected) });
            }
        }
    }
}
