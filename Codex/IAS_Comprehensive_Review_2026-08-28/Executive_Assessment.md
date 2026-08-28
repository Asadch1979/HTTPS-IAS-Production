# Executive Software Assessment (Interim — Evidence Based)

## Current management position

IAS cannot yet be certified as functionally correct or production-secure. The review has completed live authentication and top-level menu baselining for all 35 supplied roles, plus initial source-security correlation and one detailed Post Compliance screen. CRUD, workflow-transition, field-boundary and cross-entity authorization coverage remains incomplete and must not be inferred from this baseline.

## Verified strengths

- All supplied accounts except Employee Para Check reach an authenticated IAS home page.
- Role menus materially differ, demonstrating active role-based menu filtering.
- The application has global authentication fallback, session validation, page/API permission filters, rate limiting, secure-cookie configuration, HTTPS/HSTS configuration, security headers and CSP middleware.
- Post Compliance monitoring supports month/year filtering, table search, pagination and export options.

## Critical weaknesses and risks

- Secrets are present in tracked configuration. Rotation and externalization are urgent.
- Sidebar navigation and Logout clicks fail in the tested browser, preventing normal operation and reliable session termination.
- Global object-scope filtering recognizes only user identifiers; record/entity/branch/zone scope must be proven in every downstream endpoint and Oracle query.
- A settled para was returned without a para number, indicating a workflow/data-integrity failure or report-join defect.
- One required test account is unusable because no valid login context is assigned.

## Answers to the four management questions

1. **Is IAS functionally correct?** Not demonstrated. Authentication coverage is mostly successful, but verified navigation, context and settled-para defects prevent a positive conclusion.
2. **Is IAS secure enough for production use?** Not currently certifiable. Tracked secrets and unproven business-object scope enforcement are release-blocking concerns.
3. **Are roles, permissions and workflows correctly enforced?** Top-level role filtering is active, but page/API/database enforcement and workflow transitions remain under verification.
4. **What should be improved first?** Rotate/externalize secrets; repair navigation/logout; close and test object-scope authorization; reconcile blank para identities; restore the missing Employee Para Check context; then execute workflow and field-level regression packs.

