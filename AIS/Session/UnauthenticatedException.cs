using System;

namespace AIS.Session
    {
    public class UnauthenticatedException : SessionMissingException
        {
        public const string DefaultMessage = "Your session has expired. Please sign in again.";

        public UnauthenticatedException()
            : base(DefaultMessage)
            {
            }

        public UnauthenticatedException(string message)
            : base(string.IsNullOrWhiteSpace(message) ? DefaultMessage : message)
            {
            }

        public UnauthenticatedException(string message, Exception innerException)
            : base(string.IsNullOrWhiteSpace(message) ? DefaultMessage : message, innerException)
            {
            }
        }
    }
