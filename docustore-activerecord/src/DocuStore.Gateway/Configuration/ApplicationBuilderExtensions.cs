using Document.API.Endpoints;

namespace DocuStore.Gateway.Configuration;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseDocuStoreSwagger(this WebApplication app)
    {
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

        return app;
    }

    public static IApplicationBuilder UseDocuStoreEndpoints(this WebApplication app)
    {
        app.UseCors("AllowAll");
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        // Map module endpoints
        app.MapDocumentEndpoints();

        // Map health and info endpoints
        app.MapHealthEndpoints();

        return app;
    }

    private static void MapHealthEndpoints(this WebApplication app)
    {
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
    }
}