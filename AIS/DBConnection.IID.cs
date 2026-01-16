using AIS.Models.IID;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public int SubmitComplaint(ComplaintModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_COMPLAINT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_nature", OracleDbType.Varchar2).Value = model.Nature ?? string.Empty;
                cmd.Parameters.Add("p_category", OracleDbType.Varchar2).Value = model.Category ?? string.Empty;
                cmd.Parameters.Add("p_source", OracleDbType.Varchar2).Value = model.Source ?? string.Empty;
                cmd.Parameters.Add("p_source_other_text", OracleDbType.Varchar2).Value = model.SourceOtherText ?? string.Empty;
                cmd.Parameters.Add("p_pertains_to", OracleDbType.Varchar2).Value = model.PertainsTo ?? string.Empty;
                cmd.Parameters.Add("p_field_type", OracleDbType.Varchar2).Value = model.FieldType ?? string.Empty;
                cmd.Parameters.Add("p_ho_unit_type_id", OracleDbType.Int32).Value = (object?)model.HOUnitTypeId ?? DBNull.Value;
                cmd.Parameters.Add("p_ho_unit_id", OracleDbType.Int32).Value = (object?)model.HOUnitId ?? DBNull.Value;
                cmd.Parameters.Add("p_region_id", OracleDbType.Int32).Value = (object?)model.RegionId ?? DBNull.Value;
                cmd.Parameters.Add("p_branch_id", OracleDbType.Int32).Value = (object?)model.BranchId ?? DBNull.Value;
                cmd.Parameters.Add("p_contents", OracleDbType.Clob).Value = model.Contents ?? string.Empty;
                cmd.Parameters.Add("p_uploaded_complaint", OracleDbType.Varchar2).Value = model.UploadedComplaint ?? string.Empty;
                cmd.Parameters.Add("p_uploaded_ffr", OracleDbType.Varchar2).Value = model.UploadedFFR ?? string.Empty;
                cmd.Parameters.Add("p_uploaded_evidence", OracleDbType.Varchar2).Value = model.UploadedEvidence ?? string.Empty;
                cmd.Parameters.Add("p_action_required", OracleDbType.Varchar2).Value = model.ActionRequired ?? string.Empty;
                cmd.Parameters.Add("p_submitted_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("o_complaint_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = Convert.ToInt32(cmd.Parameters["o_complaint_id"].Value.ToString());
                return id;
                }
            }

        public DataTable GetComplaintsByUser()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_COMPLAINTS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var dt = new DataTable();
                using (var rdr = cmd.ExecuteReader())
                    {
                    dt.Load(rdr);
                    }
                return dt;
                }
            }

        public List<InitialAssessmentModel> Get_Complaints_Without_Assessment()
            {
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_COMPLAINTS_WITHOUT_ASSESSMENT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var list = new List<InitialAssessmentModel>();
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        list.Add(new InitialAssessmentModel
                            {
                            ComplaintId = rdr["COMPLAINT_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["COMPLAINT_ID"]),
                            Nature = rdr["NATURE"].ToString(),
                            SubmittedOn = rdr["SUBMITTED_ON"].ToString()
                            });
                        }
                    }
                return list;
                }
            }

        public InitialAssessmentModel GetComplaint(int complaintId)
            {
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_COMPLAINT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                InitialAssessmentModel model = null;
                using (var rdr = cmd.ExecuteReader())
                    {
                    if (rdr.Read())
                        {
                        model = new InitialAssessmentModel
                            {
                            ComplaintId = complaintId,
                            Nature = rdr["NATURE"].ToString(),
                            Contents = rdr["CONTENTS"].ToString(),
                            UploadedComplaint = rdr["UPLOADED_COMPLAINT"].ToString(),
                            UploadedFFR = rdr["UPLOADED_FFR"].ToString(),
                            UploadedEvidence = rdr["UPLOADED_EVIDENCE"].ToString(),
                            ActionRequired = rdr["ACTION_REQUIRED"].ToString(),
                            SubmittedOn = rdr["SUBMITTED_ON"].ToString()
                            };
                        }
                    }
                return model;
                }
            }

        public int AddAssessment(InitialAssessmentModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_ASSESSMENT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = model.ComplaintId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_received_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("p_assessment", OracleDbType.Clob).Value = model.Assessment ?? string.Empty;
                cmd.Parameters.Add("p_recommendation", OracleDbType.Varchar2).Value = model.Recommendation ?? string.Empty;
                cmd.Parameters.Add("o_assessment_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = Convert.ToInt32(cmd.Parameters["o_assessment_id"].Value.ToString());
                return id;
                }
            }

        public int AddHeadReview(HeadReviewModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_HEAD_REVIEW";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = model.ComplaintId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_assessment_id", OracleDbType.Int32).Value = model.AssessmentId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_reviewed_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("p_directions", OracleDbType.Clob).Value = model.Directions ?? string.Empty;
                cmd.Parameters.Add("p_assigned_to_unit", OracleDbType.Int32).Value = model.AssignedToUnit == 0 ? (object)DBNull.Value : (object)model.AssignedToUnit;
                cmd.Parameters.Add("p_team_lead", OracleDbType.Int32).Value = model.TeamLeadId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_team_members", OracleDbType.Clob).Value = model.TeamMembers ?? string.Empty;
                cmd.Parameters.Add("p_assigned_on", OracleDbType.Varchar2).Value = model.AssignedOn ?? string.Empty;
                cmd.Parameters.Add("p_due_date", OracleDbType.Varchar2).Value = model.DueDate ?? string.Empty;
                cmd.Parameters.Add("p_referred_back_comments", OracleDbType.Clob).Value = model.ReferredBackComments ?? string.Empty;
                cmd.Parameters.Add("p_action", OracleDbType.Varchar2).Value = model.Action ?? string.Empty;
                cmd.Parameters.Add("o_review_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = Convert.ToInt32(cmd.Parameters["o_review_id"].Value.ToString());
                return id;
                }
            }

        public int AddInvestigationPlan(InvestigationPlanModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_INV_PLAN";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = model.ComplaintId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_plan_details", OracleDbType.Clob).Value = model.PlanDetails ?? string.Empty;
                cmd.Parameters.Add("p_submitted_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("p_status", OracleDbType.Varchar2).Value = model.Status ?? string.Empty;
                cmd.Parameters.Add("o_plan_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = Convert.ToInt32(cmd.Parameters["o_plan_id"].Value.ToString());
                return id;
                }
            }

        public int AddPlanApproval(PlanApprovalModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_PLAN_APPROVAL";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_plan_id", OracleDbType.Int32).Value = model.PlanId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_approved_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("p_is_approved", OracleDbType.Varchar2).Value = model.IsApproved ?? string.Empty;
                cmd.Parameters.Add("p_edited_plan", OracleDbType.Clob).Value = model.EditedPlan ?? string.Empty;
                cmd.Parameters.Add("p_further_actions", OracleDbType.Clob).Value = model.FurtherActions ?? string.Empty;
                cmd.Parameters.Add("o_approval_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = Convert.ToInt32(cmd.Parameters["o_approval_id"].Value.ToString());
                return id;
                }
            }

        public int AddInquiryReport(InquiryReportModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_INQUIRY_REPORT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                var submittedOn = DateTime.Now;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = model.ComplaintId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_name_complainant", OracleDbType.Varchar2).Value = model.NameComplainant ?? string.Empty;
                cmd.Parameters.Add("p_name_accused", OracleDbType.Varchar2).Value = model.NameAccused ?? string.Empty;
                cmd.Parameters.Add("p_gist", OracleDbType.Clob).Value = model.Gist ?? string.Empty;
                cmd.Parameters.Add("p_proceedings", OracleDbType.Clob).Value = model.Proceedings ?? string.Empty;
                cmd.Parameters.Add("p_findings", OracleDbType.Clob).Value = model.Findings ?? string.Empty;
                cmd.Parameters.Add("p_recommendation", OracleDbType.Clob).Value = model.Recommendation ?? string.Empty;
                cmd.Parameters.Add("p_uploaded_report", OracleDbType.Varchar2).Value = model.UploadedReport ?? string.Empty;
                cmd.Parameters.Add("p_uploaded_evidence", OracleDbType.Varchar2).Value = model.UploadedEvidence ?? string.Empty;
                cmd.Parameters.Add("p_uploaded_dsa", OracleDbType.Varchar2).Value = model.UploadedDsa ?? string.Empty;
                cmd.Parameters.Add("p_submitted_on", OracleDbType.Date).Value = submittedOn;
                cmd.Parameters.Add("p_submitted_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("o_report_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = Convert.ToInt32(cmd.Parameters["o_report_id"].Value.ToString());
                return id;
                }
            }

        public int AddAnalysis(AnalysisModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_ANALYSIS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_report_id", OracleDbType.Int32).Value = model.ReportId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_policy_gaps", OracleDbType.Clob).Value = model.PolicyGaps ?? string.Empty;
                cmd.Parameters.Add("p_control_gaps", OracleDbType.Clob).Value = model.ControlGaps ?? string.Empty;
                cmd.Parameters.Add("p_procedural_violations", OracleDbType.Clob).Value = model.ProceduralViolations ?? string.Empty;
                cmd.Parameters.Add("p_forward_to", OracleDbType.Varchar2).Value = model.ForwardTo ?? string.Empty;
                cmd.Parameters.Add("p_comments", OracleDbType.Clob).Value = model.Comments ?? string.Empty;
                cmd.Parameters.Add("p_decision", OracleDbType.Varchar2).Value = model.Decision ?? string.Empty;
                cmd.Parameters.Add("p_refer_back_comments", OracleDbType.Clob).Value = model.ReferBackComments ?? string.Empty;
                cmd.Parameters.Add("p_analyzed_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("o_analysis_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = Convert.ToInt32(cmd.Parameters["o_analysis_id"].Value.ToString());
                return id;
                }
            }

        public int AddFinalApproval(FinalApprovalModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_FINAL_APPROVAL";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_report_id", OracleDbType.Int32).Value = model.ReportId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_comments", OracleDbType.Clob).Value = model.Comments ?? string.Empty;
                cmd.Parameters.Add("p_approved", OracleDbType.Varchar2).Value = model.Decision ?? string.Empty;
                cmd.Parameters.Add("p_approved_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("o_final_approval_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = Convert.ToInt32(cmd.Parameters["o_final_approval_id"].Value.ToString());
                return id;
                }
            }

        public InquiryReportFilesModel GetInquiryReportFiles(int reportId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_INQUIRY_REPORT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_report_id", OracleDbType.Int32).Value = reportId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                InquiryReportFilesModel model = null;
                using (var rdr = cmd.ExecuteReader())
                    {
                    if (rdr.Read())
                        {
                        model = new InquiryReportFilesModel
                            {
                            ReportId = reportId,
                            UploadedReport = rdr["UPLOADED_REPORT"].ToString(),
                            UploadedEvidence = rdr["UPLOADED_EVIDENCE"].ToString(),
                            UploadedDsa = rdr["UPLOADED_DSA"].ToString()
                            };
                        }
                    }
                return model;
                }
            }

        public int AddCaseStudy(CaseStudyModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_CASE_STUDY";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = model.ComplaintId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_origin_process_owner", OracleDbType.Varchar2).Value = model.OriginProcessOwner ?? string.Empty;
                cmd.Parameters.Add("p_name_complainant", OracleDbType.Varchar2).Value = model.NameComplainant ?? string.Empty;
                cmd.Parameters.Add("p_branch", OracleDbType.Varchar2).Value = model.Branch ?? string.Empty;
                cmd.Parameters.Add("p_gist", OracleDbType.Clob).Value = model.Gist ?? string.Empty;
                cmd.Parameters.Add("p_outcome", OracleDbType.Clob).Value = model.Outcome ?? string.Empty;
                cmd.Parameters.Add("p_modus_operandi", OracleDbType.Clob).Value = model.ModusOperandi ?? string.Empty;
                cmd.Parameters.Add("p_gaps", OracleDbType.Clob).Value = model.Gaps ?? string.Empty;
                cmd.Parameters.Add("p_root_cause", OracleDbType.Clob).Value = model.RootCause ?? string.Empty;
                cmd.Parameters.Add("p_actions_rec", OracleDbType.Clob).Value = model.ActionsRec ?? string.Empty;
                cmd.Parameters.Add("p_status", OracleDbType.Varchar2).Value = model.Status ?? string.Empty;
                cmd.Parameters.Add("p_policy_gaps_identified", OracleDbType.Clob).Value = model.PolicyGapsIdentified ?? string.Empty;
                cmd.Parameters.Add("p_control_violations", OracleDbType.Clob).Value = model.ControlViolations ?? string.Empty;
                cmd.Parameters.Add("p_risk_identified", OracleDbType.Clob).Value = model.RiskIdentified ?? string.Empty;
                cmd.Parameters.Add("p_reg_compliance_failure", OracleDbType.Clob).Value = model.RegulatoryComplianceFailure ?? string.Empty;
                cmd.Parameters.Add("o_case_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = Convert.ToInt32(cmd.Parameters["o_case_id"].Value.ToString());
                return id;
                }
            }

        public DataTable GetReports(ReportFilterModel filter)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_REPORTS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_filter", OracleDbType.Varchar2).Value = filter?.Nature ?? string.Empty;
                cmd.Parameters.Add("p_source", OracleDbType.Varchar2).Value = filter?.Source ?? string.Empty;
                cmd.Parameters.Add("p_category", OracleDbType.Varchar2).Value = filter?.Category ?? string.Empty;
                cmd.Parameters.Add("p_pertains_to", OracleDbType.Varchar2).Value = filter?.PertainsTo ?? string.Empty;
                cmd.Parameters.Add("p_date_from", OracleDbType.Varchar2).Value = filter?.DateFrom ?? string.Empty;
                cmd.Parameters.Add("p_date_to", OracleDbType.Varchar2).Value = filter?.DateTo ?? string.Empty;
                cmd.Parameters.Add("p_region_id", OracleDbType.Int32).Value = (object?)filter?.RegionId ?? DBNull.Value;
                cmd.Parameters.Add("p_branch_id", OracleDbType.Int32).Value = (object?)filter?.BranchId ?? DBNull.Value;
                cmd.Parameters.Add("p_ho_unit_type_id", OracleDbType.Int32).Value = (object?)filter?.HOUnitTypeId ?? DBNull.Value;
                cmd.Parameters.Add("p_ho_unit_id", OracleDbType.Int32).Value = (object?)filter?.HOUnitId ?? DBNull.Value;
                cmd.Parameters.Add("p_status", OracleDbType.Varchar2).Value = filter?.Status ?? string.Empty;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var dt = new DataTable();
                using (var rdr = cmd.ExecuteReader())
                    {
                    dt.Load(rdr);
                    }
                return dt;
                }
            }
        }
    }
