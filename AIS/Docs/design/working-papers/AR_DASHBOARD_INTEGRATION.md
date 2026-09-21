# Working Paper V2 AR Dashboard Integration

## Entry and rollout control

V2 is rendered inside the existing AR Dashboard `WORKING_PAPER` step and inherits its selected engagement,
page permission, engagement status and active-engagement session context. The former standalone URL redirects
to that dashboard step. Set `WorkingPapersV2:Enabled=true` through approved environment configuration for a
test deployment; the default when absent is disabled, preserving the legacy dashboard working-paper view.

The embedded overview shows Loan Case File, Voucher Checking, Account Opening, Fixed Assets and Cash Count,
including current status and calculated completion percentage. Selecting a paper stays inside the dashboard.
Legacy rows remain available through the V2 Legacy tab and no legacy table is changed.

## Entity and authorization model

- `P_AUDITOR_ENTITY_ID` is the authenticated auditor's posting/organizational entity from the IAS session.
- `T_WP_HEADER.ENTITY_ID` is the audited entity and is always derived from `T_AU_PLAN_ENG.ENTITY_ID`.
- Application access requires the engagement to be returned by `PKG_AR.P_GET_AR_DASHBOARD_DROPDOWN` for the
  logged-in PPNO.
- Database access repeats the dashboard assignment rule using `T_AU_AUDIT_TEAM_TASKLIST` and the eligible
  engagement status range. Assignment is required regardless of role.
- Existing preparer/reviewer separation and review-note controls remain additional restrictions.

## Permission test matrix

| Persona | Assignment | Expected result |
|---|---|---|
| Branch Auditor - Team Lead | Selected engagement, `ISTEAMLEAD=Y` | Dashboard step visible when legacy working-paper page permission exists; assigned papers can be prepared/reviewed only according to V2 assignment and status |
| Team Member | Selected engagement, `ISTEAMLEAD=N` | Same navigation visibility; may prepare an assigned paper but cannot review or close notes unless assigned reviewer |
| Supervisory role | Explicit engagement-team assignment | Can open the same dashboard step; review actions require reviewer assignment |
| Supervisory role | No engagement-team assignment | Engagement absent from selector; direct view/API access returns 403 and package raises access denied |
| Any role | Feature flag disabled | Existing legacy dashboard working-paper content remains; V2 view/API returns unavailable |
| Any role | Different engagement ID | Dashboard selection fails and V2 read/create returns 403 |

## Release checks

1. Apply `005_integrate_ar_dashboard_context.sql` in a non-production schema.
2. Run `003_verify_working_paper_install.sql`; require valid package objects, no `USER_ERRORS`, no missing
   dependencies and no header/audited-entity mismatches.
3. Enable V2 only in the test environment and execute the matrix above with representative accounts.
4. Verify all five summary cards, in-dashboard paper switching, browser back/refresh, legacy-tab visibility,
   create/save/submit/review permissions, and disabled-feature fallback.
5. Obtain testing approval before enabling the feature in production.
