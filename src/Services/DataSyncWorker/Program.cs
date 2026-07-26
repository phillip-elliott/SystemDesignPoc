using DataSyncWorker.Consumers;
using MassTransit;
using MongoDB.Driver;

var builder = Host.CreateApplicationBuilder(args);

// 1. Configure MongoDB
var mongoConn = builder.Configuration.GetConnectionString("ReadDb") 
    ?? "mongodb://localhost:27017";
var mongoClient = new MongoClient(mongoConn);
var mongoDatabase = mongoClient.GetDatabase("readdb");

builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(mongoDatabase);

// 2. Configure MassTransit Consumers & RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();
    x.AddConsumer<ProductDeletedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        // MassTransit automatically sets up exchange bindings and queues for these consumers
        cfg.ReceiveEndpoint("data-sync-products-queue", e =>
        {
            
            e.UseDelayedRedelivery(r =>
            {
                r.Intervals(
                    TimeSpan.FromMinutes(5), 
                    TimeSpan.FromMinutes(15), 
                    TimeSpan.FromMinutes(30)
                );
            });

            // Retry transient failures (e.g., Read DB temporary outage or lock)
            // Tries 5 times with growing intervals: ~1s, ~3s, ~7s, ~15s, ~30s
            e.UseMessageRetry(r =>
            {
                r.Exponential(
                    retryLimit: 5,
                    minInterval: TimeSpan.FromSeconds(1),
                    maxInterval: TimeSpan.FromSeconds(30),
                    intervalDelta: TimeSpan.FromSeconds(2)
                );
            });

            e.ConfigureConsumer<ProductCreatedConsumer>(context);
            e.ConfigureConsumer<ProductDeletedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();