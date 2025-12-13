using Document.Application.Interfaces;
using Document.Domain.Services;
using Document.Infrastructure.Data;
using Document.Infrastructure.Repositories;
using Document.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Versioning.Application.Interfaces;

namespace Document.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DocumentDbContext>(
            options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DocuStoreDb"),
                    b => b.MigrationsAssembly(typeof(DocumentDbContext).Assembly.FullName)),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        // Register repositories and Unit of Work
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register services
        services.AddSingleton<IFileStorageService, FileStorageService>();
        
        // Register cross-module service for Versioning module
        services.AddScoped<IDocumentQueryService, DocumentQueryService>();

        return services;
    }
}
