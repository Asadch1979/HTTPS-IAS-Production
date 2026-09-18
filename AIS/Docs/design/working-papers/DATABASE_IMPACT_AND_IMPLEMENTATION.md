# Database Impact, Legacy Preservation and Implementation Plan

## 1. Scope and decision status

This document assesses likely implementation impact. It does not authorize or apply database changes. Physical table names, package boundaries, storage choices and retention periods remain subject to architecture, DBA, security and methodology approval.

## 2. Proposed logical data model

The preferred model uses normalized shared lifecycle tables plus typed detail tables for each paper. JSON may be retained for non-authoritative presentation snapshots, but core audit facts, workflow and calculations should remain constrained and queryable.

| Logical entity | Purpose and key relationships |
|---|---|
| `WP_HEADER` | One stable paper per engagement/type/scope unit; references engagement/entity; current status/version/assignments |
| `WP_VERSION` | Immutable/signed version metadata plus draft concurrency token |
| `WP_OBJECTIVE` | Objectives and final achievement assessment by version |
| `WP_RISK_CONTROL` | Risks and expected controls linked to objectives/version |
| `WP_SCOPE_POPULATION` | Scope, source, cut-off, count/value/currency and reconciliation |
| `WP_SAMPLE_PLAN` | Sampling method, rationale, size, strata, seed/tool and coverage |
| `WP_PROCEDURE_DEF` | Versioned paper-type procedure template maintained under change control |
| `WP_TEST_ITEM` | Common identity/selection/result metadata for one sampled item |
| `WP_PROCEDURE_RESULT` | Procedure result per item, performer/date, comment and applicability rationale |
| `WP_EVIDENCE` | Evidence metadata, integrity/retention/access attributes and version |
| `WP_EVIDENCE_LINK` | Many-to-many links from evidence to paper/item/result/exception/conclusion |
| `WP_EXCEPTION` | Structured condition/criteria/cause/impact/risk/disposition and status |
| `WP_OBSERVATION_LINK` | Exception-to-IAS-observation relationship and immutable link history |
| `WP_CONCLUSION` | Calculated metrics snapshot and preparer conclusion by version |
| `WP_REVIEW_NOTE` | Anchored review notes, response, closure and ownership |
| `WP_SIGNOFF` | Prepared/reviewed/approved/reopened/superseded events |
| `WP_CHANGE_HISTORY` | Append-only audit history and controlled before/after metadata |
| `WP_IMPORT_BATCH` | Population/source imports, checksums, counts, reconciliation and errors |
| Five typed detail sets | `WP_LOAN_ITEM`, `WP_VOUCHER_ITEM`, `WP_ACCOUNT_ITEM`, `WP_ASSET_ITEM`, and cash header/denomination/other-item schedules |

### 2.1 Keys, integrity and indexing

- Use Oracle sequences or identity columns; do not use `MAX(id)+1`.
- Use foreign keys for engagement, entity, header/version, test item, exception, evidence and observation references where system boundaries permit.
- Use unique constraints for paper reference/version, test-item identity within a version, procedure result per item/procedure, and denomination per count point/currency.
- Add check constraints or reference tables for status/result/risk/currency/enumeration values.
- Use decimal precision approved for IAS money, integer quantities, `DATE`/timestamp types, and explicit currency.
- Index engagement/type/status, entity/status, header/version, reviewer/status, item/header, exception/status, observation ID and evidence links.
- Use optimistic-concurrency version/timestamp on editable drafts.
- Prevent changes to signed versions through both service authorization and database controls appropriate to the approved architecture.

### 2.2 Cash-specific detail

Cash requires a count header, count-point/currency grouping, denomination rows, other accountable items, timing items and reconciliation totals. Store source quantities and balances plus formula version; persist server-calculated signed-version values for historical reproduction while recalculating and comparing during validation.

### 2.3 Procedure and template versioning

Procedure definitions, mandatory evidence rules, account-document checklists, approval matrices, denomination masters and calculation methodologies are effective-dated. A working-paper version references the exact template/configuration version used; later configuration changes do not rewrite historical conclusions.

## 3. Database and package impact assessment

| Area | Expected impact | Risk/decision |
|---|---|---|
| New tables/sequences | Shared lifecycle plus five typed detail areas | Moderate-to-high schema growth; naming/tablespace/partitioning review |
| Existing five tables | Remain unchanged and readable | Do not add destructive constraints or reinterpret columns in place |
| `PKG_AR` | Legacy procedures remain during transition; new package/API preferred for redesigned module | Avoid destabilizing broad package; decide dedicated `PKG_WORKING_PAPER` ownership |
| Transactions | New service operation should be atomic and own commit/rollback | Existing internal commits must not be copied into new procedures |
| Authorization | Retrieval/mutation must validate engagement/entity access | Define trusted authorization source and database enforcement boundary |
| Reporting | New signed-version reporting views/data sets | Ensure version-specific and masked output; avoid `SELECT *` contracts |
| Observations | Foreign/reference link to existing observation identifier | Confirm lifecycle behavior when observation status/number changes |
| Evidence | Metadata integration with approved IAS evidence storage | Confirm retention, encryption, malware scanning and access model |
| Audit logging | Extend application log and append-only history | Define sensitive-value masking and log retention |
| Data volume | Procedure results/evidence/history multiply row count | Capacity and performance test using realistic samples/attachments |

New procedures should use explicit column lists, typed parameters, bind by name, standardized result/error contracts and no dynamic SQL unless separately reviewed. They should not accept caller-supplied identity/role as proof of authorization. Read contracts should return explicit columns and paginate large grids.

## 4. Existing-record preservation

### 4.1 Preservation principles

1. The five legacy tables and their records remain unchanged, with original entered-by/date and engagement values preserved.
2. Legacy records remain readable through a clearly labelled `Legacy` view and existing exports during the retention period.
3. No legacy free-text value is silently converted into a pass/fail result, calculated figure, exception risk or supervisory sign-off.
4. The redesign never fabricates missing planning, evidence, conclusion or review metadata.
5. Any migrated copy retains source table, source primary key, source engagement, migration batch, timestamp and checksum.
6. Reconciliation proves source/target record counts and critical-field hashes by table and engagement.
7. Rollback disables the new module/mappings without deleting either legacy or new records.

### 4.2 Recommended approach: coexistence with optional controlled adoption

Use side-by-side storage. Existing legacy papers remain read-only in their original schema once the redesigned paper type is activated for an engagement. New engagements use the redesigned model. In-flight engagements receive an explicit conversion decision:

| Option | Use | Treatment |
|---|---|---|
| Complete in legacy | Near-complete engagement | Legacy remains editable until agreed cut-off, then read-only; no synthetic new paper |
| Start redesigned and reference legacy | Early/incomplete engagement | Create a new draft with approved plan; expose legacy rows as source references/evidence candidates |
| Controlled copy to draft | High-value exception approved by owner | Copy factual fields only, mark `Unverified migrated data`, require preparer validation, evidence and new sign-off |

The default should be no bulk semantic migration. Legacy rows lack enough information to satisfy the new framework and cannot become approved redesigned papers without professional work and review.

### 4.3 Legacy mapping for factual fields

| Legacy source | Factual fields eligible for reference/copy | Fields that must remain absent until completed |
|---|---|---|
| Loan Case | Loan number, amount, disbursement date, category, observation text, para text | Objective, sample rationale, procedures/results, evidence, structured exception, conclusion, review |
| Voucher | Voucher/account number, observation text, para text | Transaction/control detail, procedures/results, evidence, exception analysis, conclusion, review |
| Account Opening | Account number, account nature, observation text, para text | KYC/CDD tests, sensitive evidence, procedure results, exception analysis, conclusion, review |
| Fixed Assets | Asset name, physical-existence text, FAR-location text, difference, remarks | Typed identity/value, derived results, evidence, exception analysis, conclusion, review |
| Cash Count | Denominations, quantities, entered totals and difference | Calculated assurance, count context, witnesses, evidence, exception disposition, conclusion, review |

Legacy monetary/text calculations should be displayed as `Legacy entered value`, not recalculated or certified. The fixed Cash `NOSR -> CSR` application mapping should be reflected when reading source fields, while original stored values remain untouched.

### 4.4 Reconciliation and acceptance

- Inventory legacy row counts per table and engagement before cut-over.
- Capture checksums/hashes for critical source fields and preserve a signed reconciliation report.
- Validate a risk-based sample plus 100% of failed/ambiguous conversions.
- Confirm links open only for users with engagement access.
- Confirm old URLs redirect to the correct legacy/read-only or redesigned context without losing engagement identity.
- Obtain Internal Audit and DBA sign-off on preservation and reconciliation.

## 5. API and integration impact

Proposed endpoints should be resource-oriented and typed; exact routes follow IAS conventions after architecture approval.

| Capability | Contract behavior |
|---|---|
| Workspace summary | Authorized engagement papers, status, owner/reviewer, completion, exception/note counts |
| Paper plan/detail | Version-specific typed DTO, ETag/concurrency value, completeness indicators |
| Test items/results | Paginated queries, batch-safe draft updates, server validation/calculation outcomes |
| Evidence | Authorized upload/link/version/download; metadata and retention/classification |
| Exceptions/observations | Structured exception CRUD in draft; controlled observation link/create actions |
| Validation | Non-mutating completeness/calculation validation with field/section errors |
| Workflow | Explicit prepare, submit, return, review, approve, reopen commands; idempotency/correlation ID |
| History/export | Version-specific history and review-ready export |

Primitive action parameters and hand-built JSON strings should not be used. Every mutation returns a typed outcome, validation errors, current version/concurrency value and correlation ID. Workflow transitions are commands, not arbitrary status updates.

## 6. Implementation workstreams

| Workstream | Main outputs |
|---|---|
| Methodology and product | Approved objectives/procedures/templates, risk taxonomy, completion rules, reviewer policy |
| UX/accessibility | Tested workspace shell, five grids/detail views, responsive/read-only/export behavior |
| Security/privacy | Threat model, authorization matrix, customer-data masking, upload model, audit-log policy |
| Database | Physical model, sequences/constraints/indexes, packages, views, rollback and reconciliation scripts |
| Application | Typed domain/service/API, calculations, workflow, evidence/observation integration, UI components |
| Quality engineering | Unit, contract, integration, database, authorization, migration, performance, accessibility and UAT suites |
| Change/operations | Training, procedures, support runbooks, monitoring, release/cut-over plan and post-implementation review |

## 7. Phased implementation plan

### Phase 0 - Approval and discovery (3-4 weeks)

- Approve this design package, ownership and paper terminology.
- Validate procedures against Internal Audit methodology and applicable policies/regulation.
- Decide approver rules, retention, evidence storage, observation integration and in-flight-engagement treatment.
- Inventory legacy data volume/quality, dependencies and user roles.
- Produce threat model, data classification, non-functional targets and acceptance matrix.

**Gate:** signed design decision record and implementation authorization.

### Phase 1 - Foundation design and prototypes (4-6 weeks)

- Finalize physical data model, package/API contracts and authorization matrix.
- Prototype workspace shell, review workflow, evidence linking and export with representative users.
- Define effective-dated procedure templates, field dictionary and formulas.
- Prepare migration/reconciliation and rollback designs using masked data.

**Gate:** architecture/security/DBA review and usability approval; no production schema change before gate.

### Phase 2 - Shared framework build (6-8 weeks)

- Build header, plan, procedure results, evidence, exception, conclusion, notes, sign-offs, versioning and history.
- Implement object-level authorization, typed validation, concurrency and audit logging.
- Build common UI shell, dashboard and version-specific export.
- Automate framework-level tests.

**Gate:** maker-reviewer lifecycle passes security, integrity and accessibility tests.

### Phase 3 - Cash Count pilot (4-6 weeks)

- Build count header, denomination/other-item schedules, reconciliations, attestations and variance workflow.
- Run parallel UAT with controlled test engagements and independently recompute all totals.
- Exercise legacy side-by-side view, rollback and support procedures.

**Gate:** Internal Audit accepts calculation accuracy, evidence sufficiency and reviewer workflow.

### Phase 4 - Remaining papers (8-12 weeks)

- Deliver Loan Case, Voucher, Account Opening and Fixed Assets in risk-prioritized increments.
- Integrate approved sources/configurations and complete privacy controls for customer data.
- Validate each paper with preparer/reviewer scenarios and realistic volumes.

**Gate:** paper-specific acceptance matrix and methodology-owner sign-off for each type.

### Phase 5 - Controlled rollout (3-5 weeks)

- Activate by selected engagement/entity cohort using feature flags/configuration.
- Freeze affected legacy pages to read-only only after reconciliation and business sign-off.
- Monitor errors, workflow cycle time, exception linkage, evidence access and performance.
- Train preparers/reviewers/administrators and staff support desk.

**Gate:** operational acceptance, no critical security/data-integrity defects, reconciliation signed.

### Phase 6 - Stabilization and review (4-8 weeks after rollout)

- Resolve adoption issues, tune performance and review exception/review-note quality.
- Complete post-implementation assurance and benefits assessment.
- Decide retirement timing for legacy write paths without deleting retained records.

## 8. Test and assurance strategy

| Test layer | Minimum coverage |
|---|---|
| Unit | Formulas, completeness rules, status transitions, risk/result mappings, masking |
| Contract | DTO/schema, validation responses, idempotency, concurrency conflicts, version selection |
| Integration | Evidence/observation links, authentication/session context, transactions, export |
| Database | Constraints, sequences, authorization queries, atomic rollback, signed-version immutability |
| Security | Horizontal/vertical authorization, CSRF, injection/XSS, upload abuse, sensitive export/access logging |
| Migration | Counts/hashes, source traceability, ambiguous values, rerun/idempotency, rollback |
| Performance | Large samples, concurrent reviewers, evidence metadata, dashboard and export |
| Accessibility | Keyboard-only, focus, labels, grid behavior, errors/status, screen-reader checks |
| UAT | Complete scenarios for all five papers, return/resubmit, observation linkage, reopen/version compare |

Cash formula tests use independently prepared expected results, multiple currencies/denominations, zero values, large values, recounts, timing items, sealed packets, shortages and overages. Money tests must cover precision/rounding rules approved by Finance/Internal Audit.

## 9. Deployment and rollback

- Use configuration/feature flags by environment, entity and engagement cohort.
- Deploy additive schema first; never require immediate modification of legacy tables.
- Back up and validate recovery before any production schema/data operation.
- Run migration/reference jobs idempotently with batch IDs and reconciliation output.
- Rollback routes users to the legacy read-only/write mode agreed for the cohort while preserving all new records for investigation; do not delete partially created audit evidence.
- Database scripts require peer review, least-privilege execution, timing plan and verified rollback/forward-fix path.
- No automatic production deployment is part of this design.

## 10. Operational metrics

Track paper completion time, first-pass review acceptance, open/aging review notes, exception-to-observation linkage, unresolved exceptions at approval, evidence completeness, calculation validation failures, authorization denials, concurrency conflicts, legacy/new usage, export failures and support incidents. Metrics should assess process quality without ranking individual auditors mechanically.

## 11. Decisions required before implementation

1. Is a separate approver required after reviewer sign-off, and for which risk levels?
2. What are the approved risk-rating taxonomy and exception tolerance rules?
3. Which systems are authoritative for population, balances, approval matrices, KYC results, asset register and denominations?
4. Can existing IAS evidence storage meet working-paper security, integrity and retention needs?
5. What customer/account data may be displayed, exported and logged for each role?
6. What is the record-retention/legal-hold policy for drafts, approved versions, evidence and legacy tables?
7. Which in-flight engagements complete in legacy versus start in the redesigned module?
8. Should `Reviewed` and `Approved` be separate mandatory roles or configurable states?
9. Which procedures/evidence are mandatory by paper type, entity type and risk?
10. What package/schema ownership and reporting integration are approved by DBA/architecture?

Implementation begins only after these decisions and the design package are formally approved.
