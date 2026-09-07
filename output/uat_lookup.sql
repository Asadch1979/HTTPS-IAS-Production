set pages 500 lines 220 trimspool on feedback off verify off
column team_name format a40
column member_name format a35
select t_id, team_name, member_ppno, member_name, isteamlead, place_of_posting, status
from t_au_team_members
where member_ppno in (125031,105975,113808,46536,111573)
order by t_id, isteamlead desc, member_ppno;

select t_id, team_name,
       max(case when isteamlead='Y' then member_ppno end) lead_ppno,
       listagg(member_ppno||':'||nvl(isteamlead,'N'), ',') within group(order by member_ppno) members
from t_au_team_members
where status='Y'
group by t_id, team_name
having max(case when member_ppno=125031 and isteamlead='Y' then 1 else 0 end)=1
order by t_id desc;

select entity_id, name, type_id, auditable, status
from t_au_entities
where entity_id in (112229,112214);
exit
