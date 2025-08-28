using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OphimIngestApi.Data.OPhimApiDb;

namespace OphimIngestApi.Controllers
{
    [ApiController]
    [Route("api/movieslist")]
    public class MoviesListController : ControllerBase
    {
        private readonly AppDb _db;
        public MoviesListController(AppDb db) => _db = db;

        // GET /api/movies?keyword=&cat=&country=&year=&type=&sort=updated|view|year&order=desc|asc&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] string? keyword, [FromQuery] string? cat,
            [FromQuery] string? country, [FromQuery] int? year,
            [FromQuery] string? type, [FromQuery] string sort = "updated",
            [FromQuery] string order = "desc",
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = Math.Clamp(pageSize, 1, 60);

            var q = _db.Movies.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var coll = "Vietnamese_100_CI_AI"; // bỏ dấu + không phân biệt hoa thường
                var k = keyword.Trim();
                q = q.Where(m =>
                    EF.Functions.Collate(m.Name!, coll).Contains(k) ||
                    (m.OriginName != null && EF.Functions.Collate(m.OriginName!, coll).Contains(k)));
            }

            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(x => x.Type == type);

            if (year.HasValue)
                q = q.Where(x => x.Year == year);

            if (!string.IsNullOrWhiteSpace(cat))
                q = q.Where(x => x.MovieCategories.Any(mc => mc.Category.Slug == cat));

            if (!string.IsNullOrWhiteSpace(country))
                q = q.Where(x => x.MovieCountries.Any(cc => cc.Country.Slug == country));

            // sort
            bool desc = (order?.ToLower() ?? "desc") == "desc";
            q = (sort?.ToLower()) switch
            {
                "view" => (desc ? q.OrderByDescending(x => x.View) : q.OrderBy(x => x.View))
                            .ThenByDescending(x => x.UpdatedAt),
                "year" => (desc ? q.OrderByDescending(x => x.Year) : q.OrderBy(x => x.Year))
                            .ThenByDescending(x => x.UpdatedAt),
                _ => (desc ? q.OrderByDescending(x => x.UpdatedAt) : q.OrderBy(x => x.UpdatedAt))
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new
                {
                    x.Slug,
                    x.Name,
                    x.OriginName,
                    x.PosterUrl,
                    x.Year,
                    x.Quality,
                    x.Lang,
                    x.Type,
                    x.Status
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }
    }
}
