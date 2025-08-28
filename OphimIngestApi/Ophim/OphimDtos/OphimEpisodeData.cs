using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimEpisodeData
    {
        [JsonPropertyName("name")] public string Name { get; set; } = default!;
        [JsonPropertyName("slug")] public string? Slug { get; set; }
        [JsonPropertyName("filename")] public string? Filename { get; set; }
        [JsonPropertyName("link_embed")] public string? LinkEmbed { get; set; }
        [JsonPropertyName("link_m3u8")] public string? LinkM3u8 { get; set; }
    }
}
