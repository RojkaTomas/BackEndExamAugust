namespace SmartParking_API.DTOs;

public sealed record StartReservationRequest(
    string ParkingSpotId,
    [property: EmailAddress][property: MaxLength(254)] string UserEmail,
    DateTime? StartTimeUtc,
    [property: MaxLength(12)][property: RegularExpression("^[A-Za-z0-9]+$")] string LicensePlate
);

public sealed record EndReservationRequest(
    string ReservationId,
    DateTime? EndTimeUtc
);
