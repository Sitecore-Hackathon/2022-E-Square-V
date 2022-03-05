#nullable enable
using GraphQL;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Mvp.Foundation.GraphQLEdgeConnector.Rendering.GraphQLFactory
{
    public class GraphQLProviderMiddleware : IGraphQLProviderMiddleware
    {
        private readonly ILogger<IGraphQLProviderMiddleware> _logger;
        private readonly GraphQLRequestBuilderMiddleware _graphQLRequestBuilder;
        private readonly GraphQLClientFactoryMiddleware _graphQLClientFactory;
        private readonly bool _isDevelopment;

        public GraphQLProviderMiddleware(ILogger<IGraphQLProviderMiddleware> logger, GraphQLRequestBuilderMiddleware graphQLRequestBuilder, GraphQLClientFactoryMiddleware graphQLClientFactory)
        {
            _logger = logger;
            _graphQLRequestBuilder = graphQLRequestBuilder ?? throw new ArgumentNullException(nameof(graphQLRequestBuilder));
            _graphQLClientFactory = graphQLClientFactory ?? throw new ArgumentNullException(nameof(graphQLClientFactory));
            _isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        }


        public Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(bool? isEditMode, GraphQLFiles queryFile, dynamic? variables) where TResponse : class
        {
            var client = isEditMode.HasValue && isEditMode.Value ? _graphQLClientFactory.CreateEditClient() : _graphQLClientFactory.CreateLiveClient();

            GraphQLRequest request = _graphQLRequestBuilder.BuildQuery(queryFile, variables);

            var result = Task.Run(async () => await client.SendQueryAsync<TResponse>(request));

            if (_isDevelopment)
            {
                _logger.LogInformation(JsonConvert.SerializeObject(result.Result.Data, Formatting.Indented));
            }
            return result;
        }
    }
}
