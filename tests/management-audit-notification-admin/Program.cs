using AIS;
using AIS.Models.Notifications;
using AIS.Services;
using Microsoft.Extensions.Configuration;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

void Check(bool condition, string label)
    {
    if (!condition) throw new InvalidOperationException(label);
    Console.WriteLine("PASS: " + label);
    }

bool IsValid(object model)
    {
    return Validator.TryValidateObject(model, new ValidationContext(model), new List<ValidationResult>(), true);
    }

var futureDate = DateTime.Today.AddYears(1);
Check(IsValid(new SaveManagementAuditNotificationDivisionModel
    {
    DivisionId = 10,
    DivisionName = "Credit Division",
    EffectiveFrom = DateTime.Today,
    EffectiveTo = futureDate
    }), "Division accepts a future Effective To");
Check(IsValid(new SaveManagementAuditNotificationDivisionModel
    {
    DivisionId = 10,
    DivisionName = "Credit Division",
    EffectiveFrom = DateTime.Today,
    EffectiveTo = null
    }), "Division accepts an open-ended Effective To");
Check(!IsValid(new SaveManagementAuditNotificationDivisionModel
    {
    DivisionId = 10,
    DivisionName = "Credit Division",
    EffectiveFrom = DateTime.Today,
    EffectiveTo = DateTime.Today.AddDays(-1)
    }), "Division rejects Effective To before Effective From");

Check(ManagementAuditRecipientEmailList.TryNormalize(
        " user1@ztbl.com.pk ; user2@ztbl.com.pk;USER1@ztbl.com.pk ", out var normalized) &&
      normalized == "user1@ztbl.com.pk; user2@ztbl.com.pk",
    "Multiple addresses are trimmed, normalized and deduplicated");
Check(!ManagementAuditRecipientEmailList.TryNormalize("user1@ztbl.com.pk;;user2@ztbl.com.pk", out _),
    "Repeated separators are rejected");
Check(!ManagementAuditRecipientEmailList.TryNormalize("user1@ztbl.com.pk;not-an-email", out _),
    "An invalid address within a list is rejected");
Check(IsValid(new SaveManagementAuditNotificationRecipientModel
    {
    DivisionId = 10,
    RecipientType = "BCC",
    EmailAddress = "user1@ztbl.com.pk; user2@ztbl.com.pk",
    EffectiveFrom = DateTime.Today,
    EffectiveTo = futureDate,
    DisplayOrder = 1
    }), "Recipient accepts BCC, multiple addresses and a future Effective To");

var summary = new ManagementAuditWeeklyDivisionSummaryModel
    {
    DivisionId = 10,
    DivisionName = "Credit Division",
    ToEmail = "to@example.test; shared@example.test",
    CcEmail = "cc@example.test; shared@example.test",
    BccEmail = "bcc@example.test; cc@example.test; shared@example.test"
    };
var request = EmailNotification.BuildManagementAuditComplianceReviewEmail(
    new DateTime(2026, 9, 1),
    new DateTime(2026, 10, 1),
    summary,
    new[]
        {
        new ManagementAuditDecision(10, "Department", "2026", "1", "Title", DateTime.Today, DateTime.Today, string.Empty, true)
        });
Check(request.BccRecipients.Single() == summary.BccEmail, "Management Audit request carries BCC separately");

var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
    {
    ["Email:From"] = "ias@example.test",
    ["Email:Password"] = "test",
    ["Email:Host"] = "127.0.0.1",
    ["Email:Port"] = "25"
    }).Build();
var emailConfiguration = new EmailConfiguration(configuration);
var prepare = typeof(EmailConfiguration).GetMethod("PrepareRequest", BindingFlags.Instance | BindingFlags.NonPublic);
var prepared = prepare.Invoke(emailConfiguration, new object[] { request });
IReadOnlyList<string> Recipients(string propertyName)
    {
    var value = prepared.GetType().GetProperty(propertyName).GetValue(prepared) as IEnumerable;
    return value.Cast<string>().ToList();
    }

var to = Recipients("ToRecipients");
var cc = Recipients("CcRecipients");
var bcc = Recipients("BccRecipients");
Check(to.SequenceEqual(new[] { "to@example.test", "shared@example.test" }), "TO routing remains intact");
Check(cc.SequenceEqual(new[] { "cc@example.test" }), "CC routing excludes addresses already in TO");
Check(bcc.SequenceEqual(new[] { "bcc@example.test" }), "BCC routing is deduplicated against TO and CC");
Check(!to.Contains("bcc@example.test") && !cc.Contains("bcc@example.test"),
    "BCC addresses are not exposed in TO or CC");

var root = FindRepositoryRoot();
var view = File.ReadAllText(Path.Combine(root, "AIS", "Views", "AdministrationPanel", "management_audit_notifications.cshtml"));
var script = File.ReadAllText(Path.Combine(root, "AIS", "wwwroot", "js", "csp", "Views_AdministrationPanel_management_audit_notifications.js"));
var schedulerView = File.ReadAllText(Path.Combine(root, "AIS", "Views", "AdministrationPanel", "ias_scheduler.cshtml"));
var adminSql = File.ReadAllText(Path.Combine(root, "AIS", "Docs", "sql", "management_audit_notification_admin.sql"));
var routingSql = File.ReadAllText(Path.Combine(root, "AIS", "Docs", "sql", "pkg_mgmt_audit_weekly.sql"));

Check(view.Contains("@section Scripts") && view.Contains("asp-append-version=\"true\"") &&
      view.IndexOf("bootstrap.bundle", StringComparison.OrdinalIgnoreCase) < 0,
    "Page script uses the Razor Scripts section without loading Bootstrap again");
Check(view.Contains("data-allow-future=\"true\"") && script.Contains("validateDateRange"),
    "Configuration Effective To fields use the scoped future-date exemption and date-order validation");
Check(view.Contains("<option value=\"BCC\">BCC</option>") && view.Contains("type=\"text\" class=\"form-control\" maxlength=\"500\""),
    "Recipient editor supports BCC and does not use single-address HTML email validation");
Check(view.Contains("data-configured=") && script.Contains("selectedOption.disabled = false") && script.Contains("divisionOption.disabled = true"),
    "Edit Division matches and displays its configured option while remaining non-editable");
Check(!view.Contains("Weekly Email", StringComparison.OrdinalIgnoreCase) &&
      !view.Contains("weekly delivery", StringComparison.OrdinalIgnoreCase),
    "Administration UI contains no obsolete weekly wording");
Check(schedulerView.Contains("Run Now</button>") && schedulerView.Contains("Edit</button>") &&
      schedulerView.Contains("? \"Disable\" : \"Enable\""),
    "Scheduler actions have readable labels");
Check(adminSql.Contains("SEQ_IAS_MGMT_NOTIFY_RECIPIENT.NEXTVAL") &&
      adminSql.Contains("IF P_RECIPIENT_ID IS NULL THEN") &&
      adminSql.Contains("WHERE R.RECIPIENT_ID = P_RECIPIENT_ID") &&
      adminSql.Contains("R.IS_ACTIVE = 'N'") &&
      !adminSql.Contains("DELETE FROM T_IAS_MGMT_NOTIFY_RECIPIENT", StringComparison.OrdinalIgnoreCase),
    "Admin package inserts from the sequence, updates by ID and removes by deactivation");
Check(adminSql.Contains("NORMALIZE_EMAIL_LIST") && adminSql.Contains("('TO', 'CC', 'BCC')"),
    "Admin package validates semicolon-separated email lists and supports BCC");
Check(routingSql.Contains("BCC_RECIPIENTS AS") && routingSql.Contains("B.BCC_EMAIL"),
    "Database routing returns BCC independently");

Console.WriteLine("All Management Audit notification administration regression checks passed.");

static string FindRepositoryRoot()
    {
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory != null)
        {
        if (File.Exists(Path.Combine(directory.FullName, "AIS", "AIS.csproj"))) return directory.FullName;
        directory = directory.Parent;
        }
    throw new DirectoryNotFoundException("Repository root not found.");
    }
