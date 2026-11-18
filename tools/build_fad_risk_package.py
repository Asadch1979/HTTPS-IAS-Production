"""Build the package signatures from one security-context contract."""
from pathlib import Path
root=Path(__file__).resolve().parents[1]
folder=root/'AIS/Docs/sql/fad-risk-register'
ctx='p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2'
args='p_user,p_role,p_entity,p_group,p_context,p_session'
signatures={
 'P_CATALOG':ctx+',o_versions OUT SYS_REFCURSOR,o_assessments OUT SYS_REFCURSOR',
 'P_READ':ctx+',p_eng NUMBER,p_id NUMBER,p_version NUMBER,o_header OUT SYS_REFCURSOR,o_details OUT SYS_REFCURSOR,o_results OUT SYS_REFCURSOR,o_history OUT SYS_REFCURSOR',
 'P_SAVE':ctx+',p_eng NUMBER,p_id IN OUT NUMBER,p_version NUMBER,p_revision NUMBER,p_size VARCHAR2,p_portfolio NUMBER,p_reason VARCHAR2,p_details CLOB',
 'P_TRANSITION':ctx+',p_eng NUMBER,p_id NUMBER,p_revision NUMBER,p_action VARCHAR2,p_remarks VARCHAR2',
}
body=(folder/'assessment-body.sql.inc').read_text(encoding='utf-8')
for name,sig in signatures.items(): body=body.replace('@'+name+'@',f'PROCEDURE {name}({sig})')
body=body.replace('@CTX@',ctx).replace('@ARGS@',args)
text='-- SOURCE ONLY. Oracle 19c+. No commits: caller owns the transaction.\nCREATE OR REPLACE PACKAGE PKG_FAD_RISK_ASSESS AS\n'
text+='\n'.join(f' {"PROCEDURE"} {k}({v});' for k,v in signatures.items())
text+='\nEND PKG_FAD_RISK_ASSESS;\n/\nCREATE OR REPLACE PACKAGE BODY PKG_FAD_RISK_ASSESS AS\n'+body+'\nEND PKG_FAD_RISK_ASSESS;\n/\n'
(folder/'30_assessment_package.sql').write_text(text,encoding='utf-8')
