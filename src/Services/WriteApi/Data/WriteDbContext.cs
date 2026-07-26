using MassTransit;
using Microsoft.EntityFrameworkCore;
using WriteApi.Domain;

namespace WriteApi.Data;

public class WriteDbContext : DbContext
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddTransactionalOutboxEntities();
    }
}