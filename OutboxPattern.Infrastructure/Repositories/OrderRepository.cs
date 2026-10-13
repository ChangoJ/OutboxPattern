using OutboxPattern.Application.Interfaces;
using OutboxPattern.Domain.Entities;
using OutboxPattern.Infrastructure.Persistence;

namespace OutboxPattern.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Order order) => await _db.Orders.AddAsync(order);
}
