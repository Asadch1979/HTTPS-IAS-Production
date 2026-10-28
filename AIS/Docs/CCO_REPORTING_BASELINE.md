# CCO source baseline and staged verification

Verified 2026-10-28 against main 1239d510 (confirmed via connected GitHub); setup checkpoint 930394fe. Branch codex/cco-organizational-reporting retains setup docs and unrelated working edits.

## Existing contracts
CCO definitions were glance, position, settlement, risk, aging, reports. CCO glance keys: openParas, zeroCycleParas, over3CycleParas, over1YearParas, settlementRatio, criticalItems. Position keys: totalCases/openParas/settledTotal/highRiskParas. Settlement: settledMtd/settlementMtdRate. Sections include trend, risk, aging, cycle and bounded critical items.

DBConnection.ComplianceDashboard uses ReadExecutiveKpiLifecycle and dated distribution/critical readers. TotalCases = opening + additions; settlement rate = settled / total when denominator positive. Cycles are current master values for selected eligible portfolio. Historical stocks are estimates; distributions use End Date. No new formulas or Oracle source edits in phase 1.

MVC validates parent dates-only filters, ISO dates, nonfuture end and <1200 months; defaults Pakistan yesterday and Jan 1 of that year. CCO rejects Snap selectors. Session resolver requires role 38 or independently verified role-1 Super User; fixed-role CCO rejects groupRootId. ValidateExecutiveDashboardSession binds user/role/posting/session/context assignment. Page 1000302 and exact API grants are independent gates. Existing report controller resolves scope, page/API permission, reports-definition allowlist B/C/D and entity narrowing. Non-null view must be reports. PDF is Super User only; CSV buttons also remain Super User only. No permissions are changed.

Reports B/C/D retain V_EXEC_REPORT_SOURCE period stock/movement predicates (end exclusive = selected end +1), risk and settlement attribution. C uses reduced mapping keys, unique AUDIT_BY matching for eligible HO units, and existing special type exclusions 6/21/22/28. V_EXEC_ENTITY_SCOPE currently chooses nearest type-18 Group, type-3 Division and type-21 GM using ancestors; these are source conventions, not verified live taxonomy. Never infer auditee ownership from AUDITED_BY.

## Staging evidence required
Read-only cco_reporting_preflight.sql is a DBA review aid, not an authorization mechanism. Run only with explicitly approved staging identity and bounded dates. Capture taxonomy, duplicate paths, GM ambiguity, hierarchy conflicts, source identities and stock counts. Also snapshot active page/API grants using approved administrative tools and compare authenticated B/C/D and lifecycle responses for identical dates. No database connection or baseline totals were obtained locally. Raw-source diagnostics are not user-authorized dashboard totals.

## Phase 2 package structure (design only; no executable DDL)
PKG_CCO_REPORTING spec/body will expose P_GET_ORG_FILTERS, P_GET_ORG_POSITION, P_GET_ORG_PERFORMANCE, P_GET_ORG_SETTLEMENT, P_GET_ORG_EXCEPTIONS per saved design. Final signatures must include a DBA-approved authenticated context binding; dashboardType alone is insufficient.

Private helpers: assert live session/context and permitted CCO scope; validate dates/enum/positive IDs; independently verify Group/Division/GM path. Common qualified source: authorized distinct COM_ID facts -> unique validated entity path -> period flags -> one GROUPING SETS aggregation. Reduce active mapping to unique keys before fact joins. Keep direct-to-Group/Division and conflict/unmapped categories without guessing GM. HO AUDIT_BY fallback only when unique and compatible. Retain separate AUDITED_BY dimension. Filters narrow authenticated scope before aggregation. Bound cursors/pagination; return quality and reconciliation differences; unknown history remains unknown. Never call KPIs per child.

Gate before implementation/activation: signed taxonomy and mapping rules, approved Oracle context/scope mechanism, unique COM_ID reconciliation against both source views and B/C/D, authorization negative tests, package compile/USER_ERRORS and execution plans on staging. Then dedicated DTO/controller/exact inactive permission registrations can be reviewed. No grants, deployable package, source-view edits or new APIs are included now.
