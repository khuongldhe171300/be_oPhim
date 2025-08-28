namespace OphimIngestApi.Data.Entities
{
    public class Category { 
        public int Id { get; set; } 
        public string Slug { get; set; } = default!;
        public string Name { get; set; } = default!; 
        public ICollection<MovieCategory> MovieCategories { get; set; } = new List<MovieCategory>();
    }

}
