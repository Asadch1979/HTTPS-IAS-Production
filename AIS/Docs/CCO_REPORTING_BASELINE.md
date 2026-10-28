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

## Phase 1 validation (2026-10-28)
- Application: dotnet build AIS/AIS.csproj --no-restore -v quiet: 0 errors, 57 existing warnings.
- dotnet run --project tests/cco-reporting/CcoReporting.Checks.csproj: 52 compiled checks pass. Includes seven definitions, all Overview dataset entitlements, dated legacy redirects, pending views with no datasets, CCO-only navigation, wrong roles/missing API, granted-route A/E denial, granted-route PDF denial, revoked sessions and invalid page/date/root/Snap requests. Uses in-memory sessions; never queries Oracle. Harness targets .NET 9 because the existing application dependency graph references System.Text.Json 9; application remains net8.0.
- node --test tests/cco-reporting/source.test.cjs: 2 pass. Protected readers/controllers/permissions, non-CCO parents, SQL packages and non-CCO definitions remain baseline-equivalent.
- Chrome headless fixtures: tests/cco-reporting/client.browser.cjs passes with bundled Playwright. Real shipped clients tested: Overview seven dataset calls, lifecycle display and historical warning; report B/C/D each makes one canonical reports request, retains dates and navigation, hides exports for ordinary CCO, invalidates stale output; summary zero versus unavailable. These are controlled fixtures, not an authenticated live IAS smoke test.
- Existing executive-dashboard Node suite: 94 tests, 77 pass, 14 fail, 3 skipped. Identical failure names/counts reproduced against untouched checkpoint 930394fe in a detached baseline checkout. No failures introduced; unrelated stale expectations/source alignment failures remain outside this phase.
- Not executed: approved Oracle preflight/compilation, authenticated successful report reads, staging totals, live role/context revocation matrix, live IAS/browser/mobile acceptance and PDF visual acceptance. No staging target supplied.

Report D's optional client-side Report C benchmark request is suppressed only on CCO so that each top-level report loads solely its assigned report. Its rankings, Oracle results and PDF server behavior are unchanged; CCO has no separate overall benchmark until an approved D response provides one.

Assessment: Phase 1 is ready for staging review with explicit pending organizational data and the above live acceptance gates. It is not approved for production release. Rollback: revert the Phase 1 presentation commit; no schema/grant rollback is needed. Preserve the original setup design and staging documentation.

## PR 349 corrections — supersedes original Phase 1 lifecycle/benchmark notes
Review correctly identified that GetKpiLifecycle is TOP-only. Original browser success payloads did not test endpoint eligibility; their success was not evidence of CCO authorization. That controller restriction and all protected server code remain unchanged.

- Overview now uses GetKpiSummary plus GetSettlementTrend/GetRiskSummary/GetAgingSummary/GetCycleSummary/GetCriticalItems only. It displays supported summary fields (combined opening + additions, period settlements, closing open, high risk, age, cycles, settlement rate and critical count). Separate opening/additions/lifecycle-quality fields are not returned by that API and are not displayed. If later required, a dedicated CCO API must undergo separate page/route/role/session/scope authorization review; never broaden TOP's endpoint.
- Rankings retains the established Report D -> Report C benchmark lookup using the same canonical reports view, CCO perspective and selected dates. No new API, grant or SQL was added. B/C remain independently selected tabs; D's authorized C lookup supplies only its existing benchmark.
- Source regression baseline is public GitHub main/PR base 1239d510f913cef79c98ef91d2e7eaad090c480c. The source test passed in a clean shallow clone after explicitly fetching that commit; local-only 930394fe was confirmed absent. This validation used local file transport for the exact public commit objects because GitHub Git transport remains unavailable.

Executed correction verification:
- dotnet build AIS/AIS.csproj --no-restore -v quiet: succeeded, 0 errors and 0 warnings on the final incremental run (earlier recompilation retained the existing 57 warnings).
- CcoReporting.Checks: 69 checks pass. Actual public GetKpiLifecycle rejects CCO even with a route grant. Actual production execution pipelines accept the six CCO Overview actions and B/C/D for ordinary role 38 when granted, with only their final Oracle delegates replaced by sentinels. Existing role/page/API/session/date/legacy/A-E/PDF negative checks pass. This proves pre-query eligibility, not successful Oracle reads.
- Source tests: 2 pass in working tree and clean shallow checkout, with baseline history explicitly available and local setup commit absent.
- Headless Chrome: pass. Fixture payloads are now constrained by the compiled eligibility manifest and real model dataset/KPI definitions; unsupported actions receive 403 rather than mocked success. Six Overview calls and supported cards, B/C/D canonical requests, D-to-C 40% fixture benchmark and matching dates, date navigation, stale-data invalidation, ordinary CCO export absence and zero/N/A checked. Data is mocked; this is not an authenticated IAS smoke test.
- No new broad executive-suite run was required for these corrections. The prior baseline-equivalent 14 failures remain documented as historical results, not a current clean-suite claim.

Still pending: correction review, explicitly approved staging connection and permission snapshot, actual successful role-38 B/C/D/summary reads, taxonomy/source reconciliation, real role/session/context revocation matrix, live browser/mobile and export acceptance. No merge, deployment or Phase 2 Oracle development is authorized by this correction work.
