using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Services.Scheduler
{
    public sealed class IasSchedulerJobHandlerRegistry : IIasSchedulerJobHandlerRegistry
    {
        private readonly IReadOnlyDictionary<string, IIasSchedulerJobHandler> handlers;

        public IasSchedulerJobHandlerRegistry(IEnumerable<IIasSchedulerJobHandler> handlers)
        {
            this.handlers = (handlers ?? Enumerable.Empty<IIasSchedulerJobHandler>())
                .Where(handler => !string.IsNullOrWhiteSpace(handler.ApplicationHandler))
                .ToDictionary(handler => handler.ApplicationHandler.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetHandler(string applicationHandler, out IIasSchedulerJobHandler handler)
        {
            if (string.IsNullOrWhiteSpace(applicationHandler))
            {
                handler = null;
                return false;
            }

            return handlers.TryGetValue(applicationHandler.Trim(), out handler);
        }
    }
}
