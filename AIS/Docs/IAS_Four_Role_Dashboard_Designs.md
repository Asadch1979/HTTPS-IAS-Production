# IAS — Four role-based dashboard concepts (design handoff)

**Status:** Visual concept and technical blueprint; **no live IAS dashboard code is changed in this branch**. Sample figures in the HTML prototype are illustrative and must never be inserted into IAS queries/UI. Standalone local prototype delivered to requestor as `ias_dashboard_concepts.html`.

## 1. Four distinct dashboards

| Concept | User audience | Visual identity | Scope/starting point | Top content |
|---|---|---|---|---|
| **01 HighBond-inspired audit command center** | CIA and Heads of Audit | White/slate with navy (#122D47) + teal (#147D92), compact assurance layout | Existing `/ExecutiveOverview/CIA` for CIA; Heads of Audit require **verified distinct role/page mapping** | Audit plan execution, high-risk findings, draft reports awaiting approval, overdue remediation, audit division comparisons, decision queue |
| **02 TeamMate-inspired engagement workbench** | Audit Team Leaders | Blue (#17284B, #5B82CA), four-column workflow board | Existing authorized Planning/Engagement/Observation workflows; not yet represented by a dedicated executive route | My active engagements, awaiting my review, issue/observation finalization, due dates; Planning → Fieldwork → Review → Reporting kanban; workpaper progress |
| **03 ZTBL Management Audit dashboard** | Group & Divisional Heads | Bank green (#103B35, #0F5C4D), airy management scorecard | `/ExecutiveOverview/GROUP`; Divisional Heads via **verified `RLM` role/entity mapping**, not a new bank-wide route | Open/settled/high-risk/aged paras, division comparisons, risk composition, management decisions and follow-ups |
| **04 Dark Design 3 Regional Audit Pulse** | GMs and Regional Heads | Midnight navy (#090F1E), teal (#5BD8BD), amber/red risk cues | `/ExecutiveOverview/RLM` for authenticated reporting descendants only; GM/region view selected through verified server organizational type | Branch heatmap, high-risk exposure, >365-day aging, >3 compliance cycles, top overdue entities and pending responses |

**Keep `TOP` nine lifecycle KPIs, `CCO`, Reports A–E, and existing page permissions unchanged.** A role-themed appearance is not authorization. No client-selected perspective may enlarge the authorized entity scope.

## 2. Existing IAS data vs new verified data

Available scoped procedures include:
- `PKG_EXEC_DASHBOARD.P_GET_KPI_SUMMARY` → open paras, high risk, month-to-date settled, current aging/cycle counts and ratio.
- `P_GET_RISK_SUMMARY`, `P_GET_AGING_SUMMARY`, `P_GET_CYCLE_SUMMARY`, `P_GET_SETTLEMENT_TREND`, `P_GET_CRITICAL_ITEMS` → risk/aging/trend/list display.
- `P_GET_KPI_LIFECYCLE` → TOP date-based lifecycle KPIs, with estimated historical quality flags; do not repurpose its metrics as an unrelated executive “plan” KPI.
- CIA recent issue count data from `P_GET_CIA_ISSUES` via the existing authorized summary endpoint; CIA's date range is **optional for Issue Identified/Rectified only**. Existing CIA open-para/risk cards remain current-state.

**Not yet confirmed by the existing executive data contracts:** plan coverage denominators, draft report approval queue, overdue remediation due dates, Team Leader assignment/task lists, completion of workpapers, management decisions requiring response, and branch-level heatmap data. Never simulate a live number. Trace verified repository tables, create the smallest scope-aware Oracle procedures where needed, F8 compile/verify, and then expose via typed C# methods with the same authenticated page+API permission checks.

## 3. Layout specification

### HighBond-inspired (CIA and Audit Heads)
- Page title: **Audit command center** with named organizational scope and as-of date.
- Four primary cards: Audit plan coverage; High-risk open; Draft reports awaiting review; Overdue remediation.
- Two mid panels: audit plan execution by audit portfolio + high-risk remediation trend.
- Bottom: leadership-attention table with responsible unit, priority, action/approval, due date.
- Mark cards as “Not available” when a verified backend source is absent; do not fabricate values or infer due dates.
- CIA can keep existing optional issue dates and additional issue counters alongside/within the approved layout. Heads of Audit must not inherit CIA-wide scope by being assigned the same style.

### TeamMate-inspired (Team Leaders)
- Page title: **My engagement workbench**. Team-assigned scope only.
- Four cards: active engagements, awaiting team-leader review, observations to finalize, due this week.
- Four workflow lanes: Planning, Fieldwork, Review, Reporting, using **actual IAS engagement state IDs** only after checking schema/engagement logs.
- Secondary panels: workpaper completion and review queue; links to existing approved IAS action pages.
- New per-user APIs must constrain team membership and review assignment on the server, never by trusting an employee ID supplied from the browser.

### ZTBL Management Audit (Group/Divisional Heads)
- Page title: **Management Audit Dashboard** and authoritative group/division hierarchy label.
- Four cards: open paras, high-risk open, settled month-to-date, over-365-days.
- “Open paras by department/division” ranked horizontal bars, risk exposure composition, decisions/follow-ups table.
- Read counts from existing current-state procedures; departmental drill-down requires verified descendant aggregation. No bank-wide totals for RLM.
- Correct distinction between dated period activity and current-state position; unknown values are N/A.

### Dark Design 3 (GM/Regional Heads)
- Page title: **Regional Audit Pulse** with user’s actual GM/Region scope.
- Four cards: open paras, high-risk cases, >365-day age, >3 compliance cycles.
- Branch risk heatmap with a **documented denominator and legend**, largest exposures list, entity-level follow-up queue.
- Use server-verified reporting ancestor chain and authorized descendants. Do not trust a free-form root, GM ID or selected branch ID.
- Preserve text labels/statuses, high contrast, and accessible colors in dark theme.

## 4. Design system and quality

- Keep IAS navigation, common header, corporate branding; use **scoped role-layout CSS** to avoid global regression.
- Data architecture: Oracle 18c `PKG_EXEC_DASHBOARD` reuse before new packages, ASP.NET Core MVC typed models/DBConnection methods, one authorized API contract per new dataset if required. No hardcoded counts.
- Responsive desktop/laptop/mobile: 4→2→1 KPI cards, larger charts vertically stacked, horizontally scrollable tables, accessible tab order/focus.
- Dashboard hierarchy: title + scope → high-signal KPIs → comparisons → critical actions. Days shown as days; month-to-date expanded; issue rectifications exclude status 23 drops.
- Semantic risk colors consistent across all variants (green good, amber caution, red high risk), with text and indicators so meaning isn't encoded by color alone.
- When metrics require dates, show the date range; never claim period filtering for current-state charts. Default yesterday for an applicable completed-day TOP date without overwriting chosen dates.
- Visual prototype includes illustrative figures/branch names. Never present them as current IAS values.

## 5. Four incremental Codex phases

1. **CIA/Heads of Audit visual foundation**, small feature branch: adopt HighBond-inspired layout under verified CIA permissions; use existing data first; unavailable backend fields show N/A. Do not change TOP/CCO. Build and user review.
2. **Team Leader workbench**, small feature branch: identify actual assigned audit/team/workpaper workflow and state mappings, then propose smallest F8 Oracle query plan before adding protected endpoints and kanban. Wait for DB validation.
3. **Group/Divisional management**, separate branch: implement grouped view through existing GROUP/RLM scope checks; verify current hierarchy; add scoped division aggregates only where missing.
4. **GM/Regional dark**, separate branch: style authorized RLM branch/GM hierarchy with verified risk/aging/branch aggregates, no client root escalation.

**Minimum verification:** source counts reconciled, denied roles receive 403, no unauthorized entities, live KPIs aren't mock numbers, responsive layouts, stateful links, no impact to existing TOP/CCO/Reports, light CSS cannot leak to Dark layout, fast focused .NET build and smoke test. Keep each PR reviewable; do not merge without user approval.

## 6. Initial Codex task — recommended

> Read `AIS/Docs/IAS_Four_Role_Dashboard_Designs.md` and current ExecutiveOverview models/Razor/JS/CSS. Implement **Phase 1 only**, HighBond-inspired CIA audit command center, with existing authenticated CIA scope, existing available KPIs/charts and optional issue date semantics. Keep TOP/CCO/GROUP/RLM/Reports unchanged. Use scoped Razor partial and CSS; do not display invented plan coverage, approval queue or remediation due dates as live—mark them “N/A” until approved data contracts exist. No SQL changes in Phase 1. Short build/smoke test, commit on feature branch, describe backend datasets still needed.

