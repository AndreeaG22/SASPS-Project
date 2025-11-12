using Tagging.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<TaggingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TaggingDb")));

var app = builder.Build();

app.MapControllers();
app.Run();