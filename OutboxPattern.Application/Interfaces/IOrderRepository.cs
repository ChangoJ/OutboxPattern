using OutboxPattern.Domain.Entities;

namespace OutboxPattern.Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order);
}
