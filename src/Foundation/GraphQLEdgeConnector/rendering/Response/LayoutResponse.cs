using Newtonsoft.Json;

namespace Mvp.Foundation.GraphQLEdgeConnector.Rendering.Response
{
    public class LayoutResponse : IGraphQlResponse
    {
        [JsonProperty(PropertyName = "layout")]
        public Layout layout { get; set; }

        public class Layout
        {
            [JsonProperty(PropertyName = "item")]
            public Item item { get; set; }
        }

        public class Item
        {
            [JsonProperty(PropertyName = "rendered")]
            public dynamic rendered { get; set; }
        }
    }
}
