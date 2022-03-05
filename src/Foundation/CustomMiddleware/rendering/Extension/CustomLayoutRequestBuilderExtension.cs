using Microsoft.Extensions.DependencyInjection;
using Mvp.Foundation.CustomMiddleware.Handlers;
using Sitecore.LayoutService.Client;
using Sitecore.LayoutService.Client.Extensions;
using Sitecore.LayoutService.Client.Request;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Mvp.Foundation.CustomMiddleware.Rendering.Extension
{
    public static class CustomLayoutRequestBuilderExtension
    {
        /// <summary>
        /// Registers a CustomHttpHandler for the layout service client
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="handlerName"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ILayoutRequestHandlerBuilder<CustomHttpRequestHandler> AddCustomLayoutRequestHandler(
            this ISitecoreLayoutClientBuilder builder,
            string handlerName,
            Uri url)
        {
            return builder.AddHttpHandler(handlerName, url);
        }

        /// <summary>
        /// Sets the LayoutRequest address
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="handlerName"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static ILayoutRequestHandlerBuilder<CustomHttpRequestHandler> AddHttpHandler(this ISitecoreLayoutClientBuilder builder, string handlerName, Uri uri)
        {
            Uri uriInternal = uri;
            return builder.AddHttpHandler(handlerName, delegate (HttpClient client)
            {
                client.BaseAddress = uriInternal;
            });
        }

        /// <summary>
        /// Registers the HttpClient
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="handlerName"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ILayoutRequestHandlerBuilder<CustomHttpRequestHandler> AddHttpHandler(this ISitecoreLayoutClientBuilder builder, string handlerName, Action<HttpClient> configure)
        {
            string handlerNameInternal = handlerName;
            builder.Services.AddHttpClient(handlerNameInternal, configure).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseCookies = false
            });
            return builder.AddHttpHandler(handlerNameInternal, (IServiceProvider sp) => sp.GetRequiredService<IHttpClientFactory>().CreateClient(handlerNameInternal));
        }

        /// <summary>
        /// Registers a HTTP request handler for the Sitecore layout service client.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="handlerName"></param>
        /// <param name="resolveClient"></param>
        /// <returns></returns>
        public static ILayoutRequestHandlerBuilder<CustomHttpRequestHandler> AddHttpHandler(this ISitecoreLayoutClientBuilder builder, string handlerName, Func<IServiceProvider, HttpClient> resolveClient)
        {
            Func<IServiceProvider, HttpClient> resolveClientInternal = resolveClient;
            ILayoutRequestHandlerBuilder<CustomHttpRequestHandler> layoutRequestHandlerBuilder = builder.AddHandler(handlerName, delegate (IServiceProvider sp)
            {
                HttpClient httpClient = resolveClientInternal(sp);
                return ActivatorUtilities.CreateInstance<CustomHttpRequestHandler>(sp, new object[1] { httpClient });
            });
            layoutRequestHandlerBuilder.MapFromRequest(delegate (SitecoreLayoutRequest request, HttpRequestMessage message)
            {
                message.RequestUri = request.BuildDefaultSitecoreLayoutRequestUri(message.RequestUri);
                if (request.TryReadValue<string>("sc_auth_header_key", out var value))
                {
                    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", value);
                }
                if (request.TryGetHeadersCollection(out var headers))
                {
                    foreach (KeyValuePair<string, string[]> item in headers)
                    {
                        message.Headers.Add(item.Key, item.Value);
                    }
                }
            });
            return layoutRequestHandlerBuilder;
        }
        /// <summary>
        /// Registers the HttpLayoutRequestHandlerOptions 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureHttpRequestMessage"></param>
        /// <returns></returns>
        public static ILayoutRequestHandlerBuilder<CustomHttpRequestHandler> MapFromRequest(this ILayoutRequestHandlerBuilder<CustomHttpRequestHandler> builder, Action<SitecoreLayoutRequest, HttpRequestMessage> configureHttpRequestMessage)
        {
            Action<SitecoreLayoutRequest, HttpRequestMessage> configureHttpRequestMessageInternal = configureHttpRequestMessage;
            builder.Services.Configure(builder.HandlerName, delegate (HttpLayoutRequestHandlerOptions options)
            {
                options.RequestMap.Add(configureHttpRequestMessageInternal);
            });
            return builder;
        }
    }
}
