#nullable enable
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mvp.Foundation.CustomMiddleware.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Sitecore.LayoutService.Client;
using Sitecore.LayoutService.Client.Exceptions;
using Sitecore.LayoutService.Client.Request;
using Sitecore.LayoutService.Client.RequestHandlers;
using Sitecore.LayoutService.Client.Response;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.Services;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.Response;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.Param;
using Microsoft.Extensions.Configuration;
using Mvp.Foundation.CustomMiddleware.Rendering;

namespace Mvp.Foundation.CustomMiddleware.Handlers
{
    public class CustomHttpRequestHandler : ILayoutRequestHandler
    {
        private readonly ISitecoreLayoutSerializer _serializer;

        private readonly HttpClient _client;

        private readonly IOptionsSnapshot<HttpLayoutRequestHandlerOptions> _options;

        private readonly ILogger<CustomHttpRequestHandler> _logger;

        private readonly IOptions<SitecoreLayoutClientOptions> _layoutClientOptions;

        private readonly IOptionsSnapshot<SitecoreLayoutRequestOptions> _layoutRequestOptions;

        private readonly IGraphQLService<LayoutResponse> _service;

        private readonly IConfiguration _configuration;

        public CustomHttpRequestHandler(HttpClient client, ISitecoreLayoutSerializer serializer, IOptionsSnapshot<HttpLayoutRequestHandlerOptions> options, ILogger<CustomHttpRequestHandler> logger, IOptions<SitecoreLayoutClientOptions> layoutClientOptions, IOptionsSnapshot<SitecoreLayoutRequestOptions> layoutRequestOptions, IGraphQLService<LayoutResponse> service, IConfiguration configuration)
        {
            _client = client;
            _serializer = serializer;
            _options = options;
            _logger = logger;
            _layoutClientOptions = layoutClientOptions;
            _service = service;
            _configuration = configuration;
        }

        /// <summary>
        /// Request Handler to handle the Jss and Edge Requests
        /// </summary>
        /// <param name="request">Sitecore Layout Request obj</param>
        /// <param name="handlerName">Default handler name</param>
        /// <returns></returns>
        public async Task<SitecoreLayoutResponse> Request(SitecoreLayoutRequest request, string handlerName)
        {
            Stopwatch timer = Stopwatch.StartNew();
            SitecoreLayoutResponseContent content = null;
            string useEdgeEndPoint = _configuration.GetValue<string>(Constants.Configuration_Middleware_EndpointConfiguration_UseExperienceEdgeEndpoint);
            ILookup<string, string> metadata = null;
            object val = string.Empty;
            List<SitecoreLayoutServiceClientException> errors = new List<SitecoreLayoutServiceClientException>();           
            try
            {
                // If Foundation:Middleware:EndpointConfiguration:UseExperienceEdgeEndpoint is set to "true"
                // Makes the GraphQL Request to the Experience Edge Endpoint
                // It returns the Custom LayoutResponse and this then converted into SitecoreLayoutResponse
                if (bool.Parse(useEdgeEndPoint))
                {
                    var layoutResponse = await _service.Fetch(new ContextParam()
                    {
                        Language = Convert.ToString(request["sc_lang"]),
                        RoutePath = Convert.ToString(request["item"]),
                        Site = Convert.ToString(request["sc_site"])
                    });
                    SitecoreLayoutResponseContent rendered = new Sitecore.LayoutService.Client.Newtonsoft.NewtonsoftLayoutServiceSerializer().Deserialize(layoutResponse?.layout?.item?.rendered.ToString());
                    _logger.LogDebug($"Layout Service layoutResponse from Edge Endpoint : {JsonConvert.SerializeObject(rendered, Formatting.Indented)}");
                    timer.Stop();
                    return new SitecoreLayoutResponse(request, errors)
                    {
                        Content = rendered,
                        Metadata = metadata
                    };
                }

                HttpLayoutRequestHandlerOptions options = _options.Get(handlerName);
                HttpRequestMessage httpRequestMessage;
                try
                {
                    httpRequestMessage = BuildMessage(request, options);
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug($"Layout Service Http Request Message : {httpRequestMessage}");
                    }
                }
                catch (Exception ex)
                {
                    errors = AddError(errors, new SitecoreLayoutServiceMessageConfigurationException(ex));
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug($"An error configuring the HTTP message  : {ex}");
                    }
                    return new SitecoreLayoutResponse(request, errors);
                }
                HttpResponseMessage httpResponse = await GetResponseAsync(httpRequestMessage).ConfigureAwait(continueOnCapturedContext: false);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"Layout Service Http Response : {httpResponse}");
                }
                int responseStatusCode = (int)httpResponse.StatusCode;
                if (!httpResponse.IsSuccessStatusCode)
                {
                    int num = responseStatusCode;
                    List<SitecoreLayoutServiceClientException> list;
                    if (responseStatusCode == 404)
                    {
                        list = AddError(errors, new ItemNotFoundSitecoreLayoutServiceClientException(), responseStatusCode);
                    }
                    else
                    {
                        list = ((responseStatusCode >= 400 && responseStatusCode < 500) ? AddError(errors, new InvalidRequestSitecoreLayoutServiceClientException(), responseStatusCode) : ((responseStatusCode < 500) ? AddError(errors, new SitecoreLayoutServiceClientException(), responseStatusCode) : AddError(errors, new InvalidResponseSitecoreLayoutServiceClientException(new SitecoreLayoutServiceServerException()), responseStatusCode)));
                    }
                    errors = list;
                }
                if (httpResponse.IsSuccessStatusCode || httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    try
                    {
                        string text = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
                        string newone = await Task.Run(() =>
                            newone = text
                        );
                        content = _serializer.Deserialize(text);
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            object arg = JsonConvert.DeserializeObject(text);
                            _logger.LogDebug($"Layout Service Response JSON : {arg}");
                        }
                    }
                    catch (Exception innerException)
                    {
                        errors = AddError(errors, new InvalidResponseSitecoreLayoutServiceClientException(innerException), responseStatusCode);
                    }
                }
                try
                {
                    metadata = httpResponse.Headers.SelectMany((KeyValuePair<string, IEnumerable<string>> x) => x.Value.Select((string y) => new
                    {
                        Key = x.Key,
                        Value = y
                    })).ToLookup(k => k.Key, v => v.Value);
                }
                catch (Exception innerException2)
                {
                    errors = AddError(errors, new InvalidResponseSitecoreLayoutServiceClientException(innerException2), responseStatusCode);
                }
            }
            catch (Exception innerException3)
            {
                errors.Add(new CouldNotContactSitecoreLayoutServiceClientException(innerException3));
            }
            timer.Stop();
            _logger.LogDebug($"Total Execution Time : {timer.ElapsedMilliseconds}");
            return new SitecoreLayoutResponse(request, errors)
            {
                Content = content,
                Metadata = metadata
            };
        }

        protected virtual HttpRequestMessage BuildMessage(SitecoreLayoutRequest request, HttpLayoutRequestHandlerOptions options)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, _client.BaseAddress);
            if (options != null)
            {
                foreach (Action<SitecoreLayoutRequest, HttpRequestMessage> item in options.RequestMap)
                {
                    item(request, httpRequestMessage);
                }
                return httpRequestMessage;
            }
            return httpRequestMessage;
        }

        protected virtual async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage message)
        {
            return await _client.SendAsync(message).ConfigureAwait(continueOnCapturedContext: false);
        }

        private static List<SitecoreLayoutServiceClientException> AddError(List<SitecoreLayoutServiceClientException> errors, SitecoreLayoutServiceClientException error, int statusCode = 0)
        {
            if (statusCode > 0)
            {
                error.Data.Add("HttpStatusCode_KeyName", statusCode);
            }
            errors.Add(error);
            return errors;
        }


        private SitecoreLayoutRequestOptions MergeLayoutRequestOptions(string handlerName)
        {
            SitecoreLayoutRequestOptions value = _layoutRequestOptions.Value;
            SitecoreLayoutRequestOptions sitecoreLayoutRequestOptions = _layoutRequestOptions.Get(handlerName);
            if (AreEqual(value.RequestDefaults, sitecoreLayoutRequestOptions.RequestDefaults))
            {
                return value;
            }
            SitecoreLayoutRequestOptions sitecoreLayoutRequestOptions2 = value;
            SitecoreLayoutRequest requestDefaults = value.RequestDefaults;
            SitecoreLayoutRequest requestDefaults2 = sitecoreLayoutRequestOptions.RequestDefaults;
            foreach (KeyValuePair<string, object> item in requestDefaults2)
            {
                if (requestDefaults.ContainsKey(item.Key))
                {
                    requestDefaults[item.Key] = requestDefaults2[item.Key];
                }
                else
                {
                    requestDefaults.Add(item.Key, requestDefaults2[item.Key]);
                }
            }
            sitecoreLayoutRequestOptions2.RequestDefaults = requestDefaults;
            return sitecoreLayoutRequestOptions2;
        }


        private static bool AreEqual(IDictionary<string, object?> dictionary1, IDictionary<string, object?> dictionary2)
        {
            if (dictionary1.Count != dictionary2.Count)
            {
                return false;
            }
            foreach (string key in dictionary1.Keys)
            {
                if (!dictionary2.TryGetValue(key, out var value) || dictionary1[key] != value)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
