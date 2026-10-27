using System;

namespace AIS.Filters
{
    /// <summary>
    /// Opts a form action into rebuilding its originating view on invalid ModelState.
    /// The action MUST reject invalid data before performing any writes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HandlesInvalidPostModelAttribute : Attribute { }
}
