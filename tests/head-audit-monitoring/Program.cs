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
    Check(HeadAuditMonitoringPolicy.IsEligible(user) == new[] { 1, 5, 6, 7 }.Contains(role), $"Unexpected eligible role {role}.");
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
