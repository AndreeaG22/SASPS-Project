using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Versioning.Domain.Common;
using Versioning.Domain.Services;
using Versioning.Infrastructure.Data;
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
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddSingleton<VersioningDbContextFactory>(sp =>
        {
            return () =>
            {
                var scope = Versioning.Domain.Common.ServiceLocator.CreateScope();
                return scope.ServiceProvider.GetRequiredService<VersioningDbContext>();
            };
        });

        services.AddSingleton<IFileStorageService, FileStorageService>();

        return services;
    }

    public static void InitializeVersioningDbContextProvider(IServiceProvider serviceProvider)
    {
        var contextFactory = serviceProvider.GetRequiredService<VersioningDbContextFactory>();
        VersioningDbContextProvider.Initialize(contextFactory);
    }
}