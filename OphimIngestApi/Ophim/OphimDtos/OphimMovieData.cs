using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimMovieData
    {
        [JsonPropertyName("item")] public OphimMovie Item { get; set; } = default!;
    }
}
