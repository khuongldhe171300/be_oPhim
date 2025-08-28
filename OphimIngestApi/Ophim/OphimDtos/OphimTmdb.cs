using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimTmdb
    {
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("vote_average")] public double? VoteAverage { get; set; }
        [JsonPropertyName("vote_count")] public int? VoteCount { get; set; }
    }
}
