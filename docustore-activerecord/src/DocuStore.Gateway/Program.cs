using Microsoft.AspNetCore.OpenApi; // pentru .WithOpenApi()

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // simplu, fără OpenApiInfo explicit

// CORS pentru front-end (dacă e nevoie)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

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

app.MapGet("/health", () => Results.Ok(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow,
        modules = new[] { "Document", "Versioning", "Tagging", "MetadataIndexing" }
    }))
    .WithTags("Health")
    .WithOpenApi();

app.MapGet("/", () => Results.Ok(new
    {
        message = "Welcome to DocuStore API Gateway",
        documentation = "/swagger",
        health = "/health",
        version = "1.0.0"
    }))
    .WithTags("Gateway")
    .WithOpenApi();

app.Run();