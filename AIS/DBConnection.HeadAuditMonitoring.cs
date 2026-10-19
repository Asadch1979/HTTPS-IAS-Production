using AIS.Models;
using AIS.Session;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        // No department, role or PPNO can be supplied by the browser. Oracle independently
        // rechecks the active IAS assignment and the verified role/department mapping.
        private List<T> ReadHeadAudit<T>(string procedure, Func<OracleDataReader, T> map,
            long? engagementId = null, string section = null, long? evidenceId = null)
        {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();
            if (session.IsSessionRevoked(user)) throw new UnauthenticatedException();
            if (!HeadAuditMonitoringPolicy.IsEligible(user)) throw new UnauthorizedAccessException();
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PKG_HEAD_AUDIT_MONITOR." + procedure;
            command.BindByName = true;
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("P_USER_ID", OracleDbType.Int32).Value = user.ID;
            command.Parameters.Add("P_ROLE_ID", OracleDbType.Int32).Value = user.UserRoleID;
            command.Parameters.Add("P_DEPT_ID", OracleDbType.Int32).Value = user.UserEntityID.Value;
            command.Parameters.Add("P_CONTEXT_ID", OracleDbType.Int32).Value = (object)user.UserContextAssignmentId ?? DBNull.Value;
            if (engagementId.HasValue) command.Parameters.Add("P_ENG_ID", OracleDbType.Int64).Value = engagementId.Value;
            if (section != null) command.Parameters.Add("P_SECTION", OracleDbType.Varchar2).Value = section;
            if (evidenceId.HasValue) command.Parameters.Add("P_EVIDENCE_ID", OracleDbType.Int64).Value = evidenceId.Value;
            command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            var rows = new List<T>();
            using var reader = command.ExecuteReader();
            while (reader.Read()) rows.Add(map(reader));
            var current = session.GetUserOrThrow();
            if (session.IsSessionRevoked(current) || current.ID != user.ID || current.SessionId != user.SessionId ||
                current.UserContextAssignmentId != user.UserContextAssignmentId || current.UserRoleID != user.UserRoleID ||
                current.UserEntityID != user.UserEntityID) throw new UnauthenticatedException();
            return rows;
        }

        internal List<HeadAuditEngagement> GetHeadAuditEngagements()
        {
            var engagements = ReadHeadAudit("P_GET_ENGAGEMENTS", r => new HeadAuditEngagement
            {
            EngagementId = RoleLong(r, "ENG_ID"), TeamId = RoleNullableLong(r, "TEAM_ID"),
            Entity = RoleText(r, "ENTITY_NAME"), Department = RoleText(r, "DEPARTMENT_NAME"),
            Team = RoleText(r, "TEAM_NAME"), Period = RoleText(r, "AUDIT_PERIOD"),
            Members = RoleText(r, "TEAM_MEMBERS"),
            StatusId = RoleRequiredInt(r, "STATUS_ID"), Status = RoleText(r, "STATUS_NAME"),
            Start = RoleDate(r, "AUDIT_STARTDATE"), End = RoleDate(r, "AUDIT_ENDDATE"),
            DaysToEnd = RoleNullableInt(r, "DAYS_TO_END"), AwaitingReview = RoleLong(r, "AWAITING_REVIEW"),
            TaskProgress = RoleText(r, "TASK_PROGRESS"), ObservationCounts = RoleText(r, "OBSERVATION_COUNTS"),
            Reports = RoleLong(r, "REPORT_COUNT"), ReportStatus = RoleText(r, "REPORT_STATUS")
            });
            var counts = ReadHeadAudit("P_GET_WP_COUNTS", r => new
            {
                EngId = RoleLong(r, "ENG_ID"), Review = RoleNullableLong(r, "AWAITING_REVIEW"),
                Progress = RoleText(r, "ITEM_PROGRESS")
            }).ToDictionary(r => r.EngId);
            foreach (var engagement in engagements)
                if (counts.TryGetValue(engagement.EngagementId, out var count))
                {
                    engagement.WorkpapersAwaitingReview = count.Review;
                    engagement.WorkpaperProgress = count.Progress;
                }
            return engagements;
        }

        internal List<HeadAuditWorkRecord> GetHeadAuditWork(long engagementId, string section)
        {
            if (engagementId <= 0 || !HeadAuditMonitoringPolicy.Sections.Contains(section)) throw new ArgumentException();
            var sections = section == "Workpapers" ? new[] { "WorkpapersLoan", "WorkpapersVoucher", "WorkpapersAccount", "WorkpapersAssets", "WorkpapersCash", "V2Headers", "V2Items", "V2Reviews", "V2Evidence" }
                : section == "History" ? new[] { "History", "ObservationReviews", "ObservationResponses" }
                : section == "DraftReport" ? new[] { "DraftReportMeta", "DraftReportText", "DraftReportParas", "DraftReportConclusion" } : new[] { section };
            return sections.SelectMany(part => ReadHeadAudit("P_GET_WORK", r => new HeadAuditWorkRecord
            {
                Id = RoleLong(r, "RECORD_ID"), Title = RoleText(r, "TITLE"), Status = RoleText(r, "STATUS_NAME"),
                Author = RoleText(r, "AUTHOR"), SavedOn = RoleDate(r, "SAVED_ON"), Content = RoleText(r, "CONTENT")
            }, engagementId, part)).ToList();
        }

        internal HeadAuditDocument GetHeadAuditDocument(long engagementId, long documentId, bool report, bool responseEvidence = false)
        {
            if (engagementId <= 0 || documentId <= 0) throw new ArgumentException();
            return ReadHeadAudit(report ? "P_GET_REPORT" : responseEvidence ? "P_GET_RESPONSE_EVIDENCE" : "P_GET_EVIDENCE", r =>
            {
                if (r.IsDBNull(0)) return null;
                var bytes = report || responseEvidence ? Convert.FromBase64String(r.GetString(0)) : r.GetOracleBlob(0).Value;
                return new HeadAuditDocument { Bytes = bytes, Name = System.IO.Path.GetFileName(RoleText(r, "FILE_NAME")) };
            }, engagementId, evidenceId: documentId).SingleOrDefault();
        }
    }
}
