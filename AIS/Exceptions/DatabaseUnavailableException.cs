using System;

namespace AIS.Exceptions
{
    /// <summary>
    /// Represents an error that occurs when a database connection cannot be established.
    /// </summary>
    public class DatabaseUnavailableException : Exception
    {
        public DatabaseUnavailableException(string message)
            : base(message)
        {
        }

        public DatabaseUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
