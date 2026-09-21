-- Read-only deployment verification. Run after approved installation.
set serveroutput on;

select object_name, object_type, status
from user_objects
where object_name in ('PKG_WORKING_PAPER','T_WP_HEADER','T_WP_ITEM','T_WP_EVIDENCE_LINK','T_WP_EXCEPTION','T_WP_REVIEW_NOTE','T_WP_HISTORY')
order by object_type, object_name;

select constraint_name, table_name, status
from user_constraints
where table_name like 'T_WP_%' and status <> 'ENABLED';

select object_name, procedure_name
from user_procedures
where object_name = 'PKG_WORKING_PAPER'
order by subprogram_id;

-- Package/package-body must both be VALID. Any rows returned below block release.
select name, type, line, position, text
from user_errors
where name = 'PKG_WORKING_PAPER'
order by sequence;

-- Review formal parameter order, type and direction against DBConnection.WorkingPaperV2.cs.
select object_name, package_name, argument_name, position, in_out, data_type
from user_arguments
where package_name = 'PKG_WORKING_PAPER'
order by object_name, overload, sequence;

-- Required IAS dependencies. Any missing row blocks installation/compilation.
select required_name, case when o.object_name is null then 'MISSING' else o.status end dependency_status
from (
  select 'T_AU_PLAN_ENG' required_name from dual union all
  select 'T_AU_AUDIT_TEAM_TASKLIST' from dual union all
  select 'T_WORKING_PAPER_LOAN_CASE_FILE' from dual union all
  select 'T_WORKING_PAPER_VOUCHER_CHECKING' from dual union all
  select 'T_WORKING_PAPER_ACCOUNT_OPENING' from dual union all
  select 'T_WORKING_PAPER_FIXED_ASSETS' from dual union all
  select 'T_WORKING_PAPER_CASH_COUNT' from dual
) d left join user_objects o on o.object_name=d.required_name and o.object_type in ('TABLE','VIEW')
order by required_name;

select referenced_name, referenced_type
from user_dependencies
where name='PKG_WORKING_PAPER'
order by referenced_type, referenced_name;

-- Must return no rows: V2 ENTITY_ID is the audited entity, not the auditor's posting entity.
select H.WP_ID,H.ENG_ID,H.ENTITY_ID stored_audited_entity,E.ENTITY_ID engagement_audited_entity
from T_WP_HEADER H join T_AU_PLAN_ENG E on E.ENG_ID=H.ENG_ID
where H.ENTITY_ID<>E.ENTITY_ID;

-- Must return five unchanged legacy table names when they exist in the deployment schema.
select table_name
from user_tables
where table_name in ('T_WORKING_PAPER_LOAN_CASE_FILE','T_WORKING_PAPER_VOUCHER_CHECKING',
  'T_WORKING_PAPER_ACCOUNT_OPENING','T_WORKING_PAPER_FIXED_ASSETS','T_WORKING_PAPER_CASH_COUNT')
order by table_name;

-- Version lineage: approved source rows must remain and reopened rows must point to them.
select WP_ID, ROOT_WP_ID, SUPERSEDES_WP_ID, ENG_ID, PAPER_TYPE, VERSION_NO, STATUS,
       PREPARED_BY, REVIEWED_BY, APPROVED_BY
from T_WP_HEADER
order by ROOT_WP_ID, VERSION_NO;
