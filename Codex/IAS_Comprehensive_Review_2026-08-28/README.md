# IAS Comprehensive Review — Evidence Workspace

Review date: 2026-08-28  
Environment: `https://localhost:50853` (development/test)  
Method: browser verification correlated with source and Oracle-access code review.

## Evidence standard

Each conclusion is tagged as one or more of:

- `BROWSER-VERIFIED`
- `SOURCE-VERIFIED`
- `DATABASE-CODE-VERIFIED`
- `PENDING-LIVE-DB`
- `NOT-YET-TESTED`

Absence of an exception is not treated as proof of correctness. Credentials, cookies, tokens, and configuration secrets are excluded from evidence.

## Current coverage snapshot

| Role | PPNO | Login | Context | Home/menu baseline | Status |
| --- | ---: | --- | --- | --- | --- |
| Employee Para Check | 704003 | Authenticated by server | No valid role/entity assignment | Not reachable | Browser verified; blocked by context configuration |
| SUPER USER | 140066 | Successful | Applied automatically | 14 module groups and eight quick links observed | Browser verified; page/field testing in progress |
| ADMINS | 139995 | Successful | Applied automatically | 9 module groups; no quick links | Browser verified; page/field testing in progress |
| Remaining listed roles | — | Not yet tested in this review run | — | — | Pending |

## Preliminary verified observations

1. `BROWSER-VERIFIED`: Employee Para Check receives “No login context assigned” despite the supplied role/entity test mapping. This prevents all requested functional coverage for that account until its assignment is corrected.
2. `BROWSER-VERIFIED`: Logout via direct `/Login/Logout` returns to `/Login/Index?ReturnUrl=%2FLogin`; the ReturnUrl is unusual and will be tested for redirect-loop/usability impact.
3. `SOURCE-VERIFIED`: Authentication, session, page permission, API permission, CSP/security-header, and rate-limit controls exist globally.
4. `SOURCE-VERIFIED`: `ObjectScopeAuthorizationFilter` extracts only `user_id`, `userid`, and `ppnumber`. Entity, branch, audit-zone, engagement, observation, para, and other business-object IDs are not recognized by this filter. Their authorization is therefore dependent on controller/database implementations and remains a high-priority IDOR test area.
5. `SOURCE-VERIFIED`: Sensitive credential values are present in a tracked application settings file. Values are intentionally omitted from this review workspace. Secret rotation and externalized configuration require immediate management attention.

## Deliverables

- `Executive_Assessment.md` — management-level conclusions (in progress)
- `Comprehensive_QA_Report.md` — screen/field/function coverage (in progress)
- `VAPT_Assessment.md` — security findings (in progress)
- `Role_Permission_Matrix.csv` — role access/actions (in progress)
- `Workflow_Matrix.csv` — verified transitions (in progress)
- `Defect_Register.csv` — defects only (active)
- `Improvement_Register.csv` — enhancements only (active)
- `Coverage_Ledger.csv` — explicit tested/not-tested accounting (active)

