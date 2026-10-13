using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OutboxPattern.Application.Interfaces;
using OutboxPattern.Infrastructure.Configuration;
using OutboxPattern.Infrastructure.Persistence;
using OutboxPattern.Infrastructure.Publishers;
using OutboxPattern.Infrastructure.Repositories;
using OutboxPattern.Infrastructure.Workers;
using RabbitMQ.Client;

namespace OutboxPattern.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sql =>
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

        // AppDbContext implementa IUnitOfWork — misma instancia por scope = misma transacción
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Opciones de configuración
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));

        // IConnection como Singleton: crear una conexión TCP a RabbitMQ es caro.
        // Se reutiliza durante toda la vida de la app; los channels (IModel) son baratos y se crean por operación.
        services.AddSingleton<IConnection>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            var factory = new ConnectionFactory
            {
                HostName = opts.Host,
                Port = opts.Port,
                UserName = opts.Username,
                Password = opts.Password
            };
            return factory.CreateConnection("OutboxPattern.Api");
        });

        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
