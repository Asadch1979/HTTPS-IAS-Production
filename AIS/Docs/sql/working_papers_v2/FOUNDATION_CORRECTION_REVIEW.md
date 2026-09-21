# Working Paper V2 Foundation Correction Review

## Authorization boundary

Every public package procedure now reaches `ASSERT_ENGAGEMENT_ACCESS` either directly (`P_CREATE`,
`P_GET_WORKSPACE`) or through `ASSERT_ACCESS` (all mutations and workflow actions). The engagement must
exist in `T_AU_PLAN_ENG`, its entity must equal the authenticated session entity, and a non-privileged actor
must have an active row in `T_AU_AUDIT_TEAM_TASKLIST`. Roles 1 and 2 use the existing privileged access
policy. An assigned reviewer must always be an active engagement-team member.

Review notes are restricted as follows: the assigned reviewer creates notes during `IN_REVIEW`; the assigned
preparer may move an existing note to `RESPONDED` with response text; only the assigned reviewer may close a
responded note. All paths still repeat engagement authorization.

## Completeness and versioning

`SUBMIT`, `REVIEW` and `APPROVE` call the same database completeness gate. It requires complete planning and
sampling metadata, at least one item, final item results, persisted server calculations, evidence for each
applicable item, an exception record for each exception result, a disposition for every exception, and a
completed objective assessment and conclusion. Review and approval additionally require all notes closed.

`REOPEN` never changes an approved row. It requires privileged engagement access and a reason, inserts a new
header version, and clones items, exceptions and evidence while remapping all child identifiers. The approved
source remains linked through `ROOT_WP_ID` and `SUPERSEDES_WP_ID` and remains read-only.

## C# to Oracle alignment

All procedures begin with the common inputs `P_AUDITOR_ENTITY_ID NUMBER IN`, `P_ACTOR_PPNO VARCHAR2 IN`,
and `P_ROLE_ID NUMBER IN`; `DBConnection.WorkingPaperV2.AddContext` supplies those exact names and compatible
ODP.NET types. The auditor entity is context only; the working-paper `ENTITY_ID` is the audited entity derived
from the engagement. Procedure-specific mappings are:

| C# method | Oracle procedure | Inputs after context | Output |
|---|---|---|---|
| `CreateWorkingPaper` | `P_CREATE` | `P_ENG_ID` Int64, `P_PAPER_TYPE` Varchar2, `P_REVIEWER_PPNO` Varchar2, `P_DUE_DATE` Date | `O_RESULT` RefCursor |
| `GetWorkingPaperWorkspace` | `P_GET_WORKSPACE` | `P_WP_ID` Int64, `P_ENG_ID` Int64, `P_PAPER_TYPE` Varchar2 | Seven output RefCursors in package order |
| `SaveWorkingPaperPlan` | `P_SAVE_PLAN` | `P_WP_ID` Int64, `P_ROW_VERSION` Int64, `P_PLAN_JSON` Clob | `O_RESULT` RefCursor |
| `SaveWorkingPaperItem` | `P_SAVE_ITEM` | `P_WP_ID`, `P_ITEM_ID` Int64; `P_ITEM_REF` Varchar2; `P_DETAILS_JSON`, `P_CALC_JSON` Clob; `P_RESULT`, `P_COMMENT` Varchar2; `P_ROW_VERSION` Int64 | `O_RESULT` RefCursor |
| `SaveWorkingPaperEvidence` | `P_LINK_EVIDENCE` | IDs Int64; evidence/title/type/source Varchar2; evidence date Date | `O_RESULT` RefCursor |
| `SaveWorkingPaperException` | `P_SAVE_EXCEPTION` | IDs/version Int64; narrative/risk/owner/status Varchar2; due date Date | `O_RESULT` RefCursor |
| `SaveWorkingPaperConclusion` | `P_SAVE_CONCLUSION` | `P_WP_ID`, `P_ROW_VERSION` Int64; `P_CONCLUSION_JSON` Clob | `O_RESULT` RefCursor |
| `SaveWorkingPaperReviewNote` | `P_SAVE_REVIEW_NOTE` | IDs Int64; section/note/response/status Varchar2 | `O_RESULT` RefCursor |
| `ApplyWorkingPaperWorkflow` | `P_WORKFLOW` | `P_WP_ID`, `P_ROW_VERSION` Int64; `P_ACTION`, `P_REMARKS` Varchar2 | `O_RESULT` RefCursor |

Every C# command uses `BindByName = true`. `003_verify_working_paper_install.sql` reports `USER_ARGUMENTS`,
package compilation errors and database dependencies so the DBA can verify the deployed schema before release.

## Deployment decision gates

- Confirm whether final approval must be performed by a third person. The current interim rule allows the
  assigned reviewer to approve after reviewer sign-off.
- Confirm whether roles 1 and 2 should retain unrestricted engagement access for this module.
- Confirm that evidence and observation identifiers have authorization APIs suitable for validation. V2 stores
  links only; cross-module identifier validation remains a release dependency.
- Oracle compilation was not executed from the application workspace. The DBA must run scripts in an approved
  non-production schema and require zero rows from `USER_ERRORS` before release.
