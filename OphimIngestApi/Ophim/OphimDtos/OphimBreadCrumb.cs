using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimBreadCrumb
    {
        [JsonPropertyName("name")] public string Name { get; set; } = default!;
        [JsonPropertyName("slug")] public string Slug { get; set; } = default!;
        [JsonPropertyName("position")] public int Position { get; set; }
        [JsonPropertyName("isCurrent")] public bool? IsCurrent { get; set; }
    }
}
