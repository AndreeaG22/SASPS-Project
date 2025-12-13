using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tagging.Application.Interfaces;
using Tagging.Infrastructure.Data;
using Tagging.Infrastructure.Repositories;

namespace Tagging.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTaggingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TaggingDbContext>(
            options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DocuStoreDb"),
                    b => b.MigrationsAssembly(typeof(TaggingDbContext).Assembly.FullName)),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        // Register repositories and Unit of Work
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IDocumentTagRepository, DocumentTagRepository>();
        services.AddScoped<ITaggingUnitOfWork, TaggingUnitOfWork>();

        return services;
    }
}
