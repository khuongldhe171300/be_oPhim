using Microsoft.EntityFrameworkCore;
using OphimIngestApi.Data.Entities;

namespace OphimIngestApi.Data.OPhimApiDb
{
    public class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> options) : base(options) { }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<MovieCategory> MovieCategories => Set<MovieCategory>();
        public DbSet<MovieCountry> MovieCountries => Set<MovieCountry>();
        public DbSet<Actor> Actors => Set<Actor>();
        public DbSet<Director> Directors => Set<Director>();
        public DbSet<Server> Servers => Set<Server>();
        public DbSet<Episode> Episodes => Set<Episode>();
        public DbSet<EpisodeSource> EpisodeSources => Set<EpisodeSource>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ===== Index / Unique cơ bản
            b.Entity<Movie>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<Country>().HasIndex(x => x.Slug).IsUnique();

            b.Entity<MovieCategory>().HasKey(x => new { x.MovieId, x.CategoryId });
            b.Entity<MovieCountry>().HasKey(x => new { x.MovieId, x.CountryId });

            b.Entity<Server>().HasIndex(x => new { x.MovieId, x.Name }).IsUnique();

            // ===== Khai báo QUAN HỆ + DeleteBehavior để tránh multiple cascade paths
            // Movie 1-* Episodes (Cascade khi xóa Movie)
            b.Entity<Episode>()
                .HasOne(e => e.Movie)
                .WithMany(m => m.Episodes)
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Movie 1-* Servers (Cascade khi xóa Movie)
            b.Entity<Server>()
                .HasOne(s => s.Movie)
                .WithMany(m => m.Servers)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // EpisodeSource có 2 FK (Episode, Server) => cắt 1 nhánh:
            // Giữ cascade theo Episode...
            b.Entity<EpisodeSource>()
                .HasOne(es => es.Episode)
                .WithMany(e => e.Sources)
                .HasForeignKey(es => es.EpisodeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ...và NGẮT cascade theo Server (Restrict/NoAction đều được)
            b.Entity<EpisodeSource>()
                .HasOne(es => es.Server)
                .WithMany(s => s.Sources)
                .HasForeignKey(es => es.ServerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Actors / Directors / Join tables: để Cascade khi xóa Movie
            b.Entity<Actor>()
                .HasOne(a => a.Movie)
                .WithMany(m => m.Actors)
                .HasForeignKey(a => a.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Director>()
                .HasOne(d => d.Movie)
                .WithMany(m => m.Directors)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<MovieCategory>()
                .HasOne(mc => mc.Movie)
                .WithMany(m => m.MovieCategories)
                .HasForeignKey(mc => mc.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<MovieCategory>()
                .HasOne(mc => mc.Category)
                .WithMany(c => c.MovieCategories)
                .HasForeignKey(mc => mc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<MovieCountry>()
                .HasOne(mc => mc.Movie)
                .WithMany(m => m.MovieCountries)
                .HasForeignKey(mc => mc.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<MovieCountry>()
                .HasOne(mc => mc.Country)
                .WithMany(c => c.MovieCountries)
                .HasForeignKey(mc => mc.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== EpisodeSources: index duy nhất
            // NOTE: cần giới hạn độ dài Url để không bị nvarchar(max) (SQL Server không cho index trên MAX).
            b.Entity<EpisodeSource>()
                .Property(x => x.Url)
                .HasMaxLength(430); // đủ an toàn cho giới hạn 900 bytes khi tạo composite index

            b.Entity<EpisodeSource>()
                .HasIndex(x => new { x.EpisodeId, x.ServerId, x.Url })
                .IsUnique();
        }

    }
}
