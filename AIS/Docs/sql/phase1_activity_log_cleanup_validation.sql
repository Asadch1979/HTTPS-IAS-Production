set serveroutput on
set define off

prompt === Phase 1 activity-log cleanup: compile affected business packages ===

alter package PKG_AD compile package;
alter package PKG_AD compile body;
alter package PKG_AE compile package;
alter package PKG_AE compile body;
alter package PKG_AR compile package;
alter package PKG_AR compile body;
alter package PKG_DB compile package;
alter package PKG_DB compile body;
alter package PKG_FAD compile package;
alter package PKG_FAD compile body;
alter package PKG_HD compile package;
alter package PKG_HD compile body;
alter package PKG_PG compile package;
alter package PKG_PG compile body;

prompt === Compile errors, if any ===
select name, type, line, position, text
  from user_errors
 where name in ('PKG_AD', 'PKG_AE', 'PKG_AR', 'PKG_DB', 'PKG_FAD', 'PKG_HD', 'PKG_PG')
 order by name, sequence;

prompt === Invalid affected packages, if any ===
select object_name, object_type, status
  from user_objects
 where object_name in ('PKG_AD', 'PKG_AE', 'PKG_AR', 'PKG_DB', 'PKG_FAD', 'PKG_HD', 'PKG_PG')
   and status <> 'VALID'
 order by object_name, object_type;

prompt === Residual business-package T_AU_ACTIVITY_LOG dependencies ===
select name, type, referenced_name, referenced_type
  from user_dependencies
 where referenced_name = 'T_AU_ACTIVITY_LOG'
   and name in ('PKG_AD', 'PKG_AE', 'PKG_AR', 'PKG_DB', 'PKG_FAD', 'PKG_HD', 'PKG_PG')
 order by name, type;

prompt === PKG_LG preserved dependency check ===
select name, type, referenced_name, referenced_type
  from user_dependencies
 where referenced_name = 'T_AU_ACTIVITY_LOG'
   and name = 'PKG_LG'
 order by name, type;

