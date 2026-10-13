using OutboxPattern.Domain.Entities;

namespace OutboxPattern.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
