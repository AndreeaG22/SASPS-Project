using MetadataIndexing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<MetadataIndexingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MetadataIndexingDb")));

var app = builder.Build();

app.MapControllers();
app.Run();