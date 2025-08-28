using System.ComponentModel.DataAnnotations;

namespace OphimIngestApi.Data.Entities
{
    public class Actor 
    { 
        public int Id { get; set; } 
        public int MovieId { get; set; } 
        public Movie Movie { get; set; } = default!; 
        [MaxLength(255)] public string Name { get; set; } = default!; 
    }

}
