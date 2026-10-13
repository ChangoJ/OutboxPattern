namespace OutboxPattern.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string ProductId { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Order() { } // EF Core

    public static Order Create(string productId, int quantity) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = productId,
        Quantity = quantity,
        CreatedAt = DateTime.UtcNow
    };
}
