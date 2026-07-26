namespace Shared.Contracts;

// DTOs
public record CreateProductRequest(string Name, string Description, decimal Price);

// Integration Events (Published to RabbitMQ)
public record ProductCreatedEvent(Guid Id, string Name, string Description, decimal Price, DateTime CreatedAtUtc);
public record ProductDeletedEvent(Guid Id, DateTime DeletedAtUtc);