using AIS.Models;
using AIS.Models.SM;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public List<ListOfReportsModel> GetIidListOfReports(int engId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<ListOfReportsModel>();
                }

            using var con = this.DatabaseConnection();
            var list = new List<ListOfReportsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ISM.T_AU_EXCEPTION_REPORT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    var report = new ListOfReportsModel
                        {
                        REPORT_ID = Convert.ToInt32(rdr["R_ID"].ToString()),
                        REPORT_TITLE = rdr["REPORT_TITLE"]?.ToString(),
                        DISCRIPTION = rdr["DISCRIPTION"]?.ToString(),
                        LOAN_STATUS = rdr["LOAN_STATUS"]?.ToString(),
                        REPORT_INDICATOR = rdr["IND"]?.ToString(),
                        ReportingPeriod = rdr["REPORTING_PERIOD"] == DBNull.Value
                            ? null
                            : rdr["REPORTING_PERIOD"]?.ToString()
                        };

                    object excObj;
                    try
                        {
                        excObj = rdr["EXCEPTION_COUNT"];
                        }
                    catch (IndexOutOfRangeException)
                        {
                        excObj = rdr["EXC_COUNT"];
                        }

                    report.ExceptionCount = excObj == DBNull.Value ? (int?)null : Convert.ToInt32(excObj);
                    list.Add(report);
                    }
                }

            return list;
            }

        public string AddIidExceptionAccountReport(string indicator, int reportId, string reportTitle, string description, string type, int loanStatusId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return string.Empty;
                }

            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "pkg_ISM.P_ADD_NEW_EXP_REPORT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Clear();
            cmd.Parameters.Add("P_IND", OracleDbType.Varchar2).Value = indicator ?? string.Empty;
            cmd.Parameters.Add("P_REPORT_ID", OracleDbType.Int32).Value = reportId;
            cmd.Parameters.Add("P_REPORT_TITLE", OracleDbType.Varchar2).Value = reportTitle ?? string.Empty;
            cmd.Parameters.Add("P_DESCRIPTION", OracleDbType.Varchar2).Value = description ?? string.Empty;
            cmd.Parameters.Add("P_R_TYPE", OracleDbType.Varchar2).Value = type ?? string.Empty;
            cmd.Parameters.Add("P_L_STATUS", OracleDbType.Int32).Value = loanStatusId;
            cmd.Parameters.Add("P_P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
            cmd.Parameters.Add("P_R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
            cmd.Parameters.Add("P_ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader["REMARKS"]?.ToString() ?? string.Empty : string.Empty;
            }

        public List<ExceptionReportFormatModel> GetIidExceptionReportFormat(long reportId)
            {
            var list = new List<ExceptionReportFormatModel>();

            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "pkg_ISM.P_GET_EXCEPTION_REPORT_FORMAT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_R_ID", OracleDbType.Int64).Value = reportId;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                list.Add(new ExceptionReportFormatModel
                    {
                    FormatId = reader["FORMAT_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["FORMAT_ID"]),
                    ReportId = reader["R_ID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["R_ID"]),
                    ColumnName = reader["COLUMN_NAME"]?.ToString(),
                    ColumnHeader = reader["COLUMN_HEADER"]?.ToString(),
                    DataType = reader["DATA_TYPE"]?.ToString(),
                    ColumnOrder = reader["COLUMN_ORDER"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COLUMN_ORDER"]),
                    IsActive = reader["IS_ACTIVE"]?.ToString()
                    });
                }

            return list;
            }

        public string InsertIidExceptionReportFormat(ExceptionReportFormatModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return string.Empty;
                }

            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "pkg_ISM.P_INSERT_EXCEPTION_REPORT_FORMAT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_R_ID", OracleDbType.Int64).Value = model.ReportId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_COLUMN_NAME", OracleDbType.Varchar2).Value = model.ColumnName ?? string.Empty;
            cmd.Parameters.Add("P_COLUMN_HEADER", OracleDbType.Varchar2).Value = model.ColumnHeader ?? string.Empty;
            cmd.Parameters.Add("P_COLUMN_ORDER", OracleDbType.Int32).Value = model.ColumnOrder ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_DATA_TYPE", OracleDbType.Varchar2).Value = model.DataType ?? string.Empty;
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = model.IsActive ?? "Y";
            cmd.Parameters.Add("P_P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
            cmd.Parameters.Add("O_FORMAT_ID", OracleDbType.Int64).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            var outputValue = cmd.Parameters["O_FORMAT_ID"].Value;
            long newId = 0;
            if (outputValue is Oracle.ManagedDataAccess.Types.OracleDecimal oracleDecimal)
                {
                newId = oracleDecimal.IsNull ? 0 : oracleDecimal.ToInt64();
                }
            else if (outputValue != null && outputValue != DBNull.Value)
                {
                newId = Convert.ToInt64(outputValue);
                }

            return newId.ToString();
            }

        public string UpdateIidExceptionReportFormat(ExceptionReportFormatModel model)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return string.Empty;
                }

            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "pkg_ISM.P_UPDATE_EXCEPTION_REPORT_FORMAT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_FORMAT_ID", OracleDbType.Int64).Value = model.FormatId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_COLUMN_HEADER", OracleDbType.Varchar2).Value = model.ColumnHeader ?? string.Empty;
            cmd.Parameters.Add("P_COLUMN_ORDER", OracleDbType.Int32).Value = model.ColumnOrder ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_DATA_TYPE", OracleDbType.Varchar2).Value = model.DataType ?? string.Empty;
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2).Value = model.IsActive ?? "Y";
            cmd.Parameters.Add("P_P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
            cmd.ExecuteNonQuery();

            return "success";
            }

        public DynamicReportResult GetIidExceptionReportData(long reportId, long engId)
            {
            var result = new DynamicReportResult();

            using var con = this.DatabaseConnection();
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ISM.P_GET_EXCEPTION_REPORT_DATA";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("P_R_ID", OracleDbType.Int64).Value = reportId;
                cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int64).Value = engId;
                cmd.Parameters.Add("IO_CURSOR1", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("IO_CURSOR2", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                    {
                    result.Columns.Add(new ExceptionReportFormatModel
                        {
                        FormatId = dr["FORMAT_ID"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["FORMAT_ID"]),
                        ReportId = dr["R_ID"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["R_ID"]),
                        ColumnName = dr["COLUMN_NAME"]?.ToString(),
                        ColumnHeader = dr["COLUMN_HEADER"]?.ToString(),
                        ColumnOrder = dr["COLUMN_ORDER"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["COLUMN_ORDER"]),
                        DataType = dr["DATA_TYPE"]?.ToString(),
                        IsActive = dr["IS_ACTIVE"]?.ToString()
                        });
                    }

                dr.NextResult();

                while (dr.Read())
                    {
                    var row = new Dictionary<string, object>();
                    for (var i = 0; i < dr.FieldCount; i++)
                        {
                        var colName = dr.GetName(i).ToUpperInvariant();
                        row[colName] = dr.IsDBNull(i) ? null : dr.GetValue(i);
                        }
                    result.Rows.Add(row);
                    }
                }

            return result;
            }

        public List<AccountTransactionSampleModel> GetIidAccountTransactions(int engId, string accountNo)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<AccountTransactionSampleModel>();
                }

            using var con = this.DatabaseConnection();
            var responseList = new List<AccountTransactionSampleModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ISM.p_get_account_transcations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("AC_number", OracleDbType.Varchar2).Value = accountNo ?? string.Empty;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    responseList.Add(new AccountTransactionSampleModel
                        {
                        TransactionMasterCode = rdr["TRANSACTIONMASTERCODE"]?.ToString(),
                        Description = rdr["DESCRIPTION"]?.ToString(),
                        Remarks = rdr["REMARKS"]?.ToString(),
                        TransactionDate = rdr["TRANSACTIONDATE"]?.ToString(),
                        TransactionDateDisp = rdr["TRANSACTIONDATE_DISP"]?.ToString(),
                        AuthorizationDate = rdr["AUTHORIZATIONDATE"]?.ToString(),
                        AuthorizationDateDisp = rdr["AUTHORIZATIONDATE_DISP"]?.ToString(),
                        DrAmount = rdr["DRAMOUNT"]?.ToString(),
                        CrAmount = rdr["CRAMOUNT"]?.ToString(),
                        ToAccountId = rdr["TOACCOUNTID"]?.ToString(),
                        ToAccountTitle = rdr["TOACCOUNTTITLE"]?.ToString(),
                        ToAccountNo = rdr["TOACCOUNTNO"]?.ToString(),
                        ToAccBranchId = rdr["TO_ACC_BRANCHID"]?.ToString(),
                        InstrumentNo = rdr["INSTRUMENTNO"]?.ToString()
                        });
                    }
                }

            return responseList;
            }

        public List<AccountDocumentBiometSamplingModel> GetIidAccountDocuments(string accountNo)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<AccountDocumentBiometSamplingModel>();
                }

            using var con = this.DatabaseConnection();
            var responseList = new List<AccountDocumentBiometSamplingModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ISM.P_GET_ACCOUNT_DOC";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("AC_number", OracleDbType.Varchar2).Value = accountNo ?? string.Empty;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    responseList.Add(new AccountDocumentBiometSamplingModel
                        {
                        OldAccountNo = rdr["OLDACCOUNTNO"]?.ToString(),
                        PageNo = rdr["PAGENO"]?.ToString(),
                        Name = rdr["NAME"]?.ToString(),
                        DocImage = rdr["DOC_IMAGE"] as byte[],
                        DocRemarks = rdr["DOC_REMARKS"]?.ToString()
                        });
                    }
                }

            return responseList;
            }

        public List<LoanCaseSampleModel> GetIidLoanExceptions(string indicator, int statusId, int engId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<LoanCaseSampleModel>();
                }

            using var con = this.DatabaseConnection();
            var list = new List<LoanCaseSampleModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ISM.P_GET_LOANS_Exceptions";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("LStatus", OracleDbType.Int32).Value = statusId;
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    list.Add(new LoanCaseSampleModel
                        {
                        LOAN_DISB_ID = rdr["LOAN_DISB_ID"]?.ToString(),
                        TYPE = rdr["TYPE"]?.ToString(),
                        SCHEME = rdr["SCHEME"]?.ToString(),
                        L_PURPOSE = rdr["L_PURPOSE"]?.ToString(),
                        LC_NO = rdr["LC_NO"]?.ToString(),
                        CNIC = rdr["CNIC"]?.ToString(),
                        CUSTOMERNAME = rdr["CUSTOMERNAME"]?.ToString(),
                        APP_DATE_DISP = rdr["APP_DATE_DISP"]?.ToString(),
                        DISB_DATE_DISP = rdr["DISB_DATE_DISP"]?.ToString(),
                        DEV_AMOUNT = rdr["DEV_AMOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["DEV_AMOUNT"]),
                        OUTSTANDING = rdr["OUTSTANDING"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["OUTSTANDING"])
                        });
                    }
                }

            return list;
            }

        public List<LoanCaseSampleDocumentsModel> GetIidLoanDocuments(int engId, string loanDisbId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<LoanCaseSampleDocumentsModel>();
                }

            using var con = this.DatabaseConnection();
            var list = new List<LoanCaseSampleDocumentsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ISM.p_get_Loan_Documents";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Varchar2).Value = engId;
                cmd.Parameters.Add("L_DISB_ID", OracleDbType.Varchar2).Value = loanDisbId ?? string.Empty;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    list.Add(new LoanCaseSampleDocumentsModel
                        {
                        IMAGE_ID = rdr["IMAGEID"]?.ToString(),
                        BRANCH_CODE = rdr["BRANCHCODE"]?.ToString(),
                        LOAN_APP_ID = rdr["LOAN_APP_ID"]?.ToString(),
                        CNIC = rdr["CNIC"]?.ToString(),
                        CUSTOMER_NAME = rdr["CUSTOMERNAME"]?.ToString(),
                        LOAN_CASE_NO = rdr["LOAN_CASE_NO"]?.ToString(),
                        LOAN_DISB_ID = rdr["LOAN_DISB_ID"]?.ToString(),
                        DOC_NAME = rdr["DOCNAME"]?.ToString()
                        });
                    }
                }

            return list;
            }

        public List<LoanCaseSampleDocumentsModel> GetIidLoanDocumentData(int imageId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<LoanCaseSampleDocumentsModel>();
                }

            using var con = this.DatabaseConnection();
            var list = new List<LoanCaseSampleDocumentsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ISM.p_get_Loan_Documents_image";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("image_ID", OracleDbType.Varchar2).Value = imageId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    var document = new LoanCaseSampleDocumentsModel();
                    if (rdr["IMAGEDATA"] != DBNull.Value)
                        {
                        var imageBytes = (byte[])rdr["IMAGEDATA"];
                        document.IMAGE_DATA = Convert.ToBase64String(imageBytes);
                        }
                    else
                        {
                        document.IMAGE_DATA = string.Empty;
                        }

                    list.Add(document);
                    }
                }

            return list;
            }

        public List<LoanCaseSampleTransactionsModel> GetIidLoanTransactions(int engId, string loanDisbId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<LoanCaseSampleTransactionsModel>();
                }

            using var con = this.DatabaseConnection();
            var list = new List<LoanCaseSampleTransactionsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ISM.p_get_Loan_Transactions";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_ID", OracleDbType.Varchar2).Value = engId;
                cmd.Parameters.Add("L_DISB_ID", OracleDbType.Varchar2).Value = loanDisbId ?? string.Empty;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    list.Add(new LoanCaseSampleTransactionsModel
                        {
                        DESCRIPTION = rdr["DESCRIPTION"]?.ToString(),
                        MANUAL_VOUCHER_NO = rdr["MANUALVOUCHERNO"]?.ToString(),
                        TRANSACTION_DATE = rdr["TRANSACTIONDATE"]?.ToString(),
                        TRANSACTION_DATE_DISP = rdr["TRANSACTIONDATE_DISP"]?.ToString(),
                        DR_AMOUNT = rdr["DRAMOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["DRAMOUNT"]),
                        CR_AMOUNT = rdr["CRAMOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["CRAMOUNT"]),
                        LN_ACCOUNT_ID = rdr["LN_ACCOUNTID"]?.ToString(),
                        CREATED_ON = rdr["CREATEDON"]?.ToString(),
                        CREATED_ON_DISP = rdr["CREATEDON_DISP"]?.ToString(),
                        REMARKS = rdr["REMARKS"]?.ToString(),
                        REJECTION_DATE = rdr["REJECTIONDATE"]?.ToString(),
                        REJECTION_DATE_DISP = rdr["REJECTIONDATE_DISP"]?.ToString(),
                        REVERSAL_DATE = rdr["REVERSALDATE"]?.ToString(),
                        REVERSAL_DATE_DISP = rdr["REVERSALDATE_DISP"]?.ToString(),
                        WORKING_DATE = rdr["WORKINGDATE"]?.ToString(),
                        WORKING_DATE_DISP = rdr["WORKINGDATE_DISP"]?.ToString(),
                        AUTHORIZATION_DATE = rdr["AUTHORIZATIONDATE"]?.ToString(),
                        AUTHORIZATION_DATE_DISP = rdr["AUTHORIZATIONDATE_DISP"]?.ToString(),
                        MCO_RECEIPT_NO = rdr["MCO_RECEIPT_NO"]?.ToString(),
                        MCO_BOOK_NO = rdr["MCO_BOOK_NO"]?.ToString()
                        });
                    }
                }

            return list;
            }
        }
    }

