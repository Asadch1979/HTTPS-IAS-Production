# PKG_AR and PKG_HD source review — 20 October 2026

## Review scope and limitations

Reviewed the main-branch Oracle package sources and their ASP.NET Core call sites for observation creation, Team Lead membership, submission, status changes, draft/final para assignment, pre-conclusion, conclusion, compliance copying, and audit-log provenance. This is **source-level** verification only. No access to the target Oracle instance, deployment, Oracle compilation or live integration tests was available. **Do not deploy these package bodies without comparing against the active DB package and compiling in a non-production schema.**

## Changes committed on main

- `c79c265d` PKG_AR: bind Team Lead authorization to the requesting PPNO and active task/engagement; remove unused role SELECT and premature commits from observation drop; disallow duplicate drops; require active Team Lead assignment for memo updates, draft and auditee submission; verify stored low-risk severity in legacy Team Lead settlement.
- `ba2f4d8a` PKG_HD: check the authenticated user/role and audit-performing office before pre-submission and audit conclusion; require the observation to belong to the supplied engagement in legacy finalization.
- `c1eaad33` PKG_AR: preserve actual low-risk Team Lead settlement as `ISSUE_RECTIFIED=1`, with settlement timestamp and PPNO, for downstream conclusion; require head actions to remain in authenticated entity hierarchy.
- `9210f1b0` PKG_HD: require office/role ownership in gist/recommendation and final-para editing procedures.
- `c4f53f9d` PKG_HD: select the latest observation text once per observation when concluding; add audit-paras by observation ID rather than potentially nonunique memo number.
- `9331d006` PKG_AR: require active assigned auditor and matching `ENTEREDBY` for new observation creation (including CAD); require active assignment for observation editing.
- `80b3af87` PKG_AR: exclude inactive tasks from the audit dashboard dropdown; reject unsupported legacy departmental-head status actions.
- `4ef4e8c0` PKG_AR: require newly created observations to enter status 1; prohibit editing already rectified status-4 and final/settled/dropped observations using the general edit procedure.
- `c8432a61` PKG_HD: record the actual caller `P_NO` rather than constant `1` in status-14 audit-conclusion log.

Package **specifications/signatures were preserved**. Changes to the package body are intended to tighten authorization and protect data integrity, not widen grants.

## Remaining high-priority issues — not automatically rewritten

1. **Non-atomic conclusion:** `PKG_HD.P_audit_Concluding` commits within its observation-transfer loop and across dependent updates. A late error can leave observations copied to `AIS_T_AU_POST_COMPLIANCE` while the engagement is not conclusively finalized (or vice versa). Design and test a single consistent transaction; check whether `PKG_SM.P_Store_Sample_data` commits internally. Do not casually remove commits from a live multistep workflow.
2. **Concurrent ID allocation:** Several procedures use `MAX(id)+1`, including `AIS_T_AU_POST_COMPLIANCE.COM_ID`, `T_AU_PLAN_ENG_LOG` and `T_AUDIT_PARA`. Simultaneous sessions can collide. Inventory actual constraints and sequences in Oracle before migration; the history sequence is not an automatic replacement for `COM_ID`.
3. **Legacy `PKG_HD.P_Finalise_para`:** still relies on historic role-15 checks and has no authenticated `ENT_ID` parameter. Its observation-to-engagement mismatch is now blocked, but complete authorization requires inspecting its actual callers and grant model. Do not add a permissive fallback.
4. **Responsibility management:** `PKG_AR.P_responibilityassigned` mutates responsibility records by supplied IDs, with no evident per-engagement ownership check inside that procedure. Verify active call paths and enforce appropriate actor/engagement scope before exposing it more broadly.
5. **Related auditee selection:** New CAD observations accept `BRANCHID`; standard observations accept `OT_ENT_ID`. Verify the selected auditee/branch belongs to the engagement's authorized audit scope. Existing active-auditor checks establish *who* can create, but not necessarily *which related auditee* can be selected.
6. **Draft-para business policy:** `P_Add_Observation_To_Draft` intentionally allows a currently assigned Team Member or Team Lead. Confirm whether moving status-3 observations to draft should be Team-Lead-only; this was not changed without an approved workflow decision.
7. **Historical consistency:** Older status-4 observations may have `ISSUE_RECTIFIED=0` or missing `STELLED_ON`. Do not mass-update blindly; reconcile against approved pre-finalization definitions and actual observation history, then prepare a reviewed, auditable data correction if required.
8. **Role mapping:** Departmental finalization currently accepts existing role IDs 6, 7, 15, whereas other IAS contexts may use different Department Head roles (e.g. FAD role 5). Confirm authorized workflow identities in `T_GROUPS`, active postings and call sites before changing the approved role list.

## Oracle and browser QA required

1. Back up active `PKG_AR`, `PKG_HD` and dependent view definitions; compare with main branch. Compile the revised bodies individually in a non-production Oracle 18c/19c schema.
2. Query `USER_OBJECTS` and `USER_ERRORS` after each compilation. Check public procedures retain their original argument signatures.
3. Test actual Team Lead / Team Member / inactive Team Lead / wrong engagement, and severity 3 versus non-3, including forged request risk and invalid status transitions. Unauthorized writes must fail with no data changes.
4. Check status-4 settlement sets `ISSUE_RECTIFIED=1`, `STELLED_ON`, `SETTLED_BY`. Check `P_audit_Concluding` copies the flag and converts status 4 to 9 exactly once.
5. Test Head finalize/settle status 8/9 for own and subordinate audit offices; reject sibling/outside offices and concluded engagements.
6. Confirm source observation with multiple `T_AU_OBSERVATION_TEXT` rows produces one post-compliance row. Check duplicate `COM_ID`, `NEW_PARA_ID`, final para numbers, and audit-para rows under concurrent usage.
7. Test pre-submission, finalization, report issuance, compliance copying, engagement status 14 and status-14 audit-log PPNO together, including simulated mid-transaction errors.
8. Reconcile full-period issues, rectifications, para added, settlements, outstanding and same-period additions/settlements against the approved `V_EXEC_KPI_LIFECYCLE` rules.
9. Check legacy views/controllers that consume the touched procedures. Run focused application tests. **No live test passed claim is made by this review.**

## Deployment decision

**Code changes committed; database release unverified.** Resolve compilation, targeted authorization tests, live-data reconciliation, and the known non-atomic-conclusion risk before declaring the complete workflow production-safe.
