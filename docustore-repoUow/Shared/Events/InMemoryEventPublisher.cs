using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Events;

public class InMemoryEventPublisher(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<InMemoryEventPublisher> logger)
    : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : class
    {
        using var scope = serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
        var handlers = serviceProvider.GetServices(handlerType);
        
        foreach (var handler in handlers)
        {
            try
            {
                var handleMethod = handlerType.GetMethod("HandleAsync");
                await (Task)handleMethod!.Invoke(handler, new object[] { @event, cancellationToken })!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling event {EventType}", @event.GetType().Name);
            }
        }
    }
}