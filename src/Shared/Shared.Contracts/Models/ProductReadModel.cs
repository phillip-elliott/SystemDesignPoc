using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Contracts.Models;

public class ProductReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastSyncedAtUtc { get; set; }
}