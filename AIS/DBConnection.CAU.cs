using AIS.Models.CAU;
using Ganss.Xss;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace AIS.Controllers
    {
        public partial class DBConnection
        {
        private void TryLogCommercialAuditSaveFailure(string action, string message, Exception ex, string userPpno)
            {
            try
                {
                var detail = ex?.GetBaseException()?.Message ?? ex?.Message ?? "Unknown save failure.";
                LogError("CommercialAudit", nameof(DBConnection), action, message, detail, null, null, userPpno);
                }
            catch
                {
                // Best-effort logging only; never hide the original save exception.
                }
            }

        private static DateTime? SafeReadNullableDate(OracleDataReader reader, string columnName)
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

                if (value is OracleDate oracleDate)
                    {
                    return oracleDate.IsNull ? (DateTime?)null : oracleDate.Value;
                    }

                return Convert.ToDateTime(value);
                }
            catch
                {
                return null;
                }
            }

        private static string CleanHtmlForReport(string input)
            {
            if (string.IsNullOrWhiteSpace(input))
                {
                return string.Empty;
                }

            var text = DecodeHtmlRepeatedly(input)
                .Replace("\u00A0", " ")
                .Trim();

            text = Regex.Replace(
                text,
                @"(^|\r\n|\r|\n)\s*((?:(?:DAC|PAC)\s+)?\d{2}-[A-Z]{3}-\d{4}:)",
                "$1<strong>$2</strong> ",
                RegexOptions.IgnoreCase);

            text = Regex.Replace(text, @"\r\n|\r|\n", "<br />");

            return CreateCommercialAuditReportSanitizer().Sanitize(text).Trim();
            }

        private static string DecodeHtmlRepeatedly(string input)
            {
            var current = input ?? string.Empty;

            for (var index = 0; index < 4; index++)
                {
                var decoded = WebUtility.HtmlDecode(current) ?? string.Empty;
                if (string.Equals(decoded, current, StringComparison.Ordinal))
                    {
                    break;
                    }

                current = decoded;
                }

            return current;
            }

        private static HtmlSanitizer CreateCommercialAuditReportSanitizer()
            {
            var sanitizer = new HtmlSanitizer();

            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowedSchemes.Add("http");
            sanitizer.AllowedSchemes.Add("https");

            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.UnionWith(new[]
                {
                "a", "b", "blockquote", "br", "caption", "col", "colgroup", "div", "em", "h1", "h2", "h3", "h4",
                "h5", "h6", "hr", "i", "li", "ol", "p", "span", "strong", "sub", "sup", "table", "tbody", "td",
                "tfoot", "th", "thead", "tr", "u", "ul"
                });

            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedAttributes.UnionWith(new[] { "href", "title", "colspan", "rowspan", "target", "rel" });

            sanitizer.AllowedCssProperties.Clear();
            sanitizer.AllowedClasses.Clear();

            return sanitizer;
            }

        private static void AddNullableIntParameter(OracleCommand cmd, string name, int? value)
            {
            cmd.Parameters.Add(name, OracleDbType.Int32).Value = value.HasValue ? value.Value : (object)DBNull.Value;
            }

        private static void AddNullableDateParameter(OracleCommand cmd, string name, DateTime? value)
            {
            cmd.Parameters.Add(name, OracleDbType.Date).Value = value.HasValue ? value.Value : (object)DBNull.Value;
            }

        private static string NormalizeActiveFlag(string value)
            {
            return string.Equals(value, "N", StringComparison.OrdinalIgnoreCase) ? "N" : "Y";
            }

        private CommercialAuditActionResultModel BuildCommercialAuditActionResult(OracleCommand cmd, string idParameterName = "P_ID")
            {
            var result = new CommercialAuditActionResultModel
                {
                Status = cmd.Parameters.Contains("P_STATUS") ? Convert.ToString(cmd.Parameters["P_STATUS"].Value) : "SUCCESS",
                Message = cmd.Parameters.Contains("P_MESSAGE") ? Convert.ToString(cmd.Parameters["P_MESSAGE"].Value) : "Saved successfully."
                };

            if (cmd.Parameters.Contains(idParameterName))
                {
                result.Id = ReadOutputNullableInt(cmd.Parameters[idParameterName].Value) ?? 0;
                }

            if (string.IsNullOrWhiteSpace(result.Status))
                {
                result.Status = result.Id > 0 ? "SUCCESS" : "FAILED";
                }

            if (string.IsNullOrWhiteSpace(result.Message))
                {
                result.Message = result.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase)
                    ? "Saved successfully."
                    : "Unable to save record.";
                }

            return result;
            }

        private static int? ReadOutputNullableInt(object value)
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
                return int.TryParse(Convert.ToString(value), out var parsed) ? parsed : (int?)null;
                }
            }

        private CommercialAuditActionResultModel InvalidCommercialAuditSessionResult()
            {
            return new CommercialAuditActionResultModel
                {
                Status = "FAILED",
                Message = "Invalid user session."
                };
            }

        public CommercialAuditActionResultModel SaveCommercialAuditOm(CommercialAuditOmModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return InvalidCommercialAuditSessionResult();
                }

            using (var con = this.DatabaseConnection())
                {
                try
                    {
                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_SAVE_OM";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.BindByName = true;
                        cmd.Parameters.Clear();

                        AddNullableIntParameter(cmd, "P_OM_ID", model?.OmId);
                        AddNullableIntParameter(cmd, "P_AUDIT_YEAR_ID", model?.AuditYearId);
                        cmd.Parameters.Add("P_OM_NO", OracleDbType.Varchar2).Value = model?.OmNo ?? string.Empty;
                        cmd.Parameters.Add("P_GIST_OF_OM", OracleDbType.Varchar2).Value = model?.GistOfOm ?? string.Empty;
                        cmd.Parameters.Add("P_BODY_OF_OM", OracleDbType.Clob).Value = model?.BodyOfOm ?? string.Empty;
                        cmd.Parameters.Add("P_MANAGEMENT_RESPONSE", OracleDbType.Clob).Value = model?.ManagementResponse ?? string.Empty;
                        cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = NormalizeActiveFlag(model?.IsActive);
                        cmd.Parameters.Add("P_USER_PPNO", OracleDbType.Int32).Value = Convert.ToInt32(loggedInUser.PPNumber);
                        cmd.Parameters.Add("P_USER_ROLE_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("P_USER_ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID.GetValueOrDefault();
                        cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_ID", OracleDbType.Int32).Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();
                        return BuildCommercialAuditActionResult(cmd);
                        }
                    }
                catch (OracleException ex)
                    {
                    TryLogCommercialAuditSaveFailure(nameof(SaveCommercialAuditOm), "Commercial Audit OM save failed at database layer.", ex, loggedInUser.PPNumber);
                    throw;
                    }
                catch (Exception ex)
                    {
                    TryLogCommercialAuditSaveFailure(nameof(SaveCommercialAuditOm), "Commercial Audit OM save failed before completion.", ex, loggedInUser.PPNumber);
                    throw;
                    }
                }
            }

        public List<CommercialAuditOmModel> GetCommercialAuditOms()
            {
            var list = new List<CommercialAuditOmModel>();

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_GET_OMS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            list.Add(new CommercialAuditOmModel
                                {
                                OmId = SafeReadNullableInt(dr, "OM_ID"),
                                AuditYearId = SafeReadNullableInt(dr, "AUDIT_YEAR_ID"),
                                AuditYearText = SafeReadString(dr, "AUDIT_YEAR_TEXT"),
                                OmNo = SafeReadString(dr, "OM_NO"),
                                GistOfOm = SafeReadString(dr, "GIST_OF_OM"),
                                BodyOfOm = SafeReadString(dr, "BODY_OF_OM"),
                                ManagementResponse = SafeReadString(dr, "MANAGEMENT_RESPONSE"),
                                IsActive = NormalizeActiveFlag(SafeReadString(dr, "IS_ACTIVE"))
                                });
                            }
                        }
                    }
                }

            return list;
            }

        public CommercialAuditActionResultModel SaveCommercialAuditPdp(CommercialAuditPdpModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return InvalidCommercialAuditSessionResult();
                }

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_SAVE_PDP";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();

                    AddNullableIntParameter(cmd, "P_PDP_ID", model?.PdpId);
                    AddNullableIntParameter(cmd, "P_AUDIT_YEAR_ID", model?.AuditYearId);
                    cmd.Parameters.Add("P_PDP_NO", OracleDbType.Varchar2).Value = model?.PdpNo ?? string.Empty;
                    cmd.Parameters.Add("P_GIST_OF_PDP", OracleDbType.Varchar2).Value = model?.GistOfPdp ?? string.Empty;
                    cmd.Parameters.Add("P_BODY_OF_PDP", OracleDbType.Clob).Value = model?.BodyOfPdp ?? string.Empty;
                    cmd.Parameters.Add("P_MANAGEMENT_RESPONSE", OracleDbType.Clob).Value = model?.ManagementResponse ?? string.Empty;
                    cmd.Parameters.Add("P_DAC_RECOMMENDATIONS", OracleDbType.Clob).Value = model?.DacRecommendations ?? string.Empty;
                    cmd.Parameters.Add("P_UPDATE_MANAGEMENT_RESPONSE", OracleDbType.Clob).Value = model?.UpdateManagementResponse ?? model?.UpdatedStatus ?? string.Empty;
                    cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = NormalizeActiveFlag(model?.IsActive);
                    cmd.Parameters.Add("P_USER_PPNO", OracleDbType.Int32).Value = Convert.ToInt32(loggedInUser.PPNumber);
                    cmd.Parameters.Add("P_USER_ROLE_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("P_USER_ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID.GetValueOrDefault();
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_ID", OracleDbType.Int32).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    return BuildCommercialAuditActionResult(cmd);
                    }
                }
            }

        public List<CommercialAuditPdpModel> GetCommercialAuditPdps()
            {
            var list = new List<CommercialAuditPdpModel>();

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_GET_PDPS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            list.Add(new CommercialAuditPdpModel
                                {
                                PdpId = SafeReadNullableInt(dr, "PDP_ID"),
                                AuditYearId = SafeReadNullableInt(dr, "AUDIT_YEAR_ID"),
                                AuditYearText = SafeReadString(dr, "AUDIT_YEAR_TEXT"),
                                PdpNo = SafeReadString(dr, "PDP_NO"),
                                GistOfPdp = SafeReadString(dr, "GIST_OF_PDP"),
                                BodyOfPdp = SafeReadString(dr, "BODY_OF_PDP"),
                                ManagementResponse = SafeReadString(dr, "MANAGEMENT_RESPONSE"),
                                DacRecommendations = SafeReadString(dr, "DAC_RECOMMENDATIONS"),
                                UpdateManagementResponse = SafeReadString(dr, "UPDATE_MANAGEMENT_RESPONSE"),
                                UpdatedStatus = SafeReadString(dr, "UPDATE_MANAGEMENT_RESPONSE"),
                                LinkedOmCount = SafeReadInt(dr, "LINKED_OM_COUNT"),
                                LinkedOmNumbers = SafeReadString(dr, "LINKED_OM_NUMBERS"),
                                IsActive = NormalizeActiveFlag(SafeReadString(dr, "IS_ACTIVE"))
                                });
                            }
                        }
                    }
                }

            return list;
            }

        public CommercialAuditActionResultModel SaveCommercialAuditPdpMappings(CommercialAuditPdpOmMappingSaveRequest request)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return InvalidCommercialAuditSessionResult();
                }

            var omIdsCsv = request?.OmIds == null
                ? string.Empty
                : string.Join(",", request.OmIds.Where(item => item > 0).Distinct());

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_SAVE_PDP_OM_MAP";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();

                    AddNullableIntParameter(cmd, "P_PDP_ID", request?.PdpId);
                    cmd.Parameters.Add("P_OM_IDS_CSV", OracleDbType.Clob).Value = omIdsCsv;
                    cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = NormalizeActiveFlag(request?.IsActive);
                    cmd.Parameters.Add("P_USER_PPNO", OracleDbType.Int32).Value = Convert.ToInt32(loggedInUser.PPNumber);
                    cmd.Parameters.Add("P_USER_ROLE_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("P_USER_ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID.GetValueOrDefault();
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_ID", OracleDbType.Int32).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    return BuildCommercialAuditActionResult(cmd);
                    }
                }
            }

        public List<CommercialAuditPdpOmMappingModel> GetCommercialAuditPdpMappings(int pdpId)
            {
            var list = new List<CommercialAuditPdpOmMappingModel>();

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_GET_PDP_OM_MAP";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_PDP_ID", OracleDbType.Int32).Value = pdpId;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            list.Add(new CommercialAuditPdpOmMappingModel
                                {
                                MappingId = SafeReadNullableInt(dr, "MAPPING_ID"),
                                PdpId = SafeReadNullableInt(dr, "PDP_ID"),
                                OmId = SafeReadNullableInt(dr, "OM_ID"),
                                OmNo = SafeReadString(dr, "OM_NO"),
                                GistOfOm = SafeReadString(dr, "GIST_OF_OM"),
                                IsActive = NormalizeActiveFlag(SafeReadString(dr, "IS_ACTIVE"))
                                });
                            }
                        }
                    }
                }

            return list;
            }

        public CommercialAuditActionResultModel SaveCommercialAuditArpseHeader(CommercialAuditArpseHeaderModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return InvalidCommercialAuditSessionResult();
                }

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_SAVE_ARPSE";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();

                    AddNullableIntParameter(cmd, "P_ARPSE_ID", model?.ArpseId);
                    AddNullableIntParameter(cmd, "P_ARPSE_YEAR_ID", model?.ArpseYearId);
                    cmd.Parameters.Add("P_PARA_NO", OracleDbType.Varchar2).Value = model?.ParaNo ?? string.Empty;
                    cmd.Parameters.Add("P_GIST_OF_PARA", OracleDbType.Varchar2).Value = model?.GistOfPara ?? string.Empty;
                    cmd.Parameters.Add("P_BODY_OF_PARA", OracleDbType.Clob).Value = model?.BodyOfPara ?? string.Empty;
                    cmd.Parameters.Add("P_MANAGEMENT_RESPONSE", OracleDbType.Clob).Value = model?.ManagementResponse ?? string.Empty;
                    cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = NormalizeActiveFlag(model?.IsActive);
                    cmd.Parameters.Add("P_USER_PPNO", OracleDbType.Int32).Value = Convert.ToInt32(loggedInUser.PPNumber);
                    cmd.Parameters.Add("P_USER_ROLE_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("P_USER_ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID.GetValueOrDefault();
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_ID", OracleDbType.Int32).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    return BuildCommercialAuditActionResult(cmd);
                    }
                }
            }

        public List<CommercialAuditArpseHeaderModel> GetCommercialAuditArpseHeaders()
            {
            var list = new List<CommercialAuditArpseHeaderModel>();

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_GET_ARPSE_HEADERS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            list.Add(new CommercialAuditArpseHeaderModel
                                {
                                ArpseId = SafeReadNullableInt(dr, "ARPSE_ID"),
                                ArpseYearId = SafeReadNullableInt(dr, "ARPSE_YEAR_ID"),
                                ArpseYearText = SafeReadString(dr, "ARPSE_YEAR_TEXT"),
                                ParaNo = SafeReadString(dr, "PARA_NO"),
                                GistOfPara = SafeReadString(dr, "GIST_OF_PARA"),
                                BodyOfPara = SafeReadString(dr, "BODY_OF_PARA"),
                                ManagementResponse = SafeReadString(dr, "MANAGEMENT_RESPONSE"),
                                LinkedPdpCount = SafeReadInt(dr, "LINKED_PDP_COUNT"),
                                LinkedPdpNumbers = SafeReadString(dr, "LINKED_PDP_NUMBERS"),
                                IsActive = NormalizeActiveFlag(SafeReadString(dr, "IS_ACTIVE"))
                                });
                            }
                        }
                    }
                }

            return list;
            }

        public CommercialAuditActionResultModel SaveCommercialAuditArpsePdpMappings(CommercialAuditArpsePdpMappingSaveRequest request)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return InvalidCommercialAuditSessionResult();
                }

            var pdpIdsCsv = request?.PdpIds == null
                ? string.Empty
                : string.Join(",", request.PdpIds.Where(item => item > 0).Distinct());

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_SAVE_ARPSE_PDP_MAP";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();

                    AddNullableIntParameter(cmd, "P_ARPSE_ID", request?.ArpseId);
                    cmd.Parameters.Add("P_PDP_IDS_CSV", OracleDbType.Clob).Value = pdpIdsCsv;
                    cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = NormalizeActiveFlag(request?.IsActive);
                    cmd.Parameters.Add("P_USER_PPNO", OracleDbType.Int32).Value = Convert.ToInt32(loggedInUser.PPNumber);
                    cmd.Parameters.Add("P_USER_ROLE_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("P_USER_ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID.GetValueOrDefault();
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_ID", OracleDbType.Int32).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    return BuildCommercialAuditActionResult(cmd);
                    }
                }
            }

        public List<CommercialAuditArpsePdpMappingModel> GetCommercialAuditArpsePdpMappings(int arpseId)
            {
            var list = new List<CommercialAuditArpsePdpMappingModel>();

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_GET_ARPSE_PDP_MAP";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_ARPSE_ID", OracleDbType.Int32).Value = arpseId;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            list.Add(new CommercialAuditArpsePdpMappingModel
                                {
                                MappingId = SafeReadNullableInt(dr, "MAPPING_ID"),
                                ArpseId = SafeReadNullableInt(dr, "ARPSE_ID"),
                                PdpId = SafeReadNullableInt(dr, "PDP_ID"),
                                PdpNo = SafeReadString(dr, "PDP_NO"),
                                GistOfPdp = SafeReadString(dr, "GIST_OF_PDP"),
                                IsActive = NormalizeActiveFlag(SafeReadString(dr, "IS_ACTIVE"))
                                });
                            }
                        }
                    }
                }

            return list;
            }

        public CommercialAuditActionResultModel SaveCommercialAuditArpseDacEntry(CommercialAuditArpseDacEntryModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return InvalidCommercialAuditSessionResult();
                }

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_SAVE_ARPSE_DAC";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();

                    AddNullableIntParameter(cmd, "P_DAC_ENTRY_ID", model?.DacEntryId);
                    AddNullableIntParameter(cmd, "P_ARPSE_ID", model?.ArpseId);
                    cmd.Parameters.Add("P_DAC_RECOMMENDATION", OracleDbType.Clob).Value = model?.DacRecommendation ?? string.Empty;
                    AddNullableDateParameter(cmd, "P_DAC_DATE", model?.DacDate);
                    cmd.Parameters.Add("P_UPDATED_STATUS", OracleDbType.Clob).Value = model?.UpdatedStatus ?? string.Empty;
                    cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = NormalizeActiveFlag(model?.IsActive);
                    cmd.Parameters.Add("P_USER_PPNO", OracleDbType.Int32).Value = Convert.ToInt32(loggedInUser.PPNumber);
                    cmd.Parameters.Add("P_USER_ROLE_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("P_USER_ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID.GetValueOrDefault();
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_ID", OracleDbType.Int32).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    return BuildCommercialAuditActionResult(cmd);
                    }
                }
            }

        public List<CommercialAuditArpseDacEntryModel> GetCommercialAuditArpseDacEntries(int arpseId)
            {
            var list = new List<CommercialAuditArpseDacEntryModel>();

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_GET_ARPSE_DAC_ENTRIES";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_ARPSE_ID", OracleDbType.Int32).Value = arpseId;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            list.Add(new CommercialAuditArpseDacEntryModel
                                {
                                DacEntryId = SafeReadNullableInt(dr, "DAC_ENTRY_ID"),
                                ArpseId = SafeReadNullableInt(dr, "ARPSE_ID"),
                                DacRecommendation = SafeReadString(dr, "DAC_RECOMMENDATION"),
                                DacDate = SafeReadNullableDate(dr, "DAC_DATE"),
                                UpdatedStatus = SafeReadString(dr, "UPDATED_STATUS"),
                                IsActive = NormalizeActiveFlag(SafeReadString(dr, "IS_ACTIVE"))
                                });
                            }
                        }
                    }
                }

            return list;
            }

        public CommercialAuditActionResultModel SaveCommercialAuditArpsePacEntry(CommercialAuditArpsePacEntryModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return InvalidCommercialAuditSessionResult();
                }

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_SAVE_ARPSE_PAC";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();

                    AddNullableIntParameter(cmd, "P_PAC_ENTRY_ID", model?.PacEntryId);
                    AddNullableIntParameter(cmd, "P_ARPSE_ID", model?.ArpseId);
                    cmd.Parameters.Add("P_PAC_DIRECTIVE", OracleDbType.Clob).Value = model?.PacDirective ?? string.Empty;
                    AddNullableDateParameter(cmd, "P_PAC_DATE", model?.PacDate);
                    cmd.Parameters.Add("P_UPDATED_STATUS", OracleDbType.Clob).Value = model?.UpdatedStatus ?? string.Empty;
                    cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = NormalizeActiveFlag(model?.IsActive);
                    cmd.Parameters.Add("P_USER_PPNO", OracleDbType.Int32).Value = Convert.ToInt32(loggedInUser.PPNumber);
                    cmd.Parameters.Add("P_USER_ROLE_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("P_USER_ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID.GetValueOrDefault();
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("P_ID", OracleDbType.Int32).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    return BuildCommercialAuditActionResult(cmd);
                    }
                }
            }

        public List<CommercialAuditArpsePacEntryModel> GetCommercialAuditArpsePacEntries(int arpseId)
            {
            var list = new List<CommercialAuditArpsePacEntryModel>();

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_GET_ARPSE_PAC_ENTRIES";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_ARPSE_ID", OracleDbType.Int32).Value = arpseId;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            list.Add(new CommercialAuditArpsePacEntryModel
                                {
                                PacEntryId = SafeReadNullableInt(dr, "PAC_ENTRY_ID"),
                                ArpseId = SafeReadNullableInt(dr, "ARPSE_ID"),
                                PacDirective = SafeReadString(dr, "PAC_DIRECTIVE"),
                                PacDate = SafeReadNullableDate(dr, "PAC_DATE"),
                                UpdatedStatus = SafeReadString(dr, "UPDATED_STATUS"),
                                IsActive = NormalizeActiveFlag(SafeReadString(dr, "IS_ACTIVE"))
                                });
                            }
                        }
                    }
                }

            return list;
            }

        public List<ARPSEYearDropdownVM> GetARPSEYears()
            {
            var list = new List<ARPSEYearDropdownVM>();

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_GET_ARPSE_YEARS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            var arpseYear = SafeReadInt(dr, "ARPSE_YEAR");
                            if (arpseYear <= 0)
                                {
                                continue;
                                }

                            list.Add(new ARPSEYearDropdownVM
                                {
                                ARPSEYear = arpseYear,
                                DisplayText = SafeReadString(dr, "DISPLAY_TEXT")
                                });
                            }
                        }
                    }
                }

            return list;
            }

        public List<ARPSEYearWiseReportVM> GetARPSEYearWiseReport(int arpseYear)
            {
            var list = new List<ARPSEYearWiseReportVM>();

            if (arpseYear <= 0)
                {
                return list;
                }

            using (var con = this.DatabaseConnection())
                {
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_COMMERCIAL_AUDIT.P_GET_ARPSE_YEAR_WISE_REPORT";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_ARPSE_YEAR", OracleDbType.Int32).Value = arpseYear;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            list.Add(new ARPSEYearWiseReportVM
                                {
                                SrNo = SafeReadInt(dr, "SR_NO"),
                                ParaNo = SafeReadString(dr, "PARA_NO"),
                                ContentsOfPara = CleanHtmlForReport(SafeReadString(dr, "CONTENTS_OF_PARA")),
                                ReplyOfManagement = CleanHtmlForReport(SafeReadString(dr, "REPLY_OF_MANAGEMENT")),
                                DACRecommendations = CleanHtmlForReport(SafeReadString(dr, "DAC_RECOMMENDATIONS")),
                                PACDirectives = CleanHtmlForReport(SafeReadString(dr, "PAC_DIRECTIVES")),
                                Progress = CleanHtmlForReport(SafeReadString(dr, "PROGRESS"))
                                });
                            }
                        }
                    }
                }

            return list;
            }
        }
    }
