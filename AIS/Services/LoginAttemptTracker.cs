using AIS.Models;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace AIS.Services
{
    public class LoginAttemptTracker
    {
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);
        private const int MaxFailedAttempts = 10;

        private readonly IMemoryCache _memoryCache;

        public LoginAttemptTracker(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public UserModel EvaluateRateLimit(LoginModel login, string ipAddress)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.PPNumber))
            {
                return null;
            }

            var cacheKey = BuildRateLimitKey(login.PPNumber, ipAddress);
            if (_memoryCache.TryGetValue(cacheKey, out LoginThrottleState state))
            {
                var now = DateTimeOffset.UtcNow;
                if (state.LockoutUntil.HasValue && state.LockoutUntil.Value > now)
                {
                    var retryAfter = (int)Math.Ceiling((state.LockoutUntil.Value - now).TotalSeconds);
                    var minutes = (int)Math.Ceiling(retryAfter / 60.0);
                    return new UserModel
                    {
                        isAuthenticate = false,
                        isAlreadyLoggedIn = false,
                        ErrorCode = "LOCKED_OUT",
                        RetryAfterSeconds = retryAfter,
                        ErrorTitle = "Too many attempts",
                        ErrorMsg = minutes > 0
                            ? $"For security, sign-in is temporarily blocked. Please try again in {minutes} minute(s)."
                            : "For security, sign-in is temporarily blocked. Please try again later."
                    };
                }

                if (state.FirstAttempt + RateLimitWindow < now)
                {
                    _memoryCache.Remove(cacheKey);
                }
            }

            return null;
        }

        public void RegisterFailedAttempt(LoginModel login, string ipAddress)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.PPNumber))
            {
                return;
            }

            var cacheKey = BuildRateLimitKey(login.PPNumber, ipAddress);
            var now = DateTimeOffset.UtcNow;
            var state = _memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = RateLimitWindow + LockoutDuration;
                return new LoginThrottleState { FirstAttempt = now };
            });

            state.FailedAttempts++;
            if (state.FailedAttempts >= MaxFailedAttempts)
            {
                state.LockoutUntil = now.Add(LockoutDuration);
            }

            _memoryCache.Set(cacheKey, state, state.LockoutUntil.HasValue ? state.LockoutUntil.Value - now : RateLimitWindow);
        }

        public void ResetAttempts(LoginModel login, string ipAddress)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.PPNumber))
            {
                return;
            }

            ResetAttempts(login.PPNumber, ipAddress);
        }

        public void ResetAttempts(string ppNumber, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ppNumber))
            {
                return;
            }

            var cacheKey = BuildRateLimitKey(ppNumber, ipAddress);
            _memoryCache.Remove(cacheKey);
        }

        private static string BuildRateLimitKey(string ppNumber, string ipAddress)
        {
            var ip = string.IsNullOrWhiteSpace(ipAddress) ? "unknown" : ipAddress;
            return $"login-throttle:{ppNumber}:{ip}";
        }

        private class LoginThrottleState
        {
            public DateTimeOffset FirstAttempt { get; set; }
            public int FailedAttempts { get; set; }
            public DateTimeOffset? LockoutUntil { get; set; }
        }
    }
}
