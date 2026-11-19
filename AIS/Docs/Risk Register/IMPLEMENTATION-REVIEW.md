# FAD Risk Register implementation review

## Application integration (2026-11-19, based on f70682c1)

This section supersedes earlier statements that the assessment application integration is missing. The four MVC actions now call the existing package signatures using Oracle.ManagedDataAccess, named binds and server-session-only identity/context values. Reads dispose all returned cursors/readers and materialize CLOB snapshots before closing the connection. SAVE and TRANSITION use one connection/transaction, check the session again before committing and roll back through transaction disposal on errors. There is no SQL scoring implementation in JavaScript or C#.

The separate `FADRiskControlPanel/Assessment` view preserves the original five configuration tabs. The catalog combines package-authorized assessments/versions with the established GetReportEntities selector. The matrix is rendered from P_READ; no Annexure list or process weights are hardcoded in the UI. Header metadata, results, evidence references, preparer/reviewer, workflow status and historical snapshots are displayed with text-only DOM insertion. Controls use API permissions, explicit risk-access preparation/review flags, ownership and workflow state; Oracle independently authorizes every operation. Interrupted writes require a refresh rather than automatic retry.

New POST request models validate IDs, revisions, sizes, amounts, unique codes, instance counts and observation/working-paper evidence syntax. Oracle validates full configuration membership and evidence ownership. Both writes use ASP.NET antiforgery validation and the established RequestVerificationToken header. Errors use safe JSON messages and HTTP 400/401/403/409/422/500; internal Oracle messages and stack traces are not returned. Existing global session, page/API and object-scope filters are unchanged.

### Registration and deployment

Review `40_application_permissions.sql`, allocate a verified new page ID and supply the existing FAD menu ID. The script adds the independent assessment page and four INACTIVE API entries, rejects conflicting anchors and leaves all existing role grants unchanged. It does not assign groups or enable access. Administrators must explicitly assign this page to approved groups, activate the required API entries and role/API grants, and configure T_FAD_RISK_ACCESS for each authorized audit office. Readers need both GET APIs; preparers additionally need SaveAssessment/TransitionAssessment and can_prepare; independent reviewers need TransitionAssessment and can_review. Clear existing permission caches using normal IAS administration after changes. No permission or migration script has been executed.

Deployment order: validate and install the reviewed Oracle schema/packages first, compile with zero USER_ERRORS, import and publish the reviewed configuration using the dedicated administrator, register permissions, then deploy the application. Old master Annexure callers retain default commits; the application transaction caller requires the new optional p_commit parameters. Application rollback precedes any package rollback. Preserve approved assessments/configuration and never delete historical data as a rollback mechanism.

### Validation and remaining release gates

Run `dotnet run --project tests/fad-risk-register/application/FadRisk.InputChecks.csproj` for actual request-model and Oracle JSON-name checks; run the Python suite for reconciliation/generation/security/application source contracts; run Node syntax and frontend regression checks. Compile AIS including Razor views. Source checks do not establish runtime security or Oracle execution.

Staging remains required: authorize representative preparer/reviewer/read-only users; verify separate page grants, missing/invalid CSRF, expired/revoked/context-switched sessions and direct non-AJAX API denials; test unauthorized engagements and forged evidence; round-trip NUMBER/CLOB/ref-cursor values; save the full sample; exercise submit/return/resubmit/approve and stale revision races; verify approved history cannot change; force a write failure to verify transaction rollback; verify all five master tabs still perform CRUD. Test the screen in the deployed IAS layout and inspect accessibility/responsiveness. No live Oracle or authenticated browser session was available here. Production migrations and merges remain prohibited until reviewed and staging-validated.

## Follow-up review of 21e89f0

Restored the session's selected user_context_id binding and explicit operation allowlist lost when generation overwrote e604ec18. All package corrections now live in assessment-body.sql.inc; regenerate with build_fad_risk_package.py. The generator refuses to overwrite the artifact when critical safeguard expressions are absent. Source regression tests and byte-for-byte regeneration tests protect these checks, but do not prove Oracle runtime authorization.

SAVE and SUBMIT lock the engagement before the assessment, recheck audit-office ownership and auditee entity, and enforce statuses 10–13. Both SUBMIT and APPROVE require exactly 127 correctly mapped detail entries, valid stored financial/count inputs and evidence, 20 correctly mapped/capped results, a matching total and a valid stored rating. Rolled-back saves restore the original output ID.

Financial evidence may now use `WP:<evidence_link_id>` with no observation ID. This identifies the existing IAS T_WP_EVIDENCE_LINK source, joined through T_WP_HEADER to the authorized engagement and auditee entity. Optional V2 tables are accessed with bound dynamic SQL; missing sources fail rather than bypass validation. Plain numeric evidence IDs still require their matching observation ID. Arbitrary URLs/paths and unknown source prefixes are rejected. Evidence ownership is rechecked at submission/approval. No evidence download or upload endpoint is introduced.

Install 24_exact_configuration.sql before 25_publication.sql. Publication checks exact V1 process identities, parent hierarchy, individual weights/denominators, sub-process criteria/gravity and all 121 Annexure mappings/descriptions. Different scoring configurations require a reviewed validator/calculator version; matching totals alone is insufficient.

Publication now requires SESSION_USER membership in T_FAD_RISK_PUBLISH_ADMIN, empty by default. DBA deployment must use a dedicated database principal distinct from the application account, provision only that principal, and grant it only the required procedure execution. Do not grant application DML on the allowlist or configuration tables, EXECUTE on publication, or broad ANY privileges. Schema owners/DBAs inherently control their own objects. Actual installed grants and separation of database accounts remain UNVERIFIED; the source cannot establish that production administration is restricted.

Local validation: 11 Python tests pass, including reconciliation, deterministic generation and source-level unauthorized-context, closed-engagement, incomplete-assessment, evidence-ownership and invalid-publication guards. Oracle compilation and behavioral denial/transaction/concurrency tests remain required in staging. No production migrations were run; PageKeyPermissionGuard, IAS permission assignments and unrelated workflows were untouched.

Status: Phase 1 corrections. **Not deployment ready.** Master controller/DBConnection and JavaScript repairs are implemented. Schema, seed, immutability, assessment/workflow package, publication procedure and Oracle tests exist as uninstalled SQL drafts. Assessment HTTP/DBConnection integration, screen and permission registration remain outstanding. No production migrations were executed.

## Correction verification (2026-11-18)

This section supersedes the original pending-decision and missing-SQL statements below. The user approved Excel formula boundaries and stopping calculation for review on zero denominators. Existing edits now lock/check the engagement and require status 10–13, matching the existing report package. Scope, ownership, revision and workflow checks remain enforced.

Observation/evidence references now require paired numeric IAS IDs. Oracle validates evidence MEMOID against observation ID, engagement, auditee entity and authenticated audit office. Arbitrary paths/URLs are rejected. Financial inputs require supporting observation evidence. HTTP request-model validation remains outstanding with the assessment API integration; no new upload facility is implemented.

P_PUBLISH_FAD_RISK locks the version and checks 121 Annexures, 20 processes, weights totaling 100, parent mappings, gravity, denominators, zero-weight fraud and calculator identity. It is a schema-owner deployment operation, with no application grants and no implicit commit. It remains uncompiled SQL source.

Risk Register Tables.xlsx was inspected. It confirms mandatory MAP_ID, but omits defaults, identities, triggers and sequences, so generation remains unverified. Run 00_preflight.sql to obtain this evidence; do not invent a sequence or deploy without verifying inserts in staging.

Included generators: tools/fad_risk_workbook.py, tools/build_fad_risk_seed.py and tools/build_fad_risk_package.py. Run `python -m unittest discover -s tests/fad-risk-register` and `node tests/fad-risk-register/frontend.cjs` locally. Local tests do not prove Oracle compilation.

Staging gates, all unverified: preflight and MAP_ID verification; compile packages with zero USER_ERRORS; execute 90_calculation_tests.sql; exercise every master CRUD operation; deny revoked sessions, wrong roles, cross-office IDs, invalid references and self-review; reject edits after engagement closure; exercise stale revisions/concurrent closure; force mapping failure and prove both Annexure and mapping writes roll back. No staging connection is available.

Compatibility: deploy the updated master package BEFORE the application. New calls send p_commit='N'; old Annexure/mapping signatures retain DEFAULT 'Y'. Roll back the application before restoring the old package. Confirm this behavior in staging before release.

## Evidence and reconciliation

The reference workbook is preserved byte-for-byte. `workbook-source.json` preserves cell addresses, formulas, cached values and source SHA-256. Regenerate it and `risk-configuration.draft.json` with `tools/fad_risk_workbook.py` using Python 3. The extraction uses the standard library and does not execute workbook macros or edit Excel.

The draft configuration contains 121 unique Annexure codes, complete descriptions and risk labels; each is associated with a matrix sub-process and a weighted process. Its 20 process entries include five quantitative achievement components, profitability and zero-weight fraud. Numerical weights total 100: lending 22, collection/recovery 12, achievement 20, operations 36, profitability 10. These group totals are normalization denominators, not additional weights to be added to the total.

The independent Decimal calculation reads raw matrix inputs, multiplies gravity by instances, normalizes each component by its group total, caps at component weight and adds marks. It reconciles every component against Final Rating column H and yields 53.5710678210678 (within 1e-9 of requested 53.571067821). This validates the sample arithmetic, not the entire Excel formula graph or an Oracle deployment.

## Decisions requiring business review

| Source | Conflict / proposed draft handling |
|---|---|
| Final Rating I51 versus D55:F57 | Formula uses `<10/<40`, `<15/<50`, `<20/<60`. Printed Low includes 10/15/20 and Medium includes 40/50/60. Draft reproduces the formula, but is unpublished pending selection. Printed integer ranges also leave fractional gaps. |
| Final Rating B55:B57 | Printed size ranges leave 300–301 and 699–700 fractional portfolios unclear. Matrix I5 is manually entered. No automatic size classifier has been invented. Confirm units and exact boundaries. |
| Rating Matrix C1, C7 | Both contain `#REF!`. Neither feeds the sample total. Preserved as source errors, not silently repaired. |
| Rating Matrix G33:G39 | Zero openings/targets divide by zero. Draft calculator raises an error requiring review rather than awarding zero marks. Policy is pending. |
| Final Rating F30, G30 | SUM starts at row 27, omitting NPL/SAM rows 25–26. H30 includes all five, so the final capped sample total remains correct. Do not use F30/G30 as complete achievement totals. |
| Final Rating G/H/I cells | Some marks/ratings are literals rather than formulas (e.g. H13, H28, H29). Independent sample arithmetic matches their saved values, but input changes in Excel need not update them. |
| Rating Matrix C45; Annexure C58/F55 | B026 / BO-26 versus canonical BO26. Draft uses explicit B026 → BO26 alias from the code column; review required. |
| Annexure C95 | BO23 has trailing whitespace. Draft trims code whitespace and preserves original cell text in the source export. |
| Annexure F11, F89 | Narrative spellings A23 and GO-16 differ from canonical code cells A-23 and GO16. Code cells and matrix rows define draft mappings. |
| Annexure G51:G59 | Process #8 spans security and cash; Final Rating rows 35/36 have separate weights. Draft maps via individual matrix rows, preserving original process number separately. |
| Annexure B51:B54 | Blank package cells follow recovery rows; naive fill-down would misclassify BO36/BO25/BO32/GO7. Draft uses matrix process ownership. |
| Annexure E34:E35 vs Matrix H23 | I-7 and I-13 are LOW but gravity is 3. Preserve both; never infer gravity from HIGH/MEDIUM/LOW. |
| Annexure CO4 | Mapped to SAM movement (Matrix row 34). BO9/BO15 map to deposit/disbursement shortfalls. Their observation counts do not substitute for financial inputs. NPL/recovery/profitability lack their own Annexure codes. |
| Matrix I64, J64 | Profitability formula is in rupees: `<-50000`=10, `<=0`=5, `<=500000`=3, otherwise 0. Confirm input units; do not silently scale the sample -22.69 to millions. |
| Fraud | A-1/I-4 use gravity 5 in the matrix, but contribute zero numerical marks. Any positive count forces High independently of score. |

## Schema and authorization prerequisites

Reviewed repository export: `AIS/Docs/Data_Structure/Table_Structure_for_Codex.txt`, tables 454–464. It is evidence of a prior schema, not current production state or existing data. No live Oracle connection has been supplied or used.

Run `AIS/Docs/sql/fad-risk-register/00_preflight.sql` using an approved read-only connection and review the output before preparing DDL/imports. It captures table columns/defaults/identities, keys, triggers, sequences, master rows, result counts and package source. Retain it in a controlled review location rather than committing operational exports.

Critical checks:

- Sub KPI export uses KPI_SUB_CODE/KPI_SUB_NAME; the existing package assumed KPI_CODE. The repair resolves either supported spelling and keeps legacy response aliases.
- Gravity has GRAVITY_SCORE but no DISPLAY_ORDER. The repaired query exposes score and uses the ID for stable ordering.
- Mapping MAP_ID is mandatory. Verify its identity/default/trigger before insertion; do not guess a sequence name or use MAX+1. This remains a blocker for claiming mapping inserts work on the target database.
- Annexure descriptions can exceed the existing VARCHAR2(200). Preserve full text in the versioned configuration design; review an additive storage solution rather than truncate imported descriptions.
- Review existing duplicate/inactive mappings, code collisions, gravity IDs, orphan rows and any historical result data before generating the import. Do not replace existing IDs.
- Existing report access uses authenticated session context and `PKG_FRPT.P_GET_REPORT_ENTITY`. Inspect its installed implementation and database context before reusing it for assessment authorization. Browser-supplied entity IDs, PP numbers and reviewer IDs must never establish scope.

## Remaining implementation contract

Create additive version header and immutable hierarchy/criteria snapshots, assessment header/detail/component result/history tables after preflight. Each assessment must reference one immutable published configuration and one existing FAD engagement, with branch resolved from that engagement. Details must use keys from the selected version. Retain raw inputs, observations/evidence references, normalization denominators, capped marks, fraud flag, calculator version and final rating for reproducibility.

Draft → Submitted → Approved or Returned transitions must lock the assessment row, check expected revision, enforce role plus engagement/entity access in both HTTP and Oracle, prohibit self-review, and commit details/results/history atomically. Approved records and their configuration must be immutable; subsequent work creates a new revision. Do not implement approval by merely changing a status supplied by the client.

The assessment screen must show authorized audit/branch metadata, matrix inputs and marks, final rating, evidence/observations, preparer/reviewer and history. Every read/write endpoint needs PageKeyPermissionGuard, role/API registration and server/database scope checks. Registration must use verified page IDs and existing role assignments; no blanket role grants or permission bypasses. Add CSRF validation to new writes. Test forged assessment, engagement, entity and evidence IDs; stale revisions; concurrent approvals; unauthorized reviewers; transaction failure; approved-version replay.

## Prepared changes and validation

- Master package: column resolution, Annexure description retrieval, gravity retrieval, SQL*Plus package terminators. Existing standalone saves retain default commits.
- Annexure controller save uses one database transaction for Annexure plus mapping, calling the two package procedures with `p_commit='N'`. Failure rolls back through transaction disposal; the returned ID is assigned only after commit.
- JavaScript accepts canonical and legacy Sub KPI fields and escapes quotes in attribute values.
- `PKG_FAD_RISK_SCORE_V1.sql` is a draft pure Oracle calculator. It performs no database writes or grants and is not connected to production endpoints.
- Python reconciliation tests cover all codes, total weights, every sample component, final score, threshold boundaries, fraud, caps, profitability and zero denominators.
- `90_calculation_tests.sql` tests the actual Oracle calculator in a disposable schema. It has been prepared but not executed here. Python tests alone are not proof of Oracle compilation or execution.

## Deployment and rollback

Do not deploy this preparation as a complete risk-register release. Resolve the above decisions and finish the remaining implementation first. No migration or permission activation script is supplied because current schema/data and authorized page/role mappings remain unverified.

For separately reviewed master repairs: capture the installed package spec/body and application release first. Validate on a representative test database, including MAP_ID generation and forced mapping failure. Deploy the backward-compatible master package before the updated application because the application uses the optional commit parameters. Compile package, inspect USER_ERRORS, test all existing CRUD operations and permission denials, then test atomic rollback. Oracle DDL commits implicitly; application rollback does not undo package installation.

Rollback the application to the prior release first, then restore the captured package spec/body if needed. Never roll back by deleting existing master rows or assessments. The pure calculator can be left unused. Do not drop any published calculator/configuration version referenced by future assessments. Preserve unrelated user modifications and all Field Audit para statuses, reporting, Post-Compliance and dashboard logic.
