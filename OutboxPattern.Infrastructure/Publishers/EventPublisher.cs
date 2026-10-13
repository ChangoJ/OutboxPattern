using System.Text;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OutboxPattern.Application.Interfaces;
using OutboxPattern.Domain.Entities;
using OutboxPattern.Infrastructure.Configuration;
using RabbitMQ.Client;

namespace OutboxPattern.Infrastructure.Publishers;

public class EventPublisher : IEventPublisher
{
    private readonly IConnection _rabbit;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(IConnection rabbit, IOptions<EmailOptions> emailOptions, ILogger<EventPublisher> logger)
    {
        _rabbit = rabbit;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        PublishToRabbitMq(message);
        await SendEmailAsync(message, cancellationToken);
    }

    private void PublishToRabbitMq(OutboxMessage message)
    {
        using var channel = _rabbit.CreateModel();

        // Topic exchange: permite routing flexible por EventType
        // ej: "OrderPlaced", "OrderCancelled", etc.
        channel.ExchangeDeclare("outbox.events", ExchangeType.Topic, durable: true);

        var body = Encoding.UTF8.GetBytes(message.Payload);
        var props = channel.CreateBasicProperties();
        props.Persistent = true;          // sobrevive reinicios de RabbitMQ
        props.MessageId = message.Id.ToString();
        props.Type = message.EventType;
        props.ContentType = "application/json";

        channel.BasicPublish(
            exchange: "outbox.events",
            routingKey: message.EventType.ToLowerInvariant(),
            basicProperties: props,
            body: body);

        _logger.LogInformation("[RABBITMQ] Publicado → {EventType} | MessageId: {Id}", message.EventType, message.Id);
    }

    private async Task SendEmailAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_emailOptions.From));
        email.To.Add(MailboxAddress.Parse(_emailOptions.To));
        email.Subject = $"[OutboxDemo] {message.EventType}";
        email.Body = new TextPart("plain")
        {
            Text = $"Evento procesado por el Outbox Worker:\n\n" +
                   $"Tipo:    {message.EventType}\n" +
                   $"ID:      {message.Id}\n" +
                   $"Creado:  {message.CreatedAt:u}\n\n" +
                   $"Payload:\n{message.Payload}"
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailOptions.SmtpHost, _emailOptions.SmtpPort, false, cancellationToken);
        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("[EMAIL] Notificación enviada a {To} vía Mailpit", _emailOptions.To);
    }
}
