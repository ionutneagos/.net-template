using System.Text.Json.Serialization;

namespace Contracts
{
    public class PagedResponse<T>
    {
        [JsonPropertyName("nextpagelink")]
        public Uri? NextPageLink { get; private set; }
        [JsonPropertyName("count")]
        public long? Count { get; private set; }
        [JsonPropertyName("items")]
        public IEnumerable<T>? Items { get; private set; }

        public PagedResponse(IEnumerable<T>? items, Uri? nextPageLink, long? count)
        {
            NextPageLink = nextPageLink;
            Items = items;
            Count = count;
        }
    }
}
