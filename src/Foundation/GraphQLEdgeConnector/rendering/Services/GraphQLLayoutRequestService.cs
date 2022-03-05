using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.GraphQLFactory;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.Param;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.Response;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Mvp.Foundation.GraphQLEdgeConnector.Rendering.Services
{
    public class GraphQlLayoutRequestService : IGraphQLService<LayoutResponse>
    {
        private readonly IConfiguration _configuration;
        private readonly IGraphQLProviderMiddleware _graphQLProvider;
        private readonly ILogger<GraphQlLayoutRequestService> _logger;


        public GraphQlLayoutRequestService(IConfiguration configuration, IGraphQLProviderMiddleware graphQLProvider, ILogger<GraphQlLayoutRequestService> logger)
        {
            _configuration = configuration;
            _graphQLProvider = graphQLProvider;
            _logger = logger;
        }

        public async Task<LayoutResponse> Fetch(ContextParam contextParam)
        {
            LayoutResponse response = (await _graphQLProvider.SendQueryAsync<LayoutResponse>(false, GraphQLFiles.LayoutRequest, new
            {
                language = contextParam.Language,
                routePath = contextParam.RoutePath,
                site = contextParam.Site
            })).Data;
            return response;
        }
    }
}
