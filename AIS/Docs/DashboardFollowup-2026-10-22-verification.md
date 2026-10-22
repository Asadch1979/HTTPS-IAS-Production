# Dashboard follow-up verification — 22 October 2026

Local follow-up to implementation commit `831e7e7efae3815a7cc84a461141b8aef7c9fa7e` on `main` is complete. No production connection, schema deployment, permission grant or remote push was performed.

## Registration

The current workbook differed from the older checkpoint: its populated registration list ended at row 256 and contained neither page ID 1000401 nor a HeadAuditDepartment route. Added one canonical entry at Sheet1 row 257 in `AIS/wwwroot/Images/Page ID.xlsx`:

- Page ID / ID: 1000401. Menu: 16, matching the existing Dashboard/entity_wise_obs registration.
- Name: Head of Audit Dashboard. Path/key: HeadAuditDepartment/AuditTeamsMonitoring. URL: /HeadAuditDepartment/AuditTeamsMonitoring.
- Active, visible, no submenu. No user or API grants added.
- The existing PageIdResolver still maps both AuditTeamsMonitoring and Review to 1000401. Review has no separate workbook registration.

The original worksheet contains 368 MB of uncompressed preformatted XML. Used a bounded copy for Artifact Tool authoring and visual review, then transplanted only the authored values/formula into the existing blank row. Independently reopened the final workbook with the bundled Python reader. Verified the canonical fields and uniqueness. ZIP comparison proved all archive entries other than sheet1.xml unchanged; replacing the new row with the original recovered sheet1.xml byte-for-byte. All existing registrations, styles, formulas, filters and workbook metadata were preserved. A first verification caught string-cell encoding during transplantation; corrected it and reran the final checks successfully. Artifact Tool produced previews and the authoring export but reported a native shutdown error; the independently validated final workbook is the deliverable.

## Focused source review

- Head Audit MVC/API entry points retain authentication, revoked-session, explicit page1000401, API permission and input validation gates. Database readers derive user/role/entity/context from the session, verify the actual page allocation, and recheck context after reading.
- Oracle REQUIRE_SCOPE validates active user/context and root membership. V_HEAD_AUDIT_ENG_SCOPE uses audit-performing AUDITBY_ID through the distinct office hierarchy. No parent/sibling or bank-wide fallback was introduced.
- Plan details preserve unengaged approved plans using a left join; any engagement must match the plan's performing office, period and authorized engagement scope. Plan KPI counts remain distinct from detail rows for multiple engagements.
- Work, evidence, response evidence and report procedures independently recheck engagement membership and bind the requested document to that engagement. Missing or unauthorized documents produce 403 in the controller.
- Assignment and history names resolve through the existing employee view without multiplying result rows. Observations select one latest text row per observation. Saved editor text remains plain, Razor-encoded content, not executable HTML.
- GROUP/RLM activity dates are fixed server-side to Pakistan January 1 through yesterday. Oracle validates the same window; role queries calculate it independently. Opening stock is retained for reconciliation; risk/aging/closing stock is estimated as of yesterday. Unknown historical workflow status is not fabricated. Historical estimates cannot reconstruct unlogged reopenings and retain the existing data-quality caveat.
- Export assets/buttons are gated by authenticated Super User status; server report-PDF paths additionally enforce it. Excel uses cells only; PNG/PDF capture behavior and existing page/data permissions remain unchanged.
- F8 working-tree changes were line-ending/index-refresh artifacts, with no content differences. Refreshed Git's index without altering SQL definitions.

No additional application changes were necessary after this review. The successful build, focused model tests and synthetic browser tests recorded in the 21 October checkpoint still apply to the unchanged implementation. They were not repeated. This review is not a live Oracle authorization or compilation test.

## Manual deployment and UAT

Review source differences and back up deployed definitions first. Existing dependency views/packages must already be valid. For the 21 October application delta, execute these F8 files individually, in order:

1. `sql/f8/PKG_EXEC_DASHBOARD/04.sql` — body.
2. `sql/f8/PKG_EXEC_REPORTS/02.sql` — body.
3. `sql/f8/PKG_HEAD_AUDIT_MONITOR/04.sql` — spec, then `05.sql` — body, then `06.sql` — validation.

After each unit execute `sql/f8/validate_errors.sql` and `validate_status.sql` separately. Stop on compilation errors, missing objects or INVALID status. For a fresh installation follow the fuller dependency order in `sql/f8/README.md`. Coordinate the app release with these procedure changes.

The menu migration updates an existing page1000401 only and fails closed if it is missing or conflicting. If production lacks that registration, use the existing approved IAS page-registration administration process with the workbook entry. Do not auto-create user allocations or run a bulk workbook overwrite. If the production registration exists, review the existing menu migration and commit only its expected single-row update.

UAT must verify allocated cluster, FAD, ISAD, CAD and QAD accounts; absent allocations; own and descendant performing-office scope; sibling engagement/document denial; empty-office results; concluded audits; unengaged plans; plan/KPI reconciliation; Pakistan date boundaries; all management tabs; Super User-only exports; and exported authorized totals. Oracle compilation and these account-based checks remain manual external dependencies. No claim of live verification is made.

Older PKG_RPT work is outside this heartbeat's completed follow-up: retain its source/deployment analysis and never replace the package with the standalone legacy R_IAS_PARA_POSITION procedure.

Unrelated `AIS/obj` NuGet changes and `.vs/` files remain untouched. Pause the resume heartbeat after this completion commit; no local work is pending for this checkpoint.
