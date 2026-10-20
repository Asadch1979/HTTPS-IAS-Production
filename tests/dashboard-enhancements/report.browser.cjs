const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs'), path = require('node:path'), http = require('node:http');
const root = path.resolve(__dirname, '../../AIS/wwwroot');
const totals = ['OpeningBalance','IssueIdentified','IssueRectified','ParaAdded','Combined','SettledAudit','SettledCompliance','Outstanding','HighRisk','MediumRisk','LowRisk'];
const fixture = `<html><body>
<select id="entityTypeSelectionField"><option value="-1">All</option><option value="2">Branches</option></select>
<input id="periodStartModalField" type="date" min="2024-01-01" max="2026-10-20" value="2026-01-01">
<input id="periodEndModalField" type="date" min="2024-01-01" max="2026-10-20" value="2026-10-19">
<button id="iasParaPositionSearch">Search</button><p id="iasParaPositionStatus"></p><div id="iasParaPositionError"></div>
<section id="iasParaPositionSummary" hidden><span id="iasParaSummaryPeriod"></span></section>
<div id="iasParaPositionTableContainer" hidden><table id="iasParaPositionGrid" data-show-reporting-office="true">
<thead><tr>${['Sr','Reporting Office','Audit Entity ID','Entity Name',...totals].map(s=>'<th>'+s+'</th>').join('')}</tr></thead><tbody></tbody>
<tfoot><tr><td colspan="4">Total</td>${totals.map(s=>'<td id="total'+s+'">0</td>').join('')}</tr></tfoot></table></div>
<script src="/lib/jquery/dist/jquery.min.js"></script><script src="/lib/datatable/datatables.min.js"></script>
<script src="/lib/select2/select2.min.js"></script>
<script>
var g_asiBaseURL='';var requests=0;
function destroyDatatable(id){if($.fn.DataTable.isDataTable('#'+id))$('#'+id).DataTable().destroy();}
function getPdfExportButtonConfig(){return {extend:'pdfHtml5'};}
function getExcelExportButtonConfig(){return {extend:'excelHtml5'};}
function getCsvExportButtonConfig(){return {extend:'csvHtml5'};}
$.ajax=function(options){requests++;window.lastRequest=options;return {abort(){}};};
</script><script src="/js/csp/Views_Reports_IAS_PARA_POSITION.js"></script></body></html>`;
(async()=>{
 const server=http.createServer((req,res)=>{
   if(req.url==='/'){res.setHeader('Content-Type','text/html');return res.end(fixture);}
   const file=path.resolve(root,'.'+req.url);
   if(!file.startsWith(root+path.sep)||!fs.existsSync(file)){res.statusCode=404;return res.end();}
   res.setHeader('Content-Type','application/javascript');res.end(fs.readFileSync(file));
 }).listen(0,'127.0.0.1');
 await new Promise(resolve=>server.once('listening',resolve));
 const browser=await chromium.launch({channel:process.env.TEST_BROWSER || 'chrome',headless:true});
 try{
   const page=await browser.newPage();const errors=[];page.on('pageerror',e=>errors.push(e.message));
   await page.goto(`http://127.0.0.1:${server.address().port}`);
   await page.waitForFunction(()=>typeof entityTypeSelectionChangeEvent==='function');
   const load=()=>page.evaluate(()=>{
     entityTypeSelectionChangeEvent();
     lastRequest.success([{reportinG_OFFICE:'Office',chilD_CODE:'2',placE_OF_POSTING:'Branches',
       openinG_BALANCE:10,issuE_IDENTIFIED:8,issuE_RECTIFIED:3,parA_ADDED:5,settleD_AUDIT:2,settleD_COM:1,outstanding:12,r1:4,r2:5,r3:3}]);
     lastRequest.complete();
     const table=$('#iasParaPositionGrid').DataTable();
     return {visible:table.columns(':visible').count(), exported:table.buttons.exportData({columns:':visible'}),
       footer:table.column(0).footer().colSpan,hidden:document.getElementById('iasParaPositionTableContainer').hidden};
   });
   let result=await load();
   assert.equal(result.visible,13);assert.equal(result.hidden,false);assert.equal(result.footer,2);
   assert.equal(result.exported.header.includes('Reporting Office'),false);
   assert.equal(result.exported.header.includes('Audit Entity ID'),false);
   assert.deepEqual(result.exported.body[0].slice(2,7),['10','8','3','5','15']);
   await page.selectOption('#entityTypeSelectionField','2');result=await load();
   assert.equal(result.visible,15);assert.equal(result.footer,4);
   assert.equal(result.exported.header.includes('Reporting Office'),true);
   const before=await page.evaluate(()=>requests);
   await page.fill('#periodStartModalField','2023-12-31');await page.evaluate(()=>entityTypeSelectionChangeEvent());
   assert.equal(await page.evaluate(()=>requests),before);
   await page.fill('#periodStartModalField','2026-01-01');await page.fill('#periodEndModalField','2026-10-21');
   await page.evaluate(()=>entityTypeSelectionChangeEvent());assert.equal(await page.evaluate(()=>requests),before);
   await page.fill('#periodEndModalField','2024-01-01');
   await page.evaluate(()=>entityTypeSelectionChangeEvent());assert.equal(await page.evaluate(()=>requests),before);
   assert.deepEqual(errors,[]);
   console.log('PASS: All/specific table restoration, visible-column exports, footer colspan, lifecycle column order and invalid-date request rejection.');
 }finally{await browser.close();await new Promise(resolve=>server.close(resolve));}
})().catch(e=>{console.error(e);process.exitCode=1;});
