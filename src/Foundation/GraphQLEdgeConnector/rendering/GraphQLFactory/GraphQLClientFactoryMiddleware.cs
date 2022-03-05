using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace Mvp.Foundation.GraphQLEdgeConnector.Rendering.GraphQLFactory
{
    public class GraphQLClientFactoryMiddleware
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public GraphQLClientFactoryMiddleware(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private IGraphQLClient LiveClient { get; set; }

        private IGraphQLClient EditClient { get; set; }


        public IGraphQLClient CreateLiveClient()
        {
            return LiveClient ??= CreateGraphQLClient("Foundation:GraphQLEdgeConnector:GraphQL:UrlLive");
        }

        public IGraphQLClient CreateEditClient()
        {
            return EditClient ??= CreateGraphQLClient("Foundation:GraphQLEdgeConnector:GraphQL:UrlEdit");
        }

        private IGraphQLClient CreateGraphQLClient(string configurationKeyUrlLiveOrEditMode)
        {
            GraphQLHttpClientOptions graphQLHttpClientOptions = new GraphQLHttpClientOptions()
            {
                EndPoint = new Uri(
                    $"{_configuration.GetValue<string>(configurationKeyUrlLiveOrEditMode)}?sc_apikey={_configuration.GetValue<string>("Sitecore:ApiKey")}"),
            };
            var options = new JsonSerializerSettings();
            options.StringEscapeHandling = StringEscapeHandling.Default;
            options.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            options.MissingMemberHandling = MissingMemberHandling.Ignore;
            options.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            {
                NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
            };
            return new GraphQLHttpClient(graphQLHttpClientOptions, new NewtonsoftJsonSerializer(options), _httpClient);
        }
    }
}
