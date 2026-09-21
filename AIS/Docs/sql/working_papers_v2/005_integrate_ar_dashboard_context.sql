-- IAS Working Papers v2 - AR Dashboard integration correction.
-- Additive repair only. Legacy working-paper tables are not modified.

-- V2 originally stored the auditor's posting entity in ENTITY_ID. The header must store
-- the audited entity owned by the engagement.
declare
  V_CONFLICTS number;
begin
  select count(*) into V_CONFLICTS from (
    select H.ENG_ID,E.ENTITY_ID,H.PAPER_TYPE,H.VERSION_NO
    from T_WP_HEADER H join T_AU_PLAN_ENG E on E.ENG_ID=H.ENG_ID
    group by H.ENG_ID,E.ENTITY_ID,H.PAPER_TYPE,H.VERSION_NO
    having count(*)>1
  );
  if V_CONFLICTS>0 then
    raise_application_error(-20050,'Duplicate V2 engagement/paper versions require approved data remediation before entity-context migration.');
  end if;
end;
/

update T_WP_HEADER H
set H.ENTITY_ID = (select E.ENTITY_ID from T_AU_PLAN_ENG E where E.ENG_ID=H.ENG_ID)
where exists (
  select 1 from T_AU_PLAN_ENG E
  where E.ENG_ID=H.ENG_ID and E.ENTITY_ID<>H.ENTITY_ID
);

-- Recreate the package with P_AUDITOR_ENTITY_ID context and AR Dashboard assignment checks.
@@002_create_working_paper_package.sql

commit;
