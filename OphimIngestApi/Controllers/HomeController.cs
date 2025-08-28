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
        public async Task<IActionResult> Get([FromQuery] int take = 24)
        {
            take = Math.Clamp(take, 1, 48);

            // mới cập nhật
            var latest = await _db.Movies.AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => new { x.Slug, x.Name, x.OriginName, x.PosterUrl, x.Year, x.Quality, x.Lang, x.Type, x.Status })
                .ToListAsync();

            // hot theo view
            var trending = await _db.Movies.AsNoTracking()
                .OrderByDescending(x => x.View)
                .ThenByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => new { x.Slug, x.Name, x.PosterUrl, x.Year, x.View, x.Quality, x.Lang })
                .ToListAsync();

            return Ok(new
            {
                latest,
                trending
            });
        }
    }
}
