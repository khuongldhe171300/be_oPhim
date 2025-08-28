using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OphimIngestApi.Data.OPhimApiDb;

namespace OphimIngestApi.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly AppDb _db;
        public SearchController(AppDb db) => _db = db;

        // GET /api/search?q=&page=&pageSize=
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new { total = 0, page = 1, pageSize, items = Array.Empty<object>() });

            var coll = "Vietnamese_100_CI_AI";
            page = page < 1 ? 1 : page;
            pageSize = Math.Clamp(pageSize, 1, 60);

            var baseQ = _db.Movies.AsNoTracking().Where(m =>
                EF.Functions.Collate(m.Name!, coll).Contains(q) ||
                (m.OriginName != null && EF.Functions.Collate(m.OriginName!, coll).Contains(q)));

            var total = await baseQ.CountAsync();
            var items = await baseQ.OrderByDescending(x => x.UpdatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new { x.Slug, x.Name, x.OriginName, x.PosterUrl, x.Year, x.Type, x.Quality, x.Lang })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // GET /api/search/suggest?q=&take=10
        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest([FromQuery] string q, [FromQuery] int take = 10)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<object>());
            var coll = "Vietnamese_100_CI_AI";
            take = Math.Clamp(take, 1, 20);

            var items = await _db.Movies.AsNoTracking()
                .Where(m => EF.Functions.Collate(m.Name!, coll).Contains(q) ||
                            (m.OriginName != null && EF.Functions.Collate(m.OriginName!, coll).Contains(q)))
                .OrderByDescending(m => m.View).ThenByDescending(m => m.UpdatedAt)
                .Take(take)
                .Select(m => new { m.Slug, m.Name, m.OriginName, m.PosterUrl, m.Year })
                .ToListAsync();

            return Ok(items);
        }
    }
}
