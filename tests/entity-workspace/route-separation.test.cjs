const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs');
const read=p=>fs.readFileSync(p,'utf8');
const legacy=read('AIS/Controllers/AdministrationPanelController.cs').split('public IActionResult Entity_Dashboard')[1].split('[HttpGet]')[0];
const workspace=read('AIS/Controllers/AdministrationPanelController.EntityWorkspace.cs');
test('legacy action exclusively renders legacy view and has no preview or reader dispatch',()=>{
 assert.match(legacy,/Entity_Dashboard.cshtml/); assert.doesNotMatch(legacy,/workspacePreview|ReadEntity.*Data|Entity_Workspace.cshtml/);
 const view=read('AIS/Views/AdministrationPanel/Entity_Dashboard.cshtml');
 assert.match(view,/administration-entity-dashboard-legacy.js/);
 assert.doesNotMatch(view,/entity-management.css|entity-workspace-selection.js/);
});
test('workspace owns its route and requires explicit mapped grants',()=>{
 assert.match(workspace,/public IActionResult Entity_Workspace/);
 assert.match(workspace,/pageId <= 0/); assert.match(workspace,/allowedViewIds.Contains\(pageId\)/);
 assert.match(workspace,/IsSessionRevoked/);
 const view=read('AIS/Views/AdministrationPanel/Entity_Workspace.cshtml');
 assert.match(view,/data-read-url="@Url.Action\("Entity_Workspace"/);
 assert.doesNotMatch(view,/administration-entity-dashboard-legacy.js|workspacePreview/);
});
