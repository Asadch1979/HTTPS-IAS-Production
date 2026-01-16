using Microsoft.AspNetCore.Http;

namespace AIS.Services
    {
    public interface IPageIdResolver
        {
        int ResolvePageId(HttpContext httpContext);
        int ResolvePageId(string requestPath);
        bool TryResolvePageId(HttpContext httpContext, out int pageId);
        bool TryResolvePageId(string requestPath, out int pageId);
        }
    }
