namespace SmartParking_API.Models;

public sealed class Reservation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("parkingSpotId")]
    [BsonRepresentation(BsonType.String)]
    public required string ParkingSpotId { get; set; }

    [BsonElement("userEmail")]
    [EmailAddress]
    [MaxLength(254)]
    public required string UserEmail { get; set; }

    [BsonElement("startTimeUtc")]
    public required DateTime StartTimeUtc { get; set; }

    [BsonElement("endTimeUtc")]
    public DateTime? EndTimeUtc { get; set; }

    [BsonElement("durationMinutes")]
    public int? DurationMinutes { get; set; }

    [BsonElement("totalCost")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? TotalCost { get; set; }
}
