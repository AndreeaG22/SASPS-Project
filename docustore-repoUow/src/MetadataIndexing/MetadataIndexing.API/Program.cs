using MetadataIndexing.Application;
using MetadataIndexing.Infrastructure;
using MetadataIndexing.Infrastructure.Data;
using MetadataIndexing.API.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add module services
builder.Services.AddMetadataIndexingApplication();
builder.Services.AddMetadataIndexingInfrastructure(builder.Configuration);

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dbContext = serviceProvider.GetRequiredService<MetadataIndexingDbContext>();
    dbContext.Database.Migrate();
}

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapSearchEndpoints();
app.MapControllers();

app.Run();