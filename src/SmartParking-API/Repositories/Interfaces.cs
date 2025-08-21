namespace SmartParking_API.Repositories;

public interface IParkingRepository
{
    Task<List<Parking>> GetAllAsync(CancellationToken ct);
    Task<Parking?> GetByIdAsync(string id, CancellationToken ct);
    Task<string> CreateAsync(Parking parking, CancellationToken ct);
    Task<bool> AddSpotAsync(string parkingId, ParkingSpot spot, CancellationToken ct);
    Task<(Parking? parking, ParkingSpot? spot)> FindSpotAsync(string spotId, CancellationToken ct);
    Task UpdateParkingAsync(Parking parking, CancellationToken ct);
}

public interface IReservationRepository
{
    Task<string> CreateAsync(Reservation reservation, CancellationToken ct);
    Task<Reservation?> GetByIdAsync(string id, CancellationToken ct);
    Task UpdateAsync(Reservation reservation, CancellationToken ct);
}
