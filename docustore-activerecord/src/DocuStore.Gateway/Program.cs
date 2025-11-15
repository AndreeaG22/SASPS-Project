using Document.Application;
using Document.Infrastructure;
using Document.Infrastructure.Data;
using Document.API.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "DocuStore API", Version = "v1" });
});

// 🔥 Add Document module layers
builder.Services.AddDocumentApplication();
builder.Services.AddDocumentInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// 🔥🔥🔥 AUTO-MIGRATE ALL DATABASES ON STARTUP
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Migrate Document module
        logger.LogInformation("🗄️  Starting Document database migration...");
        var documentContext = services.GetRequiredService<DocumentDbContext>();
        await documentContext.Database.MigrateAsync();
        logger.LogInformation("✅ Document database migration completed successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred while migrating the database.");
        throw; // Fail fast if migration fails
    }
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DocuStore API v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "DocuStore API Documentation";
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapDocumentEndpoints();

app.MapGet("/health", () => Results.Ok(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow,
        modules = new[] { "Document", "Versioning", "Tagging", "MetadataIndexing" }
    }))
    .WithTags("Health");

app.MapGet("/", () => Results.Ok(new
    {
        message = "Welcome to DocuStore API Gateway",
        documentation = "/swagger",
        health = "/health",
        version = "1.0.0"
    }))
    .WithTags("Gateway");

app.Run();