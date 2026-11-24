# FAD Branch Risk Rating Engine (Phase 1)

## What the button does

1. The Audit Period dropdown uses the existing T_AU_PERIOD and
   T_AU_PERIOD_STATUS tables (all statuses, not the older PKG_PG.P_GetAuditPeriods
   status_id=2 filter).
2. Clicking **Calculate Branch Ratings** calls PKG_FAD_RISK_ENGINE.P_RUN_PERIOD
   for the chosen audit period. Oracle selects
   T_AU_PLAN_ENG.PERIOD_ID = :period AND ENTITY_TYPE = 6.
3. For each ENG_ID, the engine reads T_AU_OBSERVATION.ANNEX, ENGPLANID and
   NO_OF_INSTANCES. T_FAD_RISK_MAP_STAGE.ANEX_ID maps the existing IAS Annexure ID
   to the canonical Annexure code in the Excel-based configuration.
4. The published model in T_FAD_RISK_VERSION, T_FAD_RISK_PROCESS,
   T_FAD_RISK_SUBPROCESS and T_FAD_RISK_ANNEX provides the 20 process weights,
   gravity, denominators and zero-weight fraud override. The actual calculations
   call PKG_FAD_RISK_SCORE_V1, not the legacy T_R_GROUP/T_R_SUB_GROUP tables.
5. Detailed Annexure counts, process marks, missing-input explanations, calculated
   ratings (when complete) and execution history are saved in the existing
   T_FAD_RISK_ENGINE_* tables. Current results use V_FAD_RISK_ENGINE_CURRENT.
6. A Super User can view results and rerun an audit period without changing
   T_AU_OBSERVATION, completed assessments, or old risk-rating tables.

## Inputs not derivable from Annexure observations

The original Excel model has five Achievement components plus Profitability.
They require opening/closing amounts, targets/achievements or profit figures.
The final rating also depends on branch size, and any positive fraud count
forces High independently of the calculated score.

**There is no verified automatic financial-data source in this repository.**
For this phase, the engine reads those additional inputs only from existing
APPROVED T_FAD_RISK_ASSESSMENT and T_FAD_RISK_DETAIL records matching the
engagement and the published risk version. If a branch lacks the approved input
record or has invalid numeric values, its observation-based component marks
are still saved, but its overall SCORE and RISK_RATING are NULL and the row
is PENDING_INPUT or FAILED as appropriate. Never infer profitability, size,
targets, balances or zero amounts.

This is now a real weighted Annexure calculation, not the previous
counts-only placeholder; however, it cannot generate a final rating for a
branch with missing required financial and size inputs.

## Simplified implementation

- P_CATALOG: simple audit-period cursor (auditperiodid, description, dates, status)
- P_RUN_PERIOD: audit period only; entity type hardcoded to 6 in Oracle
- P_RUN_ENTITY: available in the package for Phase 2; its UI and route are inactive
- P_RESULTS: four existing read cursors; only Period is exposed in Phase 1 UI
- P_CHECK_SUPER_USER: validates the active server-derived role/group 1/1 session
- P_SCORE_BRANCH and P_EXECUTE: shared process scoring and batch execution

There is no T_FAD_ENGINE_POLICY or T_FAD_ENGINE_COMPLETENESS dependency,
no T_FAD_ENGINE_LOCK or T_FAD_ENGINE_MAP_CHECK dependency, and no DML against
legacy risk rating tables. The C# DBConnection calls stored procedures only,
and consumes OUT parameters for the run summary, without a direct SELECT.
The application still enforces the exact API permission and antiforgery token.

## Deployment checklist

1. Execute the updated 00_preflight.sql in a controlled IAS schema and review
   existing Audit Periods, Type-6 engagements, staged ANEX_ID mappings and the
   scoring version. PUBLISHED Excel configuration must have 20 processes with
   weights totaling 100 and 121 Annexures. Source fingerprints must be valid.
2. Install existing engine tables via 10_schema.sql only if not already present.
   Review 11_review_upgrade.sql and apply only once if the needed run counters,
   current view and columns have not already been created. Do not recreate
   existing tables or triggers.
3. Compile PKG_FAD_RISK_SCORE_V1.sql if not installed. Compile the updated
   20_package.sql with SQL Developer F5. Confirm PACKAGE and PACKAGE BODY VALID
   and zero USER_ERRORS. Confirm the new OUT-parameter signature matches C#.
4. Ensure three Phase 1 APIs are registered by 40_permissions.sql and, following
   an explicit administrative approval, enable them for Super User 1/1 using
   45_super_user_enable.sql. RunEngineEntity must remain inactive until Phase 2.
5. Deploy updated DBConnection.FAD.Engine.cs, the controller and Razor/JS UI.
   Log out and in to refresh IAS API permissions and static files.
6. Verify the period dropdown. Run one period on a *test* database and reconcile
   Annexure counts, process score normalization and fraud override against a
   known Excel example. Then confirm missing input rows show PENDING_INPUT,
   completed rows show CALCULATED, and old history is unchanged.

No Oracle deployment or compilation has been performed through ChatGPT.
This remains a source implementation requiring controlled staging verification.
