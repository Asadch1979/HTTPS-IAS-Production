# IAS Working Papers v2 SQL

These scripts are additive and must be reviewed and executed by the approved Oracle DBA release process. The application does not execute them automatically.

Order:

1. `001_create_working_paper_objects.sql`
2. `002_create_working_paper_package.sql`
3. `005_integrate_ar_dashboard_context.sql`
4. `003_verify_working_paper_install.sql`

For an environment where the original `935fc5d` foundation was already installed, run
`004_upgrade_foundation_security_versioning.sql` instead of rerunning `001`; it adds lineage/sign-off
columns and recompiles the corrected package. Then run `005` followed by `003`; treat compilation errors
or missing dependencies as a release blocker.

Run `005_integrate_ar_dashboard_context.sql` for both new and previously installed V2 schemas before
verification. It repairs V2 header entity context from `T_AU_PLAN_ENG` and recompiles the package. It does
not modify legacy working papers.

The scripts do not alter or delete any legacy `T_WORKING_PAPER_*` table. The package reads legacy rows for a read-only history panel and writes only to the new `T_WP_*` objects.

Authorization follows the AR Dashboard source: the actor must be assigned to the engagement in
`T_AU_AUDIT_TEAM_TASKLIST`, and the engagement must be in the dashboard's eligible plan-status range.
The assigned reviewer must also be an engagement-team member. The package repeats authorization for
create, read and every write/workflow operation. `P_AUDITOR_ENTITY_ID` records authenticated auditor
context; `T_WP_HEADER.ENTITY_ID` stores the audited entity derived from `T_AU_PLAN_ENG`.

Before production execution, confirm schema/tablespace standards, grants, backup/recovery, evidence
identifier authorization, observation identifier authorization, retention policy and whether a separate
approver is mandatory. The implemented interim policy requires preparer/reviewer separation and permits
the assigned reviewer to perform final approval. Changing that policy requires an approved design decision.

Submission and approval are blocked until planning, sampling, execution results, server calculations,
item-level evidence, exception records/dispositions and conclusion fields are complete. Reopen is limited
to privileged roles, requires a reason and creates a new editable row with cloned detail; the approved source
row is never updated.
