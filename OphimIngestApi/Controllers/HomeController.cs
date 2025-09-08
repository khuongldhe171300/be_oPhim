using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OphimIngestApi.Data.OPhimApiDb;

namespace OphimIngestApi.Controllers
{
    [ApiController]
    [Route("api/home")]
    public class HomeController : ControllerBase
    {
        private readonly AppDb _db;
        public HomeController(AppDb db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int take = 12)
        {
            take = Math.Clamp(take, 1, 24);

            // Helper method để tạo movie object
            var movieSelector = new Func<OphimIngestApi.Data.Entities.Movie, object>(x => new
            {
                id = x.Id,
                slug = x.Slug,
                name = x.Name,
                originName = x.OriginName,
                content = x.Content,
                posterUrl = x.PosterUrl,
                thumbUrl = x.ThumbUrl,
                year = x.Year,
                quality = x.Quality,
                lang = x.Lang,
                type = x.Type,
                status = x.Status,
                view = x.View,
                episodeCurrent = x.EpisodeCurrent,
                episodeTotal = x.EpisodeTotal,
                chieurap = x.Chieurap,
                updatedAt = x.UpdatedAt
            });

            // Phim mới cập nhật
            var latest = await _db.Movies.AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => movieSelector(x))
                .ToListAsync();

            // Top lượt xem
            var topViews = await _db.Movies.AsNoTracking()
                .Where(x => x.View > 0)
                .OrderByDescending(x => x.View)
                .ThenByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => movieSelector(x))
                .ToListAsync();

            // Top 10 phim bộ hôm nay (series updated today)
            var today = DateTime.UtcNow.Date;
            var topSeriesToday = await _db.Movies.AsNoTracking()
                .Where(x => x.Type == "series" && x.UpdatedAt >= today)
                .OrderByDescending(x => x.View)
                .ThenByDescending(x => x.UpdatedAt)
                .Take(10)
                .Select(x => movieSelector(x))
                .ToListAsync();

            // Phim lẻ mới 
            var newSingleMovies = await _db.Movies.AsNoTracking()
                .Where(x => x.Type == "single")
                .OrderByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => movieSelector(x))
                .ToListAsync();

            // Phim chiếu rạp
            var cinemaMovies = await _db.Movies.AsNoTracking()
                .Where(x => x.Chieurap == true)
                .OrderByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => movieSelector(x))
                .ToListAsync();

            // Phim sắp tới (status = "trailer" hoặc "coming_soon")
            var comingSoon = await _db.Movies.AsNoTracking()
                .Where(x => x.Status == "trailer" || x.Status == "coming_soon" || x.Status == "upcoming")
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => movieSelector(x))
                .ToListAsync();

            // Phim Hàn Quốc
            var koreanMovies = await _db.Movies.AsNoTracking()
                .Where(x => x.MovieCountries.Any(mc => mc.Country.Slug == "han-quoc"))
                .OrderByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => movieSelector(x))
                .ToListAsync();

            // Phim Nhật Bản
            var japaneseMovies = await _db.Movies.AsNoTracking()
                .Where(x => x.MovieCountries.Any(mc => mc.Country.Slug == "nhat-ban"))
                .OrderByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => movieSelector(x))
                .ToListAsync();

            // Phim Trung Quốc
            var chineseMovies = await _db.Movies.AsNoTracking()
                .Where(x => x.MovieCountries.Any(mc => mc.Country.Slug == "trung-quoc"))
                .OrderByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => movieSelector(x))
                .ToListAsync();

            // Phim Thái Lan
            var thaiMovies = await _db.Movies.AsNoTracking()
                .Where(x => x.MovieCountries.Any(mc => mc.Country.Slug == "thai-lan"))
                .OrderByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => movieSelector(x))
                .ToListAsync();

            var sliderMovies = await _db.Movies.AsNoTracking()
            .Where(x => x.View > 0)
            .OrderByDescending(x => x.View)
            .ThenByDescending(x => x.UpdatedAt)
            .Take(10)
            .Select(x => movieSelector(x))
            .ToListAsync();

            return Ok(new
            {
                latest,
                topViews,
                topSeriesToday,
                newSingleMovies,
                cinemaMovies,
                comingSoon,
                sliderMovies,
                byCountry = new
                {
                    korean = koreanMovies,
                    japanese = japaneseMovies,
                    chinese = chineseMovies,
                    thai = thaiMovies
                }
            });
        }

        [HttpGet("top-series-today")]
        public async Task<IActionResult> GetTopSeriesToday([FromQuery] int take = 10)
        {
            take = Math.Clamp(take, 1, 50);
            var today = DateTime.UtcNow.Date;

            var result = await _db.Movies.AsNoTracking()
                .Where(x => x.Type == "series" && x.UpdatedAt >= today)
                .OrderByDescending(x => x.View)
                .ThenByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => new
                {
                    id = x.Id,
                    slug = x.Slug,
                    name = x.Name,
                    posterUrl = x.PosterUrl,
                    year = x.Year,
                    view = x.View,
                    episodeCurrent = x.EpisodeCurrent,
                    episodeTotal = x.EpisodeTotal,
                    quality = x.Quality,
                    lang = x.Lang
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("by-country/{countrySlug}")]
        public async Task<IActionResult> GetByCountry(string countrySlug, [FromQuery] int take = 24, [FromQuery] int skip = 0)
        {
            take = Math.Clamp(take, 1, 48);
            skip = Math.Max(skip, 0);

            var result = await _db.Movies.AsNoTracking()
                .Where(x => x.MovieCountries.Any(mc => mc.Country.Slug == countrySlug))
                .OrderByDescending(x => x.UpdatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    id = x.Id,
                    slug = x.Slug,
                    name = x.Name,
                    originName = x.OriginName,
                    posterUrl = x.PosterUrl,
                    year = x.Year,
                    quality = x.Quality,
                    lang = x.Lang,
                    type = x.Type,
                    view = x.View
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("cinema-movies")]
        public async Task<IActionResult> GetCinemaMovies([FromQuery] int take = 24, [FromQuery] int skip = 0)
        {
            take = Math.Clamp(take, 1, 48);
            skip = Math.Max(skip, 0);

            var result = await _db.Movies.AsNoTracking()
                .Where(x => x.Chieurap == true)
                .OrderByDescending(x => x.UpdatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    id = x.Id,
                    slug = x.Slug,
                    name = x.Name,
                    originName = x.OriginName,
                    posterUrl = x.PosterUrl,
                    year = x.Year,
                    quality = x.Quality,
                    lang = x.Lang,
                    type = x.Type
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("coming-soon")]
        public async Task<IActionResult> GetComingSoon([FromQuery] int take = 24, [FromQuery] int skip = 0)
        {
            take = Math.Clamp(take, 1, 48);
            skip = Math.Max(skip, 0);

            var result = await _db.Movies.AsNoTracking()
                .Where(x => x.Status == "trailer" || x.Status == "coming_soon" || x.Status == "upcoming")
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.UpdatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    id = x.Id,
                    slug = x.Slug,
                    name = x.Name,
                    originName = x.OriginName,
                    posterUrl = x.PosterUrl,
                    year = x.Year,
                    quality = x.Quality,
                    lang = x.Lang,
                    type = x.Type,
                    status = x.Status,
                    trailerUrl = x.TrailerUrl
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("by-type/{movieType}")]
        public async Task<IActionResult> GetByType(string movieType, [FromQuery] int take = 24, [FromQuery] int skip = 0)
        {
            take = Math.Clamp(take, 1, 48);
            skip = Math.Max(skip, 0);

            if (movieType != "single" && movieType != "series")
            {
                return BadRequest("movieType must be 'single' or 'series'");
            }

            var result = await _db.Movies.AsNoTracking()
                .Where(x => x.Type == movieType)
                .OrderByDescending(x => x.UpdatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    id = x.Id,
                    slug = x.Slug,
                    name = x.Name,
                    originName = x.OriginName,
                    posterUrl = x.PosterUrl,
                    year = x.Year,
                    quality = x.Quality,
                    lang = x.Lang,
                    type = x.Type,
                    episodeCurrent = x.EpisodeCurrent,
                    episodeTotal = x.EpisodeTotal
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}
