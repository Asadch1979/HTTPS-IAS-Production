# Unified Working-Paper Architecture and Workflow

## 1. Purpose and target outcome

The redesigned module will replace five disconnected append-only registers with a common, reviewable audit-working-paper framework. Each working paper will document why work was performed, what was tested, what evidence supports the result, what exceptions arose, what conclusion was reached, and who prepared and reviewed the work.

The five supported types remain:

- Loan Case File (`LCF`)
- Voucher Checking (`VCH`)
- Account Opening (`AOF`)
- Fixed Assets (`FAS`)
- Cash Count (`CCT`)

The design does not merge their audit procedures or data. It standardizes identity, planning, evidence, exceptions, conclusions, review, history, and navigation around a paper-specific test sheet.

## 2. Conceptual architecture

```text
Engagement / Audit Program / Authorized Entity Context
                         |
              Working Paper Workspace
                         |
   +----------+----------+----------+----------+
   |          |          |          |          |
 Planning  Test Sheet  Evidence  Exceptions  Conclusion
   |          |          |          |          |
   +----------+----------+----------+----------+
                         |
       Preparation -> Review -> Approval/Lock
                         |
        Version history and application audit log
```

### 2.1 Shared domain components

| Component | Responsibility |
|---|---|
| Working-paper header | Stable identity, type, engagement, entity, period, owner, reviewer, status, current version |
| Plan | Objective, risks, controls, scope, population, sampling, audit-program reference |
| Procedure catalogue | Paper-type procedure definitions, applicability, expected evidence, display order |
| Test item | One sampled case, voucher, account, asset, or cash count sheet |
| Procedure result | Result for a procedure against a test item: Pass, Exception, Not Applicable, or Not Tested |
| Evidence | Controlled file/reference metadata linked to a paper, item, result, exception, or conclusion |
| Exception | Structured finding candidate and its disposition, risk, ownership, and IAS observation link |
| Conclusion | Coverage metrics, unresolved issues, objective-achievement rating, narrative and residual risk |
| Review note | Reviewer question/requirement, preparer response, resolution and closure evidence |
| Sign-off | Prepared, reviewed, approved, reopened, or superseded event with actor and timestamp |
| Version/history | Immutable record of material changes and state transitions |

### 2.2 Application layers

| Layer | Proposed responsibility |
|---|---|
| Razor/UI components | Common workspace shell and paper-specific typed grids; no business-rule authority |
| API/controller | Typed requests/responses, anti-forgery controls, model validation, consistent outcomes |
| Working-paper service | Authorization, workflow transitions, completeness checks, calculations, versioning, transactions |
| Repository/data access | Bind-by-name stored calls or approved ORM/data-access pattern; no UI concerns |
| Oracle package/data model | Constraints, sequences/identity, atomic persistence, authorized retrieval, reporting views |
| File/evidence service | Authorized upload/download, metadata, malware/content checks, retention and integrity controls |
| Audit logging | Before/after metadata, record/version, actor, action, outcome, correlation ID and timestamp |

Existing IAS observation and evidence capabilities should be integrated through stable identifiers, not duplicated by free-text copies. Reuse is subject to a security and retention review because current upload areas may have different authorization boundaries.

## 3. Audit planning framework

Every working paper starts with a plan. Template defaults may be supplied, but the assigned preparer must confirm or tailor them for the engagement.

| Planning area | Required design |
|---|---|
| Objective | One or more explicit objectives, each with an objective ID and achievement status at conclusion |
| Risks | Risk statement, inherent risk, relevant control, expected control frequency/owner, residual risk expectation |
| Audit-program link | Engagement procedure/checklist ID and revision; snapshot the wording used at preparation time |
| Scope | Entity/location, audit period, system/product/currency boundaries, exclusions and rationale |
| Population | Source system/report, extraction timestamp, source owner, record count, total value where applicable, reconciliation status |
| Sampling | Method, sample size, selection criteria, random seed/tool where applicable, judgmental rationale, coverage count/value |
| Assignment | Preparer, reviewer, due dates, independence confirmation and required competencies |

Plan completeness is required before a paper can move from `Draft` to `Prepared`. A reviewer may return the plan even when execution rows are complete.

## 4. Execution framework

### 4.1 Result vocabulary

Each applicable procedure result uses a controlled value:

| Result | Meaning |
|---|---|
| Pass | Evidence supports the expected control/condition |
| Exception | Evidence does not support the expected control/condition; an exception record is required |
| Not Applicable | Procedure is not relevant; rationale is required |
| Not Tested | Procedure was planned but not performed; reason and impact on conclusion are required |

Blank is an incomplete state, not a result. Result changes after preparation must be versioned and explained.

### 4.2 Calculation controls

- Quantities use whole-number types; money uses an approved decimal precision and currency.
- Source values are distinguishable from auditor-entered values and calculated values.
- Calculated values are read-only and recomputed on the server.
- Formula name/version is retained when methodology may change.
- Client calculations are for immediate feedback only; the persisted server result is authoritative.
- Totals reconcile to row detail, and overrides are prohibited unless explicitly designed, authorized, reasoned, and logged.
- Imports retain source file/report ID, extraction time, row reference, and import batch.

### 4.3 Evidence controls

Evidence may link at paper, sample-item, procedure-result, exception, or conclusion level. Required metadata includes evidence ID, title, evidence type, source/custodian, source date, received/created date, classification, file/reference location, integrity hash for stored files, uploader, upload time, and retention class.

Minimum controls:

- Enforce engagement-level authorization for upload, view and download.
- Allow links to existing authorized IAS evidence without copying files unnecessarily.
- Prevent replacement of evidence used in a signed version; upload a new version instead.
- Record evidence access and preserve metadata after paper lock.
- Scan uploaded files, restrict type/size, normalize names, and store outside executable web paths.
- Clearly mark external links and verify availability before sign-off.

## 5. Exception and observation framework

An exception is a working-paper result. An IAS observation is a formally raised engagement issue. They are related but not interchangeable.

### 5.1 Exception fields

| Group | Fields |
|---|---|
| Identity | Exception ID, paper/item/procedure links, status, owner |
| Analysis | Criteria, condition, cause, consequence/impact, control failure |
| Risk | Likelihood, impact, risk rating, financial/exposure amount and currency |
| Disposition | Isolated/systemic indicator, auditee explanation, auditor evaluation, action/due date |
| Observation | IAS observation ID, link status, linked by/at, unlink reason/history |
| Resolution | Resolved/unresolved, resolution evidence, resolved by/at, reviewer acceptance |

Risk rating must use the approved IAS risk taxonomy. A paper cannot be approved with an `Exception` test result lacking an exception record. An unresolved exception may remain at approval only when the conclusion identifies it and policy allows it.

### 5.2 Observation linkage

- Create or link an observation through authorized IAS observation services.
- Store the observation's immutable ID, not only memo/para text.
- Display current observation number/status from its source, while retaining the value seen at paper sign-off in the version snapshot.
- Many exceptions may support one observation; one exception may link to at most one active observation unless methodology owners approve otherwise.
- Unlinking requires a reason and review permission and never deletes history.

## 6. Conclusion framework

The conclusion page is generated from execution data and completed by the preparer.

| Section | Content |
|---|---|
| Coverage | Population and sample counts/values, procedures planned/performed, evidence completeness |
| Results | Pass/Exception/Not Applicable/Not Tested counts and rates; paper-specific reconciliations |
| Issues | Unresolved exceptions, linked observations, overdue actions, limitations |
| Objective assessment | Achieved, Partly Achieved, or Not Achieved for each objective, with rationale |
| Overall conclusion | Concise professional judgment, residual risk, scope limitations and follow-up needed |
| Completion checklist | All required fields/evidence/results present; calculations valid; exceptions dispositioned |

Metrics are system-generated from the signed version. Narrative cannot contradict calculated unresolved-issue status without a prominently displayed override explanation and reviewer acceptance.

## 7. Workflow and supervisory review

### 7.1 Status model

```text
Draft -> Prepared -> In Review -> Reviewed -> Approved/Locked
  ^          |           |           |
  |          +-----------+-----------+
  |                    Returned
  |
Reopened (creates a new editable version from an approved version)
```

| Status | Editable by | Permitted actions |
|---|---|---|
| Draft | Assigned preparer/delegate | Edit plan and execution; add evidence/exceptions; save |
| Prepared | No content editing | Preparer sign-off; submit to assigned reviewer |
| In Review | Reviewer adds notes; content remains read-only | Add/close review notes; accept or return |
| Returned | Assigned preparer | Address notes and edit; resubmit as incremented draft version |
| Reviewed | No content editing | Reviewer sign-off; submit to approver or approve if role permits |
| Approved/Locked | Read-only | View/export; authorized reopen request only |
| Reopened | Assigned preparer | New version; prior approved version remains immutable |

`Prepared` may be represented as a sign-off event followed immediately by `In Review`; it remains a distinct auditable state. Cancellation/supersession requires a reason, authority, and retained history. Hard deletion is not available for signed papers.

### 7.2 Segregation and permissions

- Preparer cannot review or approve the same version.
- Reviewer must have engagement access and paper-review permission.
- Approver requirement is configurable by engagement/risk; approval cannot be silently delegated.
- Reopen permission is separate from edit permission.
- Administrators may configure templates and assignments but cannot alter signed audit content outside the controlled workflow.
- Every operation revalidates object-level engagement and entity access on the server and database boundary.

### 7.3 Review notes

Review notes carry note ID, severity, section/field anchor, text, author/date, assigned preparer, due date, response, response evidence, status, resolver/date and closure rationale. Notes are `Open`, `Responded`, or `Closed`. Only the reviewer (or authorized replacement) closes a note. Approval is blocked while required notes remain open.

## 8. Consistent interface design

### 8.1 Workspace shell

```text
[Breadcrumb: Engagement > Working Papers > Paper]
[WP reference] [Paper name] [Version] [Status] [Owner] [Reviewer] [Due]
[Entity] [Audit period] [Last saved] [Open notes] [Unresolved exceptions]

[Plan] [Test Sheet] [Evidence] [Exceptions] [Conclusion] [Review] [History]

Contextual page content

[Save draft] [Validate] [Sign prepared / Return / Approve]  (role/status dependent)
```

The header remains visible without consuming excessive vertical space. Tabs show completeness/error badges. The primary action reflects the workflow state; secondary actions appear in a menu. Read-only/locked state is unmistakable.

### 8.2 Interaction standards

- Use the engagement selector/context already established by IAS, then derive paper access on the server.
- Use a left frozen identity column in wide test grids and permit column groups to collapse.
- Provide keyboard-accessible grid editing, filters, saved views, and clear validation summaries.
- Use drawers for item detail/evidence and a modal only for short confirmations.
- Show source, auditor, and calculated values with distinct labels, not color alone.
- Use controlled selectors for enumerations and date/number controls for typed values.
- Autosave drafts with visible saved/error state; workflow transitions always require explicit confirmation.
- Never expose unsupported Update/Delete controls. Actions appear only when authorized and functional.
- Encode all displayed content, preserve line breaks safely, and avoid building HTML from returned values.
- Exports include reference, version, status, page numbering, sign-offs, evidence index, exceptions and generation timestamp.

### 8.3 Common dashboard

The engagement Working Papers page lists all five papers with owner, status, completion percentage, sample coverage, exception count, open review notes, last activity and due date. Sequence navigation remains available, but users are not forced through papers in a fixed order. A paper may be marked `Not Applicable` only with scope rationale and reviewer approval.

## 9. Versioning, audit trail and concurrency

- A working-paper reference remains stable; each return/reopen creates or advances a version according to approved policy.
- Signed snapshots include planning, procedures/results, calculations, evidence metadata, exceptions, conclusion and sign-offs.
- Use optimistic concurrency for draft edits. A stale client receives a conflict response and must refresh/merge; last-write-wins is prohibited.
- History records actor, timestamp, action, entity/engagement, record/version, reason, correlation ID, changed field names and protected before/after values.
- Sensitive field values should be masked or excluded from general audit logs while remaining available through authorized version history.

## 10. Non-functional requirements

| Area | Requirement |
|---|---|
| Security | Object-level authorization, least privilege, anti-forgery, output encoding, secure upload, audit logging |
| Privacy | Mask customer/account identifiers in list views and exports according to role; log sensitive access |
| Integrity | Server validation/calculation, database constraints, atomic transactions, immutable signed versions |
| Availability | Draft recovery, idempotent transitions, evidence-link health checks, graceful error messages |
| Performance | Paginated/virtualized grids; indexed engagement/status/item queries; bounded evidence metadata loads |
| Accessibility | WCAG 2.1 AA target, keyboard workflow, focus management, semantic labels, non-color status cues |
| Retention | Configurable approved retention class; legal hold; no purge independent of linked engagement/observation |
| Reporting | Review-ready PDF and controlled data export generated from a specified signed version |

## 11. Definition of done for the framework

A paper type is ready for rollout only when it can demonstrate end-to-end planning, typed execution, evidence traceability, exception-to-observation linkage, calculated conclusion metrics, maker-checker review, immutable approval, authorized reopening, complete audit history, object-level authorization, and preservation of accessible legacy records.
