# PKG_RPT.R_IAS_PARA_POSITION — integration guidance

**Prepared against repository main on 21 Oct 2026; NOT deployed or Oracle-compiled.**

## Source-of-truth warning

The live IAS application calls `PKG_RPT.R_IAS_PARA_POSITION` using the parameters `P_NO, ENT_ID, R_ID, R_T, S_DATE, E_DATE, IO_CURSOR`, but the previously checked-in `AIS/Docs/sql/PKG_RPT.sql` **did not contain that production member**. The member now staged in the **package specification and package body** of this file is a **candidate implementation derived from the legacy read-only reference** `R_IAS_PARA_POSITION.sql`, not a verified copy of the production member.

**Never compile/replace the complete checked-in PKG_RPT package in production until all existing live members and customizations are compared and reconciled.** A complete `CREATE OR REPLACE PACKAGE BODY` can erase live-only procedures. Do not run the standalone reference file to fix the package member either.

## Scope parity with PKG_RPT.R_FAD_MONTHLY_REVIEW

- `R_ID IN (1,3,5)`, `R_T = -1`: one row per audit entity type; only audit types B/D, following the monthly-review All convention.
- `R_ID IN (6,7)`: own audit-performing office, `AUDITED_BY_ID=ENT_ID`, following the monthly-review department branch.
- `R_ID IN (43,44)`: compliance reviewer/approver mapping for logged-in `ENT_ID`, selected `R_T`, using `EXISTS` to avoid multiplying fact rows.
- `R_ID IN (15,16)`: audit-performing office `ENT_ID` and requested entity type.
- `R_ID IN (1,3,5)`, `R_T>0`: selected entity type, following the monthly-review selectable-type branch.
- Other roles: fail closed instead of copying the historical monthly-review catch-all that can return data outside an audit-office scope. Confirm any additional intended roles before enabling them.
- `P_NO`, `R_ID` and `ENT_ID` are checked against a currently active user/role/posting or valid active context; the C# reader must continue passing values from the authenticated IAS session, never from query parameters.
- Preserve `V_EXEC_KPI_LIFECYCLE` for approved `ISSUE_IDENTIFIED`, `ISSUE_RECTIFIED`, `PARA_ADDED`, same-period settlement and end-position KPI definitions. This source cannot reconstruct information absent from post-compliance history.

## Manual Oracle 18c / PL/SQL Developer F8 integration

1. Use existing **read-only** `AIS/Docs/sql/extract_pkg_rpt_spec.sql`, `extract_pkg_rpt_body.sql` and `extract_pkg_rpt_arguments.sql` in separate SQL Windows. Save complete CLOB output (not grid-truncated snippets), before making changes.
2. Compare **production** `R_IAS_PARA_POSITION`, including full signature, calculation logic, and authorized user/role branches, with this candidate member. Preserve every unrelated package member. Do not silently discard stronger production-specific restrictions or newer KPI changes.
3. Review the live `R_FAD_MONTHLY_REVIEW` semantics and confirm role codes 1/3/5, 6/7, 43/44 and 15/16. Review effective contexts. Check selected entity types and agreement on bank-wide `R_T=-1` for roles 1/3/5.
4. Merge only the declaration and body of `R_IAS_PARA_POSITION` into **copies of the exported live package spec and body**. Compile each complete package unit separately in PL/SQL Developer with F8, first in a controlled nonproduction schema. No SQL*Plus `/` delimiter inside an F8 SQL Window unit.
5. Run `USER_ERRORS`, `USER_OBJECTS`, and `USER_ARGUMENTS` checks for the actual `PKG_RPT`. Confirm all existing callers and package members remain usable. Only then schedule controlled production deployment.

## Required tests

- Roles 1/3/5 with `R_T=-1`: grouped entity types; no duplicated issue or para facts; reconcile totals.
- Roles 6/7: `AUDITED_BY_ID=ENT_ID`, no sibling department leakage even when `R_T=-1`.
- Roles 43/44: compliance reviewer assignment and `R_T` restriction; outside assignments denied.
- Roles 15/16: same office and selected entity type. Other roles fail closed pending explicit authorization approval.
- Invalid or stale user context and altered `P_NO`, `R_ID`, `ENT_ID` parameters fail. Browser cannot supply posting/role.
- Cross-check opening, issue identified, issue rectified, para added, settled-compliance, settled-audit, closing outstanding, R1/R2/R3 and same-period events against approved dashboard facts for identical date ranges.
- Confirm no columns are missing from `IO_CURSOR` for `DBConnection.IASParaPosition.cs`.
- Test no-record, multiple reporting-office mappings, null dates, role-specific All-type selection and 2024 historical boundary.

## Status

**GitHub source staged only**. No claim of production DB update, compilation or approval.
