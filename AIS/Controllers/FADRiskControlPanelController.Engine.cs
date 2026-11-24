using Microsoft.AspNetCore.Mvc;
namespace AIS.Controllers {
 public partial class FADRiskControlPanelController {
  [HttpGet] public IActionResult EngineCatalog() => RiskAction(user=>dBConnection.EngineRead(true,0,0,0,0),"Risk register filters are unavailable.");
  [HttpGet] public IActionResult EngineResults(int period,int type=0,int entity=0,int result=0) {
   if(period<=0 || type<0 || entity<0 || result<0) ModelState.AddModelError("selection","Invalid selection.");
   return RiskAction(user=>dBConnection.EngineRead(false,period,type,entity,result),"Risk register results are unavailable.");
  }
  [HttpPost,ValidateAntiForgeryToken,FadRiskJsonFailure]
  public IActionResult RunEnginePeriod(int period,int type) {
   if(period<=0 || type<=0) ModelState.AddModelError("selection","Select period and entity type.");
   return RiskAction(user=>dBConnection.EngineRun(true,period,type),"Risk register execution could not be completed.", "Check the selected period and entity, approved execution policy, and shared engine prerequisites. Contact the administrator with the request reference.");
  }
  [HttpPost,ValidateAntiForgeryToken,FadRiskJsonFailure]
  public IActionResult RunEngineEntity(int period,int entity) {
   if(period<=0 || entity<=0) ModelState.AddModelError("selection","Select period and entity.");
   return RiskAction(user=>dBConnection.EngineRun(false,period,entity),"Risk register execution could not be completed.", "Check the selected period and entity, approved execution policy, and shared engine prerequisites. Contact the administrator with the request reference.");
  }
 }
}
