using System.ComponentModel.DataAnnotations;

namespace OphimIngestApi.Data.Entities
{
    public class MovieList
    {
        public int Id { get; set; }

        [MaxLength(150)] public string Slug { get; set; } = default!;
        [MaxLength(255)] public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
