using System;

namespace AIS.Session
    {
    public class SessionMissingException : Exception
        {
        public SessionMissingException()
            {
            }

        public SessionMissingException(string message)
            : base(message)
            {
            }

        public SessionMissingException(string message, Exception innerException)
            : base(message, innerException)
            {
            }
        }
    }
