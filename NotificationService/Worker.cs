using System.Text;
using MailKit.Net.Smtp;
using MimeKit;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel? _channel;

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMq:Host"] ?? "localhost",
            Port = int.Parse(_config["RabbitMq:Port"] ?? "5672"),
            UserName = _config["RabbitMq:Username"] ?? "guest",
            Password = _config["RabbitMq:Password"] ?? "guest",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                _connection = factory.CreateConnection("NotificationService");
                break;
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException) when (attempt < 10)
            {
                _logger.LogWarning("RabbitMQ no disponible, reintentando ({Attempt}/10)...", attempt);
                await Task.Delay(3000, cancellationToken);
            }
        }
        _channel = _connection.CreateModel();

        // 1. Declarar el mismo Exchange que usa la API
        _channel.ExchangeDeclare("outbox.events", ExchangeType.Topic, durable: true);

        // 2. Crear la COLA — aquí se guardan los mensajes esperando ser leídos
        _channel.QueueDeclare(
            queue: "notifications",
            durable: true,
            exclusive: false,
            autoDelete: false);

        // 3. BIND — conectar la cola al Exchange
        //    "#" = escuchar TODOS los eventos
        _channel.QueueBind(
            queue: "notifications",
            exchange: "outbox.events",
            routingKey: "#");

        _logger.LogInformation("Cola 'notifications' creada y conectada al exchange 'outbox.events'");

        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel!);

        consumer.Received += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var eventType = ea.BasicProperties.Type ?? "Unknown";

            _logger.LogInformation("[RECIBIDO] {EventType} → {Body}", eventType, body);

            try
            {
                await SendEmailAsync(eventType, body);

                // ACK = "lo procesé bien, bórralo de la cola"
                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                _logger.LogInformation("[ACK] Mensaje confirmado y eliminado de la cola");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje, se reintentará");
                // NACK + requeue = "falló, devuélvelo a la cola"
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel!.BasicConsume(queue: "notifications", autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task SendEmailAsync(string eventType, string payload)
    {
        var smtpHost = _config["Email:SmtpHost"] ?? "localhost";
        var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "1025");
        var from = _config["Email:From"] ?? "noreply@outbox-demo.com";
        var to = _config["Email:To"] ?? "dev@outbox-demo.com";

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(from));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = $"[OutboxDemo] {eventType}";
        email.Body = new TextPart("plain")
        {
            Text = $"Evento recibido por NotificationService:\n\n" +
                   $"Tipo:  {eventType}\n\n" +
                   $"Payload:\n{payload}"
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(smtpHost, smtpPort, false);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        _logger.LogInformation("[EMAIL] Enviado a {To} vía Mailpit", to);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
