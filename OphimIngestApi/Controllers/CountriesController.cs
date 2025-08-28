using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OphimIngestApi.Data.OPhimApiDb;

namespace OphimIngestApi.Controllers
{
    [ApiController]
    [Route("api/countries")]
    public class CountriesController : ControllerBase
    {
        private readonly AppDb _db;
        public CountriesController(AppDb db) => _db = db;

        // GET /api/countries
        [HttpGet]
        public async Task<IActionResult> List() =>
            Ok(await _db.Countries.AsNoTracking()
                .OrderBy(c => c.Name).Select(c => new { c.Slug, c.Name }).ToListAsync());

        // GET /api/countries/{slug}/movies?cat=&year=&sort=&order=&page=&pageSize=
        [HttpGet("{slug}/movies")]
        public async Task<IActionResult> MoviesByCountry(
            [FromRoute] string slug,
            [FromQuery] string? cat, [FromQuery] int? year,
            [FromQuery] string sort = "updated", [FromQuery] string order = "desc",
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = Math.Clamp(pageSize, 1, 60);

            var q = _db.Movies.AsNoTracking()
                .Where(m => m.MovieCountries.Any(cc => cc.Country.Slug == slug));

            if (!string.IsNullOrWhiteSpace(cat))
                q = q.Where(m => m.MovieCategories.Any(mc => mc.Category.Slug == cat));

            if (year.HasValue) q = q.Where(m => m.Year == year);

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
