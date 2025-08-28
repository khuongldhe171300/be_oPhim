using System.Text.Json.Serialization;

namespace OphimIngestApi.Ophim.OphimDtos
{
    public class OphimMovie
    {
        [JsonPropertyName("_id")] public string Id { get; set; } = default!;
        [JsonPropertyName("name")] public string Name { get; set; } = default!;
        [JsonPropertyName("slug")] public string Slug { get; set; } = default!;
        [JsonPropertyName("origin_name")] public string? OriginName { get; set; }
        [JsonPropertyName("content")] public string? Content { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; } = "single";
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("thumb_url")] public string? ThumbUrl { get; set; }
        [JsonPropertyName("poster_url")] public string? PosterUrl { get; set; }
        [JsonPropertyName("trailer_url")] public string? TrailerUrl { get; set; }
        [JsonPropertyName("time")] public string? Time { get; set; }
        [JsonPropertyName("episode_current")] public string? EpisodeCurrent { get; set; }
        [JsonPropertyName("episode_total")] public string? EpisodeTotal { get; set; }
        [JsonPropertyName("quality")] public string? Quality { get; set; }
        [JsonPropertyName("lang")] public string? Lang { get; set; }
        [JsonPropertyName("year")] public int? Year { get; set; }
        [JsonPropertyName("view")] public long? View { get; set; }

        [JsonPropertyName("actor")] public List<string> Actor { get; set; } = new();
        [JsonPropertyName("director")] public List<string> Director { get; set; } = new();
        [JsonPropertyName("category")] public List<OphimSlugName> Category { get; set; } = new();
        [JsonPropertyName("country")] public List<OphimSlugName> Country { get; set; } = new();
        [JsonPropertyName("episodes")] public List<OphimEpisodeBlock> Episodes { get; set; } = new();

        [JsonPropertyName("tmdb")] public OphimTmdb? Tmdb { get; set; }
        [JsonPropertyName("imdb")] public OphimImdb? Imdb { get; set; }
    }
}
