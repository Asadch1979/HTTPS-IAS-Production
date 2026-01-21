using AIS.Exceptions;
using AIS.Models;
using AIS.Security.Cryptography;
using AIS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {
        private readonly SessionHandler sessionHandler;
        private readonly SecurityTokenService _tokenService;
        private readonly LoginAttemptTracker _loginAttemptTracker;
        private readonly LocalIPAddress iPAddress = new LocalIPAddress();
        private readonly DateTimeHandler dtime = new DateTimeHandler();
        private readonly CAUEncodeDecode encoderDecoder = new CAUEncodeDecode();
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpCon;
        private readonly IConfiguration _configuration;
        private readonly string _cauKey;
        private static readonly ConcurrentDictionary<string, int> SessionTokenIndex = new ConcurrentDictionary<string, int>();
        private static readonly ConcurrentDictionary<int, string> UserSessionIndex = new ConcurrentDictionary<int, string>();

        [Obsolete]
        private readonly IHostingEnvironment _env;

        [Obsolete]
        public DBConnection(IHttpContextAccessor httpContextAccessor, IHostingEnvironment env, IConfiguration configuration, SessionHandler sessionHandler, SecurityTokenService tokenService, LoginAttemptTracker loginAttemptTracker)
              : this(httpContextAccessor, configuration, sessionHandler, tokenService, loginAttemptTracker)
            {
            _env = env;
            }

        public DBConnection(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, SessionHandler sessionHandler, SecurityTokenService tokenService, LoginAttemptTracker loginAttemptTracker)
            {
            if (httpContextAccessor == null)
                throw new ArgumentNullException(nameof(httpContextAccessor));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (sessionHandler == null)
                throw new ArgumentNullException(nameof(sessionHandler));
            if (tokenService == null)
                throw new ArgumentNullException(nameof(tokenService));

            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available.");
            var session = httpContext.Session ?? throw new InvalidOperationException("Session has not been configured for the current context.");

            _session = session;
            _httpCon = httpContextAccessor;
            _configuration = configuration;
            this.sessionHandler = sessionHandler;
            _tokenService = tokenService;
            _loginAttemptTracker = loginAttemptTracker;
            _cauKey = ResolveCauKey();
            }

        private DBConnection(ISession session, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, SessionHandler sessionHandler, SecurityTokenService tokenService, LoginAttemptTracker loginAttemptTracker)
            {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _httpCon = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _loginAttemptTracker = loginAttemptTracker;
            _cauKey = ResolveCauKey();
            }

        private string ResolveCauKey()
            {
            var cauKey = _configuration["Security:CauKey"];
            if (string.IsNullOrWhiteSpace(cauKey))
                {
                throw new InvalidOperationException("Security:CauKey configuration is required.");
                }

            return cauKey;
            }

        public static DBConnection CreateFromHttpContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, SessionHandler sessionHandler, SecurityTokenService tokenService, LoginAttemptTracker loginAttemptTracker = null)
            {
            if (httpContextAccessor == null)
                throw new ArgumentNullException(nameof(httpContextAccessor));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (sessionHandler == null)
                throw new ArgumentNullException(nameof(sessionHandler));
            if (tokenService == null)
                throw new ArgumentNullException(nameof(tokenService));

            var context = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available.");
            var session = context.Session ?? throw new InvalidOperationException("Session has not been configured for the current context.");

            return new DBConnection(session, httpContextAccessor, configuration, sessionHandler, tokenService, loginAttemptTracker);
            }

        protected SessionHandler CreateSessionHandler()
            {
            return sessionHandler;
            }

        protected DBConnection CreateChildDbConnection()
            {
            if (_httpCon == null)
                throw new InvalidOperationException("HTTP context accessor has not been provided.");
            if (_configuration == null)
                throw new InvalidOperationException("Configuration has not been provided.");

            var session = _session ?? _httpCon.HttpContext?.Session;
            if (session == null)
                throw new InvalidOperationException("Session has not been provided to the database connection.");

            return new DBConnection(session, _httpCon, _configuration, sessionHandler, _tokenService, _loginAttemptTracker);
            }
        #region Database Connection
        private OracleConnection DatabaseConnection(bool requireActiveSession = true)
            {
            try
                {
                if (requireActiveSession)
                    {
                    CreateSessionHandler().GetUserOrThrow();
                    }

                OracleConnection con = new OracleConnection();
                OracleConnectionStringBuilder ocsb = new OracleConnectionStringBuilder();
                ocsb.Password = _configuration["ConnectionStrings:DBUserPassword"];
                ocsb.UserID = _configuration["ConnectionStrings:DBUserName"];
                ocsb.DataSource = _configuration["ConnectionStrings:DBDataSource"];
                ocsb.IncrPoolSize = 5;
                ocsb.MaxPoolSize = 5000;
                ocsb.MinPoolSize = 1;
                ocsb.Pooling = true;
                ocsb.ConnectionTimeout = 3540;
                con.ConnectionString = ocsb.ConnectionString;
                con.Open();
                return con;
                }
            catch (Exception ex)
                {
                throw new DatabaseUnavailableException("Unable to open a connection to the database.", ex);
                }
            }

        public bool TryValidateLoginConnection()
            {
            try
                {
                using (var connection = DatabaseConnection(requireActiveSession: false))
                    {
                    return connection?.State == ConnectionState.Open;
                    }
                }
            catch (DatabaseUnavailableException)
                {
                return false;
                }
            catch (Exception)
                {
                return false;
                }
            }
        #endregion

        protected void GuardAgainstDynamicSql(OracleCommand command)
            {
            if (command == null)
                {
                throw new ArgumentNullException(nameof(command));
                }

            if (command.CommandType == CommandType.Text)
                {
                throw new InvalidOperationException("Dynamic SQL commands are blocked; use stored procedures with bind variables instead.");
                }
            }

        private string DecryptPassword(string encryptedPassword)
            {
            byte[] bytes = Convert.FromBase64String(encryptedPassword);
            return Encoding.UTF8.GetString(bytes);
            }

        private string HashPassword(string plainTextPassword)
            {
            if (plainTextPassword == null)
                {
                throw new ArgumentNullException(nameof(plainTextPassword));
                }

            return _tokenService.ComputeHmacHash(plainTextPassword);
            }

        private string HashEncryptedPassword(string encryptedPassword)
            {
            var decrypted = DecryptPassword(encryptedPassword);
            return HashPassword(decrypted);
            }
        #region Session Handling
        public void CreateSession(string token, int ppno, string ip, string agent)
            {
            if (string.IsNullOrWhiteSpace(token) || ppno == 0)
                {
                return;
                }

            KillSessions(ppno);
            SessionTokenIndex[token] = ppno;
            UserSessionIndex[ppno] = token;
            }

        public void KillSessions(int ppno)
            {
            if (UserSessionIndex.TryRemove(ppno, out var existingToken))
                {
                SessionTokenIndex.TryRemove(existingToken, out _);
                ResetLoginAttemptsForPpNumber(ppno.ToString());
                }
            }

        public bool IsSessionValid(string token)
            {
            if (string.IsNullOrWhiteSpace(token))
                {
                return false;
                }

            return SessionTokenIndex.ContainsKey(token);
            }

        public void InvalidateSession(string token)
            {
            if (string.IsNullOrWhiteSpace(token))
                {
                return;
                }

            if (SessionTokenIndex.TryRemove(token, out var ppNumber))
                {
                UserSessionIndex.TryRemove(ppNumber, out _);
                ResetLoginAttemptsForPpNumber(ppNumber.ToString());
                }
            }

        private void ResetLoginAttemptsForPpNumber(string ppNumber)
            {
            if (string.IsNullOrWhiteSpace(ppNumber))
                {
                return;
                }

            _loginAttemptTracker?.ResetAttempts(ppNumber, GetRemoteIpAddress());
            }

        private string GetRemoteIpAddress()
            {
            return _httpCon?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            }
        public bool DisposeLoginSession()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            var sessionUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
           
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_lg.Session_END";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = sessionUser.PPNumber;
                cmd.Parameters.Add("SessionId", OracleDbType.Varchar2).Value = sessionUser.SessionId;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = sessionUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = sessionUser.UserRoleID;
                cmd.ExecuteReader();
                }
            con.Dispose();
            ResetLoginAttemptsForPpNumber(sessionUser.PPNumber);
            sessionHandler.DisposeUserSession();
            return true;
            }
        public bool IsLoginSessionExist(string PPNumber = "")
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var sessionUser = sessionHandler.GetUserOrThrow();

            if (PPNumber == "")
                PPNumber = sessionUser.PPNumber;
            bool isSession = false;
            if (PPNumber != null && PPNumber != "")
                {
                var con = this.DatabaseConnection();
               

                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_lg.p_get_user_session";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = PPNumber;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        if (rdr["ID"].ToString() != "" && rdr["ID"].ToString() != null)
                            isSession = true;
                        }
                    }
                con.Dispose();
                }

            return isSession;
            }
        public bool KillExistSession(LoginModel login)
            {
            // Auth bypass allowed here because this method is called before session creation / for session cleanup.
            if (login == null)
                {
                throw new ArgumentNullException(nameof(login));
                }

            var enc_pass = HashEncryptedPassword(login.Password);
            using (var con = this.DatabaseConnection(requireActiveSession: false))
                {
                if (con == null)
                    {
                    throw new DatabaseUnavailableException("Unable to create a database connection with the provided configuration.");
                    }

                bool isSession = false;
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    string _sql = "pkg_lg.p_get_user";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = login.PPNumber;
                    cmd.Parameters.Add("enc_pass", OracleDbType.Varchar2).Value = enc_pass;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    cmd.CommandText = _sql;
                    using (OracleDataReader rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            cmd.CommandText = "pkg_lg.Session_Kill";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = login.PPNumber;
                            cmd.ExecuteReader();
                            isSession = true;
                            }
                        }
                    }

                return isSession;
                }
            }
            public bool TerminateIdleSession()
            {
            // Auth bypass allowed here because this method is called before session creation / for session cleanup.
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            bool isTerminate = false;
            if (!string.IsNullOrEmpty(loggedInUser.PPNumber))
                {
                var con = this.DatabaseConnection(requireActiveSession: false);

                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_lg.Session_Kill";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.ExecuteReader();
                    isTerminate = true;
                    }
                con.Dispose();
                ResetLoginAttemptsForPpNumber(loggedInUser.PPNumber);
                sessionHandler.DisposeUserSession();
                }
            return isTerminate;
            }
        public IActionResult Logout()
            {
            this.DisposeLoginSession();
            return RedirectToAction("Index", "Login");
            }
        #endregion

        #region Authentication
        public UserModel AutheticateLogin(LoginModel login)
            {
            // Auth bypass allowed here because this method is called before session creation / for session cleanup.
            var con = this.DatabaseConnection(requireActiveSession: false);
            if (con == null)
                {
                throw new DatabaseUnavailableException("Unable to create a database connection with the provided configuration.");
                }

            try
                {
                UserModel user = new UserModel
                    {
                    isAlreadyLoggedIn = false,
                    isAuthenticate = false
                    };
                var enc_pass = HashEncryptedPassword(login.Password);
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    string _sql = "pkg_lg.p_get_user";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = login.PPNumber;
                    cmd.Parameters.Add("enc_pass", OracleDbType.Varchar2).Value = enc_pass;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    cmd.CommandText = _sql;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        user.isAuthenticate = true;
                        user.changePassword = rdr["password_change_req"].ToString();
                        user.passwordChangeRequired = enc_pass == HashPassword("Ztbl@1234");
                        user.ID = Convert.ToInt32(rdr["USERID"]);
                        user.Name = rdr["Employeefirstname"].ToString() + " " + rdr["employeelastname"].ToString();
                        user.Email = rdr["LOGIN_NAME"].ToString();
                        user.UserEntityName = rdr["ENT_NAME"].ToString();
                        user.UserRoleName = rdr["GROUP_NAME"].ToString();
                        user.PPNumber = rdr["PPNO"].ToString();
                        if (rdr["ENTITY_ID"].ToString() != null && rdr["ENTITY_ID"].ToString() != "")
                            user.UserEntityID = Convert.ToInt32(rdr["ENTITY_ID"]);

                        user.UserLocationType = rdr["USER_LOCATION_TYPE"].ToString();
                        user.IsActive = rdr["ISACTIVE"].ToString();
                        if (rdr["DIVISIONID"].ToString() != null && rdr["DIVISIONID"].ToString() != "")
                            user.UserPostingDiv = Convert.ToInt32(rdr["DIVISIONID"]);
                        else
                            user.UserPostingDiv = 0;

                        if (rdr["DEPARTMENTID"].ToString() != null && rdr["DEPARTMENTID"].ToString() != "")
                            user.UserPostingDept = Convert.ToInt32(rdr["DEPARTMENTID"]);
                        else
                            user.UserPostingDept = 0;

                        if (rdr["ZONEID"].ToString() != null && rdr["ZONEID"].ToString() != "")
                            user.UserPostingZone = Convert.ToInt32(rdr["ZONEID"]);
                        else
                            user.UserPostingZone = 0;

                        if (rdr["BRANCHID"].ToString() != null && rdr["BRANCHID"].ToString() != "")
                            user.UserPostingBranch = Convert.ToInt32(rdr["BRANCHID"]);
                        else
                            user.UserPostingBranch = 0;

                        if (rdr["AUDIT_ZONEID"].ToString() != null && rdr["AUDIT_ZONEID"].ToString() != "")
                            user.UserPostingAuditZone = Convert.ToInt32(rdr["AUDIT_ZONEID"]);
                        else
                            user.UserPostingAuditZone = 0;

                        if (rdr["GROUP_ID"].ToString() != null && rdr["GROUP_ID"].ToString() != "")
                            user.UserGroupID = Convert.ToInt32(rdr["GROUP_ID"]);
                        else
                            user.UserGroupID = 0;

                        if (rdr["ROLE_ID"].ToString() != null && rdr["ROLE_ID"].ToString() != "")
                            user.UserRoleID = Convert.ToInt32(rdr["ROLE_ID"]);
                    else
                        user.UserRoleID = 0;

                    bool isSessionAvailable = false;
                    string _sql2 = "pkg_lg.p_get_user_id";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = login.PPNumber;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    cmd.CommandText = _sql2;
                    OracleDataReader rdr2 = cmd.ExecuteReader();
                    while (rdr2.Read())
                        {
                        if (rdr2["ID"].ToString() != null && rdr2["ID"].ToString() != "")
                            {
                            isSessionAvailable = !isSessionAvailable;
                            }
                        }

                    var sessionHandler = CreateSessionHandler();


                    if (isSessionAvailable)
                        {
                        user.isAlreadyLoggedIn = true;
                        }
                    else
                        {
                        var resp = sessionHandler.SetSessionUser(user);
                        cmd.CommandText = "pkg_lg.User_SESSION";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = user.PPNumber;
                        cmd.Parameters.Add("UserRoleID", OracleDbType.Int32).Value = user.UserRoleID;
                        cmd.Parameters.Add("LocalIpAddress", OracleDbType.Varchar2).Value = iPAddress.GetLocalIpAddress();
                        cmd.Parameters.Add("SessionId", OracleDbType.Varchar2).Value = resp.SessionId;
                        cmd.Parameters.Add("UserLocationType", OracleDbType.Varchar2).Value = user.UserLocationType;
                        cmd.Parameters.Add("MACAddress", OracleDbType.Varchar2).Value = iPAddress.GetMACAddress();
                        cmd.Parameters.Add("FirstMACCardAddress", OracleDbType.Varchar2).Value = iPAddress.GetFirstMACCardAddress();
                        cmd.Parameters.Add("UserPostingDiv", OracleDbType.Int32).Value = user.UserPostingDiv;
                        cmd.Parameters.Add("UserGroupID", OracleDbType.Varchar2).Value = user.UserGroupID;
                        cmd.Parameters.Add("UserPostingDept", OracleDbType.Int32).Value = user.UserPostingDept;
                        cmd.Parameters.Add("UserPostingZone", OracleDbType.Int32).Value = user.UserPostingZone;
                        cmd.Parameters.Add("UserPostingBranch", OracleDbType.Int32).Value = user.UserPostingBranch;
                        cmd.Parameters.Add("UserPostingAuditZone", OracleDbType.Int32).Value = user.UserPostingAuditZone;
                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = user.UserEntityID;
                        cmd.ExecuteReader();
                        //this.CreateAuditReport();
                        }
                    }
                }
            return user;
                }
            finally
                {
                con.Dispose();
                }
            }
        #endregion
        public async Task<List<AuditeeResponseEvidenceModel>> GetUploadedAuditReportsFromDirectory(string subfolder)
            {
            var filesData = new List<AuditeeResponseEvidenceModel>();
            try
                {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Audit_Report", subfolder);

                if (!Directory.Exists(uploadPath))
                    {
                    return filesData;
                    }

                var files = Directory.GetFiles(uploadPath);

                foreach (var filePath in files)
                    {
                    var fileName = Path.GetFileName(filePath);
                    var fileType = Path.GetExtension(filePath).TrimStart('.'); // Get the file extension without the dot
                    var fileLength = new FileInfo(filePath).Length;

                    string mimeType = GetMimeType(filePath); // Get MIME type

                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
                        {
                        using (var memoryStream = new MemoryStream())
                            {
                            await fileStream.CopyToAsync(memoryStream);
                            var base64String = Convert.ToBase64String(memoryStream.ToArray());

                            filesData.Add(new AuditeeResponseEvidenceModel
                                {
                                FILE_NAME = fileName,
                                IMAGE_NAME = fileName,
                                IMAGE_LENGTH = Convert.ToInt64(fileLength),
                                IMAGE_TYPE = mimeType, // Store MIME type instead of just file extension
                                IMAGE_DATA = base64String
                                });
                            }
                        }
                    }
                }
            catch (Exception)
                {
                // Handle exception (e.g., log error)
                return new List<AuditeeResponseEvidenceModel>();
                }

            return filesData;
            }
        public async Task<List<AuditeeResponseEvidenceModel>> GetAttachedFilesFromDirectory(string subfolder)
            {
            var filesData = new List<AuditeeResponseEvidenceModel>();
            try
                {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostCompliance_Evidences", subfolder);

                if (!Directory.Exists(uploadPath))
                    {
                    return filesData;
                    }

                var files = Directory.GetFiles(uploadPath);

                foreach (var filePath in files)
                    {
                    var fileName = Path.GetFileName(filePath);
                    var fileType = Path.GetExtension(filePath).TrimStart('.'); // Get the file extension without the dot
                    var fileLength = new FileInfo(filePath).Length;

                    string mimeType = GetMimeType(filePath); // Get MIME type

                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
                        {
                        using (var memoryStream = new MemoryStream())
                            {
                            await fileStream.CopyToAsync(memoryStream);
                            var base64String = Convert.ToBase64String(memoryStream.ToArray());

                            filesData.Add(new AuditeeResponseEvidenceModel
                                {
                                FILE_NAME = fileName,                                
                                IMAGE_LENGTH = Convert.ToInt64(fileLength),
                                LENGTH = Convert.ToInt32(fileLength),
                                IMAGE_TYPE = mimeType, // Store MIME type instead of just file extension
                                IMAGE_DATA = base64String
                                });
                            }
                        }
                    }
                }
            catch (Exception)
                {
                // Handle exception (e.g., log error)
                return new List<AuditeeResponseEvidenceModel>();
                }

            return filesData;
            }
        public async Task<List<AuditeeResponseEvidenceModel>> GetAttachedAuditeeEvidencesFromDirectory(string subfolder)
            {
            var filesData = new List<AuditeeResponseEvidenceModel>();
            try
                {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Auditee_Evidences", subfolder);

                if (!Directory.Exists(uploadPath))
                    {
                    return filesData;
                    }

                var files = Directory.GetFiles(uploadPath);

                foreach (var filePath in files)
                    {
                    var fileName = Path.GetFileName(filePath);
                    var fileType = Path.GetExtension(filePath).TrimStart('.'); // Get the file extension without the dot
                    var fileLength = new FileInfo(filePath).Length;

                    string mimeType = GetMimeType(filePath); // Get MIME type

                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
                        {
                        using (var memoryStream = new MemoryStream())
                            {
                            await fileStream.CopyToAsync(memoryStream);
                            var base64String = Convert.ToBase64String(memoryStream.ToArray());

                            filesData.Add(new AuditeeResponseEvidenceModel
                                {
                                FILE_NAME = fileName,
                                IMAGE_LENGTH = Convert.ToInt64(fileLength),
                                LENGTH = Convert.ToInt32(fileLength),
                                IMAGE_TYPE = mimeType, // Store MIME type instead of just file extension
                                IMAGE_DATA = base64String
                                });
                            }
                        }
                    }
                }
            catch (Exception)
                {
                // Handle exception (e.g., log error)
                return new List<AuditeeResponseEvidenceModel>();
                }

            return filesData;
            }
        public async Task<List<AuditeeResponseEvidenceModel>> GetAttachedCAUEvidencesFromDirectory(string subfolder)
            {
            var filesData = new List<AuditeeResponseEvidenceModel>();
            try
                {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CAU_Evidences", subfolder);

                if (!Directory.Exists(uploadPath))
                    {
                    return filesData;
                    }

                var files = Directory.GetFiles(uploadPath);

                foreach (var filePath in files)
                    {
                    var fileName = Path.GetFileName(filePath);
                    var fileType = Path.GetExtension(filePath).TrimStart('.'); // Get the file extension without the dot
                    var fileLength = new FileInfo(filePath).Length;

                    string mimeType = GetMimeType(filePath); // Get MIME type

                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
                        {
                        using (var memoryStream = new MemoryStream())
                            {
                            await fileStream.CopyToAsync(memoryStream);
                            var base64String = Convert.ToBase64String(memoryStream.ToArray());

                            filesData.Add(new AuditeeResponseEvidenceModel
                                {
                                FILE_NAME = fileName,
                                LENGTH = Convert.ToInt32(fileLength),
                                IMAGE_LENGTH = Convert.ToInt64(fileLength),
                                IMAGE_TYPE = mimeType, // Store MIME type instead of just file extension
                                IMAGE_DATA = base64String
                                });
                            }
                        }
                    }
                }
            catch (Exception)
                {
                // Handle exception (e.g., log error)
                return new List<AuditeeResponseEvidenceModel>();
                }

            return filesData;
            }
        public bool DeleteAuditReportSubFolderDirectoryFromServer(string subfolder)
            {
            try
                {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Audit_Report", subfolder);

                if (Directory.Exists(uploadPath))
                    {
                    // Delete the directory and all its contents
                    Directory.Delete(uploadPath, true);

                    return true;
                    }
                else
                    {

                    return false;
                    }
                }
            catch (Exception)
                {

                return false;
                }
            }
        public bool DeleteSubFolderDirectoryFromServer(string subfolder)
            {
            try
                {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostCompliance_Evidences", subfolder);

                if (Directory.Exists(uploadPath))
                    {
                    // Delete the directory and all its contents
                    Directory.Delete(uploadPath, true);

                    return true;
                    }
                else
                    {

                    return false;
                    }
                }
            catch (Exception)
                {

                return false;
                }
            }

        public bool DeleteSubFolderDirectoryInAuditeeEvidenceFromServer(string subfolder)
            {
            try
                {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Auditee_Evidences", subfolder);

                if (Directory.Exists(uploadPath))
                    {
                    // Delete the directory and all its contents
                    Directory.Delete(uploadPath, true);

                    return true;
                    }
                else
                    {

                    return false;
                    }
                }
            catch (Exception)
                {

                return false;
                }
            }
        public bool DeleteSubFolderDirectoryInCAUEvidenceFromServer(string subfolder)
            {
            try
                {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CAU_Evidences", subfolder);

                if (Directory.Exists(uploadPath))
                    {
                    // Delete the directory and all its contents
                    Directory.Delete(uploadPath, true);

                    return true;
                    }
                else
                    {

                    return false;
                    }
                }
            catch (Exception)
                {

                return false;
                }
            }
        // Function to get the MIME type based on file extension
        private string GetMimeType(string filePath)
            {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            string mimeType;
            if (!provider.TryGetContentType(filePath, out mimeType))
                {
                mimeType = "application/octet-stream"; // Default MIME type
                }
            return mimeType;
            }


        #region MenuPage
        public List<MenuModel> GetTopMenus()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();
           
            List<MenuModel> modelList = new List<MenuModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_lg.p_GetTopMenus";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserRoleID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MenuModel menu = new MenuModel();
                    menu.Menu_Id = Convert.ToInt32(rdr["MENU_ID"]);
                    menu.Menu_Name = rdr["MENU_NAME"].ToString();
                    menu.Menu_Order = rdr["MENU_ORDER"].ToString();
                    menu.Menu_Description = rdr["MENU_DESCRIPTION"].ToString();
                    modelList.Add(menu);
                    }
                }
            con.Dispose();
            return modelList;
            }
        public List<MenuPagesModel> GetTopMenuPages()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            return GetTopMenuPages(loggedInUser);
            }

        public List<MenuPagesModel> GetTopMenuPages(SessionUser user)
            {
            if (user == null)
                {
                throw new ArgumentNullException(nameof(user));
                }

            var con = this.DatabaseConnection();

            List<MenuPagesModel> modelList = new List<MenuPagesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_lg.p_GetTopMenuPages";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserGroupID", OracleDbType.Int32).Value = user.UserGroupID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = user.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = user.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = user.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MenuPagesModel menuPage = new MenuPagesModel();
                    menuPage.Id = Convert.ToInt32(rdr["ID"]);
                    menuPage.Menu_Id = Convert.ToInt32(rdr["MENU_ID"]);
                    menuPage.PageId = Convert.ToInt32(rdr["PAGE_ID"]);
                    menuPage.Page_Name = rdr["PAGE_NAME"].ToString();
                    menuPage.Page_Path = rdr["PAGE_PATH"].ToString();
                    menuPage.Page_Order = Convert.ToInt32(rdr["PAGE_ORDER"]);
                    menuPage.Status = rdr["STATUS"].ToString();
                    menuPage.Sub_Menu = rdr["Sub_Menu"].ToString();
                    menuPage.Sub_Menu_Id = rdr["Sub_Menu_Id"].ToString();
                    menuPage.Sub_Menu_Name = rdr["Sub_Menu_Name"].ToString();
                    menuPage.Status = rdr["STATUS"].ToString();
                    menuPage.Page_Key= rdr["PAGE_KEY"].ToString();
                    if (rdr["HIDE_MENU"].ToString() != null && rdr["HIDE_MENU"].ToString() != "")
                        menuPage.Hide_Menu = Convert.ToInt32(rdr["HIDE_MENU"]);
                    modelList.Add(menuPage);
                    }
                }
            //AppendFieldAuditReportMenuPages(modelList);
            con.Dispose();
            return modelList;
            }      
          
        public List<ApiPermissionModel> GetApiPermissions(SessionUser user)
            {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var con = this.DatabaseConnection();
            var modelList = new List<ApiPermissionModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_lg.p_GetApiPermissions";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = user.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = user.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = user.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                    while (rdr.Read())
                        {
                        var apiPath = rdr["API_PATH"]?.ToString();
                        var httpMethod = rdr["HTTP_METHOD"]?.ToString();

                        if (string.IsNullOrWhiteSpace(apiPath) || string.IsNullOrWhiteSpace(httpMethod))
                            continue;

                        modelList.Add(new ApiPermissionModel
                            {
                            ApiPath = apiPath.Trim(),
                            HttpMethod = httpMethod.Trim().ToUpperInvariant()
                            });
                        }
                    }
                }

            con.Dispose();
            return modelList;
            }


        #endregion
        public List<AuditPeriodModel> GetInsYearsForCAU(int dept_code = 0)
            {
            var con = this.DatabaseConnection();
            List<AuditPeriodModel> periodList = new List<AuditPeriodModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_cm.p_CAU_OM_YEAR";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditPeriodModel period = new AuditPeriodModel();
                    period.AUDITPERIODID = Convert.ToInt32(rdr["auditperiodid"]);
                    period.DESCRIPTION = rdr["period"].ToString();
                    periodList.Add(period);
                    }
                }
            con.Dispose();
            return periodList;
            }
        public List<AuditPeriodModel> GetParaPrintingYearsForCAU()
            {
            var con = this.DatabaseConnection();
            List<AuditPeriodModel> periodList = new List<AuditPeriodModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_cm.p_CAU_ARPSE_YEAR";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditPeriodModel period = new AuditPeriodModel();
                    period.AUDITPERIODID = Convert.ToInt32(rdr["auditperiodid"]);
                    period.DESCRIPTION = rdr["period"].ToString();
                    periodList.Add(period);
                    }
                }
            con.Dispose();
            return periodList;
            }
        public string UpdateAuditPeriod(AuditPeriodModel periodModel)
            {
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_Update_AuditPeriod";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ID", OracleDbType.Int32).Value = periodModel.AUDITPERIODID;
                cmd.Parameters.Add("S_ID", OracleDbType.Int32).Value = periodModel.STATUS_ID;
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
        public string AddAuditPeriod(AuditPeriodModel periodModel)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_AddAuditPeriod";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("DESCRIPTION", OracleDbType.Varchar2).Value = periodModel.DESCRIPTION;
                cmd.Parameters.Add("STARTDATE", OracleDbType.Date).Value = periodModel.START_DATE;
                cmd.Parameters.Add("ENDDATE", OracleDbType.Date).Value = periodModel.END_DATE;

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
        public List<AuditTeamModel> GetAuditTeams(int dept_code = 0, int userEntId = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditTeamModel> teamList = new List<AuditTeamModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetAuditTeams";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("dept_code", OracleDbType.Int32).Value = dept_code;
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = userEntId != 0 ? userEntId : loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditTeamModel team = new AuditTeamModel();
                    team.ID = Convert.ToInt32(rdr["ID"]);
                    team.T_ID = Convert.ToInt32(rdr["T_ID"]);
                    team.CODE = rdr["T_CODE"].ToString();
                    team.NAME = rdr["TEAM_NAME"].ToString();
                    team.AUDIT_DEPARTMENT = Convert.ToInt32(rdr["PLACE_OF_POSTING"]);
                    team.TEAMMEMBER_ID = Convert.ToInt32(rdr["MEMBER_PPNO"]);
                    team.IS_TEAMLEAD = rdr["ISTEAMLEAD"].ToString();
                    team.PLACE_OF_POSTING = rdr["AUDIT_DEPARTMENT"].ToString();
                    team.EMPLOYEENAME = rdr["MEMBER_NAME"].ToString();
                    team.STATUS = rdr["STATUS"].ToString();
                    teamList.Add(team);
                    }
                }
            con.Dispose();
            return teamList;
            }
        public string AddAuditTeam(AuditTeamModel aTeam)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_addauditteam";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("teamname", OracleDbType.Varchar2).Value = aTeam.NAME;
                cmd.Parameters.Add("TEAMMEMBER_ID", OracleDbType.Int32).Value = aTeam.TEAMMEMBER_ID;
                cmd.Parameters.Add("MAX_T_ID", OracleDbType.Int32).Value = aTeam.T_ID;
                cmd.Parameters.Add("EMPLOYEENAME", OracleDbType.Varchar2).Value = aTeam.EMPLOYEENAME;
                cmd.Parameters.Add("IS_TEAMLEAD", OracleDbType.Varchar2).Value = aTeam.IS_TEAMLEAD;
                cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = aTeam.STATUS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
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
        public bool DeleteAuditTeam(string T_CODE)
            {
            if (T_CODE != "" && T_CODE != null)
                {
                var sessionHandler = CreateSessionHandler();
                var loggedInUser = sessionHandler.GetUserOrThrow();


                var con = this.DatabaseConnection();

                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_pg.P_DeleteAuditTeam";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("TID", OracleDbType.Int32).Value = T_CODE;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.ExecuteReader();

                    }
                con.Dispose();
                return true;
                }
            else
                return false;
            }
        public int GetLatestTeamID()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            int maxTeamId = 1;
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_MAXTEAMID";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["MAX_T_ID"].ToString() != null && rdr["MAX_T_ID"].ToString() != "")
                        {
                        maxTeamId = Convert.ToInt32(rdr["MAX_T_ID"]);
                        }
                    }

                }
            con.Dispose();
            return maxTeamId;
            }
        public bool AddAuditCriteria(AddAuditCriteriaModel acm)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            bool isAlreadyAdded = true;

            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_ADDAUDITCRITERIA";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYTYPEID", OracleDbType.Int32).Value = acm.ENTITY_TYPEID;
                cmd.Parameters.Add("SIZEID", OracleDbType.Int32).Value = acm.SIZE_ID;
                cmd.Parameters.Add("RISKID", OracleDbType.Int32).Value = acm.RISK_ID;
                cmd.Parameters.Add("FREQUENCYID", OracleDbType.Int32).Value = acm.FREQUENCY_ID;
                cmd.Parameters.Add("NOOFDAYS", OracleDbType.Int32).Value = acm.NO_OF_DAYS;
                cmd.Parameters.Add("visit", OracleDbType.Varchar2).Value = acm.VISIT;
                cmd.Parameters.Add("APPROVALSTATUS", OracleDbType.Int32).Value = acm.APPROVAL_STATUS;
                cmd.Parameters.Add("AUDITPERIODID", OracleDbType.Int32).Value = acm.AUDITPERIODID;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = acm.ENTITY_ID;
                cmd.Parameters.Add("REMARKS", OracleDbType.Varchar2).Value = "AUDIT CRITERIA CREATED";
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["REF"].ToString() != "" && rdr["REF"].ToString() != null && rdr["REF"].ToString() == "1")
                        {
                        isAlreadyAdded = false;
                        }

                    }
                }
            con.Dispose();
            return !isAlreadyAdded;
            }
        public bool UpdateAuditCriteria(AddAuditCriteriaModel acm, string COMMENTS)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_UpdateAuditCriteria";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CID", OracleDbType.Int32).Value = acm.ID;
                cmd.Parameters.Add("ENTITYTYPEID", OracleDbType.Int32).Value = acm.ENTITY_TYPEID;
                cmd.Parameters.Add("SIZEID", OracleDbType.Int32).Value = acm.SIZE_ID;
                cmd.Parameters.Add("RISKID", OracleDbType.Int32).Value = acm.RISK_ID;
                cmd.Parameters.Add("FREQUENCYID", OracleDbType.Int32).Value = acm.FREQUENCY_ID;
                cmd.Parameters.Add("NOOFDAYS", OracleDbType.Int32).Value = acm.NO_OF_DAYS;
                cmd.Parameters.Add("VISITS", OracleDbType.Varchar2).Value = acm.VISIT;
                cmd.Parameters.Add("AUDITPERIOD_ID", OracleDbType.Int32).Value = acm.AUDITPERIODID;
                cmd.Parameters.Add("REMARKS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return true;
            }
        public bool SetAuditCriteriaStatusReferredBack(int ID, string REMARKS)
            {
            if (REMARKS == "")
                REMARKS = "REFERRED BACK";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_SetAuditCriteriaStatusReferredBack";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CID", OracleDbType.Int32).Value = ID;
                cmd.Parameters.Add("REMARKS", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return true;
            }
        public string SetAuditCriteriaStatusApprove(int id, string remarks)
            {
            if (id <= 0)
                return "Invalid criteria id.";

            if (string.IsNullOrWhiteSpace(remarks))
                remarks = "APPROVED";

            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using var con = this.DatabaseConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = "pkg_pg.P_SetAuditCriteriaStatusApprove";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();

            cmd.Parameters.Add("CAID", OracleDbType.Int32).Value = id;
            cmd.Parameters.Add("REMARKS", OracleDbType.Varchar2).Value = remarks;
            cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
            cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
            cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            string remark = "";
            using (var rdr = cmd.ExecuteReader())
                {
                while (rdr.Read())
                    remark = rdr["remark"]?.ToString() ?? "";
                }

            return remark;
            }

        public List<AuditCriteriaModel> GetPendingAuditCriterias()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditCriteriaModel> criteriaList = new List<AuditCriteriaModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetPendingAuditCriterias";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditCriteriaModel acr = new AuditCriteriaModel();
                    acr.ID = Convert.ToInt32(rdr["ID"]);
                    acr.ENTITY_TYPEID = Convert.ToInt32(rdr["ENTITY_TYPEID"]);
                    if (rdr["ENTITY_ID"].ToString() != null && rdr["ENTITY_ID"].ToString() != "")
                        acr.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    if (rdr["SIZE_ID"].ToString() != null && rdr["SIZE_ID"].ToString() != "")
                        acr.SIZE_ID = Convert.ToInt32(rdr["SIZE_ID"]);
                    acr.RISK_ID = Convert.ToInt32(rdr["RISK_ID"]);
                    acr.FREQUENCY_ID = Convert.ToInt32(rdr["FREQUENCY_ID"]);
                    acr.NO_OF_DAYS = Convert.ToInt32(rdr["NO_OF_DAYS"]);
                    acr.APPROVAL_STATUS = Convert.ToInt32(rdr["APPROVAL_STATUS"]);
                    acr.AUDITPERIODID = Convert.ToInt32(rdr["AUDITPERIODID"]);
                    acr.PERIOD = rdr["PERIOD"].ToString();
                    acr.ENTITY = rdr["ENTITY"].ToString();
                    acr.ENTITY_NAME = rdr["NAME"].ToString();
                    acr.FREQUENCY = rdr["FREQUENCY"].ToString();
                    acr.SIZE = rdr["BRSIZE"].ToString();
                    acr.RISK = rdr["RISK"].ToString();
                    acr.VISIT = rdr["VISIT"].ToString();
                    acr.COMMENTS = rdr["REMARKS"].ToString();
                    if (rdr["no_of_entity"].ToString() != null && rdr["no_of_entity"].ToString() != "")
                        acr.ENTITIES_COUNT = Convert.ToInt32(rdr["no_of_entity"].ToString());
                    criteriaList.Add(acr);
                    }
                }
            con.Dispose();
            return criteriaList;
            }
        public List<AuditCriteriaModel> GetRefferedBackAuditCriterias()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            var con = this.DatabaseConnection();
            List<AuditCriteriaModel> criteriaList = new List<AuditCriteriaModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetRefferedBackAuditCriterias";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditCriteriaModel acr = new AuditCriteriaModel();
                    acr.ID = Convert.ToInt32(rdr["ID"]);
                    acr.ENTITY_TYPEID = Convert.ToInt32(rdr["ENTITY_TYPEID"]);
                    if (rdr["SIZE_ID"].ToString() != null && rdr["SIZE_ID"].ToString() != "")
                        acr.SIZE_ID = Convert.ToInt32(rdr["SIZE_ID"]);
                    if (rdr["ENTITY_ID"].ToString() != null && rdr["ENTITY_ID"].ToString() != "")
                        acr.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    acr.RISK_ID = Convert.ToInt32(rdr["RISK_ID"]);
                    acr.FREQUENCY_ID = Convert.ToInt32(rdr["FREQUENCY_ID"]);
                    acr.NO_OF_DAYS = Convert.ToInt32(rdr["NO_OF_DAYS"]);
                    acr.APPROVAL_STATUS = Convert.ToInt32(rdr["APPROVAL_STATUS"]);
                    acr.AUDITPERIODID = Convert.ToInt32(rdr["AUDITPERIODID"]);
                    acr.PERIOD = rdr["PERIOD"].ToString();
                    acr.ENTITY = rdr["ENTITY"].ToString();
                    acr.FREQUENCY = rdr["FREQUENCY"].ToString();
                    acr.SIZE = rdr["BRSIZE"].ToString();
                    acr.RISK = rdr["RISK"].ToString();
                    acr.ENTITY_NAME = rdr["NAME"].ToString();
                    acr.VISIT = rdr["VISIT"].ToString();
                    acr.COMMENTS = rdr["REMARKS"].ToString();// this.GetAuditCriteriaLogLastStatus(acr.ID);
                    criteriaList.Add(acr);
                    }
                }
            con.Dispose();
            return criteriaList;
            }

        public List<AuditCriteriaModel> GetPostChangesAuditCriterias()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditCriteriaModel> criteriaList = new List<AuditCriteriaModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_pg.P_GetPostChangesAuditCriterias";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditCriteriaModel acr = new AuditCriteriaModel();
                    acr.ID = Convert.ToInt32(rdr["ID"]);
                    acr.ENTITY_TYPEID = Convert.ToInt32(rdr["ENTITY_TYPEID"]);
                    if (rdr["SIZE_ID"].ToString() != null && rdr["SIZE_ID"].ToString() != "")
                        acr.SIZE_ID = Convert.ToInt32(rdr["SIZE_ID"]);
                    if (rdr["ENTITY_ID"].ToString() != null && rdr["ENTITY_ID"].ToString() != "")
                        acr.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    acr.RISK_ID = Convert.ToInt32(rdr["RISK_ID"]);
                    acr.FREQUENCY_ID = Convert.ToInt32(rdr["FREQUENCY_ID"]);
                    acr.NO_OF_DAYS = Convert.ToInt32(rdr["NO_OF_DAYS"]);
                    acr.APPROVAL_STATUS = Convert.ToInt32(rdr["APPROVAL_STATUS"]);
                    acr.AUDITPERIODID = Convert.ToInt32(rdr["AUDITPERIODID"]);
                    acr.PERIOD = rdr["PERIOD"].ToString();
                    acr.ENTITY = rdr["ENTITY"].ToString();
                    acr.FREQUENCY = rdr["FREQUENCY"].ToString();
                    acr.SIZE = rdr["BRSIZE"].ToString();
                    acr.RISK = rdr["RISK"].ToString();
                    acr.VISIT = rdr["VISIT"].ToString();
                    acr.ENTITY_NAME = rdr["NAME"].ToString();
                    acr.COMMENTS = rdr["REMARKS"].ToString();// this.GetAuditCriteriaLogLastStatus(acr.ID);
                    criteriaList.Add(acr);
                    }
                }
            con.Dispose();
            return criteriaList;
            }
        public List<AuditeeEntitiesModel> GetAuditeeEntitiesForOutstandingParas(int ENTITY_CODE = 0)
            {
            //Functionality Completed, no further need of this code is required as of now...
            // Once Page will be removed, this function will be removed as well
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            return entitiesList;
            }
        public string GeneratePlanForAuditCriteria(int CRITERIA_ID)
            {
            string resMsg = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.Tentative_Audit_Plan";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CRITERIA_ID", OracleDbType.Int32).Value = CRITERIA_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    resMsg = rdr["REMARKS"].ToString();
                    }

                }
            con.Dispose();
            return resMsg;
            }



        public List<AuditEntitiesModel> GetAuditEntities()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();



            var con = this.DatabaseConnection();


            List<AuditEntitiesModel> entitiesList = new List<AuditEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetAuditEntities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditEntitiesModel entity = new AuditEntitiesModel();
                    entity.AUTID = Convert.ToInt32(rdr["AUTID"]);
                    entity.ENTITYCODE = rdr["ENTITYCODE"].ToString();
                    entity.ENTITYTYPEDESC = rdr["ENTITYTYPEDESC"].ToString();
                    entity.AUDITABLE = rdr["AUDITABLE"].ToString();
                    entity.D_RISK = rdr["d_risk"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }
        public List<AuditeeEntitiesModel> GetAuditeeEntitiesForOldParas(int ENTITY_ID = 0)
            {
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_GetAuditeeEntitiesForOldParas";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITY_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;

                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                    if (rdr["ENTITY_ID"].ToString() != "" && rdr["ENTITY_ID"].ToString() != null)
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    if (rdr["entity_code"].ToString() != "" && rdr["entity_code"].ToString() != null)
                        entity.CODE = Convert.ToInt32(rdr["entity_code"]);
                    if (rdr["entity_name"].ToString() != "" && rdr["entity_name"].ToString() != null)
                        entity.NAME = rdr["entity_name"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }
        public List<AuditeeEntitiesModel> GetEntitiesByParentEntityTypeId(int ENTITY_TYPE_ID = 0)
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetAuditeeEntities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("TYPEID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
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

        public string GeneratePassword(int? length = null)
            {
            var configuredLength = _configuration.GetValue<int?>("PasswordPolicy:MinimumLength") ?? 12;
            var targetLength = Math.Max(length ?? configuredLength, 12);
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()-_=+[]{}<>?";
            var characterSets = new[] { upper, lower, digits, special };
            var allCharacters = string.Concat(characterSets);

            var passwordChars = new List<char>(targetLength);
            var randomBytes = new byte[targetLength];
            RandomNumberGenerator.Fill(randomBytes);

            for (int i = 0; i < characterSets.Length; i++)
                {
                passwordChars.Add(characterSets[i][randomBytes[i] % characterSets[i].Length]);
                }

            for (int i = characterSets.Length; i < targetLength; i++)
                {
                passwordChars.Add(allCharacters[randomBytes[i] % allCharacters.Length]);
                }

            Shuffle(passwordChars);
            return new string(passwordChars.ToArray());
            }
        public bool ChangePassword(string Password, string NewPassowrd)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();



            var con = this.DatabaseConnection();

            var enc_pass = HashEncryptedPassword(Password);
            bool correctPass = false;
            bool res = false;
            var enc_new_pass = HashEncryptedPassword(NewPassowrd);
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_lg.p_get_user";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("enc_pass", OracleDbType.Varchar2).Value = enc_pass;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["USERID"].ToString() != null && rdr["USERID"].ToString() != "")
                        {
                        correctPass = true;
                        res = true;
                        }

                    }
                if (correctPass)
                    {
                    cmd.CommandText = "pkg_lg.P_ChangePassword";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("PP_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("enc_pass", OracleDbType.Varchar2).Value = enc_new_pass;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.ExecuteReader();
                    res = true;
                    }
                }
            con.Dispose();
            return res;
            }

        private static void Shuffle(IList<char> list)
            {
            for (var i = list.Count - 1; i > 0; i--)
                {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
                }
            }

        public BranchModel AddBranch(BranchModel br)
            {
            return br;
            }
        public BranchModel UpdateBranch(BranchModel br)
            {
            return br;
            }

        public ControlViolationsModel AddControlViolation(ControlViolationsModel cv)
            {
            return cv;
            }
        public List<DivisionModel> GetDivisions(bool sessionCheck = true)
            {
            var con = this.DatabaseConnection();
            List<DivisionModel> divList = new List<DivisionModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_GetDepartments";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("E_id", OracleDbType.Int32).Value = 3;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DivisionModel div = new DivisionModel();
                    div.DIVISIONID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    div.NAME = rdr["NAME"].ToString();
                    div.CODE = rdr["CODE"].ToString();
                    div.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    if (rdr["ACTIVE"].ToString() == "Y")
                        div.ISACTIVE = "Active";
                    else if (rdr["ACTIVE"].ToString() == "N")
                        div.ISACTIVE = "InActive";
                    else
                        div.ISACTIVE = rdr["ACTIVE"].ToString();
                    divList.Add(div);
                    }
                }
            con.Dispose();
            return divList;
            }
        public List<AuditObservationTemplateModel> GetAuditObservationTemplates(int activity_id)
            {
            List<AuditObservationTemplateModel> templateList = new List<AuditObservationTemplateModel>();
            return templateList;
            }
        public List<AuditEmployeeModel> GetAuditEmployees(int dept_code = 0)
            {
            var con = this.DatabaseConnection();
            List<AuditEmployeeModel> empList = new List<AuditEmployeeModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetAuditEmployees";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("dept_code", OracleDbType.Int32).Value = dept_code;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditEmployeeModel emp = new AuditEmployeeModel();
                    emp.PPNO = Convert.ToInt32(rdr["PPNO"]);
                    emp.DEPARTMENTCODE = Convert.ToInt32(rdr["DEPARTMENTCODE"]);
                    emp.RANKCODE = Convert.ToInt32(rdr["RANKCODE"]);
                    emp.DESIGNATIONCODE = Convert.ToInt32(rdr["DESIGNATIONCODE"]);

                    emp.DEPTARMENT = rdr["DEPTARMENT"].ToString();
                    emp.EMPLOYEEFIRSTNAME = rdr["EMPLOYEEFIRSTNAME"].ToString();
                    emp.EMPLOYEELASTNAME = rdr["EMPLOYEELASTNAME"].ToString();
                    emp.CURRENT_RANK = rdr["CURRENT_RANK"].ToString();
                    emp.FUN_DESIGNATION = rdr["FUN_DESIGNATION"].ToString();
                    emp.TYPE = rdr["TYPE"].ToString();
                    empList.Add(emp);
                    }
                }
            con.Dispose();
            return empList;
            }
        public List<TentativePlanModel> GetTentativePlansForFields(bool sessionCheck = true)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();

            List<TentativePlanModel> tplansList = new List<TentativePlanModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                string _sql = "pkg_pg.p_get_audit_plan";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                cmd.CommandText = _sql;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    TentativePlanModel tplan = new TentativePlanModel();
                    tplan.PLAN_ID = Convert.ToInt32(rdr["PLAN_ID"]);
                    tplan.CRITERIA_ID = Convert.ToInt32(rdr["CRITERIA_ID"]);
                    tplan.AUDIT_PERIOD_ID = Convert.ToInt32(rdr["AUDITPERIODID"]);
                    tplan.AUDITEDBY = Convert.ToInt32(rdr["AUDITEDBY"]);
                    tplan.BR_SIZE = rdr["AUDITEE_SIZE"].ToString();
                    tplan.RISK = rdr["AUDITEE_RISK"].ToString();
                    tplan.NATURE_OF_AUDIT = rdr["NATURE_OF_AUDIT"].ToString();
                    tplan.NO_OF_DAYS = Convert.ToInt32(rdr["NO_OF_DAYS"]);
                    tplan.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    tplan.CODE = rdr["ENTITY_CODE"].ToString();
                    tplan.ENTITY_TYPE_ID = Convert.ToInt32(rdr["ENTITY_TYPE_ID"].ToString());
                    tplan.ENTITY_NAME = rdr["AUDITEE_NAME"].ToString();
                    tplan.FREQUENCY_DESCRIPTION = rdr["FREQUENCY_DISCRIPTION"].ToString();
                    tplan.PERIOD_NAME = rdr["PERIOD_NAME"].ToString();
                    tplan.REPORTING_OFFICE = rdr["REPORTING_OFFICE"].ToString();
                    tplan.ENT_TYPE = rdr["ent_type"].ToString();
                    tplansList.Add(tplan);
                    }
                }
            con.Dispose();
            return tplansList;
            }
        public string GetAuditOperationalStartDate(int auditPeriodId = 0, int entityCode = 0)
            {
            string result = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetAuditOperationalStartDate";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("entityCode", OracleDbType.Int32).Value = entityCode;
                cmd.Parameters.Add("auditPeriodId", OracleDbType.Int32).Value = auditPeriodId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    result = rdr["YEAR"].ToString() + "-";
                    result += rdr["MONTH"].ToString() + "-";
                    result += rdr["DAY"].ToString();
                    }
                }
            con.Dispose();
            return result;
            }
        public List<AuditRefEngagementPlanModel> GetAuditEngagementPlans()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditRefEngagementPlanModel> list = new List<AuditRefEngagementPlanModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetAuditEngagementPlans";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader ardr = cmd.ExecuteReader();
                while (ardr.Read())
                    {
                    AuditRefEngagementPlanModel eng = new AuditRefEngagementPlanModel();
                    eng.ENG_ID = Convert.ToInt32(ardr["eng_id"].ToString());
                    eng.TEAM_NAME = ardr["team_name"].ToString();
                    eng.ENTITY_NAME = ardr["name"].ToString();
                    eng.AUDIT_STARTDATE = Convert.ToDateTime(ardr["audit_startdate"].ToString()).ToString("dd/MM/yyyy");
                    eng.AUDIT_ENDDATE = Convert.ToDateTime(ardr["audit_enddate"].ToString()).ToString("dd/MM/yyyy");
                    eng.OP_STARTDATE = Convert.ToDateTime(ardr["op_startdate"].ToString()).ToString("dd/MM/yyyy");
                    eng.OP_ENDDATE = Convert.ToDateTime(ardr["op_enddate"].ToString()).ToString("dd/MM/yyyy");
                    eng.ENTITY_ID = Convert.ToInt32(ardr["entity_id"].ToString());
                    list.Add(eng);
                    }
                }
            con.Dispose();
            return list;
            }
        public AuditEngagementPlanModel AddAuditEngagementPlan(AuditEngagementPlanModel ePlan)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            ePlan.CREATED_ON = System.DateTime.Now;
            int placeofposting = Convert.ToInt32(loggedInUser.UserEntityID);
            bool isContinue = false;

            ePlan.CREATEDBY = Convert.ToInt32(loggedInUser.PPNumber);
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_AddAuditEngagementPlan";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PERIODID", OracleDbType.Int32).Value = ePlan.PERIOD_ID ?? (object)DBNull.Value;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ePlan.ENTITY_ID ?? (object)DBNull.Value;
                cmd.Parameters.Add("AUDIT_STARTDATE", OracleDbType.Date).Value = ePlan.AUDIT_STARTDATE ?? (object)DBNull.Value;
                cmd.Parameters.Add("CREATEDBY", OracleDbType.Int32).Value = ePlan.CREATEDBY;
                cmd.Parameters.Add("AUDIT_ENDDATE", OracleDbType.Date).Value = ePlan.AUDIT_ENDDATE ?? (object)DBNull.Value;
                cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = ePlan.STATUS ?? (object)DBNull.Value;
                cmd.Parameters.Add("TEAMID", OracleDbType.Int32).Value = ePlan.TEAM_ID ?? (object)DBNull.Value;
                cmd.Parameters.Add("TEAM_NAME", OracleDbType.Varchar2).Value = ePlan.TEAM_NAME;
                cmd.Parameters.Add("PLANID", OracleDbType.Int32).Value = ePlan.PLAN_ID ?? (object)DBNull.Value;
                cmd.Parameters.Add("OP_STARTDATE", OracleDbType.Date).Value = ePlan.OP_STARTDATE ?? (object)DBNull.Value;
                cmd.Parameters.Add("OP_ENDDATE", OracleDbType.Date).Value = ePlan.OP_ENDDATE ?? (object)DBNull.Value;
                cmd.Parameters.Add("TRAVELDAY", OracleDbType.Int32).Value = ePlan.TRAVELDAY ?? (object)DBNull.Value;
                cmd.Parameters.Add("RRDAY", OracleDbType.Int32).Value = ePlan.RRDAY ?? (object)DBNull.Value;
                cmd.Parameters.Add("D_Day", OracleDbType.Int32).Value = ePlan.D_Day ?? (object)DBNull.Value;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ePlan.REMARKS_OUT = rdr["REMARKS"].ToString();
                    if (rdr["REF"].ToString() != "" && rdr["REF"].ToString() != null && rdr["REF"].ToString() == "1")
                        {
                        isContinue = true;
                        ePlan.IS_SUCCESS = "Yes";
                        }
                    }

                if (isContinue)
                    {
                    cmd.CommandText = "pkg_pg.P_AddAuditteamtasklist";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("TEAMID", OracleDbType.Int32).Value = ePlan.TEAM_ID;
                    cmd.Parameters.Add("PLANID", OracleDbType.Int32).Value = ePlan.PLAN_ID;
                    cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ePlan.ENTITY_ID;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.ExecuteReader();

                    }

                }
            con.Dispose();
            return ePlan;

            }
        public bool RefferedBackAuditEngagementPlan(int ENG_ID, string REMARKS)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_RefferedBackAuditEngagementPlan";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("REMARKS", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return true;
            }
        public string RerecommendAuditEngagementPlan(int ENG_ID, int PLAN_ID, int ENTITY_ID, DateTime OP_START_DATE, DateTime OP_END_DATE, DateTime START_DATE, DateTime END_DATE, int TEAM_ID, string COMMENTS)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_RerecommendAuditEngagementPlan";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("STARTDATE", OracleDbType.Date).Value = START_DATE;
                cmd.Parameters.Add("ENDDATE", OracleDbType.Date).Value = END_DATE;
                cmd.Parameters.Add("TEAMID", OracleDbType.Int32).Value = TEAM_ID;
                cmd.Parameters.Add("PLANID", OracleDbType.Int32).Value = PLAN_ID;
                cmd.Parameters.Add("OP_STARTDATE", OracleDbType.Date).Value = OP_START_DATE;
                cmd.Parameters.Add("OP_ENDDATE", OracleDbType.Date).Value = OP_END_DATE;
                cmd.Parameters.Add("REMARKS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARK"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }
        public bool ApproveAuditEngagementPlan(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_ApproveAuditEngagementPlan";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.ExecuteReader();

                }
            con.Dispose();
            return true;
            }
        public List<AuditPlanModel> GetAuditPlan(int period_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditPlanModel> planList = new List<AuditPlanModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                string _sql = "pkg_ais.p_get_audit_plan";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("AUDITED_BY", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                cmd.CommandText = _sql;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditPlanModel plan = new AuditPlanModel();
                    plan.PLAN_ID = Convert.ToInt32(rdr["PLAN_ID"]);
                    plan.AUDITPERIOD_ID = Convert.ToInt32(rdr["AUDITPERIOD_ID"]);
                    if (rdr["NO_OF_DAYS_AUDIT"].ToString() != null && rdr["NO_OF_DAYS_AUDIT"].ToString() != "")
                        plan.NO_OF_DAYS_AUDIT = Convert.ToInt32(rdr["NO_OF_DAYS_AUDIT"]);
                    if (rdr["AUDITZONE_ID"].ToString() != null && rdr["AUDITZONE_ID"].ToString() != "")
                        plan.AUDITZONE_ID = Convert.ToInt32(rdr["AUDITZONE_ID"]);
                    if (rdr["BRANCH_ID"].ToString() != null && rdr["BRANCH_ID"].ToString() != "")
                        plan.BRANCH_ID = Convert.ToInt32(rdr["BRANCH_ID"]);
                    if (rdr["DIVISION_ID"].ToString() != null && rdr["DIVISION_ID"].ToString() != "")
                        plan.DIVISION_ID = Convert.ToInt32(rdr["DIVISION_ID"]);
                    if (rdr["DEPARTMENT_ID"].ToString() != null && rdr["DEPARTMENT_ID"].ToString() != "")
                        plan.DEPARTMENT_ID = Convert.ToInt32(rdr["DEPARTMENT_ID"]);
                    if (rdr["PLAN_STATUS_ID"].ToString() != null && rdr["PLAN_STATUS_ID"].ToString() != "")
                        plan.PLAN_STATUS_ID = Convert.ToInt32(rdr["PLAN_STATUS_ID"]);
                    if (rdr["BRANCH_SIZE_ID"].ToString() != null && rdr["BRANCH_SIZE_ID"].ToString() != "")
                        plan.BRANCH_SIZE_ID = Convert.ToInt32(rdr["BRANCH_SIZE_ID"]);
                    if (rdr["RISK_LEVEL_ID"].ToString() != null && rdr["RISK_LEVEL_ID"].ToString() != "")
                        plan.RISK_LEVEL_ID = Convert.ToInt32(rdr["RISK_LEVEL_ID"]);
                    if (rdr["SUB_ENTITY_ID"].ToString() != null && rdr["SUB_ENTITY_ID"].ToString() != "")
                        plan.SUB_ENTITY_ID = Convert.ToInt32(rdr["SUB_ENTITY_ID"]);
                    plan.DEPARTMENT_NAME = rdr["DEPARTMENT_NAME"].ToString();
                    plan.BRANCH_NAME = rdr["BRANCH_NAME"].ToString();
                    plan.DIVISION_NAME = rdr["DIVISION_NAME"].ToString();
                    plan.AUDITZONE_NAME = rdr["AUDITZONE_NAME"].ToString();
                    planList.Add(plan);
                    }
                }
            con.Dispose();
            return planList;
            }
        public List<RiskProcessDefinition> GetRiskProcessDefinition()
            {
            var con = this.DatabaseConnection();
            List<RiskProcessDefinition> pdetails = new List<RiskProcessDefinition>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_lg.P_GetRiskProcessDefinition";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessDefinition proc = new RiskProcessDefinition();
                    proc.P_ID = Convert.ToInt32(rdr["T_ID"]);
                    if (rdr["ENTITY_TYPE"].ToString() != null && rdr["ENTITY_TYPE"].ToString() != "")
                        proc.RISK_ID = Convert.ToInt32(rdr["ENTITY_TYPE"]);
                    proc.P_NAME = rdr["HEADING"].ToString();
                    pdetails.Add(proc);
                    }
                }
            con.Dispose();
            return pdetails;
            }
        public List<AuditFrequencyModel> GetAuditFrequencies()
            {
            var con = this.DatabaseConnection();
            List<AuditFrequencyModel> freqList = new List<AuditFrequencyModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.p_GetAuditFrequencies";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditFrequencyModel freq = new AuditFrequencyModel();
                    freq.ID = Convert.ToInt32(rdr["ID"]);
                    freq.FREQUENCY_ID = Convert.ToInt32(rdr["FREQUENCY_ID"]);
                    freq.FREQUENCY_DISCRIPTION = rdr["FREQUENCY_DISCRIPTION"].ToString();
                    freq.STATUS = rdr["STATUS"].ToString();
                    freqList.Add(freq);
                    }
                }
            con.Dispose();
            return freqList;
            }
        public List<GlHeadDetailsModel> GetGlheadDetails(int engId = 0, int gl_code = 0)
            {
            int ENG_ID = this.GetLoggedInUserEngId();
            var con = this.DatabaseConnection();

            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<GlHeadDetailsModel> list = new List<GlHeadDetailsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.p_getglheadsummary";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENG_ID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    GlHeadDetailsModel GlHeadDetails = new GlHeadDetailsModel();
                    GlHeadDetails.BRANCHID = Convert.ToInt32(rdr["BRANCHID"]);
                    GlHeadDetails.GL_TYPEID = Convert.ToInt32(rdr["GL_TYPEID"]);

                    GlHeadDetails.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    // GlHeadDetails.GLSUBCODE = Convert.ToInt32(rdr["GLSUBCODE"]);
                    //GlHeadDetails.GLSUBNAME = rdr["GLSUBNAME"].ToString();
                    //GlHeadDetails.DATETIME = Convert.ToDateTime(rdr["DATETIME"]);
                    GlHeadDetails.BALANCE = Convert.ToDouble(rdr["BALANCE"]);
                    if (rdr["DEBIT"].ToString() != null && rdr["DEBIT"].ToString() != "")
                        GlHeadDetails.DEBIT = Convert.ToDouble(rdr["DEBIT"]);
                    if (rdr["CREDIT"].ToString() != null && rdr["CREDIT"].ToString() != "")
                        GlHeadDetails.CREDIT = Convert.ToDouble(rdr["CREDIT"]);
                    list.Add(GlHeadDetails);
                    }
                }
            con.Dispose();
            return list;

            }
        public GlHeadSubDetailsModel GetGlheadSubDetails(int gltypeid = 0)
            {
            var con = this.DatabaseConnection();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            GlHeadSubDetailsModel GlHeadSubDetails = new GlHeadSubDetailsModel();
            List<GlHeadSubDetailsModel> GlSubHeadList = new List<GlHeadSubDetailsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.p_getglheadsum";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("gltypeid", OracleDbType.Int32).Value = gltypeid;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    GlHeadSubDetailsModel GHSD = new GlHeadSubDetailsModel();
                    GHSD.GLSUBCODE = Convert.ToInt32(rdr["GLSUBCODE"]);
                    GHSD.BRANCHID = Convert.ToInt32(rdr["BRANCHID"]);
                    GHSD.GLSUBNAME = rdr["GLSUBNAME"].ToString();
                    GHSD.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    //GHSD.DATETIME = Convert.ToDateTime(rdr["DATETIME"]);
                    GHSD.BALANCE = Convert.ToDouble(rdr["BALANCE"]);
                    GHSD.DEBIT = Convert.ToDouble(rdr["DEBIT"]);
                    GHSD.CREDIT = Convert.ToDouble(rdr["CREDIT"]);
                    GlSubHeadList.Add(GHSD);
                    GlHeadSubDetails.GL_SUBDETAILS = GlSubHeadList;
                    }
                }
            con.Dispose();
            return GlHeadSubDetails;

            }
        public List<LoanCaseModel> GetLoanCaseDetails(int lid = 0, string type = "", int ENG_ID = 0)
            {

            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<LoanCaseModel> list = new List<LoanCaseModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.P_GetLoanCaseDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENG_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("loantype", OracleDbType.Varchar2).Value = type;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    LoanCaseModel LoanCaseDetails = new LoanCaseModel();
                    //LoanCaseDetails.BRANCHID = Convert.ToInt32(rdr["BRANCHID"]);
                    LoanCaseDetails.CNIC = Convert.ToDouble(rdr["CNIC"]);
                    LoanCaseDetails.LOAN_CASE_NO = Convert.ToInt32(rdr["LOAN_CASE_NO"]);
                    LoanCaseDetails.CUSTOMERNAME = rdr["CUSTOMERNAME"].ToString();
                    LoanCaseDetails.FATHERNAME = rdr["FATHERNAME"].ToString();
                    LoanCaseDetails.DISBURSED_AMOUNT = Convert.ToDouble(rdr["DISBURSED_AMOUNT"]);
                    LoanCaseDetails.PRIN = Convert.ToDouble(rdr["PRIN"]);
                    LoanCaseDetails.MARKUP = Convert.ToDouble(rdr["MARKUP"]);
                    LoanCaseDetails.GLSUBCODE = Convert.ToInt32(rdr["GLSUBCODE"]);
                    // LoanCaseDetails.LOAN_DISB_ID = Convert.ToDouble(rdr["LOAN_DISB_ID"]);
                    LoanCaseDetails.DISB_DATE = Convert.ToDateTime(rdr["DISB_DATE"]);
                    LoanCaseDetails.DISB_STATUSID = Convert.ToInt32(rdr["DISB_STATUSID"]);
                    list.Add(LoanCaseDetails);
                    }
                }
            con.Dispose();
            return list;
            }
        public List<LoanCasedocModel> GetLoanCaseDocuments(int ENG_ID)
            {
            List<LoanCasedocModel> list = new List<LoanCasedocModel>();

            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_GetLoanCaseDocuments";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENG_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    LoanCasedocModel LoanCaseDetails = new LoanCasedocModel();
                    LoanCaseDetails.TEAM_MEM_PPNO = Convert.ToString(rdr["TEAM_MEM_PPNO"]);
                    LoanCaseDetails.BRANCHCODE = Convert.ToString(rdr["BRANCHCODE"]);
                    LoanCaseDetails.LOAN_APP_ID = Convert.ToString(rdr["LOAN_APP_ID"]);
                    LoanCaseDetails.CNIC = Convert.ToString(rdr["CNIC"]);
                    LoanCaseDetails.LOAN_CASE_NO = Convert.ToString(rdr["LOAN_CASE_NO"]);
                    LoanCaseDetails.GLSUBCODE = Convert.ToString(rdr["GLSUBCODE"]);
                    LoanCaseDetails.CUSTOMERNAME = rdr["CUSTOMERNAME"].ToString();
                    LoanCaseDetails.LOAN_DISB_ID = Convert.ToString(rdr["LOAN_DISB_ID"]);
                    LoanCaseDetails.DOCUMENTS = rdr["DOCUMENTS"].ToString();
                    LoanCaseDetails.IMAGES = rdr["IMAGES"].ToString();

                    list.Add(LoanCaseDetails);
                    }
                }
            con.Dispose();
            return list;
            }
        public List<GlHeadDetailsModel> GetIncomeExpenceDetails(int bid = 0, int ENG_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<GlHeadDetailsModel> list = new List<GlHeadDetailsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.P_GetIncomeExpenceDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENG_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GlHeadDetailsModel GlHeadDetails = new GlHeadDetailsModel();
                    //GlHeadDetails.TEAM_MEM_PPNO = Convert.ToDouble(rdr["TEAM_MEM_PPNO"]);
                    GlHeadDetails.NAME = rdr["NAME"].ToString();
                    GlHeadDetails.GLSUBNAME = rdr["GLSUBNAME"].ToString();
                    GlHeadDetails.GLSUBCODE = Convert.ToInt32(rdr["GLSUBCODE"]);
                    // GlHeadDetails.DESCRIPTION = rdr["DESCRIPTION"].ToString();



                    //GlHeadDetails.DAY_END_BALANCE_DATE = Convert.ToDateTime(rdr["DAY_END_BALANCE_DATE"]);
                    // GlHeadDetails.BALANCE = Convert.ToDouble(rdr["BALANCE"]);
                    if (rdr["DEBIT"].ToString() != null && rdr["DEBIT"].ToString() != "")
                        GlHeadDetails.DEBIT = Convert.ToDouble(rdr["DEBIT"]);
                    if (rdr["CREDIT"].ToString() != null && rdr["CREDIT"].ToString() != "")
                        GlHeadDetails.CREDIT = Convert.ToDouble(rdr["CREDIT"]);
                    list.Add(GlHeadDetails);
                    }
                }
            con.Dispose();
            return list;

            }
        public List<DepositAccountModel> GetDepositAccountdetails()
            {
            List<DepositAccountModel> depositacclist = new List<DepositAccountModel>();
            /*
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

           var con = this.DatabaseConnection();
            
            using (OracleCommand cmd = con.CreateCommand())
            {

                cmd.CommandText = "pkg_ais.P_GetDepositAccountdetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    DepositAccountModel depositaccdetails = new DepositAccountModel();
                    depositaccdetails.NAME = rdr["NAME"].ToString();
                    depositacclist.Add(depositaccdetails);
                }
            }
           con.Dispose();*/
            return depositacclist;
            }
        public List<DepositAccountModel> GetDepositAccountSubdetails(string bname = "")
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            int ENG_ID = this.GetLoggedInUserEngId();
            var con = this.DatabaseConnection();
            List<DepositAccountModel> depositaccsublist = new List<DepositAccountModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.P_GetDepositAccountSubdetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DepositAccountModel depositaccsubdetails = new DepositAccountModel();

                    depositaccsubdetails.BRANCH_NAME = rdr["BRANCH_NAME"].ToString();
                    if (rdr["ACC_NUMBER"].ToString() != null && rdr["ACC_NUMBER"].ToString() != "")
                        depositaccsubdetails.ACC_NUMBER = Convert.ToDouble(rdr["ACC_NUMBER"]);
                    if (rdr["ACCOUNTCATEGORY"].ToString() != null && rdr["ACCOUNTCATEGORY"].ToString() != "")
                        depositaccsubdetails.ACCOUNTCATEGORY = rdr["ACCOUNTCATEGORY"].ToString();
                    if (rdr["CUSTOMERNAME"].ToString() != null && rdr["CUSTOMERNAME"].ToString() != "")
                        depositaccsubdetails.CUSTOMERNAME = rdr["CUSTOMERNAME"].ToString();
                    if (rdr["BMVS_VERIFIED"].ToString() != null && rdr["BMVS_VERIFIED"].ToString() != "")
                        depositaccsubdetails.BMVS_VERIFIED = rdr["BMVS_VERIFIED"].ToString();
                    if (rdr["OPENINGDATE"].ToString() != null && rdr["OPENINGDATE"].ToString() != "")
                        {
                        depositaccsubdetails.OPENINGDATE = Convert.ToDateTime(rdr["OPENINGDATE"]);
                        }
                    if (rdr["CNIC"].ToString() != null && rdr["CNIC"].ToString() != "")
                        {
                        depositaccsubdetails.CNIC = Convert.ToDouble(rdr["CNIC"]);
                        }
                    if (rdr["TITLE"].ToString() != null && rdr["TITLE"].ToString() != "")
                        depositaccsubdetails.TITLE = rdr["TITLE"].ToString();


                    if (rdr["ACCOCUNTSTATUS"].ToString() != null && rdr["ACCOCUNTSTATUS"].ToString() != "")
                        depositaccsubdetails.ACCOUNTSTATUS = rdr["ACCOCUNTSTATUS"].ToString();
                    if (rdr["LASTTRANSACTIONDATE"].ToString() != null && rdr["LASTTRANSACTIONDATE"].ToString() != "")
                        {
                        depositaccsubdetails.LASTTRANSACTIONDATE = Convert.ToDateTime(rdr["LASTTRANSACTIONDATE"]);
                        }
                    if (rdr["CNICEXPIRYDATE"].ToString() != null && rdr["CNICEXPIRYDATE"].ToString() != "")
                        {
                        depositaccsubdetails.CNICEXPIRYDATE = Convert.ToDateTime(rdr["CNICEXPIRYDATE"]);
                        }
                    depositaccsublist.Add(depositaccsubdetails);
                    }
                }
            con.Dispose();
            return depositaccsublist;
            }
        public List<LoanCaseModel> GetBranchDesbursementAccountdetails(int bid = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            int brId = Convert.ToInt32(loggedInUser.UserPostingBranch);
            List<LoanCaseModel> list = new List<LoanCaseModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.P_GetBranchDesbursementAccountdetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    LoanCaseModel LoanCaseDetails = new LoanCaseModel();
                    //  LoanCaseDetails.BRANCHID = Convert.ToInt32(rdr["BRANCHID"]);
                    LoanCaseDetails.CNIC = Convert.ToDouble(rdr["CNIC"]);
                    LoanCaseDetails.LOAN_CASE_NO = Convert.ToInt32(rdr["LOAN_CASE_NO"]);
                    LoanCaseDetails.CUSTOMERNAME = rdr["CUSTOMERNAME"].ToString();
                    LoanCaseDetails.FATHERNAME = rdr["FATHERNAME"].ToString();
                    LoanCaseDetails.DISBURSED_AMOUNT = Convert.ToDouble(rdr["DISBURSED_AMOUNT"]);
                    LoanCaseDetails.PRIN = Convert.ToDouble(rdr["PRIN"]);
                    LoanCaseDetails.MARKUP = Convert.ToDouble(rdr["MARKUP"]);
                    LoanCaseDetails.GLSUBCODE = Convert.ToInt32(rdr["GLSUBCODE"]);
                    //  LoanCaseDetails.LOAN_DISB_ID = Convert.ToDouble(rdr["LOAN_DISB_ID"]);
                    LoanCaseDetails.DISB_DATE = Convert.ToDateTime(rdr["DISB_DATE"]);
                    LoanCaseDetails.DISB_STATUSID = Convert.ToInt32(rdr["DISB_STATUSID"]);
                    list.Add(LoanCaseDetails);
                    }
                }
            con.Dispose();
            return list;
            }
        public int GetLoggedInUserEngId()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();

            int engId = 0;
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_lg.P_GetLoggedInUserEngId";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    engId = Convert.ToInt32(rdr["eng_plan_id"]);
                    }
                }
            con.Dispose();
            return engId;
            }
        public string GetRiskDescByID(int risk_id = 0)
            {
            var con = this.DatabaseConnection();
            string response = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_GetRiskDescByID";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("risk_id", OracleDbType.Int32).Value = risk_id;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    response = rdr["DESCRIPTION"].ToString();
                    }
                }
            con.Dispose();
            return response;
            }
        public string GetLatestCommentsOnEngagement(int engId = 0)
            {
            var con = this.DatabaseConnection();
            string response = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetLatestCommentsOnEngagement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    response = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return response;
            }

        public int GetExpectedCountOfAuditEntitiesOnCriteria(int CRITERIA_ID)
            {
            var con = this.DatabaseConnection();
            int count = 0;
            using (OracleCommand cmd = con.CreateCommand())
                {


                cmd.CommandText = "pkg_pg.P_get_Criteria_ent_count";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CID", OracleDbType.Int32).Value = CRITERIA_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr2 = cmd.ExecuteReader();


                while (rdr2.Read())
                    {
                    if (rdr2["NO_OF_ENTITY"].ToString() != null && rdr2["NO_OF_ENTITY"].ToString() != "")
                        count = Convert.ToInt32(rdr2["NO_OF_ENTITY"]);
                    }
                }
            con.Dispose();
            return count;
            }
        public bool DeletePendingCriteria(int CID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_DeletePendingCriteria";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("CID", OracleDbType.Int32).Value = CID;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return true;
            }
        public bool SubmitAuditCriteriaForApproval(int PERIOD_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_SubmitAuditCriteriaForApproval";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("CID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.ExecuteReader();

                string emailSubject = "IAS~ Notification regarding submission of Audit Criteria";
                /* string emailBody = $@"
                 Dear {userFullName},


                 Your password has been successfully reset. Please find your new login details below:

                 Username: {PPNumber}
                 Password: {pass}

                 For security reasons, we recommend that you change this password immediately after logging in.

                 If you did not request this password reset, please contact our support team immediately.


                 Best Regards,

                 Internal Audit System (IAS)
 ";
                EmailConfiguration email = new EmailConfiguration(_configuration);
                 email.ConfigEmail(userEmail, userCCEmail, emailSubject, emailBody); */

                /*cmd.CommandText = "pkg_ais_email.P_ADDAUDITCRITERIA";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr2 = cmd.ExecuteReader();
                while (rdr2.Read())
                {
                    if (rdr2["email_to"].ToString() != "" && rdr2["email_to"].ToString() != null)
                    {
                        email_to = rdr2["email_to"].ToString();

                    }
                    if (rdr2["email_cc"].ToString() != "" && rdr2["email_cc"].ToString() != null)
                    {
                        email_cc = rdr2["email_cc"].ToString();

                    }
                    if (rdr2["subject"].ToString() != "" && rdr2["subject"].ToString() != null)
                    {
                        email_subject = rdr2["subject"].ToString();

                    }
                    if (rdr2["email_body"].ToString() != "" && rdr2["email_body"].ToString() != null)
                    {
                        email_body = rdr2["email_body"].ToString();

                    }
                    EmailConfiguration email = new EmailConfiguration(_configuration);
                    email.ConfigEmail(email_to, email_cc, email_subject, email_body);
                }*/

                }
            con.Dispose();
            return true;
            }
        public List<COSORiskModel> GetCOSORiskForDepartment(int PERIOD_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();

            List<COSORiskModel> list = new List<COSORiskModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_GetCOSORiskForDepartment";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PERIOD_ID", OracleDbType.Int32).Value = PERIOD_ID;
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    COSORiskModel chk = new COSORiskModel();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.DEPT_NAME = rdr["DEPT_NAME"].ToString();
                    chk.RATING_FACTORS = rdr["RATING_FACTORS"].ToString();
                    chk.WEIGHT_ASSIGNED = Convert.ToInt32(rdr["WEIGHT_ASSIGNED"]);
                    chk.SUB_FACTORS = Convert.ToInt32(rdr["SUB_FACTORS"]);
                    chk.MAX_SCORE = Convert.ToInt32(rdr["MAX_SCORE"]);
                    chk.FINAL_SCORE = Convert.ToInt32(rdr["FINAL_SCORE"]);
                    chk.NO_OF_OBSERVATIONS = Convert.ToInt32(rdr["NO_OF_OBSERVATIONS"]);
                    chk.WEIGHTED_AVERAGE_SCORE = Convert.ToInt32(rdr["WEIGHTED_AVERAGE_SCORE"]);
                    chk.AUDIT_RATING = rdr["AUDIT_RATING"].ToString();
                    chk.FINAL_AUDIT_RATING = rdr["FINAL_AUDIT_RATING"].ToString();
                    chk.STATUS = rdr["STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }
        public List<COSORiskModel> GetCOSORiskForBranches(int PERIOD_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();

            List<COSORiskModel> list = new List<COSORiskModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_GetCOSORiskForDepartment";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PERIOD_ID", OracleDbType.Int32).Value = PERIOD_ID;
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    COSORiskModel chk = new COSORiskModel();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.DEPT_NAME = rdr["DEPT_NAME"].ToString();
                    chk.RATING_FACTORS = rdr["RATING_FACTORS"].ToString();
                    chk.WEIGHT_ASSIGNED = Convert.ToInt32(rdr["WEIGHT_ASSIGNED"]);
                    chk.SUB_FACTORS = Convert.ToInt32(rdr["SUB_FACTORS"]);
                    chk.MAX_SCORE = Convert.ToInt32(rdr["MAX_SCORE"]);
                    chk.FINAL_SCORE = Convert.ToInt32(rdr["FINAL_SCORE"]);
                    chk.NO_OF_OBSERVATIONS = Convert.ToInt32(rdr["NO_OF_OBSERVATIONS"]);
                    chk.WEIGHTED_AVERAGE_SCORE = Convert.ToInt32(rdr["WEIGHTED_AVERAGE_SCORE"]);
                    chk.AUDIT_RATING = rdr["AUDIT_RATING"].ToString();
                    chk.FINAL_AUDIT_RATING = rdr["FINAL_AUDIT_RATING"].ToString();
                    chk.STATUS = rdr["STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }
        public CAUOMAssignmentResponseModel CAUOMAssignment(CAUOMAssignmentModel om)
            {
            string encodedMsg = "";
            if (om.CONTENTS_OF_OM != "")
                encodedMsg = encoderDecoder.Encrypt(om.CONTENTS_OF_OM);


            string encodedReply = "";
            if (om.CONTENTS_OF_OM != "")
                encodedReply = encoderDecoder.Encrypt(om.CONTENTS_OF_OM);
            CAUOMAssignmentResponseModel resp = new CAUOMAssignmentResponseModel();

            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_CM.P_CAU_OM";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OM_NO", OracleDbType.Varchar2).Value = om.OM_NO;
                cmd.Parameters.Add("ENCODED_MSG", OracleDbType.Clob).Value = encodedMsg;
                cmd.Parameters.Add("DIV_ID", OracleDbType.Int32).Value = om.DIV_ID;
                cmd.Parameters.Add("key_id", OracleDbType.Varchar2).Value = _cauKey;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("insp_year", OracleDbType.Int32).Value = om.INS_YEAR;

                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["OM_ID"].ToString() != null && rdr["OM_ID"].ToString() != "")
                        resp.ID = Convert.ToInt32(rdr["OM_ID"].ToString());
                    }

                if (resp.ID > 0)
                    {
                    cmd.CommandText = "PKG_CM.P_CAU_OM_REPLY";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("OM_ID", OracleDbType.Int32).Value = resp.ID;
                    cmd.Parameters.Add("OM_NO", OracleDbType.Varchar2).Value = om.OM_NO;
                    cmd.Parameters.Add("ENCODED_MSG", OracleDbType.Clob).Value = encodedReply;
                    cmd.Parameters.Add("EVIDANCE", OracleDbType.Clob).Value = "";
                    cmd.Parameters.Add("DIV_ID", OracleDbType.Int32).Value = om.DIV_ID;
                    cmd.Parameters.Add("key_id", OracleDbType.Varchar2).Value = _cauKey;
                    cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr2 = cmd.ExecuteReader();
                    while (rdr2.Read())
                        {
                        if (rdr2["REF_OUT"].ToString() != null && rdr2["REF_OUT"].ToString() != "")
                            resp.RESPONSE = rdr2["REF_OUT"].ToString();
                        }

                    }
                }
            con.Dispose();
            return resp;
            }
        public CAUOMAssignmentResponseModel CAUOMAssignmentAIR(CAUOMAssignmentAIRModel om)
            {

            string encodedMsg = "";
            if (om.CONTENTS_OF_OM != "")
                encodedMsg = encoderDecoder.Encrypt(om.CONTENTS_OF_OM);


            string encodedReply = "";
            if (om.CONTENTS_OF_OM != "")
                encodedReply = encoderDecoder.Encrypt(om.CONTENTS_OF_OM);
            CAUOMAssignmentResponseModel resp = new CAUOMAssignmentResponseModel();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_CM.P_CAU_AIR";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OM_ID", OracleDbType.Int32).Value = om.OM_NO;
                cmd.Parameters.Add("PARA_NO", OracleDbType.Varchar2).Value = om.PARA_NO;
                cmd.Parameters.Add("ENCODED_MSG", OracleDbType.Clob).Value = encodedMsg;
                cmd.Parameters.Add("STAGE", OracleDbType.Int32).Value = 2;
                cmd.Parameters.Add("STATUS", OracleDbType.Int32).Value = 2;
                cmd.Parameters.Add("key_id", OracleDbType.Varchar2).Value = _cauKey;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["AIRID"].ToString() != null && rdr["AIRID"].ToString() != "")
                        resp.ID = Convert.ToInt32(rdr["AIRID"].ToString());
                    }
                if (resp.ID > 0)
                    {
                    cmd.CommandText = "PKG_CM.P_CAU_AIR_REPLY";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("AIRID", OracleDbType.Int32).Value = om.OM_NO;
                    cmd.Parameters.Add("PARA_ID", OracleDbType.Varchar2).Value = om.PARA_NO;
                    cmd.Parameters.Add("ENCODED_MSG", OracleDbType.Clob).Value = encodedReply;
                    cmd.Parameters.Add("EVIDANCE", OracleDbType.Clob).Value = 2;
                    cmd.Parameters.Add("DIV_ID", OracleDbType.Int32).Value = om.DIV_ID;
                    cmd.Parameters.Add("STATUS", OracleDbType.Int32).Value = 2;
                    cmd.Parameters.Add("key_id", OracleDbType.Varchar2).Value = _cauKey;
                    cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr2 = cmd.ExecuteReader();
                    while (rdr2.Read())
                        {
                        if (rdr2["REF_OUT"].ToString() != null && rdr2["REF_OUT"].ToString() != "")
                            resp.RESPONSE = rdr2["REF_OUT"].ToString();
                        }
                    }
                }
            con.Dispose();
            return resp;
            }
        public CAUOMAssignmentResponseModel CAUOMAssignmentPDP(CAUOMAssignmentPDPModel om)
            {
            string encodedMsg = "";
            if (om.CONTENTS_OF_OM != "")
                encodedMsg = encoderDecoder.Encrypt(om.CONTENTS_OF_OM);
            CAUOMAssignmentResponseModel resp = new CAUOMAssignmentResponseModel();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_CM.T_CAU_PDP";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("DAC_DATES", OracleDbType.Date).Value = om.DAC_DATES;
                cmd.Parameters.Add("Para_id", OracleDbType.Varchar2).Value = om.PARA_ID;
                cmd.Parameters.Add("DAC_Recommendation", OracleDbType.Clob).Value = encodedMsg;
                cmd.Parameters.Add("Report_frequency", OracleDbType.Varchar2).Value = om.REPORT_FREQUENCY;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr2 = cmd.ExecuteReader();
                while (rdr2.Read())
                    {
                    if (rdr2["REF_OUT"].ToString() != null && rdr2["REF_OUT"].ToString() != "")
                        resp.RESPONSE = rdr2["REF_OUT"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }
        public CAUOMAssignmentResponseModel CAUOMAssignmentARPSE(CAUOMAssignmentARPSEModel om)
            {

            string encodedMsg = "";
            if (om.CONTENTS_OF_OM != "")
                encodedMsg = encoderDecoder.Encrypt(om.CONTENTS_OF_OM);
            CAUOMAssignmentResponseModel resp = new CAUOMAssignmentResponseModel();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_CM.T_CAU_ARPSE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PAC_DATES", OracleDbType.Date).Value = om.PAC_DATES;
                cmd.Parameters.Add("Para_id", OracleDbType.Varchar2).Value = om.PARA_ID;
                cmd.Parameters.Add("PAC_DIRECTIVE", OracleDbType.Clob).Value = encodedMsg;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = om.STATUS;
                cmd.Parameters.Add("aprse_year", OracleDbType.Int32).Value = om.PRINTING_DATE;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr2 = cmd.ExecuteReader();
                while (rdr2.Read())
                    {
                    if (rdr2["REF_OUT"].ToString() != null && rdr2["REF_OUT"].ToString() != "")
                        resp.RESPONSE = rdr2["REF_OUT"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }
        public CAUOMAssignmentModel CAUGetPreAddedOM(string OM_NO, string INS_YEAR)
            {
            CAUOMAssignmentModel resp = new CAUOMAssignmentModel();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_CM.P_CAU_OM_Get";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OMNO", OracleDbType.Varchar2).Value = OM_NO;
                cmd.Parameters.Add("insp_year", OracleDbType.Varchar2).Value = INS_YEAR;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["ID"].ToString() != null && rdr["ID"].ToString() != "")
                        resp.ID = Convert.ToInt32(rdr["ID"].ToString());
                    resp.DIV_ID = Convert.ToInt32(rdr["DIV_ID"].ToString());
                    resp.CONTENTS_OF_OM = rdr["CONTENTS_OF_OM"].ToString();
                    resp.CONTENTS_OF_OM = rdr["CONTENTS_OF_OM"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }
        public List<CAUOMAssignmentModel> CAUGetAssignedOMs()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();


            List<CAUOMAssignmentModel> list = new List<CAUOMAssignmentModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_cm.P_CAUGetAssignedOMs";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    CAUOMAssignmentModel chk = new CAUOMAssignmentModel();
                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.DIV_ID = Convert.ToInt32(rdr["DIV_ID"]);
                    chk.STATUS = Convert.ToInt32(rdr["STATUS"]);
                    chk.OM_NO = rdr["OM_NO"].ToString();
                    chk.STATUS_DES = rdr["DISCRIPTION"].ToString();
                    chk.CONTENTS_OF_OM = encoderDecoder.Decrypt(rdr["CONTENTS_OF_OM"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;

            }
        public List<AuditCCQModel> GetCCQ(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<AuditCCQModel> list = new List<AuditCCQModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetCCQ";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditCCQModel chk = new AuditCCQModel();
                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    if (rdr["ENTITY_ID"].ToString() != null && rdr["ENTITY_ID"].ToString() != "")
                        {
                        chk.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                        chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                        }
                    else
                        {
                        chk.ENTITY_NAME = "";

                        }

                    chk.QUESTIONS = rdr["QUESTIONS"].ToString();
                    if (rdr["CONTROL_VIOLATION_ID"].ToString() != null && rdr["CONTROL_VIOLATION_ID"].ToString() != "")
                        {
                        chk.CONTROL_VIOLATION_ID = Convert.ToInt32(rdr["CONTROL_VIOLATION_ID"]);
                        chk.CONTROL_VIOLATION = rdr["VIOLATION_NAME"].ToString();

                        }
                    else
                        {
                        chk.CONTROL_VIOLATION = "";
                        }
                    if (rdr["RISK_ID"].ToString() != null && rdr["RISK_ID"].ToString() != "")
                        {
                        chk.RISK_ID = Convert.ToInt32(rdr["RISK_ID"].ToString());
                        chk.RISK = rdr["RISK_DEF"].ToString();
                        }
                    else
                        {
                        chk.RISK = "";
                        }

                    chk.STATUS = rdr["STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }
        public bool UpdateCCQ(AuditCCQModel ccq)
            {
            bool resp = false;
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_UpdateCCQ";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CID", OracleDbType.Int32).Value = ccq.ID;
                cmd.Parameters.Add("QUESTIONS", OracleDbType.Varchar2).Value = ccq.QUESTIONS;
                cmd.Parameters.Add("CONTROL_VIOLATION_ID", OracleDbType.Int32).Value = ccq.CONTROL_VIOLATION_ID;
                cmd.Parameters.Add("RISK_ID", OracleDbType.Int32).Value = ccq.RISK_ID;
                cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = ccq.STATUS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.ExecuteReader();
                resp = true;
                }
            con.Dispose();
            return resp;
            }
        public bool AuditeeOldParaResponse(AuditeeOldParasResponseModel ob)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();
            bool success = false;

            ob.REPLIEDBY = Convert.ToInt32(loggedInUser.PPNumber);
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_AuditeeOldParaResponse";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = ob.AU_OBS_ID;
                cmd.Parameters.Add("REPLY", OracleDbType.Clob).Value = ob.REPLY;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.ExecuteReader();
                success = true;
                }
            con.Dispose();
            return success;
            }


        public List<AuditeeOldParasModel> GetOutstandingParas(string ENTITY_ID)
            {
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            return list;
            }
        public List<OldParasModel> GetOldParasAuditYear()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();
            List<OldParasModel> list = new List<OldParasModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_GetOldParasAuditYear";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }
        public List<OldParasModel> GetOutstandingParasAuditYear()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();
            List<OldParasModel> list = new List<OldParasModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_GetOutstandingParasAuditYear";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }
        public List<UserWiseOldParasPerformanceModel> GetUserWiseOldParasPerformance()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();


            List<UserWiseOldParasPerformanceModel> list = new List<UserWiseOldParasPerformanceModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_GetUserWiseOldParasPerformance";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    UserWiseOldParasPerformanceModel chk = new UserWiseOldParasPerformanceModel();
                    chk.AUDIT_ZONEID = rdr["AUDIT_ZONEID"].ToString();
                    chk.ZONENAME = rdr["ZONENAME"].ToString();
                    chk.PARA_ENTERED = rdr["PARA_ENTERED"].ToString();
                    chk.PPNO = rdr["PPNO"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }
        public List<StaffPositionModel> GetStaffPosition()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<StaffPositionModel> list = new List<StaffPositionModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.P_GetStaffPosition";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();


                while (rdr.Read())
                    {
                    StaffPositionModel staffposition = new StaffPositionModel();
                    staffposition.PPNO = Convert.ToInt32(rdr["PPNO"]);
                    staffposition.EMPLOYEE_NAME = Convert.ToString(rdr["EMPLOYEE_NAME"]);

                    staffposition.QUALIFICATION = Convert.ToString(rdr["QUALIFICATION"]);
                    staffposition.DATE_OF_POSTING = Convert.ToDateTime(rdr["DATE_OF_POSTING"]);
                    staffposition.DESIGNATION = Convert.ToString(rdr["DESIGNATION"]);
                    staffposition.RANK_DESC = Convert.ToString(rdr["RANK_DESC"]);
                    staffposition.PLACE_OF_POSTING = Convert.ToString(rdr["PLACE_OF_POSTING"]);


                    list.Add(staffposition);
                    }
                }
            con.Dispose();
            return list;
            }
        public bool AddDivisionalHeadRemarksOnFunctionalLegacyPara(int CONCERNED_DEPT_ID = 0, string COMMENTS = "", int REF_PARA_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ais.P_AddDivisionalHeadRemarksOnFunctionalLegacyPara";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CONCERNED_DEPTID", OracleDbType.Int32).Value = CONCERNED_DEPT_ID;
                cmd.Parameters.Add("COMMENTS", OracleDbType.Varchar2).Value = COMMENTS;
                cmd.Parameters.Add("REF_PARAID", OracleDbType.Int32).Value = REF_PARA_ID;
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return true;
            }
        [Obsolete]
        public void SaveImage(string base64img, string outputImgFilename = "image.jpg")
            {
            var folderPath = System.IO.Path.Combine(_env.WebRootPath, "Auditee_Evidences");
            if (!System.IO.Directory.Exists(folderPath))
                {
                System.IO.Directory.CreateDirectory(folderPath);
                }
            System.IO.File.WriteAllBytes(Path.Combine(folderPath, outputImgFilename), Convert.FromBase64String(base64img));
            }
        [Obsolete]
        public void DeleteImage(string Filename = "image.jpg")
            {
            var filePath = System.IO.Path.Combine(_env.WebRootPath, "Auditee_Evidences", Filename);
            if (System.IO.File.Exists(filePath))
                {
                System.IO.File.Delete(filePath);
                }
            }
        public string CreateAuditReport(int ENG_ID)
            {
            List<ManageObservations> list = new List<ManageObservations>();
            string filename = "";
            return filename;

            /*list = this.GetManagedObservations(ENG_ID, 0);
            var folderPath = "";
            string entityname = list[0].ENTITY_NAME;
            string period = list[0].PERIOD;
            using (MemoryStream mem = new MemoryStream())
            {
                StringBuilder sb = new StringBuilder();
                //Table For Practice
                sb.Append(@"<center><h1><u>Audit Report on " + entityname + " </u></h1><h3>" + period + "</h3><h3>Version: Draft</h3></center>");

                sb.Append(@"<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><h1>Audit Observations</h1>");



                foreach(var item in list)
                {
                    List<object> outText = new List<object>();

                    outText=this.GetObservationText(item.OBS_ID,0);
                    sb.Append("<h3 style='margin-top:50px;'>Memo No : "+item.MEMO_NO+"</h3>");
                    sb.Append("<div style='margin-top:10px;'>"+ outText [0]+ "</div>");
                    sb.Append("<h3 style='margin-top:10px;'>Auditee Reply</h3>");
                    sb.Append("<div style='margin-top:10px;'>" + outText[1] + "</div>");

                }              

               
                string path = "";
               
                //ltTable.Text = sb.ToString();
                folderPath = System.IO.Path.Combine(_env.WebRootPath, "Audit_Reports");
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }
                filename = "DraftReport_" + ENG_ID + ".Pdf"; ;
                //path = Path.Combine(contentRootPath, filename + ".Pdf");
                path = Path.Combine(folderPath, filename);

                PdfWriter writer = new PdfWriter(path);
                PdfDocument pdf = new PdfDocument(writer);
                pdf.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A0);                

                ConverterProperties converterProperties = new ConverterProperties();
                PdfDocument pdfDocument = new PdfDocument(writer);
                
                iText.Layout.Document document = HtmlConverter.ConvertToDocument(sb.ToString(), pdfDocument, converterProperties);



                var xmlParse = new XMLParser();
                xmlParse.Parse(new StringReader(sb.ToString()));
                xmlParse.Flush();

                document.Close();

                
            }
            return filename;
            */
            }
        public List<Glheadsummaryyearlymodel> GetGlheadDetailsyearwise(int engId = 0, int gl_code = 0)
            {
            int ENG_ID = this.GetLoggedInUserEngId();
            var con = this.DatabaseConnection();

            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<Glheadsummaryyearlymodel> list = new List<Glheadsummaryyearlymodel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.p_getglheadsummary_Yearly";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    Glheadsummaryyearlymodel GlHeadDetails = new Glheadsummaryyearlymodel();
                    GlHeadDetails.GLSUBCODE = Convert.ToInt32(rdr["GLSUBCODE"]);
                    GlHeadDetails.BRANCHID = Convert.ToInt32(rdr["BRANCHID"]);
                    GlHeadDetails.GLSUBNAME = rdr["GLSUBNAME"].ToString();

                    if (rdr["BALANCE_2021"].ToString() != null && rdr["BALANCE_2021"].ToString() != "")
                        GlHeadDetails.BALANCE_2021 = Convert.ToDouble(rdr["BALANCE_2021"]);
                    if (rdr["DEBIT_2021"].ToString() != null && rdr["DEBIT_2021"].ToString() != "")
                        GlHeadDetails.DEBIT_2021 = Convert.ToDouble(rdr["DEBIT_2021"]);
                    if (rdr["CREDIT_2021"].ToString() != null && rdr["CREDIT_2021"].ToString() != "")
                        GlHeadDetails.CREDIT_2021 = Convert.ToDouble(rdr["CREDIT_2021"]);
                    if (rdr["BALANCE_2022"].ToString() != null && rdr["BALANCE_2022"].ToString() != "")
                        GlHeadDetails.BALANCE_2022 = Convert.ToDouble(rdr["BALANCE_2022"]);
                    if (rdr["DEBIT_2022"].ToString() != null && rdr["DEBIT_2022"].ToString() != "")
                        GlHeadDetails.DEBIT_2022 = Convert.ToDouble(rdr["DEBIT_2022"]);
                    if (rdr["CREDIT_2022"].ToString() != null && rdr["CREDIT_2022"].ToString() != "")
                        GlHeadDetails.CREDIT_2022 = Convert.ToDouble(rdr["CREDIT_2022"]);

                    GlHeadDetails.COL1 = rdr["COL1"].ToString();
                    GlHeadDetails.COL2 = rdr["COL2"].ToString();
                    GlHeadDetails.COL3 = rdr["COL3"].ToString();

                    GlHeadDetails.LAST_CREDIT = rdr["LAST_CREDIT"].ToString();
                    GlHeadDetails.LAST_DEBIT = rdr["LAST_DEBIT"].ToString();
                    GlHeadDetails.LAST_BALANCE = rdr["LAST_BALANCE"].ToString();

                    GlHeadDetails.CURRENT_CREDIT = rdr["CURRENT_CREDIT"].ToString();
                    GlHeadDetails.CURRENT_DEBIT = rdr["CURRENT_DEBIT"].ToString();
                    GlHeadDetails.CURRENT_BALANCE = rdr["CURRENT_BALANCE"].ToString();

                    list.Add(GlHeadDetails);
                    }
                }
            con.Dispose();
            return list;

            }
        public List<DepositAccountCatModel> GetDepositCat()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            int ENG_ID = this.GetLoggedInUserEngId();

            var con = this.DatabaseConnection();
            List<DepositAccountCatModel> list = new List<DepositAccountCatModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_AI.P_GetDepositACCOUNTCATEGORY";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    DepositAccountCatModel depcat = new DepositAccountCatModel();

                    depcat.BRANCH_NAME = rdr["BRANCH_NAME"].ToString();
                    depcat.ACCOUNTCATEGORY = rdr["ACCOUNTCATEGORY"].ToString();
                    depcat.ACCOUNTCATEGORYID = Convert.ToInt32(rdr["ACCOUNTCATEGORYID"]);
                    depcat.ACCOCUNTSTATUS = rdr["ACCOCUNTSTATUS"].ToString();
                    if (rdr["AMOUNT"].ToString() != null && rdr["AMOUNT"].ToString() != "")
                        depcat.AMOUNT = Convert.ToDouble(rdr["AMOUNT"]);

                    list.Add(depcat);
                    }
                }
            con.Dispose();
            return list;

            }
        public List<DepositAccountCatDetailsModel> GetDepositAccountcatdetails(int catid = 0)

            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            int ENG_ID = this.GetLoggedInUserEngId();
            var con = this.DatabaseConnection();
            List<DepositAccountCatDetailsModel> depositaccsublist = new List<DepositAccountCatDetailsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_AIS.P_GetDepositACCOUNTCATEGORY_details";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("catid", OracleDbType.Int32).Value = catid;

                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DepositAccountCatDetailsModel depositaccsubdetails = new DepositAccountCatDetailsModel();

                    depositaccsubdetails.BRANCH_NAME = rdr["BRANCH_NAME"].ToString();
                    if (rdr["ACC_NUMBER"].ToString() != null && rdr["ACC_NUMBER"].ToString() != "")
                        depositaccsubdetails.ACC_NUMBER = Convert.ToDouble(rdr["ACC_NUMBER"]);
                    if (rdr["ACCOUNTCATEGORY"].ToString() != null && rdr["ACCOUNTCATEGORY"].ToString() != "")
                        depositaccsubdetails.ACCOUNTCATEGORY = rdr["ACCOUNTCATEGORY"].ToString();

                    if (rdr["CUSTOMERNAME"].ToString() != null && rdr["CUSTOMERNAME"].ToString() != "")
                        depositaccsubdetails.CUSTOMERNAME = rdr["CUSTOMERNAME"].ToString();
                    if (rdr["BMVS_VERIFIED"].ToString() != null && rdr["BMVS_VERIFIED"].ToString() != "")
                        depositaccsubdetails.BMVS_VERIFIED = rdr["BMVS_VERIFIED"].ToString();
                    if (rdr["OPENINGDATE"].ToString() != null && rdr["OPENINGDATE"].ToString() != "")
                        {
                        depositaccsubdetails.OPENINGDATE = Convert.ToDateTime(rdr["OPENINGDATE"]);
                        }
                    if (rdr["CNIC"].ToString() != null && rdr["CNIC"].ToString() != "")
                        {
                        depositaccsubdetails.CNIC = Convert.ToDouble(rdr["CNIC"]);
                        }
                    if (rdr["TITLE"].ToString() != null && rdr["TITLE"].ToString() != "")
                        depositaccsubdetails.TITLE = rdr["TITLE"].ToString();


                    if (rdr["ACCOCUNTSTATUS"].ToString() != null && rdr["ACCOCUNTSTATUS"].ToString() != "")
                        depositaccsubdetails.ACCOUNTSTATUS = rdr["ACCOCUNTSTATUS"].ToString();
                    if (rdr["LASTTRANSACTIONDATE"].ToString() != null && rdr["LASTTRANSACTIONDATE"].ToString() != "")
                        {
                        depositaccsubdetails.LASTTRANSACTIONDATE = Convert.ToDateTime(rdr["LASTTRANSACTIONDATE"]);
                        }
                    if (rdr["CNICEXPIRYDATE"].ToString() != null && rdr["CNICEXPIRYDATE"].ToString() != "")
                        {
                        depositaccsubdetails.CNICEXPIRYDATE = Convert.ToDateTime(rdr["CNICEXPIRYDATE"]);
                        }
                    depositaccsublist.Add(depositaccsubdetails);
                    }
                }
            con.Dispose();
            return depositaccsublist;
            }
        public List<LoanSchemeModel> GetLoansScheme(int engId)
            {
            int ENG_ID = this.GetLoggedInUserEngId();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<LoanSchemeModel> list = new List<LoanSchemeModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.P_preauditinfo_loan_scheme";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENG_ID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;

                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    LoanSchemeModel LoanSchemeDetails = new LoanSchemeModel();

                    LoanSchemeDetails.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    //LoanSchemeDetails.DISB_STATUSID = Convert.ToInt32(rdr["DISB_STATUSID"]);
                    LoanSchemeDetails.GLSUBCODE = Convert.ToInt32(rdr["GLSUBCODE"]);
                    LoanSchemeDetails.GLSUBNAME = rdr["GLSUBNAME"].ToString();
                    LoanSchemeDetails.DISBURSED_AMOUNT = Convert.ToDouble(rdr["DISBURSED_AMOUNT"]);
                    LoanSchemeDetails.PRIN_OUT = Convert.ToDouble(rdr["PRIN_OUT"]);
                    LoanSchemeDetails.MARKUP_OUT = Convert.ToDouble(rdr["MARKUP_OUT"]);

                    list.Add(LoanSchemeDetails);
                    }
                }
            con.Dispose();
            return list;
            }
        public List<LoanSchemeYearlyModel> GetLoansSchemeYearly(int engId)
            {
            int ENG_ID = this.GetLoggedInUserEngId();
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<LoanSchemeYearlyModel> list = new List<LoanSchemeYearlyModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ai.P_preauditinfo_loan_scheme_yearly";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENG_ID", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;

                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    LoanSchemeYearlyModel LoanSchemeDetails = new LoanSchemeYearlyModel();

                    LoanSchemeDetails.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    LoanSchemeDetails.DISB_STATUSID = Convert.ToInt32(rdr["DISB_STATUSID"]);
                    LoanSchemeDetails.GLSUBCODE = Convert.ToInt32(rdr["GLSUBCODE"]);
                    LoanSchemeDetails.GLSUBNAME = rdr["GLSUBNAME"].ToString();
                    if (rdr["DISBURSED_AMOUNT_2021"].ToString() != null && rdr["DISBURSED_AMOUNT_2021"].ToString() != "")
                        LoanSchemeDetails.DISBURSED_AMOUNT_2021 = Convert.ToDouble(rdr["DISBURSED_AMOUNT_2021"]);
                    if (rdr["PRIN_OUT_2021"].ToString() != null && rdr["PRIN_OUT_2021"].ToString() != "")
                        LoanSchemeDetails.PRIN_OUT_2021 = Convert.ToDouble(rdr["PRIN_OUT_2021"]);
                    if (rdr["MARKUP_OUT_2021"].ToString() != null && rdr["MARKUP_OUT_2021"].ToString() != "")
                        LoanSchemeDetails.MARKUP_OUT_2021 = Convert.ToDouble(rdr["MARKUP_OUT_2021"]);
                    if (rdr["DISBURSED_AMOUNT_2022"].ToString() != null && rdr["DISBURSED_AMOUNT_2022"].ToString() != "")
                        LoanSchemeDetails.DISBURSED_AMOUNT_2022 = Convert.ToDouble(rdr["DISBURSED_AMOUNT_2022"]);


                    if (rdr["PRIN_OUT_2022"].ToString() != null && rdr["PRIN_OUT_2022"].ToString() != "")
                        LoanSchemeDetails.PRIN_OUT_2022 = Convert.ToDouble(rdr["PRIN_OUT_2022"]);
                    if (rdr["MARKUP_OUT_2022"].ToString() != null && rdr["MARKUP_OUT_2022"].ToString() != "")
                        LoanSchemeDetails.MARKUP_OUT_2022 = Convert.ToDouble(rdr["MARKUP_OUT_2022"]);

                    list.Add(LoanSchemeDetails);
                    }
                }
            con.Dispose();
            return list;
            }
        public DraftReportSummaryModel GetDraftReportSummary(int ENG_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ManageObservations> paras = new List<ManageObservations>();
            DraftReportSummaryModel list = new DraftReportSummaryModel();

            if (loggedInUser.UserLocationType == "Z")
                {
                paras = this.GetManagedObservationsForBranches(ENG_ID, OBS_ID);
                }
            else
                {
                paras = this.GetManagedObservations(ENG_ID, OBS_ID);
                }

            foreach (var p in paras)
                {
                list.Total++;
                if (p.OBS_STATUS_ID == 7)
                    list.Dropped++;
                if (p.OBS_STATUS_ID == 5)
                    list.AddtoDraft++;
                if (p.OBS_STATUS_ID == 4)
                    list.Settled++;
                if (p.OBS_RISK_ID == 3)
                    list.Low++;
                if (p.OBS_RISK_ID == 2)
                    list.Medium++;
                if (p.OBS_RISK_ID == 1)
                    list.High++;
                }

            return list;
            }

        //------------------- Special Audit Plan 
        public List<SpecialAuditPlanModel> GetSaveSpecialAuditPlan()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var list = new List<SpecialAuditPlanModel>();
            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_pg.P_GET_Specical_Audit_for_Approval";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                        cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                        cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                var review = new SpecialAuditPlanModel
                                    {
                                    REPORTING_OFFICE = rdr["reporting"].ToString(),
                                    REPORTING_OFFICE_ID = rdr["reporting_id"].ToString(),
                                    ENTITY_NAME = rdr["auditee"].ToString(),
                                    ENTITY_ID = rdr["auditee_id"].ToString(),
                                    AUDITED_BY = rdr["auditor"].ToString(),
                                    AUDITED_BY_ID = rdr["auditor_id"].ToString(),
                                    PLAN_ID = rdr["P_ID"].ToString(),
                                    AUDIT_PERIOD = rdr["period"].ToString(),
                                    AUDIT_PERIOD_ID = rdr["period_id"].ToString(),
                                    NO_DAYS = rdr["no_of_days"].ToString(),
                                    NATURE = rdr["nature"].ToString(),
                                    NATURE_ID = rdr["nature_id"].ToString(),
                                    // FIELD_VISIT = rdr["visit"].ToString(),
                                    };


                                list.Add(review);
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
        public string AddSpecialAuditPlan(string NATURE, string PERIOD, string ENTITY_ID, string NO_DAYS, string PLAN_ID, string INDICATOR)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            var list = new List<SpecialAuditPlanModel>();
            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_pg.P_ADD_Special_Audit_Plan";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("P_ID", OracleDbType.Int32).Value = PLAN_ID;
                        cmd.Parameters.Add("NOOFDAYS", OracleDbType.Int32).Value = NO_DAYS;
                        cmd.Parameters.Add("Nature", OracleDbType.Int32).Value = NATURE;
                        cmd.Parameters.Add("AUDITPERIODID", OracleDbType.Int32).Value = PERIOD;
                        cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ENTITY_ID;
                        cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                        cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                        cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                resp = rdr["remarks"].ToString();
                                }
                            }
                        }
                    }
                }
            catch (Exception)
                {

                throw;
                }

            return resp;
            }
        public string DeleteSpecialAuditPlan(string PLAN_ID, string INDICATOR)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_pg.P_Update_Special_Audit";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("P_ID", OracleDbType.Int32).Value = PLAN_ID;
                        cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                        cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;

                        cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                resp = rdr["remarks"].ToString();
                                }
                            }
                        }
                    }
                }
            catch (Exception)
                {

                throw;
                }

            return resp;
            }
        public string SubmitSpecialAuditPlan(string PLAN_ID, string INDICATOR)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_pg.P_Update_Special_Audit";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("P_ID", OracleDbType.Int32).Value = PLAN_ID;
                        cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                        cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                        cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                resp = rdr["remarks"].ToString();
                                }
                            }
                        }
                    }
                }
            catch (Exception)
                {

                throw;
                }

            return resp;
            }
        public string AddResponsiblePersonsToObservation(int NEW_PARA_ID, int eng_id, int com_id, string INDICATOR, ObservationResponsiblePPNOModel RESPONSIBLE, int paraStatus)
            {
            if (paraStatus < 8)
                {
                return AddInitialResponsibilityAssignment(NEW_PARA_ID, eng_id, com_id, RESPONSIBLE, INDICATOR);
                }
            else
                {
                return UpdateResponsibilityAssignment(NEW_PARA_ID, eng_id, com_id, INDICATOR, RESPONSIBLE);
                }
            }

        public string DeleteResponsibilityFromObservation(int paraId, int eng_id, ObservationResponsiblePPNOModel responsible)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_Delete_responsibility";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("PARA_ID", OracleDbType.Int32).Value = paraId;
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = eng_id;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = responsible.PP_NO;
                cmd.Parameters.Add("L_CASE", OracleDbType.Int32).Value = responsible.LOAN_CASE.HasValue ? (object)responsible.LOAN_CASE.Value : DBNull.Value;
                cmd.Parameters.Add("NO_ACCOUNT", OracleDbType.Int32).Value = responsible.ACCOUNT_NUMBER;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
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

        public List<MenuPagesModel> GetMenuPagesId(string Page_Path)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            var con = this.DatabaseConnection();

            List<MenuPagesModel> modelList = new List<MenuPagesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_lg.p_GetTopMenuPages";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Page_Path", OracleDbType.Varchar2).Value = Page_Path;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    MenuPagesModel menuPage = new MenuPagesModel();
                    menuPage.Id = Convert.ToInt32(rdr["ID"]);
                    menuPage.Menu_Id = Convert.ToInt32(rdr["MENU_ID"]);
                    menuPage.Page_Name = rdr["PAGE_NAME"].ToString();
                    menuPage.Page_Path = rdr["PAGE_PATH"].ToString();
                    menuPage.Page_Order = Convert.ToInt32(rdr["PAGE_ORDER"]);
                    menuPage.Status = rdr["STATUS"].ToString();
                    menuPage.Sub_Menu = rdr["Sub_Menu"].ToString();
                    menuPage.Sub_Menu_Id = rdr["Sub_Menu_Id"].ToString();
                    menuPage.Sub_Menu_Name = rdr["Sub_Menu_Name"].ToString();
                    menuPage.Status = rdr["STATUS"].ToString();
                    if (rdr["HIDE_MENU"].ToString() != null && rdr["HIDE_MENU"].ToString() != "")
                        menuPage.Hide_Menu = Convert.ToInt32(rdr["HIDE_MENU"]);
                    modelList.Add(menuPage);
                    }
                }
            con.Dispose();
            return modelList;
            }
        }

    }
