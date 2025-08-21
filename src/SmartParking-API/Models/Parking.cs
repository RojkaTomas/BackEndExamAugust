namespace SmartParking_API.Models;

public sealed class Parking
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    [MaxLength(100)]
    public required string Name { get; set; }

    [BsonElement("location")]
    public required Location Location { get; set; }

    [BsonElement("spots")]
    public List<ParkingSpot> Spots { get; set; } = new();
}
