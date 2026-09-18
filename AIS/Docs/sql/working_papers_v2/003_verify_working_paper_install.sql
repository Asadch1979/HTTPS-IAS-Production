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

-- Must return five unchanged legacy table names when they exist in the deployment schema.
select table_name
from user_tables
where table_name in ('T_WORKING_PAPER_LOAN_CASE_FILE','T_WORKING_PAPER_VOUCHER_CHECKING',
  'T_WORKING_PAPER_ACCOUNT_OPENING','T_WORKING_PAPER_FIXED_ASSETS','T_WORKING_PAPER_CASH_COUNT')
order by table_name;
