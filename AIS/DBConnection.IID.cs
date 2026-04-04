using AIS.Constants;
using AIS.Models;
using AIS.Models.IID;
using AIS.Models.IID.InquiryReport;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        private static int? SafeOracleNullableIntValue(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return null;
                }

            if (value is OracleDecimal oracleDecimal)
                {
                return oracleDecimal.IsNull ? (int?)null : oracleDecimal.ToInt32();
                }

            try
                {
                return Convert.ToInt32(value);
                }
            catch
                {
                return int.TryParse(value.ToString(), out var parsed) ? parsed : (int?)null;
                }
            }

        private static int SafeOracleIntValue(object value, int defaultValue = 0)
            {
            return SafeOracleNullableIntValue(value) ?? defaultValue;
            }

        private static long? SafeOracleNullableLongValue(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return null;
                }

            if (value is OracleDecimal oracleDecimal)
                {
                return oracleDecimal.IsNull ? (long?)null : oracleDecimal.ToInt64();
                }

            try
                {
                return Convert.ToInt64(value);
                }
            catch
                {
                return long.TryParse(value.ToString(), out var parsed) ? parsed : (long?)null;
                }
            }

        private static int? ParseSessionPpNumber(string ppNumber)
            {
            return int.TryParse(ppNumber, out var parsed) ? parsed : (int?)null;
            }

        public int SubmitComplaint(ComplaintModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return 0;
                }
            var (complaintId, _) = CreateComplaintHeader("IAID");
            if (complaintId <= 0)
                {
                return 0;
                }
            SaveComplaintIAID(model, complaintId);
            return complaintId;
            }

        public (int complaintId, string complaintNo) CreateComplaintHeader(string intakeChannel)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return (0, string.Empty);
                }
            var submittedByPpNo = ParseSessionPpNumber(loggedInUser.PPNumber);
            if (!submittedByPpNo.HasValue)
                {
                return (0, string.Empty);
                }
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_CREATE_COMPLAINT_HDR";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("P_INTAKE_CHANNEL", OracleDbType.Varchar2).Value = intakeChannel ?? string.Empty;
                cmd.Parameters.Add("P_SUBMITTED_BY_PP_NO", OracleDbType.Decimal).Value = submittedByPpNo.Value;
                cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("P_COMPLAINT_NO", OracleDbType.Varchar2, 50).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = SafeOracleIntValue(cmd.Parameters["P_COMPLAINT_ID"].Value);
                var complaintNo = cmd.Parameters["P_COMPLAINT_NO"].Value?.ToString() ?? string.Empty;
                return (id, complaintNo);
                }
            }

        public void SaveComplaintIAID(ComplaintModel model, int complaintId)
            {
            var (locationTypeId, gmOfficeId, regionId, branchId) = DeriveLocationValues(
                model.PertainsTo,
                model.FieldType,
                model.HOUnitId,
                model.RegionId,
                model.BranchId);
            var receivedFrom = string.IsNullOrWhiteSpace(model.ReceivedFrom) ? model.Source : model.ReceivedFrom;
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_SAVE_COMPLAINT_IAID";
                LogIidSaveDebug("PKG_INQ.P_SAVE_COMPLAINT_IAID", $"ComplaintId={complaintId}, Nature={model?.Nature}, Category={model?.Category}, ContentsLength={(model?.Contents ?? string.Empty).Length}");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("P_NATURE", OracleDbType.Varchar2).Value = model.Nature ?? string.Empty;
                cmd.Parameters.Add("P_CATEGORY", OracleDbType.Varchar2).Value = model.Category ?? string.Empty;
                cmd.Parameters.Add("P_RECEIVED_FROM", OracleDbType.Varchar2).Value = receivedFrom ?? string.Empty;
                cmd.Parameters.Add("P_LOCATION_TYPE_ID", OracleDbType.Int32).Value = locationTypeId;
                cmd.Parameters.Add("P_GM_OFFICE_ID", OracleDbType.Int32).Value = gmOfficeId.HasValue && gmOfficeId > 0 ? gmOfficeId : DBNull.Value;
                cmd.Parameters.Add("P_REGION_ID", OracleDbType.Int32).Value = regionId.HasValue && regionId > 0 ? regionId : DBNull.Value;
                cmd.Parameters.Add("P_BRANCH_ID", OracleDbType.Int32).Value = branchId.HasValue && branchId > 0 ? branchId : DBNull.Value;
                cmd.Parameters.Add("P_COMPLAINANT_NAME", OracleDbType.Varchar2).Value = model.ComplainantName ?? string.Empty;
                cmd.Parameters.Add("P_CNIC", OracleDbType.Varchar2).Value = model.CNIC ?? string.Empty;
                cmd.Parameters.Add("P_CELLULAR_NUMBER", OracleDbType.Varchar2).Value = model.CellularNumber ?? string.Empty;
                cmd.Parameters.Add("P_MAILING_ADDRESS", OracleDbType.Varchar2).Value = model.MailingAddress ?? string.Empty;
                cmd.Parameters.Add("P_GENDER", OracleDbType.Varchar2).Value = model.Gender ?? string.Empty;
                cmd.Parameters.Add("P_CONTENTS", OracleDbType.Clob).Value = model.Contents ?? string.Empty;
                cmd.Parameters.Add("P_UPLOADED_COMPLAINT", OracleDbType.Varchar2).Value = model.UploadedComplaint ?? string.Empty;
                cmd.Parameters.Add("P_UPLOADED_FFR", OracleDbType.Varchar2).Value = model.UploadedFFR ?? string.Empty;
                cmd.Parameters.Add("P_UPLOADED_EVIDENCE", OracleDbType.Varchar2).Value = model.UploadedEvidence ?? string.Empty;
                cmd.Parameters.Add("P_ACTION_REQUIRED", OracleDbType.Varchar2).Value = model.ActionRequired ?? string.Empty;
                cmd.ExecuteNonQuery();
                }
            }

        public void SaveFFRPart1(FFRPart1Model model, int complaintId)
            {
            var (locationTypeId, gmOfficeId, regionId, branchId) = DeriveLocationValues(
                model.PertainsTo,
                model.FieldType,
                model.HOUnitId,
                model.RegionId,
                model.BranchId);
            var originatingSource = model.Source;
            if (string.Equals(model.Source, "Other", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(model.SourceOtherText))
                {
                originatingSource = model.SourceOtherText;
                }
            var ffrDate = ParseDate(model.FFRDate);
            var incidentDate = ParseDate(model.IncidentDate);
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_SAVE_FFR_P1";
                LogIidSaveDebug("PKG_INQ.P_SAVE_FFR_P1", $"ComplaintId={complaintId}, Source={model?.Source}, Nature={model?.Nature}, AttachmentsLength={(model?.AttachmentsPath ?? string.Empty).Length}");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("P_LOCATION_TYPE_ID", OracleDbType.Int32).Value = locationTypeId;
                cmd.Parameters.Add("P_GM_OFFICE_ID", OracleDbType.Int32).Value = gmOfficeId.HasValue && gmOfficeId > 0 ? gmOfficeId : DBNull.Value;
                cmd.Parameters.Add("P_REGION_ID", OracleDbType.Int32).Value = regionId.HasValue && regionId > 0 ? regionId : DBNull.Value;
                cmd.Parameters.Add("P_BRANCH_ID", OracleDbType.Int32).Value = branchId.HasValue && branchId > 0 ? branchId : DBNull.Value;
                cmd.Parameters.Add("P_ORIG_DEPT_SOURCE", OracleDbType.Varchar2).Value = originatingSource ?? string.Empty;
                cmd.Parameters.Add("P_NATURE", OracleDbType.Varchar2).Value = model.Nature ?? string.Empty;
                cmd.Parameters.Add("P_REFERENCE_NO", OracleDbType.Varchar2).Value = model.ReferenceNo ?? string.Empty;
                cmd.Parameters.Add("P_FFR_DATE", OracleDbType.Date).Value = (object?)ffrDate ?? DBNull.Value;
                cmd.Parameters.Add("P_INCIDENT_DATE", OracleDbType.Date).Value = (object?)incidentDate ?? DBNull.Value;
                cmd.Parameters.Add("P_INCIDENT_VENUE", OracleDbType.Varchar2).Value = model.IncidentVenue ?? string.Empty;
                cmd.Parameters.Add("P_INCIDENT_HOW", OracleDbType.Clob).Value = model.IncidentNarrative ?? string.Empty;
                cmd.Parameters.Add("P_COMPLAINANT_NAME", OracleDbType.Varchar2).Value = model.ComplainantName ?? string.Empty;
                cmd.Parameters.Add("P_COMPLAINANT_CNIC", OracleDbType.Varchar2).Value = model.ComplainantCNIC ?? string.Empty;
                cmd.Parameters.Add("P_COMPLAINANT_ACCOUNT_NO", OracleDbType.Varchar2).Value = model.AccountNo ?? string.Empty;
                cmd.Parameters.Add("P_COMPLAINANT_MOBILE", OracleDbType.Varchar2).Value = model.ComplainantMobile ?? string.Empty;
                cmd.Parameters.Add("P_COMPLAINANT_ADDRESS", OracleDbType.Varchar2).Value = model.ComplainantAddress ?? string.Empty;
                cmd.Parameters.Add("P_MAIN_ACCUSED_DETAILS", OracleDbType.Clob).Value = model.MainAccused ?? string.Empty;
                cmd.Parameters.Add("P_CO_ACCUSED_DETAILS", OracleDbType.Clob).Value = model.CoAccused ?? string.Empty;
                cmd.Parameters.Add("P_ACCUSATION_DETAILS", OracleDbType.Clob).Value = model.Accusations ?? string.Empty;
                cmd.Parameters.Add("P_APPROACH_ADOPTED", OracleDbType.Clob).Value = model.ApproachAdopted ?? string.Empty;
                cmd.Parameters.Add("P_RECORD_SCRUTINIZED", OracleDbType.Clob).Value = model.RecordScrutinized ?? string.Empty;
                cmd.Parameters.Add("P_ROOT_CAUSE", OracleDbType.Clob).Value = model.RootCause ?? string.Empty;
                cmd.Parameters.Add("P_KEY_FINDINGS", OracleDbType.Clob).Value = model.KeyFindings ?? string.Empty;
                cmd.Parameters.Add("P_RECOMMENDATIONS", OracleDbType.Clob).Value = model.ClearRecommendations ?? string.Empty;
                cmd.Parameters.Add("P_ATTACHMENTS_PATH", OracleDbType.Varchar2).Value = model.AttachmentsPath ?? string.Empty;
                cmd.ExecuteNonQuery();
                }
            }

        public void SaveFFRPart2(FFRPart2Model model, int complaintId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_SAVE_FFR_P2";
                LogIidSaveDebug("PKG_INQ.P_SAVE_FFR_P2", $"ComplaintId={complaintId}, ComplainantStatementTime={model?.ComplainantStatementTime}, PrimaryEvidenceLength={(model?.PrimaryEvidence ?? string.Empty).Length}");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("P_COMP_STMT_TIME", OracleDbType.Varchar2).Value = model.ComplainantStatementTime ?? string.Empty;
                cmd.Parameters.Add("P_COMP_STMT_PLACE", OracleDbType.Varchar2).Value = model.ComplainantStatementPlace ?? string.Empty;
                cmd.Parameters.Add("P_COMP_STMT_MODE", OracleDbType.Varchar2).Value = model.ComplainantStatementMode ?? string.Empty;
                cmd.Parameters.Add("P_COMP_KEY_POINTS", OracleDbType.Clob).Value = model.ComplainantStatementPoints ?? string.Empty;
                cmd.Parameters.Add("P_ACC_STMT_TIME", OracleDbType.Varchar2).Value = model.AccusedStatementTime ?? string.Empty;
                cmd.Parameters.Add("P_ACC_STMT_PLACE", OracleDbType.Varchar2).Value = model.AccusedStatementPlace ?? string.Empty;
                cmd.Parameters.Add("P_ACC_STMT_MODE", OracleDbType.Varchar2).Value = model.AccusedStatementMode ?? string.Empty;
                cmd.Parameters.Add("P_ACC_KEY_POINTS", OracleDbType.Clob).Value = model.AccusedStatementPoints ?? string.Empty;
                cmd.Parameters.Add("P_PRIMARY_EVIDENCE", OracleDbType.Clob).Value = model.PrimaryEvidence ?? string.Empty;
                cmd.Parameters.Add("P_SECONDARY_EVIDENCE", OracleDbType.Clob).Value = model.SecondaryEvidence ?? string.Empty;
                cmd.ExecuteNonQuery();
                }
            }

        public void SaveFFRPart3(FFRPart3Model model, int complaintId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_SAVE_FFR_P3";
                LogIidSaveDebug("PKG_INQ.P_SAVE_FFR_P3", $"ComplaintId={complaintId}, AuditHighlighted={model?.AuditHighlighted}, PolicyViolatedLength={(model?.PolicyViolated ?? string.Empty).Length}");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("P_AUDIT_REPORT_FLAG", OracleDbType.Varchar2).Value = NormalizeAuditFlag(model.AuditHighlighted);
                cmd.Parameters.Add("P_AUDIT_REPORT_DETAILS", OracleDbType.Clob).Value = model.AuditHighlightDetails ?? string.Empty;
                cmd.Parameters.Add("P_IMPL_REPUTATIONAL", OracleDbType.Char).Value = model.ImplicationReputational ? "Y" : "N";
                cmd.Parameters.Add("P_IMPL_OPERATIONAL", OracleDbType.Char).Value = model.ImplicationOperational ? "Y" : "N";
                cmd.Parameters.Add("P_IMPL_FINANCIAL", OracleDbType.Char).Value = model.ImplicationFinancial ? "Y" : "N";
                cmd.Parameters.Add("P_IMPL_PRECEDENCE", OracleDbType.Char).Value = model.ImplicationPrecedence ? "Y" : "N";
                cmd.Parameters.Add("P_IMPL_OTHER_FLAG", OracleDbType.Char).Value = model.ImplicationOther ? "Y" : "N";
                cmd.Parameters.Add("P_IMPL_OTHER_TEXT", OracleDbType.Varchar2).Value = model.ImplicationOtherDetails ?? string.Empty;
                cmd.Parameters.Add("P_SOP_VIOLATIONS", OracleDbType.Clob).Value = model.PolicyViolated ?? string.Empty;
                cmd.Parameters.Add("P_CONTROL_GAPS", OracleDbType.Clob).Value = model.SopGaps ?? string.Empty;
                cmd.Parameters.Add("P_ACTION_RECOMMENDED", OracleDbType.Varchar2).Value = model.ActionRecommended ?? string.Empty;
                cmd.ExecuteNonQuery();
                }
            }

        public DataTable GetComplaintHeader(int complaintId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_GET_COMPLAINT_HDR";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var dt = new DataTable();
                using (var rdr = cmd.ExecuteReader())
                    {
                    dt.Load(rdr);
                    }
                return dt;
                }
            }

        public DataTable GetFFRPart1(int complaintId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_GET_FFR_P1";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var dt = new DataTable();
                using (var rdr = cmd.ExecuteReader())
                    {
                    dt.Load(rdr);
                    }
                return dt;
                }
            }

        public DataTable GetFFRPart2(int complaintId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_GET_FFR_P2";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var dt = new DataTable();
                using (var rdr = cmd.ExecuteReader())
                    {
                    dt.Load(rdr);
                    }
                return dt;
                }
            }

        public DataTable GetFFRPart3(int complaintId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_GET_FFR_P3";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var dt = new DataTable();
                using (var rdr = cmd.ExecuteReader())
                    {
                    dt.Load(rdr);
                    }
                return dt;
                }
            }

        private static bool HasColumn(IDataRecord reader, string columnName)
            {
            for (var i = 0; i < reader.FieldCount; i++)
                {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    {
                    return true;
                    }
                }
            return false;
            }

        private static (int locationTypeId, int? gmOfficeId, int? regionId, int? branchId) DeriveLocationValues(
            string pertainsTo,
            string fieldType,
            int? hoUnitId,
            int? regionId,
            int? branchId)
            {
            var locationTypeId = 0;
            int? gmOfficeId = null;
            int? resolvedRegionId = null;
            int? resolvedBranchId = null;

            if (string.Equals(pertainsTo, "HO", StringComparison.OrdinalIgnoreCase))
                {
                locationTypeId = 1;
                }
            else if (string.Equals(pertainsTo, "FIELD", StringComparison.OrdinalIgnoreCase))
                {
                if (string.Equals(fieldType, "HO_UNIT", StringComparison.OrdinalIgnoreCase))
                    {
                    locationTypeId = 2;
                    gmOfficeId = hoUnitId;
                    }
                else if (string.Equals(fieldType, "BRANCH", StringComparison.OrdinalIgnoreCase))
                    {
                    locationTypeId = 3;
                    resolvedRegionId = regionId;
                    resolvedBranchId = branchId;
                    }
                }

            return (locationTypeId, gmOfficeId, resolvedRegionId, resolvedBranchId);
            }

        private static DateTime? ParseDate(string dateValue)
            {
            if (string.IsNullOrWhiteSpace(dateValue))
                {
                return null;
                }
            if (DateTime.TryParse(dateValue, out var parsed))
                {
                return parsed;
                }
            return null;
            }

        private static string NormalizeAuditFlag(string auditFlag)
            {
            if (string.IsNullOrWhiteSpace(auditFlag))
                {
                return string.Empty;
                }
            var normalized = auditFlag.Trim().ToUpperInvariant();
            if (normalized == "YES" || normalized == "NO" || normalized == "NA")
                {
                return normalized;
                }
            if (normalized == "Y")
                {
                return "YES";
                }
            if (normalized == "N")
                {
                return "NO";
                }
            return auditFlag;
            }

        public List<ComplaintRowDto> GetComplaintsByUser()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<ComplaintRowDto>();
                }

            if (!int.TryParse(loggedInUser.PPNumber, out var parsedPpno))
                {
                return new List<ComplaintRowDto>();
                }

            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_COMPLAINTS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = parsedPpno;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var complaints = new List<ComplaintRowDto>();
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        complaints.Add(new ComplaintRowDto
                            {
                            ComplaintId = SafeOracleNullableLongValue(rdr["COMPLAINT_ID"]) ?? 0,
                            ComplaintNo = rdr["COMPLAINT_NO"]?.ToString() ?? string.Empty,
                            ComplainantName = rdr["COMPLAINANT_NAME"]?.ToString() ?? string.Empty,
                            Nature = rdr["NATURE"]?.ToString() ?? string.Empty,
                            Source = rdr["SOURCE"]?.ToString() ?? string.Empty,
                            AssignedUnit = rdr["ASSIGNED_UNIT"]?.ToString() ?? string.Empty,
                            Status = rdr["STATUS"]?.ToString() ?? string.Empty,
                            SubmittedOn = rdr["SUBMITTED_ON"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                return complaints;
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
                            ComplaintId = SafeReadInt(rdr, "COMPLAINT_ID"),
                            Nature = rdr["NATURE"].ToString(),
                            SubmittedOn = rdr["SUBMITTED_ON"].ToString()
                            });
                        }
                    }
                return list;
                }
            }

        public List<ComplaintDropdownItemModel> GetComplaintsDropdown(int pageId)
            {
            var con = this.DatabaseConnection();
            var list = new List<ComplaintDropdownItemModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_COMPLAINTS_DD";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("P_PAGE_ID", OracleDbType.Int32).Value = pageId;
                cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        list.Add(new ComplaintDropdownItemModel
                            {
                            ComplaintId = SafeReadInt(rdr, "COMPLAINT_ID"),
                            Nature = rdr["NATURE"]?.ToString(),
                            Status = rdr["STATUS"]?.ToString()
                            });
                        }
                    }
                }

            con.Dispose();
            return list;
            }

        public List<InspectionUnitsModel> GetInspectionUnits()
            {
            var con = this.DatabaseConnection();
            var list = new List<InspectionUnitsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_GETINSPECTIONUNITS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Clear();

                // Match DB param name: IO_CURSOR OUT T_CURSOR
                cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        var z = new InspectionUnitsModel();

                        // These must exist in the cursor output
                        z.I_ID = Convert.ToInt32(rdr["I_ID"]);
                        z.I_CODE = rdr["I_CODE"]?.ToString();
                        z.UNIT_NAME = rdr["UNIT_NAME"]?.ToString();
                        z.DISCRIPTION = rdr["DISCRIPTION"]?.ToString();

                        var status = rdr["STATUS"]?.ToString();

                        if (status == "Y")
                            z.STATUS = "Active";
                        else if (status == "N")
                            z.STATUS = "InActive";
                        else
                            z.STATUS = rdr["ISACTIVE"]?.ToString(); // keep as-is if you still want this fallback
                                                                    // but make sure ISACTIVE is also returned by the cursor
                        list.Add(z);
                        }
                    }
                }

            con.Dispose();
            return list;
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
                            ComplaintNo = HasColumn(rdr, "COMPLAINT_NO") ? rdr["COMPLAINT_NO"].ToString() : null,
                            Nature = rdr["NATURE"].ToString(),
                            Category = HasColumn(rdr, "CATEGORY") ? rdr["CATEGORY"].ToString() : null,
                            ComplainantName = HasColumn(rdr, "COMPLAINANT_NAME") ? rdr["COMPLAINANT_NAME"].ToString() : null,
                            CNIC = HasColumn(rdr, "CNIC") ? rdr["CNIC"].ToString() : null,
                            CellularNumber = HasColumn(rdr, "CELLULAR_NUMBER") ? rdr["CELLULAR_NUMBER"].ToString() : null,
                            MailingAddress = HasColumn(rdr, "MAILING_ADDRESS") ? rdr["MAILING_ADDRESS"].ToString() : null,
                            Gender = HasColumn(rdr, "GENDER") ? rdr["GENDER"].ToString() : null,
                            ReceivedFrom = HasColumn(rdr, "RECEIVED_FROM") ? rdr["RECEIVED_FROM"].ToString() : null,
                            LocationTypeText = HasColumn(rdr, "LOCATION_TYPE") ? rdr["LOCATION_TYPE"].ToString() : null,
                            Contents = rdr["CONTENTS"].ToString(),
                            UploadedComplaint = rdr["UPLOADED_COMPLAINT"].ToString(),
                            UploadedFFR = rdr["UPLOADED_FFR"].ToString(),
                            UploadedEvidence = rdr["UPLOADED_EVIDENCE"].ToString(),
                            ActionRequired = rdr["ACTION_REQUIRED"].ToString(),
                            SubmittedOn = rdr["SUBMITTED_ON"].ToString(),
                            Status = HasColumn(rdr, "STATUS") ? rdr["STATUS"].ToString() : null,
                            Assessment = HasColumn(rdr, "ASSESSMENT") ? rdr["ASSESSMENT"].ToString() : null,
                            AssessmentId = HasColumn(rdr, "assessment_id") && rdr["assessment_id"] != DBNull.Value ? Convert.ToInt32(rdr["assessment_id"]) : (int?)null,                            
                            Recommendation = HasColumn(rdr, "RECOMMENDATION") ? rdr["RECOMMENDATION"].ToString() : null,
                            LocationTypeId = HasColumn(rdr, "LOCATION_TYPE_ID") && rdr["LOCATION_TYPE_ID"] != DBNull.Value ? Convert.ToInt32(rdr["LOCATION_TYPE_ID"]) : (int?)null,
                            GMOfficeId = HasColumn(rdr, "GM_OFFICE_ID") && rdr["GM_OFFICE_ID"] != DBNull.Value ? Convert.ToInt32(rdr["GM_OFFICE_ID"]) : (int?)null,
                            RegionId = HasColumn(rdr, "REGION_ID") && rdr["REGION_ID"] != DBNull.Value ? Convert.ToInt32(rdr["REGION_ID"]) : (int?)null,
                            BranchId = HasColumn(rdr, "BRANCH_ID") && rdr["BRANCH_ID"] != DBNull.Value ? Convert.ToInt32(rdr["BRANCH_ID"]) : (int?)null,
                            GMOffice = HasColumn(rdr, "GM_OFFICE") ? rdr["GM_OFFICE"]?.ToString() : null,
                            Region = HasColumn(rdr, "REGION") ? rdr["REGION"]?.ToString() : null,
                            Branch = HasColumn(rdr, "BRANCH") ? rdr["BRANCH"]?.ToString() : null,
                            AssignedUnitId = HasColumn(rdr, "ASSIGNED_UNIT_ID") && rdr["ASSIGNED_UNIT_ID"] != DBNull.Value
                                ? Convert.ToInt32(rdr["ASSIGNED_UNIT_ID"])
                                : 0,
                            AssignedUnit = HasColumn(rdr, "ASSIGNED_UNIT") ? rdr["ASSIGNED_UNIT"]?.ToString() : null
                            };
                        }
                    }
                return model;
                }
            }

        public int AddAssessment(InitialAssessmentModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return 0;
                }
            var receivedByPpNo = ParseSessionPpNumber(loggedInUser.PPNumber);
            if (!receivedByPpNo.HasValue)
                {
                return 0;
                }
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_ASSESSMENT";
                LogIidSaveDebug("PKG_INQ.ADD_ASSESSMENT", $"ComplaintId={model?.ComplaintId}, AssignedUnitId={model?.AssignedUnitId}, AssessmentLength={(model?.Assessment ?? string.Empty).Length}");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = model.ComplaintId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_received_by", OracleDbType.Decimal).Value = receivedByPpNo.Value;
                cmd.Parameters.Add("p_assessment", OracleDbType.Clob).Value = model.Assessment ?? string.Empty;
                cmd.Parameters.Add("p_recommendation", OracleDbType.Varchar2).Value = model.Recommendation ?? string.Empty;
                cmd.Parameters.Add("p_assigned_unit_id", OracleDbType.Int32).Value = model.AssignedUnitId;
                cmd.Parameters.Add("o_assessment_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = SafeOracleIntValue(cmd.Parameters["o_assessment_id"].Value);
                return id;
                }
            }

        public int AddHeadReview(HeadReviewModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return 0;
                }
            var reviewedByPpNo = ParseSessionPpNumber(loggedInUser.PPNumber);
            if (!reviewedByPpNo.HasValue)
                {
                return 0;
                }
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_HEAD_REVIEW";
                LogIidSaveDebug("PKG_INQ.ADD_HEAD_REVIEW", $"ComplaintId={model?.ComplaintId}, AssessmentId={model?.AssessmentId}, AssignedToUnit={model?.AssignedToUnit}, Action={model?.Action}");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = model.ComplaintId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_assessment_id", OracleDbType.Int32).Value = model.AssessmentId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_reviewed_by", OracleDbType.Decimal).Value = reviewedByPpNo.Value;
                cmd.Parameters.Add("p_directions", OracleDbType.Clob).Value = model.Directions ?? string.Empty;
                cmd.Parameters.Add("p_assigned_to_unit", OracleDbType.Int32).Value = model.AssignedToUnit == 0 ? (object)DBNull.Value : (object)model.AssignedToUnit;
                cmd.Parameters.Add("p_team_lead", OracleDbType.Int32).Value = model.TeamLeadId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_team_members", OracleDbType.Clob).Value = model.TeamMembers ?? string.Empty;
                cmd.Parameters.Add("p_assigned_on", OracleDbType.Varchar2).Value = model.AssignedOn ?? string.Empty;
                cmd.Parameters.Add("p_due_date", OracleDbType.Varchar2).Value = model.DueDate ?? string.Empty;
                cmd.Parameters.Add("p_referred_back_comments", OracleDbType.Clob).Value = model.ReferredBackComments ?? string.Empty;
                cmd.Parameters.Add("p_action", OracleDbType.Varchar2).Value = model.Action ?? string.Empty;
                cmd.Parameters.Add("o_review_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = SafeOracleIntValue(cmd.Parameters["o_review_id"].Value);
                return id;
                }
            }

        public int AddInvestigationPlan(InvestigationPlanModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return 0;
                }
            var submittedByPpNo = ParseSessionPpNumber(loggedInUser.PPNumber);
            if (!submittedByPpNo.HasValue)
                {
                return 0;
                }
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_INV_PLAN";
                LogIidSaveDebug("PKG_INQ.ADD_INV_PLAN", $"ComplaintId={model?.ComplaintId}, PlanDetailsLength={(model?.PlanDetails ?? string.Empty).Length}");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                model.Status = IidStatuses.PlanDrafted;

                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = model.ComplaintId;
                cmd.Parameters.Add("p_plan_details", OracleDbType.Clob).Value = model.PlanDetails ?? string.Empty;
                cmd.Parameters.Add("p_submitted_by", OracleDbType.Decimal).Value = submittedByPpNo.Value;
                cmd.Parameters.Add("p_status", OracleDbType.Varchar2).Value = model.Status ?? string.Empty;

                // New params (must match PL/SQL signature EXACTLY)
                cmd.Parameters.Add("p_inv_risk", OracleDbType.Varchar2).Value = model.InvestigationRisk ?? string.Empty;
                cmd.Parameters.Add("p_inv_size", OracleDbType.Varchar2).Value = model.InvestigationSize ?? string.Empty;

                cmd.Parameters.Add("p_no_of_days", OracleDbType.Int32).Value =
                    model.NoOfDays.HasValue ? model.NoOfDays.Value : (object)DBNull.Value;

                cmd.Parameters.Add("p_travelling_days", OracleDbType.Int32).Value =
                    model.TravellingDays.HasValue ? model.TravellingDays.Value : (object)DBNull.Value;

                cmd.Parameters.Add("p_team_lead", OracleDbType.Varchar2).Value = model.TeamLead ?? string.Empty;
                cmd.Parameters.Add("p_team_members", OracleDbType.Varchar2).Value = model.TeamMembers ?? string.Empty;

                cmd.Parameters.Add("p_start_date", OracleDbType.Date).Value =
                    model.StartDate.HasValue ? model.StartDate.Value : (object)DBNull.Value;

                // If ACTIVITIES_TEXT exists in table/proc
                cmd.Parameters.Add("p_activities_text", OracleDbType.Varchar2).Value = model.ActivitiesText ?? string.Empty;

                cmd.Parameters.Add("o_plan_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = SafeOracleIntValue(cmd.Parameters["o_plan_id"].Value);
                return id;
                }
            }

        public DataTable GetIidTaskList(int unitId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_IID_TASK_LIST";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_unit_id", OracleDbType.Int32).Value = unitId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var dt = new DataTable();
                using (var rdr = cmd.ExecuteReader())
                    {
                    dt.Load(rdr);
                    }
                return dt;
                }
            }

        public int EnqueueEmail(string eventCode, int? refId1, int? refId2, string mailTo, string mailCc, string subject, string body)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_ENQUEUE_EMAIL";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_event_code", OracleDbType.Varchar2).Value = eventCode ?? string.Empty;
                cmd.Parameters.Add("p_ref_id1", OracleDbType.Int32).Value = refId1 ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_ref_id2", OracleDbType.Int32).Value = refId2 ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_mail_to", OracleDbType.Varchar2).Value = mailTo ?? string.Empty;
                cmd.Parameters.Add("p_mail_cc", OracleDbType.Varchar2).Value = mailCc ?? string.Empty;
                cmd.Parameters.Add("p_subject", OracleDbType.Varchar2).Value = subject ?? string.Empty;
                cmd.Parameters.Add("p_body", OracleDbType.Clob).Value = body ?? string.Empty;
                cmd.Parameters.Add("o_email_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return SafeOracleIntValue(cmd.Parameters["o_email_id"].Value);
                }
            }

        public List<EmailQueueItemModel> GetEmailQueue(string status, DateTime? fromDate, DateTime? toDate)
            {
            using var con = this.DatabaseConnection();
            var items = new List<EmailQueueItemModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_GET_EMAIL_QUEUE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_status", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status;
                cmd.Parameters.Add("p_from_date", OracleDbType.Date).Value = fromDate ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_to_date", OracleDbType.Date).Value = toDate ?? (object)DBNull.Value;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        items.Add(new EmailQueueItemModel
                            {
                            EmailId = Convert.ToInt32(rdr["EMAIL_ID"]),
                            EventCode = rdr["EVENT_CODE"]?.ToString(),
                            RefId1 = rdr["REF_ID1"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["REF_ID1"]),
                            RefId2 = rdr["REF_ID2"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["REF_ID2"]),
                            MailTo = rdr["MAIL_TO"]?.ToString(),
                            MailCc = rdr["MAIL_CC"]?.ToString(),
                            Subject = rdr["SUBJECT"]?.ToString(),
                            Body = rdr["BODY"]?.ToString(),
                            Status = rdr["STATUS"]?.ToString(),
                            CreatedOn = rdr["CREATED_ON"] == DBNull.Value ? null : Convert.ToDateTime(rdr["CREATED_ON"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            SentOn = rdr["SENT_ON"] == DBNull.Value ? null : Convert.ToDateTime(rdr["SENT_ON"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            ErrorText = rdr["ERROR_TEXT"]?.ToString()
                            });
                        }
                    }
                }

            return items;
            }

        public void MarkEmailSent(int emailId)
            {
            using var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_MARK_EMAIL_SENT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_email_id", OracleDbType.Int32).Value = emailId;
                cmd.ExecuteNonQuery();
                }
            }

        public void MarkEmailFailed(int emailId, string errorText)
            {
            using var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.P_MARK_EMAIL_FAILED";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_email_id", OracleDbType.Int32).Value = emailId;
                cmd.Parameters.Add("p_error_text", OracleDbType.Varchar2).Value = errorText ?? string.Empty;
                cmd.ExecuteNonQuery();
                }
            }

        public int? GetComplaintIdByPlanId(int? planId)
            {
            if (!planId.HasValue || planId.Value <= 0) return null;

            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_COMPLAINT_ID_BY_PLAN";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("P_PLAN_ID", OracleDbType.Decimal).Value = planId.Value;
            cmd.Parameters.Add("O_COMPLAINT_ID", OracleDbType.Decimal).Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();

            var v = cmd.Parameters["O_COMPLAINT_ID"].Value;
            return SafeOracleNullableIntValue(v);
            }

        public int? GetComplaintIdByReportId(int? reportId)
            {
            if (!reportId.HasValue || reportId.Value <= 0) return null;

            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_COMPLAINT_ID_BY_REPORT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("P_REPORT_ID", OracleDbType.Decimal).Value = reportId.Value;
            cmd.Parameters.Add("O_COMPLAINT_ID", OracleDbType.Decimal).Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();

            var v = cmd.Parameters["O_COMPLAINT_ID"].Value;
            return SafeOracleNullableIntValue(v);
            }

        public IDictionary<string, object> GetIidPlanDetails(int complaintId)
            {
            using var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_INV_PLAN";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                IDictionary<string, object> model = null;

                string FormatDate(object value)
                    {
                    if (value == null || value == DBNull.Value)
                        {
                        return string.Empty;
                        }

                    return Convert.ToDateTime(value).ToString("yyyy-MM-dd");
                    }

                using (var rdr = cmd.ExecuteReader())
                    {
                    if (rdr.Read())
                        {
                        model = new Dictionary<string, object>
                            {
                            ["planId"] = SafeReadInt(rdr, "PLAN_ID"),
                            ["complaintId"] = SafeReadNullableInt(rdr, "COMPLAINT_ID") ?? complaintId,
                            ["planDetails"] = HasColumn(rdr, "PLAN_DETAILS") ? rdr["PLAN_DETAILS"]?.ToString() ?? string.Empty : string.Empty,
                            ["submittedBy"] = SafeReadInt(rdr, "SUBMITTED_BY"),
                            ["submittedOn"] = HasColumn(rdr, "SUBMITTED_ON") ? FormatDate(rdr["SUBMITTED_ON"]) : string.Empty,
                            ["status"] = HasColumn(rdr, "STATUS") ? rdr["STATUS"]?.ToString() ?? string.Empty : string.Empty,
                            ["planTitle"] = HasColumn(rdr, "PLAN_TITLE") ? rdr["PLAN_TITLE"]?.ToString() ?? string.Empty : string.Empty,
                            ["investigationRisk"] = HasColumn(rdr, "INVESTIGATION_RISK") ? rdr["INVESTIGATION_RISK"]?.ToString() ?? string.Empty : string.Empty,
                            ["investigationSize"] = HasColumn(rdr, "INVESTIGATION_SIZE") ? rdr["INVESTIGATION_SIZE"]?.ToString() ?? string.Empty : string.Empty,
                            ["noOfDays"] = SafeReadNullableInt(rdr, "NO_OF_DAYS"),
                            ["travellingDays"] = SafeReadNullableInt(rdr, "TRAVELLING_DAYS"),
                            ["startDate"] = HasColumn(rdr, "START_DATE") ? FormatDate(rdr["START_DATE"]) : string.Empty,
                            ["teamLead"] = HasColumn(rdr, "TEAM_LEAD") ? rdr["TEAM_LEAD"]?.ToString() ?? string.Empty : string.Empty,
                            ["teamMembers"] = HasColumn(rdr, "TEAM_MEMBERS") ? rdr["TEAM_MEMBERS"]?.ToString() ?? string.Empty : string.Empty,
                            ["activitiesText"] = HasColumn(rdr, "ACTIVITIES_TEXT") ? rdr["ACTIVITIES_TEXT"]?.ToString() ?? string.Empty : string.Empty
                            };
                        }
                    }

                return model;
                }
            }

        public DataTable GetLatestPlanByComplaintId(int complaintId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_LATEST_PLAN_BY_COMPLAINT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var dt = new DataTable();
                using (var rdr = cmd.ExecuteReader())
                    {
                    dt.Load(rdr);
                    }
                return dt;
                }
            }

        public InquiryReportFilesModel GetLatestInquiryReportByComplaintId(int complaintId)
            {
            using var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.GET_LATEST_INQUIRY_REPORT_BY_COMPLAINT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_complaint_id", OracleDbType.Int32).Value = complaintId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                InquiryReportFilesModel model = null;
                using (var rdr = cmd.ExecuteReader())
                    {
                    if (rdr.Read())
                        {
                        model = new InquiryReportFilesModel
                            {
                            ReportId = rdr["REPORT_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["REPORT_ID"]),
                            NameComplainant = HasColumn(rdr, "NAME_COMPLAINANT") ? rdr["NAME_COMPLAINANT"]?.ToString() : string.Empty,
                            NameAccused = HasColumn(rdr, "NAME_ACCUSED") ? rdr["NAME_ACCUSED"]?.ToString() : string.Empty,
                            Gist = HasColumn(rdr, "GIST") ? rdr["GIST"]?.ToString() : string.Empty,
                            Proceedings = HasColumn(rdr, "PROCEEDINGS") ? rdr["PROCEEDINGS"]?.ToString() : string.Empty,
                            Findings = HasColumn(rdr, "FINDINGS") ? rdr["FINDINGS"]?.ToString() : string.Empty,
                            Recommendation = HasColumn(rdr, "RECOMMENDATION") ? rdr["RECOMMENDATION"]?.ToString() : string.Empty,
                            UploadedReport = rdr["UPLOADED_REPORT"]?.ToString(),
                            UploadedEvidence = rdr["UPLOADED_EVIDENCE"]?.ToString(),
                            UploadedDsa = rdr["UPLOADED_DSA"]?.ToString(),
                            SubmittedOn = HasColumn(rdr, "SUBMITTED_ON") ? rdr["SUBMITTED_ON"]?.ToString() : string.Empty
                            };
                        }
                    }
                return model;
                }
            }

        public int AddPlanApproval(PlanApprovalModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return 0;
                }
            var approvedByPpNo = ParseSessionPpNumber(loggedInUser.PPNumber);
            if (!approvedByPpNo.HasValue)
                {
                return 0;
                }
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_PLAN_APPROVAL";
                LogIidSaveDebug("PKG_INQ.ADD_PLAN_APPROVAL", $"PlanId={model?.PlanId}, IsApproved={model?.IsApproved}, EditedPlanLength={(model?.EditedPlan ?? string.Empty).Length}");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_plan_id", OracleDbType.Decimal).Value = model.PlanId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_approved_by", OracleDbType.Decimal).Value = approvedByPpNo.Value;
                cmd.Parameters.Add("p_is_approved", OracleDbType.Varchar2).Value = model.IsApproved ?? string.Empty;
                cmd.Parameters.Add("p_edited_plan", OracleDbType.Clob).Value = model.EditedPlan ?? string.Empty;
                cmd.Parameters.Add("p_further_actions", OracleDbType.Clob).Value = model.FurtherActions ?? string.Empty;
                cmd.Parameters.Add("o_approval_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = SafeOracleIntValue(cmd.Parameters["o_approval_id"].Value);
                return id;
                }
            }

        public int AddInquiryReport(InquiryReportModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return 0;
                }
            var submittedByPpNo = ParseSessionPpNumber(loggedInUser.PPNumber);
            if (!submittedByPpNo.HasValue)
                {
                return 0;
                }
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_INQUIRY_REPORT";
                LogIidSaveDebug("PKG_INQ.ADD_INQUIRY_REPORT", $"ComplaintId={model?.ComplaintId}, NameAccused={model?.NameAccused}, FindingsLength={(model?.Findings ?? string.Empty).Length}");
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
                cmd.Parameters.Add("p_submitted_by", OracleDbType.Decimal).Value = submittedByPpNo.Value;
                cmd.Parameters.Add("o_report_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = SafeOracleIntValue(cmd.Parameters["o_report_id"].Value);
                return id;
                }
            }

        public int AddAnalysis(AnalysisModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return 0;
                }
            var analyzedByPpNo = ParseSessionPpNumber(loggedInUser.PPNumber);
            if (!analyzedByPpNo.HasValue)
                {
                return 0;
                }
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_ANALYSIS";
                LogIidSaveDebug("PKG_INQ.ADD_ANALYSIS", $"ReportId={model?.ReportId}, Decision={model?.Decision}, CommentsLength={(model?.Comments ?? string.Empty).Length}");
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
                cmd.Parameters.Add("p_analyzed_by", OracleDbType.Decimal).Value = analyzedByPpNo.Value;
                cmd.Parameters.Add("o_analysis_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = SafeOracleIntValue(cmd.Parameters["o_analysis_id"].Value);
                return id;
                }
            }

        private int SaveFinalApprovalRecord(FinalApprovalModel model, string decision)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return 0;
                }
            var approvedByPpNo = ParseSessionPpNumber(loggedInUser.PPNumber);
            if (!approvedByPpNo.HasValue)
                {
                return 0;
                }
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_FINAL_APPROVAL";
                LogIidSaveDebug("PKG_INQ.ADD_FINAL_APPROVAL", $"ReportId={model?.ReportId}, Decision={decision}, CommentsLength={(model?.Comments ?? string.Empty).Length}");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_report_id", OracleDbType.Int32).Value = model.ReportId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_comments", OracleDbType.Clob).Value = model.Comments ?? string.Empty;
                cmd.Parameters.Add("p_approved", OracleDbType.Varchar2).Value = decision ?? string.Empty;
                cmd.Parameters.Add("p_approved_by", OracleDbType.Decimal).Value = approvedByPpNo.Value;
                cmd.Parameters.Add("o_final_approval_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var id = SafeOracleIntValue(cmd.Parameters["o_final_approval_id"].Value);
                return id;
                }
            }

        public int AddFinalApproval(FinalApprovalModel model)
            {
            return SaveFinalApprovalRecord(model, model?.Decision ?? string.Empty);
            }

        public int FinalizeIidReport(FinalApprovalModel model)
            {
            return SaveFinalApprovalRecord(model, "APPROVE");
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
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return 0;
                }
            using var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_INQ.ADD_CASE_STUDY";
                LogIidSaveDebug("PKG_INQ.ADD_CASE_STUDY", $"ComplaintId={model?.ComplaintId}, Branch={model?.Branch}, GistLength={(model?.Gist ?? string.Empty).Length}");
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
                var id = SafeOracleIntValue(cmd.Parameters["o_case_id"].Value);
                return id;
                }
            }

        private static string GetStringValue(OracleDataReader reader, string columnName)
            {
            if (!HasColumn(reader, columnName))
                {
                return string.Empty;
                }

            var value = reader[columnName];
            if (value == null || value == DBNull.Value)
                {
                return string.Empty;
                }

            if (value is OracleClob clob)
                {
                using (clob)
                    {
                    return clob.IsNull ? string.Empty : clob.Value;
                    }
                }

            return value.ToString() ?? string.Empty;
            }

        private static long GetLongValue(OracleDataReader reader, string columnName)
            {
            if (!HasColumn(reader, columnName) || reader[columnName] == DBNull.Value)
                {
                return 0;
                }

            return Convert.ToInt64(reader[columnName]);
            }

        private static long? GetNullableLongValue(OracleDataReader reader, string columnName)
            {
            if (!HasColumn(reader, columnName) || reader[columnName] == DBNull.Value)
                {
                return null;
                }

            return Convert.ToInt64(reader[columnName]);
            }

        private static int GetIntValue(OracleDataReader reader, string columnName)
            {
            if (!HasColumn(reader, columnName) || reader[columnName] == DBNull.Value)
                {
                return 0;
                }

            return Convert.ToInt32(reader[columnName]);
            }

        private static int? GetNullableIntValue(OracleDataReader reader, string columnName)
            {
            if (!HasColumn(reader, columnName) || reader[columnName] == DBNull.Value)
                {
                return null;
                }

            return Convert.ToInt32(reader[columnName]);
            }

        private static DateTime GetDateValue(OracleDataReader reader, string columnName)
            {
            if (!HasColumn(reader, columnName) || reader[columnName] == DBNull.Value)
                {
                return DateTime.MinValue;
                }

            return Convert.ToDateTime(reader[columnName]);
            }

        private static DateTime? GetNullableDateValue(OracleDataReader reader, string columnName)
            {
            if (!HasColumn(reader, columnName) || reader[columnName] == DBNull.Value)
                {
                return null;
                }

            return Convert.ToDateTime(reader[columnName]);
            }

        private static IidInqProcResult ExecuteIidResult(OracleCommand cmd)
            {
            var result = new IidInqProcResult
                {
                Ok = true,
                Message = "Operation completed successfully.",
                Id = null
                };

            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read())
                {
                return result;
                }

            var okValue = GetStringValue(rdr, "OK");
            var message = GetStringValue(rdr, "MESSAGE");

            if (!string.IsNullOrWhiteSpace(okValue))
                {
                result.Ok = okValue.Equals("Y", StringComparison.OrdinalIgnoreCase)
                    || okValue.Equals("1", StringComparison.OrdinalIgnoreCase)
                    || okValue.Equals("TRUE", StringComparison.OrdinalIgnoreCase)
                    || okValue.Equals("T", StringComparison.OrdinalIgnoreCase)
                    || okValue.Equals("OK", StringComparison.OrdinalIgnoreCase);
                }

            if (!string.IsNullOrWhiteSpace(message))
                {
                result.Message = message;
                }

            var id = GetNullableLongValue(rdr, "ID");
            if (id.HasValue)
                {
                result.Id = id.Value;
                }

            return result;
            }

        private static void AddIidIoCursor(OracleCommand cmd)
            {
            // InquiryReport flow procedures use io_cursor OUT t_cursor consistently.
            cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            }

        public List<IidInqAccusationRow> GetIidInqAccusationsByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_ACCUSATIONS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidInqAccusationRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                list.Add(new IidInqAccusationRow
                    {
                    AccusationId = GetLongValue(rdr, "ACCUSATION_ID"),
                    ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                    AccusationText = GetStringValue(rdr, "ACCUSATION_TEXT"),
                    SortOrder = GetIntValue(rdr, "SORT_ORDER"),
                    Status = GetStringValue(rdr, "STATUS"),
                    CreatedBy = GetNullableLongValue(rdr, "CREATED_BY"),
                    CreatedOn = GetDateValue(rdr, "CREATED_ON"),
                    UpdatedBy = GetNullableLongValue(rdr, "UPDATED_BY"),
                    UpdatedOn = GetNullableDateValue(rdr, "UPDATED_ON")
                    });
                }

            return list;
            }

        public IidInqProcResult AddIidInqAccusation(IidInqAccusationRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_ADD_INQ_ACCUSATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = model.ComplaintId;
            cmd.Parameters.Add("P_ACCUSATION_TEXT", OracleDbType.Clob).Value = model.AccusationText ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_CREATED_BY", OracleDbType.Int64).Value = model.CreatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult UpdateIidInqAccusation(IidInqAccusationRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_UPDATE_INQ_ACCUSATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_ACCUSATION_ID", OracleDbType.Int64).Value = model.AccusationId;
            cmd.Parameters.Add("P_ACCUSATION_TEXT", OracleDbType.Clob).Value = model.AccusationText ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = model.UpdatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult DeleteIidInqAccusation(long accusationId, long userId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_DELETE_INQ_ACCUSATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_ACCUSATION_ID", OracleDbType.Int64).Value = accusationId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = userId;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidEmployeeInfoModel GetIidEmployeeInfo(long ppNo)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.GET_EMPLOYEE_INFO";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_PP_NO", OracleDbType.Int64).Value = ppNo;
            AddIidIoCursor(cmd);

            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read())
                {
                return null;
                }

            return new IidEmployeeInfoModel
                {
                Ppno = GetStringValue(rdr, "PPNO"),
                Name = GetStringValue(rdr, "ENAME"),
                FatherName = GetStringValue(rdr, "FATHERNAME"),
                Cnic = DigitsOnly(GetStringValue(rdr, "CNIC"))
                };
            }

        public List<IidInqAccusedRow> GetIidInqAccusedListByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_ACCUSED_LIST";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidInqAccusedRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                list.Add(new IidInqAccusedRow
                    {
                    AccusedRowId = GetLongValue(rdr, "ACCUSED_ROW_ID"),
                    ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                    PersonName = GetStringValue(rdr, "PERSON_NAME"),
                    FatherName = GetStringValue(rdr, "FATHER_NAME"),
                    Designation = GetStringValue(rdr, "DESIGNATION"),
                    RoleType = GetStringValue(rdr, "ROLE_TYPE"),
                    StatementType = GetStringValue(rdr, "STATEMENT_TYPE"),
                    PpnoNumber = GetStringValue(rdr, "PPNO_NUMBER"),
                    Cnic = DigitsOnly(GetStringValue(rdr, "CNIC")),
                    Remarks = GetStringValue(rdr, "REMARKS"),
                    SortOrder = GetIntValue(rdr, "SORT_ORDER"),
                    Status = GetStringValue(rdr, "STATUS"),
                    CreatedBy = GetNullableLongValue(rdr, "CREATED_BY"),
                    CreatedOn = GetDateValue(rdr, "CREATED_ON"),
                    UpdatedBy = GetNullableLongValue(rdr, "UPDATED_BY"),
                    UpdatedOn = GetNullableDateValue(rdr, "UPDATED_ON")
                    });
                }

            return list;
            }

        public IidInqProcResult AddIidInqAccused(IidInqAccusedRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_ADD_INQ_ACCUSED";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = model.ComplaintId;
            cmd.Parameters.Add("P_PERSON_NAME", OracleDbType.Varchar2).Value = model.PersonName ?? string.Empty;
            cmd.Parameters.Add("P_DESIGNATION", OracleDbType.Varchar2).Value = model.Designation ?? string.Empty;
            cmd.Parameters.Add("P_ROLE_TYPE", OracleDbType.Varchar2).Value = model.RoleType ?? string.Empty;
            cmd.Parameters.Add("P_PPNO_NUMBER", OracleDbType.Varchar2).Value = model.PpnoNumber ?? string.Empty;
            cmd.Parameters.Add("P_CNIC", OracleDbType.Varchar2).Value = DigitsOnly(model.Cnic);
            cmd.Parameters.Add("P_FATHER_NAME", OracleDbType.Varchar2).Value = model.FatherName ?? string.Empty;
            cmd.Parameters.Add("P_REMARKS", OracleDbType.Varchar2).Value = model.Remarks ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_CREATED_BY", OracleDbType.Int64).Value = model.CreatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult UpdateIidInqAccused(IidInqAccusedRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_UPDATE_INQ_ACCUSED";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_ACCUSED_ROW_ID", OracleDbType.Int64).Value = model.AccusedRowId;
            cmd.Parameters.Add("P_PERSON_NAME", OracleDbType.Varchar2).Value = model.PersonName ?? string.Empty;
            cmd.Parameters.Add("P_DESIGNATION", OracleDbType.Varchar2).Value = model.Designation ?? string.Empty;
            cmd.Parameters.Add("P_ROLE_TYPE", OracleDbType.Varchar2).Value = model.RoleType ?? string.Empty;
            cmd.Parameters.Add("P_PPNO_NUMBER", OracleDbType.Varchar2).Value = model.PpnoNumber ?? string.Empty;
            cmd.Parameters.Add("P_CNIC", OracleDbType.Varchar2).Value = DigitsOnly(model.Cnic);
            cmd.Parameters.Add("P_FATHER_NAME", OracleDbType.Varchar2).Value = model.FatherName ?? string.Empty;
            cmd.Parameters.Add("P_REMARKS", OracleDbType.Varchar2).Value = model.Remarks ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = model.UpdatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult DeleteIidInqAccused(long accusedRowId, long userId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_DELETE_INQ_ACCUSED";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_ACCUSED_ROW_ID", OracleDbType.Int64).Value = accusedRowId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = userId;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public List<IidInqRecordRow> GetIidInqRecordsByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_RECORDS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidInqRecordRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                list.Add(new IidInqRecordRow
                    {
                    RecId = GetLongValue(rdr, "REC_ID"),
                    ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                    RecordTitle = GetStringValue(rdr, "RECORD_TITLE"),
                    RecordDetails = GetStringValue(rdr, "RECORD_DETAILS"),
                    SortOrder = GetIntValue(rdr, "SORT_ORDER"),
                    Status = GetStringValue(rdr, "STATUS"),
                    CreatedBy = GetNullableLongValue(rdr, "CREATED_BY"),
                    CreatedOn = GetDateValue(rdr, "CREATED_ON"),
                    UpdatedBy = GetNullableLongValue(rdr, "UPDATED_BY"),
                    UpdatedOn = GetNullableDateValue(rdr, "UPDATED_ON")
                    });
                }

            return list;
            }

        public IidInqProcResult AddIidInqRecord(IidInqRecordRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_ADD_INQ_RECORD";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = model.ComplaintId;
            cmd.Parameters.Add("P_RECORD_TITLE", OracleDbType.Varchar2).Value = model.RecordTitle ?? string.Empty;
            cmd.Parameters.Add("P_RECORD_DETAILS", OracleDbType.Varchar2).Value = model.RecordDetails ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_CREATED_BY", OracleDbType.Int64).Value = model.CreatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult UpdateIidInqRecord(IidInqRecordRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_UPDATE_INQ_RECORD";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_REC_ID", OracleDbType.Int64).Value = model.RecId;
            cmd.Parameters.Add("P_RECORD_TITLE", OracleDbType.Varchar2).Value = model.RecordTitle ?? string.Empty;
            cmd.Parameters.Add("P_RECORD_DETAILS", OracleDbType.Varchar2).Value = model.RecordDetails ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = model.UpdatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult DeleteIidInqRecord(long recId, long userId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_DELETE_INQ_RECORD";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_REC_ID", OracleDbType.Int64).Value = recId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = userId;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public List<IidInqProceedingRow> GetIidInqProceedingsByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_PROCEEDINGS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidInqProceedingRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                list.Add(new IidInqProceedingRow
                    {
                    // Asad DB wiring: align row id / sort order output with the new child table or proc cursor.
                    ProceedingId = HasColumn(rdr, "PROCEEDING_ID") ? GetLongValue(rdr, "PROCEEDING_ID") : GetLongValue(rdr, "ROW_ID"),
                    ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                    NoticeReference = GetStringValue(rdr, "NOTICE_REFERENCE"),
                    VisitDate = GetNullableDateValue(rdr, "VISIT_DATE"),
                    PlaceVisited = GetStringValue(rdr, "PLACE_VISITED"),
                    ParticipantsDetail = GetStringValue(rdr, "PARTICIPANTS_DETAIL"),
                    MissingParticipantsReason = GetStringValue(rdr, "MISSING_PARTICIPANTS_REASON"),
                    SortOrder = HasColumn(rdr, "SORT_ORDER") ? GetIntValue(rdr, "SORT_ORDER") : (HasColumn(rdr, "SR_NO") ? GetIntValue(rdr, "SR_NO") : 0),
                    Status = HasColumn(rdr, "STATUS") ? GetStringValue(rdr, "STATUS") : "A",
                    CreatedBy = HasColumn(rdr, "CREATED_BY") ? GetNullableLongValue(rdr, "CREATED_BY") : null,
                    CreatedOn = HasColumn(rdr, "CREATED_ON") ? GetDateValue(rdr, "CREATED_ON") : default,
                    UpdatedBy = HasColumn(rdr, "UPDATED_BY") ? GetNullableLongValue(rdr, "UPDATED_BY") : null,
                    UpdatedOn = HasColumn(rdr, "UPDATED_ON") ? GetNullableDateValue(rdr, "UPDATED_ON") : null
                    });
                }

            return list;
            }

        public IidInqProcResult SaveIidInqProceeding(IidInqProceedingRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            var userId = model?.UserId ?? model?.UpdatedBy ?? model?.CreatedBy;
            cmd.CommandText = "PKG_INQ.P_SAVE_INQ_PROCEEDING";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_PROCEEDING_ID", OracleDbType.Int64).Value = model?.ProceedingId > 0 ? model.ProceedingId : 0;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = model.ComplaintId;
            cmd.Parameters.Add("P_NOTICE_REFERENCE", OracleDbType.Varchar2).Value = model.NoticeReference ?? string.Empty;
            cmd.Parameters.Add("P_VISIT_DATE", OracleDbType.Date).Value = model.VisitDate ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_PLACE_VISITED", OracleDbType.Clob).Value = model.PlaceVisited ?? string.Empty;
            cmd.Parameters.Add("P_PARTICIPANTS_DETAIL", OracleDbType.Clob).Value = model.ParticipantsDetail ?? string.Empty;
            cmd.Parameters.Add("P_MISSING_PARTICIPANTS_REASON", OracleDbType.Clob).Value = model.MissingParticipantsReason ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2).Value = model.Status ?? "A";
            cmd.Parameters.Add("P_USER_ID", OracleDbType.Int64).Value = userId ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult AddIidInqProceeding(IidInqProceedingRow model)
            {
            return SaveIidInqProceeding(model);
            }

        public IidInqProcResult UpdateIidInqProceeding(IidInqProceedingRow model)
            {
            return SaveIidInqProceeding(model);
            }

        public IidInqProcResult DeleteIidInqProceeding(long proceedingId, long userId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_DELETE_INQ_PROCEEDING";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_PROCEEDING_ID", OracleDbType.Int64).Value = proceedingId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = userId;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public List<IidInqStatementRow> GetIidInqStatementsByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_STATEMENTS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidInqStatementRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                list.Add(new IidInqStatementRow
                    {
                    StatementId = GetLongValue(rdr, "STATEMENT_ID"),
                    ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                    PersonName = GetStringValue(rdr, "PERSON_NAME"),
                    RoleType = GetStringValue(rdr, "ROLE_TYPE"),
                    PpnoNumber = GetStringValue(rdr, "PPNO_NUMBER"),
                    Cnic = DigitsOnly(GetStringValue(rdr, "CNIC")),
                    StatementDatetime = GetNullableDateValue(rdr, "STATEMENT_DATETIME"),
                    Place = GetStringValue(rdr, "PLACE"),
                    ModeType = GetStringValue(rdr, "MODE_TYPE"),
                    KeyPoints = GetStringValue(rdr, "KEY_POINTS"),
                    UploadedStatement = GetStringValue(rdr, "UPLOADED_STATEMENT"),
                    Status = GetStringValue(rdr, "STATUS"),
                    CreatedBy = GetNullableLongValue(rdr, "CREATED_BY"),
                    CreatedOn = GetDateValue(rdr, "CREATED_ON"),
                    UpdatedBy = GetNullableLongValue(rdr, "UPDATED_BY"),
                    UpdatedOn = GetNullableDateValue(rdr, "UPDATED_ON")
                    });
                }

            return list;
            }

        public IidInqProcResult SaveIidInqStatement(IidInqStatementRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            var userId = model?.UserId ?? model?.UpdatedBy ?? model?.CreatedBy;
            cmd.CommandText = "PKG_INQ.P_ADD_INQ_STATEMENT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = model.ComplaintId;
            cmd.Parameters.Add("P_PERSON_NAME", OracleDbType.Varchar2).Value = model.PersonName ?? string.Empty;
            cmd.Parameters.Add("P_ROLE_TYPE", OracleDbType.Varchar2).Value = model.RoleType ?? string.Empty;
            cmd.Parameters.Add("P_PPNO_NUMBER", OracleDbType.Varchar2).Value = model.PpnoNumber ?? string.Empty;
            cmd.Parameters.Add("P_CNIC", OracleDbType.Varchar2).Value = DigitsOnly(model.Cnic);
            cmd.Parameters.Add("P_STATEMENT_DATETIME", OracleDbType.Date).Value = model.StatementDatetime ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_PLACE", OracleDbType.Varchar2).Value = model.Place ?? string.Empty;
            cmd.Parameters.Add("P_MODE_TYPE", OracleDbType.Varchar2).Value = model.ModeType ?? string.Empty;
            cmd.Parameters.Add("P_KEY_POINTS", OracleDbType.Clob).Value = model.KeyPoints ?? string.Empty;
            cmd.Parameters.Add("P_UPLOADED_STATEMENT", OracleDbType.Clob).Value = model.UploadedStatement ?? string.Empty;
            cmd.Parameters.Add("P_USER_ID", OracleDbType.Int64).Value = userId ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult AddIidInqStatement(IidInqStatementRow model)
            {
            return SaveIidInqStatement(model);
            }

        public IidInqProcResult UpdateIidInqStatement(IidInqStatementRow model)
            {
            return SaveIidInqStatement(model);
            }

        public IidInqProcResult DeleteIidInqStatement(long statementId, long userId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_DELETE_INQ_STATEMENT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_STATEMENT_ID", OracleDbType.Int64).Value = statementId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = userId;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public List<IidInqEvidenceFileRow> GetIidInqEvidenceFilesByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_EVIDENCE_FILES";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidInqEvidenceFileRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                list.Add(new IidInqEvidenceFileRow
                    {
                    EvidenceId = GetLongValue(rdr, "EVIDENCE_ID"),
                    ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                    EvidenceType = GetStringValue(rdr, "EVIDENCE_TYPE"),
                    Description = GetStringValue(rdr, "DESCRIPTION"),
                    FileName = GetStringValue(rdr, "FILE_NAME"),
                    FilePath = GetStringValue(rdr, "FILE_PATH"),
                    FileExt = GetStringValue(rdr, "FILE_EXT"),
                    FileSizeKb = GetNullableIntValue(rdr, "FILE_SIZE_KB"),
                    Status = GetStringValue(rdr, "STATUS"),
                    UploadedBy = GetNullableLongValue(rdr, "UPLOADED_BY"),
                    UploadedOn = GetDateValue(rdr, "UPLOADED_ON"),
                    UpdatedBy = GetNullableLongValue(rdr, "UPDATED_BY"),
                    UpdatedOn = GetNullableDateValue(rdr, "UPDATED_ON")
                    });
                }

            return list;
            }

        public IidInqProcResult AddIidInqEvidenceFile(IidInqEvidenceFileRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_ADD_INQ_EVIDENCE_FILE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = model.ComplaintId;
            cmd.Parameters.Add("P_EVIDENCE_TYPE", OracleDbType.Varchar2).Value = model.EvidenceType ?? string.Empty;
            cmd.Parameters.Add("P_DESCRIPTION", OracleDbType.Clob).Value = model.Description ?? string.Empty;
            cmd.Parameters.Add("P_FILE_NAME", OracleDbType.Varchar2).Value = model.FileName ?? string.Empty;
            cmd.Parameters.Add("P_FILE_PATH", OracleDbType.Varchar2).Value = model.FilePath ?? string.Empty;
            cmd.Parameters.Add("P_FILE_EXT", OracleDbType.Varchar2).Value = model.FileExt ?? string.Empty;
            cmd.Parameters.Add("P_FILE_SIZE_KB", OracleDbType.Int32).Value = model.FileSizeKb ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_UPLOADED_BY", OracleDbType.Int64).Value = model.UploadedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult DeleteIidInqEvidenceFile(long evidenceId, long userId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_DELETE_INQ_EVIDENCE_FILE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_EVIDENCE_ID", OracleDbType.Int64).Value = evidenceId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = userId;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public List<IidInqViolationRow> GetIidInqViolationsByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_VIOLATIONS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidInqViolationRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                list.Add(new IidInqViolationRow
                    {
                    ViolationId = GetLongValue(rdr, "VIOLATION_ID"),
                    ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                    Category = GetStringValue(rdr, "CATEGORY"),
                    ViolationDetail = GetStringValue(rdr, "VIOLATION_DETAIL"),
                    ReferenceText = GetStringValue(rdr, "REFERENCE_TEXT"),
                    Recommendation = GetStringValue(rdr, "RECOMMENDATION"),
                    SortOrder = GetIntValue(rdr, "SORT_ORDER"),
                    Status = GetStringValue(rdr, "STATUS"),
                    CreatedBy = GetNullableLongValue(rdr, "CREATED_BY"),
                    CreatedOn = GetDateValue(rdr, "CREATED_ON"),
                    UpdatedBy = GetNullableLongValue(rdr, "UPDATED_BY"),
                    UpdatedOn = GetNullableDateValue(rdr, "UPDATED_ON")
                    });
                }

            return list;
            }

        public IidInqProcResult AddIidInqViolation(IidInqViolationRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_ADD_INQ_VIOLATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = model.ComplaintId;
            cmd.Parameters.Add("P_CATEGORY", OracleDbType.Varchar2).Value = model.Category ?? string.Empty;
            cmd.Parameters.Add("P_VIOLATION_DETAIL", OracleDbType.Clob).Value = model.ViolationDetail ?? string.Empty;
            cmd.Parameters.Add("P_REFERENCE_TEXT", OracleDbType.Varchar2).Value = model.ReferenceText ?? string.Empty;
            cmd.Parameters.Add("P_RECOMMENDATION", OracleDbType.Clob).Value = model.Recommendation ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_CREATED_BY", OracleDbType.Int64).Value = model.CreatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult UpdateIidInqViolation(IidInqViolationRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_UPDATE_INQ_VIOLATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_VIOLATION_ID", OracleDbType.Int64).Value = model.ViolationId;
            cmd.Parameters.Add("P_CATEGORY", OracleDbType.Varchar2).Value = model.Category ?? string.Empty;
            cmd.Parameters.Add("P_VIOLATION_DETAIL", OracleDbType.Clob).Value = model.ViolationDetail ?? string.Empty;
            cmd.Parameters.Add("P_REFERENCE_TEXT", OracleDbType.Varchar2).Value = model.ReferenceText ?? string.Empty;
            cmd.Parameters.Add("P_RECOMMENDATION", OracleDbType.Clob).Value = model.Recommendation ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = model.UpdatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult DeleteIidInqViolation(long violationId, long userId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_DELETE_INQ_VIOLATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_VIOLATION_ID", OracleDbType.Int64).Value = violationId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = userId;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }


        public List<IidInqFindingsRecommRow> GetIidInqFindingsRecommByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            var accusationId = 0;
            cmd.CommandText = "PKG_INQ.GET_INQ_FINDINGS_RECOMM";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("p_complaint_id", OracleDbType.Int64).Value = complaintId;
            cmd.Parameters.Add("p_accusation_id", OracleDbType.Int64).Value = accusationId;
            AddIidIoCursor(cmd);

            var list = new List<IidInqFindingsRecommRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                var accusationIdValue = GetLongValue(rdr, "ACCUSATION_ID");
                if (accusationIdValue <= 0)
                    {
                    continue;
                    }

                list.Add(new IidInqFindingsRecommRow
                    {
                    ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                    AccusationId = accusationIdValue,
                    FindingText = GetStringValue(rdr, "FINDING_TEXT"),
                    RecommendationText = GetStringValue(rdr, "RECOM_TEXT"),
                    Outcome = GetStringValue(rdr, "OUTCOME"),
                    Ppno = GetStringValue(rdr, "PPNO"),
                    UpdatedOn = GetNullableDateValue(rdr, "UPDATED_ON")
                    });
                }

            return list;
            }

        public IidInqProcResult SaveIidInqFindingsRecomm(long complaintId, long accusationId, string findingText, string recommendationText)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            var ppno = loggedInUser?.PPNumber ?? string.Empty;
            return SaveIidFindingsRecommByAccusation(complaintId, accusationId, findingText, recommendationText, ppno);
            }

        public List<IidAccusationForFindingsRow> GetIidAccusationsForFindings(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_ACCUSATIONS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("p_complaint_id", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidAccusationForFindingsRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                var accusationIdValue = GetLongValue(rdr, "ACCUSATION_ID");
                if (accusationIdValue <= 0)
                    {
                    continue;
                    }

                list.Add(new IidAccusationForFindingsRow
                    {
                    AccusationId = accusationIdValue,
                    AccusationText = GetStringValue(rdr, "ACCUSATION_TEXT")
                    });
                }

            return list;
            }

        public IidInqFindingsRecommRow GetIidFindingsRecommByAccusation(long complaintId, long accusationId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.GET_INQ_FINDINGS_RECOMM";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("p_complaint_id", OracleDbType.Int64).Value = complaintId;
            cmd.Parameters.Add("p_accusation_id", OracleDbType.Int64).Value = accusationId;
            AddIidIoCursor(cmd);

            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read())
                {
                return new IidInqFindingsRecommRow
                    {
                    ComplaintId = complaintId,
                    AccusationId = accusationId,
                    FindingText = string.Empty,
                    RecommendationText = string.Empty,
                    Outcome = string.Empty,
                    Ppno = string.Empty,
                    UpdatedOn = null
                    };
                }

            return new IidInqFindingsRecommRow
                {
                ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                AccusationId = GetLongValue(rdr, "ACCUSATION_ID"),
                FindingText = GetStringValue(rdr, "FINDING_TEXT"),
                RecommendationText = GetStringValue(rdr, "RECOM_TEXT"),
                Outcome = GetStringValue(rdr, "OUTCOME"),
                Ppno = GetStringValue(rdr, "PPNO"),
                UpdatedOn = GetNullableDateValue(rdr, "UPDATED_ON")
                };
            }

        public IidInqProcResult SaveIidFindingsRecommByAccusation(long complaintId, long accusationId, string findingText, string recomText, string ppno)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.SAVE_INQ_FINDINGS_RECOMM";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("p_complaint_id", OracleDbType.Int64).Value = complaintId;
            cmd.Parameters.Add("p_accusation_id", OracleDbType.Int64).Value = accusationId;
            cmd.Parameters.Add("p_finding_text", OracleDbType.Clob).Value = findingText ?? string.Empty;
            cmd.Parameters.Add("p_recom_text", OracleDbType.Clob).Value = recomText ?? string.Empty;
            cmd.Parameters.Add("p_ppno", OracleDbType.Varchar2).Value = ppno ?? string.Empty;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public List<IidFindingsRecommStatusRow> GetIidFindingsRecommStatus(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.GET_INQ_FIND_RECOMM_STATUS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("p_complaint_id", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidFindingsRecommStatusRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                var accusationIdValue = GetLongValue(rdr, "ACCUSATION_ID");
                if (accusationIdValue <= 0)
                    {
                    continue;
                    }

                list.Add(new IidFindingsRecommStatusRow
                    {
                    AccusationId = accusationIdValue,
                    AccusationText = GetStringValue(rdr, "ACCUSATION_TEXT"),
                    IsSaved = GetStringValue(rdr, "IS_SAVED"),
                    Outcome = GetStringValue(rdr, "OUTCOME"),
                    SavedOn = GetNullableDateValue(rdr, "SAVED_ON")
                    });
                }

            return list;
            }

        public IidInqEvidenceStepModel GetIidInqEvidenceStepByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_EVIDENCE_STEP";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var model = new IidInqEvidenceStepModel
                {
                ComplaintId = complaintId
                };

            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
                {
                model.MaterialEvidenceDetail = HasColumn(rdr, "MATERIAL_EVIDENCE_DETAIL") ? GetStringValue(rdr, "MATERIAL_EVIDENCE_DETAIL") : string.Empty;
                model.CircumstantialEvidenceDetail = HasColumn(rdr, "CIRCUMSTANTIAL_EVIDENCE_DETAIL") ? GetStringValue(rdr, "CIRCUMSTANTIAL_EVIDENCE_DETAIL") : string.Empty;
                }

            model.EvidenceFiles = GetIidInqEvidenceFilesByComplaintId(complaintId);
            return model;
            }

        public IidInqProcResult SaveIidInqEvidenceStep(IidInqEvidenceStepModel model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_SAVE_INQ_EVIDENCE_STEP";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = model.ComplaintId;
            cmd.Parameters.Add("P_MATERIAL_EVIDENCE_DETAIL", OracleDbType.Clob).Value = model.MaterialEvidenceDetail ?? string.Empty;
            cmd.Parameters.Add("P_CIRCUMSTANTIAL_EVIDENCE_DETAIL", OracleDbType.Clob).Value = model.CircumstantialEvidenceDetail ?? string.Empty;
            // Asad DB wiring: persist MATERIAL_EVIDENCE_DETAIL and CIRCUMSTANTIAL_EVIDENCE_DETAIL in the evidence-step proc.
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public List<IidInqDsaRow> GetIidInqDsaByComplaintId(long complaintId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_GET_INQ_DSA";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            AddIidIoCursor(cmd);

            var list = new List<IidInqDsaRow>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                list.Add(new IidInqDsaRow
                    {
                    DsaId = GetLongValue(rdr, "DSA_ID"),
                    ComplaintId = GetLongValue(rdr, "COMPLAINT_ID"),
                    PersonName = GetStringValue(rdr, "PERSON_NAME"),
                    Designation = GetStringValue(rdr, "DESIGNATION"),
                    PpnoNumber = GetStringValue(rdr, "PPNO_NUMBER"),
                    Cnic = DigitsOnly(GetStringValue(rdr, "CNIC")),
                    DsaStatus = GetStringValue(rdr, "DSA_STATUS"),
                    Remarks = GetStringValue(rdr, "REMARKS"),
                    SortOrder = GetIntValue(rdr, "SORT_ORDER"),
                    Status = GetStringValue(rdr, "STATUS"),
                    CreatedBy = GetNullableLongValue(rdr, "CREATED_BY"),
                    CreatedOn = GetDateValue(rdr, "CREATED_ON"),
                    UpdatedBy = GetNullableLongValue(rdr, "UPDATED_BY"),
                    UpdatedOn = GetNullableDateValue(rdr, "UPDATED_ON")
                    });
                }

            return list;
            }

        public IidInqProcResult AddIidInqDsa(IidInqDsaRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_ADD_INQ_DSA";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = model.ComplaintId;
            cmd.Parameters.Add("P_PERSON_NAME", OracleDbType.Varchar2).Value = model.PersonName ?? string.Empty;
            cmd.Parameters.Add("P_DESIGNATION", OracleDbType.Varchar2).Value = model.Designation ?? string.Empty;
            cmd.Parameters.Add("P_PPNO_NUMBER", OracleDbType.Varchar2).Value = model.PpnoNumber ?? string.Empty;
            cmd.Parameters.Add("P_CNIC", OracleDbType.Varchar2).Value = DigitsOnly(model.Cnic);
            cmd.Parameters.Add("P_DSA_STATUS", OracleDbType.Varchar2).Value = model.DsaStatus ?? string.Empty;
            cmd.Parameters.Add("P_REMARKS", OracleDbType.Varchar2).Value = model.Remarks ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_CREATED_BY", OracleDbType.Int64).Value = model.CreatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult UpdateIidInqDsa(IidInqDsaRow model)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_UPDATE_INQ_DSA";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_DSA_ID", OracleDbType.Int64).Value = model.DsaId;
            cmd.Parameters.Add("P_PERSON_NAME", OracleDbType.Varchar2).Value = model.PersonName ?? string.Empty;
            cmd.Parameters.Add("P_DESIGNATION", OracleDbType.Varchar2).Value = model.Designation ?? string.Empty;
            cmd.Parameters.Add("P_PPNO_NUMBER", OracleDbType.Varchar2).Value = model.PpnoNumber ?? string.Empty;
            cmd.Parameters.Add("P_CNIC", OracleDbType.Varchar2).Value = DigitsOnly(model.Cnic);
            cmd.Parameters.Add("P_DSA_STATUS", OracleDbType.Varchar2).Value = model.DsaStatus ?? string.Empty;
            cmd.Parameters.Add("P_REMARKS", OracleDbType.Varchar2).Value = model.Remarks ?? string.Empty;
            cmd.Parameters.Add("P_SORT_ORDER", OracleDbType.Int32).Value = model.SortOrder;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = model.UpdatedBy ?? (object)DBNull.Value;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        public IidInqProcResult DeleteIidInqDsa(long dsaId, long userId)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_DELETE_INQ_DSA";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_DSA_ID", OracleDbType.Int64).Value = dsaId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = userId;
            AddIidIoCursor(cmd);
            return ExecuteIidResult(cmd);
            }

        private static string DigitsOnly(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return string.Empty;
                }
            var buffer = new StringBuilder();
            foreach (var ch in value)
                {
                if (char.IsDigit(ch))
                    {
                    buffer.Append(ch);
                    }
                }
            return buffer.ToString();
            }

        public int FinalizeIidInquiryReport(long complaintId, long? updatedBy)
            {
            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_INQ.P_FINALIZE_IID_INQUIRY_REPORT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_COMPLAINT_ID", OracleDbType.Int64).Value = complaintId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Int64).Value = updatedBy ?? (object)DBNull.Value;
            cmd.ExecuteNonQuery();
            return 1;
            }


        private static void LogIidSaveDebug(string procedureName, string summary)
            {
            System.Diagnostics.Debug.WriteLine($"[IID SAVE] Procedure={procedureName}; {summary}");
            }

        private static bool ContainsFilterValue(DataRow row, string filterValue, params string[] columnNames)
            {
            if (row == null || string.IsNullOrWhiteSpace(filterValue))
                {
                return true;
                }

            var search = filterValue.Trim();
            foreach (var columnName in columnNames ?? Array.Empty<string>())
                {
                if (string.IsNullOrWhiteSpace(columnName) || !row.Table.Columns.Contains(columnName))
                    {
                    continue;
                    }

                var value = row[columnName]?.ToString() ?? string.Empty;
                if (value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                    return true;
                    }
                }

            return false;
            }

        private static bool EqualsFilterValue(DataRow row, int? filterValue, params string[] columnNames)
            {
            if (row == null || !filterValue.HasValue || filterValue.Value <= 0)
                {
                return true;
                }

            foreach (var columnName in columnNames ?? Array.Empty<string>())
                {
                if (string.IsNullOrWhiteSpace(columnName) || !row.Table.Columns.Contains(columnName))
                    {
                    continue;
                    }

                var value = row[columnName]?.ToString() ?? string.Empty;
                if (int.TryParse(value, out var parsed) && parsed == filterValue.Value)
                    {
                    return true;
                    }
                }

            return false;
            }

        private static DataTable ApplyReportFilter(DataTable source, ReportFilterModel filter)
            {
            if (source == null || source.Rows.Count == 0 || filter == null)
                {
                return source ?? new DataTable();
                }

            var filtered = source.Clone();
            foreach (DataRow row in source.Rows)
                {
                var include = true;

                include &= EqualsFilterValue(row, filter.ComplaintId, "COMPLAINT_ID", "ComplaintId");
                include &= ContainsFilterValue(row, filter.Complaint, "COMPLAINT_NO", "ComplaintNo", "COMPLAINT");
                include &= ContainsFilterValue(row, filter.Nature, "NATURE", "Nature");
                include &= ContainsFilterValue(row, filter.Source, "SOURCE", "RECEIVED_FROM", "Source");
                include &= ContainsFilterValue(row, filter.Category, "CATEGORY", "Category");
                include &= ContainsFilterValue(row, filter.PertainsTo, "PERTAINS_TO_SUMMARY", "PERTAINS_TO", "LOCATION_TYPE_TEXT");
                include &= ContainsFilterValue(row, filter.Region, "REGION", "REGION_NAME");
                include &= ContainsFilterValue(row, filter.Branch, "BRANCH", "BRANCH_NAME");
                include &= ContainsFilterValue(row, filter.Unit, "UNIT", "UNIT_NAME", "ASSIGNED_UNIT", "ASSIGNED_UNIT_NAME");
                include &= ContainsFilterValue(row, filter.Accused, "NAME_ACCUSED", "ACCUSED", "ACCUSED_NAME");
                include &= ContainsFilterValue(row, filter.Status, "STATUS", "Status");

                if (include)
                    {
                    filtered.ImportRow(row);
                    }
                }

            return filtered;
            }

        public DataTable GetReports(ReportFilterModel filter)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new DataTable();
                }
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
                return ApplyReportFilter(dt, filter);
                }
            }
        }
    }
