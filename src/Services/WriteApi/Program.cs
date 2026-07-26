using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;
using WriteApi.Data;
using WriteApi.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add API Explorer / Swagger
builder.Services.AddEndpointsApiExplorer();

// 1. Configure EF Core with PostgreSQL
var postgresConn = builder.Configuration.GetConnectionString("WriteDb") 
    ?? "Host=localhost;Database=writedb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<WriteDbContext>(options =>
    options.UseNpgsql(postgresConn));

// 2. Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<WriteDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox(); // Automatically delivers pending outbox messages to RabbitMQ
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Ensure DB is created for quick local testing
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
    db.Database.Migrate();
}

// --- ENDPOINTS ---

// POST /api/products
app.MapPost("/api/products", async (
    CreateProductRequest request, 
    WriteDbContext db, 
    IPublishEndpoint publishEndpoint,
    CancellationToken ct) =>
{
    var product = new Product
    {
        Name = request.Name,
        Description = request.Description,
        Price = request.Price
    };

    await using var transaction = await db.Database.BeginTransactionAsync(ct);

    try {

        db.Products.Add(product);

        // Publish event to RabbitMQ for Read DB Sync
        await publishEndpoint.Publish(new ProductCreatedEvent(
            product.Id, 
            product.Name, 
            product.Description, 
            product.Price, 
            product.CreatedAtUtc), ct);
        
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);


        return Results.Created($"/api/products/{product.Id}", new { product.Id });
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        throw;
    }
});

// DELETE /api/products/{id}
app.MapDelete("/api/products/{id:guid}", async (
    Guid id, 
    WriteDbContext db, 
    IPublishEndpoint publishEndpoint,
    CancellationToken ct) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound();
    }

    await using var transaction = await db.Database.BeginTransactionAsync(ct);

    // Publish delete event to RabbitMQ
    await publishEndpoint.Publish(new ProductDeletedEvent(id, DateTime.UtcNow));

    db.Products.Remove(product);
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);    

    return Results.NoContent();
});

app.Run();