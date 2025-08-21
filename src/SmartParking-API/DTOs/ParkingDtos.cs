namespace SmartParking_API.DTOs;

public sealed record CreateParkingRequest(
    [property: MaxLength(100)] string Name,
    LocationDto Location,
    List<ParkingSpotDto>? Spots
);

public sealed record LocationDto(
    [property: MaxLength(100)] string Street,
    [property: MaxLength(80)] string City,
    [property: MaxLength(10)] string PostalCode,
    double Latitude,
    double Longitude
);

public sealed record ParkingSpotDto(
    [property: MaxLength(12)][property: RegularExpression("^[A-Za-z0-9]*$")] string? LicensePlate,
    SpotStatus Status
);
