using System.ComponentModel.DataAnnotations;

namespace OphimIngestApi.Data.Entities
{
    public class Server
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = default!;
        [MaxLength(120)] public string Name { get; set; } = default!;
        public ICollection<EpisodeSource> Sources { get; set; } = new List<EpisodeSource>();
    }
}
