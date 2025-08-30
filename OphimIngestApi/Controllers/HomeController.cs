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
                .Select(x => new { 
                    movie = new { 
                        id = x.Id, 
                        slug = x.Slug, 
                        name = x.Name, 
                        originName = x.OriginName,
                        content = x.Content,
                        posterUrl = x.PosterUrl, 
                        year = x.Year, 
                        quality = x.Quality, 
                        lang = x.Lang, 
                        type = x.Type, 
                        status = x.Status 
                    }
                })
                .ToListAsync();

            // hot theo view
            var trending = await _db.Movies.AsNoTracking()
                .OrderByDescending(x => x.View)
                .ThenByDescending(x => x.UpdatedAt)
                .Take(take)
                .Select(x => new { 
                    movie = new { 
                        id = x.Id, 
                        slug = x.Slug, 
                        name = x.Name, 
                        posterUrl = x.PosterUrl,
                        content = x.Content,
                        year = x.Year, 
                        view = x.View, 
                        quality = x.Quality, 
                        lang = x.Lang 
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                latest,
                trending
            });
        }
    }
}
