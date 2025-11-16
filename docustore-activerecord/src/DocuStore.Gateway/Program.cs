using Document.Domain.Common;
using DocuStore.Gateway.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDocuStoreApi();
builder.Services.AddDocuStoreServices(builder.Configuration);
builder.Services.AddDocuStoreCors();

var app = builder.Build();

// Initialize service locator
ServiceLocator.Initialize(app.Services);

// Run database migrations
await app.Services.MigrateDocuStoreDatabaseAsync();

// Configure pipeline
app.UseDocuStoreSwagger();
app.UseDocuStoreEndpoints();

app.Run();