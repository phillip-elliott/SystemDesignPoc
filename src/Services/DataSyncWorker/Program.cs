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
            e.ConfigureConsumer<ProductCreatedConsumer>(context);
            e.ConfigureConsumer<ProductDeletedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();