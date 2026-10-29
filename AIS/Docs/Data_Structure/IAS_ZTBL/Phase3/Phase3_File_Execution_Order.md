# IAS_ZTBL Phase 3 File Execution Order

## Core DDL Pack

1. `sql/00_create_schema_or_prerequisites.sql`
2. `sql/01_create_tables.sql`
3. `sql/02_create_indexes_constraints.sql`
4. `sql/03_create_sequences_triggers.sql`
5. `sql/04_create_views.sql`
6. `sql/05_seed_lookup_types_values.sql`
7. `sql/06_create_package_specs.sql`
8. `sql/07_create_package_bodies.sql`
9. `sql/08_post_deploy_validation_queries.sql`

## Migration Pack

1. `migration/10_migration_precheck.sql`
2. `migration/11_migration_master_data.sql`
3. `migration/12_migration_security_data.sql`
4. `migration/13_migration_planning_engagement.sql`
5. `migration/14_migration_observation_execution.sql`
6. `migration/15_migration_compliance_para.sql`
7. `migration/16_migration_iid.sql`
8. `migration/17_migration_commercial_audit.sql`
9. `migration/18_migration_reporting_frpt.sql`
10. `migration/19_migration_documents_notifications.sql`
11. `migration/20_migration_reconciliation_checks.sql`

## Order Rules

- do not run migration scripts before `05_seed_lookup_types_values.sql`
- do not run package bodies before package specs
- do not run migration scripts without a registered migration batch from `10_migration_precheck.sql`
- treat `08_post_deploy_validation_queries.sql` and `20_migration_reconciliation_checks.sql` as validation layers, not deployment steps
