using System.ComponentModel.DataAnnotations;

namespace OphimIngestApi.Data.Entities
{
    public class Episode
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = default!;
        [MaxLength(100)] public string Name { get; set; } = default!;   // "Full", "Tập 1"
        [MaxLength(150)] public string? Slug { get; set; }
        [MaxLength(200)] public string? Filename { get; set; }
        public ICollection<EpisodeSource> Sources { get; set; } = new List<EpisodeSource>();
    }
}
