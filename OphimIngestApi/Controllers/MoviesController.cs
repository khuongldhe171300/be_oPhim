using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OphimIngestApi.Data.OPhimApiDb;
using OphimIngestApi.Ophim.IngestService;
using OphimIngestApi.Ophim.OphimDtos;
using static System.Net.WebRequestMethods;

namespace OphimIngestApi.Controllers
{
    [ApiController]
    [Route("api/movies")]
    public class MoviesController : ControllerBase
    {
        private readonly AppDb _db;
        private readonly IngestService _ingest;
        public MoviesController(AppDb db, IngestService ingest) { _db = db; _ingest = ingest; }

        
        [HttpPost("ingest/{slug}")]
        public async Task<IActionResult> Ingest([FromRoute] string slug)
        {
            await _ingest.IngestBySlugAsync(slug);
            return Ok(new { ok = true, slug });
        }

      
        // GET /api/movies?keyword=choi&page=1&pageSize=12&cat=hanh-dong&country=han-quoc&year=2021
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string? keyword,
                                      [FromQuery] string? cat,
                                      [FromQuery] string? country,
                                      [FromQuery] int? year,
                                      [FromQuery] int page = 1,
                                      [FromQuery] int pageSize = 20)
        {
            var q = _db.Movies.AsNoTracking().AsQueryable();  

         
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var keywordLower = keyword.ToLower(); 
                q = q.Where(x => x.Name.ToLower().Contains(keywordLower)
                                 || (x.OriginName != null && x.OriginName.ToLower().Contains(keywordLower)));
            }

            if (year.HasValue)
            {
                q = q.Where(x => x.Year == year);
            }

            if (!string.IsNullOrWhiteSpace(cat))
            {
                q = q.Where(x => x.MovieCategories.Any(mc => mc.Category.Slug == cat));
            }

            // Tìm kiếm theo quốc gia (country)
            if (!string.IsNullOrWhiteSpace(country))
            {
                q = q.Where(x => x.MovieCountries.Any(mc => mc.Country.Slug == country));
            }

            q = q.OrderByDescending(x => x.UpdatedAt);

            var total = await q.CountAsync();  
            var items = await q.Select(x => new {
                x.Slug,
                x.Name,
                x.OriginName,
                x.Year,
                x.Quality,
                x.Lang,
                x.PosterUrl,
                x.Type,
                x.Status
            }).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();  

            if (!items.Any())
            {
                return Ok(new { message = "Không có kết quả phù hợp.", total, page, pageSize, items });
            }

            return Ok(new { total, page, pageSize, items });
        }


        // Chi tiết theo slug
        [HttpGet("{slug}")]
        public async Task<IActionResult> Detail([FromRoute] string slug)
        {
            var m = await _db.Movies.AsNoTracking()
     .FirstOrDefaultAsync(x => x.Slug == slug);

            if (m == null) return NotFound();

            var cats = await _db.MovieCategories.AsNoTracking().Where(mc => mc.MovieId == m.Id)
                                .Select(mc => new { mc.Category.Slug, mc.Category.Name }).ToListAsync();
            var countries = await _db.MovieCountries.AsNoTracking().Where(mc => mc.MovieId == m.Id)
                                .Select(mc => new { mc.Country.Slug, mc.Country.Name }).ToListAsync();
            var actors = await _db.Actors.AsNoTracking().Where(a => a.MovieId == m.Id)
                                .Select(a => a.Name).ToListAsync();
            var directors = await _db.Directors.AsNoTracking().Where(d => d.MovieId == m.Id)
                                .Select(d => d.Name).ToListAsync();

            var episodes = await _db.Episodes.AsNoTracking()
                .Where(e => e.MovieId == m.Id)
                .Select(e => new {
                    e.Name,
                    e.Slug,
                    e.Filename,
                    sources = e.Sources.Select(s => new { s.Kind, s.Url, s.Label })
                }).ToListAsync();

            var servers = await _db.Servers.AsNoTracking()
                .Where(s => s.MovieId == m.Id)
                .Select(s => new {
                    s.Name,
                    sources = s.Sources.Select(x => new { x.Kind, x.Url, x.Label })
                }).ToListAsync();

            return Ok(new
            {
                movie = new
                {
                    m.Slug,
                    m.Name,
                    m.OriginName,
                    m.Content,
                    m.Type,
                    m.Status,
                    m.PosterUrl,
                    m.ThumbUrl,
                    m.TrailerUrl,
                    m.Time,
                    m.EpisodeCurrent,
                    m.EpisodeTotal,
                    m.Quality,
                    m.Lang,
                    m.Year,
                    m.View,
                    imdb = new { id = m.ImdbId, vote_average = m.ImdbVoteAverage, vote_count = m.ImdbVoteCount },
                    tmdb = new { id = m.TmdbId, vote_average = m.TmdbVoteAverage, vote_count = m.TmdbVoteCount },
                    categories = cats,
                    countries,
                    actors,
                    directors
                },
                episodes,
                servers
            });

        }


        //ingest-all




    }
}
