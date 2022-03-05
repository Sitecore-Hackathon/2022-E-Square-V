namespace Mvp.Foundation.Caching.Providers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Newtonsoft.Json;
    using Sitecore.LayoutService.Client;

    public class DistributedCachingProvider : ICachingProvider
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ISitecoreLayoutSerializer _layoutSerializer;

        public DistributedCachingProvider(IDistributedCache distributedCache, ISitecoreLayoutSerializer layoutSerializer)
        {
            _distributedCache = distributedCache;
            _layoutSerializer = layoutSerializer;
        }

        public async Task<T> GetFromCacheAsync<T>(string key) where T : class
        {
            var cachedValue = await _distributedCache.GetStringAsync(key);
            return cachedValue != null ? JsonConvert.DeserializeObject<T>(cachedValue) : null;
        }

        public T GetFromCache<T>(string key) where T : class
        {
            var cachedValue = _distributedCache.GetString(key);
            return cachedValue != null ? JsonConvert.DeserializeObject<T>(cachedValue) : null;
        }

        public async Task SetCache<T>(string key, T value, DistributedCacheEntryOptions options = null) where T : class
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            options ??= new DistributedCacheEntryOptions()
            { AbsoluteExpiration = DateTimeOffset.Parse(DateTime.Now.AddDays(1).ToString()) };
            await _distributedCache.SetStringAsync(key, serializedValue, options);
        }

        public async Task ClearCache(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        /// <summary>
        /// Get the cached value from the Distributed cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>Cached values corresponds to the key passed</returns>
        public string GetCacheValue(string key)
        {
            return _distributedCache.GetString(key);
        }

        /// <summary>
        /// Set the input string to Distributed Cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache Value</param>
        /// <param name="options">Cache entry options</param>
        public void SetCacheAsString(string key, string value, DistributedCacheEntryOptions options = null)
        {
            options ??= new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Parse(DateTime.Now.AddDays(1).ToString())
            };
            _distributedCache.SetString(key, value, options);
        }
    }
}