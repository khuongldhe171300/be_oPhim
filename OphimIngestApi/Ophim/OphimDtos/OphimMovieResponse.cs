using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimMovieResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; } = default!;
        [JsonPropertyName("message")] public string Message { get; set; } = default!;
        [JsonPropertyName("data")] public OphimMovieData Data { get; set; } = default!;
    }
}
