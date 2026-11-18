"""Generate reviewable Oracle import from the complete, hashed workbook extraction."""
import json, pathlib
from fad_risk_workbook import extract, configuration
ROOT = pathlib.Path(__file__).resolve().parents[1]
source = extract()
cfg = configuration(source)
def q(s): return "'" + str(s).replace("'", "''") + "'"
lines = ["-- Generated from the reference workbook; review, then execute once. No automatic publication.",
 "SET DEFINE OFF", "WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK", "DECLARE v NUMBER; BEGIN",
 "INSERT INTO T_FAD_RISK_VERSION(version_name,source_sha256,rules) VALUES(" + q('excel-formula-1')+','+q(source['sha256'])+','+
 q(json.dumps({'thresholds':{'Small':[10,40],'Medium':[15,50],'Large':[20,60]},'comparisons':'strict-less-than',
 'zero_denominator':'STOP_FOR_REVIEW','branch_size':'explicit-reviewed-input','profit_unit':'rupees','fraud':'zero-marks-force-High'}))+') RETURNING version_id INTO v;']
matrix = source['sheets']['Rating Matrix']
for p in cfg['processes']:
 lines.append('INSERT INTO T_FAD_RISK_PROCESS VALUES(v,'+','.join(q(p[k]) for k in ['id','main_kpi','sub_kpi','name'])+f",{p['weight']},{p['normalization_total']});")
 for r in p['matrix_rows']:
  criterion = 'movement' if r in (33,34) else 'shortfall' if r in (37,38,39) else 'profit' if r==64 else 'fraud_override' if r==62 else 'instances'
  description = matrix.get(f'C{r}',matrix.get(f'B{r}'))['value']
  gravity = matrix[f'H{r}']['value'] if criterion in ('instances','fraud_override') else 'NULL'
  lines.append('INSERT INTO T_FAD_RISK_SUBPROCESS VALUES(v,'+q(f'RM{r}')+','+q(p['id'])+','+q(description)+','+gravity+','+q(criterion)+');')
for a in cfg['annexures']:
 lines.append('INSERT INTO T_FAD_RISK_ANNEX VALUES(v,'+','.join(q(a[k]) for k in ['code','sub_process_id','description','risk_level','source'])+');')
lines += ['END;', '/', 'COMMIT;']
out = ROOT/'AIS/Docs/sql/fad-risk-register/20_workbook_seed.sql'
out.write_text('\n'.join(lines)+'\n',encoding='utf-8')
# Row triggers lock the parent version/header before checking state, avoiding a
# race between publication/approval and a concurrent child update.
locks=[]
for table,key,parent,parentkey,state in [
 ('PROCESS','version_id','T_FAD_RISK_VERSION','version_id','PUBLISHED'),
 ('SUBPROCESS','version_id','T_FAD_RISK_VERSION','version_id','PUBLISHED'),
 ('ANNEX','version_id','T_FAD_RISK_VERSION','version_id','PUBLISHED'),
 ('DETAIL','assessment_id','T_FAD_RISK_ASSESSMENT','assessment_id','APPROVED'),
 ('RESULT','assessment_id','T_FAD_RISK_ASSESSMENT','assessment_id','APPROVED')]:
 col='state' if state=='PUBLISHED' else 'status'
 locks.append(f'''CREATE OR REPLACE TRIGGER TR_FR_{table}_LOCK
BEFORE INSERT OR UPDATE OR DELETE ON T_FAD_RISK_{table} FOR EACH ROW
DECLARE s VARCHAR2(15); k NUMBER;
BEGIN
 IF UPDATING AND :OLD.{key}<>:NEW.{key} THEN RAISE_APPLICATION_ERROR(-20045,'Parent cannot change.'); END IF;
 IF DELETING THEN k:=:OLD.{key}; ELSE k:=:NEW.{key}; END IF;
 SELECT {col} INTO s FROM {parent} WHERE {parentkey}=k FOR UPDATE;
 IF s='{state}' THEN RAISE_APPLICATION_ERROR(-20045,'Immutable parent.'); END IF;
END;
/''')
(out.parent/'11_immutability.sql').write_text('\n'.join(locks)+'\n',encoding='utf-8')
print('Generated 121 Annexures and immutable snapshot guards.')
checks=['-- Generated exact workbook contract for calculator V1.',
 'CREATE OR REPLACE PROCEDURE P_CHECK_FAD_RISK_V1(v NUMBER) AUTHID DEFINER IS n NUMBER; BEGIN']
for table,count in [('PROCESS',20),('ANNEX',121),('SUBPROCESS',sum(len(p['matrix_rows']) for p in cfg['processes']))]:
 checks += [f"SELECT COUNT(*) INTO n FROM T_FAD_RISK_{table} WHERE version_id=v;",
 f"IF n<>{count} THEN RAISE_APPLICATION_ERROR(-20040,'Incomplete {table} configuration.'); END IF;"]
for p in cfg['processes']:
 checks += [f"SELECT COUNT(*) INTO n FROM T_FAD_RISK_PROCESS WHERE version_id=v AND process_code={q(p['id'])} AND main_kpi={q(p['main_kpi'])} AND sub_kpi={q(p['sub_kpi'])} AND weight={p['weight']} AND denominator={p['normalization_total']};",
 "IF n<>1 THEN RAISE_APPLICATION_ERROR(-20040,'Incorrect process distribution.'); END IF;"]
 for r in p['matrix_rows']:
  criterion='movement' if r in (33,34) else 'shortfall' if r in (37,38,39) else 'profit' if r==64 else 'fraud_override' if r==62 else 'instances'
  gravity=matrix[f'H{r}']['value'] if criterion in ('instances','fraud_override') else None
  checks += [f"SELECT COUNT(*) INTO n FROM T_FAD_RISK_SUBPROCESS WHERE version_id=v AND sub_code='RM{r}' AND process_code={q(p['id'])} AND criterion={q(criterion)} AND gravity " + ('IS NULL;' if gravity is None else '='+gravity+';'),
   "IF n<>1 THEN RAISE_APPLICATION_ERROR(-20040,'Incorrect sub-process mapping.'); END IF;"]
for a in cfg['annexures']:
 checks += [f"SELECT COUNT(*) INTO n FROM T_FAD_RISK_ANNEX WHERE version_id=v AND code={q(a['code'])} AND sub_code={q(a['sub_process_id'])} AND risk_level={q(a['risk_level'])} AND description={q(a['description'])};",
 "IF n<>1 THEN RAISE_APPLICATION_ERROR(-20040,'Incorrect Annexure mapping.'); END IF;"]
checks += ['END;','/']
(out.parent/'24_exact_configuration.sql').write_text('\n'.join(checks)+'\n',encoding='utf-8')
