using AIS.Controllers;
using AIS.Models;
using AIS.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace AIS
    {
    public class SessionHandler
        {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> ActiveSessionStamps = new System.Collections.Concurrent.ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configurationRoot;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly LocalIPAddress _ipAddressProvider = new LocalIPAddress();
        private readonly AIS.Security.Cryptography.SecurityTokenService _tokenService;

        public SessionHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, AIS.Security.Cryptography.SecurityTokenService tokenService)
            {
            _httpContextAccessor = httpContextAccessor;
            _configurationRoot = configuration;
            _tokenService = tokenService;
            _serializerOptions = CreateSerializerOptions();
            }

        private static JsonSerializerOptions CreateSerializerOptions()
            {
            return new JsonSerializerOptions
                {
                PropertyNameCaseInsensitive = true
                };
            }

        private HttpContext HttpContext => _httpContextAccessor?.HttpContext;

        private ISession Session => HttpContext?.Session;

        public bool TryGetUser(out SessionUser user)
            {
            user = null;
            if (!TryGetRawValue(SessionKeys.User, out var payload))
                {
                return false;
                }

            try
                {
                user = JsonSerializer.Deserialize<SessionUser>(payload, _serializerOptions);
                }
            catch (JsonException)
                {
                return false;
                }

            return user != null;
            }

        public SessionUser GetUserOrThrow()
            {
            if (!TryGetUser(out var user))
                {
                throw new UnauthenticatedException();
                }

            ValidateUser(user);
            return user;
            }

        private static void ValidateUser(SessionUser user)
            {
            if (user == null)
                {
                throw new UnauthenticatedException();
                }

            var missingFields = new List<string>();

            if (!user.UserEntityID.HasValue || user.UserEntityID.Value <= 0)
                {
                missingFields.Add(nameof(SessionUser.UserEntityID));
                }

            if (user.UserRoleID <= 0)
                {
                missingFields.Add(nameof(SessionUser.UserRoleID));
                }

            if (string.IsNullOrWhiteSpace(user.PPNumber))
                {
                missingFields.Add(nameof(SessionUser.PPNumber));
                }

            if (missingFields.Count > 0)
                {
                throw new UnauthenticatedException($"Session user is missing required fields: {string.Join(", ", missingFields)}.");
                }
            }

        public void SetUser(SessionUser user)
            {
            if (user == null)
                {
                throw new ArgumentNullException(nameof(user));
                }

            var session = Session;
            if (session == null || !session.IsAvailable)
                {
                throw new SessionMissingException("Session is not available to persist the authenticated user.");
                }

            user.SessionId = session.Id;
            if (string.IsNullOrWhiteSpace(user.IPAddress))
                {
                user.IPAddress = _ipAddressProvider.GetLocalIpAddress();
                }
            if (string.IsNullOrWhiteSpace(user.MACAddress))
                {
                user.MACAddress = _ipAddressProvider.GetMACAddress();
                }
            if (string.IsNullOrWhiteSpace(user.FirstMACCardAddress))
                {
                user.FirstMACCardAddress = _ipAddressProvider.GetFirstMACCardAddress();
                }

            var serialized = JsonSerializer.Serialize(user, _serializerOptions);
            session.Remove("_sessionId");
            session.SetString(SessionKeys.User, serialized);
            session.SetInt32(SessionKeys.UserRole, user.UserRoleID);
            session.SetString(SessionKeys.IsSuperUser, user.UserRoleID == 1 ? "Y" : "N");
            IssueSessionStamp(user);
            }

        public void ClearUser()
            {
            if (Session == null || !Session.IsAvailable)
                {
                return;
                }

            if (TryGetUser(out var existingUser) && !string.IsNullOrWhiteSpace(existingUser.PPNumber))
                {
                ActiveSessionStamps.TryRemove(existingUser.PPNumber, out _);
                }

            Session.Remove(SessionKeys.User);
            Session.Remove("_sessionId");
            Session.Remove(SessionKeys.SbpAccessGranted);
            Session.Remove(SessionKeys.SessionStamp);
            Session.Remove(SessionKeys.AllowedViewIds);
            Session.Remove(SessionKeys.AllowedApiPaths);
            Session.Remove(SessionKeys.UserRole);
            Session.Remove(SessionKeys.IsSuperUser);
            }

        public void SetPageId(int pageId)
            {
            if (Session == null || !Session.IsAvailable)
                {
                return;
                }

            Session.SetInt32(SessionKeys.PageId, pageId);
            }

        public int GetPageId()
            {
            if (Session == null || !Session.IsAvailable)
                {
                return 0;
                }

            return Session.GetInt32(SessionKeys.PageId) ?? 0;
            }

        public void SetActiveEngagementId(int? engagementId)
            {
            if (Session == null || !Session.IsAvailable)
                {
                return;
                }

            if (!engagementId.HasValue || engagementId.Value <= 0)
                {
                Session.Remove(SessionKeys.ActiveEngagementId);
                return;
                }

            Session.SetInt32(SessionKeys.ActiveEngagementId, engagementId.Value);
            }

        public bool TryGetActiveEngagementId(out int engagementId)
            {
            engagementId = 0;
            if (Session == null || !Session.IsAvailable)
                {
                return false;
                }

            var stored = Session.GetInt32(SessionKeys.ActiveEngagementId);
            if (!stored.HasValue || stored.Value <= 0)
                {
                return false;
                }

            engagementId = stored.Value;
            return true;
            }

        public int GetActiveEngagementIdOrThrow()
            {
            if (!TryGetActiveEngagementId(out var engagementId))
                {
                throw new InvalidOperationException("Active engagement is required but not available in session.");
                }

            return engagementId;
            }

        public void ClearActiveEngagementId()
            {
            if (Session == null || !Session.IsAvailable)
                {
                return;
                }

            Session.Remove(SessionKeys.ActiveEngagementId);
            }

        public void CacheMenuPages(IEnumerable<MenuPagesModel> menuPages)
            {
            if (Session == null || !Session.IsAvailable || menuPages == null)
                {
                return;
                }

            var viewIds = new HashSet<int>();

            foreach (var page in menuPages.Where(p => p != null))
                {
                if (page.PageId > 0)
                    {
                    viewIds.Add(page.PageId);
                    }
                }

            var serializerSettings = _serializerOptions;
            Session.SetString(SessionKeys.AllowedViewIds, JsonSerializer.Serialize(viewIds, serializerSettings));
            }

        public void CacheApiPermissions(IEnumerable<ApiPermissionModel> apiPermissions)
            {
            if (Session == null || !Session.IsAvailable || apiPermissions == null)
                {
                return;
                }

            var allowedApiPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var pathBase = HttpContext?.Request?.PathBase.Value ?? string.Empty;

            foreach (var permission in apiPermissions)
                {
                if (permission == null || string.IsNullOrWhiteSpace(permission.ApiPath))
                    {
                    continue;
                    }

                var (method, rawPath) = SplitApiPath(permission.ApiPath);
                var normalizedPath = NormalizePermissionPath(pathBase, rawPath);
                if (string.IsNullOrWhiteSpace(normalizedPath))
                    {
                    continue;
                    }

                var resolvedMethod = !string.IsNullOrWhiteSpace(permission.HttpMethod)
                    ? permission.HttpMethod
                    : method;

                var key = BuildVerbQualifiedKey(resolvedMethod, normalizedPath);
                if (!string.IsNullOrWhiteSpace(key))
                    {
                    allowedApiPaths.Add(key);
                    }
                }

            Session.SetString(SessionKeys.AllowedApiPaths, JsonSerializer.Serialize(allowedApiPaths, _serializerOptions));
            }

        public bool TryGetAllowedViewIds(out HashSet<int> allowedViewIds)
            {
            allowedViewIds = new HashSet<int>();
            if (Session == null || !Session.IsAvailable)
                {
                return false;
                }

            var serialized = Session.GetString(SessionKeys.AllowedViewIds);
            if (string.IsNullOrWhiteSpace(serialized))
                {
                return false;
                }

            try
                {
                var deserialized = JsonSerializer.Deserialize<IEnumerable<int>>(serialized, _serializerOptions);
                foreach (var id in deserialized ?? Array.Empty<int>())
                    {
                    if (id > 0)
                        {
                        allowedViewIds.Add(id);
                        }
                    }

                return allowedViewIds.Count > 0;
                }
            catch (JsonException)
                {
                return false;
                }
            }

        public bool HasApiPermissionForPath(string method, string pathBase, string path)
            {
            if (Session == null || !Session.IsAvailable)
                {
                return false;
                }

            var serialized = Session.GetString(SessionKeys.AllowedApiPaths);
            if (string.IsNullOrWhiteSpace(serialized))
                {
                return false;
                }

            try
                {
                var normalizedPath = NormalizePermissionPath(pathBase, path);
                if (string.IsNullOrWhiteSpace(normalizedPath))
                    {
                    return false;
                    }

                var allowedApiPaths = JsonSerializer.Deserialize<HashSet<string>>(serialized, _serializerOptions);
                if (allowedApiPaths == null || allowedApiPaths.Count == 0)
                    {
                    return false;
                    }

                var key = BuildVerbQualifiedKey(method, normalizedPath);
                if (!string.IsNullOrWhiteSpace(key) && allowedApiPaths.Contains(key))
                    {
                    return true;
                    }
                }
            catch (JsonException)
                {
                }

            return false;
            }

        public bool HasCachedApiPermissions()
            {
            if (Session == null || !Session.IsAvailable)
                {
                return false;
                }

            var serialized = Session.GetString(SessionKeys.AllowedApiPaths);
            if (string.IsNullOrWhiteSpace(serialized))
                {
                return false;
                }

            try
                {
                var pathMap = JsonSerializer.Deserialize<HashSet<string>>(serialized, _serializerOptions);
                return pathMap != null && pathMap.Count > 0;
                }
            catch (JsonException)
                {
                return false;
                }
            }

        public int? GetCurrentUserRoleId()
            {
            if (Session == null || !Session.IsAvailable)
                {
                return null;
                }

            var storedRoleId = Session.GetInt32(SessionKeys.UserRole);
            if (storedRoleId.HasValue)
                {
                return storedRoleId.Value;
                }

            return TryGetUser(out var user) ? user.UserRoleID : (int?)null;
            }

        public bool IsSuperUser()
            {
            if (Session == null || !Session.IsAvailable)
                {
                return false;
                }

            var flag = Session.GetString(SessionKeys.IsSuperUser);
            if (!string.IsNullOrWhiteSpace(flag))
                {
                return string.Equals(flag, "Y", StringComparison.OrdinalIgnoreCase);
                }

            if (TryGetUser(out var user))
                {
                return user.UserRoleID == 1;
                }

            return false;
            }

        private string NormalizePath(string path)
            {
            return NormalizePermissionPath(HttpContext?.Request?.PathBase.Value ?? string.Empty, path);
            }

        private string NormalizePermissionPath(string pathBase, string path)
            {
            var combined = $"{pathBase ?? string.Empty}{path ?? string.Empty}";
            if (string.IsNullOrWhiteSpace(combined))
                {
                return string.Empty;
                }

            var normalized = combined
                .Trim()
                .TrimStart('~')
                .Replace("\\", "/")
                .Replace("//", "/");

            if (!normalized.StartsWith("/"))
                {
                normalized = $"/{normalized.TrimStart('/')}";
                }

            normalized = normalized.ToLowerInvariant();

            if (normalized.EndsWith("/") && normalized.Length > 1)
                {
                normalized = normalized.TrimEnd('/');
                }

            var normalizedBase = (pathBase ?? string.Empty)
                .Trim()
                .TrimStart('~')
                .Replace("\\", "/");

            if (!string.IsNullOrWhiteSpace(normalizedBase))
                {
                if (!normalizedBase.StartsWith("/"))
                    {
                    normalizedBase = $"/{normalizedBase.TrimStart('/')}";
                    }

                normalizedBase = normalizedBase.ToLowerInvariant();

                if (string.Equals(normalizedBase, "/", StringComparison.Ordinal))
                    {
                    normalizedBase = string.Empty;
                    }

                if (normalizedBase.EndsWith("/") && normalizedBase.Length > 1)
                    {
                    normalizedBase = normalizedBase.TrimEnd('/');
                    }

                if (!string.IsNullOrWhiteSpace(normalizedBase))
                    {
                    while (normalized.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
                        {
                        normalized = normalized.Substring(normalizedBase.Length);

                        if (string.IsNullOrWhiteSpace(normalized))
                            {
                            normalized = "/";
                            break;
                            }

                        if (!normalized.StartsWith("/"))
                            {
                            normalized = $"/{normalized.TrimStart('/')}";
                            }
                        }
                    }
                }

            return normalized;
            }

        private static (string method, string path) SplitApiPath(string apiPath)
            {
            if (string.IsNullOrWhiteSpace(apiPath))
                {
                return (string.Empty, string.Empty);
                }

            var segments = apiPath.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 2 && !segments[0].Contains("/"))
                {
                return (segments[0].Trim(), segments[1]);
                }

            return (string.Empty, apiPath.Trim());
            }

        private static string BuildVerbQualifiedKey(string method, string normalizedPath)
            {
            if (string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(normalizedPath))
                {
                return string.Empty;
                }

            var normalizedMethod = method.Trim();
            if (string.IsNullOrWhiteSpace(normalizedMethod))
                {
                return string.Empty;
                }

            return $"{normalizedMethod.ToUpperInvariant()}:{normalizedPath}";
            }

        public string GenerateRandomCryptographicSessionKey(int keyLength = 128)
            {
            Span<byte> randomBytes = stackalloc byte[keyLength];
            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
            }

        [Obsolete("Use ClearUser instead.")]
        public bool DisposeUserSession()
            {
            ClearUser();
            Session?.Clear();
            return true;
            }

        [Obsolete("Use GetUserOrThrow instead.")]
        public SessionUser GetSessionUser()
            {
            return TryGetUser(out var user) ? user : new SessionUser();
            }

        [Obsolete("Use controller helpers to validate authentication instead.")]
        public bool IsUserLoggedIn()
            {
            if (!TryGetUser(out var user))
                {
                return false;
                }

            if (TryCreateDbConnection(out var dbConnection))
                {
                try
                    {
                    return dbConnection.IsLoginSessionExist(user.PPNumber);
                    }
                catch
                    {
                    return false;
                    }
                }

            return false;
            }

        [Obsolete("Permissions should be resolved outside of the session handler.")]
        public bool HasPermissionToViewPage(string page_name)
            {
            if (string.IsNullOrWhiteSpace(page_name))
                {
                return false;
                }

            if (IsSuperUser())
                {
                return true;
                }

            if (!int.TryParse(page_name, out var parsedPageId))
                {
                return false;
                }

            return TryGetAllowedViewIds(out var allowedViewIds) && allowedViewIds.Contains(parsedPageId);
            }

        [Obsolete("Use SetUser with a SessionUser instance instead.")]
        public SessionUser SetSessionUser(UserModel user)
            {
            if (user == null)
                {
                throw new ArgumentNullException(nameof(user));
                }

            var sessionUser = new SessionUser
                {
                Email = user.Email,
                Name = user.Name,
                PPNumber = user.PPNumber,
                ID = user.ID,
                UserEntityName = user.UserEntityName,
                UserRoleName = user.UserRoleName,
                UserPostingAuditZone = user.UserPostingAuditZone,
                UserPostingBranch = user.UserPostingBranch,
                UserPostingDept = user.UserPostingDept,
                UserPostingDiv = user.UserPostingDiv,
                UserPostingZone = user.UserPostingZone,
                UserEntityID = user.UserEntityID,
                IsActive = user.IsActive,
                UserLocationType = user.UserLocationType,
                UserGroupID = Convert.ToInt32(user.UserGroupID),
                UserRoleID = Convert.ToInt32(user.UserRoleID)
                };

            SetUser(sessionUser);
            return sessionUser;
            }

        private bool TryGetRawValue(string key, out string value)
            {
            value = null;
            var session = Session;
            if (session == null || !session.IsAvailable)
                {
                return false;
                }

            value = session.GetString(key);
            if (!string.IsNullOrWhiteSpace(value))
                {
                return true;
                }

            value = session.GetString("_sessionId");
            return !string.IsNullOrWhiteSpace(value);
            }

        private bool TryCreateDbConnection(out DBConnection dbConnection)
            {
            dbConnection = null;
            var accessor = _httpContextAccessor;
            var configuration = _configurationRoot;
            if (accessor == null || configuration == null)
                {
                return false;
                }

            try
                {
                dbConnection = DBConnection.CreateFromHttpContext(accessor, configuration, this, _tokenService);
                return true;
                }
            catch
                {
                return false;
                }
            }

        public void GrantSbpAccess()
            {
            if (Session == null || !Session.IsAvailable)
                {
                throw new SessionMissingException("Session is not available to persist SBP access state.");
                }

            Session.SetString(SessionKeys.SbpAccessGranted, "Y");
            }

        public bool HasSbpAccess()
            {
            var value = Session?.GetString(SessionKeys.SbpAccessGranted);
            return string.Equals(value, "Y", StringComparison.OrdinalIgnoreCase);
            }

        public string IssueSessionStamp(SessionUser user)
            {
            if (user == null)
                {
                throw new ArgumentNullException(nameof(user));
                }

            var stamp = GenerateRandomCryptographicSessionKey(32);
            Session?.SetString(SessionKeys.SessionStamp, stamp);

            if (!string.IsNullOrWhiteSpace(user.PPNumber))
                {
                ActiveSessionStamps[user.PPNumber] = stamp;
                }

            return stamp;
            }

        public bool IsSessionRevoked(SessionUser user)
            {
            if (user == null || Session == null || !Session.IsAvailable)
                {
                return true;
                }

            var sessionStamp = Session.GetString(SessionKeys.SessionStamp);
            if (string.IsNullOrWhiteSpace(sessionStamp))
                {
                return true;
                }

            if (string.IsNullOrWhiteSpace(user.PPNumber))
                {
                return true;
                }

            if (!ActiveSessionStamps.TryGetValue(user.PPNumber, out var expectedStamp))
                {
                return true;
                }

            return !string.Equals(sessionStamp, expectedStamp, StringComparison.Ordinal);
            }
        }
    }
