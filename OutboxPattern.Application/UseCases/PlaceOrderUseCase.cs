using System.Text.Json;
using OutboxPattern.Application.Interfaces;
using OutboxPattern.Domain.Entities;

namespace OutboxPattern.Application.UseCases;

public class PlaceOrderUseCase
{
    private readonly IOrderRepository _orders;
    private readonly IOutboxRepository _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceOrderUseCase(IOrderRepository orders, IOutboxRepository outbox, IUnitOfWork unitOfWork)
    {
        _orders = orders;
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(string productId, int quantity)
    {
        var order = Order.Create(productId, quantity);

        var payload = JsonSerializer.Serialize(new
        {
            OrderId  = order.Id,
            order.ProductId,
            order.Quantity,
            order.CreatedAt
        });

        var outboxMessage = OutboxMessage.Create("OrderPlaced", payload);

        // ============================================================
        // Aquí está el Outbox Pattern:
        // Order + OutboxMessage se guardan en LA MISMA transacción.
        // O los dos persisten, o ninguno. No hay estado inconsistente.
        // Si la app se cae aquí, el worker reintentará al volver.
        // ============================================================
        await _orders.AddAsync(order);
        await _outbox.AddAsync(outboxMessage);
        await _unitOfWork.CommitAsync();

        return order.Id;
    }
}
