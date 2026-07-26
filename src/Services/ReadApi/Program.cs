using MongoDB.Driver;
using Shared.Contracts.Models;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReadApi.Models;

var builder = WebApplication.CreateBuilder(args);
var serviceName = "read-api";
var serviceVersion = "1.0.0";

// Add API Explorer / Swagger
builder.Services.AddEndpointsApiExplorer();

// 1. Configure MongoDB Client & Database DI
var mongoConn = builder.Configuration.GetConnectionString("ReadDb") 
    ?? "mongodb://localhost:27017";

var mongoClient = new MongoClient(mongoConn);
var mongoDatabase = mongoClient.GetDatabase("readdb");

builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(mongoDatabase);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .UseOtlpExporter()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation() // Captures incoming HTTP requests
            .AddHttpClientInstrumentation() // Captures outgoing HTTP calls
            .AddEntityFrameworkCoreInstrumentation(); // Captures EF Core SQL queries
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation(); // CPU, Memory, GC metrics
    });

builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});

var app = builder.Build();

// Helper to get Mongo Collection
IMongoCollection<ProductReadModel> GetCollection(IMongoDatabase db) =>
    db.GetCollection<ProductReadModel>("Products");

// --- QUERY ENDPOINTS ---

// GET /api/products (Fetch all products)
app.MapGet("/api/products", async (IMongoDatabase db) =>
{
    var collection = GetCollection(db);
    var products = await collection.Find(_ => true).ToListAsync();

    var response = products.Select(p => new ProductResponse(
        p.Id, 
        p.Name, 
        p.Description, 
        p.Price, 
        p.CreatedAtUtc));

    return Results.Ok(response);
});

// GET /api/products/{id} (Fetch single product by GUID)
app.MapGet("/api/products/{id:guid}", async (Guid id, IMongoDatabase db) =>
{
    var collection = GetCollection(db);
    var product = await collection.Find(p => p.Id == id).FirstOrDefaultAsync();

    if (product is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new ProductResponse(
        product.Id, 
        product.Name, 
        product.Description, 
        product.Price, 
        product.CreatedAtUtc));
});

app.Run();