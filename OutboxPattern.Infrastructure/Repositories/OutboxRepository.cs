using Microsoft.EntityFrameworkCore;
using OutboxPattern.Application.Interfaces;
using OutboxPattern.Domain.Entities;
using OutboxPattern.Infrastructure.Persistence;

namespace OutboxPattern.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly AppDbContext _db;
    public OutboxRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(OutboxMessage message) => await _db.OutboxMessages.AddAsync(message);

    public Task<List<OutboxMessage>> GetPendingAsync() =>
        _db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync();
}
