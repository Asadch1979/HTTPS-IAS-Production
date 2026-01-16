using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {
        public List<RiskModel> GetCOSORisks()
            {
            var con = this.DatabaseConnection();
            List<RiskModel> riskList = new List<RiskModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.p_GetCOSORisks";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskModel risk = new RiskModel();
                    risk.R_ID = Convert.ToInt32(rdr["R_ID"]);
                    risk.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    risk.RATING = rdr["RATING"].ToString();
                    riskList.Add(risk);
                    }
                }
            con.Dispose();
            return riskList;
            }

        public List<AssignedObservations> GetAssignedObservations(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AssignedObservations> list = new List<AssignedObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.p_GetAssignedObservations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AssignedObservations chk = new AssignedObservations();
                    chk.ID = Convert.ToInt32(rdr["OBS_ID"]);
                    chk.OBS_ID = Convert.ToInt32(rdr["OBS_ID"]);
                    chk.OBS_TEXT_ID = Convert.ToInt32(rdr["OBS_TEXT_ID"]);
                    chk.MEMO_DATE = rdr["MEMO_DATE"].ToString();
                    chk.MEMO_REPLY_DATE = rdr["REPLYDATE"].ToString();
                    chk.MEMO_NUMBER = rdr["MEMO_NUMBER"].ToString();
                    chk.AUDIT_YEAR = rdr["AUDIT_YEAR"].ToString();
                    chk.STATUS = rdr["STATUS"].ToString();
                    chk.STATUS_ID = rdr["STATUS_ID"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.GIST = rdr["GIST"].ToString();
                    chk.RESPONSE_ID = rdr["RESPONSE_ID"].ToString();
                    chk.CAN_REPLY = Convert.ToInt32(rdr["CANREPLY"].ToString());
                    chk.EDITABLE = Convert.ToInt32(rdr["EDITABLE"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AssignedObservations> GetAssignedObservationsForBranch(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AssignedObservations> list = new List<AssignedObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetAssignedObservationsForBranch";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("entityid", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();


                while (rdr.Read())
                    {
                    AssignedObservations chk = new AssignedObservations();
                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.OBS_ID = Convert.ToInt32(rdr["OBS_ID"]);
                    chk.OBS_TEXT_ID = Convert.ToInt32(rdr["OBS_TEXT_ID"]);
                    chk.ASSIGNEDTO_ROLE = Convert.ToInt32(rdr["ENTITY_ID"]);
                    chk.ASSIGNEDBY = Convert.ToInt32(rdr["ASSIGNEDBY"]);
                    chk.ASSIGNED_DATE = Convert.ToDateTime(rdr["ASSIGNED_DATE"]);
                    chk.IS_ACTIVE = rdr["IS_ACTIVE"].ToString();
                    chk.REPLIED = rdr["REPLIED"].ToString(); chk.REPLY_TEXT = rdr["REPLY_TEXT"].ToString();
                    chk.OBSERVATION_TEXT = rdr["OBSERVATION_TEXT"].ToString();

                    if (rdr["VIOLATION"].ToString() != null && rdr["VIOLATION"].ToString() != "")
                        chk.VIOLATION = rdr["VIOLATION"].ToString();

                    if (rdr["NATURE"].ToString() != null && rdr["NATURE"].ToString() != "")
                        chk.NATURE = rdr["NATURE"].ToString();

                    chk.STATUS = rdr["STATUS"].ToString();
                    chk.STATUS_ID = rdr["STATUS_ID"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    if (rdr["CANREPLY"].ToString() != null && rdr["CANREPLY"].ToString() != "")
                        chk.CAN_REPLY = Convert.ToInt32(rdr["CANREPLY"].ToString());
                    chk.MEMO_DATE = rdr["MEMO_DATE"].ToString();
                    chk.MEMO_REPLY_DATE = rdr["REPLYDATE"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<object> GetObservationText(int OBS_ID, int RESP_ID)
            {
            var sessionHandler = CreateSessionHandler();


            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            string ob_text = "";
            string ob_resp = "";

            List<object> list = new List<object>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetObservationText";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ob_text = rdr["TEXT"].ToString();

                    }
                list.Add(ob_text);
                if (RESP_ID > 0)
                    {
                    cmd.CommandText = "pkg_ar.P_GetOBSERVATIONSAUDITEERESPONSE";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr2 = cmd.ExecuteReader();

                    while (rdr2.Read())
                        {
                        ob_resp = rdr2["REPLY"].ToString();
                        }
                    list.Add(ob_resp);
                    List<AuditeeResponseEvidenceModel> modellist = new List<AuditeeResponseEvidenceModel>();
                    cmd.CommandText = "pkg_ar.P_get_AUDITEE_OBSERVATION_RESPONSE_evidences";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("RESP_ID", OracleDbType.Int32).Value = RESP_ID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr3 = cmd.ExecuteReader();
                    while (rdr3.Read())
                        {
                        AuditeeResponseEvidenceModel am = new AuditeeResponseEvidenceModel();
                        am.FILE_ID = rdr3["ID"].ToString();
                        am.IMAGE_NAME = rdr3["FILE_NAME"].ToString();
                        am.IMAGE_DATA = "";
                        am.SEQUENCE = Convert.ToInt32(rdr3["SEQUENCE"].ToString());
                        am.IMAGE_TYPE = rdr3["FILE_TYPE"].ToString();
                        modellist.Add(am);
                        }
                    list.Add(modellist);

                    }
                else
                    {
                    list.Add("");
                    list.Add(new List<object>());
                    }
                }
            con.Dispose();
            return list;
            }
        public List<ObservationResponsiblePPNOModel> GetObservationResponsiblePPNOs(int OBS_ID, int ENG_ID)
            {
            var con = this.DatabaseConnection();
            List<ObservationResponsiblePPNOModel> list = new List<ObservationResponsiblePPNOModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetObservationResponsible";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationResponsiblePPNOModel usr = new ObservationResponsiblePPNOModel();
                    usr.RESP_ROW_ID = Convert.ToInt32(rdr["RESP_ROW_ID"].ToString());
                    usr.EMP_NAME = rdr["EMP_NAME"].ToString();
                    usr.PP_NO = rdr["PP_NO"].ToString();
                    usr.LOAN_CASE = rdr["LOANCASE"].ToString();
                    usr.LC_AMOUNT = rdr["LCAMOUNT"].ToString();
                    usr.ACCOUNT_NUMBER = rdr["ACCNUMBER"].ToString();
                    usr.ACC_AMOUNT = rdr["ACAMOUNT"].ToString();
                    list.Add(usr);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<ObservationResponsiblePPNOModel> GetOldParasObservationResponsiblePPNOs(int OLD_PARA_ID = 0, int NEW_PARA_ID = 0, string INDICATOR = "")
            {
            var con = this.DatabaseConnection();
            List<ObservationResponsiblePPNOModel> list = new List<ObservationResponsiblePPNOModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.p_GetParaComplianceResponsible";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Old_id", OracleDbType.Int32).Value = OLD_PARA_ID;
                cmd.Parameters.Add("new_id", OracleDbType.Int32).Value = NEW_PARA_ID;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationResponsiblePPNOModel usr = new ObservationResponsiblePPNOModel();
                    usr.EMP_NAME = rdr["EMP_NAME"].ToString();
                    usr.PP_NO = rdr["PP_NO"].ToString();
                    usr.LOAN_CASE = rdr["LOANCASE"].ToString();
                    usr.LC_AMOUNT = rdr["LCAMOUNT"].ToString();
                    usr.ACCOUNT_NUMBER = rdr["ACCNUMBER"].ToString();
                    usr.ACC_AMOUNT = rdr["ACAMOUNT"].ToString();
                    list.Add(usr);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeResponseEvidenceModel> GetOldParasEvidences(string textId)
            {
            var list = new List<AuditeeResponseEvidenceModel>();

            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_ae.P_GetPostAuditCompliance_Evidence";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("TEXT_ID", OracleDbType.Varchar2).Value = textId;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                var usr = new AuditeeResponseEvidenceModel
                                    {
                                    IMAGE_NAME = rdr["FILE_NAME"].ToString(),
                                    IMAGE_DATA = "",
                                    SEQUENCE = Convert.ToInt32(rdr["SEQUENCE"]),
                                    LENGTH = Convert.ToInt32(rdr["LENGTH"]),
                                    FILE_ID = (rdr["id"].ToString())
                                    };

                                // Handle CLOB data
                                /*var clob = rdr.GetOracleClob(rdr.GetOrdinal("FILE_DATA"));
                                if (clob != null)
                                {
                                    usr.IMAGE_DATA = clob.Value; // Get the entire CLOB data as a string
                                }*/

                                list.Add(usr);
                                }
                            }
                        }
                    }
                }
            catch (Exception)
                {

                throw;
                }

            return list;
            }

        public List<AuditeeResponseEvidenceModel> GetOldParasEvidencesTEMP(string TEXT_ID)
            {
            var con = this.DatabaseConnection();
            List<AuditeeResponseEvidenceModel> list = new List<AuditeeResponseEvidenceModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetPostAuditCompliance_Evidence";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("TEXT_ID", OracleDbType.Varchar2).Value = TEXT_ID;
                cmd.FetchSize = 10000;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    var usr = new AuditeeResponseEvidenceModel
                        {
                        IMAGE_NAME = rdr["FILE_NAME"].ToString(),
                        SEQUENCE = Convert.ToInt32(rdr["SEQUENCE"]),
                        LENGTH = Convert.ToInt32(rdr["LENGTH"])
                        };

                    // Handle CLOB data


                    list.Add(usr);
                    }
                }
            con.Dispose();
            return list;
            }

        public async Task<bool> ResponseAuditObservation(ObservationResponseModel ob, string SUBFOLDER)
            {
            int AUD_RESP_ID = 0;
            List<AuditeeResponseEvidenceModel> EVIDENCE_LIST = new List<AuditeeResponseEvidenceModel>();
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            ob.REPLIEDBY = Convert.ToInt32(loggedInUser.PPNumber);
            ob.REPLIEDDATE = System.DateTime.Now;
            ob.REMARKS = "";
            ob.SUBMITTED = "Y";
            ob.REPLY_ROLE = 0;
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_AUDITEE_OBSERVATION_RESPONSE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("AUOBSID", OracleDbType.Int32).Value = ob.AU_OBS_ID ?? (object)DBNull.Value;
                cmd.Parameters.Add("REPLYDATA", OracleDbType.Clob).Value = ob.REPLY ?? string.Empty;
                cmd.Parameters.Add("REPLIEDBY", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("OBSTEXTID", OracleDbType.Int32).Value = ob.OBS_TEXT_ID ?? (object)DBNull.Value;
                cmd.Parameters.Add("REPLYROLE", OracleDbType.Int32).Value = ob.REPLY_ROLE ?? (object)DBNull.Value;
                cmd.Parameters.Add("REMARKS", OracleDbType.Varchar2).Value = ob.REMARKS;
                cmd.Parameters.Add("SUBMITTED", OracleDbType.Varchar2).Value = ob.SUBMITTED;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AUD_RESP_ID = Convert.ToInt32(rdr["RESP_ID"]);
                    }

                EVIDENCE_LIST = await this.GetAttachedAuditeeEvidencesFromDirectory(SUBFOLDER);
                int index = 1;
                if (EVIDENCE_LIST != null)
                    {
                    if (EVIDENCE_LIST.Count > 0)
                        {
                        foreach (var item in EVIDENCE_LIST)
                            {
                            if (!item.LENGTH.HasValue && item.IMAGE_LENGTH.HasValue)
                                {
                                item.LENGTH = Convert.ToInt32(item.IMAGE_LENGTH.Value);
                                }
                            if (!item.LENGTH.HasValue)
                                {
                                continue;
                                }
                            string fileName = item.FILE_NAME;
                            cmd.CommandText = "pkg_ae.P_AUDITEE_OBSERVATION_RESPONSE_EVIDENCES";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("RESPID", OracleDbType.Int32).Value = AUD_RESP_ID;
                            cmd.Parameters.Add("AUOBSID", OracleDbType.Int32).Value = ob.AU_OBS_ID ?? (object)DBNull.Value;
                            cmd.Parameters.Add("FILENAME", OracleDbType.Varchar2).Value = fileName;
                            cmd.Parameters.Add("FILETYPE", OracleDbType.Varchar2).Value = item.IMAGE_TYPE;
                            cmd.Parameters.Add("LENGTH", OracleDbType.Int32).Value = item.LENGTH;
                            cmd.Parameters.Add("ENTEREDBY", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                            cmd.Parameters.Add("FILEDATA", OracleDbType.Clob).Value = item.IMAGE_DATA;
                            cmd.Parameters.Add("SEQUENCE", OracleDbType.Int32).Value = (index);
                            cmd.Parameters.Add("TEXT_ID", OracleDbType.Int32).Value = ob.OBS_TEXT_ID ?? (object)DBNull.Value;
                            cmd.ExecuteReader();
                            index++;


                            }
                        }
                    }

                this.DeleteSubFolderDirectoryInAuditeeEvidenceFromServer(SUBFOLDER);
                }
            con.Dispose();
            return true;
            }

        public List<AuditeeOldParasModel> GetAuditeeOldParasEntities()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetAuditeeOldParasentitiesFAD";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
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

        public List<AuditeeOldParasModel> GetAuditeeOldParas(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetAuditeeOldParas";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeOldParasModel chk = new AuditeeOldParasModel();


                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    //chk.REF_P = rdr["ref_p"].ToString();
                    chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                    //chk.AMOUNT = rdr["amount"].ToString();
                    chk.PARA_CATEGORY = rdr["PARA_CATEGORY"].ToString();
                    chk.REPORT_NAME = rdr["PARA_CATEGORY"].ToString();
                    //chk.AU_OBS_ID = rdr["AU_OBS_ID"].ToString();
                    //chk.VOL_I_II = rdr["vol_i_ii"].ToString();
                    chk.AUDITEDBY = rdr["AUDITED_BY"].ToString();




                    chk.ENTITY_CODE = Convert.ToInt32(rdr["ENTITY_CODE"]);
                    chk.TYPE_ID = Convert.ToInt32(rdr["TYPE_ID"]);


                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.AUDITEE_RESPONSE = rdr["AUDITEE_RESPONSE"].ToString();
                    chk.AUDITOR_REMARKS = rdr["AUDITOR_REMARKS"].ToString();


                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.TYPE_DES = rdr["entitytypedesc"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<OldParasModelCAD> GetOldParasManagement()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<OldParasModelCAD> list = new List<OldParasModelCAD>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetOldParaManagement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModelCAD chk = new OldParasModelCAD();
                    chk.PARA_ID = Convert.ToInt32(rdr["PARA_ID"]);
                    chk.AUDIT_PERIOD = rdr["PERIOD"].ToString();
                    chk.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"].ToString());
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.AUDITED_BY = rdr["AUDITED_BY"].ToString();
                    chk.PARA_STATUS = rdr["PARA_STATUS"].ToString();
                    if (rdr["V_CAT_ID"].ToString() != null && rdr["V_CAT_ID"].ToString() != "")
                        chk.V_CAT_ID = Convert.ToInt32(rdr["V_CAT_ID"].ToString());
                    if (rdr["V_CAT_NATURE_ID"].ToString() != null && rdr["V_CAT_NATURE_ID"].ToString() != "")
                        chk.V_CAT_NATURE_ID = Convert.ToInt32(rdr["V_CAT_NATURE_ID"].ToString());
                    if (rdr["RISK_ID"].ToString() != null && rdr["RISK_ID"].ToString() != "")
                        chk.RISK_ID = Convert.ToInt32(rdr["RISK_ID"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AddOldParasCADReply(int ID, int V_CAT_ID, int V_CAT_NATURE_ID, int RISK_ID, string REPLY)
            {
            string response = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_updateoldparamanagement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PARAID", OracleDbType.Int32).Value = ID;
                cmd.Parameters.Add("VCATID", OracleDbType.Int32).Value = V_CAT_ID;
                cmd.Parameters.Add("VCATNATUREID", OracleDbType.Int32).Value = V_CAT_NATURE_ID;
                cmd.Parameters.Add("RISKID", OracleDbType.Int32).Value = RISK_ID;
                cmd.Parameters.Add("PARATEXT", OracleDbType.Clob).Value = REPLY;
                cmd.Parameters.Add("CREATEDBY", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    response = rdr["REMARKS"].ToString();
                    }

                }
            con.Dispose();
            return response;
            }

        public string AddOldParasCADCompliance(OldParaComplianceModel opc)
            {
            string response = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_UpdateAuditeeOldParasresponse";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Paraid", OracleDbType.Int32).Value = opc.ParaRef;
                cmd.Parameters.Add("cdate", OracleDbType.Date).Value = opc.ComplianceDate;
                cmd.Parameters.Add("Text", OracleDbType.Clob).Value = opc.AuditeeCompliance;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("Remarks", OracleDbType.Clob).Value = opc.AuditorRemarks;
                cmd.Parameters.Add("imprec", OracleDbType.NVarchar2).Value = opc.CnIRecommendation;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    response = rdr["REMARKS"].ToString();
                    }

                }
            con.Dispose();
            return response;
            }

        public List<AuditeeEntitiesModel> GetAuditeeAssignedEntities()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> list = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetAuditeeAssignedEntities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel chk = new AuditeeEntitiesModel();
                    chk.CODE = Convert.ToInt32(rdr["CODE"].ToString());
                    chk.NAME = rdr["NAME"].ToString();
                    chk.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"].ToString());
                    chk.ENG_ID = Convert.ToInt32(rdr["engplanid"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeEntitiesModel> GetCCQsEntities()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> list = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetCCQsEntities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel chk = new AuditeeEntitiesModel();
                    chk.CODE = Convert.ToInt32(rdr["CODE"].ToString());
                    chk.NAME = rdr["NAME"].ToString();
                    chk.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<GetOldParasBranchComplianceModel> GetParasForComplianceByAuditee()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<GetOldParasBranchComplianceModel> list = new List<GetOldParasBranchComplianceModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetParasForComplianceByAuditee";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GetOldParasBranchComplianceModel chk = new GetOldParasBranchComplianceModel();
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.NAME = rdr["name"].ToString();
                    chk.PARA_NO = rdr["para_no"].ToString();
                    chk.NEW_PARA_ID = rdr["new_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["new_para_id"].ToString());
                    chk.OLD_PARA_ID = rdr["old_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["old_para_id"].ToString());
                    chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                    chk.AUDIT_BY_ID = rdr["auditby_id"].ToString();
                    chk.AUDITOR_REMARKS = rdr["audit_reply"].ToString();
                    chk.NEXT_R_ID = rdr["next_r_id"].ToString();
                    chk.PREV_R_ID = rdr["per_r_id"].ToString();
                    chk.STATUS_UP = rdr["c_status_up"].ToString();
                    chk.STATUS_DOWN = rdr["c_status_down"].ToString();
                    chk.INDICATOR = rdr["ind"].ToString();
                    chk.COM_ID = rdr["COM_ID"].ToString();
                    chk.AUDIT_DATE = rdr["audit_date"].ToString();
                    chk.RECEIVED_FROM = rdr["rec_from"].ToString();
                    chk.PREV_ROLE = "";
                    chk.NEXT_ROLE = "SUBMIT COMPLIANCE";
                    list.Add(chk);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<GetOldParasBranchComplianceModel> GetParasForReviewComplianceByAuditee()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<GetOldParasBranchComplianceModel> list = new List<GetOldParasBranchComplianceModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetParasForCompliancereview ";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GetOldParasBranchComplianceModel chk = new GetOldParasBranchComplianceModel();
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.NAME = rdr["name"].ToString();
                    chk.PARA_NO = rdr["para_no"].ToString();
                    chk.PARA_RISK = rdr["rsk"].ToString();
                    chk.NEW_PARA_ID = rdr["new_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["new_para_id"].ToString());
                    chk.OLD_PARA_ID = rdr["old_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["old_para_id"].ToString());
                    chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                    chk.AUDIT_BY_ID = rdr["auditby_id"].ToString();
                    chk.NEXT_R_ID = rdr["next_r_id"].ToString();
                    chk.PREV_R_ID = rdr["per_r_id"].ToString();
                    chk.STATUS_UP = rdr["c_status_up"].ToString();
                    chk.AUDITOR_REMARKS = rdr["audit_reply"].ToString();
                    chk.STATUS_DOWN = rdr["c_status_down"].ToString();
                    chk.PREV_ROLE = "Referred Back";
                    chk.NEXT_ROLE = (loggedInUser.UserRoleID == 44 || loggedInUser.UserRoleID == 41) ? "Settle" : "Recommend";
                    chk.RECEIVED_FROM = rdr["rec_from"].ToString();
                    chk.INDICATOR = rdr["ind"].ToString();
                    chk.COM_ID = rdr["COM_ID"].ToString();

                    list.Add(chk);

                    }
                }
            con.Dispose();
            return list;
            }

        public GetOldParasBranchComplianceTextModel GetParaComplianceText(int OLD_PARA_ID, int NEW_PARA_ID, string INDICATOR)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            GetOldParasBranchComplianceTextModel chk = new GetOldParasBranchComplianceTextModel();


            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetParasForComplianceByAuditee_text";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Old_id", OracleDbType.Int32).Value = OLD_PARA_ID;
                cmd.Parameters.Add("new_id", OracleDbType.Int32).Value = NEW_PARA_ID;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {

                    chk.PARA_TEXT = rdr["text"].ToString();
                    chk.PARA_TEXT_ID = rdr["text_id"].ToString();
                    chk.GIST_OF_PARA = rdr["gist_of_paras"].ToString();
                    chk.RESPONSIBLE_PPs = this.GetOldParasObservationResponsiblePPNOs(OLD_PARA_ID, NEW_PARA_ID, INDICATOR);
                    }
                }
            con.Dispose();
            return chk;
            }

        public GetOldParasBranchComplianceTextModel GetOldParasComplianceCycleText(string COM_ID, string C_CYCLE)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            GetOldParasBranchComplianceTextModel resp = new GetOldParasBranchComplianceTextModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetParasForComplianceforhistory";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_CYCLE", OracleDbType.Int32).Value = C_CYCLE;
                cmd.Parameters.Add("COM_ID", OracleDbType.Int32).Value = COM_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp.PARA_TEXT = rdr["reply"].ToString();
                    resp.PARA_TEXT_ID = rdr["text_id"].ToString();
                    resp.OBS_TEXT = rdr["para_text"].ToString();
                    resp.EVIDENCES = this.GetOldParasEvidences(resp.PARA_TEXT_ID);
                    }
                }
            con.Dispose();
            return resp;
            }

        public AuditeeResponseEvidenceModel GetPostComplianceEvidenceData(string FILE_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var resp = new AuditeeResponseEvidenceModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetPostAuditCompliance_Evidence_FileData";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("FILE_ID", OracleDbType.Varchar2).Value = FILE_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = new AuditeeResponseEvidenceModel
                        {
                        IMAGE_NAME = rdr["FILE_NAME"].ToString(),
                        SEQUENCE = Convert.ToInt32(rdr["SEQUENCE"]),
                        LENGTH = Convert.ToInt32(rdr["LENGTH"]),
                        FILE_ID = (rdr["id"].ToString())
                        };

                    // Handle CLOB data
                    var clob = rdr.GetOracleClob(rdr.GetOrdinal("FILE_DATA"));
                    if (clob != null)
                        {
                        resp.IMAGE_DATA = clob.Value; // Get the entire CLOB data as a string
                        }

                    }
                }
            con.Dispose();
            return resp;
            }

        public AuditeeResponseEvidenceModel GetCAUParasPostComplianceEvidenceData(string FILE_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var resp = new AuditeeResponseEvidenceModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetPostAuditCompliance_Evidence_FileData_CAU";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("FILE_ID", OracleDbType.Varchar2).Value = FILE_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = new AuditeeResponseEvidenceModel
                        {
                        IMAGE_NAME = rdr["FILE_NAME"].ToString(),
                        SEQUENCE = Convert.ToInt32(rdr["SEQUENCE"]),
                        LENGTH = Convert.ToInt32(rdr["LENGTH"]),
                        FILE_ID = (rdr["id"].ToString())
                        };

                    // Handle CLOB data
                    var clob = rdr.GetOracleClob(rdr.GetOrdinal("FILE_DATA"));
                    if (clob != null)
                        {
                        resp.IMAGE_DATA = clob.Value; // Get the entire CLOB data as a string
                        }

                    }
                }
            con.Dispose();
            return resp;
            }

        public GetOldParasBranchComplianceTextModel GetOldParasBranchComplianceTextRef(string Ref_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            GetOldParasBranchComplianceTextModel chk = new GetOldParasBranchComplianceTextModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetAuditeeOldParasFADText_Ref";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = Ref_P;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    chk.GIST_OF_PARA = rdr["checklistdetail"].ToString();
                    chk.PARA_TEXT = rdr["para_text"].ToString();
                    // chk.RESPONSIBLE_PPs = this.GetOldParasObservationResponsiblePPNOs(OLD_PARA_ID, NEW_PARA_ID,INDICATOR);
                    //   chk.EVIDENCES = this.GetOldParasEvidences(Ref_P, chk.PARA_CATEGORY, REPLY_DATE, OBS_ID);
                    }
                }
            con.Dispose();
            return chk;
            }

        public GetOldParasBranchComplianceTextModel GetOldParasBranchComplianceTextForZone(string Ref_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            GetOldParasBranchComplianceTextModel chk = new GetOldParasBranchComplianceTextModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetAuditeeOldParasFADtext_Reviewer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = Ref_P;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    chk.PARA_TEXT = rdr["para_text"].ToString();
                    chk.GIST_OF_PARA = rdr["para_text"].ToString();
                    //chk.RESPONSIBLE_PPs = this.GetOldParasObservationResponsiblePPNOs(Ref_P, chk.PARA_CATEGORY);
                    //chk.EVIDENCES = this.GetOldParasEvidences(Ref_P, chk.PARA_CATEGORY, REPLY_DATE, OBS_ID);
                    }
                }
            con.Dispose();
            return chk;
            }

        public GetOldParasBranchComplianceTextModel GetOldParasBranchComplianceTextForZoneRef(string Ref_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            GetOldParasBranchComplianceTextModel chk = new GetOldParasBranchComplianceTextModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetAuditeeOldParasFADtext_Reviewer_Ref";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = Ref_P;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    chk.PARA_TEXT = rdr["para_text"].ToString();
                    chk.GIST_OF_PARA = rdr["para_text"].ToString();
                    //chk.RESPONSIBLE_PPs = this.GetOldParasObservationResponsiblePPNOs(Ref_P, chk.PARA_CATEGORY);
                    //chk.EVIDENCES = this.GetOldParasEvidences(Ref_P, chk.PARA_CATEGORY, REPLY_DATE, OBS_ID);
                    }
                }
            con.Dispose();
            return chk;
            }

        public async Task<string> SubmitPostAuditCompliance(string OLD_PARA_ID, int NEW_PARA_ID, string INDICATOR, string COMPLIANCE, string COMMENTS, List<AuditeeResponseEvidenceModel> EVIDENCE_LIST, string SUBFOLDER)
            {

            string resp = "";
            int TEXT_ID = 0;
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_SubmitPostAuditCompliance";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Old_id", OracleDbType.Int32).Value = OLD_PARA_ID;
                cmd.Parameters.Add("new_id", OracleDbType.Int32).Value = NEW_PARA_ID;
                cmd.Parameters.Add("Entity_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("Auditee_COM", OracleDbType.Clob).Value = COMPLIANCE;
                cmd.Parameters.Add("A_COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("P_IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    TEXT_ID = Convert.ToInt32(rdr["text_id"].ToString());

                    }

                EVIDENCE_LIST = await this.GetAttachedFilesFromDirectory(SUBFOLDER);
                int index = 1;
                if (EVIDENCE_LIST != null)
                    {
                    if (EVIDENCE_LIST.Count > 0)
                        {
                        foreach (var item in EVIDENCE_LIST)
                            {
                            string fileName = item.FILE_NAME;
                            cmd.CommandText = "pkg_ae.P_SubmitPostAuditCompliance_Evidence";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("TEXT_ID", OracleDbType.Varchar2).Value = TEXT_ID;
                            cmd.Parameters.Add("FILENAME", OracleDbType.Varchar2).Value = fileName;
                            cmd.Parameters.Add("LEN_ID", OracleDbType.Int32).Value = item.IMAGE_LENGTH;
                            cmd.Parameters.Add("ENTER_BY", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                            cmd.Parameters.Add("FILETYPE", OracleDbType.Varchar2).Value = item.IMAGE_TYPE;
                            cmd.Parameters.Add("FILEDATA", OracleDbType.Clob).Value = item.IMAGE_DATA;
                            cmd.Parameters.Add("SEQ_ID", OracleDbType.Int32).Value = (index);
                            cmd.ExecuteReader();
                            index++;
                            //this.SaveImage(item.IMAGE_DATA, fileName);
                            }
                        }
                    }

                this.DeleteSubFolderDirectoryFromServer(SUBFOLDER);
                }

            con.Dispose();
            return resp;
            }

        public string SubmitPostAuditComplianceReview(string OLD_PARA_ID, int NEW_PARA_ID, string INDICATOR, string COMPLIANCE, string COMMENTS, List<AuditeeResponseEvidenceModel> EVIDENCE_LIST)
            {

            string resp = "";
            string to_email = "";
            string cc_email = "";
            string cc2_email = "";
            string para_no = "";
            string para_gist = "";
            string para_status = "";

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_SubmitPostAuditCompliance_review";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Old_id", OracleDbType.Int32).Value = OLD_PARA_ID;
                cmd.Parameters.Add("new_id", OracleDbType.Int32).Value = NEW_PARA_ID;
                cmd.Parameters.Add("Ent_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("A_COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("P_IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    para_no = rdr["PARA_NO"].ToString();
                    para_gist = rdr["GIST_OF_PARAS"].ToString();
                    to_email = rdr["TO_EMAIL"].ToString();
                    cc_email = rdr["CC_EMAIL"].ToString();
                    cc2_email = rdr["CC_EMAIL2"].ToString();
                    cc2_email = rdr["CC_EMAIL2"].ToString();
                    para_status = rdr["para_status"].ToString();


                    }
                }
            if (to_email != "")
                {
                EmailNotification.NotifyParaStatus(_configuration, para_no, para_status, para_gist, to_email, cc_email, cc2_email, _httpCon?.HttpContext?.RequestServices);
                }

            con.Dispose();
            return resp;
            }

        public List<GetOldParasForComplianceReviewer> GetOldParasForReviewer()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<GetOldParasForComplianceReviewer> list = new List<GetOldParasForComplianceReviewer>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.p_getoldparasforreviewer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("userentityid", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GetOldParasForComplianceReviewer chk = new GetOldParasForComplianceReviewer();
                    chk.AUDITEENAME = rdr["AUDITEENAME"].ToString();

                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GISTOFPARA = rdr["GISTOFPARA"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.AU_OBS_ID = rdr["OBS_ID"].ToString();
                    chk.ID = rdr["ID"].ToString();
                    if (rdr["replieddate"].ToString() != null && rdr["replieddate"].ToString() != "")
                        chk.REPLY_DATE = rdr["replieddate"].ToString().Split(" ")[0];
                    chk.PARA_CATEGORY = rdr["PARA_CATEGORY"].ToString();
                    chk.PARENT_ID = rdr["PARENT_ID"].ToString();
                    chk.SEQUENCE = rdr["sequence"].ToString();
                    chk.AUDITED_BY = rdr["auditedby"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<GetOldParasForComplianceReviewer> GetOldParasForReviewerRef()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<GetOldParasForComplianceReviewer> list = new List<GetOldParasForComplianceReviewer>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.p_getoldparasforreviewer_ref";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("userentityid", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GetOldParasForComplianceReviewer chk = new GetOldParasForComplianceReviewer();
                    chk.AUDITEENAME = rdr["AUDITEENAME"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GISTOFPARA = rdr["GISTOFPARA"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.AU_OBS_ID = rdr["OBS_ID"].ToString();
                    chk.ID = rdr["ID"].ToString();
                    chk.REPLY = rdr["REPLY"].ToString();
                    if (rdr["replieddate"].ToString() != null && rdr["replieddate"].ToString() != "")
                        chk.REPLY_DATE = rdr["replieddate"].ToString().Split(" ")[0];
                    chk.PARA_CATEGORY = rdr["PARA_CATEGORY"].ToString();
                    chk.PARENT_ID = rdr["PARENT_ID"].ToString();
                    chk.SEQUENCE = rdr["SEQUENCE"].ToString();
                    chk.AUDITED_BY = rdr["auditedby"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AddOldParasComplianceReviewer(string Para_ID, string PARA_CAT, string Reply, string r_status, string OBS_ID, int PARENT_ID, string SEQUENCE, string AUDITED_BY)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_AddOldParasReviewer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("PID", OracleDbType.Varchar2).Value = Para_ID;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("Remark", OracleDbType.Varchar2).Value = Reply;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CAT;
                cmd.Parameters.Add("PARENTID", OracleDbType.Int32).Value = PARENT_ID;
                cmd.Parameters.Add("ROL_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("r_status", OracleDbType.Int32).Value = r_status;
                cmd.Parameters.Add("seq_id", OracleDbType.Varchar2).Value = SEQUENCE;
                cmd.Parameters.Add("AUDIT_ID", OracleDbType.Varchar2).Value = AUDITED_BY;
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

        public bool AddOldParasReply(int ID, string REPLY)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            bool success = false;
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_AddOldParasReply";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("PID", OracleDbType.Int32).Value = ID;
                cmd.Parameters.Add("REPLY", OracleDbType.Clob).Value = REPLY;
                cmd.ExecuteReader();
                success = true;
                }
            con.Dispose();
            return success;
            }

        public List<GetOldParasBranchComplianceModel> GetOldParasBranchComplianceTextupdate()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<GetOldParasBranchComplianceModel> list = new List<GetOldParasBranchComplianceModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetAuditeeAllParasFAD";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GetOldParasBranchComplianceModel chk = new GetOldParasBranchComplianceModel();
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.NAME = rdr["name"].ToString();
                    chk.PARA_NO = rdr["para_no"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string GetComplianceTextAuditee(int COMPLIANCE_ID)
            {
            string resp = "";
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_get_v_auditee_paras_compliance_history_auditee_text";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("para_id", OracleDbType.Int32).Value = COMPLIANCE_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["reply"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string GetComplianceHistoryCountAuditee(string REF_P, string OBS_ID)
            {
            string resp = "";
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_get_v_auditee_paras_compliance_history_num_auditee";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("REF_P", OracleDbType.Varchar2).Value = REF_P;
                cmd.Parameters.Add("OBSID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["no_of_records"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<PostComplianceHistoryModel> GetComplianceHistory(string COM_ID)
            {

            List<PostComplianceHistoryModel> stList = new List<PostComplianceHistoryModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetParasForCompliancehistory";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("COM_ID", OracleDbType.Varchar2).Value = COM_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    PostComplianceHistoryModel st = new PostComplianceHistoryModel();
                    st.HIST_ID = Convert.ToInt32(rdr["HIST_ID"].ToString());
                    st.COM_ID = Convert.ToInt32(rdr["COM_ID"].ToString());
                    st.COM_CYCLE = rdr["COM_CYCLE"].ToString();
                    st.COM_STAGE = rdr["COM_STAGE"].ToString();
                    st.COM_STATUS = rdr["COM_STATUS"].ToString();
                    st.COMMENT_BY_ROLE = rdr["COMMENT_BY_ROLE"].ToString();
                    st.NAME = rdr["NAME"].ToString();
                    st.DESIGNATION = "";
                    st.PP_NO = rdr["PP_NO"].ToString();
                    st.COMMENT_ON = rdr["COMMENT_ON"].ToString();
                    st.COMMENTS = rdr["COMMENTS"].ToString();
                    st.COM_FLOW = rdr["COM_FLOW"].ToString();
                    stList.Add(st);
                    }
                }
            con.Dispose();
            return stList;

            }

        public List<CAUParaForComplianceModel> GetCAUParasForPostCompliance()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<CAUParaForComplianceModel> list = new List<CAUParaForComplianceModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetParasForComplianceByCAU";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    CAUParaForComplianceModel chk = new CAUParaForComplianceModel();
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.PARA_NO = rdr["para_no"].ToString();
                    chk.NEW_PARA_ID = rdr["new_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["new_para_id"].ToString());
                    chk.OLD_PARA_ID = rdr["old_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["old_para_id"].ToString());
                    chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                    chk.INDICATOR = rdr["ind"].ToString();
                    chk.CAU_STATUS = rdr["cau_status"].ToString();
                    chk.CAU_ASSIGNED_ENT_ID = rdr["cau_assigned_ent_id"].ToString();
                    chk.COM_ID = rdr["com_id"].ToString();


                    list.Add(chk);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<UserRelationshipModel> GetrealtionshiptypeForCAU()
            {

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetrealtionshiptypeforCAU";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    UserRelationshipModel entity = new UserRelationshipModel();
                    entity.ENTITY_REALTION_ID = Convert.ToInt32(rdr["ENTITY_REALTION_ID"]);
                    entity.FIELD_NAME = rdr["FIELD_NAME"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<UserRelationshipModel> GetParentRelationshipForCAU(int r_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();


            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetparentrepofficeforCAU";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("rid", OracleDbType.Int32).Value = r_id;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    UserRelationshipModel entity = new UserRelationshipModel();
                    entity.ENTITY_REALTION_ID = Convert.ToInt32(rdr["ENTITY_REALTION_ID"]);
                    entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    entity.ACTIVE = rdr["ACTIVE"].ToString();
                    entity.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    entity.ENTITYTYPEDESC = rdr["ENTITYTYPEDESC"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<UserRelationshipModel> GetChildRelationshipForCAU(int e_r_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            if (e_r_id == 0)
                e_r_id = Convert.ToInt32(loggedInUser.UserEntityID);

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetchildpostingforCAU";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ENT_ID", OracleDbType.Int32).Value = e_r_id;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    UserRelationshipModel entity = new UserRelationshipModel();
                    entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    entity.C_NAME = rdr["C_NAME"].ToString();
                    entity.C_TYPE_ID = rdr["TYPEID"].ToString();
                    entity.COMPLICE_BY = rdr["COMPLICE_BY"].ToString();
                    entity.AUDIT_BY = rdr["AUDIT_BY"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public string SubmitCAUParaToBranch(string COM_ID, string BR_ENT_ID, string CAU_COMMENTS)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_FORWARD_CAU_PARA_TO_BRANCH";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_ID", OracleDbType.Varchar2).Value = COM_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("B_ENT_ID", OracleDbType.Int32).Value = BR_ENT_ID;
                cmd.Parameters.Add("CAU_COMMENTS", OracleDbType.Varchar2).Value = CAU_COMMENTS;
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

        public ParaTextModel GetCAUParaToBranchParaText(string COM_ID, string INDICATOR)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            ParaTextModel resp = new ParaTextModel();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetParasForCompliance_CAU_para_text";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = COM_ID;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp.MEMO_TXT = rdr["para_text"].ToString();
                    resp.BRANCH_REPLY = rdr["reply"].ToString();
                    resp.CAU_INSTRUCTION = rdr["cau_instructions"].ToString();
                    resp.TEXT_ID = Convert.ToInt32(rdr["text_id"].ToString());
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<CAUParaForComplianceModel> GetCAUParasForPostComplianceSubmittedToBranch()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<CAUParaForComplianceModel> list = new List<CAUParaForComplianceModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetParasForComplianceByCAU_BY_BRANCH";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    CAUParaForComplianceModel chk = new CAUParaForComplianceModel();
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.PARA_NO = rdr["para_no"].ToString();
                    chk.NEW_PARA_ID = rdr["new_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["new_para_id"].ToString());
                    chk.OLD_PARA_ID = rdr["old_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["old_para_id"].ToString());
                    chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                    chk.INDICATOR = rdr["ind"].ToString();
                    chk.CAU_INSTRUCTIONS = rdr["cau_instructions"].ToString();
                    chk.CAU_NAME = rdr["CAU_NAME"].ToString();
                    chk.COM_ID = rdr["com_id"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public async Task<string> SubmitCAUParaByBranch(string COM_ID, string TEXT_ID, string BR_COMMENTS)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_SubmitPostAuditCompliance_BY_BRANCH";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_ID", OracleDbType.Varchar2).Value = COM_ID;
                cmd.Parameters.Add("T_ID", OracleDbType.Varchar2).Value = TEXT_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("Auditee_COM", OracleDbType.Clob).Value = BR_COMMENTS;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                List<AuditeeResponseEvidenceModel> EVIDENCE_LIST = new List<AuditeeResponseEvidenceModel>();
                EVIDENCE_LIST = await this.GetAttachedCAUEvidencesFromDirectory(COM_ID);
                int index = 1;
                if (EVIDENCE_LIST != null)
                    {
                    if (EVIDENCE_LIST.Count > 0)
                        {
                        foreach (var item in EVIDENCE_LIST)
                            {
                            string fileName = item.FILE_NAME;
                            cmd.CommandText = "pkg_ae.P_SubmitPostAuditCompliance_Evidence_By_BRANCH";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("TEXT_ID", OracleDbType.Int32).Value = TEXT_ID;
                            cmd.Parameters.Add("filename", OracleDbType.Varchar2).Value = fileName;
                            cmd.Parameters.Add("len_id", OracleDbType.Int32).Value = item.LENGTH;
                            cmd.Parameters.Add("enter_by", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                            cmd.Parameters.Add("filetype", OracleDbType.Varchar2).Value = item.IMAGE_TYPE;
                            cmd.Parameters.Add("filedata", OracleDbType.Clob).Value = item.IMAGE_DATA;
                            cmd.Parameters.Add("seq_id", OracleDbType.Int32).Value = (index);
                            cmd.ExecuteReader();
                            index++;


                            }
                        }
                    }

                this.DeleteSubFolderDirectoryInCAUEvidenceFromServer(COM_ID);
                }
            con.Dispose();
            return resp;
            }

        public List<CAUParaForComplianceModel> GetCAUParasForPostComplianceForReview()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<CAUParaForComplianceModel> list = new List<CAUParaForComplianceModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ae.P_GetParasForComplianceByCAU_FOR_REVIEW";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    CAUParaForComplianceModel chk = new CAUParaForComplianceModel();
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.PARA_NO = rdr["para_no"].ToString();
                    chk.NEW_PARA_ID = rdr["new_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["new_para_id"].ToString());
                    chk.OLD_PARA_ID = rdr["old_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["old_para_id"].ToString());
                    chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                    chk.INDICATOR = rdr["ind"].ToString();
                    chk.CAU_STATUS = rdr["cau_status"].ToString();
                    chk.CAU_ASSIGNED_ENT_ID = rdr["cau_assigned_ent_id"].ToString();
                    chk.COM_ID = rdr["com_id"].ToString();


                    list.Add(chk);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeResponseEvidenceModel> GetCAUAllComplianceEvidence(string textId)
            {
            var list = new List<AuditeeResponseEvidenceModel>();

            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_ae.P_GetAllCompliance_Evidence_CAU";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("TEXT_ID", OracleDbType.Varchar2).Value = textId;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                var usr = new AuditeeResponseEvidenceModel
                                    {
                                    IMAGE_NAME = rdr["FILE_NAME"].ToString(),
                                    IMAGE_DATA = "",
                                    SEQUENCE = Convert.ToInt32(rdr["SEQUENCE"]),
                                    LENGTH = Convert.ToInt32(rdr["LENGTH"]),
                                    FILE_ID = (rdr["id"].ToString())
                                    };


                                list.Add(usr);
                                }
                            }
                        }
                    }
                }
            catch (Exception)
                {

                throw;
                }

            return list;
            }
        }
    }
