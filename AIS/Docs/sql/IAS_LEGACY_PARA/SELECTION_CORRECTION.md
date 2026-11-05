# Legacy selection and historical mapping correction

Source review only, 2026-11-04. No Oracle connections, SQL execution, builds,
tests, application execution, activation or deployment were performed.

## Baseline and selection

Reviewed pre-retirement `89c4e09^` view, JavaScript, DBConnection.HD methods and
PKG_HD selectors; retirement and redesign history through PR 367 and main
`8e0fa09008b07152fadcc2e49b44b0c73f9414e2`. The original type selector uses
AUDITABLE='A', direct audited_by_enitity for offices 112242/112248, and
V_GET_PARENT_OFFICE for other offices. Entity selection uses AUDITBY_ID,
TYPE_ID, AUDITABLE='Y', DESCRIPTION, and no ACTIVE filter.

The page now loads relation/entity types, read-only reporting office, and
entities through the protected Lookup endpoint. Audit By comes exclusively
from the active selected assignment. DESCRIPTION falls back to NAME, with code.
The same eligibility predicates govern entities and posting scope.

T_AUDITEE_ENTITIES_MAPING.AUDITEDBY is separate corroborating evidence, never
a replacement authority. Entity-management profile source exposes both values
and multiple mapping rows; its diagnostic sources explicitly flag disagreement.
Any non-null conflicting mapping denies eligibility. Inactive entities require
at least one matching mapping in addition to the original master/type rules.
This permits retained inactive organizational records, not inferred historical
transfers. Missing or contradictory authority fails closed. No live records or
historical effective dates were inspected or invented.

## Posting matrix

| Submitted Audit By | Treatment |
| --- | --- |
| 112242, 112248 | Original CAD/HO destination was T_AU_OBSERVATION_OLD_CAD_PARAS. New submission and authorization disabled with CAD_HO_UNSUPPORTED. Existing requests remain reviewable and returnable/rejectable by their owning office. |
| Other eligible offices | Existing atomic FAD master/text/compliance/responsibility/attributes contract, subject to explicit permissions, approved allocator bindings and recognition policy. |

No direct-insert procedure is restored. No Volume field or requirement is added.

## Attribute mapping

| Input/attribute | Staging and authorized destination |
| --- | --- |
| Entity type/entity | ENTITY_TYPE_ID/ENTITY_ID; FAD TYPE_ID/ENTITY_ID and compliance equivalents; derived and checked against current scope |
| Audit By | AUDITED_BY from selected context; immutable request ownership; FAD/compliance AUDITED_BY |
| Audit nature/year | NATURE_ID/AUDIT_YEAR; FAD NATURE_OF_AUDIT/AUDIT_PERIOD and compliance period |
| Para number | PARA_NO to FAD/compliance PARA_NO |
| Gist | GIST CLOB and payload gist; FAD and compliance GIST_OF_PARAS; never inferred from heading |
| Historical annexure details | ANNEXURE_DETAILS CLOB and payload annexureDetails; FAD ANNEXURE |
| Modern annexure | ANNEXURE_ID; numeric ANNEX and derived risk |
| Heading | HEADING and permanent attributes payload; not historical gist |
| Rich observation | OBSERVATION_HTML and payload; FAD_TEXT.PARA_TEXT |
| Amount/instances | AMOUNT_INVOLVED/NO_OF_INSTANCES; FAD equivalents and compliance AMOUNT/NO_OF_INSTANCES |
| Category/subchecklist/checklist | Staging hierarchy, payload; FAD PROCESS_DETAIL receives checklist ID |
| Risk | Derived from annexure; staging/FAD/compliance RISK |
| Reference | REFERENCE_ID and permanent attributes payload |
| Responsible persons | Payload and responsibility table; role/name remain in attributes payload |
| Attachments | Validated bytes and metadata remain in payload/history/permanent attributes |
| Maker/context/timestamps | Existing staging and history; original maker/date retained for FAD ENTERED_BY/ENTERED_ON |
| Recognition/status | Approved recognition policy supplies PARA_ADDED_ON; IND='O', cycle 0, status 8, stages 13/21 and update status 2 unchanged |
| IDs/references | Existing fail-closed approved allocator and REF_P contract unchanged |

Historical text has a 4000-character request limit; destination-anchored PL/SQL
variables additionally reject incompatible target capacity without truncation.
Fresh staging DDL includes nullable historical fields so old requests can be
reviewed. The separate upgrade source adds only these columns. Do not apply
both creation and upgrade sources. Old requests need maker correction, not a
heading-to-gist backfill. History is never rewritten.

## Ownership and permissions

LIST/DETAIL use submitted AUDITED_BY, with a visible reassignment notice. The
new office cannot see the old office's request. The original office can review,
return or reject with recorded remarks/history; it cannot authorize an entity
outside current scope. There is no automatic transfer. Resubmission preserves
AUDITED_BY and revalidates the original and selected entities.

The controller uses explicit cached page/API grants instead of the generic
permission service's Super User shortcuts. Oracle additionally requires the
exact selected role's active route/verb grant and active page dependency.
This prevents the generic PKG_LG team-role union from widening legacy authority.
PKG_AD already preserves inactive grants and explicit denial; no global grants
or security-default changes are needed. Pre-existing manifest edits are left
uncommitted with their original owner. Existing canonical routes, CSRF, JSON
401/403, safe references, fixed public messages and asset versioning remain.

## Files and unresolved dependencies

- ExecutionController: replace eager historical type loading with protected API loading.
- LegacyParaRequest model: separate gist/details and allow context/type lookups.
- Add_Legacy_Para view: read-only office and separate historical fields.
- Maker JavaScript: selection resets, stale entity-response guard, correction restoration.
- Shared request JavaScript: distinct review fields, scope notice, fixed error-code messages.
- LegacyParaRequestsController: explicit permissions and safe business error codes.
- PKG_IAS_LEGACY_PARA: scope parity, ownership, CAD/HO guard and historical posting.
- Staging DDL and historical-field upgrade source: independent historical columns.
- DBConnection.LegacyParaRequests remains unchanged: it already passes server-session identity only.

Unresolved: CAD/HO replacement posting contract; approved allocator bindings;
approved recognition dates; historically effective ownership transfer policy;
deployed schema capacities and mapping quality. Features remain disabled by
default. No claims of compilation, runtime validity or deployment are made.
