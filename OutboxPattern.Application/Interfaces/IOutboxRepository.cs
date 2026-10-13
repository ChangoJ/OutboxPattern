using OutboxPattern.Domain.Entities;

namespace OutboxPattern.Application.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message);
    Task<List<OutboxMessage>> GetPendingAsync();
}
