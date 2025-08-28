using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OphimIngestApi.Data.OPhimApiDb;

namespace OphimIngestApi.Controllers
{
    [ApiController]
    [Route("api/moviedetails")]
    public class MoviesDetailController : ControllerBase
    {
        private readonly AppDb _db;
        public MoviesDetailController(AppDb db) => _db = db;

        // GET /api/movies/{slug}?epPage=&epPageSize=&server=
       
        // DTO nhỏ cho nguồn phát
        private record EpisodeSourceVm(string Kind, string Url, string Label, string Server);

        [HttpGet("{slug}")]
        public async Task<IActionResult> Detail(
     [FromRoute] string slug,
     [FromQuery] int epPage = 1,
     [FromQuery] int epPageSize = 20,
     [FromQuery] string? server = null)
        {
            // Xác định trang bắt đầu và giới hạn số lượng mỗi trang
            epPage = epPage < 1 ? 1 : epPage;
            epPageSize = Math.Clamp(epPageSize, 1, 100);

            // 1) Lấy thông tin cơ bản của phim theo slug
            var m = await _db.Movies.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Slug == slug);
            if (m == null) return NotFound();

            // 2) Truy vấn dữ liệu liên quan đến thể loại, quốc gia, diễn viên, đạo diễn
            var cats = await _db.MovieCategories.AsNoTracking()
                .Where(mc => mc.MovieId == m.Id)
                .Select(mc => new { mc.Category.Slug, mc.Category.Name })
                .ToListAsync();

            var countries = await _db.MovieCountries.AsNoTracking()
                .Where(cc => cc.MovieId == m.Id)
                .Select(cc => new { cc.Country.Slug, cc.Country.Name })
                .ToListAsync();

            var actors = await _db.Actors.AsNoTracking()
                .Where(a => a.MovieId == m.Id)
                .Select(a => a.Name)
                .ToListAsync();

            var directors = await _db.Directors.AsNoTracking()
                .Where(d => d.MovieId == m.Id)
                .Select(d => d.Name)
                .ToListAsync();

            var serverNames = await _db.Servers.AsNoTracking()
                .Where(s => s.MovieId == m.Id)
                .Select(s => s.Name)
                .ToListAsync();

            // 3) Lấy thông tin các tập phim (episodes) và phân trang
            var epBase = _db.Episodes.AsNoTracking().Where(e => e.MovieId == m.Id);
            var totalEpisodes = await epBase.CountAsync();
            var totalPages = (int)Math.Ceiling(totalEpisodes / (double)epPageSize);

            var epPageItems = await epBase.OrderBy(e => e.Id)
                .Skip((epPage - 1) * epPageSize)
                .Take(epPageSize)
                .Select(e => new { e.Id, e.Name, e.Slug, e.Filename })
                .ToListAsync();

            var epIds = epPageItems.Select(x => x.Id).ToList();

            // 4) Lấy nguồn phát (sources) cho các tập trong trang hiện tại
            var srcQ = _db.EpisodeSources.AsNoTracking()
                .Where(s => epIds.Contains(s.EpisodeId));

            // Nếu có thông số server thì lọc thêm theo server
            if (!string.IsNullOrWhiteSpace(server))
                srcQ = srcQ.Where(s => s.Server.Name == server);

            var srcFlat = await srcQ.Select(s => new
            {
                s.EpisodeId,
                s.Kind,
                s.Url,
                s.Label,
                Server = s.Server.Name
            }).ToListAsync();

            // Sử dụng ToLookup để nhóm dữ liệu theo EpisodeId
            var srcLookup = srcFlat.ToLookup(x => x.EpisodeId, x => new EpisodeSourceVm(x.Kind, x.Url, x.Label, x.Server));

            // 5) Xây dựng dữ liệu trả về cho các tập phim
            var episodes = new
            {
                total = totalEpisodes,
                page = epPage,
                pageSize = epPageSize,
                totalPages,
                items = epPageItems.Select(e => new
                {
                    e.Name,
                    e.Slug,
                    e.Filename,
                    sources = srcLookup[e.Id]  // Các nguồn phát của tập phim
                })
            };

            // 6) Xây dựng payload cho movie
            var movie = new
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
                directors,
                servers = serverNames
            };

            // Trả về kết quả
            return Ok(new { movie, episodes });
        }



        // GET /api/movies/{slug}/images
        [HttpGet("{slug}/images")]
        public async Task<IActionResult> Images([FromRoute] string slug)
        {
            var m = await _db.Movies.AsNoTracking()
                .Where(x => x.Slug == slug)
                .Select(x => new { poster = x.PosterUrl, thumb = x.ThumbUrl })
                .FirstOrDefaultAsync();
            return m == null ? NotFound() : Ok(m);
        }

        // GET /api/movies/{slug}/actors
        [HttpGet("{slug}/actors")]
        public async Task<IActionResult> Actors([FromRoute] string slug)
        {
            var movieId = await _db.Movies.AsNoTracking()
                .Where(m => m.Slug == slug).Select(m => m.Id).FirstOrDefaultAsync();
            if (movieId == 0) return NotFound();

            var actors = await _db.Actors.AsNoTracking().Where(a => a.MovieId == movieId)
                .Select(a => a.Name).ToListAsync();
            var directors = await _db.Directors.AsNoTracking().Where(d => d.MovieId == movieId)
                .Select(d => d.Name).ToListAsync();

            return Ok(new { actors, directors });
        }

        // GET /api/movies/{slug}/keywords
        [HttpGet("{slug}/keywords")]
        public async Task<IActionResult> Keywords([FromRoute] string slug)
        {
            var movie = await _db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Slug == slug);
            if (movie == null) return NotFound();

            var cats = await _db.MovieCategories.AsNoTracking()
                .Where(mc => mc.MovieId == movie.Id).Select(mc => mc.Category.Name).ToListAsync();
            var countries = await _db.MovieCountries.AsNoTracking()
                .Where(cc => cc.MovieId == movie.Id).Select(cc => cc.Country.Name).ToListAsync();

            var words = new List<string>();
            words.AddRange(cats);
            words.AddRange(countries);
            if (!string.IsNullOrWhiteSpace(movie.Type)) words.Add(movie.Type);
            if (movie.Year.HasValue) words.Add(movie.Year.Value.ToString());
            words.AddRange((await _db.Actors.AsNoTracking().Where(a => a.MovieId == movie.Id).Select(a => a.Name).ToListAsync()).Take(5));
            words.AddRange((await _db.Directors.AsNoTracking().Where(d => d.MovieId == movie.Id).Select(d => d.Name).ToListAsync()).Take(3));

            return Ok(words.Distinct().ToList());
        }
    }
}
