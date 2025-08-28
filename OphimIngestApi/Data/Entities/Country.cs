namespace OphimIngestApi.Data.Entities
{
    public class Country
    {
        public int Id { get; set; }
        public string Slug { get; set; } = default!;
        public string Name { get; set; } = default!;
        public ICollection<MovieCountry> MovieCountries { get; set; } = new List<MovieCountry>(); }

}
