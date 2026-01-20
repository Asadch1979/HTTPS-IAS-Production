using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {

        public string SaveCircularDocument(CircularDocumentModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_InsertCircularDoc";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_circular_id", OracleDbType.Int32).Value = model.CircularId;
                    cmd.Parameters.Add("p_file_name", OracleDbType.Varchar2).Value = model.FileName;
                    cmd.Parameters.Add("p_file_type", OracleDbType.Varchar2).Value = model.FileType;
                    cmd.Parameters.Add("p_file_size", OracleDbType.Int32).Value = model.FileSize;
                    cmd.Parameters.Add("p_file_blob", OracleDbType.Blob).Value = model.FileBlob;
                    cmd.Parameters.Add("p_uploaded_by", OracleDbType.Varchar2).Value = model.UploadedBy;
                    cmd.Parameters.Add("o_status", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    var status = cmd.Parameters["o_status"].Value?.ToString();
                    return status;
                    }
                }
            }

        public void InsertCircularDoc(
            int circularId,
            string fileName,
            string fileType,
            long fileSize,
            byte[] fileBlob,
            string uploadedBy,
            out string status)
            {
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_InsertCircularDoc";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_circular_id", OracleDbType.Int32).Value = circularId;
                    cmd.Parameters.Add("p_file_name", OracleDbType.Varchar2).Value = fileName;
                    cmd.Parameters.Add("p_file_type", OracleDbType.Varchar2).Value = fileType;
                    cmd.Parameters.Add("p_file_size", OracleDbType.Int32).Value = fileSize;
                    cmd.Parameters.Add("p_file_blob", OracleDbType.Blob).Value = fileBlob;
                    cmd.Parameters.Add("p_uploaded_by", OracleDbType.Varchar2).Value = uploadedBy;
                    cmd.Parameters.Add("o_status", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    status = cmd.Parameters["o_status"].Value?.ToString();
                    }
                }
            }

        public CircularDocumentModel GetCircularDocument(int docId)
            {
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GetCircularDoc";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_doc_id", OracleDbType.Int32).Value = docId;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var rdr = cmd.ExecuteReader())
                        {
                        if (rdr.Read())
                            {
                            return new CircularDocumentModel
                                {
                                DocId = docId,
                                CircularId = rdr["CIRCULAR_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["CIRCULAR_ID"]),
                                FileName = rdr["FILE_NAME"].ToString(),
                                FileType = rdr["FILE_TYPE"].ToString(),
                                FileSize = rdr["FILE_SIZE"] == DBNull.Value ? 0 : Convert.ToInt64(rdr["FILE_SIZE"]),
                                FileBlob = rdr["FILE_BLOB"] == DBNull.Value ? null : ((OracleBlob)rdr.GetOracleBlob(rdr.GetOrdinal("FILE_BLOB"))).Value,
                                UploadedBy = rdr["UPLOADED_BY"].ToString(),
                                UploadedOn = rdr["UPLOADED_ON"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["UPLOADED_ON"])
                                };
                            }
                        }
                    }
                }
            return null;
            }



        public List<AuditChecklistAnnexureCircularModel> GetAuditChecklistAnnexureCirculars()
            {

            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var list = new List<AuditChecklistAnnexureCircularModel>();
            var con = this.DatabaseConnection();
           
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD.P_GetAuditChecklistAnnexureCirculars";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        list.Add(new AuditChecklistAnnexureCircularModel
                            {
                            ID = rdr["ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ID"]),
                            DivisionEntId = rdr["DIVISION_ENT_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["DIVISION_ENT_ID"]),
                            ReferenceTypeId = rdr["REFERENCE_TYPE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["REFERENCE_TYPE_ID"]),
                            ReferenceType = rdr["REFERENCE_TYPE"]?.ToString(),
                            InstructionsDetails = rdr["INSTRUCTIONSDETAILS"]?.ToString(),
                            Keywords = rdr["KEYWORDS"]?.ToString(),
                            RedirectedPage = rdr["REDIRECTEDPAGE"]?.ToString(),
                            Division = rdr["DIVISION"]?.ToString(),
                            InstructionsTitle = rdr["INSTRUCTIONSTITLE"]?.ToString(),
                            InstructionsDate = rdr["INSTRUCTIONSDATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["INSTRUCTIONSDATE"]),
                            DocType = rdr["DOCTYPE"]?.ToString()
                            });
                        }
                    }

                }
            return list;
            }







        public List<ObservationResponsiblePPNOModel> GetResponsibilityForAuthorize(int C_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var list = new List<ObservationResponsiblePPNOModel>();
            using (var con = this.DatabaseConnection())
                {
               
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_ar.P_Get_responsibility_for_Authorize";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = C_ID;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (OracleDataReader rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            list.Add(new ObservationResponsiblePPNOModel
                                {
                                INDICATOR = rdr["IND"]?.ToString(),
                                OLD_PARA_ID = rdr["OLD_PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OLD_PARA_ID"]),
                                NEW_PARA_ID = rdr["NEW_PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["NEW_PARA_ID"]),
                                PP_NO = rdr["PP_NO"]?.ToString(),
                                EMP_NAME = rdr["EMP_NAME"]?.ToString(),
                                LOAN_CASE = rdr["LOAN_CASE"]?.ToString(),
                                LC_AMOUNT = rdr["LOAN_AMOUNT"]?.ToString(),
                                ACCOUNT_NUMBER = rdr["ACCOUNT_NO"]?.ToString(),
                                ACC_AMOUNT = rdr["ACCCOUNT_AMOUNT"]?.ToString(),
                                REMARKS = rdr["REASONS"]?.ToString(),
                                ACTION = rdr["ACTION"]?.ToString()
                                });
                            }
                        }
                    }
                }
            return list;
            }


        public List<AuditEmployeeModel> GetFadAuditEmployees()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
           

            var list = new List<AuditEmployeeModel>();
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD.P_GetAuditEmployees";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        var m = new AuditEmployeeModel();
                        m.PPNO = rdr["PPNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PPNO"]);
                        m.DEPARTMENTCODE = rdr["DEPARTMENTCODE"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["DEPARTMENTCODE"]);
                        m.DESIGNATIONCODE = rdr["DESIGNATIONCODE"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["DESIGNATIONCODE"]);
                        m.RANKCODE = rdr["RANKCODE"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["RANKCODE"]);
                        m.DEPTARMENT = rdr["DEPTARMENT"].ToString();
                        m.EMPLOYEEFIRSTNAME = rdr["EMPLOYEEFIRSTNAME"].ToString();
                        m.EMPLOYEELASTNAME = rdr["EMPLOYEELASTNAME"].ToString();
                        m.CURRENT_RANK = rdr["CURRENT_RANK"].ToString();
                        m.FUN_DESIGNATION = rdr["FUN_DESIGNATION"].ToString();
                        m.TYPE = rdr["TYPE"].ToString();
                        m.TASK_ALLOCATED = rdr["TASK_ALLOCATED"] == DBNull.Value ? string.Empty : rdr["TASK_ALLOCATED"].ToString();
                        list.Add(m);
                        }
                    }
                }
            con.Close();
            return list;
            }

        public string AddResponsibilityforoldparas(
        int com_id,
        ObservationResponsiblePPNOModel responsible,
        string IND_Action)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            string resp = string.Empty;

            using (var con = this.DatabaseConnection())
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_responibilityforoldpara";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                // Mandatory parameters (all actions)
                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = com_id;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = IND_Action;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("RES_PP", OracleDbType.Int32).Value = responsible.PP_NO;

                // Action-based values
                if (string.Equals(IND_Action, "D", StringComparison.OrdinalIgnoreCase))
                    {
                    // Neutral values for DELETE
                    cmd.Parameters.Add("LOANCASE", OracleDbType.Int32).Value = 0;
                    cmd.Parameters.Add("ACCNUMBER", OracleDbType.Int32).Value =0;
                    cmd.Parameters.Add("LCAMOUNT", OracleDbType.Int32).Value = 0;
                    cmd.Parameters.Add("ACAMOUNT", OracleDbType.Int32).Value = 0;
                    }
                else
                    {
                    // Real values for ADD / UPDATE
                    cmd.Parameters.Add("LOANCASE", OracleDbType.Int32).Value = responsible.LOAN_CASE;
                    cmd.Parameters.Add("ACCNUMBER", OracleDbType.Int32).Value = responsible.ACCOUNT_NUMBER;
                    cmd.Parameters.Add("LCAMOUNT", OracleDbType.Int32).Value = responsible.LC_AMOUNT;
                    cmd.Parameters.Add("ACAMOUNT", OracleDbType.Int32).Value = responsible.ACC_AMOUNT;
                    }

                // Output cursor
                cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor)
                               .Direction = ParameterDirection.Output;

                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    if (rdr.Read())
                        {
                        resp = rdr["remarks"]?.ToString();
                        }
                    }
                }

            return resp;
            }




        public List<ManageAuditParasModel> GetObservationsForManageAuditParas(int ENTITY_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();

            List<ManageAuditParasModel> list = new List<ManageAuditParasModel>();
            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    var loggedInUser = sessionHandler.GetUserOrThrow();

                    using (OracleCommand cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_ar.P_GetObservationsForManageAuditParas";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("S_ENT_ID", OracleDbType.Int32).Value = ENTITY_ID;
                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                        cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                        cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                var para = new ManageAuditParasModel
                                    {
                                    COM_ID = rdr["COM_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["COM_ID"]),
                                    PARA_NO = rdr["PARA_NO"]?.ToString(),
                                    AUDIT_PERIOD = rdr["AUDIT_PERIOD"]?.ToString(),
                                    OBS_GIST = rdr["GIST_OF_PARAS"]?.ToString(),
                                    OBS_RISK = rdr["RISK"]?.ToString(),
                                    ANNEX = rdr["ANNEX"]?.ToString()
                                    };
                                list.Add(para);

                                }
                            }
                        }
                    }
                }
            catch (Exception ex)
                {
                Console.WriteLine($"Error fetching audit paras: {ex.Message}");
                }

            return list;
            }

        public viewMemoModel GetObservationDetailsForManageAuditParas(int COM_ID)
            {
            var sessionHandler = CreateSessionHandler();
            viewMemoModel para = null;
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUserOrThrow();
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_ar.P_GetObservationsDetailsForManageAuditParas";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = COM_ID;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (OracleDataReader rdr = cmd.ExecuteReader())
                        {
                        if (rdr.Read())
                            {
                            para = new viewMemoModel
                                {
                                COM_ID = rdr["COM_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["COM_ID"]),
                                NEW_PARA_ID = rdr["NEW_PARA_ID"]?.ToString(),
                                OLD_PARA_ID = rdr["OLD_PARA_ID"]?.ToString(),
                                AUDIT_PERIOD = rdr["AUDIT_PERIOD"]?.ToString(),
                                PARA_NO = rdr["PARA_NO"]?.ToString(),
                                ANNEX = rdr["ANNEX"]?.ToString(),
                                ANNEX_ID = rdr["ANNEX_ID"]?.ToString(),
                                OBS_GIST = rdr["GIST_OF_PARAS"]?.ToString(),
                                OBS_RISK = rdr["RISK"]?.ToString(),
                                OBS_RISK_ID = rdr["RISK_ID"]?.ToString(),
                                PARA_TEXT = rdr["PARA_TEXT"]?.ToString(),
                                AMOUNT_INV = rdr["AMOUNT_INV"]?.ToString(),
                                NO_INSTANCES = rdr["NO_INSTANCES"]?.ToString(),
                                INDICATOR = rdr["INDICATOR"]?.ToString()
                                };
                            }
                        }
                    }
                }
            return para;
            }

        public List<ObservationResponsiblePPNOModel> GetResponsiblePPNOforoldPara(int C_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var list = new List<ObservationResponsiblePPNOModel>();
            using (var con = this.DatabaseConnection())
                {
               
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_ar.GetResponsiblePPNOforoldPara";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = C_ID;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (OracleDataReader rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            list.Add(new ObservationResponsiblePPNOModel
                                {
                                RESP_ROW_ID = rdr["RESP_ROW_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["RESP_ROW_ID"]),
                                PP_NO = rdr["PP_NO"]?.ToString(),
                                EMP_NAME = rdr["EMP_NAME"]?.ToString(),
                                LOAN_CASE = rdr["LOANCASE"]?.ToString(),
                                BR_CODE = rdr["BR_CODE"]?.ToString(),
                                LC_AMOUNT = rdr["LCAMOUNT"]?.ToString(),
                                ACCOUNT_NUMBER = rdr["ACCNUMBER"]?.ToString(),
                                ACC_AMOUNT = rdr["ACAMOUNT"]?.ToString()
                                });
                            }
                        }
                    }
                }
            return list;
            }



        public List<ManageAuditParasModel> GetObservationsForMangeAuditParasForAuthorization()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ManageAuditParasModel> list = new List<ManageAuditParasModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GET_Para_details_for_Authorize";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageAuditParasModel chk = new ManageAuditParasModel();
                    chk.COM_ID = rdr["com_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["COM_ID"]);
                    chk.NEW_PARA_ID = rdr["new_para_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["new_para_id"]);                    ;
                    chk.OLD_PARA_ID = rdr["old_para_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["old_para_id"]);
                    chk.AUDITEE = rdr["AUDITEE"].ToString();
                    chk.OBS_RISK = rdr["risk"].ToString();
                    chk.OBS_RISK_ID = rdr["risk_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["risk_id"]);
                    chk.OBS_GIST = rdr["gist_of_paras"].ToString();
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.PARA_NO = rdr["para_no"].ToString();
                    chk.INDICATOR = rdr["ind"].ToString();
                    chk.ANNEX = rdr["annex"].ToString();
                    chk.ANNEX_ID = rdr["annex_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["annex_id"]);
                    chk.NO_INSTANCES = rdr["no_instances"].ToString();
                    chk.AMOUNT_INV = rdr["Amount"].ToString();
                    chk.UPDATED_ON = rdr["UPDATED_ON"].ToString();
                    chk.UPDATED_BY = rdr["UPDATED_BY"].ToString();
                    chk.P_TYPE_IND = rdr["P_TYPE_IND"].ToString();
                    chk.PARA_TEXT = rdr["PARA_TEXT"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }
        public List<ManageAuditParasModel> GetProposedChangesInManageParasAuth(int COM_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ManageAuditParasModel> list = new List<ManageAuditParasModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GET_Para_changes_for_Authorize";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = COM_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageAuditParasModel chk = new ManageAuditParasModel();
                    chk.COM_ID = rdr["com_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["COM_ID"]);
                    chk.NEW_PARA_ID = rdr["NEW_PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["NEW_PARA_ID"]);
                    chk.OLD_PARA_ID = rdr["OLD_PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OLD_PARA_ID"]);
                    chk.OBS_RISK = rdr["risk"].ToString();
                    chk.OBS_RISK_ID = rdr["risk_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["risk_id"]);
                    chk.OBS_GIST = rdr["gist_of_para"].ToString();
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.PARA_NO = rdr["para_no"].ToString();
                    chk.INDICATOR = rdr["ind"].ToString();
                    chk.ANNEX_ID = rdr["annex_id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["annex_id"]);
                    chk.NO_INSTANCES = rdr["no_instances"].ToString();
                    chk.AMOUNT_INV = rdr["Amount"].ToString();
                    //chk.UPDATED_ON = rdr["UPDATED_ON"].ToString();
                    //chk.UPDATED_BY = rdr["UPDATED_BY"].ToString();
                    chk.P_TYPE_IND = rdr["P_TYPE_IND"].ToString();
                    chk.PARA_TEXT = rdr["PARA_TEXT"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }
        public List<IdNameModel> GetRelationTypes()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
           

            var list = new List<IdNameModel>();
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD.P_GetRelationTypes";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        list.Add(new IdNameModel
                            {
                            Id = rdr["ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ID"]),
                            Name = rdr["NAME"].ToString()
                            });
                        }
                    }
                }
            con.Close();
            return list;
            }

        public List<IdNameModel> GetReportingOffices(int relationTypeId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
           

            var list = new List<IdNameModel>();
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD.P_GetReportingOffices";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_relation_id", OracleDbType.Int32).Value = relationTypeId;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        list.Add(new IdNameModel
                            {
                            Id = rdr["ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ID"]),
                            Name = rdr["NAME"].ToString()
                            });
                        }
                    }
                }
            con.Close();
            return list;
            }

        public List<EntityModel> Get_Entities_For_Office(int reportingOfficeId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
           

            var list = new List<EntityModel>();
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD.P_GetEntitiesForOffice";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_office_id", OracleDbType.Int32).Value = reportingOfficeId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        list.Add(new EntityModel
                            {
                            EntityId = rdr["ENTITY_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ENTITY_ID"]),
                            EntityCode = rdr["ENTITY_CODE"]?.ToString(),
                            Name = rdr["NAME"].ToString(),
                            Type = rdr["TYPE"]?.ToString(),
                            Allocatedto = rdr["ALLOCATEDTO"]?.ToString(),
                            TotalParas = rdr["TOTAL_PARAS"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TOTAL_PARAS"])
                            });
                        }
                    }
                }
            con.Close();
            return list;
            }

        public string AllocateEntityToAuditor(int azId, int entId, int auditorPpno)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            string result = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_allocate_entity_to_auditor";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_az_id", OracleDbType.Int32).Value = azId;
                cmd.Parameters.Add("p_ent_id", OracleDbType.Int32).Value = entId;
                cmd.Parameters.Add("p_auditor_ppno", OracleDbType.Int32).Value = auditorPpno;
                cmd.Parameters.Add("p_assigned_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    if (rdr.Read())
                        {
                        result = rdr["remarks"].ToString();
                        }
                    }
                }
            con.Dispose();
            return result;
            }



        public List<ObservationReferenceModel> GetObservationsForReferenceUpdate(int? entId, int? assignedAuditorId, int? referenceId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
           

            var list = new List<ObservationReferenceModel>();
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD.P_GetObservationsForReferenceUpdate";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_ent_id", OracleDbType.Int32).Value = entId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_auditor", OracleDbType.Int32).Value = assignedAuditorId ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_ref_id", OracleDbType.Int32).Value = referenceId ?? (object)DBNull.Value;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        list.Add(new ObservationReferenceModel
                            {
                            ComId = rdr["COM_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["COM_ID"]),
                            EntId = rdr["ENT_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ENT_ID"]),
                            ParaTitle = rdr["PARA_TITLE"].ToString(),
                            ReferenceType = rdr["REFERENCE_TYPE"].ToString(),
                            AssignedAuditorId = rdr["ASSIGNED_AUDITOR"] == DBNull.Value ? null : (int?)Convert.ToInt32(rdr["ASSIGNED_AUDITOR"]),
                            Status = rdr["STATUS"].ToString()
                            });
                        }
                    }
                }
            con.Close();
            return list;
            }

        public string UpdateParaReference(int comId, int? linkId, int newRef)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            // Call the unified procedure with UPDATE action
            var result = ManageReference(
                "UPDATE",
                new ParaReferenceLinkModel { LinkId = linkId },
                comId,
                null,
                newRef,
                loggedInUser.PPNumber);
            return result.remarks;
            }

        public List<UpdateLogModel> GetUpdateLog(int comId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
           

            var list = new List<UpdateLogModel>();
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD.P_GetReferenceUpdateLog";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_com_id", OracleDbType.Int32).Value = comId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        list.Add(new UpdateLogModel
                            {
                            Date = rdr["ACTION_DATE"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["ACTION_DATE"]),
                            User = rdr["ACTION_USER"].ToString(),
                            Field = rdr["ACTION_FIELD"].ToString(),
                            OldValue = rdr["OLD_VALUE"].ToString(),
                            NewValue = rdr["NEW_VALUE"].ToString(),
                            ActionType = rdr["ACTION_TYPE"].ToString()
                            });
                        }
                    }
                }
            con.Close();
            return list;
            }

        public List<ReferenceSearchResultModel> SearchReferences(string referenceType, string keyword)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
           

            var list = new List<ReferenceSearchResultModel>();
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD.P_SearchReferences";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_ref_type", OracleDbType.Varchar2).Value = referenceType ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_keyword", OracleDbType.Varchar2).Value = keyword ?? (object)DBNull.Value;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        list.Add(new ReferenceSearchResultModel
                            {
                            ID = rdr["ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ID"]),
                            Title = rdr["TITLE"] == DBNull.Value ? "" : rdr["TITLE"].ToString(),
                            InstructionsDate = rdr["INSTRUCTIONSDATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["INSTRUCTIONSDATE"]),
                            ReferenceType = rdr["REFERENCE_TYPE"] == DBNull.Value ? "" : rdr["REFERENCE_TYPE"].ToString(),
                            INSTRUCTIONSDETAILS = rdr["INSTRUCTIONSDETAILS"] == DBNull.Value ? "" : rdr["INSTRUCTIONSDETAILS"].ToString(),
                            KEYWORDS = rdr["KEYWORDS"] == DBNull.Value ? "" : rdr["KEYWORDS"].ToString(),
                            REFERENCEURL = rdr["REFERENCEURL"] == DBNull.Value ? "" : rdr["REFERENCEURL"].ToString()
                            });
                        }
                    }
                }
            con.Close();
            return list;
            }


        public List<PendingParaModel> GetPendingParas(int entityId, int auditYear)
            {
            var list = new List<PendingParaModel>();
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GetPendingParas";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_entity_id", OracleDbType.Int32).Value = entityId;
                    cmd.Parameters.Add("p_audit_year", OracleDbType.Int32).Value = auditYear;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (var rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            list.Add(new PendingParaModel
                                {
                                ParaId = Convert.ToInt32(rdr["PARA_ID"]),
                                AuditYear = rdr["AUDIT_YEAR"].ToString(),
                                ParaNo = rdr["PARA_NO"].ToString(),
                                Gist = rdr["GIST"].ToString(),
                                Risk = rdr["RISK"].ToString()
                                });
                            }
                        }
                    }
                }
            return list;
            }

        public List<EntityTaskSummaryModel> GetEntityTaskSummary()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var list = new List<EntityTaskSummaryModel>();
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GetEntityTaskSummary";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_auditor_ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (var rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            list.Add(new EntityTaskSummaryModel
                                {
                                EntityId = Convert.ToInt32(rdr["ENTITY_ID"]),
                                EntityCode = rdr["ENTITY_CODE"].ToString(),
                                EntityName = rdr["ENTITY_NAME"].ToString(),
                                AuditYear = rdr["AUDIT_YEAR"].ToString(),
                                TotalParas = Convert.ToInt32(rdr["TOTAL_PARAS"]),
                                ParasUpdated = Convert.ToInt32(rdr["PARAS_UPDATED"])
                                });
                            }
                        }
                    }
                }
            return list;
            }

        public List<ReferenceEntitySummaryModel> GetReferenceEntitySummary()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var list = new List<ReferenceEntitySummaryModel>();
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GetEntityTaskSummary";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_auditor_ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (var rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            list.Add(new ReferenceEntitySummaryModel
                                {
                                EntityId = Convert.ToInt32(rdr["ENTITY_ID"]),
                                EntityCode = rdr["ENTITY_CODE"].ToString(),
                                EntityName = rdr["ENTITY_NAME"].ToString(),
                                AuditPeriod = rdr["AUDIT_PERIOD"].ToString(),
                                TotalParas = Convert.ToInt32(rdr["TOTAL_PARAS"]),
                                UpdatedParas = Convert.ToInt32(rdr["UPDATED_PARAS"]),
                                Pendency = Convert.ToInt32(rdr["PENDENCY"])
                                });
                            }
                        }
                    }
                }
            return list;
            }

        public List<PendingReferenceParaModel> GetPendingReferenceParas()
            {
            var list = new List<PendingReferenceParaModel>();
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GetPendingReferenceParas";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (var rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            list.Add(new PendingReferenceParaModel
                                {
                                ComId = rdr["COM_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["COM_ID"]),
                                AuditPeriod = rdr["AUDIT_PERIOD"]?.ToString(),
                                ParaNo = rdr["PARA_NO"]?.ToString(),
                                GistOfParas = rdr["GIST_OF_PARAS"]?.ToString()
                                });
                            }
                        }
                    }
                }
            return list;
            }

        public ParaReferenceDataModel GetParaReferenceData(int comId)
            {
            var model = new ParaReferenceDataModel
                {
                References = new List<int>(),
                ReferenceDetails = new List<AuditChecklistAnnexureCircularModel>(),
                ReferenceLinks = new List<ParaReferenceLinkModel>()
                };

            model.ParaText = GetParaText(comId);

            var links = GetParaReferenceLinks(comId);
            model.ReferenceLinks = links;
            model.References = links.Select(l => l.ReferenceId).ToList();

            var allRefs = GetAuditChecklistAnnexureCirculars();
            if (model.References != null && model.References.Count > 0)
                {
                model.ReferenceDetails = allRefs.Where(r => model.References.Contains(r.ID)).ToList();
                foreach (var det in model.ReferenceDetails)
                    {
                    var lnk = links.FirstOrDefault(l => l.ReferenceId == det.ID);
                    if (lnk != null)
                        det.LinkId = lnk.LinkId;
                    }
                }

            return model;
            }

        public List<ParaReferenceLinkModel> GetParaReferenceLinks(int comId)
            {
            var list = new List<ParaReferenceLinkModel>();
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GetParaReferences";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_com_id", OracleDbType.Int32).Value = comId;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (var rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            list.Add(new ParaReferenceLinkModel
                                {
                                LinkId = rdr["LINK_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["LINK_ID"]),
                                EntityId = rdr["ENTITY_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ENTITY_ID"]),
                                OldParaId = rdr["OLD_PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OLD_PARA_ID"]),
                                NewParaId = rdr["NEW_PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["NEW_PARA_ID"]),
                                ParaId = rdr["PARA_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PARA_ID"]),
                                ReferenceId = rdr["REFERENCE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["REFERENCE_ID"]),
                                ReferenceTitle = rdr["REFERENCE_TITLE"].ToString(),
                                CreditManualId = rdr["CREDIT_MANUAL_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["CREDIT_MANUAL_ID"]),
                                OpManualId = rdr["OP_MANUAL_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["OP_MANUAL_ID"]),
                                ManualType = rdr["MANUAL_TYPE"].ToString(),
                                Chapter = rdr["CHAPTER"].ToString(),
                                InstructionsDate = rdr["INSTRUCTIONSDATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["INSTRUCTIONSDATE"]),
                                MatchedText = rdr["MATCHED_TEXT"].ToString(),
                                LinkType = rdr["LINK_TYPE"].ToString()
                                });
                            }
                        }
                    }
                }
            return list;
            }

        public List<int> GetParaReferences(int comId)
            {
            var list = new List<int>();
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GetParaReferences";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_com_id", OracleDbType.Int32).Value = comId;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (var rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            list.Add(rdr["REFERENCE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["REFERENCE_ID"]));
                            }
                        }
                    }
                }
            return list;
            }

        public string GetParaText(int comId)
            {
            string text = string.Empty;
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GetParaText";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_com_id", OracleDbType.Int32).Value = comId;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    using (var rdr = cmd.ExecuteReader())
                        {
                        if (rdr.Read())
                            text = rdr["PARA_TEXT"]?.ToString();
                        }
                    }
                }
            return text;
            }

        public AuditChecklistAnnexureCircularModel GetReferenceDetail(int refId)
            {
            return GetAuditChecklistAnnexureCirculars().FirstOrDefault(r => r.ID == refId);
            }

        public List<AuditChecklistAnnexureCircularModel> GetReferenceDetails(List<int> ids)
            {
            var all = GetAuditChecklistAnnexureCirculars();
            return all.Where(r => ids.Contains(r.ID)).ToList();
            }

        /// <summary>
        /// Centralized method to execute <c>PKG_FAD.P_ManageReference</c>. It
        /// accepts the action to perform and relevant parameters. Unused
        /// parameters for a given action may be <c>null</c>.
        /// </summary>
        private (string remarks, string action, int? paraId) ManageReference(
            string action,
            ParaReferenceLinkModel link,
            int? paraId,
            int? refId,
            int? newRef,
            string ppno)
            {
            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_ManageReference";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_action", OracleDbType.Varchar2).Value = action;
                    cmd.Parameters.Add("p_link_id", OracleDbType.Int32).Value = link?.LinkId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_entity_id", OracleDbType.Int32).Value = link?.EntityId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_old_para_id", OracleDbType.Int32).Value = link?.OldParaId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_new_para_id", OracleDbType.Int32).Value = link?.NewParaId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_para_id", OracleDbType.Int32).Value = paraId ?? link?.ParaId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_ref_id", OracleDbType.Int32).Value = refId ?? link?.ReferenceId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_ref_title", OracleDbType.Varchar2).Value = link?.ReferenceTitle ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_credit_manual_id", OracleDbType.Int32).Value = link?.CreditManualId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_op_manual_id", OracleDbType.Int32).Value = link?.OpManualId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_manual_type", OracleDbType.Varchar2).Value = link?.ManualType ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_chapter", OracleDbType.Varchar2).Value = link?.Chapter ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_instructions_date", OracleDbType.Date).Value = link?.InstructionsDate ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_matched_text", OracleDbType.Varchar2).Value = link?.MatchedText ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_link_type", OracleDbType.Varchar2).Value = link?.LinkType ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_new_ref", OracleDbType.Int32).Value = newRef ?? (object)DBNull.Value;
                    cmd.Parameters.Add("p_user", OracleDbType.Varchar2).Value = ppno;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        if (reader.Read())
                            {
                            var remarks = reader["remarks"]?.ToString();
                            var returnedAction = reader["action"]?.ToString();
                            int? returnedParaId = null;
                            if (reader["para_id"] != DBNull.Value)
                                returnedParaId = Convert.ToInt32(reader["para_id"]);

                            return (remarks, returnedAction, returnedParaId);
                            }
                        }

                    return (null, null, null);
                    }
                }
            }

        /// <summary>
        /// Wrapper for managing para references. This now calls the unified
        /// <c>PKG_FAD.P_ManageReference</c> procedure with the <c>ADD</c> action
        /// instead of the legacy <c>P_AddReference</c> procedure.
        /// </summary>
        private string AddReference(ParaReferenceLinkModel link, string ppno)
            {
            var resp = ManageReference(
                "ADD",
                link,
                link?.ParaId,
                link?.ReferenceId,
                null,
                ppno);
            return resp.remarks;
            }

        /// <summary>
        /// Removes a para reference using <c>P_ManageReference</c> with the
        /// <c>DELETE</c> action.
        /// </summary>
        private string DeleteReference(int comId, int? linkId, int refId, string ppno)
            {
            var resp = ManageReference(
                "DELETE",
                new ParaReferenceLinkModel { LinkId = linkId },
                comId,
                refId,
                null,
                ppno);
            return resp.remarks;
            }

        // reference_reviewed flag is now updated inside P_ManageReference so
        // the standalone MarkParaAsReviewed method is no longer required.

        public string SaveParaReferences(int comId, List<ParaReferenceLinkModel> references)
            {
            var sessionHandler = CreateSessionHandler();
            var user = sessionHandler.GetUserOrThrow();

            var existing = GetParaReferenceLinks(comId);
            string result = string.Empty;

            foreach (var oldRef in existing)
                {
                if (!references.Any(r => r.LinkId == oldRef.LinkId))
                    {
                    result = DeleteReference(comId, oldRef.LinkId, oldRef.ReferenceId, user.PPNumber);
                    }
                }

            foreach (var r in references)
                {
                var match = existing.FirstOrDefault(x => x.LinkId == r.LinkId);
                r.ParaId = comId;

                if (match == null)
                    {
                    result = AddReference(r, user.PPNumber);
                    }
                else if (match.ReferenceId != r.ReferenceId && r.LinkId.HasValue)
                    {
                    result = UpdateParaReference(comId, r.LinkId.Value, r.ReferenceId);
                    }
                }

            return string.IsNullOrEmpty(result) ? "Saved" : result;
            }

        public List<ParaModel> GETPARASTATUSCHANGEREQUEST(int entityId, int status)
            {
            var result = new List<ParaModel>();
            using (var con = DatabaseConnection())
                {
               
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GET_PARA_STATUS_REQUEST";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = entityId;
                    cmd.Parameters.Add("P_STATUS", OracleDbType.Int32).Value = status;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            result.Add(new ParaModel
                                {
                                ComId = Convert.ToInt32(reader["COM_ID"]),
                                AuditYear = reader["AUDIT_PERIOD"].ToString(),
                                ParaNo = reader["PARA_NO"].ToString(),
                                Annexure = reader["ANNEX"].ToString(),
                                Title = reader["GIST_OF_PARAS"].ToString(),
                                Risk = reader["RISK"].ToString(),
                                Status = Convert.ToInt32(reader["PARA_STATUS"])
                                });
                            }
                        }
                    }
                }
            return result;
            }

        public string ADDPARASTATUSCHANGEREQUEST(int comId, int newStatus, string makerRemarks, int userId)
            {
            using (var con = DatabaseConnection())
                {
               
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_ADD_PARA_STATUS_CHANGE";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_COM_ID", OracleDbType.Int32).Value = comId;
                    cmd.Parameters.Add("P_NEW_STATUS", OracleDbType.Int32).Value = newStatus;
                    cmd.Parameters.Add("P_MAKER_REMARKS", OracleDbType.Varchar2).Value = makerRemarks;
                    cmd.Parameters.Add("P_USER_ID", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        if (reader.Read())
                            {
                            return reader["RESULT_MSG"].ToString();
                            }
                        }
                    }
                }
            return "Error: No response";
            }

        public List<ParaStatusChangeLogModel> GETPARASTATUSAUTHORIZATION()
            {
            var result = new List<ParaStatusChangeLogModel>();
            using (var con = DatabaseConnection())
                {
               
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_GET_PARA_STATUS_AUTHORIZATION";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            result.Add(new ParaStatusChangeLogModel
                                {
                                LogId = Convert.ToInt32(reader["LOG_ID"]),
                                ComId = Convert.ToInt32(reader["COM_ID"]),
                                OldStatus = Convert.ToInt32(reader["OLD_STATUS"]),
                                NewStatus = Convert.ToInt32(reader["NEW_STATUS"]),
                                MakerRemarks = reader["MAKER_REMARKS"].ToString(),
                                AuthorizerRemarks = reader["AUTHORIZER_REMARKS"].ToString(),
                                ChangedBy = Convert.ToInt32(reader["CHANGED_BY"]),
                                ChangedOn = Convert.ToDateTime(reader["CHANGED_ON"]),
                                ActionStatus = reader["ACTION_STATUS"].ToString(),
                                AuditYear = reader["AUDIT_PERIOD"].ToString(),
                                ParaNo = reader["PARA_NO"].ToString(),
                                Annexure = reader["ANNEX"].ToString(),
                                Title = reader["GIST_OF_PARAS"].ToString(),
                                Risk = reader["RISK"].ToString()
                                });
                            }
                        }
                    }
                }
            return result;
            }

        public string GetIASParaText(int COM_ID)
            {
            string resp = "";
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_FAD.P_GET_IAS_PARA_TEXT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CM_ID", OracleDbType.Int32).Value = COM_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["text"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }


        public string AUTHORIZEPARASTATUSCHANGEREQUEST(int logId, string action, string authRemarks, int userId)
            {
            using (var con = DatabaseConnection())
                {
               
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_FAD.P_AUTHORIZE_PARA_STATUS_CHANGE";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_LOG_ID", OracleDbType.Int32).Value = logId;
                    cmd.Parameters.Add("P_ACTION", OracleDbType.Varchar2).Value = action;
                    cmd.Parameters.Add("P_AUTH_BY", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("P_AUTH_REMARKS", OracleDbType.Varchar2).Value = authRemarks;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        if (reader.Read())
                            {
                            return reader["RESULT_MSG"].ToString();
                            }
                        }
                    }
                }
            return "Error: No response";
            }
        public List<RoleRespModel> GetRoleResponsibleForChecklistDetail()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<RoleRespModel> groupList = new List<RoleRespModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_get_role_responsible";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    RoleRespModel grp = new RoleRespModel();
                    grp.DESIGNATIONCODE = Convert.ToInt32(rdr["DESIGNATIONCODE"]);
                    grp.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    groupList.Add(grp);
                    }
                }
            con.Dispose();
            return groupList;
            }

        public List<AuditeeEntitiesModel> GetProcOwnerForChecklistDetail()
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_get_process_owner";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    if (rdr["ENTITY_ID"].ToString() != "" && rdr["ENTITY_ID"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);

                    if (rdr["name"].ToString() != "" && rdr["name"].ToString() != null)
                        entity.NAME = rdr["name"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;
            }

        public List<ControlViolationsModel> GetViolationsForChecklistDetail()
            {
            var con = this.DatabaseConnection();
            List<ControlViolationsModel> controlViolationList = new List<ControlViolationsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_get_violations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ControlViolationsModel v = new ControlViolationsModel();
                    v.ID = Convert.ToInt32(rdr["S_GR_ID"]);
                    v.V_NAME = rdr["DESCRIPTION"].ToString();
                    if (rdr["MAX_NUMBER"].ToString() != null && rdr["MAX_NUMBER"].ToString() != "")
                        v.MAX_NUMBER = Convert.ToInt32(rdr["MAX_NUMBER"]);
                    v.STATUS = "Y";
                    controlViolationList.Add(v);
                    }
                }
            con.Dispose();
            return controlViolationList;
            }

        public List<OldParasModel> GetCurrentParasForStatusChangeRequestAuthorize()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_GetnewParasForResponseAuthorize";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityId", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.ENTITY_CODE = rdr["ENTITY_CODE"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.PARA_RISK = rdr["PARA_RISK"].ToString();
                    chk.GIST_OF_PARAS = rdr["gist_of_para"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.MAKER_REMARKS = rdr["remarks"].ToString();
                    chk.REVIEWER_REMARKS = rdr["reviewer_comments"].ToString();
                    chk.PARA_STATUS = rdr["PARA_STATUS"].ToString() == "6" ? "Settled" : "Un-settled";
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeOldParasModel> GetLegacyParasEntitiesFAD()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_FAD.P_GetEntitiesForLegacyPara";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PP_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeOldParasModel chk = new AuditeeOldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    chk.ENTITY_NAME = rdr["NAME"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeOldParasModel> GetSettledParasEntitiesForMonitoringFAD()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_FAD.P_GET_SETTLED_PARA_ENTITIES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeOldParasModel chk = new AuditeeOldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    chk.ENTITY_NAME = rdr["NAME"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<OldParasModel> GetLegacyParasForUpdateFAD(int ENTITY_ID, string PARA_REF = "", int PARA_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())

                {
                cmd.CommandText = "pkg_fad.P_GetLeagacyObservations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("entityId", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("paraRef", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.RISK_ID = rdr["RISK"].ToString();
                    chk.ENTITY_CODE = rdr["ENTITY_CODE"].ToString();
                    chk.TYPE_ID = rdr["TYPE_ID"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    if (PARA_REF != null)
                        {
                        chk.PROCESS = Convert.ToInt32(rdr["PROCESS"].ToString());
                        chk.SUB_PROCESS = Convert.ToInt32(rdr["SUB_PROCESS"].ToString());
                        chk.PROCESS_DETAIL = Convert.ToInt32(rdr["PROCESS_DETAIL"].ToString());
                        chk.PARA_TEXT = rdr["PARA_TEXT"].ToString();


                        }

                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.ANNEXURE = rdr["ANNEXURE"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.VOL_I_II = rdr["VOL_I_II"].ToString();

                    if (PARA_REF != null)
                        chk.PARA_RESP = this.GetLegacyParaResponsiblePersonsFAD(PARA_REF);
                    list.Add(chk);
                    }



                }
            con.Dispose();
            return list;
            }

        public string AddResponsibilityToLegacyParasFAD(ObservationResponsiblePPNOModel RESP_PP, string REF_P, int P_ID)
            {
            string responseRes = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_add_para_responsibility";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("refid", OracleDbType.Int32).Value = P_ID;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = RESP_PP.PP_NO;
                cmd.Parameters.Add("AZ_Entity_id", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("user_ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("lC_no", OracleDbType.Varchar2).Value = RESP_PP.LOAN_CASE;
                cmd.Parameters.Add("LC_AMOUNT", OracleDbType.Varchar2).Value = RESP_PP.LC_AMOUNT;
                cmd.Parameters.Add("AC_NO", OracleDbType.Varchar2).Value = RESP_PP.ACCOUNT_NUMBER;
                cmd.Parameters.Add("AC_AMOUNT", OracleDbType.Varchar2).Value = RESP_PP.ACC_AMOUNT;
                cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = REF_P;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr2 = cmd.ExecuteReader();
                while (rdr2.Read())
                    {
                    responseRes = rdr2["REMARKS"].ToString();

                    }

                }
            con.Dispose();
            return responseRes;
            }

        public string UpdateLegacyParasWithResponsibilityNoChanges(AddLegacyParaModel LEGACY_PARA)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_reviewed_legacy_Para";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ref_id", OracleDbType.Varchar2).Value = LEGACY_PARA.REF_P;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["REF"].ToString() != "" && rdr["REF"].ToString() != null && rdr["REF"].ToString() == "2")
                        {
                        resp = rdr["REMARKS"].ToString();
                        return resp;
                        }
                    else
                        {
                        resp = rdr["REMARKS"].ToString();
                        }

                    }
                }
            con.Dispose();
            return resp;
            }

        public string UpdateLegacyParasWithResponsibilityFAD(AddLegacyParaModel LEGACY_PARA)
            {
            string resp = "";
            string responseRes = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_update_legacy_Para_text";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ref_id", OracleDbType.Varchar2).Value = LEGACY_PARA.REF_P;
                cmd.Parameters.Add("obtext", OracleDbType.Clob).Value = LEGACY_PARA.PARA_TEXT;
                cmd.Parameters.Add("process_id", OracleDbType.Int32).Value = LEGACY_PARA.PROCESS_ID;
                cmd.Parameters.Add("subprocessid", OracleDbType.Int32).Value = LEGACY_PARA.SUB_PROCESS_ID;
                cmd.Parameters.Add("checklistid", OracleDbType.Int32).Value = LEGACY_PARA.CHECKLIST_DETAIL_ID;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("risk_id", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["REF"].ToString() != "" && rdr["REF"].ToString() != null && rdr["REF"].ToString() == "2")
                        {
                        resp = rdr["REMARKS"].ToString();
                        return resp;
                        }
                    else
                        {
                        resp = rdr["REMARKS"].ToString();
                        }

                    }
                if (LEGACY_PARA.RESP_PP != null)
                    {
                    if (LEGACY_PARA.RESP_PP.Count > 0)
                        {
                        foreach (ObservationResponsiblePPNOModel respRow in LEGACY_PARA.RESP_PP)
                            {
                            responseRes = "";
                            cmd.CommandText = "pkg_fad.p_add_para_responsibility";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("refid", OracleDbType.Int32).Value = LEGACY_PARA.ID;
                            cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = respRow.PP_NO;
                            cmd.Parameters.Add("AZ_Entity_id", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                            cmd.Parameters.Add("user_ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                            cmd.Parameters.Add("lC_no", OracleDbType.Varchar2).Value = respRow.LOAN_CASE;
                            cmd.Parameters.Add("LC_AMOUNT", OracleDbType.Varchar2).Value = respRow.LC_AMOUNT;
                            cmd.Parameters.Add("AC_NO", OracleDbType.Varchar2).Value = respRow.ACCOUNT_NUMBER;
                            cmd.Parameters.Add("AC_AMOUNT", OracleDbType.Varchar2).Value = respRow.ACC_AMOUNT;
                            cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = LEGACY_PARA.REF_P;
                            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                            OracleDataReader rdr2 = cmd.ExecuteReader();
                            while (rdr2.Read())
                                {
                                responseRes = rdr2["REMARKS"].ToString();

                                }
                            }
                        }

                    }
                }
            con.Dispose();
            return resp + "<br/>" + responseRes;
            }

        public string AuthorizerAddChangeStatusRequestForSettledPara(string REFID, string IND, int NEW_STATUS, string REMARKS, string Action_IND)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_AuthorizeChangeStatusRequestForSettledPara_new";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obs_id", OracleDbType.Varchar2).Value = REFID;
                cmd.Parameters.Add("P_IND", OracleDbType.Varchar2).Value = IND;
                cmd.Parameters.Add("remark", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("indicator", OracleDbType.Varchar2).Value = Action_IND;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remarks"].ToString();
                    }

                }
            con.Dispose();
            return resp;
            }

        public string AddAuthorizeChangeStatusRequestForSettledPara(string REFID, string OBS_ID, string IND, string Action_IND)

            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            string resp = "";
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_authorizechangestatusrequestforsettledpara";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = REFID;
                cmd.Parameters.Add("au_obs_id", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("P_IND", OracleDbType.Varchar2).Value = IND;
                cmd.Parameters.Add("ind", OracleDbType.Varchar2).Value = Action_IND;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remark"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<OldParasAuthorizeModel> GetOldSettledParasForResponseAuthorize()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasAuthorizeModel> list = new List<OldParasAuthorizeModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_getoldparasforresponseauthorize";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasAuthorizeModel chk = new OldParasAuthorizeModel();
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.AU_OBS_ID = rdr["AU_OBS_ID"].ToString();
                    chk.IND = rdr["IND"].ToString();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.ANNEXURE = rdr["ANNEXURE"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.VOL_I_II = rdr["VOL_I_II"].ToString();
                    chk.PARA_STATUS = rdr["PARA_STATUS"].ToString();
                    chk.PARA_CHANGE_REQUEST_STATUS = rdr["TEMP_STATUS_FOR_CHANGE"].ToString();

                    chk.REMARKS = rdr["REMARKS"].ToString();


                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<ObservationResponsiblePPNOModel> GetLegacyParaResponsiblePersonsFAD(string PARA_REF)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ObservationResponsiblePPNOModel> list = new List<ObservationResponsiblePPNOModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_get_legacy_para_responsibles";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("paraRef", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationResponsiblePPNOModel rp = new ObservationResponsiblePPNOModel();

                    rp.LOAN_CASE = rdr["LOAN_CASE"].ToString();
                    rp.EMP_NAME = rdr["EMP_NAME"].ToString();
                    rp.LC_AMOUNT = rdr["LC_AMOUNT"].ToString();
                    rp.ACCOUNT_NUMBER = rdr["ACCOUNT_NUMBER"].ToString();
                    rp.ACC_AMOUNT = rdr["AC_AMOUNT"].ToString();
                    rp.PP_NO = rdr["PP_NO"].ToString();
                    list.Add(rp);
                    }
                }
            con.Dispose();
            return list;
            }

        public string DeleteLegacyParaResponsibility(string PARA_REF, int PARA_ID, int PP_NO)
            {
            string resp = "Failed to delete responsibility, Please try again";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_delete_para_responsibility";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("refid", OracleDbType.Int32).Value = PARA_ID;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = PP_NO;
                // cmd.Parameters.Add("USER_PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["REMARKS"].ToString() != null && rdr["REMARKS"].ToString() != "")
                        resp = rdr["REMARKS"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string ReferBackLegacyPara(string PARA_REF, int PARA_ID)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditNatureModel> entitiesList = new List<AuditNatureModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_referback_legacy_para";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ref_id", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();

                    }
                }
            con.Dispose();
            return resp;

            }

        public List<ZoneModel> GetZonesForAnnexureAssignment()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<ZoneModel> zoneList = new List<ZoneModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_Get_Auditee_Parent_FAD";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ZoneModel z = new ZoneModel();
                    z.ZONEID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    z.ZONENAME = rdr["NAME"].ToString();
                    zoneList.Add(z);
                    }
                }
            con.Dispose();
            return zoneList;
            }

        public List<BranchModel> GetZoneBranchesForAnnexureAssignment(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<BranchModel> branchList = new List<BranchModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_Get_Auditee_Child_FAD";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BranchModel br = new BranchModel();
                    br.BRANCHID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    br.BRANCHNAME = rdr["NAME"].ToString();
                    branchList.Add(br);
                    }
                }
            con.Dispose();
            return branchList;
            }

        public List<AllParaForAnnexureAssignmentModel> GetAllParasForAnnexureAssignment(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AllParaForAnnexureAssignmentModel> list = new List<AllParaForAnnexureAssignmentModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_Get_all_paras_fad";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("EntityID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AllParaForAnnexureAssignmentModel chk = new AllParaForAnnexureAssignmentModel();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.OBS_ID = rdr["OBS_ID"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.PARA_CATEGORY = rdr["PARA_CATEGORY"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.ENTITY_NAME = rdr["NAME"].ToString();
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.ANNEX_CODE = rdr["ANNEX_ID"].ToString();
                    chk.ANNEX_ID = rdr["ID"].ToString();
                    chk.ANNEXURE = rdr["NAME"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AssignAnnexureWithPara(string OBS_ID, string REF_P, string ANNEX_ID, string PARA_CATEGORY)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AllParaForAnnexureAssignmentModel> list = new List<AllParaForAnnexureAssignmentModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_Update_paras_annex_fad";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CAT", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("REFP", OracleDbType.Varchar2).Value = REF_P;
                cmd.Parameters.Add("ANEX", OracleDbType.Varchar2).Value = ANNEX_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Varchar2).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Varchar2).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();

                    }
                }
            con.Dispose();
            return resp;
            }

        public string ParaShiftedTo(int OBS_ID, int NEW_ENT_ID, int OLD_ENT_ID, string P_IND)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            var resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_PARA_SHIFTING";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("NEW_ENT_ID", OracleDbType.Varchar2).Value = NEW_ENT_ID;
                cmd.Parameters.Add("OLD_ENT_ID", OracleDbType.Varchar2).Value = OLD_ENT_ID;
                cmd.Parameters.Add("O_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("P_IND", OracleDbType.Varchar2).Value = P_IND;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Varchar2).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<SettledParasMonitoringModel> GetSettledParasForMonitoring(int ENTITY_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<SettledParasMonitoringModel> list = new List<SettledParasMonitoringModel>();
            using (OracleCommand cmd = con.CreateCommand())

                {
                cmd.CommandText = "pkg_fad.P_GET_SETTLED_PARA_DETAILS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Varchar2).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("AUDITEE_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    SettledParasMonitoringModel chk = new SettledParasMonitoringModel();
                    chk.REPORTING_OFFICE = rdr["REPORTING_OFFICE"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.AU_OBS_ID = rdr["AU_OBS_ID"].ToString();
                    chk.COM_ID = rdr["COM_ID"].ToString();
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.SETTLED_BY = rdr["SETTLED_BY"].ToString();
                    chk.SETTLED_ON = rdr["settled_on"].ToString();
                    chk.RISK = rdr["RISK"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.PARA_CATEGORY = rdr["PARA_CATEGORY"].ToString();
                    chk.COMPLIANCE_CYCLE = rdr["COMPLIANCE_CYCLE"].ToString();
                    chk.AUDITED_BY = rdr["AUDITEDBY"].ToString();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    list.Add(chk);
                    }

                }
            con.Dispose();
            return list;
            }

        public List<ComplianceHistoryModel> GetSettledParaComplianceHistory(string REF_P, string OBS_ID)
            {

            List<ComplianceHistoryModel> stList = new List<ComplianceHistoryModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_GET_SETTLED_PARA_DETAILS_PARA_COMPLIANCE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("REFP", OracleDbType.Varchar2).Value = REF_P;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ComplianceHistoryModel st = new ComplianceHistoryModel();

                    st.REMARKS = rdr["remarks"].ToString();
                    st.ATTENDED_BY = rdr["attended_by"].ToString();

                    st.NAME = rdr["EMP_NAME"].ToString();
                    st.DESIGNATION = rdr["DESIGNATION"].ToString();
                    st.COM_SEQ_NO = rdr["COMPLIANCE_CYCLE"].ToString();
                    stList.Add(st);
                    }
                }
            con.Dispose();
            return stList;

            }

        public string SaveSettledParaCompliacne(string REF_P, string OBS_ID, string COMMENTS)
            {

            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.P_GET_SETTLED_PARA_DETAILS_PARA_COMPLIANCE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("REFP", OracleDbType.Varchar2).Value = REF_P;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ComplianceHistoryModel st = new ComplianceHistoryModel();

                    st.REMARKS = rdr["remarks"].ToString();
                    st.ATTENDED_BY = rdr["attended_by"].ToString();

                    st.NAME = rdr["EMP_NAME"].ToString();
                    st.DESIGNATION = rdr["DESIGNATION"].ToString();
                    st.COM_SEQ_NO = rdr["COMPLIANCE_CYCLE"].ToString();
                    resp = "";
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<ObservationReversalModel> GetEngagementDetailsForFadReview(int ENTITY_ID = 0)
            {
            List<ObservationReversalModel> resp = new List<ObservationReversalModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_get_audit_engagement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ent_id", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationReversalModel os = new ObservationReversalModel();
                    os.PLAN_ID = rdr["plan_id"].ToString();
                    os.ENG_ID = rdr["ENG_ID"].ToString();
                    os.TEAM_NAME = rdr["TEAM_NAME"].ToString();
                    os.AUDIT_START_DATE = rdr["AUDIT_STARTDATE"].ToString();
                    os.AUDIT_END_DATE = rdr["AUDIT_ENDDATE"].ToString();
                    os.OP_START_DATE = rdr["OP_STARTDATE"].ToString();
                    os.OP_END_DATE = rdr["OP_ENDDATE"].ToString();
                    os.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    os.AUDITED_BY_ID = rdr["Auditby_Id"].ToString();
                    os.STATUS_ID = rdr["STATUS_ID"].ToString();
                    os.STATUS = rdr["STATUS"].ToString();
                    os.REPORT_ID = rdr["RPT_ID"].ToString();
                    resp.Add(os);
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<EngagementObservationsForStatusReversalModel> GetAuditDetailsFAD(int ENG_ID = 0)
            {
            List<EngagementObservationsForStatusReversalModel> resp = new List<EngagementObservationsForStatusReversalModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_get_audit_glance";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    EngagementObservationsForStatusReversalModel os = new EngagementObservationsForStatusReversalModel();
                    os.ID = rdr["ID"].ToString();
                    os.MEMO_NO = rdr["MEMO_NO"].ToString();
                    os.FINAL_PARA = rdr["FINAL_PARA_NO"].ToString();
                    os.GIST = rdr["GIST"].ToString();
                    os.MEMO_DATE = rdr["MEMO_DATE"].ToString();
                    os.HEADING = rdr["HEADINGS"].ToString();
                    os.RISK = rdr["RISK"].ToString();
                    os.STATUS = rdr["STATUS"].ToString();
                    resp.Add(os);
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<FADAuditParasReviewModel> GetObservationDetailsForReport(int OBS_ID = 0)
            {
            List<FADAuditParasReviewModel> resp = new List<FADAuditParasReviewModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_get_audit_observtion";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OB_ID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FADAuditParasReviewModel os = new FADAuditParasReviewModel();
                    os.MEMO_NO = rdr["MEMO"].ToString();
                    os.PARA_NO = rdr["PARA_NO"].ToString();
                    os.ANNEX = rdr["ANNEX"].ToString();
                    os.PROCESS = rdr["HEADINGS"].ToString();
                    os.SUB_PROCESS = rdr["ASSIGNED_TO"].ToString();
                    os.CHECK_LIST = rdr["CHECK_LIST"].ToString();
                    os.OBS_GIST = rdr["GIST"].ToString();
                    os.PARA_TEXT = rdr["PARA_TEXT"].ToString();
                    os.AMOUNT_INV = rdr["AMOUNT_INV"].ToString();
                    os.NO_INSTANCES = rdr["NO_INSTANCES"].ToString();
                    os.PPNO = rdr["PPNO"].ToString();
                    os.RESP_ROLE = rdr["RESP_ROLE"].ToString();
                    os.RESP_AMOUNT = rdr["RESP_AMOUNT"].ToString();
                    os.AUDITEE_REPLY = rdr["auditee_reply"].ToString();
                    os.AUDITOR_COMMENTS = rdr["auditor_comments"].ToString();
                    os.HEADCOMMENTS = rdr["HEAD_COMMENTS"].ToString();
                    os.ROOT_CAUSE = rdr["ROOT_CAUSE"].ToString();
                    resp.Add(os);
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<AuditReportModel> GetAuditReportForFadReview(int RPT_ID = 0, int ENG_ID = 0)
            {
            List<AuditReportModel> resp = new List<AuditReportModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_fad.p_get_audit_Report";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("RPT_ID", OracleDbType.Int32).Value = RPT_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditReportModel or = new AuditReportModel();
                    or.ID = rdr["id"].ToString();
                    or.ENG_ID = rdr["eng_id"].ToString();
                    or.AUDIT_REPORT = rdr["audit_report"].ToString();
                    or.DOC_TYPE = rdr["doc_type"].ToString();
                    resp.Add(or);
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<ParaTextModel> Get_All_Para_Text(int comId)
            {

            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
           
            List<ParaTextModel> paraTexts = new List<ParaTextModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD.P_GetParaText";

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_com_id", OracleDbType.Int32).Value = comId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                    while (reader.Read())
                        {
                        ParaTextModel rpt = new ParaTextModel
                            {
                            ComId = reader["com_id"] as int? ?? default,
                            EntityId = reader["entity_id"] as int? ?? default,
                            OldParaId = reader["old_para_id"] as int? ?? default,
                            NewParaId = reader["new_para_id"] as int? ?? default,
                            AuditPeriod = reader["audit_period"]?.ToString(),
                            ParaStatus = reader["para_status"]?.ToString(),
                            AuditedBy = reader["audited_by"]?.ToString(),
                            Risk = reader["risk"]?.ToString(),
                            IND = reader["IND"]?.ToString(),
                            ParaNo = reader["para_no"]?.ToString(),
                            ParaAddedOn = reader["para_added_on"] as DateTime? ?? default,
                            GistOfParas = reader["gist_of_paras"]?.ToString(),
                            Text = reader["text"]?.ToString(),
                            ParaText = reader["para_text"]?.ToString()
                            };
                        paraTexts.Add(rpt); // ? Add to list
                        }
                    }
                }

            con.Dispose();
            return paraTexts; // ? Return correct variable
            }
        }
    }
