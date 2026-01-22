using System;
using AIS.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AIS.Services
    {
    public class PasswordChangeStateStore
        {
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);
        private readonly IMemoryCache _memoryCache;

        public PasswordChangeStateStore(IMemoryCache memoryCache)
            {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            }

        public void Store(string token, UserModel user)
            {
            if (string.IsNullOrWhiteSpace(token) || user == null)
                {
                return;
                }

            var entry = new PasswordChangeState(user);
            _memoryCache.Set(BuildCacheKey(token), entry, new MemoryCacheEntryOptions
                {
                AbsoluteExpirationRelativeToNow = DefaultTtl
                });
            }

        public bool TryGet(string token, out PasswordChangeState state)
            {
            state = null;
            if (string.IsNullOrWhiteSpace(token))
                {
                return false;
                }

            return _memoryCache.TryGetValue(BuildCacheKey(token), out state);
            }

        public void Remove(string token)
            {
            if (string.IsNullOrWhiteSpace(token))
                {
                return;
                }

            _memoryCache.Remove(BuildCacheKey(token));
            }

        private static string BuildCacheKey(string token)
            {
            return $"pwd-change:{token}";
            }
        }

    public sealed record PasswordChangeState(UserModel User);
    }
