namespace SmartParking_API.Models;

public sealed class ParkingSpot
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // License plate: letters and numbers only when occupied.
    // Not marked [Required] because new spots can be created as Free.
    // Validation is enforced in service layer.
    [BsonElement("licensePlate")]
    [RegularExpression("^[A-Za-z0-9]*$")]
    [MaxLength(12)]
    public string? LicensePlate { get; set; }

    [BsonElement("status")]
    public SpotStatus Status { get; set; } = SpotStatus.Free;
}
