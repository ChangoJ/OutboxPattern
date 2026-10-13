namespace OutboxPattern.Infrastructure.Configuration;

public class EmailOptions
{
    public string SmtpHost { get; init; } = "localhost";
    public int SmtpPort { get; init; } = 1025;
    public string From { get; init; } = "noreply@outbox-demo.com";
    public string To { get; init; } = "dev@outbox-demo.com";
}
