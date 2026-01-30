using AIS.Models;
using AIS.Models.SM;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {
        public string CreateSampleDataAfterEngagementApproval(int ENG_ID)
            {
            string resp = "";
            string email = "";
            string email_cc = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.P_add_sample_data";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    //email = rdr["email"].ToString();
                    // email_cc = rdr["email_cc"].ToString();
                    }
                }
            con.Dispose();

            if (resp == "N")
                {
                EmailNotification.NotifyAuditSampleIssue(_configuration, ENG_ID.ToString(), email, email_cc, _httpCon?.HttpContext?.RequestServices);
                }
            return resp;
            }

        public string CreateExceptionDataAfterEngagementApproval(int ENG_ID)
            {
            string resp = "";
            string email = "";
            string email_cc = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.P_add_exception_data";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    //email = rdr["email"].ToString();
                    // email_cc = rdr["email_cc"].ToString();
                    }
                }
            con.Dispose();

            if (resp == "N")
                {
                EmailNotification.NotifyAuditExceptionIssue(_configuration, ENG_ID.ToString(), email, email_cc, _httpCon?.HttpContext?.RequestServices);
                }
            return resp;
            }

        public List<BiometSamplingModel> GetBiometSamplingDetails(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
           

            List<BiometSamplingModel> responseList = new List<BiometSamplingModel>();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.p_get_Account";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        BiometSamplingModel record = new BiometSamplingModel()
                            {
                            BRANCH_CODE = rdr["branchcode"].ToString(),
                            ACCOUNT_NO = rdr["oldaccountno"].ToString(),
                            ACCOUNT_TITLE = rdr["name"].ToString(),
                            CUSTOMER_NAME = rdr["customername"].ToString(),
                            DOB = rdr["dob"].ToString(),
                            DOB_DISP = rdr["dob_disp"].ToString(),
                            PHONE_CELL = rdr["phonecell"].ToString(),
                            CNIC = rdr["cnic"].ToString(),
                            CNIC_EXPIRY_DATE = rdr["cnicexpirydate"].ToString(),
                            CNIC_EXPIRY_DATE_DISP = rdr["cnicexpirydate_disp"].ToString(),
                            OPENING_DATE = rdr["openingdate"].ToString(),
                            OPENING_DATE_DISP = rdr["openingdate_disp"].ToString(),
                            BMVS_VERIFIED = rdr["bmvs_verified"].ToString(),
                            PURPOSE = rdr["purpose"].ToString(),
                            ACCOUNT_TYPE = rdr["acc_type"].ToString(),
                            ACCOUNT_CATEGORY = rdr["acc_category"].ToString(),
                            RISK = rdr["risk"].ToString()
                            };

                        responseList.Add(record);
                        }
                    }
                }
            con.Dispose();
            return responseList;
            }

        public List<AccountTransactionSampleModel> GetBiometAccountTransactionSamplingDetails(int ENG_ID, string AC_NO)
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
           

            List<AccountTransactionSampleModel> responseList = new List<AccountTransactionSampleModel>();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.p_get_account_transcations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("AC_number", OracleDbType.Varchar2).Value = AC_NO;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        AccountTransactionSampleModel record = new AccountTransactionSampleModel()
                            {
                            TransactionMasterCode = rdr["transactionmastercode"].ToString(),
                            Description = rdr["description"].ToString(),
                            Remarks = rdr["REMARKS"].ToString(),
                            TransactionDate = rdr["transactiondate"].ToString(),
                            TransactionDateDisp = rdr["transactiondate_disp"].ToString(),
                            AuthorizationDate = rdr["authorizationdate"].ToString(),
                            AuthorizationDateDisp = rdr["authorizationdate_disp"].ToString(),
                            DrAmount = rdr["dramount"].ToString(),
                            CrAmount = rdr["cramount"].ToString(),
                            ToAccountId = rdr["toaccountid"].ToString(),
                            ToAccountTitle = rdr["toaccounttitle"].ToString(),
                            ToAccountNo = rdr["toaccountno"].ToString(),
                            ToAccBranchId = rdr["to_acc_branchid"].ToString(),
                            InstrumentNo = rdr["instrumentno"].ToString()
                            };
                        responseList.Add(record);
                        }
                    }
                }
            con.Dispose();
            return responseList;
            }

        public List<AccountDocumentBiometSamplingModel> GetBiometAccountDocumentsSamplingDetails(string AC_NO)
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
           

            List<AccountDocumentBiometSamplingModel> responseList = new List<AccountDocumentBiometSamplingModel>();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.P_GET_ACCOUNT_DOC ";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("AC_number", OracleDbType.Varchar2).Value = AC_NO;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        AccountDocumentBiometSamplingModel record = new AccountDocumentBiometSamplingModel()
                            {
                            OldAccountNo = rdr["OLDACCOUNTNO"].ToString(),
                            PageNo = rdr["PAGENO"].ToString(),
                            Name = rdr["NAME"].ToString(),
                            DocImage = rdr["DOC_IMAGE"] as byte[], // Assuming DOC_IMAGE is a BLOB in the database
                            DocRemarks = rdr["DOC_REMARKS"].ToString()
                            };
                        responseList.Add(record);
                        }
                    }
                }
            con.Dispose();
            return responseList;
            }

        public DynamicReportResult GetExceptionReportData(long reportId, long engId)
            {
            var result = new DynamicReportResult();
            System.Diagnostics.Debug.WriteLine(result.Columns.GetType());

            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_SM.P_GET_EXCEPTION_REPORT_DATA";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_R_ID", OracleDbType.Int64).Value = reportId;
                    cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int64).Value = engId;
                    cmd.Parameters.Add("IO_CURSOR1", OracleDbType.RefCursor).Direction = ParameterDirection.Output; // Format
                    cmd.Parameters.Add("IO_CURSOR2", OracleDbType.RefCursor).Direction = ParameterDirection.Output; // Data

                    using (var dr = cmd.ExecuteReader())
                        {
                        // -------------------------------
                        // CURSOR 1  Report Format
                        // -------------------------------
                        while (dr.Read())
                            {
                            result.Columns.Add(new ExceptionReportFormatModel
                                {
                                FormatId = dr.GetInt64(dr.GetOrdinal("FORMAT_ID")),
                                ReportId = dr.GetInt64(dr.GetOrdinal("R_ID")),
                                ColumnName = dr["COLUMN_NAME"].ToString(),
                                ColumnHeader = dr["COLUMN_HEADER"].ToString(),
                                ColumnOrder = Convert.ToInt32(dr["COLUMN_ORDER"]),
                                DataType = dr["DATA_TYPE"].ToString(),
                                IsActive = dr["IS_ACTIVE"].ToString()
                                });
                            }

                        // Move to cursor 2
                        dr.NextResult();

                        // -------------------------------
                        // CURSOR 2  Report Data
                        // -------------------------------
                        while (dr.Read())
                            {
                            var row = new Dictionary<string, object>();

                            for (int i = 0; i < dr.FieldCount; i++)
                                {
                                var colName = dr.GetName(i).ToUpper();  // Normalize
                                var val = dr.IsDBNull(i) ? null : dr.GetValue(i);
                                row[colName] = val;
                                }

                            result.Rows.Add(row);
                            }
                        }
                    }
                }

            return result;
            }


        public List<ListOfSamplesModel> GetListOfSamples(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ListOfSamplesModel> list = new List<ListOfSamplesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.P_GET_SAMPLE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;

                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ListOfSamplesModel chk = new ListOfSamplesModel();
                    chk.SAMPLE_ID = Convert.ToInt32(rdr["S_ID"].ToString());
                    chk.SAMPLE_TYPE = rdr["SAMPLE_TYPE"].ToString();
                    chk.SAMPLE_PERCENTAGE = rdr["SAMPLE_PERCENTAGE"].ToString();
                    chk.TOTAL_COUNT = rdr["samp_tot"].ToString();
                    chk.SAMPLE_COUNT = rdr["sample_final"].ToString();
                    chk.LOAN_STATUS = rdr["sample_final"].ToString();
                    chk.SAMPLE_INDICATOR = rdr["IND"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<LoanCaseSampleModel> GetLoanSamples(string INDICATOR, int STATUS_ID, int ENG_ID, int SAMPLE_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<LoanCaseSampleModel> list = new List<LoanCaseSampleModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.P_GET_LOANS_SAMPLE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                // cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("S_ID", OracleDbType.Varchar2).Value = SAMPLE_ID;
                cmd.Parameters.Add("LStatus", OracleDbType.Int32).Value = STATUS_ID;
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    LoanCaseSampleModel chk = new LoanCaseSampleModel();
                    chk.LOAN_DISB_ID = rdr["loan_disb_id"].ToString();
                    chk.TYPE = rdr["TYPE"].ToString();
                    chk.SCHEME = rdr["SCHEME"].ToString();
                    chk.L_PURPOSE = rdr["L_PURPOSE"].ToString();
                    chk.LC_NO = rdr["LC_NO"].ToString();
                    chk.CNIC = rdr["CNIC"].ToString();
                    chk.CUSTOMERNAME = rdr["CUSTOMERNAME"].ToString();
                    chk.APP_DATE_DISP = rdr["APP_DATE_DISP"].ToString();
                    chk.DISB_DATE_DISP = rdr["DISB_DATE_DISP"].ToString();
                    chk.DEV_AMOUNT = Convert.ToDecimal(rdr["DEV_AMOUNT"]);
                    chk.OUTSTANDING = Convert.ToDecimal(rdr["OUTSTANDING"]);
                    list.Add(chk);
                    }

                }
            con.Dispose();
            return list;
            }

        public List<LoanCaseSampleModel> GetLoanExceptions(string INDICATOR, int STATUS_ID, int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<LoanCaseSampleModel> list = new List<LoanCaseSampleModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.P_GET_LOANS_Exceptions";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                // cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("LStatus", OracleDbType.Int32).Value = STATUS_ID;
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    LoanCaseSampleModel chk = new LoanCaseSampleModel();
                    chk.LOAN_DISB_ID = rdr["loan_disb_id"].ToString();
                    chk.TYPE = rdr["TYPE"].ToString();
                    chk.SCHEME = rdr["SCHEME"].ToString();
                    chk.L_PURPOSE = rdr["L_PURPOSE"].ToString();
                    chk.LC_NO = rdr["LC_NO"].ToString();
                    chk.CNIC = rdr["CNIC"].ToString();
                    chk.CUSTOMERNAME = rdr["CUSTOMERNAME"].ToString();
                    chk.APP_DATE_DISP = rdr["APP_DATE_DISP"].ToString();
                    chk.DISB_DATE_DISP = rdr["DISB_DATE_DISP"].ToString();
                    chk.DEV_AMOUNT = Convert.ToDecimal(rdr["DEV_AMOUNT"]);
                    chk.OUTSTANDING = Convert.ToDecimal(rdr["OUTSTANDING"]);
                    list.Add(chk);
                    }

                }
            con.Dispose();
            return list;
            }

        public List<LoanCaseSampleDocumentsModel> GetLoanSamplesDocuments(int ENG_ID, string LOAN_DISB_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<LoanCaseSampleDocumentsModel> list = new List<LoanCaseSampleDocumentsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.p_get_Loan_Documents";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                // cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("E_ID", OracleDbType.Varchar2).Value = ENG_ID;
                cmd.Parameters.Add("L_DISB_ID", OracleDbType.Varchar2).Value = LOAN_DISB_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    LoanCaseSampleDocumentsModel chk = new LoanCaseSampleDocumentsModel();
                    chk.IMAGE_ID = rdr["IMAGEID"].ToString();
                    chk.BRANCH_CODE = rdr["branchcode"].ToString();
                    chk.LOAN_APP_ID = rdr["loan_app_id"].ToString();
                    chk.CNIC = rdr["cnic"].ToString();
                    chk.CUSTOMER_NAME = rdr["customername"].ToString();
                    chk.LOAN_CASE_NO = rdr["loan_case_no"].ToString();
                    chk.LOAN_DISB_ID = rdr["loan_disb_id"].ToString();
                    chk.DOC_NAME = rdr["docname"].ToString();
                    list.Add(chk);

                    }

                }
            con.Dispose();
            return list;
            }

        public List<LoanCaseSampleDocumentsModel> GetLoanSamplesDocumentData(int IMAGE_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<LoanCaseSampleDocumentsModel> list = new List<LoanCaseSampleDocumentsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.p_get_Loan_Documents_image";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                // cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("image_ID", OracleDbType.Varchar2).Value = IMAGE_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    LoanCaseSampleDocumentsModel chk = new LoanCaseSampleDocumentsModel();

                    if (rdr["imagedata"] != DBNull.Value)
                        {
                        byte[] imageBytes = (byte[])rdr["imagedata"];
                        chk.IMAGE_DATA = Convert.ToBase64String(imageBytes);
                        }
                    else
                        {
                        chk.IMAGE_DATA = string.Empty;
                        }

                    list.Add(chk);
                    }


                }
            con.Dispose();
            return list;
            }

        public List<LoanCaseSampleTransactionsModel> GetLoanSamplesTransactions(int ENG_ID, string LOAN_DISB_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<LoanCaseSampleTransactionsModel> list = new List<LoanCaseSampleTransactionsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.p_get_Loan_Transactions";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Varchar2).Value = ENG_ID;
                cmd.Parameters.Add("L_DISB_ID", OracleDbType.Varchar2).Value = LOAN_DISB_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                        LoanCaseSampleTransactionsModel chk = new LoanCaseSampleTransactionsModel
                            {
                            DESCRIPTION = rdr["description"]?.ToString(),
                            MANUAL_VOUCHER_NO = rdr["manualvoucherno"]?.ToString(),
                            TRANSACTION_DATE = rdr["transactiondate"]?.ToString(),
                            TRANSACTION_DATE_DISP = rdr["transactiondate_disp"]?.ToString(),
                            DR_AMOUNT = rdr["dramount"] != DBNull.Value ? Convert.ToDecimal(rdr["dramount"]) : 0,
                            CR_AMOUNT = rdr["cramount"] != DBNull.Value ? Convert.ToDecimal(rdr["cramount"]) : 0,
                            LN_ACCOUNT_ID = rdr["ln_accountid"]?.ToString(),
                            CREATED_ON = rdr["createdon"]?.ToString(),
                            CREATED_ON_DISP = rdr["createdon_disp"]?.ToString(),
                            REMARKS = rdr["remarks"]?.ToString(),
                            REJECTION_DATE = rdr["rejectiondate"]?.ToString(),
                            REJECTION_DATE_DISP = rdr["rejectiondate_disp"]?.ToString(),
                            REVERSAL_DATE = rdr["reversaldate"]?.ToString(),
                            REVERSAL_DATE_DISP = rdr["reversaldate_disp"]?.ToString(),
                            WORKING_DATE = rdr["workingdate"]?.ToString(),
                            WORKING_DATE_DISP = rdr["workingdate_disp"]?.ToString(),
                            AUTHORIZATION_DATE = rdr["authorizationdate"]?.ToString(),
                            AUTHORIZATION_DATE_DISP = rdr["authorizationdate_disp"]?.ToString(),
                            MCO_RECEIPT_NO = rdr["mco_receipt_no"]?.ToString(),
                            MCO_BOOK_NO = rdr["mco_book_no"]?.ToString()
                            };

                    list.Add(chk);
                    }


                }
            con.Dispose();
            return list;
            }

        public List<AuditeeEntitiesModel> GetSampleEntities()
            {

            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.P_GET_SAMPLE_ENTITIES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    if (rdr["eng_id"].ToString() != "" && rdr["eng_id"].ToString() != null)
                        entity.ENG_ID = Convert.ToInt32(rdr["eng_id"]);

                    if (rdr["E_NAME"].ToString() != "" && rdr["E_NAME"].ToString() != null)
                        entity.NAME = rdr["E_NAME"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public string RegenerateSampleofLoan(int ENG_ID, int LOAN_SAMPLE_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.P_add_sample_data_update";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Varchar2).Value = ENG_ID;
                cmd.Parameters.Add("SID", OracleDbType.Varchar2).Value = LOAN_SAMPLE_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
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

        public List<ExceptionMonitorEntityModel> GetExceptionMonitorEntities()
            {
            var con = this.DatabaseConnection();
            List<ExceptionMonitorEntityModel> entities = new List<ExceptionMonitorEntityModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_SM.P_GET_EXCEPTION_MONITOR_ENTITIES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        ExceptionMonitorEntityModel entity = new ExceptionMonitorEntityModel()
                            {
                            EngId = rdr["ENG_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ENG_ID"]),
                            EntId = rdr["ENT_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ENT_ID"]),
                            EntName = rdr["ENT_NAME"]?.ToString(),
                            TotalEng = rdr["TOTAL_ENG"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TOTAL_ENG"]),
                            EngWithExc = rdr["ENG_WITH_EXC"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ENG_WITH_EXC"]),
                            LastRunDateDisp = rdr["LAST_RUN_DATE_DISP"]?.ToString()
                            };

                        entities.Add(entity);
                        }
                    }
                }

            con.Dispose();
            return entities;
            }

        public List<ExceptionMonitorModel> GetExceptionMonitorDetails(int engId)
            {
            var con = this.DatabaseConnection();
            List<ExceptionMonitorModel> exceptionMonitors = new List<ExceptionMonitorModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_SM.P_GET_EXCEPTION_MONITOR_DETAILS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        ExceptionMonitorModel detail = new ExceptionMonitorModel()
                            {
                            ErId = rdr["ER_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ER_ID"]),
                            EngId = rdr["ENG_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ENG_ID"]),
                            EntId = rdr["ENT_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ENT_ID"]),
                            ExecutionDates = rdr["ExecutionDates"]?.ToString(),
                            ReportingPeriod = rdr["ReportingPeriod"]?.ToString(),
                            ReportTitle = rdr["REPORT_TITLE"]?.ToString(),
                            ReportType = rdr["Report_Type"]?.ToString(),
                            ExceptionCount = rdr["EXC_COUNT"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["EXC_COUNT"]),
                            };

                        exceptionMonitors.Add(detail);
                        }
                    }
                }

            con.Dispose();
            return exceptionMonitors;
            }

        public string RegenerateException(int engId, int erId)
            {
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_SM.P_REGENERATE_EXCEPTION";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("P_ER_ID", OracleDbType.Int32).Value = erId;

                cmd.ExecuteNonQuery();
                }

            con.Dispose();
            return "success";
            }

        public List<CDMSMasterTransactionModel> GetCDMSMasterTransactions(string ENTITY_ID, DateTime START_DATE, DateTime END_DATE, string CNIC_NO, string ACC_NO)
            {

            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<CDMSMasterTransactionModel> list = new List<CDMSMasterTransactionModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.p_get_account_transcations_master";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("AC_number", OracleDbType.Varchar2).Value = ACC_NO;
                cmd.Parameters.Add("CNIC_NO", OracleDbType.Varchar2).Value = CNIC_NO;
                cmd.Parameters.Add("ST_DATE", OracleDbType.Date).Value = START_DATE;
                cmd.Parameters.Add("ED_DATE", OracleDbType.Date).Value = END_DATE;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    CDMSMasterTransactionModel m = new CDMSMasterTransactionModel
                        {
                        TRANSACTION_ID = rdr["transactionid"]?.ToString(),
                        ENTITY_NAME = rdr["b_name"]?.ToString(),
                        OLD_ACCOUNT_NO = rdr["oldaccountno"]?.ToString(),
                        CNIC = rdr["cnic"]?.ToString(),
                        ACCOUNT_NAME = rdr["title"]?.ToString(),
                        CUSTOMER_NAME = rdr["customername"]?.ToString(),
                        TR_MASTER_CODE = rdr["transactionmastercode"]?.ToString(),
                        DESCRIPTION = rdr["description"]?.ToString(),
                        REMARKS = rdr["remarks"]?.ToString(),
                        TRANSACTION_DATE = rdr["transactiondate"]?.ToString(),
                        AUTHORIZATION_DATE = rdr["authorizationdate"]?.ToString(),
                        DR_AMOUNT = rdr["dramount"]?.ToString(),
                        CR_AMOUNT = rdr["cramount"]?.ToString(),
                        TO_ACCOUNT_ID = rdr["toaccountid"]?.ToString(),
                        TO_ACCOUNT_TITLE = rdr["toaccounttitle"]?.ToString(),
                        TO_ACCOUNT_NO = rdr["toaccountno"]?.ToString(),
                        TO_ACC_BRANCH_ID = rdr["to_acc_branchid"]?.ToString(),
                        INSTRUMENT_NO = rdr["instrumentno"]?.ToString()
                        };
                    list.Add(m);
                    }
                }
            con.Dispose();
            return list;

            }

        public List<ListOfReportsModel> GetListOfreports(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ListOfReportsModel> list = new List<ListOfReportsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.T_AU_EXCEPTION_REPORT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ListOfReportsModel chk = new ListOfReportsModel();
                    chk.REPORT_ID = Convert.ToInt32(rdr["R_ID"].ToString());
                    chk.REPORT_TITLE = rdr["REPORT_TITLE"].ToString();
                    chk.DISCRIPTION = rdr["DISCRIPTION"].ToString();
                    chk.LOAN_STATUS = rdr["loan_status"].ToString();
                    chk.REPORT_INDICATOR = rdr["IND"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AddExceptionAccountReport(string IND, int REPORT_ID, string REPORT_TITLE, string DESCRIPTION, string TYPE, int LOAN_STATUS_ID)

            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_sm.P_Add_new_exp_report";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = IND;
                cmd.Parameters.Add("REPORT_ID", OracleDbType.Int32).Value = REPORT_ID;
                cmd.Parameters.Add("REPORT_TITLE", OracleDbType.Varchar2).Value = REPORT_TITLE;
                cmd.Parameters.Add("DESCRIPTION", OracleDbType.Varchar2).Value = DESCRIPTION;
                cmd.Parameters.Add("R_TYPE", OracleDbType.Varchar2).Value = TYPE;
                cmd.Parameters.Add("L_Status", OracleDbType.Int32).Value = LOAN_STATUS_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
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

        public List<ExceptionReportFormatModel> GetExceptionReportFormat(long reportId)
            {
            var list = new List<ExceptionReportFormatModel>();

            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_SM.P_GET_EXCEPTION_REPORT_FORMAT";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("P_R_ID", OracleDbType.Int64).Value = reportId;
                    cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var dr = cmd.ExecuteReader())
                        {
                        while (dr.Read())
                            {
                            list.Add(new ExceptionReportFormatModel
                                {
                                FormatId = dr["FORMAT_ID"] == DBNull.Value ? 0 : Convert.ToInt64(dr["FORMAT_ID"]),
                                ReportId = dr["R_ID"] == DBNull.Value ? 0 : Convert.ToInt64(dr["R_ID"]),
                                ColumnName = dr["COLUMN_NAME"].ToString(),
                                ColumnHeader = dr["COLUMN_HEADER"].ToString(),
                                DataType = dr["DATA_TYPE"].ToString(),
                                ColumnOrder = dr["COLUMN_ORDER"] == DBNull.Value ? 0 : Convert.ToInt32(dr["COLUMN_ORDER"]),
                                IsActive = dr["IS_ACTIVE"].ToString()
                                });
                            }
                        }
                    }
                }

            return list;
            }

        public string InsertExceptionReportFormat(ExceptionReportFormatModel model)
            {
            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_SM.P_INSERT_EXCEPTION_REPORT_FORMAT";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_R_ID", OracleDbType.Int64).Value = model.ReportId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("P_COLUMN_NAME", OracleDbType.Varchar2).Value = model.ColumnName;
                    cmd.Parameters.Add("P_COLUMN_HEADER", OracleDbType.Varchar2).Value = model.ColumnHeader;
                    cmd.Parameters.Add("P_COLUMN_ORDER", OracleDbType.Int32).Value = model.ColumnOrder ?? (object)DBNull.Value;
                    cmd.Parameters.Add("P_DATA_TYPE", OracleDbType.Varchar2).Value = model.DataType;
                    cmd.Parameters.Add("O_FORMAT_ID", OracleDbType.Int64).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    var outputValue = cmd.Parameters["O_FORMAT_ID"].Value;
                    long newId = 0;

                    if (outputValue is OracleDecimal oracleDecimal)
                        {
                        newId = oracleDecimal.IsNull ? 0 : oracleDecimal.ToInt64();
                        }
                    else if (outputValue != null && outputValue != DBNull.Value)
                        {
                        newId = Convert.ToInt64(outputValue);
                        }

                    return newId.ToString();
                    }
                }
            }

        public string UpdateExceptionReportFormat(ExceptionReportFormatModel model)
            {
            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_sm.P_UPDATE_EXCEPTION_REPORT_FORMAT";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_FORMAT_ID", OracleDbType.Int64).Value = model.FormatId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("P_COLUMN_HEADER", OracleDbType.Varchar2).Value = model.ColumnHeader;
                    cmd.Parameters.Add("P_COLUMN_ORDER", OracleDbType.Int32).Value = model.ColumnOrder ?? (object)DBNull.Value;
                    cmd.Parameters.Add("P_DATA_TYPE", OracleDbType.Varchar2).Value = model.DataType;
                    cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = model.IsActive;

                    cmd.ExecuteNonQuery();
                    }
                }
            return "success";
            }
        }
    }
