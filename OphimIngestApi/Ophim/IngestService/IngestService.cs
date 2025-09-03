using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OphimIngestApi.Data.Entities;
using OphimIngestApi.Data.OPhimApiDb;
using OphimIngestApi.Ophim.OphimDtos;

namespace OphimIngestApi.Ophim.IngestService
{
    public class IngestService
    {
        private readonly AppDb _db;
        private readonly IHttpClientFactory _http;
        private readonly OphimOptions _opt;

        public IngestService(AppDb db, IHttpClientFactory http, IOptions<OphimOptions> opt)
        { _db = db; _http = http; _opt = opt.Value; }

        public async Task IngestBySlugAsync(string slug, CancellationToken ct = default)
        {
            var client = _http.CreateClient("ophim");
            client.BaseAddress = new Uri(_opt.BaseUrl);

            var path = string.Format(_opt.DetailPath, slug);
            var res = await client.GetFromJsonAsync<OphimMovieResponse>(path, cancellationToken: ct);
            var item = res?.Data?.Item ?? throw new Exception("Không lấy được chi tiết phim.");
            var breadCrumb = res?.Data?.BreadCrumb ?? new List<OphimBreadCrumb>();

            // 1) Chỉ lấy Id phim (KHÔNG Include)
            var movie = await _db.Movies.SingleOrDefaultAsync(x => x.Slug == item.Slug, ct);
            var isNew = movie == null;

            if (isNew)
            {
                movie = new Movie { Slug = item.Slug, UpdatedAt = DateTime.UtcNow };
                _db.Movies.Add(movie);
            }

            // map field cơ bản
            movie.Name = item.Name;
            movie.OriginName = item.OriginName;
            movie.Content = item.Content;
            movie.Type = item.Type ?? "single";
            movie.Status = item.Status;
            movie.ThumbUrl = "https://img.ophim.live/uploads/movies/" + item.ThumbUrl;
            movie.PosterUrl = "https://img.ophim.live/uploads/movies/" + item.PosterUrl;
            movie.TrailerUrl = item.TrailerUrl;
            movie.Time = item.Time;
            movie.EpisodeCurrent = item.EpisodeCurrent;
            movie.EpisodeTotal = item.EpisodeTotal;
            movie.Quality = item.Quality;
            movie.Lang = item.Lang;
            movie.Year = item.Year;
            movie.View = item.View;
            movie.ImdbId = item.Imdb?.Id;
            movie.ImdbVoteAverage = item.Imdb?.VoteAverage;
            movie.ImdbVoteCount = item.Imdb?.VoteCount;
            movie.TmdbId = item.Tmdb?.Id;
            movie.TmdbVoteAverage = item.Tmdb?.VoteAverage;
            movie.TmdbVoteCount = item.Tmdb?.VoteCount;
            movie.IsCopyright = item.IsCopyright;
            movie.SubDocquyen = item.SubDocquyen;
            movie.Chieurap = item.Chieurap;
            movie.UpdatedAt = DateTime.UtcNow;

            // Lưu để có movie.Id (nếu là phim mới)
            await _db.SaveChangesAsync(ct);
            var movieId = movie.Id;

            // 1.5) Xử lý MovieList từ breadCrumb (tìm slug có pattern /danh-sach/*)
            var listBreadCrumb = breadCrumb.FirstOrDefault(b => b.Slug.StartsWith("/danh-sach/"));
            if (listBreadCrumb != null)
            {
                var listSlug = listBreadCrumb.Slug.Replace("/danh-sach/", "");
                var movieList = await _db.MovieLists.FirstOrDefaultAsync(ml => ml.Slug == listSlug, ct);
                if (movieList == null)
                {
                    movieList = new Data.Entities.MovieList 
                    { 
                        Slug = listSlug, 
                        Name = listBreadCrumb.Name 
                    };
                    _db.MovieLists.Add(movieList);
                    await _db.SaveChangesAsync(ct);
                }
                movie.MovieListId = movieList.Id;
                await _db.SaveChangesAsync(ct);
            }

            // 2) Upsert Category/Country (xoá link cũ bằng ExecuteDelete)
            await _db.MovieCategories.Where(mc => mc.MovieId == movieId).ExecuteDeleteAsync(ct);
            foreach (var c in item.Category)
            {
                var cat = await _db.Categories.FirstOrDefaultAsync(x => x.Slug == c.Slug, ct)
                          ?? _db.Categories.Add(new Data.Entities.Category { Slug = c.Slug, Name = c.Name }).Entity;
                _db.MovieCategories.Add(new MovieCategory { MovieId = movieId, Category = cat });
            }

            await _db.MovieCountries.Where(mc => mc.MovieId == movieId).ExecuteDeleteAsync(ct);
            foreach (var c in item.Country)
            {
                var co = await _db.Countries.FirstOrDefaultAsync(x => x.Slug == c.Slug, ct)
                         ?? _db.Countries.Add(new Data.Entities.Country { Slug = c.Slug, Name = c.Name }).Entity;
                _db.MovieCountries.Add(new MovieCountry { MovieId = movieId, Country = co });
            }

            // 3) Diễn viên / đạo diễn
            await _db.Actors.Where(a => a.MovieId == movieId).ExecuteDeleteAsync(ct);
            await _db.Directors.Where(d => d.MovieId == movieId).ExecuteDeleteAsync(ct);
            _db.Actors.AddRange(item.Actor.Select(a => new Actor { MovieId = movieId, Name = a }));
            _db.Directors.AddRange(item.Director.Select(d => new Director { MovieId = movieId, Name = d }));

            await _db.SaveChangesAsync(ct);

            // 4) Xoá Episodes/Sources/Servers cũ bằng DELETE trực tiếp (rất nhanh)
            await _db.EpisodeSources
                .Where(es => _db.Episodes.Where(e => e.MovieId == movieId).Select(e => e.Id).Contains(es.EpisodeId))
                .ExecuteDeleteAsync(ct);
            await _db.Episodes.Where(e => e.MovieId == movieId).ExecuteDeleteAsync(ct);
            await _db.Servers.Where(s => s.MovieId == movieId).ExecuteDeleteAsync(ct);

            // 5) Thêm mới theo LÔ để tránh timeout
            _db.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var block in item.Episodes)
            {
                var server = new Server { MovieId = movieId, Name = block.ServerName };
                _db.Servers.Add(server);
                await _db.SaveChangesAsync(ct); // cần server.Id

                // buffer các tập để insert theo lô
                var buffer = new List<(Episode Ep, OphimEpisodeData Dto)>(256);

                foreach (var ep in block.ServerData)
                {
                    var epEntity = new Episode { MovieId = movieId, Name = ep.Name, Slug = ep.Slug, Filename = ep.Filename };
                    _db.Episodes.Add(epEntity);
                    buffer.Add((epEntity, ep));

                    if (buffer.Count >= 200) // cỡ lô
                    {
                        await _db.SaveChangesAsync(ct); // lấy Id cho Episode
                        foreach (var pair in buffer)
                        {
                            if (!string.IsNullOrWhiteSpace(pair.Dto.LinkM3u8))
                                _db.EpisodeSources.Add(new EpisodeSource { EpisodeId = pair.Ep.Id, ServerId = server.Id, Kind = "m3u8", Url = pair.Dto.LinkM3u8, Label = "auto" });
                            if (!string.IsNullOrWhiteSpace(pair.Dto.LinkEmbed))
                                _db.EpisodeSources.Add(new EpisodeSource { EpisodeId = pair.Ep.Id, ServerId = server.Id, Kind = "embed", Url = pair.Dto.LinkEmbed, Label = "embed" });
                        }
                        await _db.SaveChangesAsync(ct);
                        buffer.Clear();
                    }
                }

                // phần dư
                if (buffer.Count > 0)
                {
                    await _db.SaveChangesAsync(ct);
                    foreach (var pair in buffer)
                    {
                        if (!string.IsNullOrWhiteSpace(pair.Dto.LinkM3u8))
                            _db.EpisodeSources.Add(new EpisodeSource { EpisodeId = pair.Ep.Id, ServerId = server.Id, Kind = "m3u8", Url = pair.Dto.LinkM3u8, Label = "auto" });
                        if (!string.IsNullOrWhiteSpace(pair.Dto.LinkEmbed))
                            _db.EpisodeSources.Add(new EpisodeSource { EpisodeId = pair.Ep.Id, ServerId = server.Id, Kind = "embed", Url = pair.Dto.LinkEmbed, Label = "embed" });
                    }
                    await _db.SaveChangesAsync(ct);
                }
            }

            _db.ChangeTracker.AutoDetectChangesEnabled = true;
        }



    }
}