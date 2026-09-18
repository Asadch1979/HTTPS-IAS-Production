# IAS Working Papers v2 SQL

These scripts are additive and must be reviewed and executed by the approved Oracle DBA release process. The application does not execute them automatically.

Order:

1. `001_create_working_paper_objects.sql`
2. `002_create_working_paper_package.sql`
3. `003_verify_working_paper_install.sql`

The scripts do not alter or delete any legacy `T_WORKING_PAPER_*` table. The package reads legacy rows for a read-only history panel and writes only to the new `T_WP_*` objects.

Before production execution, confirm schema/tablespace standards, engagement authorization source, grants, backup/recovery, evidence identifier contract, observation identifier contract, retention policy and whether a separate approver is mandatory. The implemented default requires preparer/reviewer separation and permits the assigned reviewer to perform final approval; change that policy only through an approved design decision.
