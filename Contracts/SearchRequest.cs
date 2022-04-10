using System.Text.Json.Serialization;

namespace Contracts
{
    public class SearchRequest
    {
        [JsonPropertyName("top")]
        public int? Top { get; set; }
        [JsonPropertyName("skip")]
        public int? Skip { get; set; }
        [JsonPropertyName("orderby")]
        public string? Orderby { get; set; }
        [JsonPropertyName("filter")]
        public string? Filter { get; set; }
        [JsonPropertyName("count")]
        public bool Count { get; set; } = true;
    }
}
