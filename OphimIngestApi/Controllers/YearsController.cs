using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OphimIngestApi.Data.OPhimApiDb;

namespace OphimIngestApi.Controllers
{
    [ApiController]
    [Route("api/years")]
    public class YearsController : ControllerBase
    {
        private readonly AppDb _db;
        public YearsController(AppDb db) => _db = db;

        // GET /api/years
        [HttpGet]
        public async Task<IActionResult> List() =>
            Ok(await _db.Movies.AsNoTracking()
                .Where(m => m.Year != null)
                .GroupBy(m => m.Year).Select(g => g.Key)
                .OrderByDescending(y => y).ToListAsync());

        // GET /api/years/{year}/movies?cat=&country=&type=&sort=&order=&page=&pageSize=
        [HttpGet("{year:int}/movies")]
        public async Task<IActionResult> MoviesByYear(
            [FromRoute] int year,
            [FromQuery] string? cat, [FromQuery] string? country, [FromQuery] string? type,
            [FromQuery] string sort = "updated", [FromQuery] string order = "desc",
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = Math.Clamp(pageSize, 1, 60);

            var q = _db.Movies.AsNoTracking().Where(m => m.Year == year);

            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(m => m.Type == type);

            if (!string.IsNullOrWhiteSpace(cat))
                q = q.Where(m => m.MovieCategories.Any(mc => mc.Category.Slug == cat));

            if (!string.IsNullOrWhiteSpace(country))
                q = q.Where(m => m.MovieCountries.Any(cc => cc.Country.Slug == country));

            bool desc = (order?.ToLower() ?? "desc") == "desc";
            q = (sort?.ToLower()) switch
            {
                "view" => desc ? q.OrderByDescending(x => x.View) : q.OrderBy(x => x.View),
                "year" => desc ? q.OrderByDescending(x => x.Year) : q.OrderBy(x => x.Year),
                _ => desc ? q.OrderByDescending(x => x.UpdatedAt) : q.OrderBy(x => x.UpdatedAt)
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new { x.Slug, x.Name, x.PosterUrl, x.Year, x.Type, x.Quality, x.Lang })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }
    }
}
