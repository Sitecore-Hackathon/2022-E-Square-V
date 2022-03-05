using Mvp.Foundation.GraphQLEdgeConnector.Rendering.Param;
using Mvp.Foundation.GraphQLEdgeConnector.Rendering.Response;
using System.Threading.Tasks;

namespace Mvp.Foundation.GraphQLEdgeConnector.Rendering.Services
{
    /// <summary>
    /// Graph QL Service
    /// </summary>
    public interface IGraphQLService<T> where T : IGraphQlResponse
    {
        /// <summary>
        /// Queries and converts to GraphQl Response
        /// </summary>
        /// <typeparam name="IGraphQlResponse"></typeparam>
        /// <param name="isEditing"></param>
        /// <param name="contextParam"></param>
        /// <returns></returns>
        Task<T> Fetch(ContextParam contextParam);
    }
}
