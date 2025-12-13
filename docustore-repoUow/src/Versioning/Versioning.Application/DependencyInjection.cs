using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Events;
using Versioning.Application.EventHandlers;

namespace Versioning.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVersioningApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register event handlers
        services.AddScoped<IEventHandler<DocumentCreatedEvent>, DocumentCreatedEventHandler>();

        return services;
    }
}
