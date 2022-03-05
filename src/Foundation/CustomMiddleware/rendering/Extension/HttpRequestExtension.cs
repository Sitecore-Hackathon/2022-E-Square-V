using Microsoft.AspNetCore.Http;
using Sitecore.LayoutService.Client.Request;

namespace Mvp.Foundation.CustomMiddleware.Extension
{
    public static class HttpRequestExtension
    {
        public static string GenerateCacheKeyForHttpRequest(this SitecoreLayoutRequest request)
        {
            string format = "{0}-{1}-{2}";
            object item = string.Empty;
            object lang = string.Empty;
            object site = string.Empty;

            if (request.TryGetValue("item", out item)
                && request.TryGetValue("sc_lang", out lang)
                && request.TryGetValue("sc_site", out site))
                return string.Format(format, site, item, lang).ToLower().Replace("/", "-");
            return string.Empty;
        }
    }
}
