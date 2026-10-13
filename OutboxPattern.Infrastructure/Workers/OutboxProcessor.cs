using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutboxPattern.Application.Interfaces;

namespace OutboxPattern.Infrastructure.Workers;

// BackgroundService: corre en segundo plano toda la vida de la aplicación.
// Cada 5 segundos revisa si hay mensajes pendientes y los publica.
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        // Creamos un scope porque IOutboxRepository y IUnitOfWork son Scoped,
        // pero el worker es Singleton. Sin scope, el DI container fallaría.
        using var scope = _scopeFactory.CreateScope();
        var outbox    = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pending = await outbox.GetPendingAsync();
        if (pending.Count == 0) return;

        foreach (var message in pending)
        {
            try
            {
                await publisher.PublishAsync(message, cancellationToken);
                message.MarkAsProcessed();
                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // No lanzamos: si falla uno, seguimos con los demás.
                // El mensaje queda pendiente y se reintentará en el próximo ciclo.
                _logger.LogError(ex, "Error publicando mensaje {Id}", message.Id);
            }
        }
    }
}
