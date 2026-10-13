using Microsoft.EntityFrameworkCore;
using OutboxPattern.Application.Interfaces;
using OutboxPattern.Domain.Entities;

namespace OutboxPattern.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.ProductId).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.EventType).IsRequired().HasMaxLength(100);
            // Índice para que el worker lea solo los pendientes eficientemente
            e.HasIndex(m => m.ProcessedAt);
        });
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);
}
