using System.Text;
using Microsoft.Extensions.Logging;
using OutboxPattern.Application.Interfaces;
using OutboxPattern.Domain.Entities;
using RabbitMQ.Client;

namespace OutboxPattern.Infrastructure.Publishers;

public class EventPublisher : IEventPublisher
{
    private readonly IConnection _rabbit;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(IConnection rabbit, ILogger<EventPublisher> logger)
    {
        _rabbit = rabbit;
        _logger = logger;
    }

    public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        using var channel = _rabbit.CreateModel();

        channel.ExchangeDeclare("outbox.events", ExchangeType.Topic, durable: true);

        var body = Encoding.UTF8.GetBytes(message.Payload);
        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.MessageId = message.Id.ToString();
        props.Type = message.EventType;
        props.ContentType = "application/json";

        channel.BasicPublish(
            exchange: "outbox.events",
            routingKey: message.EventType.ToLowerInvariant(),
            basicProperties: props,
            body: body);

        _logger.LogInformation("[RABBITMQ] Publicado → {EventType} | MessageId: {Id}", message.EventType, message.Id);

        return Task.CompletedTask;
    }
}
