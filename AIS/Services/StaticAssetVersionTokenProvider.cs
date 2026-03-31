using AIS.Models;
using AIS.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Services
    {
    public class StaticAssetVersionTokenProvider : IStaticAssetVersionTokenProvider
        {
        private const string CacheKey = "AIS.StaticAssetVersionToken";
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DBConnection _dbConnection;

        public StaticAssetVersionTokenProvider(
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor,
            DBConnection dbConnection)
            {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            }

        public string GetToken()
            {
            if (_cache.TryGetValue(CacheKey, out string cachedToken))
                {
                return cachedToken ?? string.Empty;
                }

            if (!CanResolveFromCurrentRequest())
                {
                return string.Empty;
                }

            var token = ResolveTokenFromDatabase();
            _cache.Set(
                CacheKey,
                token ?? string.Empty,
                new MemoryCacheEntryOptions
                    {
                    SlidingExpiration = TimeSpan.FromMinutes(30),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
                    });

            return token ?? string.Empty;
            }

        public void Invalidate()
            {
            _cache.Remove(CacheKey);
            }

        private bool CanResolveFromCurrentRequest()
            {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
            }

        private string ResolveTokenFromDatabase()
            {
            var versions = _dbConnection.GetAllVersionHistory();
            if (versions == null || versions.Count == 0)
                {
                return string.Empty;
                }

            var selected = SelectCurrentVersion(versions);
            return NormalizeToken(selected?.VersionNo);
            }

        private static VersionHistoryModel SelectCurrentVersion(IReadOnlyCollection<VersionHistoryModel> versions)
            {
            return versions
                .OrderByDescending(v => string.Equals(v?.IsActive, "Y", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(v => v?.UpdatedOn ?? v?.CreatedOn ?? v?.ReleaseDate ?? DateTime.MinValue)
                .ThenByDescending(v => v?.VersionId ?? 0)
                .FirstOrDefault();
            }

        private static string NormalizeToken(string token)
            {
            return string.IsNullOrWhiteSpace(token) ? string.Empty : token.Trim();
            }
        }
    }
