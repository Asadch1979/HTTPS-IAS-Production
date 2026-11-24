using Microsoft.AspNetCore.Mvc;
namespace AIS.Controllers {
 public partial class FADRiskControlPanelController {
  [HttpGet] public IActionResult EngineCatalog() => RiskAction(user=>dBConnection.EngineRead(true,0,0,0,0),"Risk register filters are unavailable.");
  [HttpGet] public IActionResult EngineResults(int period,int type=0,int entity=0,int result=0) {
   if(period<=0 || type<0 || entity<0 || result<0) ModelState.AddModelError("selection","Invalid selection.");
   return RiskAction(user=>dBConnection.EngineRead(false,period,type,entity,result),"Risk register results are unavailable.");
  }
  [HttpPost,ValidateAntiForgeryToken,FadRiskJsonFailure]
  public IActionResult RunEnginePeriod(int period) {
   if(period<=0) ModelState.AddModelError("selection","Select a valid audit period.");
   return RiskAction(user=>dBConnection.EngineRun(false,period),
       "Risk register calculation could not be completed.",
       "Verify the selected audit period, published Excel scoring version and IAS Annexure mapping.");
  }
  [HttpPost,ValidateAntiForgeryToken,FadRiskJsonFailure]
  public IActionResult RunEngineEntity(int period,int entity) {
   if(period<=0 || entity<=0) ModelState.AddModelError("selection","Select period and entity.");
   return RiskAction(user=>dBConnection.EngineRun(true,period,entity),"Risk register calculation could not be completed.", "Verify the selected audit period, branch and published scoring version.");
  }
 }
}
