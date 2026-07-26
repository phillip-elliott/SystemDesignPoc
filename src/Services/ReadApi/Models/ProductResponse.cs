namespace ReadApi.Models;

public record ProductResponse(
    Guid Id, 
    string Name, 
    string Description, 
    decimal Price, 
    DateTime CreatedAtUtc);