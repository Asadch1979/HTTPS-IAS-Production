# FAD Risk Register Execution Engine

Source-only implementation. **Oracle compilation and database execution have not been performed.**
The current engine stores audited-branch observation counts and hierarchy details. It does
**not** calculate an overall numeric risk score without an approved scoring crosswalk.

## Package structure

PKG_FAD_RISK_ENGINE follows the existing IAS stored-procedure pattern:

- P_CATALOG: one ref cursor for Audit Period, Entity Type and Entity dropdowns
- P_RUN_PERIOD: execute all engagements of the selected period/entity type
- P_RUN_ENTITY: execute engagements of one entity within the selected period
- P_RESULTS: four ref cursors for current results, history, detail and exceptions

There are only two internal routines: P_CHECK_SUPER_USER (checks a real active
Super User role/group 1/1 session) and P_PROCESS_ENGAGEMENT (shared work by ENG_ID).
The application continues to verify the exact API grant with RiskAction.
Run procedures use server-supplied context parameters, not user-entered role IDs.

No separate engine-wide configuration lock, MAP_CHECK temporary-table dependency,
manually provisioned engagement lock, or manually approved engagement-by-engagement
completeness record is required. 30_locks.sql and the unused compatibility tables
are not prerequisites for the rewritten package; do not drop existing tables during
this update. The run snapshots its approved policy, and the calculator reads that
snapshot throughout that run. Concurrent runs can create distinct history; they
are not prevented by the rewritten engine, so control execution at the UI.

## Deployment / verification order

1. Verify ANEX_ID in the deployed T_FAD_RISK_MAP_STAGE. It must map uniquely
   to T_AUDIT_CHECKLIST_ANNEXURE.ID and match the intended source Annexure code
   (a hyphen-only code difference is accepted). Never alter original source
   fingerprints. Run 00_preflight.sql and review any discrepancies.
2. If the engine tables do not exist, review and install 10_schema.sql once.
   For an existing installation, do not re-run creation scripts.
3. If not already deployed, review and apply 11_review_upgrade.sql once:
   policy_version, approved_on, reference_rule, the run policy snapshot/version,
   calculated count, and the current-results view are required.
   Unused T_FAD_ENGINE_COMPLETENESS and T_FAD_ENGINE_MAP_CHECK objects from this
   older migration do not need to be populated for this version.
4. Configure T_FAD_ENGINE_POLICY with reviewed, approved entity-type, completed
   engagement-status and final-observation-status combinations, a consistent
   policy_version and reference_rule. **Do not assume STATUS values** from a
   different IAS module. No default statuses or risk weights are inserted.
5. Compile 20_package.sql using SQL Developer F5 and verify both PACKAGE and
   PACKAGE BODY are VALID and USER_ERRORS contains no errors.
6. Register four API masters with 40_permissions.sql, initially inactive.
   After approval, run 45_super_user_enable.sql to validate and grant the four
   routes to Super User role/group 1/1 only. Check the existing Index page
   mapping. Log out and back in to refresh IAS API permissions.
7. Deploy the current C#, Razor and fad-risk-engine.js files. Verify that
   EngineCatalog returns actual period/type/entity rows before attempting
   execution. Period 5 is selected automatically when returned by the catalog.
8. In a controlled development/test database, test one entity, then one period
   and entity type; compare raw observations and Annexure counts to IAS data.
   Verify non-Super-User denial, missing policy, bad ANEX_ID, no observations,
   invalid instance counts, repeated references and historical re-execution.

## Execution behavior

- Audit engagements are identified by T_AU_PLAN_ENG.ENG_ID, period by PERIOD_ID,
  type by T_AUDITEE_ENTITIES.TYPE_ID and entity by ENTITY_ID.
- The verified Super User can report and execute across IAS audit offices.
  AUDIT_OFFICE_ID in the run table records the initiating user's IAS context.
- PKG_FAD_RISK.P_GET_REGISTER remains the sole approved fingerprint validation.
  The package validates unique operational ANEX_ID mappings once per run.
- Final observations are selected only according to the approved policy snapshot
  and counted once by T_AU_OBSERVATION.ID. NO_OF_INSTANCES remains unknown when
  invalid. Zero qualifying observations are explicitly flagged for review.
- Annexure results and hierarchy aggregates are stored in the existing RESULT
  and DETAIL tables. One exception record identifies unresolved scoring inputs.
  All numeric SCORES/RATINGS remain NULL and status PENDING_INPUT until a
  separately approved, implemented source-to-calculator crosswalk is available.
- Ineligible engagements are SKIPPED. Per-engagement errors are recorded as
  FAILED. Each new execution creates a new run/history; existing completed
  history triggers protect past records. The caller owns the transaction, and
  C# rechecks the server session before committing.
- Neither package procedure changes audit observations, final audit reports,
  post-compliance records, approved scoring weightages or published assessments.

**Validation status:** GitHub source inspection only; no Oracle compilation,
live database results or end-to-end integration testing is claimed.
