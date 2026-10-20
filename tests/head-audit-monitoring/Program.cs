using AIS.Controllers;
using AIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

static void Check(bool valid, string message)
{
    if (!valid) throw new InvalidOperationException(message);
}
Check(!HeadAuditMonitoringPolicy.IsEligible(null), "Missing session must be denied.");
foreach (var role in Enumerable.Range(0, 49))
{
    var user = new SessionUser { UserRoleID = role, UserEntityID = 112243 };
    Check(HeadAuditMonitoringPolicy.IsEligible(user) == (role > 0), $"Unexpected eligible role {role}.");
    foreach (int? entity in new int?[] { null, 0, -1 })
    {
        user.UserEntityID = entity;
        Check(!HeadAuditMonitoringPolicy.IsEligible(user), "Department identity is required.");
    }
}
var controller = typeof(HeadAuditDepartmentController);
Check(controller.GetCustomAttribute<AuthorizeAttribute>() != null, "Controller must require authentication.");
Check(controller.GetCustomAttribute<ResponseCacheAttribute>()?.NoStore == true, "Audit work must not be cached.");
var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
Check(actions.Length == 7, "Review the protected GET surface when adding actions.");
foreach (var action in actions)
{
    Check(action.GetCustomAttribute<HttpGetAttribute>() != null, $"Unexpected non-GET action {action.Name}.");
    Check(action.GetParameters().All(p => p.Name is "engId" or "section" or "evidenceId" or "reportId"),
        $"Browser-supplied authority on {action.Name}.");
}
Console.WriteLine("PASS: role/entity eligibility, authenticated GET-only surface, no-store, and no browser-supplied authority parameters.");

Check(!IASParaPositionPolicy.IsValid(new DateTime(2023,12,31), new DateTime(2024,1,1)), "Pre-2024 date accepted.");
Check(IASParaPositionPolicy.IsValid(new DateTime(2024,1,1), new DateTime(2024,1,1)), "Minimum date rejected.");
Check(!IASParaPositionPolicy.IsValid(new DateTime(2024,1,2), new DateTime(2024,1,1)), "Reversed range accepted.");
Check(!IASParaPositionPolicy.IsValid(new DateTime(2024,1,1), IASParaPositionPolicy.Today.AddDays(1)), "Future date accepted.");
Check(IASParaPositionPolicy.StartFor(new DateTime(2027,1,1)) == new DateTime(2026,1,1), "January 1 default reversed.");
Check(IASParaPositionPolicy.StartFor(new DateTime(2027,1,2)) == new DateTime(2027,1,1), "January 2 default incorrect.");
var dates = new ExecutiveDashboardFilterRequest { StartDate="2024-01-01", EndDate="2024-02-29", View="risk", SnapDivision="1" };
AuditParaSnapPolicy.Validate(dates);
dates.SnapDepartmentId=112243;
try { AuditParaSnapPolicy.Validate(dates); throw new Exception("All Divisions accepted a department."); }
catch (ArgumentException) { }
dates.SnapDepartmentId=null; dates.SnapDivision="OTHER";
try { AuditParaSnapPolicy.Validate(dates); throw new Exception("Unknown division accepted."); }
catch (ArgumentException) { }
dates.SnapDivision=null;
ComplianceDashboardPolicy.Validate(dates);
dates.EndDate=AuditParaSnapPolicy.Today.AddDays(1).ToString("yyyy-MM-dd");
try { ComplianceDashboardPolicy.Validate(dates); throw new Exception("Compliance accepted future dates."); }
catch (ArgumentException) { }
Console.WriteLine("PASS: minimum/maximum dates, reversed periods, January boundary, All Divisions and CCO validation.");
