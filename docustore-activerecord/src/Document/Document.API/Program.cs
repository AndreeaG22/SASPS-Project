using Document.Application;
using Document.Infrastructure;
using Document.Infrastructure.Data;
using Document.API.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Document module layers
builder.Services.AddDocumentApplication();
builder.Services.AddDocumentInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg => cfg.LicenseKey = builder.Configuration.GetConnectionString("MediatRLicenseKey"));

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