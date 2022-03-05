using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mvp.Foundation.Caching.Providers;
using Sitecore.LayoutService.Client;
using System;

namespace Mvp.Foundation.Caching
{
    public static class ServiceCollectionExtension
    {
        public static void AddCachingServices(this IServiceCollection services)
        {
            services.AddSingleton<IDistributedCache, RedisCache>();
            services.AddSingleton<ICachingProvider, DistributedCachingProvider>();
        }
    }
}