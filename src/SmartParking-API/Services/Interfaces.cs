namespace SmartParking_API.Services;

public interface IParkingService
{
    Task<List<Parking>> GetAllAsync(CancellationToken ct);
    Task<Parking?> GetAsync(string id, CancellationToken ct);
    Task<string> CreateAsync(CreateParkingRequest req, CancellationToken ct);
    Task<bool> CreateSpotAsync(CreateParkingSpotRequest req, CancellationToken ct);
}

public interface IReservationService
{
    Task<string> StartAsync(StartReservationRequest req, CancellationToken ct);
    Task<Reservation?> EndAsync(EndReservationRequest req, CancellationToken ct);
    (int minutes, decimal pricePerMinute, decimal total) CalculatePrice(DateTime startUtc, DateTime endUtc);
}
