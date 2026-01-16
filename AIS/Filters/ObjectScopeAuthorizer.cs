using System;
using System.Collections.Generic;
using AIS.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIS.Filters
    {
    public interface IObjectScopeAuthorizer
        {
        ScopeDecision IsAuthorized(SessionUser user, IReadOnlyCollection<ScopedIdentifier> identifiers);
        }

    public sealed class ObjectScopeAuthorizer : IObjectScopeAuthorizer
        {
        public ScopeDecision IsAuthorized(SessionUser user, IReadOnlyCollection<ScopedIdentifier> identifiers)
            {
            if (user == null)
                {
                return ScopeDecision.Deny("No session user present", new StatusCodeResult(401));
                }

            if (identifiers == null || identifiers.Count == 0)
                {
                return ScopeDecision.Allow();
                }

            foreach (var identifier in identifiers)
                {
                switch (identifier.Scope)
                    {
                    case IdentifierScope.User:
                        if (!IsUserMatch(user, identifier.Value))
                            {
                            return ScopeDecision.Deny($"User mismatch for {identifier.Name}");
                            }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                    }
                }

            return ScopeDecision.Allow();
            }

        private static bool IsUserMatch(SessionUser user, long userIdentifier)
            {
            return userIdentifier == user.ID || (user.UserEntityID.HasValue && user.UserEntityID.Value == userIdentifier);
            }
        }

    public sealed class ScopeDecision
        {
        private ScopeDecision(bool isAuthorized, string reason, IActionResult result = null)
            {
            IsAuthorized = isAuthorized;
            Reason = reason;
            Result = result;
            }

        public bool IsAuthorized { get; }
        public string Reason { get; }
        public IActionResult Result { get; }

        public static ScopeDecision Allow()
            {
            return new ScopeDecision(true, null);
            }

        public static ScopeDecision Deny(string reason, IActionResult result = null)
            {
            return new ScopeDecision(false, reason, result);
            }
        }

    public sealed class ScopedIdentifier
        {
        public ScopedIdentifier(string name, long value, IdentifierScope scope)
            {
            Name = name;
            Value = value;
            Scope = scope;
            }

        public string Name { get; }
        public long Value { get; }
        public IdentifierScope Scope { get; }
        }

    public enum IdentifierScope
        {
        User
        }
    }
