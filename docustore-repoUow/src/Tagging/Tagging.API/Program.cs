using Tagging.Application;
using Tagging.Infrastructure;
using Tagging.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Tagging.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add module services
builder.Services.AddTaggingApplication();
builder.Services.AddTaggingInfrastructure(builder.Configuration);

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dbContext = serviceProvider.GetRequiredService<TaggingDbContext>();
    dbContext.Database.Migrate();
}

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapTagEndpoints();
app.MapControllers();

app.Run();