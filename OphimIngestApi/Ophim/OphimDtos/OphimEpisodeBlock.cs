using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimEpisodeBlock
    {
        [JsonPropertyName("server_name")] public string ServerName { get; set; } = default!;
        [JsonPropertyName("server_data")] public List<OphimEpisodeData> ServerData { get; set; } = new();
    }
}
