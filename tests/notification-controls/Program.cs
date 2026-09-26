using AIS;
using AIS.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

void Check(bool condition, string label) { if (!condition) throw new Exception(label); Console.WriteLine("PASS: " + label); }
string FindRepoRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory != null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "AIS", "AIS.csproj")))
            return directory.FullName;
        directory = directory.Parent;
    }
    throw new DirectoryNotFoundException("Repository root not found.");
}

var reviewScript = File.ReadAllText(Path.Combine(FindRepoRoot(), "AIS", "wwwroot", "js", "csp", "Views_PostCompliance_post_compliance_review.js"));
Check(reviewScript.Contains("if (g_reviewPending) return;") && reviewScript.Contains("reviewButtons.prop('disabled', true)") &&
      reviewScript.Contains("if (data && data.Status)"), "Repeated-click UI protection is present");

var sqlRoot = Path.Combine(FindRepoRoot(), "AIS", "Docs", "sql");
var notificationSql = Directory.GetFiles(sqlRoot, "*.sql", SearchOption.AllDirectories)
    .Select(File.ReadAllText).ToArray();
var allNotificationSql = string.Join("\n", notificationSql);
Check(!Regex.IsMatch(allNotificationSql, @"LISTAGG\s*\(\s*DISTINCT", RegexOptions.IgnoreCase),
    "Oracle 18c SQL contains no LISTAGG(DISTINCT ...)");
Check(!Regex.IsMatch(allNotificationSql,
        @"(?:UPSERT_JOB|UPSERT_JOB_DISABLED)\s*\(\s*'JOB_MGMT_AUDIT_WEEKLY_NOTIFY'|DBMS_SCHEDULER\.ENABLE\s*\(\s*'JOB_MGMT_AUDIT_WEEKLY_NOTIFY'",
        RegexOptions.IgnoreCase),
    "No SQL creates or enables the retired weekly Oracle job");

var cutoverSql = File.ReadAllText(Path.Combine(sqlRoot, "management_audit_application_scheduler.sql"));
Check(!Regex.IsMatch(cutoverSql,
        @"^\s*(?:SET\s+(?:DEFINE|SERVEROUTPUT|SQLBLANKLINES|PAGESIZE|LINESIZE|FEEDBACK|VERIFY)|WHENEVER|PROMPT|SHOW\s+ERRORS|@@)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline),
    "Weekly database cutover is plain Oracle SQL/PLSQL");
Check(!cutoverSql.Contains("DBMS_METADATA", StringComparison.OrdinalIgnoreCase) &&
      !cutoverSql.Contains("GET_DDL", StringComparison.OrdinalIgnoreCase),
    "Weekly database cutover does not modify package source dynamically");
var databaseStep = cutoverSql.IndexOf("Run this database script", StringComparison.Ordinal);
var applicationStep = cutoverSql.IndexOf("Deploy the ASP.NET application", StringComparison.Ordinal);
var enableStep = cutoverSql.IndexOf("ManagementAuditWeekly:Enabled=true", StringComparison.Ordinal);
Check(databaseStep >= 0 && databaseStep < applicationStep && applicationStep < enableStep,
    "Deployment order is database, application, then weekly enablement");

var packageSql = File.ReadAllText(Path.Combine(sqlRoot, "ias_notification_centralization.sql"));
Check(packageSql.Contains("CREATE OR REPLACE PACKAGE PKG_IAS_NOTIFICATION AS") &&
      packageSql.Contains("CREATE OR REPLACE PACKAGE BODY PKG_IAS_NOTIFICATION AS") &&
      packageSql.Contains("Weekly notification delivery moved to ASP.NET ManagementAuditWeeklyService.") &&
      packageSql.Contains("Management Audit weekly ASP.NET execution status") &&
      packageSql.Contains("FROM T_IAS_NOTIFY_EXECUTION"),
    "Controlled complete package retires Oracle weekly delivery and reports ASP.NET execution");

var managementAuditSql = Directory.GetFiles(sqlRoot, "management_audit*.sql", SearchOption.TopDirectoryOnly)
    .Concat(Directory.GetFiles(Path.Combine(sqlRoot, "management_audit_notification_steps"), "*.sql"))
    .Select(File.ReadAllText);
Check(managementAuditSql.All(sql =>
        !sql.Contains("T_AU_IID_EMAIL_QUEUE", StringComparison.OrdinalIgnoreCase) &&
        !sql.Contains("PKG_INQ.P_ENQUEUE_EMAIL", StringComparison.OrdinalIgnoreCase) &&
        !sql.Contains("PKG_INQ.P_GET_EMAIL_QUEUE", StringComparison.OrdinalIgnoreCase) &&
        !sql.Contains("UQ_IID_EMAIL_SCHED_NOTIFY", StringComparison.OrdinalIgnoreCase)),
    "Management Audit deployment SQL contains no legacy Inquiry email queue references");
Check(Directory.GetFiles(Path.Combine(sqlRoot, "management_audit_notification_steps"), "*.sql")
        .All(path => File.ReadAllText(path).Contains("RAISE_APPLICATION_ERROR(-20998")),
    "Legacy staged Management Audit SQL is fail-fast retired");
Check(File.Exists(Path.Combine(FindRepoRoot(), "tests", "notification-controls", "repeated-click.cjs")),
    "Repeated-click executable regression test is retained");

var genericQueueSql = File.ReadAllText(Path.Combine(sqlRoot, "email_notification_architecture_refactor.sql"));
Check(genericQueueSql.Contains("T_EMAIL_QUEUE") && genericQueueSql.Contains("UQ_EMAIL_SCHED_NOTIFY") &&
      !genericQueueSql.Contains("MGMT_AUDIT_WEEKLY_PARA_STATUS"),
    "Generic queue foundation excludes application-owned weekly delivery");
var inquirySnapshots = new[] { "PKG_INQ.sql", "IID_module_PKG_INQ_full.sql" }
    .Select(name => File.ReadAllText(Path.Combine(sqlRoot, name)));
Check(inquirySnapshots.All(sql => !sql.Contains("T_AU_IID_EMAIL_QUEUE", StringComparison.OrdinalIgnoreCase)),
    "Inquiry package snapshots cannot restore the retired email queue");

var monday = new DateTime(2026, 9, 28);
Check(ManagementAuditWeeklyService.ReportingStart(monday.AddHours(6), DayOfWeek.Monday, TimeSpan.FromHours(7)) == null, "Not due before Monday 07:00");
Check(ManagementAuditWeeklyService.ReportingStart(monday.AddHours(7), DayOfWeek.Monday, TimeSpan.FromHours(7)) == monday.AddDays(-7), "Previous Monday-Sunday selected");
Check(ManagementAuditWeeklyService.ReportingStart(monday.AddDays(2), DayOfWeek.Monday, TimeSpan.FromHours(7)) == monday.AddDays(-7), "Restart catches up within reporting week");
Check(ManagementAuditWeeklyService.ReportingStart(monday.AddDays(4).AddHours(9), DayOfWeek.Friday, TimeSpan.FromHours(8)) == monday.AddDays(-7), "Configurable schedule");
var actionRuns = 0;
var completed = new Dictionary<string, NotificationExecutionStore.ExecutionRecord>();
var first = NotificationExecutionStore.ExecuteReviewCore("REVIEW:test", "hash1", () => { actionRuns++; return "{\"Status\":true,\"Message\":\"Rejected/Referred Back\"}"; },
    (key, hash) => !completed.ContainsKey(key),
    (key, status, response) => completed[key] = new NotificationExecutionStore.ExecutionRecord("hash1", status, response),
    key => completed.TryGetValue(key, out var record) ? record : null);
Check(JsonDocument.Parse(first).RootElement.GetProperty("Status").GetBoolean() && actionRuns == 1, "Single rejection executes once");
var duplicate = NotificationExecutionStore.ExecuteReviewCore("REVIEW:test", "hash1", () => { actionRuns++; return "{}"; },
    (key, hash) => false,
    (_, _, _) => throw new Exception("Duplicate should not complete again"),
    key => completed.TryGetValue(key, out var record) ? record : null);
Check(JsonDocument.Parse(duplicate).RootElement.GetProperty("Status").GetBoolean() && actionRuns == 1, "Server duplicate returns cached result without re-execution");
var conflicting = NotificationExecutionStore.ExecuteReviewCore("REVIEW:test", "hash2", () => { actionRuns++; return "{}"; },
    (key, hash) => false,
    (_, _, _) => throw new Exception("Conflicting duplicate should not complete"),
    key => completed.TryGetValue(key, out var record) ? record : null);
Check(!JsonDocument.Parse(conflicting).RootElement.GetProperty("Status").GetBoolean() && actionRuns == 1, "Server duplicate with changed payload is blocked");

var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
var messages = new List<string>();
var server = Task.Run(async () => {
    for (var i = 0; i < 2; i++)
    {
        using var client = await listener.AcceptTcpClientAsync();
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\r\n" };
        await writer.WriteLineAsync("220 localhost test");
        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("DATA"))
            {
                await writer.WriteLineAsync("354 continue");
                var body = new StringBuilder();
                while ((line = await reader.ReadLineAsync()) != "." && line != null) body.AppendLine(line);
                messages.Add(body.ToString());
                await writer.WriteLineAsync("250 accepted");
            }
            else if (line.StartsWith("QUIT")) { await writer.WriteLineAsync("221 bye"); break; }
            else await writer.WriteLineAsync("250 OK");
        }
    }
});
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string> {
    ["Email:From"]="ias@example.test",["Email:Password"]="test",["Email:Host"]="127.0.0.1",
    ["Email:Port"]=((IPEndPoint)listener.LocalEndpoint).Port.ToString(),["Email:EnableSsl"]="false"
}).Build();
Check(EmailNotification.NotifyManagementAuditParaStatus(config,"2","Rejected","2026","High","Test gist","Decision reason",
    "division@example.test","group@example.test","","Report","Division","Department"), "Rejection SMTP succeeds");
var records = new List<ManagementAuditDecision> {
    new(1,"Division","division@example.test","group@example.test","Entity","2026","2","Test",monday.AddDays(-6),monday.AddDays(-5),"",true),
    new(1,"Division","division@example.test","group@example.test","Entity","2026","3","Test",monday.AddDays(-6),monday.AddDays(-4),"Reason",false)
};
Check((await EmailNotification.SendManagementAuditWeeklyAsync(config,monday.AddDays(-7),records)).IsSuccess,"Weekly SMTP succeeds");
await server.WaitAsync(TimeSpan.FromSeconds(15));
listener.Stop();
Check(messages.Count==2,"One SMTP message per invocation");
Check(messages[1].Contains("SETTLED PARAS") && messages[1].Contains("REJECTED PARAS"),"Weekly includes both tables");
Check(messages[0].Contains("Rejected") && messages[0].Contains("Decision reason"),"Rejection content preserved");
try {
    await EmailNotification.SendManagementAuditWeeklyAsync(config,monday,records.Append(records[0] with { DivisionId=2 }).ToList());
    throw new Exception("Mixed divisions accepted");
} catch (InvalidOperationException) { Console.WriteLine("PASS: mixed divisions rejected"); }
