using Versioning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<VersioningDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("VersioningDb")));

var app = builder.Build();

app.MapControllers();
app.Run();