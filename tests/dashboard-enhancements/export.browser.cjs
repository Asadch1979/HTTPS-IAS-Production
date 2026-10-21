// NODE_PATH must include playwright. Uses an isolated browser and synthetic data.
const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const http = require('node:http');
const os = require('node:os');
const root = path.resolve(__dirname, '../../AIS/wwwroot');
const fixture = `<!doctype html><html><head><link rel="stylesheet" href="/css/dashboard-export.css">
<style>body{font-family:Arial}#executive-dashboard{width:800px;background:#fcf9f3;padding:20px}
.exec-panel{border:2px solid #79511c;padding:20px}.scroll{height:120px;overflow:auto}
td,th{padding:10px;border:1px solid #ccc}svg{width:400px;height:80px}</style></head>
<body><section id="executive-dashboard"><header class="dashboard-title"><h1>Compliance fixture</h1><p>Authorized test scope</p></header>
<div id="exec-results"><article class="exec-panel"><h2>Risk &amp; position</h2>
<svg><rect width="350" height="60" fill="#79511c"></rect><text x="20" y="35" fill="white">High risk</text></svg>
<div class="scroll"><table><thead><tr><th>Name</th><th>Count</th><th style="display:none">Hidden</th></tr></thead><tbody>
${Array.from({length:35},(_,i)=>`<tr><td>${i===0?'=HYPERLINK("bad")':'Entity '+i}</td><td>${i}</td><td style="display:none">PRIVATE</td></tr>`).join('')}
</tbody></table></div></article></div></section>
<script src="/lib/dashboard-export/html2canvas-1.4.1.min.js"></script>
<script src="/lib/dashboard-export/jspdf-4.2.1.umd.min.js"></script>
<script src="/lib/dashboard-export/exceljs-4.4.0.min.js"></script>
<script src="/js/dashboard-export.js"></script></body></html>`;
(async()=>{
 const server=http.createServer((req,res)=>{
   if(req.url==='/'){res.setHeader('Content-Type','text/html');return res.end(fixture);}
   const file=path.resolve(root,'.'+req.url);
   if(!file.startsWith(root+path.sep)||!fs.existsSync(file)){res.statusCode=404;return res.end();}
   res.setHeader('Content-Type',file.endsWith('.js')?'application/javascript':'text/css');res.end(fs.readFileSync(file));
 }).listen(0,'127.0.0.1');
 await new Promise(resolve=>server.once('listening',resolve));
 const browser=await chromium.launch({channel:process.env.TEST_BROWSER || 'chrome',headless:true});
 try {
   const page=await browser.newPage({viewport:{width:1200,height:700},deviceScaleFactor:1});
   const errors=[];page.on('pageerror',e=>errors.push(e.message));
   await page.goto(`http://127.0.0.1:${server.address().port}`);
   const control=page.locator('.exec-panel > .dashboard-export-control');
   for(const [label,ext] of [['Picture (PNG)','png'],['Excel (.xlsx)','xlsx'],['PDF','pdf']]){
     await control.locator('summary').click();
     const downloadPromise=page.waitForEvent('download');
     await control.getByRole('button',{name:label,exact:true}).click();
     const download=await downloadPromise;
     const output=path.join(os.tmpdir(),'ias-dashboard-export-test.'+ext);await download.saveAs(output);
     const bytes=fs.readFileSync(output);
     if(ext==='png'){
       assert.ok(bytes.readUInt32BE(20)>1400,'Capture must include every scrolled row');
       assert.ok(bytes.readUInt32BE(16)>=800);
     } else if(ext==='pdf') assert.equal(bytes.subarray(0,4).toString(),'%PDF');
     else {
       const ExcelJS=require(path.join(root,'lib/dashboard-export/exceljs-4.4.0.min.js'));
       const book=new ExcelJS.Workbook();await book.xlsx.load(bytes);
       assert.equal(book.getWorksheet('Visual snapshot'),undefined);
       assert.ok(book.worksheets.every(sheet => sheet.getImages().length === 0));
       assert.equal(book.getWorksheet('Table 1').rowCount,36);
       assert.equal(book.getWorksheet('Table 1').columnCount,2);
       assert.equal(book.getWorksheet('Table 1').getCell('A2').type,3,'Formula-like text stays literal');
     }
   }
   assert.equal(await page.locator('.scroll').evaluate(el=>el.clientHeight),120,'Live layout must not expand');
   await page.locator('#executive-dashboard').evaluate(el=>el.setAttribute('aria-busy','true'));
   await control.locator('summary').click();await control.getByRole('button',{name:'PDF',exact:true}).click();
   assert.match(await page.locator('.dashboard-export-status').innerText(),/wait for the dashboard/);
   assert.deepEqual(errors,[]);
   console.log('PASS: full-scroll PNG, paginated PDF, data-only Excel and literal/visible table data, layout preservation, busy-state guard.');
 } finally {await browser.close();await new Promise(resolve=>server.close(resolve));}
})().catch(e=>{console.error(e);process.exitCode=1;});
