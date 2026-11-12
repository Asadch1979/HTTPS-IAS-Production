# GROUP Report E EntityScope investigation

The existing production warning is ambiguous: both an application
ExecutiveDashboardAccessException and Oracle ORA-20802 become EntityScope.
It cannot establish which failed for user 6090. No production SQL was run.

## Request path

1. ExecutiveReportsController.GetReportE calls Execute, which resolves the
   authenticated scope. Role 4 requires one non-conflicting Type 18 posting;
   Role 14 requires Type 3. Ordinary client root overrides are rejected.
2. Exact page, API and Report E permissions remain enforced.
3. NarrowExecutiveDashboardScope validates any entity filter against the
   authenticated root and descendants from PKG_ORG_STRUCTURE.P_GET_NODES.
4. GetExecutiveReportE calls ReadExecutiveReport, which revalidates session,
   root and entity filter, then binds scope.GroupRootId as P_SCOPE_ROOT_ID.
   A selected descendant never replaces the immutable reporting root.
5. P_MANAGEMENT_SETTLEMENT calls OPEN_REPORT('E'), then VALIDATE_REQUEST.
   ORA-20802 can mean invalid scope parameters, an invalid/duplicate root,
   or an out-of-scope/missing/duplicate entity or reporting-office filter.
6. Report SQL limits rows to that root and reporting_1 through reporting_6.

## Source evidence and remaining verification

V_EXEC_ENTITY_SCOPE includes the entity itself at hierarchy level zero.
With one active category 18 mapping, BUSINESS_GROUP_ID should therefore equal
that Group's own entity ID. The current package also permits a verified
category 3 Division without requiring its parent Group ID to equal the root.
No repository evidence justifies removing the Group equality check.

The application reads PKG_ORG_STRUCTURE.P_GET_NODES, whose implementation is
not present in the repository. Equivalence with the deployed report view
cannot be established without the DBA source and data diagnostics. The GROUP
page working does not prove that the report view or selected filters agree.

The earlier commit e54f0ed8 corrected a proven client mismatch: ordinary report
requests sent groupRootId, which the resolver rejects. Confirm that deployment
and refreshed JavaScript first; do not assume it explains subsequent denials.

New internal logs distinguish Application from Oracle / ORA-20802 and record
the stage, authenticated root, filters and trace identifier. These details are
not added to the HTTP response. The existing generic 403 remains unchanged.

## Deployment / diagnostics

Deploy the controller logging change with the prior report-client fix.
No Oracle package or view replacement is required by this commit.
Run group_report_e_scope_diagnostics.sql manually with the authenticated
session root and role plus exact request filters (NULL if absent). It is
read-only. Missing, duplicate or conflicting mappings require investigation,
not automatic approval. Compare deployed source before proposing a data repair.

Three outdated regression checks were updated: role 4/14 posting validation,
PKG_LG page permission Group ID binding, and controller Group ID bindings.
Tests and builds were not executed; source diffs were reviewed only.
