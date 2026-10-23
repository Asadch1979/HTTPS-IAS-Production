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
        // rechecks the active IAS assignment and the authorized entity hierarchy.
        private List<T> ReadHeadAudit<T>(string procedure, Func<OracleDataReader, T> map,
            long? engagementId = null, string section = null, long? evidenceId = null, int? periodId = null, DateTime? positionAsOn = null)
        {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();
            if (session.IsSessionRevoked(user)) throw new UnauthenticatedException();
            if (!HeadAuditMonitoringPolicy.IsEligible(user)) throw new UnauthorizedAccessException();
            if (!GetTopMenuPages(user).Any(p => p.PageId == HeadAuditMonitoringPolicy.PageId))
                throw new UnauthorizedAccessException();
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
            if (periodId.HasValue) command.Parameters.Add("P_PERIOD_ID", OracleDbType.Int32).Value = periodId.Value;
            if (positionAsOn.HasValue) command.Parameters.Add("P_AS_OF", OracleDbType.Date).Value = positionAsOn.Value.Date;
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

        internal List<HeadAuditEngagement> GetHeadAuditEngagements(int? periodId = null, DateTime? positionAsOn = null)
        {
            var engagements = ReadHeadAudit("P_GET_ENGAGEMENTS", r => new HeadAuditEngagement
            {
            EngagementId = RoleLong(r, "ENG_ID"), TeamId = RoleNullableLong(r, "TEAM_ID"),
            OfficeId = RoleRequiredInt(r, "OFFICE_ID"), PeriodId = RoleRequiredInt(r, "PERIOD_ID"),
            OperationStart = RoleDate(r, "OPERATION_STARTDATE"),
            Entity = RoleText(r, "ENTITY_NAME"), Department = RoleText(r, "DEPARTMENT_NAME"),
            Team = RoleText(r, "TEAM_NAME"), Period = RoleText(r, "AUDIT_PERIOD"),
            Members = RoleText(r, "TEAM_MEMBERS"),
            StatusId = RoleRequiredInt(r, "STATUS_ID"), Status = RoleText(r, "STATUS_NAME"),
            Start = RoleDate(r, "AUDIT_STARTDATE"), End = RoleDate(r, "AUDIT_ENDDATE"),
            DaysToEnd = RoleNullableInt(r, "DAYS_TO_END"), AwaitingReview = RoleLong(r, "AWAITING_REVIEW"),
            TaskProgress = RoleText(r, "TASK_PROGRESS"), ObservationCounts = RoleText(r, "OBSERVATION_COUNTS"),
            Reports = RoleLong(r, "REPORT_COUNT"), ReportStatus = RoleText(r, "REPORT_STATUS")
            }, periodId: periodId, positionAsOn: positionAsOn);
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

        internal HeadAuditDashboard GetHeadAuditDashboard(int? auditPeriodId, int? officeId, int? statusId, string exception, DateTime? positionAsOn = null)
        {
            if (!HeadAuditDashboard.ValidException(exception) || auditPeriodId <= 0 || officeId <= 0 || statusId < 0)
                throw new ArgumentException("Invalid dashboard filters.");
            var periods = ReadHeadAudit("P_GET_PERIODS", r => new AuditPeriodModel {
                AUDITPERIODID = RoleRequiredInt(r,"AUDITPERIODID"), DESCRIPTION = RoleText(r,"DESCRIPTION"),
                START_DATE = RoleDate(r,"START_DATE") ?? DateTime.MinValue,
                END_DATE = RoleDate(r,"END_DATE") ?? DateTime.MinValue
            });
            var selected = auditPeriodId ?? HeadAuditDashboard.DefaultPeriod(periods, AuditParaSnapPolicy.Today);
            if (selected.HasValue && !periods.Any(p => p.AUDITPERIODID == selected))
                throw new ArgumentException("Select a valid IAS audit period.");
            var result = new HeadAuditDashboard { Periods=periods, AuditPeriodId=selected,
                OfficeId=officeId, StatusId=statusId, Exception=exception };
            if (!selected.HasValue)
            {
                if (officeId.HasValue) throw new UnauthorizedAccessException();
                return result;
            }
            result.PositionAsOn = HeadAuditDashboard.ReportingDate(periods.Single(p => p.AUDITPERIODID == selected), positionAsOn, AuditParaSnapPolicy.Today);
            result.Offices = ReadHeadAudit("P_GET_OFFICE_SUMMARY", r => new HeadAuditOfficeSummary {
                OfficeId=RoleRequiredInt(r,"OFFICE_ID"), Office=RoleText(r,"OFFICE_NAME"),
                Planned=RoleLong(r,"PLANNED"), PlannedEntities=RoleLong(r,"PLANNED_ENTITIES"),
                CompletedPlans=RoleLong(r,"COMPLETED_PLANS"), OngoingPlans=RoleLong(r,"ONGOING_PLANS"),
                NotStarted=RoleLong(r,"NOT_STARTED"), Completed=RoleLong(r,"COMPLETED"),
                Ongoing=RoleLong(r,"ONGOING"), Delayed=RoleLong(r,"DELAYED"),
                PendingReports=RoleLong(r,"PENDING_REPORTS"), AwaitingReview=RoleLong(r,"AWAITING_REVIEW")
            }, periodId:selected, positionAsOn:result.PositionAsOn);
            if (officeId.HasValue && !result.Offices.Any(o => o.OfficeId == officeId))
                throw new UnauthorizedAccessException();
            result.Engagements = GetHeadAuditEngagements(selected, result.PositionAsOn);
            result.Plans = ReadHeadAudit("P_GET_PLAN_DETAILS", r => new HeadAuditPlanDetail {
                PlanId=RoleLong(r,"PLAN_ID"), EngagementId=RoleNullableLong(r,"ENG_ID"),
                Entity=RoleText(r,"ENTITY_NAME"), Days=RoleNullableInt(r,"NO_OF_DAYS"),
                Start=RoleDate(r,"AUDIT_STARTDATE"), End=RoleDate(r,"AUDIT_ENDDATE"),
                Team=RoleText(r,"TEAM_NAME"), Stage=RoleText(r,"STAGE"), Status=RoleText(r,"PLAN_STATUS")
            }, periodId:selected, positionAsOn:result.PositionAsOn);
            return result;
        }

        internal List<HeadAuditWorkRecord> GetHeadAuditWork(long engagementId, string section)
        {
            if (engagementId <= 0 || !HeadAuditMonitoringPolicy.Sections.Contains(section)) throw new ArgumentException();
            if (new[] { "Plan", "Criteria", "Team" }.Contains(section)) section = "Tasks";
            var sections = section == "Workpapers" ? new[] { "WorkpapersLoan", "WorkpapersVoucher", "WorkpapersAccount", "WorkpapersAssets", "WorkpapersCash", "V2Headers", "V2Items", "V2Reviews", "V2Evidence" }
                : section == "History" ? new[] { "History", "ObservationReviews", "ObservationResponses" }
                : section == "DraftReport" ? new[] { "DraftReportMeta", "DraftReportText", "DraftReportParas", "DraftReportConclusion" } : new[] { section };
            return sections.SelectMany(part => ReadHeadAudit("P_GET_WORK", r => new HeadAuditWorkRecord
            {
                Id = RoleLong(r, "RECORD_ID"), Title = AIS.Services.HeadAuditReviewText.Plain(RoleText(r, "TITLE")), Status = RoleText(r, "STATUS_NAME"),
                Author = RoleText(r, "AUTHOR"), SavedOn = RoleDate(r, "SAVED_ON"), Content = AIS.Services.HeadAuditReviewText.Plain(RoleText(r, "CONTENT")),
                Start = part == "Tasks" ? RoleDate(r,"START_DATE") : null,
                End = part == "Tasks" ? RoleDate(r,"END_DATE") : null,
                Memo = part == "Observations" ? RoleText(r,"MEMO_NO") : null,
                DraftPara = part == "Observations" ? RoleText(r,"DRAFT_PARA_NO") : null,
                FinalPara = part == "Observations" ? RoleText(r,"FINAL_PARA_NO") : null,
                Risk = part == "Observations" ? RoleText(r,"RISK_NAME") : null
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
