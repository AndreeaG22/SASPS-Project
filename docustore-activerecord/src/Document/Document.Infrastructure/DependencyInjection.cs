using Document.Application.Services;
using Document.Infrastructure.Data;
using Document.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Document.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DocumentDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DocuStoreDb"),
                b => b.MigrationsAssembly(typeof(DocumentDbContext).Assembly.FullName)));

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<DocumentDbContext>());

        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}