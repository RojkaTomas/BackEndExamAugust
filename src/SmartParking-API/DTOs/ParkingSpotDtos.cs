namespace SmartParking_API.DTOs;

public sealed record CreateParkingSpotRequest(
    string ParkingId,
    [property: MaxLength(12)][property: RegularExpression("^[A-Za-z0-9]*$")] string? LicensePlate,
    SpotStatus Status
);
