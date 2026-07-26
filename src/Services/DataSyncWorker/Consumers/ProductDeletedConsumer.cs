using MassTransit;
using MongoDB.Driver;
using Shared.Contracts;
using Shared.Contracts.Models;

namespace DataSyncWorker.Consumers;

public class ProductDeletedConsumer : IConsumer<ProductDeletedEvent>
{
    private readonly IMongoCollection<ProductReadModel> _products;
    private readonly ILogger<ProductDeletedConsumer> _logger;

    public ProductDeletedConsumer(IMongoDatabase database, ILogger<ProductDeletedConsumer> logger)
    {
        _products = database.GetCollection<ProductReadModel>("Products");
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Syncing deleted product from Read DB (Mongo): {ProductId}", msg.Id);

        await _products.DeleteOneAsync(p => p.Id == msg.Id);
    }
}