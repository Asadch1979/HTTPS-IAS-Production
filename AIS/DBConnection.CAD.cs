using AIS.Models;
using AIS.Models.HD;
using AIS.Models.HM;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        private static string SafeReadString(OracleDataReader reader, string columnName)
            {
            if (reader == null || string.IsNullOrWhiteSpace(columnName))
                {
                return null;
                }

            try
                {
                var value = reader[columnName];
                return value == DBNull.Value ? null : value.ToString();
                }
            catch (IndexOutOfRangeException)
                {
                try
                    {
                    var ordinal = reader.GetOrdinal(columnName);
                    if (ordinal >= 0)
                        {
                        var fallback = reader.GetValue(ordinal);
                        return fallback == DBNull.Value ? null : fallback.ToString();
                        }
                    }
                catch
                    {
                    // Column not available; ignore
                    }

                return null;
                }
            }

        private static long SafeReadLong(OracleDataReader reader, string columnName)
            {
            var value = SafeReadNullableLong(reader, columnName);
            return value ?? 0L;
            }

        private static long? SafeReadNullableLong(OracleDataReader reader, string columnName)
            {
            if (reader == null || string.IsNullOrWhiteSpace(columnName))
                {
                return null;
                }

            try
                {
                var value = reader[columnName];
                if (value == DBNull.Value)
                    {
                    return null;
                    }

                if (value is OracleDecimal oracleDecimal)
                    {
                    return oracleDecimal.IsNull ? (long?)null : oracleDecimal.ToInt64();
                    }

                return Convert.ToInt64(value);
                }
            catch
                {
                return null;
                }
            }

        private static int SafeReadInt(OracleDataReader reader, string columnName)
            {
            var value = SafeReadNullableInt(reader, columnName);
            return value ?? 0;
            }

        private static int? SafeReadNullableInt(OracleDataReader reader, string columnName)
            {
            if (reader == null || string.IsNullOrWhiteSpace(columnName))
                {
                return null;
                }

            try
                {
                var value = reader[columnName];
                if (value == DBNull.Value)
                    {
                    return null;
                    }

                if (value is OracleDecimal oracleDecimal)
                    {
                    return oracleDecimal.IsNull ? (int?)null : oracleDecimal.ToInt32();
                    }

                return Convert.ToInt32(value);
                }
            catch
                {
                return null;
                }
            }

        public SBPPasswordValidationResult ValidateSbpAccessPassword(string inputPassword)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var result = new SBPPasswordValidationResult { Success = false, Message = "Invalid password." };
            using (var con = this.DatabaseConnection())
                {

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_VALIDATE_SBP_PASSWORD";
                    cmd.CommandType = CommandType.StoredProcedure;
                    var hashedInput = HashPassword(inputPassword ?? string.Empty);
                    cmd.Parameters.Add("p_input_key", OracleDbType.Varchar2, 200).Value = hashedInput;
                    var output = new OracleParameter("p_is_valid", OracleDbType.Varchar2, 1)
                        {
                        Direction = ParameterDirection.Output
                        };
                    cmd.Parameters.Add(output);
                    cmd.ExecuteNonQuery();
                    var flag = (output.Value ?? string.Empty).ToString();
                    result.Success = string.Equals(flag, "Y", StringComparison.OrdinalIgnoreCase);
                    result.Message = result.Success ? "Authenticated." : "Invalid password.";
                    }
                }
            return result;
            }

        public long InsertSbpObservation(SBPObservationCreateModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            if (model == null)
                {
                throw new ArgumentNullException(nameof(model));
                }

            if (model.ObservationTypeId == null || model.ObservationTypeId <= 0)
                {
                throw new ArgumentException("Observation type id is required.", nameof(model.ObservationTypeId));
                }
            var observationTypeId = model.ObservationTypeId.Value;
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_INSERT_SBP_OBSERVATION";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_ref_no", OracleDbType.Varchar2, 20).Value = model.RefNo;
                    cmd.Parameters.Add("p_function_name", OracleDbType.Varchar2, 255).Value = model.FunctionName;
                    cmd.Parameters.Add("p_para_no", OracleDbType.Varchar2, 50).Value = string.IsNullOrWhiteSpace(model.ParaNo) ? (object)DBNull.Value : model.ParaNo;
                    cmd.Parameters.Add("p_sbp_obs_text", OracleDbType.Clob).Value = model.SBPObservation;
                    cmd.Parameters.Add("p_sbp_dir_text", OracleDbType.Varchar2, 500).Value = string.IsNullOrWhiteSpace(model.SBPDirections) ? (object)DBNull.Value : model.SBPDirections;
                    cmd.Parameters.Add("p_compliance_qtr", OracleDbType.Varchar2, 50).Value = string.IsNullOrWhiteSpace(model.ComplianceQuarter) ? (object)DBNull.Value : model.ComplianceQuarter;
                    cmd.Parameters.Add("p_observation_type", OracleDbType.Int64).Value = observationTypeId;
                    cmd.Parameters.Add("p_user", OracleDbType.Varchar2, 50).Value = model.User;
                    var paraIdParameter = new OracleParameter("p_para_id", OracleDbType.Int64)
                        {
                        Direction = ParameterDirection.Output
                        };
                    cmd.Parameters.Add(paraIdParameter);
                    cmd.ExecuteNonQuery();

                    long paraId = 0;
                    var paraIdValue = paraIdParameter.Value;
                    if (paraIdValue != null && paraIdValue != DBNull.Value)
                        {
                        if (paraIdValue is OracleDecimal oracleDecimal)
                            {
                            if (!oracleDecimal.IsNull)
                                {
                                paraId = oracleDecimal.ToInt64();
                                }
                            }
                        else
                            {
                            paraId = Convert.ToInt64(paraIdValue);
                            }
                        }

                    model.ParaId = paraId;
                    model.ObservationTypeId = observationTypeId;
                    return paraId;
                    }
                }
            }

        public void InsertSbpObservationResponse(SBPObservationResponseCreateModel model)
            {
            if (model == null)
                {
                throw new ArgumentNullException(nameof(model));
                }

            if (model.ParaId <= 0)
                {
                throw new ArgumentException("ParaId must be greater than zero.", nameof(model.ParaId));
                }

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_INSERT_SBP_RESPONSE";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_para_id", OracleDbType.Int64).Value = model.ParaId;
                    cmd.Parameters.Add("p_ref_no", OracleDbType.Varchar2, 20).Value = model.RefNo;
                    cmd.Parameters.Add("p_bank_response", OracleDbType.Clob).Value = model.BankResponse;
                    var replyDate = model.ReplyDate ?? DateTime.UtcNow;
                    cmd.Parameters.Add("p_reply_date", OracleDbType.Date).Value = replyDate;
                    cmd.Parameters.Add("p_compliance_status", OracleDbType.Varchar2, 50).Value = model.ComplianceStatus;
                    cmd.Parameters.Add("p_iad_validation", OracleDbType.Varchar2, 50).Value = string.IsNullOrWhiteSpace(model.IADValidation) ? (object)DBNull.Value : model.IADValidation;
                    cmd.Parameters.Add("p_user", OracleDbType.Varchar2, 50).Value = model.User;
                    cmd.ExecuteNonQuery();
                    }
                }
            }

        public SBPObservationHistoryItem GetSbpObservationResponse(long responseId)
            {
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_GET_SBP_RESPONSE";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_response_id", OracleDbType.Int64).Value = responseId;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        if (reader.Read())
                            {
                            var item = new SBPObservationHistoryItem
                                {
                                ResponseId = reader["RESPONSE_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["RESPONSE_ID"]),
                                ParaId = reader["PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["PARA_ID"]),
                                BankResponse = reader["BANK_RESPONSE"] == DBNull.Value ? null : reader["BANK_RESPONSE"].ToString(),
                                ComplianceStatus = reader["COMPLIANCE_STATUS"] == DBNull.Value ? null : reader["COMPLIANCE_STATUS"].ToString(),
                                IADValidation = reader["IAD_VALIDATION"] == DBNull.Value ? null : reader["IAD_VALIDATION"].ToString(),
                                EnteredBy = reader["ENTERED_BY"] == DBNull.Value ? null : reader["ENTERED_BY"].ToString()
                                };
                            item.ReplyDate = reader["REPLY_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["REPLY_DATE"]);
                            item.EnteredOn = reader["ENTERED_ON"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ENTERED_ON"]);
                            return item;
                            }
                        }
                    }
                }

            return null;
            }

        public (string Success, string Message) UpdateSbpObservationResponse(SBPObservationResponseUpdateModel model)
            {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (model.ParaId <= 0)
                throw new ArgumentException("ParaId must be greater than zero.", nameof(model.ParaId));

            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_UPDATE_SBP_RESPONSE";
                    cmd.CommandType = CommandType.StoredProcedure;

                    // IN parameters
                    cmd.Parameters.Add("p_response_id", OracleDbType.Int64)
                        .Value = model.ResponseId == 0 ? (object)DBNull.Value : model.ResponseId;

                    cmd.Parameters.Add("p_para_id", OracleDbType.Int64).Value = model.ParaId;
                    cmd.Parameters.Add("p_ref_no", OracleDbType.Varchar2, 50).Value = model.RefNo;

                    cmd.Parameters.Add("p_bank_response", OracleDbType.Clob).Value = model.BankResponse;


                    var replyDate = model.ReplyDate ?? DateTime.UtcNow;
                    cmd.Parameters.Add("p_reply_date", OracleDbType.Date).Value = replyDate;

                    cmd.Parameters.Add("p_compliance_status", OracleDbType.Varchar2, 50)
                        .Value = model.ComplianceStatus;

                    cmd.Parameters.Add("p_iad_validation", OracleDbType.Varchar2, 50)
                        .Value = string.IsNullOrWhiteSpace(model.IADValidation)
                                    ? (object)DBNull.Value
                                    : model.IADValidation;

                    cmd.Parameters.Add("p_user", OracleDbType.Varchar2, 50)
                        .Value = string.IsNullOrWhiteSpace(model.User)
                                    ? (object)DBNull.Value
                                    : model.User;

                    // OUT parameters
                    var outSuccess = cmd.Parameters.Add("p_success", OracleDbType.Varchar2, 10);
                    outSuccess.Direction = ParameterDirection.Output;

                    var outMessage = cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 4000);
                    outMessage.Direction = ParameterDirection.Output;

                    // Execute
                    cmd.ExecuteNonQuery();

                    // Read OUT values
                    string success = outSuccess.Value?.ToString();
                    string message = outMessage.Value?.ToString();

                    return (success, message);
                    }
                }
            }


        public List<SBPObservationRegisterItem> GetSbpObservationRegister(long observationTypeId)
            {
            if (observationTypeId <= 0)
                {
                throw new ArgumentException("Observation type id is required.", nameof(observationTypeId));
                }

            var items = new List<SBPObservationRegisterItem>();
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_GET_SBP_REGISTER";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_observation_type", OracleDbType.Int64).Value = observationTypeId;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            var item = new SBPObservationRegisterItem
                                {
                                ParaId = reader["PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["PARA_ID"]),
                                RefNo = reader["REF_NO"] == DBNull.Value ? null : reader["REF_NO"].ToString(),
                                FunctionName = reader["FUNCTION_NAME"] == DBNull.Value ? null : reader["FUNCTION_NAME"].ToString(),
                                ParaNo = reader["PARA_NO"] == DBNull.Value ? null : reader["PARA_NO"].ToString(),
                                SBPObservation = reader["SBP_OBSERVATION"] == DBNull.Value ? null : reader["SBP_OBSERVATION"].ToString(),
                                SBPDirections = reader["SBP_DIRECTIONS"] == DBNull.Value ? null : reader["SBP_DIRECTIONS"].ToString(),
                                LatestBankResponse = reader["BANK_RESPONSE"] == DBNull.Value ? null : reader["BANK_RESPONSE"].ToString(),
                                LatestResponseId = SafeReadNullableLong(reader, "RESPONSE_ID")
                                    ?? SafeReadNullableLong(reader, "LATEST_RESPONSE_ID"),
                                ComplianceStatus = reader["COMPLIANCE_STATUS"] == DBNull.Value ? null : reader["COMPLIANCE_STATUS"].ToString(),
                                IADValidation = reader["IAD_VALIDATION"] == DBNull.Value ? null : reader["IAD_VALIDATION"].ToString(),
                                ComplianceQuarter = reader["COMPLIANCE_QUARTER"] == DBNull.Value ? null : reader["COMPLIANCE_QUARTER"].ToString(),
                                EnteredBy = reader["ENTERED_BY"] == DBNull.Value ? null : reader["ENTERED_BY"].ToString(),
                                ObservationTypeId = SafeReadNullableLong(reader, "OBSERVATION_TYPE_ID"),
                                ObservationTypeName = SafeReadString(reader, "OBSERVATION_TYPE_NAME") ?? SafeReadString(reader, "OBSERVATION_TYPE")
                                };
                            item.ReplyDate = reader["REPLY_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["REPLY_DATE"]);
                            item.EnteredOn = reader["ENTERED_ON"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ENTERED_ON"]);
                            item.CreatedOn = reader["CREATED_ON"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CREATED_ON"]);
                            items.Add(item);
                            }
                        }
                    }
                }
            return items;
            }

        public IReadOnlyList<SbpObservationTypeOption> GetSbpObservationTypes()
            {
            var types = new List<SbpObservationTypeOption>();
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_GET_SBP_OBS_TYPES";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            var type = new SbpObservationTypeOption
                                {
                                ObservationTypeId = SafeReadInt(reader, "OBSERVATION_TYPE_ID"),
                                ObservationTypeCode = SafeReadString(reader, "OBSERVATION_TYPE_CODE"),
                                ObservationTypeName = SafeReadString(reader, "OBSERVATION_TYPE_NAME") ?? SafeReadString(reader, "OBSERVATION_TYPE"),
                                ActiveFlag = SafeReadString(reader, "ACTIVE_FLAG"),
                                SortOrder = SafeReadNullableInt(reader, "SORT_ORDER")
                                };

                            if (type.ObservationTypeId > 0 && !string.IsNullOrWhiteSpace(type.ObservationTypeName))
                                {
                                types.Add(type);
                                }
                            }
                        }
                    }
                }

            return types
                .Where(type => type != null && type.ObservationTypeId > 0 && type.IsActive && !string.IsNullOrWhiteSpace(type.ObservationTypeName))
                .OrderBy(type => type.SortOrder ?? int.MaxValue)
                .ThenBy(type => type.ObservationTypeName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            }
        public (bool Success, string Message) UpdateSbpObservation(SBPObservationCreateModel model)
            {

            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            if (model?.ParaId == null || model.ParaId <= 0)
                {
                throw new ArgumentException("ParaId must be provided for update operations.", nameof(model.ParaId));
                }

            if (model.ObservationTypeId == null || model.ObservationTypeId <= 0)
                {
                throw new ArgumentException("Observation type id is required for updates.", nameof(model.ObservationTypeId));
                }
            var observationTypeId = model.ObservationTypeId.Value;

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_UPDATE_SBP_OBSERVATION";
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Match parameter names and types EXACTLY
                    cmd.Parameters.Add("p_para_id", OracleDbType.Int64).Value = model.ParaId;
                    cmd.Parameters.Add("p_ref_no", OracleDbType.Varchar2, 20).Value = model.RefNo;
                    cmd.Parameters.Add("p_function_name", OracleDbType.Varchar2, 255).Value = model.FunctionName;
                    cmd.Parameters.Add("p_para_no", OracleDbType.Varchar2, 50).Value =
                        string.IsNullOrWhiteSpace(model.ParaNo) ? (object)DBNull.Value : model.ParaNo;

                    cmd.Parameters.Add("p_sbp_observation", OracleDbType.Clob).Value =
                        string.IsNullOrWhiteSpace(model.SBPObservation) ? (object)DBNull.Value : model.SBPObservation;

                    cmd.Parameters.Add("p_sbp_directions", OracleDbType.Clob).Value =
                        string.IsNullOrWhiteSpace(model.SBPDirections) ? (object)DBNull.Value : model.SBPDirections;

                    cmd.Parameters.Add("p_compliance_quarter", OracleDbType.Varchar2, 50).Value =
                        string.IsNullOrWhiteSpace(model.ComplianceQuarter) ? (object)DBNull.Value : model.ComplianceQuarter;

                    cmd.Parameters.Add("p_observation_type", OracleDbType.Int64).Value = observationTypeId;

                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;

                    // OUT parameters
                    var pSuccess = new OracleParameter("p_success", OracleDbType.Varchar2, 1)
                        {
                        Direction = ParameterDirection.Output
                        };
                    var pMessage = new OracleParameter("p_message", OracleDbType.Varchar2, 4000)
                        {
                        Direction = ParameterDirection.Output
                        };

                    cmd.Parameters.Add(pSuccess);
                    cmd.Parameters.Add(pMessage);

                    try
                        {
                        cmd.ExecuteNonQuery();
                        bool success = (pSuccess.Value?.ToString() ?? "N") == "Y";
                        string message = pMessage.Value?.ToString() ?? string.Empty;
                        return (success, message);
                        }
                    catch (Exception ex)
                        {
                        return (false, ex.Message);
                        }
                    }
                }
            }
        public SBPObservationHistoryModel GetSbpObservationHistory(long paraId)
            {
            var history = new SBPObservationHistoryModel { ParaId = paraId };
            var headerFilled = false;
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_GET_SBP_HISTORY";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_para_id", OracleDbType.Int64).Value = paraId;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            if (!headerFilled)
                                {
                                history.ParaId = reader["PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["PARA_ID"]);
                                history.RefNo = reader["REF_NO"] == DBNull.Value ? null : reader["REF_NO"].ToString();
                                history.FunctionName = reader["FUNCTION_NAME"] == DBNull.Value ? null : reader["FUNCTION_NAME"].ToString();
                                history.ParaNo = reader["PARA_NO"] == DBNull.Value ? null : reader["PARA_NO"].ToString();
                                history.SBPObservation = reader["SBP_OBSERVATION"] == DBNull.Value ? null : reader["SBP_OBSERVATION"].ToString();
                                history.SBPDirections = reader["SBP_DIRECTIONS"] == DBNull.Value ? null : reader["SBP_DIRECTIONS"].ToString();
                                headerFilled = true;
                                }

                            if (reader["RESPONSE_ID"] != DBNull.Value)
                                {
                                var response = new SBPObservationHistoryItem
                                    {
                                    ResponseId = Convert.ToInt64(reader["RESPONSE_ID"]),
                                    ParaId = reader["PARA_ID"] == DBNull.Value ? history.ParaId : Convert.ToInt64(reader["PARA_ID"]),
                                    BankResponse = reader["BANK_RESPONSE"] == DBNull.Value ? null : reader["BANK_RESPONSE"].ToString(),
                                    ComplianceStatus = reader["COMPLIANCE_STATUS"] == DBNull.Value ? null : reader["COMPLIANCE_STATUS"].ToString(),
                                    IADValidation = reader["IAD_VALIDATION"] == DBNull.Value ? null : reader["IAD_VALIDATION"].ToString(),
                                    EnteredBy = reader["ENTERED_BY"] == DBNull.Value ? null : reader["ENTERED_BY"].ToString()
                                    };
                                response.ReplyDate = reader["REPLY_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["REPLY_DATE"]);
                                response.EnteredOn = reader["ENTERED_ON"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ENTERED_ON"]);
                                history.Responses.Add(response);
                                }
                            }
                        }
                    }
                }
            return history;
            }

        public (bool Success, string Message) UpdateSbpPassword(string newPassword, string updatedBy)
            {
            var sessionHandler = CreateSessionHandler();

            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUserOrThrow();
                var ppNumber = loggedInUser.PPNumber;
                var roleId = loggedInUser.UserGroupID;

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_hd.P_Update_Cad_Passwrod";
                    cmd.CommandType = CommandType.StoredProcedure;

                    // --- Input parameters ---
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = roleId;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = ppNumber;
                    var hashedPassword = HashPassword(newPassword ?? string.Empty);
                    cmd.Parameters.Add("pMd5Hex", OracleDbType.Varchar2).Value = hashedPassword;

                    // --- Output parameters ---
                    var pSuccess = new OracleParameter("p_success", OracleDbType.Varchar2, 1)
                        {
                        Direction = ParameterDirection.Output
                        };
                    var pMessage = new OracleParameter("p_message", OracleDbType.Varchar2, 4000)
                        {
                        Direction = ParameterDirection.Output
                        };
                    cmd.Parameters.Add(pSuccess);
                    cmd.Parameters.Add(pMessage);

                    try
                        {
                        cmd.ExecuteNonQuery();

                        var successVal = pSuccess.Value?.ToString();
                        var messageVal = pMessage.Value?.ToString();

                        var success = string.Equals(successVal, "Y", StringComparison.OrdinalIgnoreCase);
                        var message = !string.IsNullOrWhiteSpace(messageVal)
                            ? messageVal
                            : (success ? "Password updated successfully." : "Password update failed.");

                        return (success, message);
                        }
                    catch (Exception)
                        {
                        return (false, "Password update failed.");
                        }
                    }
                }
            }
        public (bool Success, string Message, long RequestId) RequestDeleteObservation(long paraId, string reason)
            {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_REQUEST_DELETE_OBSERVATION";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_para_id", OracleDbType.Int64).Value = paraId;
                    cmd.Parameters.Add("p_reason", OracleDbType.Clob).Value = (object)(reason ?? string.Empty);
                    cmd.Parameters.Add("p_ppnumber", OracleDbType.Int64).Value = user.PPNumber;

                    var o_request_id = new OracleParameter("o_request_id", OracleDbType.Int64) { Direction = ParameterDirection.Output };
                    var o_msg = new OracleParameter("o_msg", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(o_request_id);
                    cmd.Parameters.Add(o_msg);

                    cmd.ExecuteNonQuery();

                    long reqId = 0;
                    long.TryParse(o_request_id.Value?.ToString(), out reqId);
                    var msg = o_msg.Value?.ToString() ?? string.Empty;
                    var ok = reqId > 0 && msg.IndexOf("submitted", StringComparison.OrdinalIgnoreCase) >= 0;

                    return (ok, msg, reqId);
                    }
                }
            }

        public (bool Success, string Message, long RequestId) RequestDeleteResponse(long responseId, string reason)
            {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_REQUEST_DELETE_RESPONSE";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_response_id", OracleDbType.Int64).Value = responseId;
                    cmd.Parameters.Add("p_reason", OracleDbType.Clob).Value = (object)(reason ?? string.Empty);
                    cmd.Parameters.Add("p_ppnumber", OracleDbType.Int64).Value = user.PPNumber;

                    var o_request_id = new OracleParameter("o_request_id", OracleDbType.Int64) { Direction = ParameterDirection.Output };
                    var o_msg = new OracleParameter("o_msg", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(o_request_id);
                    cmd.Parameters.Add(o_msg);

                    cmd.ExecuteNonQuery();

                    long reqId = 0;
                    long.TryParse(o_request_id.Value?.ToString(), out reqId);
                    var msg = o_msg.Value?.ToString() ?? string.Empty;
                    var ok = reqId > 0 && msg.IndexOf("submitted", StringComparison.OrdinalIgnoreCase) >= 0;

                    return (ok, msg, reqId);
                    }
                }
            }

        public (bool Success, string Message, long RequestId) RequestReverse(long requestIdToReverse, string reason)
            {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_REQUEST_REVERSE";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_request_id_to_reverse", OracleDbType.Int64).Value = requestIdToReverse;
                    cmd.Parameters.Add("p_reason", OracleDbType.Clob).Value = (object)(reason ?? string.Empty);
                    cmd.Parameters.Add("p_ppnumber", OracleDbType.Int64).Value = user.PPNumber;

                    var o_request_id = new OracleParameter("o_request_id", OracleDbType.Int64) { Direction = ParameterDirection.Output };
                    var o_msg = new OracleParameter("o_msg", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(o_request_id);
                    cmd.Parameters.Add(o_msg);

                    cmd.ExecuteNonQuery();

                    long reqId = 0;
                    long.TryParse(o_request_id.Value?.ToString(), out reqId);
                    var msg = o_msg.Value?.ToString() ?? string.Empty;
                    var ok = reqId > 0 && msg.IndexOf("submitted", StringComparison.OrdinalIgnoreCase) >= 0;

                    return (ok, msg, reqId);
                    }
                }
            }

        public (bool Success, string Message) ApproveRequest(long requestId)
            {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_APPROVE_REQUEST";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_request_id", OracleDbType.Int64).Value = requestId;
                    cmd.Parameters.Add("p_approver_pp", OracleDbType.Int64).Value = user.PPNumber;

                    var o_msg = new OracleParameter("o_msg", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(o_msg);

                    cmd.ExecuteNonQuery();

                    var msg = o_msg.Value?.ToString() ?? string.Empty;
                    var ok = msg.IndexOf("approved", StringComparison.OrdinalIgnoreCase) >= 0
                        || msg.IndexOf("applied", StringComparison.OrdinalIgnoreCase) >= 0;

                    return (ok, msg);
                    }
                }
            }

        public (bool Success, string Message) RejectRequest(long requestId, string reason)
            {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_REJECT_REQUEST";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_request_id", OracleDbType.Int64).Value = requestId;
                    cmd.Parameters.Add("p_approver_pp", OracleDbType.Int64).Value = user.PPNumber;
                    cmd.Parameters.Add("p_reason", OracleDbType.Varchar2, 4000).Value = (object)(reason ?? "Rejected");

                    var o_msg = new OracleParameter("o_msg", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(o_msg);

                    cmd.ExecuteNonQuery();

                    var msg = o_msg.Value?.ToString() ?? string.Empty;
                    var ok = msg.IndexOf("rejected", StringComparison.OrdinalIgnoreCase) >= 0;

                    return (ok, msg);
                    }
                }
            }

        public List<Dictionary<string, object>> GetRequests(string status)
            {
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_GET_REQUESTS";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_status", OracleDbType.Varchar2, 50).Value =
                        string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status;

                    var cursor = new OracleParameter("io_cursor", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(cursor);

                    using (var reader = cmd.ExecuteReader())
                        {
                        var rows = new List<Dictionary<string, object>>();
                        while (reader.Read())
                            {
                            var o = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < reader.FieldCount; i++)
                                {
                                o[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }
                            rows.Add(o);
                            }
                        return rows;
                        }
                    }
                }
            }

        public List<Dictionary<string, object>> GetRequestHistory(long requestId)
            {
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_HD.P_GET_REQUEST_HISTORY";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_request_id", OracleDbType.Int64).Value = requestId;

                    var cursor = new OracleParameter("io_cursor", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(cursor);

                    using (var reader = cmd.ExecuteReader())
                        {
                        var rows = new List<Dictionary<string, object>>();
                        while (reader.Read())
                            {
                            var o = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < reader.FieldCount; i++)
                                {
                                o[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }
                            rows.Add(o);
                            }
                        return rows;
                        }
                    }
                }
            }
        }
    }
