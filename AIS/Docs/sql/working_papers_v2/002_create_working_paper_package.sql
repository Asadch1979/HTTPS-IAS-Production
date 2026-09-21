-- IAS Working Papers v2 package. Review and execute only after 001_create_working_paper_objects.sql.

create or replace package PKG_WORKING_PAPER as
  type T_CURSOR is ref cursor;
  procedure P_CREATE(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_ENG_ID number, P_PAPER_TYPE varchar2, P_REVIEWER_PPNO varchar2, P_DUE_DATE date, O_RESULT out T_CURSOR);
  procedure P_GET_WORKSPACE(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_WP_ID number, P_ENG_ID number, P_PAPER_TYPE varchar2, O_HEADER out T_CURSOR, O_ITEMS out T_CURSOR,
    O_EVIDENCE out T_CURSOR, O_EXCEPTIONS out T_CURSOR, O_NOTES out T_CURSOR, O_HISTORY out T_CURSOR, O_LEGACY out T_CURSOR);
  procedure P_SAVE_PLAN(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_WP_ID number, P_ROW_VERSION number, P_PLAN_JSON clob, O_RESULT out T_CURSOR);
  procedure P_SAVE_ITEM(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_WP_ID number, P_ITEM_ID number, P_ITEM_REF varchar2, P_DETAILS_JSON clob, P_CALC_JSON clob,
    P_RESULT varchar2, P_COMMENT varchar2, P_ROW_VERSION number, O_RESULT out T_CURSOR);
  procedure P_LINK_EVIDENCE(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_WP_ID number, P_ITEM_ID number, P_EXCEPTION_ID number, P_EXISTING_EVIDENCE_ID varchar2,
    P_TITLE varchar2, P_EVIDENCE_TYPE varchar2, P_SOURCE varchar2, P_EVIDENCE_DATE date, O_RESULT out T_CURSOR);
  procedure P_SAVE_EXCEPTION(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_WP_ID number, P_EXCEPTION_ID number, P_ITEM_ID number, P_CRITERIA varchar2, P_CONDITION varchar2,
    P_CAUSE varchar2, P_IMPACT varchar2, P_RISK varchar2, P_OWNER_PPNO varchar2, P_DUE_DATE date,
    P_OBSERVATION_ID number, P_STATUS varchar2, P_ROW_VERSION number, O_RESULT out T_CURSOR);
  procedure P_SAVE_CONCLUSION(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_WP_ID number, P_ROW_VERSION number, P_CONCLUSION_JSON clob, O_RESULT out T_CURSOR);
  procedure P_SAVE_REVIEW_NOTE(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_WP_ID number, P_NOTE_ID number, P_SECTION_KEY varchar2, P_NOTE_TEXT varchar2,
    P_RESPONSE_TEXT varchar2, P_STATUS varchar2, O_RESULT out T_CURSOR);
  procedure P_WORKFLOW(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_WP_ID number, P_ACTION varchar2, P_REMARKS varchar2, P_ROW_VERSION number, O_RESULT out T_CURSOR);
end PKG_WORKING_PAPER;
/

create or replace package body PKG_WORKING_PAPER as
  procedure OPEN_RESULT(O_RESULT out T_CURSOR, P_STATUS varchar2, P_MESSAGE varchar2,
    P_WP_ID number default null, P_RECORD_ID number default null, P_ROW_VERSION number default null) is
  begin
    open O_RESULT for select P_STATUS STATUS, P_MESSAGE MESSAGE, nvl(P_WP_ID,0) WP_ID,
      nvl(P_RECORD_ID,0) RECORD_ID, nvl(P_ROW_VERSION,0) ROW_VERSION from dual;
  end;

  procedure ASSERT_COMPLETE(P_WP_ID number) is V_COUNT number; V_PLAN clob; V_CONCLUSION clob;
  begin
    select PLAN_JSON,CONCLUSION_JSON into V_PLAN,V_CONCLUSION from T_WP_HEADER where WP_ID=P_WP_ID;
    if json_value(V_PLAN,'$.Objectives') is null or json_value(V_PLAN,'$.Risks') is null
       or json_value(V_PLAN,'$.Controls') is null or json_value(V_PLAN,'$.Scope') is null
       or json_value(V_PLAN,'$.PopulationSource') is null or json_value(V_PLAN,'$.SamplingMethod') is null
       or nvl(json_value(V_PLAN,'$.SampleSize' returning number),0)<1 then
      raise_application_error(-20030,'Planning, population and sampling fields must be complete.');
    end if;
    select count(*) into V_COUNT from T_WP_ITEM where WP_ID=P_WP_ID;
    if V_COUNT=0 then raise_application_error(-20031,'At least one test item is required.'); end if;
    select count(*) into V_COUNT from T_WP_ITEM where WP_ID=P_WP_ID and (RESULT in ('PENDING','NOT_TESTED') or CALCULATIONS_JSON is null);
    if V_COUNT>0 then raise_application_error(-20032,'All test items require final results and server calculations.'); end if;
    select count(*) into V_COUNT from T_WP_ITEM I where I.WP_ID=P_WP_ID and I.RESULT<>'NOT_APPLICABLE'
      and not exists(select 1 from T_WP_EVIDENCE_LINK E where E.WP_ID=I.WP_ID and E.ITEM_ID=I.ITEM_ID);
    if V_COUNT>0 then raise_application_error(-20033,'Each applicable test item requires linked evidence.'); end if;
    select count(*) into V_COUNT from T_WP_ITEM I where I.WP_ID=P_WP_ID and I.RESULT='EXCEPTION'
      and not exists(select 1 from T_WP_EXCEPTION X where X.WP_ID=I.WP_ID and X.ITEM_ID=I.ITEM_ID);
    if V_COUNT>0 then raise_application_error(-20034,'Each exception result requires an exception record.'); end if;
    select count(*) into V_COUNT from T_WP_EXCEPTION where WP_ID=P_WP_ID and STATUS='OPEN';
    if V_COUNT>0 then raise_application_error(-20035,'Every exception requires a resolved or linked disposition.'); end if;
    if json_value(V_CONCLUSION,'$.ObjectiveAssessment') is null
       or json_value(V_CONCLUSION,'$.ObjectiveAssessment')='PENDING'
       or json_value(V_CONCLUSION,'$.OverallConclusion') is null then
      raise_application_error(-20036,'Objective assessment and overall conclusion must be complete.');
    end if;
  end;

  procedure ADD_HISTORY(P_WP_ID number, P_ACTION varchar2, P_ACTOR varchar2, P_ROLE number,
    P_DETAILS varchar2, P_ROW_VERSION number) is
  begin
    insert into T_WP_HISTORY(HISTORY_ID,WP_ID,ACTION,ACTOR_PPNO,ACTOR_ROLE_ID,DETAILS,ROW_VERSION)
    values(SEQ_WP_HISTORY.nextval,P_WP_ID,P_ACTION,P_ACTOR,P_ROLE,substr(P_DETAILS,1,4000),P_ROW_VERSION);
  end;

  procedure ASSERT_ENGAGEMENT_ACCESS(P_ENG_ID number, P_ENTITY_ID number, P_ACTOR varchar2, P_ROLE_ID number) is
    V_ENTITY number; V_COUNT number;
  begin
    select ENTITY_ID into V_ENTITY from T_AU_PLAN_ENG where ENG_ID=P_ENG_ID;
    if V_ENTITY<>P_ENTITY_ID then raise_application_error(-20020,'Engagement not found or access denied.'); end if;
    if nvl(P_ROLE_ID,-1) not in (1,2) then
      select count(*) into V_COUNT from T_AU_AUDIT_TEAM_TASKLIST
       where ENG_PLAN_ID=P_ENG_ID and to_char(TEAMMEMBER_PPNO)=trim(P_ACTOR)
         and nvl(to_char(ISACTIVE),'1') in ('1','Y');
      if V_COUNT=0 then raise_application_error(-20020,'Engagement not found or access denied.'); end if;
    end if;
  exception when no_data_found then raise_application_error(-20020,'Engagement not found or access denied.');
  end;

  procedure ASSERT_REVIEWER_ACCESS(P_ENG_ID number, P_REVIEWER varchar2) is V_COUNT number;
  begin
    select count(*) into V_COUNT from T_AU_AUDIT_TEAM_TASKLIST
     where ENG_PLAN_ID=P_ENG_ID and to_char(TEAMMEMBER_PPNO)=trim(P_REVIEWER)
       and nvl(to_char(ISACTIVE),'1') in ('1','Y');
    if V_COUNT=0 then raise_application_error(-20022,'Reviewer is not an active engagement-team member.'); end if;
  end;

  procedure ASSERT_ACCESS(P_WP_ID number, P_ENTITY_ID number, P_ACTOR varchar2, P_ROLE_ID number, P_MODE varchar2,
    O_STATUS out varchar2, O_CURRENT out number, O_PREPARER out varchar2, O_REVIEWER out varchar2) is
    V_STATUS T_WP_HEADER.STATUS%type; V_ENG_ID number; V_ROLE_ID number:=P_ROLE_ID;
  begin
    select STATUS,ROW_VERSION,PREPARER_PPNO,REVIEWER_PPNO,ENG_ID into V_STATUS,O_CURRENT,O_PREPARER,O_REVIEWER,V_ENG_ID
      from T_WP_HEADER where WP_ID=P_WP_ID and ENTITY_ID=P_ENTITY_ID for update;
    ASSERT_ENGAGEMENT_ACCESS(V_ENG_ID,P_ENTITY_ID,P_ACTOR,V_ROLE_ID);
    O_STATUS:=V_STATUS;
    if P_MODE='EDIT' and (V_STATUS not in ('DRAFT','RETURNED') or O_PREPARER<>P_ACTOR) then
      raise_application_error(-20021,'Working paper is not editable by this user.');
    elsif P_MODE='REVIEW' and O_REVIEWER<>P_ACTOR then
      raise_application_error(-20022,'Working paper is not assigned to this reviewer.');
    end if;
  exception when no_data_found then raise_application_error(-20020,'Working paper not found or access denied.');
  end;

  procedure P_CREATE(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_ENG_ID number, P_PAPER_TYPE varchar2, P_REVIEWER_PPNO varchar2, P_DUE_DATE date, O_RESULT out T_CURSOR) is
    V_ID number; V_REF varchar2(60);
  begin
    ASSERT_ENGAGEMENT_ACCESS(P_ENG_ID,P_ENTITY_ID,P_ACTOR_PPNO,P_ROLE_ID);
    ASSERT_REVIEWER_ACCESS(P_ENG_ID,P_REVIEWER_PPNO);
    if P_PAPER_TYPE not in ('LCF','VCH','AOF','FAS','CCT') then raise_application_error(-20001,'Invalid paper type.'); end if;
    if P_REVIEWER_PPNO=P_ACTOR_PPNO then raise_application_error(-20002,'Preparer and reviewer must be different.'); end if;
    V_ID:=SEQ_WP_HEADER.nextval;
    V_REF:=P_PAPER_TYPE||'-'||P_ENG_ID||'-'||to_char(V_ID,'FM000000');
    insert into T_WP_HEADER(WP_ID,ROOT_WP_ID,ENG_ID,ENTITY_ID,PAPER_TYPE,REFERENCE_NO,PREPARER_PPNO,REVIEWER_PPNO,DUE_DATE,
      PLAN_JSON,CONCLUSION_JSON,CREATED_BY,UPDATED_BY)
    values(V_ID,V_ID,P_ENG_ID,P_ENTITY_ID,P_PAPER_TYPE,V_REF,P_ACTOR_PPNO,P_REVIEWER_PPNO,P_DUE_DATE,'{}','{}',P_ACTOR_PPNO,P_ACTOR_PPNO);
    ADD_HISTORY(V_ID,'CREATED',P_ACTOR_PPNO,P_ROLE_ID,'Working paper created',1);
    commit; OPEN_RESULT(O_RESULT,'SUCCESS','Working paper created.',V_ID,V_ID,1);
  exception when dup_val_on_index then rollback; OPEN_RESULT(O_RESULT,'ERROR','A working paper already exists for this engagement and type.');
    when others then rollback; raise;
  end;

  procedure P_GET_WORKSPACE(P_ENTITY_ID number, P_ACTOR_PPNO varchar2, P_ROLE_ID number,
    P_WP_ID number, P_ENG_ID number, P_PAPER_TYPE varchar2, O_HEADER out T_CURSOR, O_ITEMS out T_CURSOR,
    O_EVIDENCE out T_CURSOR, O_EXCEPTIONS out T_CURSOR, O_NOTES out T_CURSOR, O_HISTORY out T_CURSOR, O_LEGACY out T_CURSOR) is
    V_ID number;
  begin
    ASSERT_ENGAGEMENT_ACCESS(P_ENG_ID,P_ENTITY_ID,P_ACTOR_PPNO,P_ROLE_ID);
    if P_WP_ID is null then
      select max(WP_ID) keep (dense_rank last order by VERSION_NO) into V_ID from T_WP_HEADER where ENTITY_ID=P_ENTITY_ID and ENG_ID=P_ENG_ID and PAPER_TYPE=P_PAPER_TYPE;
    else
      select max(WP_ID) into V_ID from T_WP_HEADER where ENTITY_ID=P_ENTITY_ID and ENG_ID=P_ENG_ID and PAPER_TYPE=P_PAPER_TYPE and WP_ID=P_WP_ID;
    end if;
    open O_HEADER for select WP_ID,ENG_ID,ENTITY_ID,PAPER_TYPE,REFERENCE_NO,VERSION_NO,STATUS,PREPARER_PPNO,REVIEWER_PPNO,
      DUE_DATE,PLAN_JSON,CONCLUSION_JSON,ROW_VERSION from T_WP_HEADER where WP_ID=V_ID;
    open O_ITEMS for select ITEM_ID,WP_ID,ITEM_REFERENCE,DETAILS_JSON,CALCULATIONS_JSON,RESULT,AUDITOR_COMMENT,ROW_VERSION
      from T_WP_ITEM where WP_ID=V_ID order by ITEM_ID;
    open O_EVIDENCE for select EVIDENCE_LINK_ID,WP_ID,ITEM_ID,EXCEPTION_ID,EXISTING_EVIDENCE_ID,TITLE,EVIDENCE_TYPE,SOURCE_NAME,EVIDENCE_DATE
      from T_WP_EVIDENCE_LINK where WP_ID=V_ID order by EVIDENCE_LINK_ID;
    open O_EXCEPTIONS for select EXCEPTION_ID,WP_ID,ITEM_ID,CRITERIA,CONDITION,CAUSE,IMPACT,RISK_RATING,OWNER_PPNO,DUE_DATE,OBSERVATION_ID,STATUS,ROW_VERSION
      from T_WP_EXCEPTION where WP_ID=V_ID order by EXCEPTION_ID;
    open O_NOTES for select NOTE_ID,WP_ID,SECTION_KEY,NOTE_TEXT,RESPONSE_TEXT,STATUS,RAISED_BY,RAISED_ON
      from T_WP_REVIEW_NOTE where WP_ID=V_ID order by NOTE_ID;
    open O_HISTORY for select HISTORY_ID,ACTION,ACTOR_PPNO,ACTION_ON,DETAILS from T_WP_HISTORY where WP_ID=V_ID order by HISTORY_ID desc;
    if P_PAPER_TYPE='LCF' then
      open O_LEGACY for select 'T_WORKING_PAPER_LOAN_CASE_FILE' SOURCE_TABLE,to_char(LC_ID) SOURCE_ID,LC_NUMBER DISPLAY_REFERENCE,
        json_object('amount' value AMOUNT,'disbursementDate' value DISB_DATE,'category' value CATEGORY,'observation' value OBSERVATION,'paraNo' value PARA_NO returning clob) LEGACY_VALUES_JSON
        from T_WORKING_PAPER_LOAN_CASE_FILE where ENG_ID=P_ENG_ID;
    elsif P_PAPER_TYPE='VCH' then
      open O_LEGACY for select 'T_WORKING_PAPER_VOUCHER_CHECKING',to_char(V_ID),V_NUMBER,
        json_object('observation' value OBSERVATION,'paraNo' value PARA_NO returning clob) from T_WORKING_PAPER_VOUCHER_CHECKING where ENG_ID=P_ENG_ID;
    elsif P_PAPER_TYPE='AOF' then
      open O_LEGACY for select 'T_WORKING_PAPER_ACCOUNT_OPENING',to_char(A_ID),V_NUMBER,
        json_object('accountNature' value A_NATURE,'observation' value OBSERVATION,'paraNo' value PARA_NO returning clob) from T_WORKING_PAPER_ACCOUNT_OPENING where ENG_ID=P_ENG_ID;
    elsif P_PAPER_TYPE='FAS' then
      open O_LEGACY for select 'T_WORKING_PAPER_FIXED_ASSETS',to_char(FA_ID),ASSET_NAME,
        json_object('physicalExistence' value PHYSICAL_EXISTANCE,'farLocation' value LOCATION_AS_PER_FAR,'difference' value DIFFERENCE,'remarks' value REMARKS returning clob) from T_WORKING_PAPER_FIXED_ASSETS where ENG_ID=P_ENG_ID;
    else
      open O_LEGACY for select 'T_WORKING_PAPER_CASH_COUNT',to_char(ID),DENOMINATION_VAULT,
        json_object('vaultNotes' value NO_CURRENCY_NOTES_VAULT,'vaultTotal' value TOTAL_AMOUNT_VAULT,'registerDenomination' value DENOMINATION_SAFE_REGISTER,
          'registerNotes' value NO_CURRENCY_NOTES_SAFE_REGISTER,'registerTotal' value TOTAL_AMOUNT_SAFE_REGISTER,'difference' value DIFFERENCE returning clob)
        from T_WORKING_PAPER_CASH_COUNT where ENG_ID=P_ENG_ID;
    end if;
  exception when no_data_found then
    open O_HEADER for select null WP_ID,null ENG_ID,null ENTITY_ID,null PAPER_TYPE,null REFERENCE_NO,null VERSION_NO,null STATUS,
      null PREPARER_PPNO,null REVIEWER_PPNO,null DUE_DATE,null PLAN_JSON,null CONCLUSION_JSON,null ROW_VERSION from dual where 1=0;
    open O_ITEMS for select * from T_WP_ITEM where 1=0; open O_EVIDENCE for select * from T_WP_EVIDENCE_LINK where 1=0;
    open O_EXCEPTIONS for select * from T_WP_EXCEPTION where 1=0; open O_NOTES for select * from T_WP_REVIEW_NOTE where 1=0;
    open O_HISTORY for select HISTORY_ID,ACTION,ACTOR_PPNO,ACTION_ON,DETAILS from T_WP_HISTORY where 1=0;
    open O_LEGACY for select null SOURCE_TABLE,null SOURCE_ID,null DISPLAY_REFERENCE,null LEGACY_VALUES_JSON from dual where 1=0;
  end;

  procedure P_SAVE_PLAN(P_ENTITY_ID number,P_ACTOR_PPNO varchar2,P_ROLE_ID number,P_WP_ID number,P_ROW_VERSION number,P_PLAN_JSON clob,O_RESULT out T_CURSOR) is
    S varchar2(20); V number; P varchar2(30); R varchar2(30);
  begin ASSERT_ACCESS(P_WP_ID,P_ENTITY_ID,P_ACTOR_PPNO,P_ROLE_ID,'EDIT',S,V,P,R); if V<>P_ROW_VERSION then raise_application_error(-20023,'Working paper was changed by another user.'); end if;
    update T_WP_HEADER set PLAN_JSON=P_PLAN_JSON,ROW_VERSION=ROW_VERSION+1,UPDATED_BY=P_ACTOR_PPNO,UPDATED_ON=systimestamp where WP_ID=P_WP_ID;
    ADD_HISTORY(P_WP_ID,'PLAN_SAVED',P_ACTOR_PPNO,P_ROLE_ID,'Plan updated',V+1); commit; OPEN_RESULT(O_RESULT,'SUCCESS','Plan saved.',P_WP_ID,P_WP_ID,V+1);
  exception when others then rollback; raise; end;

  procedure P_SAVE_ITEM(P_ENTITY_ID number,P_ACTOR_PPNO varchar2,P_ROLE_ID number,P_WP_ID number,P_ITEM_ID number,P_ITEM_REF varchar2,P_DETAILS_JSON clob,P_CALC_JSON clob,P_RESULT varchar2,P_COMMENT varchar2,P_ROW_VERSION number,O_RESULT out T_CURSOR) is
    S varchar2(20); V number; P varchar2(30); R varchar2(30); I number; RV number;
  begin ASSERT_ACCESS(P_WP_ID,P_ENTITY_ID,P_ACTOR_PPNO,P_ROLE_ID,'EDIT',S,V,P,R);
    if P_ITEM_ID is null then I:=SEQ_WP_ITEM.nextval; RV:=1; insert into T_WP_ITEM(ITEM_ID,WP_ID,ITEM_REFERENCE,DETAILS_JSON,CALCULATIONS_JSON,RESULT,AUDITOR_COMMENT,CREATED_BY,UPDATED_BY)
      values(I,P_WP_ID,P_ITEM_REF,P_DETAILS_JSON,P_CALC_JSON,P_RESULT,P_COMMENT,P_ACTOR_PPNO,P_ACTOR_PPNO);
    else update T_WP_ITEM set ITEM_REFERENCE=P_ITEM_REF,DETAILS_JSON=P_DETAILS_JSON,CALCULATIONS_JSON=P_CALC_JSON,RESULT=P_RESULT,AUDITOR_COMMENT=P_COMMENT,
      ROW_VERSION=ROW_VERSION+1,UPDATED_BY=P_ACTOR_PPNO,UPDATED_ON=systimestamp where ITEM_ID=P_ITEM_ID and WP_ID=P_WP_ID and ROW_VERSION=P_ROW_VERSION;
      if sql%rowcount=0 then raise_application_error(-20023,'Item was changed by another user.'); end if; I:=P_ITEM_ID; RV:=P_ROW_VERSION+1; end if;
    update T_WP_HEADER set ROW_VERSION=ROW_VERSION+1,UPDATED_BY=P_ACTOR_PPNO,UPDATED_ON=systimestamp where WP_ID=P_WP_ID;
    ADD_HISTORY(P_WP_ID,'ITEM_SAVED',P_ACTOR_PPNO,P_ROLE_ID,'Item '||I,V+1); commit; OPEN_RESULT(O_RESULT,'SUCCESS','Test item saved.',P_WP_ID,I,RV);
  exception when others then rollback; raise; end;

  procedure P_LINK_EVIDENCE(P_ENTITY_ID number,P_ACTOR_PPNO varchar2,P_ROLE_ID number,P_WP_ID number,P_ITEM_ID number,P_EXCEPTION_ID number,P_EXISTING_EVIDENCE_ID varchar2,P_TITLE varchar2,P_EVIDENCE_TYPE varchar2,P_SOURCE varchar2,P_EVIDENCE_DATE date,O_RESULT out T_CURSOR) is
    S varchar2(20); V number; P varchar2(30); R varchar2(30); I number; C number;
  begin ASSERT_ACCESS(P_WP_ID,P_ENTITY_ID,P_ACTOR_PPNO,P_ROLE_ID,'EDIT',S,V,P,R);
    if P_ITEM_ID is not null then select count(*) into C from T_WP_ITEM where ITEM_ID=P_ITEM_ID and WP_ID=P_WP_ID; if C=0 then raise_application_error(-20026,'Evidence item is outside this working paper.'); end if; end if;
    if P_EXCEPTION_ID is not null then select count(*) into C from T_WP_EXCEPTION where EXCEPTION_ID=P_EXCEPTION_ID and WP_ID=P_WP_ID; if C=0 then raise_application_error(-20027,'Evidence exception is outside this working paper.'); end if; end if;
    I:=SEQ_WP_EVIDENCE_LINK.nextval;
    insert into T_WP_EVIDENCE_LINK values(I,P_WP_ID,P_ITEM_ID,P_EXCEPTION_ID,P_EXISTING_EVIDENCE_ID,P_TITLE,P_EVIDENCE_TYPE,P_SOURCE,P_EVIDENCE_DATE,P_ACTOR_PPNO,systimestamp);
    update T_WP_HEADER set ROW_VERSION=ROW_VERSION+1,UPDATED_BY=P_ACTOR_PPNO,UPDATED_ON=systimestamp where WP_ID=P_WP_ID;
    ADD_HISTORY(P_WP_ID,'EVIDENCE_LINKED',P_ACTOR_PPNO,P_ROLE_ID,'Evidence link '||I,V+1); commit; OPEN_RESULT(O_RESULT,'SUCCESS','Evidence linked.',P_WP_ID,I,V+1);
  exception when others then rollback; raise; end;

  procedure P_SAVE_EXCEPTION(P_ENTITY_ID number,P_ACTOR_PPNO varchar2,P_ROLE_ID number,P_WP_ID number,P_EXCEPTION_ID number,P_ITEM_ID number,P_CRITERIA varchar2,P_CONDITION varchar2,P_CAUSE varchar2,P_IMPACT varchar2,P_RISK varchar2,P_OWNER_PPNO varchar2,P_DUE_DATE date,P_OBSERVATION_ID number,P_STATUS varchar2,P_ROW_VERSION number,O_RESULT out T_CURSOR) is
    S varchar2(20); V number; P varchar2(30); R varchar2(30); I number; RV number; C number;
  begin ASSERT_ACCESS(P_WP_ID,P_ENTITY_ID,P_ACTOR_PPNO,P_ROLE_ID,'EDIT',S,V,P,R);
    select count(*) into C from T_WP_ITEM where ITEM_ID=P_ITEM_ID and WP_ID=P_WP_ID;
    if C=0 then raise_application_error(-20028,'Exception item is outside this working paper.'); end if;
    if P_EXCEPTION_ID is null then I:=SEQ_WP_EXCEPTION.nextval; RV:=1; insert into T_WP_EXCEPTION(EXCEPTION_ID,WP_ID,ITEM_ID,CRITERIA,CONDITION,CAUSE,IMPACT,RISK_RATING,OWNER_PPNO,DUE_DATE,OBSERVATION_ID,STATUS,CREATED_BY,UPDATED_BY)
      values(I,P_WP_ID,P_ITEM_ID,P_CRITERIA,P_CONDITION,P_CAUSE,P_IMPACT,P_RISK,P_OWNER_PPNO,P_DUE_DATE,P_OBSERVATION_ID,case when P_OBSERVATION_ID is not null then 'LINKED' when P_STATUS='RESOLVED' then 'RESOLVED' else 'OPEN' end,P_ACTOR_PPNO,P_ACTOR_PPNO);
    else update T_WP_EXCEPTION set CRITERIA=P_CRITERIA,CONDITION=P_CONDITION,CAUSE=P_CAUSE,IMPACT=P_IMPACT,RISK_RATING=P_RISK,OWNER_PPNO=P_OWNER_PPNO,DUE_DATE=P_DUE_DATE,
      OBSERVATION_ID=P_OBSERVATION_ID,STATUS=case when P_OBSERVATION_ID is not null then 'LINKED' when P_STATUS='RESOLVED' then 'RESOLVED' else 'OPEN' end,ROW_VERSION=ROW_VERSION+1,UPDATED_BY=P_ACTOR_PPNO,UPDATED_ON=systimestamp
      where EXCEPTION_ID=P_EXCEPTION_ID and WP_ID=P_WP_ID and ROW_VERSION=P_ROW_VERSION; if sql%rowcount=0 then raise_application_error(-20023,'Exception was changed by another user.'); end if; I:=P_EXCEPTION_ID; RV:=P_ROW_VERSION+1; end if;
    update T_WP_HEADER set ROW_VERSION=ROW_VERSION+1,UPDATED_BY=P_ACTOR_PPNO,UPDATED_ON=systimestamp where WP_ID=P_WP_ID;
    ADD_HISTORY(P_WP_ID,'EXCEPTION_SAVED',P_ACTOR_PPNO,P_ROLE_ID,'Exception '||I,V+1); commit; OPEN_RESULT(O_RESULT,'SUCCESS','Exception saved.',P_WP_ID,I,RV);
  exception when others then rollback; raise; end;

  procedure P_SAVE_CONCLUSION(P_ENTITY_ID number,P_ACTOR_PPNO varchar2,P_ROLE_ID number,P_WP_ID number,P_ROW_VERSION number,P_CONCLUSION_JSON clob,O_RESULT out T_CURSOR) is
    S varchar2(20); V number; P varchar2(30); R varchar2(30);
  begin ASSERT_ACCESS(P_WP_ID,P_ENTITY_ID,P_ACTOR_PPNO,P_ROLE_ID,'EDIT',S,V,P,R); if V<>P_ROW_VERSION then raise_application_error(-20023,'Working paper was changed by another user.'); end if;
    update T_WP_HEADER set CONCLUSION_JSON=P_CONCLUSION_JSON,ROW_VERSION=ROW_VERSION+1,UPDATED_BY=P_ACTOR_PPNO,UPDATED_ON=systimestamp where WP_ID=P_WP_ID;
    ADD_HISTORY(P_WP_ID,'CONCLUSION_SAVED',P_ACTOR_PPNO,P_ROLE_ID,'Conclusion updated',V+1); commit; OPEN_RESULT(O_RESULT,'SUCCESS','Conclusion saved.',P_WP_ID,P_WP_ID,V+1);
  exception when others then rollback; raise; end;

  procedure P_SAVE_REVIEW_NOTE(P_ENTITY_ID number,P_ACTOR_PPNO varchar2,P_ROLE_ID number,P_WP_ID number,P_NOTE_ID number,P_SECTION_KEY varchar2,P_NOTE_TEXT varchar2,P_RESPONSE_TEXT varchar2,P_STATUS varchar2,O_RESULT out T_CURSOR) is
    S varchar2(20); V number; P varchar2(30); R varchar2(30); I number; OLD_STATUS varchar2(20);
  begin ASSERT_ACCESS(P_WP_ID,P_ENTITY_ID,P_ACTOR_PPNO,P_ROLE_ID,'ANY',S,V,P,R);
    if S<>'IN_REVIEW' then raise_application_error(-20029,'Review notes may only change during review.'); end if;
    if P_NOTE_ID is null then
      if P_ACTOR_PPNO<>R then raise_application_error(-20022,'Only the assigned reviewer may create review notes.'); end if;
      I:=SEQ_WP_REVIEW_NOTE.nextval; insert into T_WP_REVIEW_NOTE(NOTE_ID,WP_ID,SECTION_KEY,NOTE_TEXT,STATUS,RAISED_BY) values(I,P_WP_ID,P_SECTION_KEY,P_NOTE_TEXT,'OPEN',P_ACTOR_PPNO);
    else
      select STATUS into OLD_STATUS from T_WP_REVIEW_NOTE where NOTE_ID=P_NOTE_ID and WP_ID=P_WP_ID for update;
      if P_STATUS='RESPONDED' then
        if P_ACTOR_PPNO<>P or P_RESPONSE_TEXT is null then raise_application_error(-20037,'Only the preparer may submit a substantive response.'); end if;
        update T_WP_REVIEW_NOTE set RESPONSE_TEXT=P_RESPONSE_TEXT,STATUS='RESPONDED',RESPONDED_BY=P_ACTOR_PPNO,RESPONDED_ON=systimestamp where NOTE_ID=P_NOTE_ID and WP_ID=P_WP_ID;
      elsif P_STATUS='CLOSED' then
        if P_ACTOR_PPNO<>R or OLD_STATUS<>'RESPONDED' then raise_application_error(-20038,'Only the assigned reviewer may close a responded note.'); end if;
        update T_WP_REVIEW_NOTE set STATUS='CLOSED',CLOSED_BY=P_ACTOR_PPNO,CLOSED_ON=systimestamp where NOTE_ID=P_NOTE_ID and WP_ID=P_WP_ID;
      else raise_application_error(-20039,'Invalid review-note transition.'); end if;
      I:=P_NOTE_ID;
    end if;
    update T_WP_HEADER set ROW_VERSION=ROW_VERSION+1,UPDATED_BY=P_ACTOR_PPNO,UPDATED_ON=systimestamp where WP_ID=P_WP_ID;
    ADD_HISTORY(P_WP_ID,'REVIEW_NOTE_SAVED',P_ACTOR_PPNO,P_ROLE_ID,'Review note '||I,V+1); commit; OPEN_RESULT(O_RESULT,'SUCCESS','Review note saved.',P_WP_ID,I,V+1);
  exception when no_data_found then rollback; raise_application_error(-20040,'Review note not found or access denied.'); when others then rollback; raise; end;

  procedure P_WORKFLOW(P_ENTITY_ID number,P_ACTOR_PPNO varchar2,P_ROLE_ID number,P_WP_ID number,P_ACTION varchar2,P_REMARKS varchar2,P_ROW_VERSION number,O_RESULT out T_CURSOR) is
    S varchar2(20); V number; P varchar2(30); R varchar2(30); N varchar2(20); C number;
    NEW_ID number; NEW_VER number; ROOT_ID number; ENG number; PT varchar2(3); REF varchar2(60); DUE date; PLAN clob; CONC clob; NI number; NX number;
    type T_ID_MAP is table of number index by varchar2(40); ITEM_MAP T_ID_MAP; EX_MAP T_ID_MAP;
  begin ASSERT_ACCESS(P_WP_ID,P_ENTITY_ID,P_ACTOR_PPNO,P_ROLE_ID,'ANY',S,V,P,R); if V<>P_ROW_VERSION then raise_application_error(-20023,'Working paper was changed by another user.'); end if;
    if P_ACTION='SUBMIT' and S in ('DRAFT','RETURNED') and P_ACTOR_PPNO=P then ASSERT_COMPLETE(P_WP_ID); N:='IN_REVIEW';
    elsif P_ACTION='RETURN' and S='IN_REVIEW' and P_ACTOR_PPNO=R then if P_REMARKS is null then raise_application_error(-20041,'Return remarks are required.'); end if; N:='RETURNED';
    elsif P_ACTION='REVIEW' and S='IN_REVIEW' and P_ACTOR_PPNO=R then ASSERT_COMPLETE(P_WP_ID); select count(*) into C from T_WP_REVIEW_NOTE where WP_ID=P_WP_ID and STATUS<>'CLOSED'; if C>0 then raise_application_error(-20024,'Open review notes must be closed.'); end if; N:='REVIEWED';
    elsif P_ACTION='APPROVE' and S='REVIEWED' and P_ACTOR_PPNO=R then ASSERT_COMPLETE(P_WP_ID); select count(*) into C from T_WP_REVIEW_NOTE where WP_ID=P_WP_ID and STATUS<>'CLOSED'; if C>0 then raise_application_error(-20024,'Open review notes must be closed.'); end if; N:='APPROVED';
    elsif P_ACTION='REOPEN' and S='APPROVED' and P_ROLE_ID in (1,2) then
      if P_REMARKS is null then raise_application_error(-20042,'Reopen reason is required.'); end if;
      select ROOT_WP_ID,ENG_ID,PAPER_TYPE,DUE_DATE,PLAN_JSON,CONCLUSION_JSON into ROOT_ID,ENG,PT,DUE,PLAN,CONC from T_WP_HEADER where WP_ID=P_WP_ID;
      select max(VERSION_NO)+1 into NEW_VER from T_WP_HEADER where ROOT_WP_ID=ROOT_ID;
      NEW_ID:=SEQ_WP_HEADER.nextval; REF:=PT||'-'||ENG||'-V'||NEW_VER||'-'||to_char(NEW_ID,'FM000000');
      insert into T_WP_HEADER(WP_ID,ROOT_WP_ID,SUPERSEDES_WP_ID,ENG_ID,ENTITY_ID,PAPER_TYPE,REFERENCE_NO,VERSION_NO,STATUS,PREPARER_PPNO,REVIEWER_PPNO,DUE_DATE,PLAN_JSON,CONCLUSION_JSON,CREATED_BY,UPDATED_BY)
      values(NEW_ID,ROOT_ID,P_WP_ID,ENG,P_ENTITY_ID,PT,REF,NEW_VER,'RETURNED',P,R,DUE,PLAN,CONC,P_ACTOR_PPNO,P_ACTOR_PPNO);
      for X in (select * from T_WP_ITEM where WP_ID=P_WP_ID order by ITEM_ID) loop NI:=SEQ_WP_ITEM.nextval; ITEM_MAP(to_char(X.ITEM_ID)):=NI;
        insert into T_WP_ITEM(ITEM_ID,WP_ID,ITEM_REFERENCE,DETAILS_JSON,CALCULATIONS_JSON,RESULT,AUDITOR_COMMENT,CREATED_BY,UPDATED_BY) values(NI,NEW_ID,X.ITEM_REFERENCE,X.DETAILS_JSON,X.CALCULATIONS_JSON,X.RESULT,X.AUDITOR_COMMENT,P_ACTOR_PPNO,P_ACTOR_PPNO); end loop;
      for X in (select * from T_WP_EXCEPTION where WP_ID=P_WP_ID order by EXCEPTION_ID) loop NX:=SEQ_WP_EXCEPTION.nextval; EX_MAP(to_char(X.EXCEPTION_ID)):=NX;
        insert into T_WP_EXCEPTION(EXCEPTION_ID,WP_ID,ITEM_ID,CRITERIA,CONDITION,CAUSE,IMPACT,RISK_RATING,OWNER_PPNO,DUE_DATE,OBSERVATION_ID,STATUS,CREATED_BY,UPDATED_BY) values(NX,NEW_ID,ITEM_MAP(to_char(X.ITEM_ID)),X.CRITERIA,X.CONDITION,X.CAUSE,X.IMPACT,X.RISK_RATING,X.OWNER_PPNO,X.DUE_DATE,X.OBSERVATION_ID,X.STATUS,P_ACTOR_PPNO,P_ACTOR_PPNO); end loop;
      for X in (select * from T_WP_EVIDENCE_LINK where WP_ID=P_WP_ID order by EVIDENCE_LINK_ID) loop
        insert into T_WP_EVIDENCE_LINK(EVIDENCE_LINK_ID,WP_ID,ITEM_ID,EXCEPTION_ID,EXISTING_EVIDENCE_ID,TITLE,EVIDENCE_TYPE,SOURCE_NAME,EVIDENCE_DATE,LINKED_BY)
        values(SEQ_WP_EVIDENCE_LINK.nextval,NEW_ID,case when X.ITEM_ID is null then null else ITEM_MAP(to_char(X.ITEM_ID)) end,case when X.EXCEPTION_ID is null then null else EX_MAP(to_char(X.EXCEPTION_ID)) end,X.EXISTING_EVIDENCE_ID,X.TITLE,X.EVIDENCE_TYPE,X.SOURCE_NAME,X.EVIDENCE_DATE,P_ACTOR_PPNO); end loop;
      ADD_HISTORY(P_WP_ID,'REOPENED_AS_NEW_VERSION',P_ACTOR_PPNO,P_ROLE_ID,'New working paper '||NEW_ID||': '||P_REMARKS,V);
      ADD_HISTORY(NEW_ID,'CREATED_FROM_APPROVED',P_ACTOR_PPNO,P_ROLE_ID,'Source working paper '||P_WP_ID,1);
      commit; OPEN_RESULT(O_RESULT,'SUCCESS','Approved version preserved; editable version created.',NEW_ID,NEW_ID,1); return;
    else raise_application_error(-20025,'Invalid workflow transition or actor.'); end if;
    update T_WP_HEADER set STATUS=N,ROW_VERSION=ROW_VERSION+1,UPDATED_BY=P_ACTOR_PPNO,UPDATED_ON=systimestamp,
      PREPARED_BY=case when N='IN_REVIEW' then P_ACTOR_PPNO else PREPARED_BY end,PREPARED_ON=case when N='IN_REVIEW' then systimestamp else PREPARED_ON end,
      REVIEWED_BY=case when N='REVIEWED' then P_ACTOR_PPNO else REVIEWED_BY end,REVIEWED_ON=case when N='REVIEWED' then systimestamp else REVIEWED_ON end,
      APPROVED_BY=case when N='APPROVED' then P_ACTOR_PPNO else APPROVED_BY end,APPROVED_ON=case when N='APPROVED' then systimestamp else APPROVED_ON end where WP_ID=P_WP_ID;
    ADD_HISTORY(P_WP_ID,P_ACTION,P_ACTOR_PPNO,P_ROLE_ID,P_REMARKS,V+1); commit; OPEN_RESULT(O_RESULT,'SUCCESS','Working paper status changed to '||N||'.',P_WP_ID,P_WP_ID,V+1);
  exception when others then rollback; raise; end;
end PKG_WORKING_PAPER;
/
