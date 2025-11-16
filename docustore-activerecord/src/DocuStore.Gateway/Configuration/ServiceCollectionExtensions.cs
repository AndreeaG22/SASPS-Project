using Document.Application;
using Document.Infrastructure;

namespace DocuStore.Gateway.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuStoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add module services
        services.AddDocumentApplication();
        services.AddDocumentInfrastructure(configuration);

        return services;
    }

    public static IServiceCollection AddDocuStoreApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "DocuStore API", Version = "v1" });
        });

        return services;
    }

    public static IServiceCollection AddDocuStoreCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        return services;
    }
}