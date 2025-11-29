using Tagging.Infrastructure.Data;
using Tagging.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Tagging.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<TaggingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TaggingDb")));

var app = builder.Build();

// Configure Active Record pattern
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    
    // Initialize ServiceLocator for Active Record pattern
    ServiceLocator.Initialize(serviceProvider);
    
    // Initialize DbContextProvider for Active Record pattern
    TaggingDbContextProvider.Initialize(() => serviceProvider.GetRequiredService<TaggingDbContext>());
    
    // Apply migrations automatically
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