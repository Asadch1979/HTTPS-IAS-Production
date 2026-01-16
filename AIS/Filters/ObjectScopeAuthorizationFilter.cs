using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIS.Middleware;
using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace AIS.Filters
    {
    /// <summary>
    ///     Enforces object-level authorization for actions that accept identifiers.
    ///     The filter runs before any database interaction and returns 403 when the
    ///     requested identifier falls outside the authenticated user's scope.
    /// </summary>
    public class ObjectScopeAuthorizationFilter : IAsyncActionFilter
        {
        private static readonly HashSet<string> UserIdentifierNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "user_id",
                "userid",
                "ppnumber"
            };

        private readonly SessionHandler _sessionHandler;
        private readonly IObjectScopeAuthorizer _scopeAuthorizer;
        private readonly ILogger<ObjectScopeAuthorizationFilter> _logger;

        public ObjectScopeAuthorizationFilter(
            SessionHandler sessionHandler,
            IObjectScopeAuthorizer scopeAuthorizer,
            ILogger<ObjectScopeAuthorizationFilter> logger)
            {
            _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
            _scopeAuthorizer = scopeAuthorizer ?? throw new ArgumentNullException(nameof(scopeAuthorizer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
            if (context == null)
                {
                throw new ArgumentNullException(nameof(context));
                }
            if (next == null)
                {
                throw new ArgumentNullException(nameof(next));
                }

            var identifiers = ExtractIdentifiers(context);
            if (identifiers.Count == 0)
                {
                await next();
                return;
                }

            if (!_sessionHandler.TryGetUser(out var user) || user == null)
                {
                context.Result = LoginRedirectHelper.IsApiRequest(context.HttpContext.Request)
                    ? BuildApiResult(StatusCodes.Status401Unauthorized, "Session missing or expired.")
                    : new StatusCodeResult(StatusCodes.Status401Unauthorized);
                return;
                }

            if (_sessionHandler.IsSuperUser())
                {
                await next();
                return;
                }

            if (_sessionHandler.IsSessionRevoked(user))
                {
                _logger.LogWarning("Session stamp mismatch for user {PPNumber}; rejecting request before DB access.", user.PPNumber);
                context.Result = LoginRedirectHelper.IsApiRequest(context.HttpContext.Request)
                    ? BuildApiResult(StatusCodes.Status401Unauthorized, "Session is no longer valid.")
                    : new StatusCodeResult(StatusCodes.Status401Unauthorized);
                return;
                }

            _logger.LogInformation("Object-level authorization delegated to database (validated via R_ID and PP_NO).");

            // OPTION A: Skip object-scope authorization for API requests
            // that only contain USER-scoped identifiers.
            // DB already enforces R_ID + PP_NO.
            if (LoginRedirectHelper.IsApiRequest(context.HttpContext.Request) &&
                identifiers.All(i => i.Scope == IdentifierScope.User))
                {
                await next();
                return;
                }

            var decision = _scopeAuthorizer.IsAuthorized(user, identifiers);
            if (!decision.IsAuthorized)
                {
                LogDenial(context, identifiers, decision);

                if (LoginRedirectHelper.IsApiRequest(context.HttpContext.Request))
                    {
                    context.Result = BuildApiResult(decision.Result as StatusCodeResult, decision.Reason);
                    }
                else
                    {
                    context.Result = decision.Result ?? new StatusCodeResult(StatusCodes.Status403Forbidden);
                    }
                return;
                }

            await next();
            }

        private static IReadOnlyList<ScopedIdentifier> ExtractIdentifiers(ActionExecutingContext context)
            {
            var identifiers = new List<ScopedIdentifier>();

            foreach (var argument in context.ActionArguments)
                {
                if (TryCreateIdentifier(argument.Key, argument.Value, out var identifier))
                    {
                    identifiers.Add(identifier);
                    }
                }

            // Also inspect route values in case model binding did not populate ActionArguments (e.g., optional parameters).
            foreach (var routeValue in context.RouteData.Values)
                {
                var key = routeValue.Key;
                if (context.ActionArguments.ContainsKey(key))
                    {
                    continue;
                    }

                if (TryCreateIdentifier(key, routeValue.Value, out var identifier))
                    {
                    identifiers.Add(identifier);
                    }
                }

            return identifiers;
            }

        private static bool TryCreateIdentifier(string key, object value, out ScopedIdentifier identifier)
            {
            identifier = null;
            if (string.IsNullOrWhiteSpace(key) || value == null)
                {
                return false;
                }

            if (!TryConvertToLong(value, out var numeric) || numeric <= 0)
                {
                return false;
                }

            var normalizedKey = NormalizeKey(key);

            if (IsUserIdentifier(normalizedKey))
                {
                identifier = new ScopedIdentifier(key, numeric, IdentifierScope.User);
                return true;
                }

            return false;
            }

        private static string NormalizeKey(string key)
            {
            return (key ?? string.Empty).Trim().ToLowerInvariant();
            }

        private static bool IsUserIdentifier(string normalizedKey)
            {
            return UserIdentifierNames.Contains(normalizedKey);
            }

        private static bool TryConvertToLong(object value, out long numeric)
            {
            switch (value)
                {
                case null:
                    numeric = 0;
                    return false;
                case long l:
                    numeric = l;
                    return true;
                case int i:
                    numeric = i;
                    return true;
                case short s:
                    numeric = s;
                    return true;
                case string str when long.TryParse(str, out var parsed):
                    numeric = parsed;
                    return true;
                default:
                    numeric = 0;
                    return false;
                }
            }

        private void LogDenial(ActionExecutingContext context, IReadOnlyCollection<ScopedIdentifier> identifiers, ScopeDecision decision)
            {
            if (_logger.IsEnabled(LogLevel.Warning))
                {
                var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = descriptor?.ActionName ?? string.Empty;
                var controllerName = descriptor?.ControllerName ?? string.Empty;
                var idList = string.Join(", ", identifiers.Select(i => $"{i.Name}:{i.Value}"));
                _logger.LogWarning(
                    "IDOR enforcement rejected access on {Controller}/{Action} for identifiers [{Identifiers}]. Reason: {Reason}",
                    controllerName,
                    actionName,
                    idList,
                    decision.Reason ?? "Unknown");
                }
            }

        private static IActionResult BuildApiResult(int statusCode, string reason)
            {
            var errorCode = statusCode == StatusCodes.Status401Unauthorized ? "unauthorized" : "forbidden";

            return new JsonResult(new
                {
                error = errorCode,
                message = string.IsNullOrWhiteSpace(reason) ? "Access denied." : reason
                })
                {
                StatusCode = statusCode
                };
            }

        private static IActionResult BuildApiResult(StatusCodeResult existingResult, string reason)
            {
            var statusCode = existingResult?.StatusCode ?? StatusCodes.Status403Forbidden;
            return BuildApiResult(statusCode, reason);
            }
        }
    }
