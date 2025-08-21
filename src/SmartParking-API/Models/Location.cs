namespace SmartParking_API.Models;

public sealed class Location
{
    [BsonElement("street")]
    [MaxLength(100)]
    public required string Street { get; set; }

    [BsonElement("city")]
    [MaxLength(80)]
    public required string City { get; set; }

    [BsonElement("postalCode")]
    [MaxLength(10)]
    public required string PostalCode { get; set; }

    [BsonElement("latitude")]
    public required double Latitude { get; set; }

    [BsonElement("longitude")]
    public required double Longitude { get; set; }
}
