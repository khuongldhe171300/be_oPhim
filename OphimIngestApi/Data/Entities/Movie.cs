using Microsoft.AspNetCore.Hosting.Server;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace OphimIngestApi.Data.Entities
{
    public class Movie
    {
        public int Id { get; set; }

        [MaxLength(150)] public string Slug { get; set; } = default!;
        [MaxLength(255)] public string Name { get; set; } = default!;
        [MaxLength(255)] public string? OriginName { get; set; }
        public string? Content { get; set; }

        [MaxLength(20)] public string Type { get; set; } = "single"; // single/series
        [MaxLength(30)] public string? Status { get; set; }
        public string? ThumbUrl { get; set; }
        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }
        [MaxLength(50)] public string? Time { get; set; }
        [MaxLength(50)] public string? EpisodeCurrent { get; set; }
        [MaxLength(50)] public string? EpisodeTotal { get; set; }
        [MaxLength(30)] public string? Quality { get; set; }
        [MaxLength(30)] public string? Lang { get; set; }
        public int? Year { get; set; }
        public long? View { get; set; }

        // Ratings / ids
        [MaxLength(20)] public string? ImdbId { get; set; }
        public double? ImdbVoteAverage { get; set; }
        public long? ImdbVoteCount { get; set; }
        [MaxLength(20)] public string? TmdbId { get; set; }
        public double? TmdbVoteAverage { get; set; }
        public int? TmdbVoteCount { get; set; }

        // Additional fields from Ophim API
        public bool IsCopyright { get; set; } = false;
        public bool SubDocquyen { get; set; } = false;
        public bool Chieurap { get; set; } = false;

        // Foreign key for MovieList
        public int? MovieListId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public MovieList? MovieList { get; set; }
        public ICollection<MovieCategory> MovieCategories { get; set; } = new List<MovieCategory>();
        public ICollection<MovieCountry> MovieCountries { get; set; } = new List<MovieCountry>();
        public ICollection<Actor> Actors { get; set; } = new List<Actor>();
        public ICollection<Director> Directors { get; set; } = new List<Director>();
        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
        public ICollection<Server> Servers { get; set; } = new List<Server>();
    }
}
