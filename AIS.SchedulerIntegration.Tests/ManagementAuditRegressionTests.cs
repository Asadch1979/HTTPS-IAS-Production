using AIS;
using AIS.Models.Notifications;
using AIS.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

internal static class ManagementAuditRegressionTests
{
    public static async Task RunAsync()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var messages = new List<string>();
        var envelopes = new List<string>();
        var server = Task.Run(async () =>
        {
            for (var i = 0; i < 3; i++)
            {
                using var client = await listener.AcceptTcpClientAsync(timeout.Token);
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream) { AutoFlush = true, NewLine = "\r\n" };
                await writer.WriteLineAsync("220 localhost regression SMTP");
                string line;
                while ((line = await reader.ReadLineAsync(timeout.Token)) != null)
                {
                    if (line.StartsWith("RCPT TO", StringComparison.OrdinalIgnoreCase)) envelopes.Add(line);
                    if (line.StartsWith("DATA", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("354 continue");
                        var body = new StringBuilder();
                        while ((line = await reader.ReadLineAsync(timeout.Token)) != null && line != ".") body.AppendLine(line);
                        messages.Add(body.ToString());
                        await writer.WriteLineAsync("250 accepted");
                    }
                    else if (line.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("221 bye");
                        break;
                    }
                    else await writer.WriteLineAsync("250 OK");
                }
            }
        }, timeout.Token);

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Email:From"] = "ias@example.test", ["Email:Password"] = "test",
            ["Email:Host"] = "127.0.0.1", ["Email:EnableSsl"] = "false",
            ["Email:Port"] = ((IPEndPoint)listener.LocalEndpoint).Port.ToString()
        }).Build();
        bool Immediate(string status) => EmailNotification.NotifyManagementAuditParaStatus(config,
            "7", status, "2026", "High", "A <para>", "Supply evidence", "head@example.test",
            "head@example.test;group@example.test", "GROUP@example.test", "Report", "Division", "Department");
        Check(!Immediate("Forwarded") && !Immediate(""), "Non-decisions must not send");
        Check(Immediate(" Settled ") && Immediate("Rejected/Referred Back"), "Immediate decisions send");

        var start = new DateTime(2026, 9, 21);
        var end = start.AddDays(7);
        var division = new ManagementAuditWeeklyDivisionSummaryModel
        {
            DivisionId = 1, DivisionName = "Test division", ToEmail = "head@example.test",
            CcEmail = "group@example.test", BccEmail = "archive@example.test",
            SettledCount = 1, RejectedCount = 1, NoComplianceCount = 1, TotalCount = 3
        };
        var details = new[]
        {
            new ManagementAuditWeeklyDivisionDetailModel { DivisionId = 1, DecisionHistoryId = 1,
                ComStatus = 16, DecisionStatus = "SETTLED", DecisionOn = start, Title = "<settled>" },
            new ManagementAuditWeeklyDivisionDetailModel { DivisionId = 1, DecisionHistoryId = 2,
                ComStatus = 12, DecisionStatus = "REJECTED", DecisionOn = end.AddTicks(-1), Reason = "Evidence" }
        };
        var noCompliance = new[] { new ManagementAuditWeeklyNoComplianceModel { DivisionId = 1,
            ComId = 3, LastComplianceSubmittedOn = start.AddDays(-3) } };
        var ledger = new Dictionary<string, string>();
        var sent = 0;
        async Task<ManagementAuditProcessingResult> Run() => await ManagementAuditWeeklyService.ProcessDivisionQueueAsync(
            start, end, new[] { division }, key => ledger.TryAdd(key, "RUNNING"), _ => details, _ => noCompliance,
            async (summary, records) =>
            {
                sent++;
                Check(records.Last().LastComplianceSubmitted == start.AddDays(-3), "Last submission date retained");
                var request = EmailNotification.BuildManagementAuditComplianceReviewEmail(start, end, summary, records);
                Check(request.Subject.Contains("27-Sep-2026"), "Exclusive end displayed inclusively");
                Check(request.Body.Contains("&lt;settled&gt;"), "Business values encoded");
                return await EmailNotification.SendManagementAuditComplianceReviewAsync(config, null, start, end, summary, records);
            }, (key, status, _) => ledger[key] = status, (error, _) => throw error, CancellationToken.None);
        var first = await Run();
        var second = await Run();
        Check(first.RecordsProcessed == 3 && first.CompletedDivisions == 1 && second.CompletedDivisions == 0 && sent == 1,
            "Completed division is not sent twice");
        await server.WaitAsync(timeout.Token);
        Check(messages.Count == 3, "Exactly two immediate and one consolidated message");
        Check(envelopes.Count(x => x.Contains("head@example.test", StringComparison.OrdinalIgnoreCase)) == 3,
            "TO and CC duplicates removed");
        Check(envelopes.Count(x => x.Contains("group@example.test", StringComparison.OrdinalIgnoreCase)) == 3,
            "Duplicate CC addresses removed");
        Check(envelopes.Count(x => x.Contains("archive@example.test")) == 1, "Consolidated BCC delivered");
        Check(messages[1].Contains("Referred Back") && messages[1].Contains("Supply evidence"), "Referral reason delivered");

        var failures = new List<string>();
        var failure = await ManagementAuditWeeklyService.ProcessDivisionQueueAsync(start, end, new[] { division },
            _ => true, _ => details, _ => noCompliance,
            (_, _) => Task.FromResult(new EmailSendResult { IsSuccess = false, ErrorMessage = "SMTP refused" }),
            (_, status, _) => failures.Add(status), (_, _) => { }, CancellationToken.None);
        Check(failure.FailedDivisions == 1 && failures.Single() == "FAILED", "Rejected delivery remains retryable");
        failures.Clear();
        await ManagementAuditWeeklyService.ProcessDivisionQueueAsync(start, end, new[] { division },
            _ => true, _ => details, _ => noCompliance,
            (_, _) => throw new IOException("Unknown SMTP outcome"),
            (_, status, _) => failures.Add(status), (_, _) => { }, CancellationToken.None);
        Check(failures.Count == 0, "Unknown delivery outcome is not blindly retried");
        details[0].DecisionOn = end;
        try
        {
            ManagementAuditWeeklyService.ValidateDivisionData(division, details, noCompliance, start, end);
            throw new Exception("Out-of-period decision accepted");
        }
        catch (InvalidOperationException) { }

        var runs = 0;
        var reviewLedger = new Dictionary<string, NotificationExecutionStore.ExecutionRecord>();
        string Review() => NotificationExecutionStore.ExecuteReviewCore("REVIEW:one", "fingerprint",
            () => { runs++; return "settled"; },
            (key, fingerprint) => reviewLedger.TryAdd(key, new(fingerprint, "RUNNING", null)),
            (key, status, response) => reviewLedger[key] = new("fingerprint", status, response),
            key => reviewLedger[key]);
        Check(Review() == "settled" && Review() == "settled" && runs == 1, "HTTP replay executes business decision once");

        using var provider = new ServiceCollection().BuildServiceProvider();
        var handler = new ManagementAuditWeeklyService(config, new NotificationExecutionStore(config), null,
            provider, NullLogger<ManagementAuditWeeklyService>.Instance);
        Check(ReferenceEquals(provider, typeof(ManagementAuditWeeklyService).GetField("serviceProvider",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(handler)), "Scheduler retains scoped logging provider");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
