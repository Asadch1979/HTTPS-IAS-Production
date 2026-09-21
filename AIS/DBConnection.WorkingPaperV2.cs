using AIS.Models.WorkingPaperV2;
using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public WorkingPaperWorkspaceModel GetWorkingPaperWorkspace(long workingPaperId, long engagementId, string paperType)
            {
            var user = RequireWorkingPaperUser();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_WORKING_PAPER.P_GET_WORKSPACE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            AddContext(cmd, user);
            cmd.Parameters.Add("P_WP_ID", OracleDbType.Int64).Value = workingPaperId > 0 ? workingPaperId : DBNull.Value;
            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int64).Value = engagementId;
            cmd.Parameters.Add("P_PAPER_TYPE", OracleDbType.Varchar2, 3).Value = paperType;
            foreach (var name in new[] { "O_HEADER", "O_ITEMS", "O_EVIDENCE", "O_EXCEPTIONS", "O_NOTES", "O_HISTORY", "O_LEGACY" })
                cmd.Parameters.Add(name, OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            var workspace = new WorkingPaperWorkspaceModel { EngagementId = engagementId, PaperType = paperType };
            using var reader = cmd.ExecuteReader();
            ReadHeader(reader, workspace);
            if (reader.NextResult()) ReadItems(reader, workspace.Items);
            if (reader.NextResult()) ReadEvidence(reader, workspace.Evidence);
            if (reader.NextResult()) ReadExceptions(reader, workspace.Exceptions);
            if (reader.NextResult()) ReadNotes(reader, workspace.ReviewNotes);
            if (reader.NextResult()) ReadHistory(reader, workspace.History);
            if (reader.NextResult()) ReadLegacy(reader, workspace.LegacyRecords);
            return workspace;
            }

        public WorkingPaperOperationResult CreateWorkingPaper(CreateWorkingPaperRequest request)
            => ExecuteWorkingPaperResult("PKG_WORKING_PAPER.P_CREATE", cmd =>
            {
                cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int64).Value = request.EngagementId;
                cmd.Parameters.Add("P_PAPER_TYPE", OracleDbType.Varchar2, 3).Value = request.PaperType;
                cmd.Parameters.Add("P_REVIEWER_PPNO", OracleDbType.Varchar2, 30).Value = request.ReviewerPpno;
                cmd.Parameters.Add("P_DUE_DATE", OracleDbType.Date).Value = (object)request.DueDate ?? DBNull.Value;
            });

        public WorkingPaperOperationResult SaveWorkingPaperPlan(SavePlanRequest request)
            => ExecuteWorkingPaperResult("PKG_WORKING_PAPER.P_SAVE_PLAN", cmd =>
            {
                cmd.Parameters.Add("P_WP_ID", OracleDbType.Int64).Value = request.WorkingPaperId;
                cmd.Parameters.Add("P_ROW_VERSION", OracleDbType.Int64).Value = request.RowVersion;
                cmd.Parameters.Add("P_PLAN_JSON", OracleDbType.Clob).Value = JsonSerializer.Serialize(request.Plan);
            });

        public WorkingPaperOperationResult SaveWorkingPaperItem(SaveItemRequest request, IReadOnlyDictionary<string, decimal> calculations)
            => ExecuteWorkingPaperResult("PKG_WORKING_PAPER.P_SAVE_ITEM", cmd =>
            {
                cmd.Parameters.Add("P_WP_ID", OracleDbType.Int64).Value = request.WorkingPaperId;
                cmd.Parameters.Add("P_ITEM_ID", OracleDbType.Int64).Value = request.ItemId > 0 ? request.ItemId : DBNull.Value;
                cmd.Parameters.Add("P_ITEM_REF", OracleDbType.Varchar2, 120).Value = request.ItemReference;
                cmd.Parameters.Add("P_DETAILS_JSON", OracleDbType.Clob).Value = request.Details.GetRawText();
                cmd.Parameters.Add("P_CALC_JSON", OracleDbType.Clob).Value = JsonSerializer.Serialize(calculations);
                cmd.Parameters.Add("P_RESULT", OracleDbType.Varchar2, 30).Value = request.Result;
                cmd.Parameters.Add("P_COMMENT", OracleDbType.Varchar2, 4000).Value = (object)request.AuditorComment ?? DBNull.Value;
                cmd.Parameters.Add("P_ROW_VERSION", OracleDbType.Int64).Value = request.RowVersion;
            });

        public WorkingPaperOperationResult SaveWorkingPaperEvidence(WorkingPaperEvidenceModel model)
            => ExecuteWorkingPaperResult("PKG_WORKING_PAPER.P_LINK_EVIDENCE", cmd =>
            {
                cmd.Parameters.Add("P_WP_ID", OracleDbType.Int64).Value = model.WorkingPaperId;
                cmd.Parameters.Add("P_ITEM_ID", OracleDbType.Int64).Value = (object)model.ItemId ?? DBNull.Value;
                cmd.Parameters.Add("P_EXCEPTION_ID", OracleDbType.Int64).Value = (object)model.ExceptionId ?? DBNull.Value;
                cmd.Parameters.Add("P_EXISTING_EVIDENCE_ID", OracleDbType.Varchar2, 100).Value = model.ExistingEvidenceId;
                cmd.Parameters.Add("P_TITLE", OracleDbType.Varchar2, 300).Value = model.Title;
                cmd.Parameters.Add("P_EVIDENCE_TYPE", OracleDbType.Varchar2, 100).Value = (object)model.EvidenceType ?? DBNull.Value;
                cmd.Parameters.Add("P_SOURCE", OracleDbType.Varchar2, 500).Value = (object)model.Source ?? DBNull.Value;
                cmd.Parameters.Add("P_EVIDENCE_DATE", OracleDbType.Date).Value = (object)model.EvidenceDate ?? DBNull.Value;
            });

        public WorkingPaperOperationResult SaveWorkingPaperException(WorkingPaperExceptionModel model)
            => ExecuteWorkingPaperResult("PKG_WORKING_PAPER.P_SAVE_EXCEPTION", cmd =>
            {
                cmd.Parameters.Add("P_WP_ID", OracleDbType.Int64).Value = model.WorkingPaperId;
                cmd.Parameters.Add("P_EXCEPTION_ID", OracleDbType.Int64).Value = model.ExceptionId > 0 ? model.ExceptionId : DBNull.Value;
                cmd.Parameters.Add("P_ITEM_ID", OracleDbType.Int64).Value = model.ItemId;
                cmd.Parameters.Add("P_CRITERIA", OracleDbType.Varchar2, 4000).Value = model.Criteria;
                cmd.Parameters.Add("P_CONDITION", OracleDbType.Varchar2, 4000).Value = model.Condition;
                cmd.Parameters.Add("P_CAUSE", OracleDbType.Varchar2, 4000).Value = (object)model.Cause ?? DBNull.Value;
                cmd.Parameters.Add("P_IMPACT", OracleDbType.Varchar2, 4000).Value = model.Impact;
                cmd.Parameters.Add("P_RISK", OracleDbType.Varchar2, 20).Value = model.RiskRating;
                cmd.Parameters.Add("P_OWNER_PPNO", OracleDbType.Varchar2, 30).Value = (object)model.OwnerPpno ?? DBNull.Value;
                cmd.Parameters.Add("P_DUE_DATE", OracleDbType.Date).Value = (object)model.DueDate ?? DBNull.Value;
                cmd.Parameters.Add("P_OBSERVATION_ID", OracleDbType.Int64).Value = (object)model.ObservationId ?? DBNull.Value;
                cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2, 20).Value = string.IsNullOrWhiteSpace(model.Status) ? "OPEN" : model.Status.ToUpperInvariant();
                cmd.Parameters.Add("P_ROW_VERSION", OracleDbType.Int64).Value = model.RowVersion;
            });

        public WorkingPaperOperationResult SaveWorkingPaperConclusion(SaveConclusionRequest request)
            => ExecuteWorkingPaperResult("PKG_WORKING_PAPER.P_SAVE_CONCLUSION", cmd =>
            {
                cmd.Parameters.Add("P_WP_ID", OracleDbType.Int64).Value = request.WorkingPaperId;
                cmd.Parameters.Add("P_ROW_VERSION", OracleDbType.Int64).Value = request.RowVersion;
                cmd.Parameters.Add("P_CONCLUSION_JSON", OracleDbType.Clob).Value = JsonSerializer.Serialize(request.Conclusion);
            });

        public WorkingPaperOperationResult SaveWorkingPaperReviewNote(WorkingPaperReviewNoteModel model)
            => ExecuteWorkingPaperResult("PKG_WORKING_PAPER.P_SAVE_REVIEW_NOTE", cmd =>
            {
                cmd.Parameters.Add("P_WP_ID", OracleDbType.Int64).Value = model.WorkingPaperId;
                cmd.Parameters.Add("P_NOTE_ID", OracleDbType.Int64).Value = model.ReviewNoteId > 0 ? model.ReviewNoteId : DBNull.Value;
                cmd.Parameters.Add("P_SECTION_KEY", OracleDbType.Varchar2, 100).Value = model.SectionKey;
                cmd.Parameters.Add("P_NOTE_TEXT", OracleDbType.Varchar2, 4000).Value = model.NoteText;
                cmd.Parameters.Add("P_RESPONSE_TEXT", OracleDbType.Varchar2, 4000).Value = (object)model.ResponseText ?? DBNull.Value;
                cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2, 20).Value = model.Status ?? "OPEN";
            });

        public WorkingPaperOperationResult ApplyWorkingPaperWorkflow(WorkflowActionRequest request)
            => ExecuteWorkingPaperResult("PKG_WORKING_PAPER.P_WORKFLOW", cmd =>
            {
                cmd.Parameters.Add("P_WP_ID", OracleDbType.Int64).Value = request.WorkingPaperId;
                cmd.Parameters.Add("P_ACTION", OracleDbType.Varchar2, 20).Value = request.Action;
                cmd.Parameters.Add("P_REMARKS", OracleDbType.Varchar2, 2000).Value = (object)request.Remarks ?? DBNull.Value;
                cmd.Parameters.Add("P_ROW_VERSION", OracleDbType.Int64).Value = request.RowVersion;
            });

        private WorkingPaperOperationResult ExecuteWorkingPaperResult(string procedure, Action<OracleCommand> addParameters)
            {
            var user = RequireWorkingPaperUser();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = procedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            AddContext(cmd, user);
            addParameters(cmd);
            cmd.Parameters.Add("O_RESULT", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return new WorkingPaperOperationResult { Message = "No result returned." };
            return new WorkingPaperOperationResult
            {
                Success = string.Equals(Text(reader, "STATUS"), "SUCCESS", StringComparison.OrdinalIgnoreCase),
                Message = Text(reader, "MESSAGE"),
                WorkingPaperId = Long(reader, "WP_ID"),
                RecordId = Long(reader, "RECORD_ID"),
                RowVersion = Long(reader, "ROW_VERSION")
            };
            }

        private SessionUser RequireWorkingPaperUser()
            {
            var user = sessionHandler.GetUser();
            if (user == null || user.UserEntityID.GetValueOrDefault() <= 0 || user.UserRoleID <= 0 || string.IsNullOrWhiteSpace(user.PPNumber))
                throw new UnauthorizedAccessException("A valid IAS session is required.");
            return user;
            }

        private static void AddContext(OracleCommand cmd, SessionUser user)
            {
            cmd.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = user.UserEntityID.Value;
            cmd.Parameters.Add("P_ACTOR_PPNO", OracleDbType.Varchar2, 30).Value = user.PPNumber;
            cmd.Parameters.Add("P_ROLE_ID", OracleDbType.Int32).Value = user.UserRoleID;
            }

        private static void ReadHeader(OracleDataReader reader, WorkingPaperWorkspaceModel w)
            {
            if (!reader.Read()) return;
            w.WorkingPaperId = Long(reader, "WP_ID"); w.EngagementId = Long(reader, "ENG_ID"); w.EntityId = Int(reader, "ENTITY_ID");
            w.PaperType = Text(reader, "PAPER_TYPE"); w.ReferenceNo = Text(reader, "REFERENCE_NO"); w.VersionNo = Int(reader, "VERSION_NO");
            w.Status = Text(reader, "STATUS"); w.PreparerPpno = Text(reader, "PREPARER_PPNO"); w.ReviewerPpno = Text(reader, "REVIEWER_PPNO");
            w.DueDate = Date(reader, "DUE_DATE"); w.RowVersion = Long(reader, "ROW_VERSION");
            w.Plan = Deserialize<WorkingPaperPlanModel>(Text(reader, "PLAN_JSON")) ?? new WorkingPaperPlanModel();
            w.Conclusion = Deserialize<WorkingPaperConclusionModel>(Text(reader, "CONCLUSION_JSON")) ?? new WorkingPaperConclusionModel();
            }

        private static void ReadItems(OracleDataReader r, List<WorkingPaperItemModel> list)
            { while (r.Read()) list.Add(new WorkingPaperItemModel { ItemId = Long(r, "ITEM_ID"), WorkingPaperId = Long(r, "WP_ID"), ItemReference = Text(r, "ITEM_REFERENCE"), Details = JsonDocument.Parse(Text(r, "DETAILS_JSON") ?? "{}").RootElement.Clone(), Result = Text(r, "RESULT"), AuditorComment = Text(r, "AUDITOR_COMMENT"), RowVersion = Long(r, "ROW_VERSION") }); }
        private static void ReadEvidence(OracleDataReader r, List<WorkingPaperEvidenceModel> list)
            { while (r.Read()) list.Add(new WorkingPaperEvidenceModel { EvidenceLinkId = Long(r, "EVIDENCE_LINK_ID"), WorkingPaperId = Long(r, "WP_ID"), ItemId = NullableLong(r, "ITEM_ID"), ExceptionId = NullableLong(r, "EXCEPTION_ID"), ExistingEvidenceId = Text(r, "EXISTING_EVIDENCE_ID"), Title = Text(r, "TITLE"), EvidenceType = Text(r, "EVIDENCE_TYPE"), Source = Text(r, "SOURCE_NAME"), EvidenceDate = Date(r, "EVIDENCE_DATE") }); }
        private static void ReadExceptions(OracleDataReader r, List<WorkingPaperExceptionModel> list)
            { while (r.Read()) list.Add(new WorkingPaperExceptionModel { ExceptionId = Long(r, "EXCEPTION_ID"), WorkingPaperId = Long(r, "WP_ID"), ItemId = Long(r, "ITEM_ID"), Criteria = Text(r, "CRITERIA"), Condition = Text(r, "CONDITION"), Cause = Text(r, "CAUSE"), Impact = Text(r, "IMPACT"), RiskRating = Text(r, "RISK_RATING"), OwnerPpno = Text(r, "OWNER_PPNO"), DueDate = Date(r, "DUE_DATE"), ObservationId = NullableLong(r, "OBSERVATION_ID"), Status = Text(r, "STATUS"), RowVersion = Long(r, "ROW_VERSION") }); }
        private static void ReadNotes(OracleDataReader r, List<WorkingPaperReviewNoteModel> list)
            { while (r.Read()) list.Add(new WorkingPaperReviewNoteModel { ReviewNoteId = Long(r, "NOTE_ID"), WorkingPaperId = Long(r, "WP_ID"), SectionKey = Text(r, "SECTION_KEY"), NoteText = Text(r, "NOTE_TEXT"), ResponseText = Text(r, "RESPONSE_TEXT"), Status = Text(r, "STATUS"), RaisedBy = Text(r, "RAISED_BY"), RaisedOn = Date(r, "RAISED_ON") }); }
        private static void ReadHistory(OracleDataReader r, List<WorkingPaperHistoryModel> list)
            { while (r.Read()) list.Add(new WorkingPaperHistoryModel { HistoryId = Long(r, "HISTORY_ID"), Action = Text(r, "ACTION"), ActorPpno = Text(r, "ACTOR_PPNO"), ActionOn = Date(r, "ACTION_ON") ?? DateTime.MinValue, Details = Text(r, "DETAILS") }); }
        private static void ReadLegacy(OracleDataReader r, List<WorkingPaperLegacyRecordModel> list)
            { while (r.Read()) list.Add(new WorkingPaperLegacyRecordModel { SourceTable = Text(r, "SOURCE_TABLE"), SourceId = Text(r, "SOURCE_ID"), DisplayReference = Text(r, "DISPLAY_REFERENCE"), LegacyValuesJson = Text(r, "LEGACY_VALUES_JSON") }); }

        private static T Deserialize<T>(string json) where T : class => string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        private static string Text(OracleDataReader r, string name) => r[name] == DBNull.Value ? null : Convert.ToString(r[name]);
        private static long Long(OracleDataReader r, string name) => r[name] == DBNull.Value ? 0 : Convert.ToInt64(r[name]);
        private static long? NullableLong(OracleDataReader r, string name) => r[name] == DBNull.Value ? null : Convert.ToInt64(r[name]);
        private static int Int(OracleDataReader r, string name) => r[name] == DBNull.Value ? 0 : Convert.ToInt32(r[name]);
        private static DateTime? Date(OracleDataReader r, string name) => r[name] == DBNull.Value ? null : Convert.ToDateTime(r[name]);
        }
    }
