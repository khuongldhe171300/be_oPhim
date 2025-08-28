namespace OphimIngestApi.Data.Entities
{
    public class MovieCountry
    { 
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = default!; 
        public int CountryId { get; set; } 
        public Country Country { get; set; } = default!; }

}
