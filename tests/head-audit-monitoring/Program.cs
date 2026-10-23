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
    Check(action.GetParameters().All(p => p.Name is "engId" or "section" or "evidenceId" or "reportId" or "auditPeriodId" or "officeId" or "statusId" or "exception" or "positionAsOn" or "tab" or "checkOnly"),
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

var today = new DateTime(2026,10,23);
var auditPeriod = new AuditPeriodModel { AUDITPERIODID=77, START_DATE=new DateTime(2026,1,1), END_DATE=new DateTime(2026,12,31) };
Check(HeadAuditDashboard.ReportingDate(auditPeriod,null,today)==today.AddDays(-1), "Current period must default to Pakistan yesterday.");
Check(HeadAuditDashboard.ReportingDate(auditPeriod,new DateTime(2026,5,2),today)==new DateTime(2026,5,2), "Explicit cutoff must survive navigation.");
foreach (var invalid in new[] { new DateTime(2025,12,31),today,new DateTime(2026,10,24),new DateTime(2027,1,1) }) {
    var rejected=false; try { HeadAuditDashboard.ReportingDate(auditPeriod,invalid,today); } catch(ArgumentException) { rejected=true; }
    Check(rejected,"Invalid period/future cutoff accepted.");
}
auditPeriod.END_DATE=new DateTime(2026,6,30);
Check(HeadAuditDashboard.ReportingDate(auditPeriod,null,today)==auditPeriod.END_DATE,"Historical period must default to its end.");
var planRows = new List<HeadAuditPlanDetail> {
    new() { PlanId=1,Days=10,EngagementId=101 },new() { PlanId=1,Days=10,EngagementId=102 },new() { PlanId=2,Days=7 }
};
var planTotals=AIS.Services.HeadAuditPlanTotals.From(planRows);
Check(planTotals.Plans==2 && planTotals.Days==17 && planTotals.Engagements==2,"Multi-engagement plan double counted.");
planRows[1].Days=11;
Check(AIS.Services.HeadAuditPlanTotals.From(planRows).Days==null,"Conflicting saved days must not invent totals.");
planRows[1].Days=null;
Check(AIS.Services.HeadAuditPlanTotals.From(planRows).Days==null,"Missing days must not become zero.");
Check(AIS.Services.HeadAuditPlanTotals.From(Array.Empty<HeadAuditPlanDetail>()).Days==0,"Empty cohort total should be zero.");
var formatted=AIS.Services.ParaDisplayHtml.Sanitize("<h2>Finding</h2><p>First <b>bold</b><br>line</p><ul><li>Item</li></ul><table><tr><td colspan='2'>Cell</td></tr></table>");
Check(formatted.Contains("<h2>") && formatted.Contains("<b>") && formatted.Contains("<li>") && formatted.Contains("<table>") && formatted.Contains("colspan=\"2\""),"Document formatting lost.");
var plain=AIS.Services.ParaDisplayHtml.Sanitize("First & second\nNext < 5");
Check(plain.Contains("&amp;") && plain.Contains("<br>") && plain.Contains("&lt; 5"),"Plain text/line breaks lost.");
foreach(var attack in new[] {
    "<p onclick='attack()'>safe<script>attack()</script></p><img src=x onerror='attack()'>",
    "<svg><a href='javascript:attack()'>active</a></svg><iframe srcdoc='<script>attack()</script>'></iframe><p>safe</p>",
    "<a href='javascript:attack()'>safe</a><form action='https://evil'><input autofocus onfocus='attack()'></form>",
    "<style>body{display:none}</style><p style='position:fixed;background:url(javascript:attack());font-weight:bold' id='clobber'>safe</p>",
    "&lt;script&gt;attack()&lt;/script&gt;&lt;p onclick='attack()'&gt;safe&lt;/p&gt;",
    "<p><b>safe<table><tr><td onmouseover='attack()'>safe</p><script>attack()</script>"
}) {
    var safe=AIS.Services.ParaDisplayHtml.Sanitize(attack);
    var doc=new AngleSharp.Html.Parser.HtmlParser().ParseDocument(safe);
    Check(doc.QuerySelectorAll("script,svg,math,iframe,form,img,input,style,object,embed,a").Length==0,"Unsafe tag survived sanitizer.");
    Check(!safe.Contains("attack()") && !safe.Contains("clobber") && !safe.Contains("javascript:"),"Executable content survived sanitizer.");
    Check(safe.Contains("safe"),"Safe narrative text lost.");
}
var longText=AIS.Services.ParaDisplayHtml.Sanitize(string.Concat(Enumerable.Repeat("<p>Long <strong>observation</strong></p>",10000)));
Check(longText.Split("<strong>").Length==10001,"Long para was truncated.");
var oversized=AIS.Services.ParaDisplayHtml.Sanitize("<table><tr><td colspan='99999999'>safe</td></tr></table>");
Check(!oversized.Contains("99999999"),"Unbounded table spans retained.");
Console.WriteLine("PASS: reporting boundaries/defaults, distinct plan totals, formatted/plain/malformed/encoded/long para and stored active-content attacks.");
HeadAuditAccessChecks.Run();

foreach (var start in new[] { today, today.AddDays(1) }) {
    var empty = new AuditPeriodModel { START_DATE=start, END_DATE=start.AddDays(30) };
    try { HeadAuditDashboard.ReportingDate(empty,null,today); throw new Exception("No-completed-day period accepted"); }
    catch (HeadAuditReportingDateException ex) { Check(ex.Message.Contains("no completed eligible day"), "Clear empty-period message required"); }
}
Check(HeadAuditDashboard.ReportingDate(new AuditPeriodModel { START_DATE=new DateTime(2028,1,1), END_DATE=new DateTime(2028,12,31) },null,new DateTime(2028,3,1))==new DateTime(2028,2,29),"Leap completed day");
