using Microsoft.Extensions.DependencyInjection;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.GraphQLFactory;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.Response;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.Services;

namespace Mvp.Foundation.GraphQLEdgeConnector.Rendering
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphQLRequestMiddlewareFoundation(this IServiceCollection serviceCollection)
        {
            //Register builder / factory / provider
            serviceCollection.AddSingleton<GraphQLRequestBuilderMiddleware>();
            serviceCollection.AddHttpClient<GraphQLClientFactoryMiddleware>();
            serviceCollection.AddSingleton<IGraphQLProviderMiddleware, GraphQLProviderMiddleware>();

            //Register custom services
            serviceCollection.AddSingleton<IGraphQLService<LayoutResponse>, GraphQlLayoutRequestService>();
        }
    }
}
