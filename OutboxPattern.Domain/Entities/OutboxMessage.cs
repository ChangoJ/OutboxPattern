namespace OutboxPattern.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private OutboxMessage() { } // EF Core

    public static OutboxMessage Create(string eventType, string payload) => new()
    {
        Id = Guid.NewGuid(),
        EventType = eventType,
        Payload = payload,
        CreatedAt = DateTime.UtcNow
    };

    public void MarkAsProcessed() => ProcessedAt = DateTime.UtcNow;
}
