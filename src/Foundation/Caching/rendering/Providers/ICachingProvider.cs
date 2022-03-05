using System;
using System.Collections.Generic;
using System.Text;

namespace Mvp.Foundation.Caching.Providers
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Sitecore.LayoutService.Client.Response;

    public interface ICachingProvider
    {
        Task<T> GetFromCacheAsync<T>(string key) where T : class;
        T GetFromCache<T>(string key) where T : class;
        Task SetCache<T>(string key, T value, DistributedCacheEntryOptions options = null) where T : class;
        Task ClearCache(string key);
        string GetCacheValue(string key);
        void SetCacheAsString(string key, string value, DistributedCacheEntryOptions options = null);
    }
}