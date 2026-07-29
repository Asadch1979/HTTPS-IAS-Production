using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using AIS.Models.EmailManagement;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public long LogEmailTriggerAttempt(string module, string triggerPoint, string referenceId, string toAddress, string ccAddress, string subject)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO T_AU_EMAIL_TRIGGER_LOG
                (ID, TRIGGER_DATE, MODULE, TRIGGER_POINT, REFERENCE_ID, TO_ADDRESS, CC_ADDRESS, EMAIL_SUBJECT, STATUS)
                VALUES (SEQ_T_AU_EMAIL_TRIGGER_LOG.NEXTVAL, SYSTIMESTAMP, :MODULE, :TRIGGER_POINT, :REFERENCE_ID, :TO_ADDRESS, :CC_ADDRESS, :EMAIL_SUBJECT, 'TRIGGERED')
                RETURNING ID INTO :LOG_ID";
            cmd.CommandType = CommandType.Text;
            cmd.BindByName = true;
            cmd.Parameters.Add("MODULE", OracleDbType.Varchar2).Value = DbValue(module, 100);
            cmd.Parameters.Add("TRIGGER_POINT", OracleDbType.Varchar2).Value = DbValue(triggerPoint, 200);
            cmd.Parameters.Add("REFERENCE_ID", OracleDbType.Varchar2).Value = DbValue(referenceId, 200);
            cmd.Parameters.Add("TO_ADDRESS", OracleDbType.Varchar2).Value = DbValue(toAddress, 2000);
            cmd.Parameters.Add("CC_ADDRESS", OracleDbType.Varchar2).Value = DbValue(ccAddress, 2000);
            cmd.Parameters.Add("EMAIL_SUBJECT", OracleDbType.Varchar2).Value = DbValue(subject, 1000);
            var id = cmd.Parameters.Add("LOG_ID", OracleDbType.Int64);
            id.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            return Convert.ToInt64(id.Value.ToString());
            }

        public void CompleteEmailTriggerAttempt(long logId, string status, string errorMessage, bool sent)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"UPDATE T_AU_EMAIL_TRIGGER_LOG
                SET STATUS = :STATUS, ERROR_MESSAGE = :ERROR_MESSAGE,
                    SENT_ON = CASE WHEN :IS_SENT = 1 THEN SYSTIMESTAMP ELSE NULL END
                WHERE ID = :LOG_ID";
            cmd.CommandType = CommandType.Text;
            cmd.BindByName = true;
            cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = status;
            cmd.Parameters.Add("ERROR_MESSAGE", OracleDbType.Varchar2).Value = DbValue(errorMessage, 4000);
            cmd.Parameters.Add("IS_SENT", OracleDbType.Int32).Value = sent ? 1 : 0;
            cmd.Parameters.Add("LOG_ID", OracleDbType.Int64).Value = logId;
            cmd.ExecuteNonQuery();
            }

        private static object DbValue(string value, int maxLength)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return DBNull.Value;
                }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
            }

        // Standalone Email Management Module methods. These call only the isolated
        // PKG_EMAIL / EM_EMAIL_* objects and are not used by existing AIS triggers.
        public List<EmailManagementEvent> GetManagedEmailEvents(long? eventId = null)
            {
            var result = new List<EmailManagementEvent>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.GET_EVENTS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64).Value = eventId ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                result.Add(new EmailManagementEvent
                    {
                    EventId = Convert.ToInt64(reader["EVENT_ID"]),
                    EventKey = reader["EVENT_KEY"]?.ToString() ?? string.Empty,
                    DisplayName = reader["DISPLAY_NAME"]?.ToString() ?? string.Empty,
                    Description = reader["DESCRIPTION"]?.ToString() ?? string.Empty,
                    IsEnabled = Convert.ToInt32(reader["IS_ENABLED"]) == 1,
                    ActiveTemplateId = reader["ACTIVE_TEMPLATE_ID"] == DBNull.Value ? null : Convert.ToInt64(reader["ACTIVE_TEMPLATE_ID"]),
                    ActiveTemplateName = reader["ACTIVE_TEMPLATE_NAME"]?.ToString() ?? string.Empty
                    });
                }
            return result;
            }

        public long SaveManagedEmailEvent(EmailManagementEvent model, string user)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.UPSERT_EVENT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            var id = cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64);
            id.Direction = ParameterDirection.InputOutput;
            id.Value = model.EventId > 0 ? model.EventId : (object)DBNull.Value;
            cmd.Parameters.Add("P_EVENT_KEY", OracleDbType.Varchar2).Value = model.EventKey;
            cmd.Parameters.Add("P_DISPLAY_NAME", OracleDbType.Varchar2).Value = model.DisplayName;
            cmd.Parameters.Add("P_DESCRIPTION", OracleDbType.Varchar2).Value = DbValue(model.Description, 500);
            cmd.Parameters.Add("P_IS_ENABLED", OracleDbType.Int32).Value = model.IsEnabled ? 1 : 0;
            cmd.Parameters.Add("P_USER", OracleDbType.Varchar2).Value = DbValue(user, 100);
            cmd.ExecuteNonQuery();
            return Convert.ToInt64(((OracleDecimal)id.Value).Value);
            }

        public void SetManagedEmailEventEnabled(long eventId, bool enabled, string user)
            {
            ExecuteManagedEmailNonQuery("PKG_EMAIL.SET_EVENT_ENABLED",
                ("P_EVENT_ID", OracleDbType.Int64, eventId),
                ("P_IS_ENABLED", OracleDbType.Int32, enabled ? 1 : 0),
                ("P_USER", OracleDbType.Varchar2, DbValue(user, 100)));
            }

        public List<EmailManagementTemplate> GetManagedEmailTemplates(long? eventId = null, long? templateId = null)
            {
            var result = new List<EmailManagementTemplate>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.GET_TEMPLATES";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64).Value = eventId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_TEMPLATE_ID", OracleDbType.Int64).Value = templateId ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                result.Add(new EmailManagementTemplate
                    {
                    TemplateId = Convert.ToInt64(reader["TEMPLATE_ID"]),
                    EventId = Convert.ToInt64(reader["EVENT_ID"]),
                    TemplateName = reader["TEMPLATE_NAME"]?.ToString() ?? string.Empty,
                    Culture = reader["CULTURE"]?.ToString() ?? "en",
                    SubjectTemplate = reader["SUBJECT_TEMPLATE"]?.ToString() ?? string.Empty,
                    BodyHtmlTemplate = reader["BODY_HTML_TEMPLATE"]?.ToString() ?? string.Empty,
                    VersionNo = Convert.ToInt32(reader["VERSION_NO"]),
                    IsActive = Convert.ToInt32(reader["IS_ACTIVE"]) == 1,
                    EventKey = reader["EVENT_KEY"]?.ToString() ?? string.Empty
                    });
                }
            return result;
            }

        public long SaveManagedEmailTemplate(EmailManagementTemplate model, string user)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.UPSERT_TEMPLATE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            var id = cmd.Parameters.Add("P_TEMPLATE_ID", OracleDbType.Int64);
            id.Direction = ParameterDirection.InputOutput;
            id.Value = model.TemplateId > 0 ? model.TemplateId : (object)DBNull.Value;
            cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64).Value = model.EventId;
            cmd.Parameters.Add("P_TEMPLATE_NAME", OracleDbType.Varchar2).Value = model.TemplateName;
            cmd.Parameters.Add("P_CULTURE", OracleDbType.Varchar2).Value = model.Culture;
            cmd.Parameters.Add("P_SUBJECT_TEMPLATE", OracleDbType.Clob).Value = model.SubjectTemplate;
            cmd.Parameters.Add("P_BODY_HTML_TEMPLATE", OracleDbType.Clob).Value = model.BodyHtmlTemplate;
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Int32).Value = model.IsActive ? 1 : 0;
            cmd.Parameters.Add("P_USER", OracleDbType.Varchar2).Value = DbValue(user, 100);
            cmd.ExecuteNonQuery();
            return Convert.ToInt64(((OracleDecimal)id.Value).Value);
            }

        public void SetManagedEmailActiveTemplate(long eventId, long templateId, string user)
            {
            ExecuteManagedEmailNonQuery("PKG_EMAIL.SET_ACTIVE_TEMPLATE",
                ("P_EVENT_ID", OracleDbType.Int64, eventId),
                ("P_TEMPLATE_ID", OracleDbType.Int64, templateId),
                ("P_USER", OracleDbType.Varchar2, DbValue(user, 100)));
            }

        public List<EmailManagementRule> GetManagedEmailRules(long? eventId = null)
            {
            var result = new List<EmailManagementRule>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.GET_RULES";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64).Value = eventId ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                result.Add(new EmailManagementRule
                    {
                    RuleId = Convert.ToInt64(reader["RULE_ID"]),
                    EventId = Convert.ToInt64(reader["EVENT_ID"]),
                    ToRecipients = reader["TO_RECIPIENTS"]?.ToString() ?? string.Empty,
                    CcRecipients = reader["CC_RECIPIENTS"]?.ToString() ?? string.Empty,
                    BccRecipients = reader["BCC_RECIPIENTS"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToInt32(reader["IS_ACTIVE"]) == 1,
                    EventKey = reader["EVENT_KEY"]?.ToString() ?? string.Empty
                    });
                }
            return result;
            }

        public long SaveManagedEmailRule(EmailManagementRule model, string user)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.UPSERT_RULE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            var id = cmd.Parameters.Add("P_RULE_ID", OracleDbType.Int64);
            id.Direction = ParameterDirection.InputOutput;
            id.Value = model.RuleId > 0 ? model.RuleId : (object)DBNull.Value;
            cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64).Value = model.EventId;
            cmd.Parameters.Add("P_TO_RECIPIENTS", OracleDbType.Varchar2).Value = DbValue(model.ToRecipients, 2000);
            cmd.Parameters.Add("P_CC_RECIPIENTS", OracleDbType.Varchar2).Value = DbValue(model.CcRecipients, 2000);
            cmd.Parameters.Add("P_BCC_RECIPIENTS", OracleDbType.Varchar2).Value = DbValue(model.BccRecipients, 2000);
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Int32).Value = model.IsActive ? 1 : 0;
            cmd.Parameters.Add("P_USER", OracleDbType.Varchar2).Value = DbValue(user, 100);
            cmd.ExecuteNonQuery();
            return Convert.ToInt64(((OracleDecimal)id.Value).Value);
            }

        public List<EmailManagementPlaceholder> GetManagedEmailPlaceholders(long? eventId = null)
            {
            var result = new List<EmailManagementPlaceholder>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.GET_PLACEHOLDERS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64).Value = eventId ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                result.Add(new EmailManagementPlaceholder
                    {
                    PlaceholderId = Convert.ToInt64(reader["PLACEHOLDER_ID"]),
                    EventId = Convert.ToInt64(reader["EVENT_ID"]),
                    Token = reader["TOKEN"]?.ToString() ?? string.Empty,
                    DisplayName = reader["DISPLAY_NAME"]?.ToString() ?? string.Empty,
                    TestValue = reader["TEST_VALUE"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToInt32(reader["IS_ACTIVE"]) == 1,
                    EventKey = reader["EVENT_KEY"]?.ToString() ?? string.Empty
                    });
                }
            return result;
            }

        public long SaveManagedEmailPlaceholder(EmailManagementPlaceholder model, string user)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.UPSERT_PLACEHOLDER";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            var id = cmd.Parameters.Add("P_PLACEHOLDER_ID", OracleDbType.Int64);
            id.Direction = ParameterDirection.InputOutput;
            id.Value = model.PlaceholderId > 0 ? model.PlaceholderId : (object)DBNull.Value;
            cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64).Value = model.EventId;
            cmd.Parameters.Add("P_TOKEN", OracleDbType.Varchar2).Value = model.Token;
            cmd.Parameters.Add("P_DISPLAY_NAME", OracleDbType.Varchar2).Value = model.DisplayName;
            cmd.Parameters.Add("P_TEST_VALUE", OracleDbType.Clob).Value = model.TestValue ?? string.Empty;
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Int32).Value = model.IsActive ? 1 : 0;
            cmd.Parameters.Add("P_USER", OracleDbType.Varchar2).Value = DbValue(user, 100);
            cmd.ExecuteNonQuery();
            return Convert.ToInt64(((OracleDecimal)id.Value).Value);
            }

        public List<EmailManagementAttachmentDefinition> GetManagedEmailAttachments(long? eventId = null)
            {
            var result = new List<EmailManagementAttachmentDefinition>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.GET_ATTACHMENTS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64).Value = eventId ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                result.Add(new EmailManagementAttachmentDefinition
                    {
                    AttachmentId = Convert.ToInt64(reader["ATTACHMENT_ID"]),
                    EventId = Convert.ToInt64(reader["EVENT_ID"]),
                    AttachmentName = reader["ATTACHMENT_NAME"]?.ToString() ?? string.Empty,
                    SourceType = reader["SOURCE_TYPE"]?.ToString() ?? string.Empty,
                    SourceReference = reader["SOURCE_REFERENCE"]?.ToString() ?? string.Empty,
                    FileNamePattern = reader["FILE_NAME_PATTERN"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToInt32(reader["IS_ACTIVE"]) == 1
                    });
                }
            return result;
            }

        public long SaveManagedEmailAttachment(EmailManagementAttachmentDefinition model, string user)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.UPSERT_ATTACHMENT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            var id = cmd.Parameters.Add("P_ATTACHMENT_ID", OracleDbType.Int64);
            id.Direction = ParameterDirection.InputOutput;
            id.Value = model.AttachmentId > 0 ? model.AttachmentId : (object)DBNull.Value;
            cmd.Parameters.Add("P_EVENT_ID", OracleDbType.Int64).Value = model.EventId;
            cmd.Parameters.Add("P_ATTACHMENT_NAME", OracleDbType.Varchar2).Value = model.AttachmentName;
            cmd.Parameters.Add("P_SOURCE_TYPE", OracleDbType.Varchar2).Value = model.SourceType;
            cmd.Parameters.Add("P_SOURCE_REFERENCE", OracleDbType.Varchar2).Value = model.SourceReference;
            cmd.Parameters.Add("P_FILE_NAME_PATTERN", OracleDbType.Varchar2).Value = DbValue(model.FileNamePattern, 250);
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Int32).Value = model.IsActive ? 1 : 0;
            cmd.Parameters.Add("P_USER", OracleDbType.Varchar2).Value = DbValue(user, 100);
            cmd.ExecuteNonQuery();
            return Convert.ToInt64(((OracleDecimal)id.Value).Value);
            }

        public long CreateManagedEmailLog(EmailManagementLog model)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.CREATE_LOG";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            var id = cmd.Parameters.Add("P_LOG_ID", OracleDbType.Int64);
            id.Direction = ParameterDirection.Output;
            AddManagedEmailLogParameters(cmd, model);
            cmd.ExecuteNonQuery();
            return Convert.ToInt64(((OracleDecimal)id.Value).Value);
            }

        public void CompleteManagedEmailLog(long logId, string status, string smtpResponse, string failureDetails)
            {
            ExecuteManagedEmailNonQuery("PKG_EMAIL.COMPLETE_LOG",
                ("P_LOG_ID", OracleDbType.Int64, logId),
                ("P_STATUS", OracleDbType.Varchar2, status),
                ("P_SMTP_RESPONSE", OracleDbType.Varchar2, DbValue(smtpResponse, 2000)),
                ("P_FAILURE_DETAILS", OracleDbType.Clob, string.IsNullOrWhiteSpace(failureDetails) ? (object)DBNull.Value : failureDetails));
            }

        public List<EmailManagementLog> GetManagedEmailLogs(EmailManagementLogFilter filter = null, long? logId = null)
            {
            filter ??= new EmailManagementLogFilter();
            var result = new List<EmailManagementLog>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL.GET_LOGS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_LOG_ID", OracleDbType.Int64).Value = logId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_FROM_UTC", OracleDbType.TimeStampTZ).Value = filter.FromDate.HasValue ? ToUtcOffset(filter.FromDate.Value.Date) : (object)DBNull.Value;
            cmd.Parameters.Add("P_TO_UTC", OracleDbType.TimeStampTZ).Value = filter.ToDate.HasValue ? ToUtcOffset(filter.ToDate.Value.Date.AddDays(1)) : (object)DBNull.Value;
            cmd.Parameters.Add("P_EVENT_KEY", OracleDbType.Varchar2).Value = DbValue(filter.EventKey, 100);
            cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2).Value = DbValue(filter.Status, 30);
            cmd.Parameters.Add("P_RECIPIENT", OracleDbType.Varchar2).Value = DbValue(filter.Recipient, 320);
            cmd.Parameters.Add("P_SUBJECT", OracleDbType.Varchar2).Value = DbValue(filter.Subject, 500);
            cmd.Parameters.Add("P_CORRELATION_ID", OracleDbType.Varchar2).Value = DbValue(filter.CorrelationId, 200);
            cmd.Parameters.Add("O_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                result.Add(MapManagedEmailLog(reader));
                }
            return result;
            }

        private void ExecuteManagedEmailNonQuery(string procedure, params (string Name, OracleDbType Type, object Value)[] values)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = procedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            foreach (var value in values)
                {
                cmd.Parameters.Add(value.Name, value.Type).Value = value.Value ?? DBNull.Value;
                }
            cmd.ExecuteNonQuery();
            }

        private static void AddManagedEmailLogParameters(OracleCommand cmd, EmailManagementLog model)
            {
            cmd.Parameters.Add("P_EVENT_KEY", OracleDbType.Varchar2).Value = model.EventKey;
            cmd.Parameters.Add("P_TEMPLATE_ID", OracleDbType.Int64).Value = model.TemplateId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_TEMPLATE_VERSION", OracleDbType.Int32).Value = model.TemplateVersion ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_SUBJECT_SENT", OracleDbType.Clob).Value = model.SubjectSent ?? string.Empty;
            cmd.Parameters.Add("P_BODY_SENT", OracleDbType.Clob).Value = model.BodySent ?? string.Empty;
            cmd.Parameters.Add("P_TO_RECIPIENTS", OracleDbType.Varchar2).Value = DbValue(model.ToRecipients, 2000);
            cmd.Parameters.Add("P_CC_RECIPIENTS", OracleDbType.Varchar2).Value = DbValue(model.CcRecipients, 2000);
            cmd.Parameters.Add("P_BCC_RECIPIENTS", OracleDbType.Varchar2).Value = DbValue(model.BccRecipients, 2000);
            cmd.Parameters.Add("P_ATTACHMENT_METADATA", OracleDbType.Clob).Value = string.IsNullOrWhiteSpace(model.AttachmentMetadata) ? (object)DBNull.Value : model.AttachmentMetadata;
            cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2).Value = model.Status;
            cmd.Parameters.Add("P_SMTP_RESPONSE", OracleDbType.Varchar2).Value = DbValue(model.SmtpResponse, 2000);
            cmd.Parameters.Add("P_ATTEMPT_NUMBER", OracleDbType.Int32).Value = model.AttemptNumber;
            cmd.Parameters.Add("P_CALLING_COMPONENT", OracleDbType.Varchar2).Value = DbValue(model.CallingComponent, 200);
            cmd.Parameters.Add("P_INITIATED_BY", OracleDbType.Varchar2).Value = DbValue(model.InitiatedBy, 100);
            cmd.Parameters.Add("P_CORRELATION_ID", OracleDbType.Varchar2).Value = DbValue(model.CorrelationId, 100);
            cmd.Parameters.Add("P_REFERENCE_ID", OracleDbType.Varchar2).Value = DbValue(model.ReferenceId, 200);
            cmd.Parameters.Add("P_FAILURE_DETAILS", OracleDbType.Clob).Value = string.IsNullOrWhiteSpace(model.FailureDetails) ? (object)DBNull.Value : model.FailureDetails;
            }

        private static EmailManagementLog MapManagedEmailLog(OracleDataReader reader)
            {
            return new EmailManagementLog
                {
                LogId = Convert.ToInt64(reader["LOG_ID"]),
                EventKey = reader["EVENT_KEY"]?.ToString() ?? string.Empty,
                TemplateId = reader["TEMPLATE_ID"] == DBNull.Value ? null : Convert.ToInt64(reader["TEMPLATE_ID"]),
                TemplateVersion = reader["TEMPLATE_VERSION"] == DBNull.Value ? null : Convert.ToInt32(reader["TEMPLATE_VERSION"]),
                SubjectSent = reader["SUBJECT_SENT"]?.ToString() ?? string.Empty,
                BodySent = reader["BODY_SENT"]?.ToString() ?? string.Empty,
                ToRecipients = reader["TO_RECIPIENTS"]?.ToString() ?? string.Empty,
                CcRecipients = reader["CC_RECIPIENTS"]?.ToString() ?? string.Empty,
                BccRecipients = reader["BCC_RECIPIENTS"]?.ToString() ?? string.Empty,
                AttachmentMetadata = reader["ATTACHMENT_METADATA"]?.ToString() ?? string.Empty,
                AttemptedOnUtc = ReadTimestampTzAsUtc(reader, "ATTEMPTED_ON_UTC"),
                Status = reader["STATUS"]?.ToString() ?? string.Empty,
                SmtpResponse = reader["SMTP_RESPONSE"]?.ToString() ?? string.Empty,
                AttemptNumber = Convert.ToInt32(reader["ATTEMPT_NUMBER"]),
                CallingComponent = reader["CALLING_COMPONENT"]?.ToString() ?? string.Empty,
                InitiatedBy = reader["INITIATED_BY"]?.ToString() ?? string.Empty,
                CorrelationId = reader["CORRELATION_ID"]?.ToString() ?? string.Empty,
                ReferenceId = reader["REFERENCE_ID"]?.ToString() ?? string.Empty,
                SentToSmtpOnUtc = reader["SENT_TO_SMTP_ON_UTC"] == DBNull.Value ? null : ReadTimestampTzAsUtc(reader, "SENT_TO_SMTP_ON_UTC"),
                FailureDetails = reader["FAILURE_DETAILS"]?.ToString() ?? string.Empty
                };
            }

        private static DateTimeOffset ToUtcOffset(DateTime value)
            {
            var local = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Local)
                : value;
            return new DateTimeOffset(local).ToUniversalTime();
            }

        private static DateTime ReadTimestampTzAsUtc(OracleDataReader reader, string columnName)
            {
            var ordinal = reader.GetOrdinal(columnName);
            var timestamp = reader.GetOracleTimeStampTZ(ordinal);
            return timestamp.IsNull
                ? DateTime.MinValue
                : DateTime.SpecifyKind(timestamp.Value, DateTimeKind.Local).ToUniversalTime();
            }
        }
    }
