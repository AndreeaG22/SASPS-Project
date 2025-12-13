using Document.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Versioning.Application.Interfaces;
using Versioning.Domain.Services;
using Versioning.Infrastructure.Data;
using Versioning.Infrastructure.Repositories;
using Versioning.Infrastructure.Services;

namespace Versioning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVersioningInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VersioningDbContext>(
            options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DocuStoreDb"),
                    b => b.MigrationsAssembly(typeof(VersioningDbContext).Assembly.FullName)),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        // Register repositories and Unit of Work
        services.AddScoped<IVersionRepository, VersionRepository>();
        services.AddScoped<IVersioningUnitOfWork, VersioningUnitOfWork>();

        // Register services
        services.AddSingleton<IFileStorageService, FileStorageService>();
        
        // Register cross-module service for Document module
        services.AddScoped<IVersionQueryService, VersionQueryService>();

        return services;
    }
}
