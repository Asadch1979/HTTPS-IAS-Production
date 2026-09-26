using AIS;
using AIS.Models.Notifications;
using AIS.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

void Check(bool condition, string label) { if (!condition) throw new Exception(label); Console.WriteLine("PASS: " + label); }
bool Rejects(Action action)
{
    try { action(); return false; }
    catch (InvalidOperationException) { return true; }
}
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
var immediateHealthStart = packageSql.IndexOf("SELECT 'Attempts='", StringComparison.Ordinal);
var immediateHealthEnd = packageSql.IndexOf(';', immediateHealthStart);
var immediateHealthSql = packageSql.Substring(immediateHealthStart, immediateHealthEnd - immediateHealthStart);
Check(immediateHealthSql.Contains("ACTION='NotifyManagementAuditParaStatus'") &&
      immediateHealthSql.Contains("LOG_LEVEL") && immediateHealthSql.Contains("LOG_TIME") &&
      immediateHealthSql.Contains("MESSAGE") && !immediateHealthSql.Contains("ACTION_NAME") &&
      !immediateHealthSql.Contains("SENT_ON"),
    "Immediate Management Audit health query uses actual T_SYS_LOG columns");
Check(packageSql.IndexOf("CREATE OR REPLACE VIEW V_IAS_MGMT_AUDIT_NOTIFY_MAP", StringComparison.Ordinal) <
      packageSql.IndexOf("CREATE OR REPLACE VIEW V_IAS_MGMT_WEEKLY_DATA", StringComparison.Ordinal) &&
      !cutoverSql.Contains("CREATE OR REPLACE VIEW V_IAS_MGMT_WEEKLY_DATA"),
    "Management Audit mapping view is created before the dependent weekly data view");

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

var weeklyAccess = File.ReadAllText(Path.Combine(FindRepoRoot(), "AIS", "DBConnection.ManagementAuditWeekly.cs"));
Check(weeklyAccess.Contains("PKG_MGMT_AUDIT_WEEKLY.P_GET_MGMT_WEEKLY_DIVISIONS") &&
      weeklyAccess.Contains("PKG_MGMT_AUDIT_WEEKLY.P_GET_MGMT_WEEKLY_DIVISION_DATA") &&
      weeklyAccess.Contains("PKG_MGMT_AUDIT_WEEKLY.P_GET_MGMT_WEEKLY_NO_COMPLIANCE") &&
      Regex.Matches(weeklyAccess, "CommandType.StoredProcedure").Count == 3 &&
      Regex.Matches(weeklyAccess, "BindByName = true").Count == 3 &&
      Regex.Matches(weeklyAccess, "GuardAgainstDynamicSql\\(cmd\\)").Count == 3 &&
      Regex.Matches(weeklyAccess, "OracleDbType.RefCursor").Count == 3 &&
      !weeklyAccess.Contains("V_IAS_MGMT_WEEKLY_DATA", StringComparison.OrdinalIgnoreCase),
    "Management Audit weekly DB access uses all three approved stored procedures");
Check(weeklyAccess.Contains("NoComplianceCount = Convert.ToInt32(reader[\"NO_COMPLIANCE_COUNT\"])") &&
      typeof(ManagementAuditWeeklyDivisionSummaryModel).GetProperty("NoComplianceCount")?.PropertyType == typeof(int),
    "Management Audit weekly summary maps NO_COMPLIANCE_COUNT");
var noComplianceMethod = weeklyAccess.Substring(
    weeklyAccess.IndexOf("GetManagementAuditWeeklyNoCompliance", StringComparison.Ordinal));
Check(noComplianceMethod.Contains("CommandType.StoredProcedure") &&
      noComplianceMethod.Contains("BindByName = true") &&
      noComplianceMethod.Contains("GuardAgainstDynamicSql(cmd)") &&
      noComplianceMethod.Contains("OracleDbType.Date") &&
      noComplianceMethod.Contains("OracleDbType.RefCursor") &&
      !Regex.IsMatch(noComplianceMethod, "CommandText\\s*=\\s*@?\"\\s*SELECT", RegexOptions.IgnoreCase),
    "Management Audit no-compliance retrieval uses binds and a RefCursor without direct SQL");
var weeklyService = File.ReadAllText(Path.Combine(FindRepoRoot(), "AIS", "Services", "ManagementAuditWeeklyService.cs"));
var weeklyEmail = File.ReadAllText(Path.Combine(FindRepoRoot(), "AIS", "EmailNotification.cs"));
var summaryCall = weeklyService.IndexOf("GetManagementAuditWeeklyDivisions(fromDate, toDate)", StringComparison.Ordinal);
var detailCall = weeklyService.IndexOf("GetManagementAuditWeeklyDivisionData(divisionId, fromDate, toDate)", StringComparison.Ordinal);
Check(!weeklyService.Contains("V_IAS_MGMT_WEEKLY_DATA", StringComparison.OrdinalIgnoreCase) &&
      !weeklyService.Contains("OracleDbType") && !weeklyService.Contains("SELECT DIVISION_ID", StringComparison.OrdinalIgnoreCase),
    "Management Audit weekly service contains no direct weekly Oracle SQL");
Check(summaryCall >= 0 && detailCall > summaryCall && weeklyService.Contains("scopeFactory.CreateScope()") &&
      weeklyService.Contains("GetRequiredService<DBConnection>()"),
    "Management Audit weekly service resolves scoped DBConnection and loads summaries before details");
Check(weeklyService.Contains("store.ClaimWeekly") && weeklyService.Contains("WEEKLY:{fromDate:yyyyMMdd}:{division.DivisionId}"),
    "Management Audit weekly service retains per-Division execution claims");
Check(weeklyEmail.Contains("division.ToEmail") && weeklyEmail.Contains("division.CcEmail") &&
      weeklyEmail.Contains("division.DivisionName") &&
      !weeklyEmail.Contains("records.Select(r => r.Cc") &&
      typeof(ManagementAuditDecision).GetProperty("To") == null &&
      typeof(ManagementAuditDecision).GetProperty("Cc") == null,
    "Weekly recipients and Division name come only from the Division summary");
Check(weeklyEmail.Contains("Paras Settled on the Basis of Satisfactory Compliance") &&
      weeklyEmail.Contains("Paras Referred Back for Further Compliance") &&
      weeklyEmail.Contains("Paras Where No Compliance Was Submitted During the Reporting Period") &&
      new[] { "Sr.", "Department", "Audit Year", "Para No.", "Title of Para", "Risk", "Compliance Submitted On", "Settled On", "Reason for Referral Back", "Last Compliance Submitted On" }
        .All(column => weeklyEmail.Contains(column)),
    "Weekly email contains all approved section and table headings");
Check(!weeklyService.Contains("T_EMAIL_QUEUE", StringComparison.OrdinalIgnoreCase) &&
      !weeklyEmail.Contains("T_EMAIL_QUEUE", StringComparison.OrdinalIgnoreCase),
    "Weekly application delivery does not use T_EMAIL_QUEUE");
var sourceSettings = File.ReadAllText(Path.Combine(FindRepoRoot(), "AIS", "appsettings.json"));
Check(Regex.IsMatch(sourceSettings, "\\\"ManagementAuditWeekly\\\"\\s*:\\s*\\{[^}]*\\\"Enabled\\\"\\s*:\\s*false",
        RegexOptions.IgnoreCase | RegexOptions.Singleline),
    "Management Audit weekly delivery remains disabled by default");

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
var firstRetryCheck = new DateTimeOffset(2026, 9, 28, 2, 0, 0, TimeSpan.Zero);
var retryPeriod = monday.AddDays(-7);
Check(ManagementAuditWeeklyService.ShouldCheckPeriod(retryPeriod, null, null, firstRetryCheck) &&
      !ManagementAuditWeeklyService.ShouldCheckPeriod(retryPeriod, retryPeriod, firstRetryCheck, firstRetryCheck.AddMinutes(14)) &&
      ManagementAuditWeeklyService.ShouldCheckPeriod(retryPeriod, retryPeriod, firstRetryCheck, firstRetryCheck.AddMinutes(15)),
    "Failed weekly period becomes eligible for recheck after the retry interval");
var executionStoreSource = File.ReadAllText(Path.Combine(FindRepoRoot(), "AIS", "Services", "NotificationExecutionStore.cs"));
Check(executionStoreSource.Contains("STATUS='FAILED'") && executionStoreSource.Contains("RETRY_COUNT<5") &&
      executionStoreSource.Contains("INTERVAL '15' MINUTE") && !executionStoreSource.Contains("STATUS='RUNNING' AND RETRY_COUNT"),
    "Weekly retry claim permits only delayed FAILED executions with the existing retry limit");

ManagementAuditWeeklyDivisionSummaryModel WeeklySummary(int divisionId, int settled = 1, int rejected = 0) => new()
{
    DivisionId = divisionId,
    DivisionName = $"Division {divisionId}",
    ToEmail = $"division{divisionId}@example.test",
    CcEmail = "group@example.test",
    SettledCount = settled,
    RejectedCount = rejected,
    TotalCount = settled + rejected
};
ManagementAuditWeeklyDivisionDetailModel WeeklyDetail(int divisionId, int historyId, DateTime decisionOn,
    int comStatus = 16, string decisionStatus = "SETTLED") => new()
{
    DecisionHistoryId = historyId,
    ComId = historyId,
    ComCycle = 1,
    EntityId = 1000 + divisionId,
    AuditedBy = 112242,
    DivisionId = divisionId,
    DivisionName = $"Division {divisionId}",
    EntityName = $"Entity {divisionId}",
    AuditPeriod = "2026",
    ParaNo = historyId.ToString(),
    Title = "Test para",
    SubmittedOn = decisionOn.AddDays(-1),
    DecisionOn = decisionOn,
    DecisionStatus = decisionStatus,
    ComStatus = comStatus,
    Reason = comStatus == 16 ? string.Empty : "Reason"
};

var queueStart = monday.AddDays(-7);
var queueEnd = queueStart.AddDays(7);
var queueSummaries = new[] { WeeklySummary(1), WeeklySummary(2) };
var queueEvents = new List<string>();
var activeSends = 0;
var maximumActiveSends = 0;
await ManagementAuditWeeklyService.ProcessDivisionQueueAsync(
    queueStart, queueEnd, queueSummaries,
    key => { queueEvents.Add("claim:" + key); return true; },
    divisionId => { queueEvents.Add("detail:" + divisionId); return new[] { WeeklyDetail(divisionId, divisionId, queueStart.AddDays(1)) }; },
    async (division, _) =>
    {
        queueEvents.Add("send-start:" + division.DivisionId);
        activeSends++;
        maximumActiveSends = Math.Max(maximumActiveSends, activeSends);
        await Task.Delay(5);
        activeSends--;
        queueEvents.Add("send-end:" + division.DivisionId);
        return new EmailSendResult { IsSuccess = true };
    },
    (key, status, _) => queueEvents.Add($"complete:{key}:{status}"),
    (exception, key) => throw new Exception($"Unexpected queue failure for {key}", exception),
    CancellationToken.None);
Check(maximumActiveSends == 1 &&
      queueEvents.Count(item => item.StartsWith("send-start:", StringComparison.Ordinal)) == 2 &&
      queueEvents.IndexOf("send-end:1") < queueEvents.IndexOf("send-start:2") &&
      queueEvents.Contains($"complete:WEEKLY:{queueStart:yyyyMMdd}:1:COMPLETE") &&
      queueEvents.Contains($"complete:WEEKLY:{queueStart:yyyyMMdd}:2:COMPLETE"),
    "Multiple Management Audit Divisions are processed independently and sequentially");

var failureCompletions = new List<string>();
var sentAfterFailure = new List<int>();
await ManagementAuditWeeklyService.ProcessDivisionQueueAsync(
    queueStart, queueEnd, new[] { WeeklySummary(1, 2), WeeklySummary(2) },
    _ => true,
    divisionId => divisionId == 1
        ? new[] { WeeklyDetail(1, 10, queueStart.AddDays(1)), WeeklyDetail(1, 10, queueStart.AddDays(2)) }
        : new[] { WeeklyDetail(2, 20, queueStart.AddDays(1)) },
    (division, _) => { sentAfterFailure.Add(division.DivisionId); return Task.FromResult(new EmailSendResult { IsSuccess = true }); },
    (key, status, _) => failureCompletions.Add($"{key}:{status}"),
    (_, _) => { },
    CancellationToken.None);
Check(sentAfterFailure.SequenceEqual(new[] { 2 }) &&
      failureCompletions.Contains($"WEEKLY:{queueStart:yyyyMMdd}:1:FAILED") &&
      failureCompletions.Contains($"WEEKLY:{queueStart:yyyyMMdd}:2:COMPLETE"),
    "One Division failure does not stop the next Division");

var validSummary = WeeklySummary(1);
var validDetail = WeeklyDetail(1, 1, queueStart.AddDays(1));
Check(Rejects(() => ManagementAuditWeeklyService.ValidateDivisionData(
        new ManagementAuditWeeklyDivisionSummaryModel { DivisionId = 1, TotalCount = 2, SettledCount = 2 },
        new[] { validDetail, WeeklyDetail(1, 1, queueStart.AddDays(2)) }, queueStart, queueEnd)),
    "Duplicate Management Audit DecisionHistoryId is rejected");
Check(Rejects(() => ManagementAuditWeeklyService.ValidateDivisionData(
        new ManagementAuditWeeklyDivisionSummaryModel { DivisionId = 1, TotalCount = 2, SettledCount = 2 },
        new[] { validDetail }, queueStart, queueEnd)) &&
      Rejects(() => ManagementAuditWeeklyService.ValidateDivisionData(
        new ManagementAuditWeeklyDivisionSummaryModel { DivisionId = 1, TotalCount = 1, RejectedCount = 1 },
        new[] { validDetail }, queueStart, queueEnd)),
    "Management Audit summary and detail counts must match");
Check(Rejects(() => ManagementAuditWeeklyService.ValidateDivisionData(validSummary,
        new[] { WeeklyDetail(2, 1, queueStart.AddDays(1)) }, queueStart, queueEnd)) &&
      Rejects(() => ManagementAuditWeeklyService.ValidateDivisionData(validSummary,
        new[] { WeeklyDetail(1, 1, queueStart.AddDays(1), 16, "REJECTED") }, queueStart, queueEnd)) &&
      Rejects(() => ManagementAuditWeeklyService.ValidateDivisionData(validSummary,
        new[] { WeeklyDetail(1, 1, queueEnd) }, queueStart, queueEnd)),
    "Invalid Management Audit Division, status, and date data is rejected");

var claimKeys = new List<string>();
var claimedDetails = new List<int>();
await ManagementAuditWeeklyService.ProcessDivisionQueueAsync(
    queueStart, queueEnd, queueSummaries,
    key => { claimKeys.Add(key); return key.EndsWith(":2", StringComparison.Ordinal); },
    divisionId => { claimedDetails.Add(divisionId); return new[] { WeeklyDetail(divisionId, divisionId, queueStart.AddDays(1)) }; },
    (_, _) => Task.FromResult(new EmailSendResult { IsSuccess = true }),
    (_, _, _) => { },
    (_, _) => { },
    CancellationToken.None);
Check(claimKeys.Count == 2 && claimedDetails.SequenceEqual(new[] { 2 }),
    "Completed or unclaimed Division jobs are not reprocessed");

var retryStates = new Dictionary<int, (string Status, int RetryCount, DateTimeOffset Updated)>
{
    [1] = ("FAILED", 0, firstRetryCheck.AddMinutes(-16)),
    [2] = ("COMPLETE", 0, firstRetryCheck.AddHours(-1)),
    [3] = ("RUNNING", 0, firstRetryCheck.AddHours(-1)),
    [4] = ("FAILED", 0, firstRetryCheck.AddMinutes(-5))
};
var retriedDivisions = new List<int>();
bool RetryClaim(string key)
{
    var divisionId = int.Parse(key.Split(':').Last());
    var state = retryStates[divisionId];
    if (state.Status != "FAILED" || state.RetryCount >= 5 || state.Updated >= firstRetryCheck.AddMinutes(-15))
        return false;
    retryStates[divisionId] = ("RUNNING", state.RetryCount + 1, firstRetryCheck);
    return true;
}
await ManagementAuditWeeklyService.ProcessDivisionQueueAsync(
    queueStart, queueEnd, new[] { WeeklySummary(1), WeeklySummary(2), WeeklySummary(3), WeeklySummary(4) },
    RetryClaim,
    divisionId => { retriedDivisions.Add(divisionId); return new[] { WeeklyDetail(divisionId, divisionId, queueStart.AddDays(1)) }; },
    (_, _) => Task.FromResult(new EmailSendResult { IsSuccess = true }),
    (key, status, _) => retryStates[int.Parse(key.Split(':').Last())] = (status, 1, firstRetryCheck),
    (_, _) => { },
    CancellationToken.None);
Check(retriedDivisions.SequenceEqual(new[] { 1 }), "Eligible FAILED Division is retried independently");
Check(!retriedDivisions.Contains(2), "Completed weekly Division is skipped");
Check(!retriedDivisions.Contains(3), "RUNNING weekly Division is not blindly retried");
Check(!retriedDivisions.Contains(4), "Weekly retries remain specific to each Division's retry age");

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
    new(1,"Entity","2026","2","Test",monday.AddDays(-6),monday.AddDays(-5),"",true,"Low"),
    new(1,"Entity","2026","3","Test",monday.AddDays(-6),monday.AddDays(-4),"Reason",false,"High")
};
var weeklyRecipientSummary = new ManagementAuditWeeklyDivisionSummaryModel
{
    DivisionId = 1,
    DivisionName = "Summary Division",
    ToEmail = "summary-head@example.test",
    CcEmail = "summary-group@example.test",
    SettledCount = 1,
    RejectedCount = 1,
    TotalCount = 2
};
var reportingStart = monday.AddDays(-7);
var approvedSubject = $"IAS Notification: Weekly Management Audit Para Decisions | Summary Division | {reportingStart:dd-MMM-yyyy} to {reportingStart.AddDays(6):dd-MMM-yyyy}";
var completeRequest = EmailNotification.BuildManagementAuditWeeklyEmail(reportingStart, weeklyRecipientSummary, records);
Check(completeRequest.Subject == approvedSubject,
    "Weekly subject includes Division and reporting period in the approved format");
var referredOnlyRequest = EmailNotification.BuildManagementAuditWeeklyEmail(reportingStart, weeklyRecipientSummary, new[] { records[1] });
Check(referredOnlyRequest.Body.Contains("1. Paras Referred Back for Further Compliance (1)") &&
      !referredOnlyRequest.Body.Contains("Paras Settled on the Basis") &&
      !referredOnlyRequest.Body.Contains("Paras Where No Compliance") &&
      !referredOnlyRequest.Body.Contains("No records", StringComparison.OrdinalIgnoreCase),
    "Empty weekly sections are completely omitted");
Check(referredOnlyRequest.Body.Contains("Referred Back") &&
      !referredOnlyRequest.Body.Contains("REJECTED", StringComparison.OrdinalIgnoreCase),
    "Rejected weekly decisions are presented as Referred Back");
var noComplianceRecord = records[1] with
{
    Para = "4",
    NoCompliance = true,
    LastComplianceSubmitted = reportingStart.AddDays(-2),
    Reason = string.Empty
};
var continuousRequest = EmailNotification.BuildManagementAuditWeeklyEmail(reportingStart, weeklyRecipientSummary,
    new[] { records[1], noComplianceRecord });
Check(continuousRequest.Body.Contains("1. Paras Referred Back for Further Compliance (1)") &&
      continuousRequest.Body.Contains("2. Paras Where No Compliance Was Submitted During the Reporting Period (1)") &&
      !continuousRequest.Body.Contains("3. Paras"),
    "Weekly section numbering is dynamic and continuous");
Check(completeRequest.ToRecipients.Single() == weeklyRecipientSummary.ToEmail &&
      completeRequest.CcRecipients.Single() == weeklyRecipientSummary.CcEmail,
    "Weekly request recipients come only from Division Summary");
var encodedRecord = records[1] with { Entity = "<Department & Co>", Title = "Para <Title>", Reason = "Use <evidence> & review" };
var encodedRequest = EmailNotification.BuildManagementAuditWeeklyEmail(reportingStart,
    weeklyRecipientSummary, new[] { encodedRecord });
Check(encodedRequest.Body.Contains("&lt;Department &amp; Co&gt;") &&
      encodedRequest.Body.Contains("Para &lt;Title&gt;") &&
      encodedRequest.Body.Contains("Use &lt;evidence&gt; &amp; review") &&
      !encodedRequest.Body.Contains("<Department & Co>"),
    "Weekly HTML encodes all business values");
var allSectionsRequest = EmailNotification.BuildManagementAuditWeeklyEmail(reportingStart,
    weeklyRecipientSummary, records.Append(noComplianceRecord).ToList());
Check(new[] { "Department", "Audit Year", "Para No.", "Title of Para", "Risk", "Compliance Submitted On", "Settled On", "Reason for Referral Back", "Last Compliance Submitted On" }
        .All(heading => allSectionsRequest.Body.Contains(heading)),
    "All populated weekly tables contain the approved headings");
Check(allSectionsRequest.Body.Contains(">Summary</h2>") &&
      new[] { "Sr.", "Department", "Settled", "Referred Back", "No Compliance", "Total" }
        .All(heading => allSectionsRequest.Body.Contains(heading)) &&
      allSectionsRequest.Body.Contains("colspan=\"2\"") &&
      allSectionsRequest.Body.Contains(">3</td>"),
    "Weekly email includes a Department summary table with totals");
Check((await EmailNotification.SendManagementAuditWeeklyAsync(config,monday.AddDays(-7),weeklyRecipientSummary,records)).IsSuccess,"Weekly SMTP succeeds");
await server.WaitAsync(TimeSpan.FromSeconds(15));
listener.Stop();
Check(messages.Count==2,"One SMTP message per invocation");
var weeklyMessage = messages.Single(message => message.Contains("Weekly Management Audit Para Decisions"));
Check(messages.Count(message => message.Contains("Weekly Management Audit Para Decisions")) == 1,
    "One returned Division produces one weekly email");
Check(weeklyMessage.Contains("summary-head@example.test", StringComparison.OrdinalIgnoreCase),
    "Weekly TO comes from DivisionSummary.ToEmail");
Check(weeklyMessage.Contains("summary-group@example.test", StringComparison.OrdinalIgnoreCase),
    "Weekly CC comes from DivisionSummary.CcEmail");
Check(typeof(ManagementAuditDecision).GetProperties().All(property => property.Name != "To" && property.Name != "Cc"),
    "Weekly detail rows cannot alter recipients");
Check(messages[0].Contains("Rejected") && messages[0].Contains("Decision reason"),"Rejection content preserved");
try {
    await EmailNotification.SendManagementAuditWeeklyAsync(config,monday,weeklyRecipientSummary,
        records.Append(records[0] with { DivisionId=2 }).ToList());
    throw new Exception("Mixed divisions accepted");
} catch (InvalidOperationException) { Console.WriteLine("PASS: mixed divisions rejected"); }
