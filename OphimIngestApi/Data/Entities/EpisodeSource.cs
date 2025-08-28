using System.ComponentModel.DataAnnotations;

namespace OphimIngestApi.Data.Entities
{
    public class EpisodeSource
    {
        public int Id { get; set; }
        public int EpisodeId { get; set; }
        public Episode Episode { get; set; } = default!;
        public int ServerId { get; set; }
        public Server Server { get; set; } = default!;

        [MaxLength(10)] public string Kind { get; set; } = "m3u8"; // m3u8/embed
        [MaxLength(50)] public string? Label { get; set; }         // "auto" / "1080p"
        public string Url { get; set; } = default!;
    }
}
