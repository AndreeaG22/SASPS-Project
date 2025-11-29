using MetadataIndexing.Infrastructure.Data;
using MetadataIndexing.Domain.Common;
using MetadataIndexing.Infrastructure.EventHandlers;
using MetadataIndexing.API.Endpoints;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<MetadataIndexingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MetadataIndexingDb")));

// Register event infrastructure
builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

// Register event handlers
builder.Services.AddScoped<IEventHandler<DocumentCreatedEvent>, DocumentCreatedEventHandler>();
builder.Services.AddScoped<IEventHandler<DocumentUpdatedEvent>, DocumentUpdatedEventHandler>();
builder.Services.AddScoped<IEventHandler<DocumentDeletedEvent>, DocumentDeletedEventHandler>();

var app = builder.Build();

// Configure Active Record pattern
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    
    // Initialize ServiceLocator for Active Record pattern
    ServiceLocator.Initialize(serviceProvider);
    
    // Initialize DbContextProvider for Active Record pattern
    MetadataIndexingDbContextProvider.Initialize(() => serviceProvider.GetRequiredService<MetadataIndexingDbContext>());
    
    // Apply migrations automatically
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