using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimSlugName
    {
        [JsonPropertyName("_id")] public string? Id { get; set; }
        [JsonPropertyName("slug")] public string Slug { get; set; } = default!;
        [JsonPropertyName("name")] public string Name { get; set; } = default!;
    }
}
