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
    Check(action.GetParameters().All(p => p.Name is "engId" or "section" or "evidenceId" or "reportId" or "auditPeriodId" or "officeId" or "statusId" or "exception"),
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

Check(ManagementDashboardPeriod.CompletedDays(new DateTime(2027,1,1)) == null, "Jan 1 must not expand into last year.");
var leap = ManagementDashboardPeriod.CompletedDays(new DateTime(2028,3,1)).Value;
Check(leap.Start == new DateTime(2028,1,1) && leap.End == new DateTime(2028,2,29), "Pakistan completed-day window handles leap day.");
var managementDates = new ExecutiveDashboardFilterRequest { StartDate="2024-01-01",EndDate="2099-01-01" };
ManagementDashboardPeriod.Apply(managementDates,"GROUP");
Check(managementDates.StartDate == new DateTime(AuditParaSnapPolicy.Today.Year,1,1).ToString("yyyy-MM-dd") &&
    managementDates.EndDate == AuditParaSnapPolicy.Today.AddDays(-1).ToString("yyyy-MM-dd"), "Browser dates cannot expand management period.");
var portfolio = new HeadAuditDashboard { Offices = new() { new() { Planned=3,CompletedPlans=1,Completed=2 } },
    Engagements = new() { new() { EngagementId=1,StatusId=14,DaysToEnd=-3 }, new() { EngagementId=2,StatusId=13 }, new() { EngagementId=3,StatusId=4,DaysToEnd=-1 } } };
Check(portfolio.Total.Achievement == 33.3m, "Plan achievement counts completed plan entries, not engagements.");
Check(!portfolio.Engagements[0].IsDelayed && !portfolio.Engagements[1].IsDelayed && portfolio.Engagements[1].HasPendingReport,
    "Conclusion and missing deadlines must not become delays; report issuance is pending.");
portfolio.Exception="delayed";
Check(portfolio.Register.Single().EngagementId == 3, "Delayed filter uses recorded unfinished engagements.");
var narrative = AIS.Services.HeadAuditReviewText.Plain("<p>First &amp; second</p><script>alert('x')</script><p>Next</p>");
Check(narrative.Contains("First & second") && narrative.Contains("Next") && !narrative.Contains("alert") && !narrative.Contains("<p>"), "Saved rich text remains readable without active content.");
Console.WriteLine("PASS: management window, forged dates, leap/January boundaries, plan reconciliation, missing deadlines and safe review narratives.");
