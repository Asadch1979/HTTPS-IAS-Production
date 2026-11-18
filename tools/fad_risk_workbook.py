"""Read-only, reproducible extraction of the risk workbook (stdlib only)."""
import hashlib, json, pathlib, re, zipfile, xml.etree.ElementTree as ET
ROOT = pathlib.Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'AIS/Docs/Risk Register/Risk Rating Master_With Zeo Weightage.xlsx'
NS = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
def extract():
    with zipfile.ZipFile(SOURCE) as z:
        strings = [''.join(n.itertext()) for n in ET.fromstring(z.read('xl/sharedStrings.xml')).findall('s:si', NS)]
        rels = {r.attrib['Id']: r.attrib['Target'] for r in ET.fromstring(z.read('xl/_rels/workbook.xml.rels'))}
        sheets = {}
        for s in ET.fromstring(z.read('xl/workbook.xml')).find('s:sheets', NS):
            target = rels[s.attrib['{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id']]
            path = target.lstrip('/') if target.startswith('/') else 'xl/' + target
            cells = {}
            for c in ET.fromstring(z.read(path)).findall('.//s:sheetData/s:row/s:c', NS):
                v, f = c.find('s:v', NS), c.find('s:f', NS)
                if v is None and f is None: continue
                value = v.text if v is not None else None
                if c.get('t') == 's': value = strings[int(value)]
                cells[c.attrib['r']] = {'value': value, **({'formula': f.text} if f is not None else {})}
            sheets[s.attrib['name']] = cells
        return {'sha256': hashlib.sha256(SOURCE.read_bytes()).hexdigest(), 'sheets': sheets}
def configuration(source):
    sheets = source['sheets']
    matrix, annex, final = (sheets[k] for k in ('Rating Matrix', 'Annexure', 'Final Rating'))
    def val(sheet, cell): return sheet.get(cell, {}).get('value')
    # Explicit reviewable alias: matrix C45 prints B026; Annexure C58 says BO26.
    aliases = {'B026': 'BO26'}
    code_rows = {}
    for row in list(range(11, 24)) + list(range(26, 31)) + list(range(42, 61)) + [62]:
        if val(matrix, f'H{row}') is None: continue
        for code in re.findall(r'\b(?:[ADI]-\d+|[BCG]O\d+|B026)\b', val(matrix, f'C{row}') or val(matrix, f'B{row}') or ''):
            code = aliases.get(code, code)
            if code in code_rows: raise ValueError('Duplicate matrix code: ' + code)
            code_rows[code] = row
    code_rows.update(BO9=37, BO15=38, CO4=34)
    # Final row, matrix source rows, normalization total, main KPI, sub KPI.
    groups = [(12,range(11,15),22,'Portfolio','Lending'),(13,range(15,18),22,'Portfolio','Lending'),
      (14,range(18,20),22,'Portfolio','Lending'),(15,[20],22,'Portfolio','Lending'),(16,range(21,24),22,'Portfolio','Lending'),
      (20,[26],12,'Portfolio','Recovery'),(21,range(27,31),12,'Portfolio','Recovery'),
      (25,[33],20,'Achievement','Targets'),(26,[34],20,'Achievement','Targets'),(27,[37],20,'Achievement','Targets'),
      (28,[38],20,'Achievement','Targets'),(29,[39],20,'Achievement','Targets'),
      (35,range(42,45),36,'Operations','Security and cash'),(36,[45],36,'Operations','Security and cash'),
      (38,range(47,50),36,'Operations','Accounts'),(40,[51],36,'Operations','Cheque books'),
      (42,range(53,56),36,'Operations','Payments'),(44,range(57,61),36,'Operations','General controls'),
      (49,[64],10,'Profitability','Profitability'),(48,[62],0,'Fraud','Fraud')]
    processes, by_matrix = [], {}
    for fr, rows, denominator, main, sub in groups:
        p = {'id': f'FR{fr}', 'main_kpi':main, 'sub_kpi':sub, 'name':val(final,f'C{fr}') or val(final,f'B{fr}'),
             'weight':0 if fr == 48 else int(val(final,f'D{fr}')), 'normalization_total':denominator,
             'matrix_rows':list(rows),'source':f'Final Rating!D{fr}'}
        processes.append(p)
        for row in rows: by_matrix[row] = p
    records = []
    for row in range(2,123):
        code = val(annex,f'C{row}').strip()
        mr = code_rows[code]
        p = by_matrix[mr]
        records.append({'code':code,'description':val(annex,f'D{row}'), 'risk_level':val(annex,f'E{row}'),
          'workbook_process_number':int(val(annex,f'G{row}')), 'process_id':p['id'], 'sub_process_id':f'RM{mr}',
          'gravity':int(val(matrix,f'H{mr}')) if mr not in [34,37,38] else None,
          'criterion':'movement' if mr == 34 else 'shortfall' if mr in [37,38] else 'fraud_override' if mr == 62 else 'instances',
          'source':f'Annexure!C{row}:G{row}', 'matrix_source':f'Rating Matrix!{mr}'})
    assert len(records) == len({r['code'] for r in records}) == 121
    assert sum(p['weight'] for p in processes) == 100
    return {'version':'excel-formula-draft-1','status':'DRAFT_REQUIRES_REVIEW','source_sha256':source['sha256'],
            'aliases':aliases,'processes':processes,'annexures':records}

if __name__ == '__main__':
    out = ROOT / 'AIS/Docs/Risk Register/workbook-source.json'
    source = extract()
    out.write_text(json.dumps(source, indent=2, ensure_ascii=False), encoding='utf-8')
    out.with_name('risk-configuration.draft.json').write_text(json.dumps(configuration(source),indent=2,ensure_ascii=False),encoding='utf-8')
    print(out)
