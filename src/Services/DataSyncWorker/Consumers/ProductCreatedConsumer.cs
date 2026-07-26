using MassTransit;
using MongoDB.Driver;
using Shared.Contracts;
using Shared.Contracts.Models;

namespace DataSyncWorker.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly IMongoCollection<ProductReadModel> _products;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(IMongoDatabase database, ILogger<ProductCreatedConsumer> logger)
    {
        _products = database.GetCollection<ProductReadModel>("Products");
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Syncing created product to Read DB (Mongo): {ProductId}", msg.Id);

        var doc = new ProductReadModel
        {
            Id = msg.Id,
            Name = msg.Name,
            Description = msg.Description,
            Price = msg.Price,
            CreatedAtUtc = msg.CreatedAtUtc,
            LastSyncedAtUtc = DateTime.UtcNow
        };

        // Use ReplaceOne with IsUpsert = true for idempotency (handles out-of-order/duplicate delivery safely)
        await _products.ReplaceOneAsync(
            p => p.Id == msg.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }
}