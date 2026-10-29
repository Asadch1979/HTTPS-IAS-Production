using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Models
{
    // Presentation entitlement, enforced server-side AFTER the secured Oracle
    // read has validated the complete result. Never omit exceptions from sums.
    // Calling clients cannot opt in to exception details with query parameters.
    public static class CcoReportVisibility
    {
        private static bool Begins(string value, string prefix) =>
            !string.IsNullOrWhiteSpace(value) &&
            value.Trim().StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        public static List<CcoFunctionPositionRow> FunctionRows(
            IEnumerable<CcoFunctionPositionRow> rows, bool isVerifiedSuperUser)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            return rows.Where(row => row != null &&
                !string.Equals(row.RowKind, "SUBTOTAL", StringComparison.OrdinalIgnoreCase) &&
                (isVerifiedSuperUser ||
                 string.Equals(row.RowKind, "TOTAL", StringComparison.OrdinalIgnoreCase) ||
                 (row.GroupId.HasValue && row.DivisionId.HasValue &&
                  !Begins(row.GroupName, "Unmapped / conflicting") &&
                  !Begins(row.DivisionName, "Unmapped / conflicting")))).ToList();
        }

        public static List<ExecutiveReportRow> ExecutiveRows(
            IEnumerable<ExecutiveReportRow> rows, bool isVerifiedSuperUser)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (isVerifiedSuperUser) return rows.ToList();
            return rows.Where(row => row != null &&
                (string.Equals(row.RowKind, "TOTAL", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(row.RowKind, "GRAND_TOTAL", StringComparison.OrdinalIgnoreCase) ||
                 (!Begins(row.GroupName, "Unassigned GM") &&
                  !Begins(row.ItemName, "Unassigned Region") &&
                  !Begins(row.GroupName, "Unmapped / conflicting") &&
                  !Begins(row.ItemName, "Unmapped / conflicting")))).ToList();
        }
    }
}
