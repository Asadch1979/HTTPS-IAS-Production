-- IAS Working Papers v2 - corrective upgrade for the original foundation.
-- Run only when 001/002 were installed before this correction. Do not run after the corrected 001.
-- This script is additive and does not update or delete legacy working-paper data.

alter table T_WP_HEADER add (
  ROOT_WP_ID       number(19),
  SUPERSEDES_WP_ID number(19),
  PREPARED_BY      varchar2(30 char),
  PREPARED_ON      timestamp,
  REVIEWED_BY      varchar2(30 char),
  REVIEWED_ON      timestamp
);

update T_WP_HEADER set ROOT_WP_ID = WP_ID where ROOT_WP_ID is null;
alter table T_WP_HEADER modify ROOT_WP_ID not null;

alter table T_WP_HEADER add constraint FK_WP_HEADER_ROOT
  foreign key (ROOT_WP_ID) references T_WP_HEADER(WP_ID);
alter table T_WP_HEADER add constraint FK_WP_HEADER_SUPERSEDES
  foreign key (SUPERSEDES_WP_ID) references T_WP_HEADER(WP_ID);
create index IX_WP_HEADER_LINEAGE on T_WP_HEADER(ROOT_WP_ID, VERSION_NO);

-- Recompile the corrected package after this script.
@@002_create_working_paper_package.sql
