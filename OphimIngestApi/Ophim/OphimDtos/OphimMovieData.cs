using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimMovieData
    {
        [JsonPropertyName("breadCrumb")] public List<OphimBreadCrumb> BreadCrumb { get; set; } = new();
        [JsonPropertyName("item")] public OphimMovie Item { get; set; } = default!;
    }
}
