#nullable enable
using GraphQL;
using System.Threading.Tasks;

namespace Mvp.Foundation.GraphQLEdgeConnector.Rendering.GraphQLFactory
{
    public interface IGraphQLProviderMiddleware
    {
        Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(bool? isEditMode, GraphQLFiles queryFile, dynamic? variables) where TResponse : class;
    }
}
