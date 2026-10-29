using System.Globalization;
using System.Reflection;
using AIS.Filters;
using AIS.Models;
using AIS.Models.FieldAuditReport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException(description);
    Console.WriteLine("PASS: " + description);
}

var filter = new PostModelValidationFilter(NullLogger<PostModelValidationFilter>.Instance);
ActionExecutingContext Context(string method, string action = nameof(FormController.Process))
{
    var http = new DefaultHttpContext();
    http.Request.Method = method;
    http.Request.Path = "/FieldAuditReport/SaveNarrativePara";
    var descriptor = new ControllerActionDescriptor { ActionName = action,
        MethodInfo = typeof(FormController).GetMethod(action), ControllerTypeInfo = typeof(FormController).GetTypeInfo() };
    return new ActionExecutingContext(new ActionContext(http, new RouteData(), descriptor),
        new List<IFilterMetadata>(), new Dictionary<string, object>(), new FormController());
}

foreach (var ajax in new[] { false, true })
{
    var context = Context("POST");
    if (ajax) context.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
    context.ModelState.AddModelError("ParaId", "Invalid para selection.");
    filter.OnActionExecuting(context);
    var result = context.Result as BadRequestObjectResult;
    var payload = result?.Value as ValidationErrorResponse;
    Check(result?.StatusCode == 400 && payload?.Status == false && payload.Errors.ContainsKey("ParaId"),
        "Invalid POST returns structured 400 with or without AJAX header: " + ajax);
    Check(!context.ModelState.IsValid, "Rejected validation errors remain intact");
}
var form = Context("POST", nameof(FormController.Form));
form.ModelState.AddModelError("Fields", "Invalid fields.");
filter.OnActionExecuting(form);
Check(form.Result == null && !form.ModelState.IsValid, "Explicit form handler receives invalid ModelState without an inferred view");
var valid = Context("POST");
filter.OnActionExecuting(valid);
Check(valid.Result == null, "Valid POST continues");
var get = Context("GET");
get.ModelState.AddModelError("x", "Invalid");
filter.OnActionExecuting(get);
Check(get.Result == null, "GET behavior is unchanged");

var builder = WebApplication.CreateBuilder(Array.Empty<string>());
builder.Services.AddControllersWithViews();
await using var app = builder.Build();
async Task<(bool Valid, T Model, ModelStateDictionary State)> Bind<T>(Dictionary<string, StringValues> fields) where T : class, new()
{
    using var scope = app.Services.CreateScope();
    var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
    http.Request.Method = "POST";
    var controller = new FormController { ControllerContext = new ControllerContext(new ActionContext(http,
        new RouteData(), new ControllerActionDescriptor())) };
    var model = new T();
    var result = await controller.TryUpdateModelAsync(model, "", new FormValueProvider(BindingSource.Form,
        new FormCollection(fields), CultureInfo.InvariantCulture));
    return (result, model, controller.ModelState);
}
var draft = await Bind<FieldAuditParaNarrativeSaveRequest>(new() {
    ["ParaId"] = "123", ["Action"] = "SAVE", ["Implications"] = "",
    ["Recommendations"] = "", ["ManagementComments"] = "", ["AuditorFurtherComments"] = "", ["SvpRemarks"] = "" });
Check(draft.Valid && draft.Model.ParaId == 123 && draft.Model.ParaNo == null, "Incomplete draft text and unsubmitted optional para number bind without invented requirements");
var badId = await Bind<FieldAuditParaNarrativeSaveRequest>(new() { ["ParaId"] = "not-an-id", ["Action"] = "SAVE" });
Check(!badId.Valid && badId.State.ContainsKey("ParaId"), "Malformed numeric para ID remains a genuine binding error");
var badNumber = await Bind<FieldAuditParaNarrativeSaveRequest>(new() { ["ParaId"] = "123", ["ParaNo"] = "bad" });
Check(!badNumber.Valid, "Malformed optional numeric value is rejected when supplied");
var section = await Bind<FieldAuditInputSaveRequest>(new() { ["SectionCode"] = "KPI_SNAPSHOT" });
Check(section.Valid && section.Model.Fields.Count == 0, "Section-only forms do not require unrelated Fields");
var values = await Bind<FieldAuditInputSaveRequest>(new() { ["Fields[FIELD_029]"] = "Submitted text", ["Fields[FIELD_030]"] = "" });
Check(values.Valid && values.Model.Fields["FIELD_029"] == "Submitted text" && values.Model.Fields.ContainsKey("FIELD_030"), "Dictionary binding retains submitted field values and allows blanks");
Console.WriteLine("All validation regression checks passed.");

public class FormController : Controller
{
    public IActionResult Process() => Ok();
    [HandlesInvalidPostModel]
    public IActionResult Form() => ModelState.IsValid ? Ok() : BadRequest();
}
