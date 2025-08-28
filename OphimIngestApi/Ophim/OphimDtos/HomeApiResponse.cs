namespace OphimIngestApi.Ophim.OphimDtos
{
    public class HomeApiResponse
    {
        public string Status { get; set; } = default!;
        public string Message { get; set; } = default!;
        public HomeData Data { get; set; } = default!;
    }

    public class HomeData
    {
        public SeoOnPage SeoOnPage { get; set; } = default!;
        public List<HomeItem> Items { get; set; } = new();
        public Pagination Pagination { get; set; } = default!;
    }

    public class SeoOnPage
    {
        public string TitleHead { get; set; } = default!;
        public string DescriptionHead { get; set; } = default!;
        public string OgType { get; set; } = default!;
        public List<string> OgImage { get; set; } = new();
    }

    public class Pagination
    {
        public int TotalItems { get; set; }
        public int TotalItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int PageRanges { get; set; }
    }

    public class HomeItem
    {
        public string Id { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string OriginName { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string ThumbUrl { get; set; } = default!;
        public string Time { get; set; } = default!;
        public string EpisodeCurrent { get; set; } = default!;
        public string Quality { get; set; } = default!;
        public string Lang { get; set; } = default!;
        public int Year { get; set; }
        public List<OphimSlugName> Category { get; set; } = new();  // Đây là danh sách Category từ API
        public List<OphimSlugName> Country { get; set; } = new();  // Danh sách Country từ API

    }

    public class Category
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
    }

    public class Country
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
    }

}
