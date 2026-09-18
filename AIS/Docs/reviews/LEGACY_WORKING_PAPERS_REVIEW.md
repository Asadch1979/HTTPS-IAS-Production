# IAS Legacy Working Papers: Professional and Technical Review

**Review date:** 18 September 2026

**Scope:** Legacy Working Paper module only; review of repository views, controllers, JavaScript, C# database methods, models, and available `PKG_AR` package source.

**Change restriction:** Review only. No application code, workflow, table, view, or Oracle package was changed.

## 1. Executive assessment

IAS contains five legacy working papers: Loan Case File, Voucher Checking, Account Opening, Fixed Assets, and Cash Count. Together they provide engagement-linked data-entry registers and preserve the entering employee and timestamp. They do not, however, constitute complete audit working papers under normal professional internal-audit expectations because they do not record the audit objective, population and sample rationale, procedures performed, evidence obtained, result/exception disposition, overall conclusion, or supervisory review.

The most significant risks are:

1. **Insufficient audit trail and review control (High):** there is no prepared-by/reviewed-by workflow, version history, review-note resolution, completion status, or lock after approval.
2. **Evidence and conclusion gap (High):** observations and memo/para references appear in three papers, but evidence references, procedure results, exception evaluation, and a signed conclusion are absent from all five.
3. **Data integrity weakness (High):** calculations are manually entered as text. Cash totals and differences are not system-calculated; Fixed Assets differences are also free text.
4. **Incomplete lifecycle (High):** all pages display an Update column, but no usable update control is rendered and the JavaScript update functions are empty or mismatched. No delete, correction reason, or controlled supersession exists.
5. **Technical control weaknesses (High):** read procedures accept entity, employee, and role parameters but filter only by engagement. Add procedures ignore entity and role, generate keys with `MAX(id)+1`, and commit internally. These patterns increase authorization, concurrency, and transaction-control risk.

The recommended direction is a common working-paper shell with paper-specific test grids. It should separate planning, execution, exceptions, conclusion, and review; enforce server-side rules; calculate derived values; link evidence and observations by identifier; and retain immutable audit history.

## 2. Inventory and traceability

All five pages are served by `WorkingPaperController`; their read/write endpoints are in `ApiCallsController`; persistence is in `DBConnection.AR.cs`; and Oracle operations are in `PKG_AR`. Entry begins from the engagement task list and the Field Audit Working Paper step at `/WorkingPaper/loan_case_file?engId={id}`. The papers form a fixed sequence: Loan Case File -> Voucher Checking -> Account Opening -> Fixed Assets -> Cash Count.

| Working paper | View and JavaScript | API actions | C# methods | Oracle procedures / table |
|---|---|---|---|---|
| Loan Case File | `Views/WorkingPaper/loan_case_file.cshtml`; `wwwroot/js/csp/Views_WorkingPaper_loan_case_file.js` | `Get_Working_Paper_Loan_Cases`; `Add_Working_Paper_Loan_Cases` | `GetWorkingPaperLoanCases`; `AddWorkingPaperLoanCases` | `P_GetLoanCaseFile`; `P_AddLoanCaseFile`; `T_WORKING_PAPER_LOAN_CASE_FILE` |
| Voucher Checking | `Views/WorkingPaper/voucher_checking.cshtml`; `wwwroot/js/csp/Views_WorkingPaper_voucher_checking.js` | `Get_Working_Paper_Voucher_Checking`; `Add_Working_Paper_Voucher_Checking` | `GetWorkingPaperVoucherChecking`; `AddWorkingVoucherChecking` | `P_GetVoucherChecking`; `P_AddVoucherChecking`; `T_WORKING_PAPER_VOUCHER_CHECKING` |
| Account Opening | `Views/WorkingPaper/account_opening.cshtml`; `wwwroot/js/csp/Views_WorkingPaper_account_opening.js` | `Get_Working_Paper_Account_Opening`; `Add_Working_Paper_Account_Opening` | `GetWorkingPaperAccountOpening`; `AddWorkingAccountOpening` | `P_GetAccountOpeningDetails`; `P_AddAccountOpeningDetails`; `T_WORKING_PAPER_ACCOUNT_OPENING` |
| Fixed Assets | `Views/WorkingPaper/fixed_assets.cshtml`; `wwwroot/js/csp/Views_WorkingPaper_fixed_assets.js` | `Get_Working_Paper_Fixed_Assets`; `Add_Working_Paper_Fixed_Assets` | `GetWorkingPaperFixedAssets`; `AddWorkingFixedAssets` | `P_GetFixedAssetsDetails`; `P_AddFixedAssetsDetails`; `T_WORKING_PAPER_FIXED_ASSETS` |
| Cash Count | `Views/WorkingPaper/cash_count.cshtml`; `wwwroot/js/csp/Views_WorkingPaper_cash_count.js` | `Get_Working_Paper_Cash_Counter`; `Add_Working_Paper_Cash_Counter` | `GetWorkingPaperCashCounter`; `AddWorkingCashCounter` | `P_GetCashCounterDetails`; `P_AddCashCounterDetails`; `T_WORKING_PAPER_CASH_COUNT` |

Related entry points are `Views/Engagement/task_list.cshtml`, `Views/FieldAudit/AR_Partials/_WorkingPaperStep.cshtml`, `Controllers/FieldAuditController.cs`, and the working-paper registration in `Services/FieldAudit/FieldAuditStepRegistry.cs`. The similarly named Field Audit replica scripts and partials are newer execution UI components and are not separate legacy Working Paper records in this review.

The five response models are `LoanCaseFileDetailsModel`, `VoucherCheckingDetailsModel`, `AccountOpeningDetailsModel`, `FixedAssetsDetailsModel`, and `CashCountDetailsModel`.

## 3. Common professional-audit findings

| Area | Current state | Risk | Recommendation |
|---|---|---|---|
| Objective and risk | No objective, control assertion, risk, or audit-program reference | Work cannot be tied clearly to engagement objectives | Require objective, risk/control, audit-program step, period, and scope |
| Population and sampling | No population size/value, source, selection method, sample size, or rationale | Coverage and sufficiency cannot be evaluated | Capture population metadata, sample rationale, seed/selection method, and coverage |
| Procedure and evidence | Rows record selected facts; no procedure steps, evidence attachments, source, custodian, or evidence date | No reproducible record of work performed | Add procedure/result fields and indexed evidence links with metadata and hashes where appropriate |
| Calculations | Totals and differences are user-entered strings | Arithmetic may be inaccurate or altered without detection | Use typed numeric fields and server-calculated, read-only derived values with reconciliation controls |
| Exceptions | Some papers contain a free-text observation and para number; Fixed Assets has remarks; Cash Count has neither | Exceptions may be incomplete, inconsistently classified, or unlinked | Use a common exception model: condition, criteria, cause, impact, risk, owner, disposition, and observation ID |
| Conclusion | No overall conclusion or unresolved-exception summary | File does not demonstrate whether the objective was achieved | Require conclusion, result rating, unresolved items, and completion checklist |
| Review | No reviewer assignment, sign-off, review notes, status, lock, or timestamps | No evidence of supervision or quality control | Add maker-checker workflow, review-note cycle, electronic sign-offs, and approved-version lock |
| Change history | Insert only; no controlled correction or history | Errors cannot be corrected transparently | Add versioned edit/supersede with reason; retain before/after values and actor/time |
| Cross-referencing | Memo/para is free text where present | Broken or unverifiable linkage | Select/link IAS observation/memo records by immutable ID and display their current status |

## 4. Working-paper-wise findings and recommendations

### 4.1 Loan Case File

**Current coverage:** loan case number, outstanding amount, disbursement date, category (General/NPL/SAM), observation, and memo/para reference.

**Findings:** The paper captures a small loan inventory but no borrower/customer identifier, facility type, sanction limit, approval authority/date, security and valuation, documentation checklist, classification/provisioning basis, repayment status, overdue days, or procedure result. The amount is passed from C# as `OracleDbType.Int32`, which is unsuitable for many monetary values and is populated from a string. The category placeholder has value `0`, while validation only rejects an empty value, so an unselected category can be stored. The displayed update link is empty, and `updateLoanCaseFile` merely opens a blank add dialog.

**Recommendation:** Turn this into a sampled loan-test sheet tied to the loan population. Add typed monetary/date fields, customer masking, approval and security tests, regulatory classification/provision comparison, exception status, evidence references, and a conclusion by loan plus an aggregate conclusion. Source key facts from authoritative systems where feasible rather than re-keying them.

### 4.2 Voucher Checking

**Current coverage:** voucher/account number, observation, and memo/para reference.

**Findings:** This is too sparse to show voucher testing. It omits transaction date, amount, GL/account, voucher type, preparer/authorizer, approval limit, supporting documents, business purpose, payee, tax/compliance checks, sample rationale, and test outcome. The update function is empty and the page renders no actionable update link. Unencoded database values are concatenated into HTML, creating a stored DOM injection risk if upstream output encoding does not intervene.

**Recommendation:** Define a voucher-testing checklist with transaction attributes and pass/fail/not-applicable assertions. Link scanned/source evidence, flag overrides and duplicate payments, distinguish exceptions from observations, and summarize sample coverage and exception rates.

### 4.3 Account Opening

**Current coverage:** account number, account nature, observation, and memo/para reference.

**Findings:** The page title is incorrectly set to "Voucher Checking," the add control is labelled for voucher checking, and reused IDs/function names obscure intent. Account nature validation accepts the placeholder value `0`. Key KYC/CDD controls are absent: customer type, risk rating, beneficial owner, identity verification, sanctions/PEP screening, source of funds, approval, mandatory documents, account opening/date/status, and exception remediation. The update function is empty and references a voucher update name.

**Recommendation:** Use account-type-specific test templates and mandatory KYC/CDD assertions. Record evidence dates/results, screening reference, risk rating, approvals, missing-document aging, exception owner/due date, and an account-level conclusion. Mask sensitive identifiers and apply least-privilege viewing.

### 4.4 Fixed Assets

**Current coverage:** asset name, physical existence, location per fixed asset register, difference, and remarks.

**Findings:** Asset identity is ambiguous without asset tag/register ID. There is no recorded register location versus observed location pair, custodian, asset class, acquisition/cost, condition, capitalization/depreciation test, disposal/impairment indicator, evidence, or count date. "Physical Existance," free-text difference, and mandatory remarks do not provide a controlled test result. JavaScript calls a nonexistent/mismatched `updateVoucherChecking` from the rendered row, while `updateFixedAssets` is empty.

**Recommendation:** Record asset tag, register details and observed details separately, then derive location/existence exceptions. Add condition, custodian confirmation, value/depreciation tests where in scope, photo/evidence reference, exception classification, and count-sheet reconciliation.

### 4.5 Cash Count / Cash Counter

**Current coverage:** denomination, note count and total for physical vault and safe/night-safe register, plus difference.

**Findings:** This is the strongest candidate for structured calculation, but all seven values are free text and all three totals/difference are manually entered. The UI duplicates denomination on both sides although reconciliation should normally align one denomination per row. It does not capture count date/time, currency, location/vault, custodian, joint count witnesses, seal/key status, cut-off details, system/register source and timestamp, non-note cash items, recount, explanation, evidence, exception disposition, or conclusion. Labels alternate among Cash Count, Cash Counter, Night Save Register, and Night Save Vault. The page title remains "Voucher Checking." The update column is inert and calls a voucher-named function. The previously identified `DSR`/`CSR` binding defect demonstrates the fragility of abbreviated, reused names; the corrected C# mapping now sends `NOSR` to formal parameter `CSR`.

**Recommendation:** Replace the modal with a controlled count sheet. Use a denomination master, one row per currency/denomination, integer quantities, read-only calculated amounts, separate system/register quantities or amounts, calculated row variances and grand totals, witness attestations, evidence, variance explanation and escalation, and reviewer sign-off.

## 5. Common technical findings

| Priority | Finding | Evidence and impact | Recommended treatment |
|---|---|---|---|
| High | Authorization context is not enforced in package reads | All five `P_Get...` procedures receive `ENT_ID`, `P_NO`, and `R_ID` but filter only by `ENG_ID`; add procedures also do not store/use entity or role | Enforce engagement access server-side and in the data layer; do not trust query-string engagement IDs |
| High | Manual, untyped calculations | Cash and Fixed Assets numeric concepts are strings; Cash values use `Varchar2`; loan amount uses `Int32` from a string | Introduce typed request DTOs, decimal/integer constraints, server calculations, and database numeric types |
| High | Incomplete update lifecycle | Empty anchors and empty/mismatched update functions across all papers; no update/delete procedures | Implement controlled edit/supersede, concurrency token, reason, and immutable history |
| High | Unsafe key generation | Each insert uses `COALESCE(MAX(id)+1,1)` | Use Oracle sequences/identity and constraints to prevent concurrent duplicate keys |
| Medium | Stored HTML injection exposure | JavaScript concatenates returned values directly into HTML strings | Render via DOM text APIs or encoded templates; maintain output encoding defense in depth |
| Medium | Validation is client-only and inconsistent | Controller actions accept primitive parameters; no DTO/model validation; select placeholders can pass | Validate authorization, required values, lengths, enums, ranges, and relationships server-side |
| Medium | Internal commits in package procedures | Each add procedure commits independently | Let the service transaction boundary control commit/rollback |
| Medium | Weak API contracts | Mutations return hand-built JSON strings; reads allow both GET and POST; exceptions are not represented consistently | Use typed DTOs and standardized HTTP/JSON outcomes; GET for read, POST/PUT for mutation |
| Medium | Naming and type drift | Cash Count/Counter terminology, `VNUMBER` for account number, `ENGID` alternates Varchar2/Int32, abbreviations obscure mappings | Adopt explicit domain names and generated/verified parameter contracts |
| Low | Repetition and stale presentation | Five near-identical controller actions/pages/scripts; incorrect titles, labels and spelling | Use a common working-paper shell/components while retaining paper-specific schemas |

Audit attributes on the five add endpoints provide useful application-event logging. They should be retained and expanded to include record ID, action, before/after version, outcome, and reviewer events. Existing page authentication and page-permission checks are also useful, but object-level engagement authorization must be enforced for every read and write.

## 6. Proposed standardized working-paper design

Each paper should use one consistent header and lifecycle, followed by a domain-specific test grid.

### Header and lifecycle

| Section | Required content |
|---|---|
| Identity | Working-paper ID/version, engagement, entity, audit period, process, owner, status |
| Planning | Objective, risk/control, audit-program step, scope, population source/date/size/value, sampling method/size/rationale |
| Execution | Procedure performed, performer/date, domain test rows, evidence references, result |
| Exceptions | Exception ID, criteria/condition/cause/impact/risk, observation link, owner/due date/status |
| Conclusion | Coverage summary, exception summary, objective achieved Yes/Partly/No, conclusion and residual risk |
| Review | Prepared-by sign-off, reviewer assignment, review notes and resolution, reviewed-by sign-off, approval/lock |
| History | Immutable version/action log with actor, timestamp, reason, and before/after values |

Suggested statuses are `Draft`, `Prepared`, `In Review`, `Returned`, `Reviewed`, and `Locked`. Only the preparer should edit a Draft/Returned paper; the reviewer must be independent of the preparer; approval should lock the version; reopening should require authorization and a reason.

The standard page should show an engagement context bar, compact status/ownership controls, tabs for Plan, Test Sheet, Exceptions, Conclusion, Review Notes, and History, and persistent actions appropriate to the current status. Tables should support filtering, totals, evidence/observation links, validation summaries, and export to a review-ready PDF without becoming the authoritative record.

## 7. Sample redesigned Cash Counter Working Paper

### 7.1 Audit purpose

**Objective:** Confirm that cash physically held at the specified location and cut-off agrees with the authorized cash register/general ledger, is safeguarded, and is supported by a witnessed count.

**Core risks:** misappropriation, unrecorded transactions, inaccurate records, cut-off error, counterfeit/damaged currency, and inadequate dual control.

**Procedure:** Obtain the register balance at the recorded cut-off; perform or observe a surprise joint count; count each currency and denomination; reconcile physical cash to the register and ledger; inspect seals, keys and dual-control arrangements; investigate and disposition every variance; retain evidence and conclude.

### 7.2 Proposed screen structure

```text
Cash Count WP  CC-2026-001   Draft   Engagement 4821   Branch ABC
Count: 18-Sep-2026 09:15     Vault: Main       Currency: PKR
Custodian: [name/id]          Witness: [name/id] Surprise count: Yes

[Plan] [Count Sheet] [Exceptions] [Conclusion] [Review] [History]

Denomination | Physical qty | Physical amount | Register qty | Register amount | Variance
       5000  |           20 |         100,000 |           20 |         100,000 |        0
       1000  |           48 |          48,000 |           50 |          50,000 |   -2,000
        ...  |              |                 |              |                 |
TOTAL        |              |         148,000 |              |         150,000 |   -2,000

Other cash items / sealed packets: [reconciled schedule]
Ledger balance: 150,000   Register-to-ledger variance: 0
Overall physical-to-ledger variance: -2,000 [Exception CC-E01]

Evidence: register extract, signed count sheet, vault photo/seal record, GL extract
Conclusion: [Achieved / Partly achieved / Not achieved] [narrative]
[Save draft] [Sign as prepared] [Send for review]
```

### 7.3 Field and calculation rules

| Field/control | Rule |
|---|---|
| Count identity | Server-generated WP/version; engagement and authorized entity derived from session/context |
| Count context | Count timestamp, location/vault, currency, cut-off timestamp, custodian and witness required |
| Denomination | Selected from active currency denomination master; unique per count sheet |
| Quantities | Non-negative whole numbers; recount quantity retained separately when used |
| Amounts | `physical amount = denomination x physical quantity`; equivalent register amount calculated or imported |
| Variance | `physical amount - register amount`; totals calculated server-side and displayed read-only |
| Reconciliation | Register total reconciled to ledger; other cash items separately scheduled and included once |
| Exception | Non-zero variance requires explanation, risk rating, owner, due date, evidence, and disposition/observation link |
| Attestation | Custodian/witness confirmation plus preparer and reviewer sign-offs with timestamps |
| Evidence | Required register and ledger extracts plus signed count evidence; immutable metadata retained |
| Review | Reviewer sees recalculation result, variances, unresolved exceptions, evidence completeness, and change history |

## 8. Phased improvement plan

| Phase | Indicative duration | Scope | Exit criteria |
|---|---:|---|---|
| 0. Control and design baseline | 2-3 weeks | Confirm owners, terminology, retention/classification, audit methodology, authorization model, field dictionary, and migration approach; reproduce defects with tests | Approved requirements, control matrix, data model and acceptance criteria |
| 1. Stabilize legacy module | 3-5 weeks | Add object-level authorization, server validation, safe output rendering, typed API contracts, error handling, tests, and clear disabled state for unsupported updates; preserve current workflow | Security/integrity tests pass; no silent invalid submissions; behavior documented |
| 2. Common working-paper foundation | 6-8 weeks | Build common header, status workflow, sign-offs, review notes, evidence links, exception links, version/history, sequence-based keys, and reusable UI components | One pilot paper completes maker-reviewer lifecycle with immutable history |
| 3. Cash Counter pilot | 4-6 weeks | Implement proposed count sheet, calculations, reconciliation, attestations, exception escalation, print/export, and migration/read-only access to legacy rows | Parallel user acceptance completed; totals independently recalculated; review sign-off evidenced |
| 4. Migrate remaining papers | 8-12 weeks | Redesign Loan Case, Voucher, Account Opening and Fixed Assets using risk-based test templates; migrate or retain legacy records read-only | Each paper meets agreed audit methodology and data-quality controls |
| 5. Assurance and rollout | 3-4 weeks | Security/performance/accessibility testing, training, operating procedures, monitoring, phased production release and post-implementation review | Control owner sign-off, support readiness, monitored adoption and issue closure |

Prioritize authorization and data integrity before visual redesign. Pilot Cash Counter because its objective and calculations are well-defined, making it suitable to validate the shared lifecycle, evidence, exception, and review controls before applying them to judgment-heavy papers.

## 9. Acceptance measures

The redesign should be considered successful when every completed paper can answer, without external explanation: why the work was performed; what population and sample were covered; what procedure was executed; what evidence supports it; what calculations were made; which exceptions remain; what conclusion was reached; who prepared and reviewed it; and what changed over time. Technical acceptance should include object-level authorization tests, server-side validation, deterministic recalculation, concurrency tests, output-encoding tests, audit-history tests, and full maker-reviewer workflow tests.

## 10. Review basis and limitations

This is a static repository review. It used the five legacy views and scripts, `WorkingPaperController`, the relevant `ApiCallsController` actions, models, `DBConnection.AR.cs`, `Docs/sql/PKG_AR.sql`, and the corresponding package source in `Docs/sql/ALL_PACKAGES.sql`. No live Oracle metadata, production data, user interviews, screenshots of the running application, policy manuals, audit methodology, or evidence-retention standard were available. Recommendations should therefore be validated with Internal Audit methodology owners, Information Security, application owners, and representative preparers/reviewers before implementation.
