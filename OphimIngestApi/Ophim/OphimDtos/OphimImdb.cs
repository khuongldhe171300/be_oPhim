using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimImdb
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("vote_average")] public double? VoteAverage { get; set; }
        [JsonPropertyName("vote_count")] public long? VoteCount { get; set; }
    }
}
