using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OphimIngestApi.Data.OPhimApiDb;
using OphimIngestApi.Ophim.IngestService;

var builder = WebApplication.CreateBuilder(args);

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== DB Context (pooled + tuned) =====
var cs = builder.Configuration.GetConnectionString("Default");
// Khuyến nghị: connection string có thêm MultipleActiveResultSets=true
// ví dụ: Server=localhost;Database=ophim_db;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true

builder.Services.AddDbContextPool<AppDb>(opt =>
{
    opt.UseSqlServer(cs, sql =>
    {
        sql.CommandTimeout(300);                 // 5 phút cho các ingest nặng
        sql.EnableRetryOnFailure(5,              // tối đa 5 lần retry
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
        sql.MaxBatchSize(1000);                  // optional: batch lớn hơn
    });

    // Tránh 1 query join khổng lồ khi có nhiều Include
    //opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);

    // Bật nếu muốn quan sát truy vấn
    // opt.LogTo(Console.WriteLine);
});

// ===== Bind options (Ophim) =====
builder.Services.Configure<OphimOptions>(builder.Configuration.GetSection("Ophim"));

// ===== HttpClient gọi Ophim =====
builder.Services.AddHttpClient("ophim", (sp, client) =>
{
    var opt = sp.GetRequiredService<IOptions<OphimOptions>>().Value;
    client.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/'));
    client.Timeout = TimeSpan.FromSeconds(120);          // đủ rộng cho API chậm
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

// ===== DI cho ingest service =====
builder.Services.AddScoped<IngestService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
