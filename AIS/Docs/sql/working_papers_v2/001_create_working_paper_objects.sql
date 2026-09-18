-- IAS Working Papers v2 - additive schema
-- Review and execute through the approved DBA release process. Do not run automatically.
-- Legacy T_WORKING_PAPER_* objects are intentionally not altered.

create sequence SEQ_WP_HEADER start with 1 increment by 1 nocache nocycle;
create sequence SEQ_WP_ITEM start with 1 increment by 1 nocache nocycle;
create sequence SEQ_WP_EVIDENCE_LINK start with 1 increment by 1 nocache nocycle;
create sequence SEQ_WP_EXCEPTION start with 1 increment by 1 nocache nocycle;
create sequence SEQ_WP_REVIEW_NOTE start with 1 increment by 1 nocache nocycle;
create sequence SEQ_WP_HISTORY start with 1 increment by 1 nocache nocycle;

create table T_WP_HEADER (
  WP_ID             number(19) not null,
  ENG_ID            number(19) not null,
  ENTITY_ID         number(10) not null,
  PAPER_TYPE        varchar2(3 char) not null,
  REFERENCE_NO      varchar2(60 char) not null,
  VERSION_NO        number(10) default 1 not null,
  STATUS            varchar2(20 char) default 'DRAFT' not null,
  PREPARER_PPNO     varchar2(30 char) not null,
  REVIEWER_PPNO     varchar2(30 char) not null,
  DUE_DATE          date,
  PLAN_JSON         clob,
  CONCLUSION_JSON   clob,
  ROW_VERSION       number(19) default 1 not null,
  CREATED_BY        varchar2(30 char) not null,
  CREATED_ON        timestamp default systimestamp not null,
  UPDATED_BY        varchar2(30 char) not null,
  UPDATED_ON        timestamp default systimestamp not null,
  APPROVED_BY       varchar2(30 char),
  APPROVED_ON       timestamp,
  constraint PK_WP_HEADER primary key (WP_ID),
  constraint UQ_WP_HEADER_REF unique (REFERENCE_NO),
  constraint UQ_WP_HEADER_SCOPE unique (ENG_ID, ENTITY_ID, PAPER_TYPE, VERSION_NO),
  constraint CK_WP_HEADER_TYPE check (PAPER_TYPE in ('LCF','VCH','AOF','FAS','CCT')),
  constraint CK_WP_HEADER_STATUS check (STATUS in ('DRAFT','IN_REVIEW','RETURNED','REVIEWED','APPROVED')),
  constraint CK_WP_HEADER_PLAN_JSON check (PLAN_JSON is json),
  constraint CK_WP_HEADER_CONCLUSION_JSON check (CONCLUSION_JSON is json)
);

create table T_WP_ITEM (
  ITEM_ID           number(19) not null,
  WP_ID             number(19) not null,
  ITEM_REFERENCE    varchar2(120 char) not null,
  DETAILS_JSON      clob not null,
  CALCULATIONS_JSON clob,
  RESULT            varchar2(30 char) default 'PENDING' not null,
  AUDITOR_COMMENT   varchar2(4000 char),
  ROW_VERSION       number(19) default 1 not null,
  CREATED_BY        varchar2(30 char) not null,
  CREATED_ON        timestamp default systimestamp not null,
  UPDATED_BY        varchar2(30 char) not null,
  UPDATED_ON        timestamp default systimestamp not null,
  constraint PK_WP_ITEM primary key (ITEM_ID),
  constraint FK_WP_ITEM_HEADER foreign key (WP_ID) references T_WP_HEADER(WP_ID),
  constraint UQ_WP_ITEM_REF unique (WP_ID, ITEM_REFERENCE),
  constraint CK_WP_ITEM_DETAILS_JSON check (DETAILS_JSON is json),
  constraint CK_WP_ITEM_CALC_JSON check (CALCULATIONS_JSON is json),
  constraint CK_WP_ITEM_RESULT check (RESULT in ('PASS','EXCEPTION','NOT_APPLICABLE','NOT_TESTED','PENDING'))
);

create table T_WP_EVIDENCE_LINK (
  EVIDENCE_LINK_ID    number(19) not null,
  WP_ID               number(19) not null,
  ITEM_ID             number(19),
  EXCEPTION_ID        number(19),
  EXISTING_EVIDENCE_ID varchar2(100 char) not null,
  TITLE               varchar2(300 char) not null,
  EVIDENCE_TYPE       varchar2(100 char),
  SOURCE_NAME         varchar2(500 char),
  EVIDENCE_DATE       date,
  LINKED_BY           varchar2(30 char) not null,
  LINKED_ON           timestamp default systimestamp not null,
  constraint PK_WP_EVIDENCE_LINK primary key (EVIDENCE_LINK_ID),
  constraint FK_WP_EVIDENCE_HEADER foreign key (WP_ID) references T_WP_HEADER(WP_ID),
  constraint FK_WP_EVIDENCE_ITEM foreign key (ITEM_ID) references T_WP_ITEM(ITEM_ID),
  constraint UQ_WP_EVIDENCE unique (WP_ID, EXISTING_EVIDENCE_ID, ITEM_ID)
);

create table T_WP_EXCEPTION (
  EXCEPTION_ID      number(19) not null,
  WP_ID             number(19) not null,
  ITEM_ID           number(19) not null,
  CRITERIA          varchar2(4000 char) not null,
  CONDITION         varchar2(4000 char) not null,
  CAUSE             varchar2(4000 char),
  IMPACT            varchar2(4000 char) not null,
  RISK_RATING       varchar2(20 char) not null,
  OWNER_PPNO        varchar2(30 char),
  DUE_DATE          date,
  OBSERVATION_ID    number(19),
  STATUS            varchar2(20 char) default 'OPEN' not null,
  ROW_VERSION       number(19) default 1 not null,
  CREATED_BY        varchar2(30 char) not null,
  CREATED_ON        timestamp default systimestamp not null,
  UPDATED_BY        varchar2(30 char) not null,
  UPDATED_ON        timestamp default systimestamp not null,
  constraint PK_WP_EXCEPTION primary key (EXCEPTION_ID),
  constraint FK_WP_EXCEPTION_HEADER foreign key (WP_ID) references T_WP_HEADER(WP_ID),
  constraint FK_WP_EXCEPTION_ITEM foreign key (ITEM_ID) references T_WP_ITEM(ITEM_ID),
  constraint CK_WP_EXCEPTION_RISK check (RISK_RATING in ('LOW','MEDIUM','HIGH','CRITICAL')),
  constraint CK_WP_EXCEPTION_STATUS check (STATUS in ('OPEN','RESOLVED','LINKED'))
);

alter table T_WP_EVIDENCE_LINK add constraint FK_WP_EVIDENCE_EXCEPTION
  foreign key (EXCEPTION_ID) references T_WP_EXCEPTION(EXCEPTION_ID);

create table T_WP_REVIEW_NOTE (
  NOTE_ID           number(19) not null,
  WP_ID             number(19) not null,
  SECTION_KEY       varchar2(100 char) not null,
  NOTE_TEXT         varchar2(4000 char) not null,
  RESPONSE_TEXT     varchar2(4000 char),
  STATUS            varchar2(20 char) default 'OPEN' not null,
  RAISED_BY         varchar2(30 char) not null,
  RAISED_ON         timestamp default systimestamp not null,
  RESPONDED_BY      varchar2(30 char),
  RESPONDED_ON      timestamp,
  CLOSED_BY         varchar2(30 char),
  CLOSED_ON         timestamp,
  constraint PK_WP_REVIEW_NOTE primary key (NOTE_ID),
  constraint FK_WP_NOTE_HEADER foreign key (WP_ID) references T_WP_HEADER(WP_ID),
  constraint CK_WP_NOTE_STATUS check (STATUS in ('OPEN','RESPONDED','CLOSED'))
);

create table T_WP_HISTORY (
  HISTORY_ID        number(19) not null,
  WP_ID             number(19) not null,
  ACTION            varchar2(50 char) not null,
  ACTOR_PPNO        varchar2(30 char) not null,
  ACTOR_ROLE_ID     number(10) not null,
  ACTION_ON         timestamp default systimestamp not null,
  DETAILS           varchar2(4000 char),
  ROW_VERSION       number(19),
  constraint PK_WP_HISTORY primary key (HISTORY_ID),
  constraint FK_WP_HISTORY_HEADER foreign key (WP_ID) references T_WP_HEADER(WP_ID)
);

create index IX_WP_HEADER_ENGAGEMENT on T_WP_HEADER(ENG_ID, ENTITY_ID, PAPER_TYPE, STATUS);
create index IX_WP_HEADER_REVIEWER on T_WP_HEADER(REVIEWER_PPNO, STATUS);
create index IX_WP_ITEM_HEADER on T_WP_ITEM(WP_ID, RESULT);
create index IX_WP_EXCEPTION_HEADER on T_WP_EXCEPTION(WP_ID, STATUS, RISK_RATING);
create index IX_WP_EXCEPTION_OBS on T_WP_EXCEPTION(OBSERVATION_ID);
create index IX_WP_NOTE_HEADER on T_WP_REVIEW_NOTE(WP_ID, STATUS);
create index IX_WP_HISTORY_HEADER on T_WP_HISTORY(WP_ID, ACTION_ON);

comment on table T_WP_HEADER is 'IAS Working Papers v2. Additive; legacy working-paper tables remain authoritative for legacy records.';
comment on column T_WP_ITEM.DETAILS_JSON is 'Paper-specific typed contract validated by application and JSON constraints; signed values are retained with version history.';
