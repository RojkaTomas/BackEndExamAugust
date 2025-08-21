namespace SmartParking_API.Services;

public sealed class ParkingService(IParkingRepository repo) : IParkingService
{
    public Task<List<Parking>> GetAllAsync(CancellationToken ct) => repo.GetAllAsync(ct);

    public Task<Parking?> GetAsync(string id, CancellationToken ct) => repo.GetByIdAsync(id, ct);

    public async Task<string> CreateAsync(CreateParkingRequest req, CancellationToken ct)
    {
        // Validate DTO
        if (!MiniValidator.TryValidate(req, out var errors))
            throw new ValidationException(string.Join("; ", errors.SelectMany(kv => kv.Value)));

        var parking = new Parking
        {
            Name = req.Name,
            Location = new Location
            {
                Street = req.Location.Street,
                City = req.Location.City,
                PostalCode = req.Location.PostalCode,
                Latitude = req.Location.Latitude,
                Longitude = req.Location.Longitude
            },
            Spots = (req.Spots ?? new()).Select(s => new ParkingSpot
            {
                Status = s.Status,
                LicensePlate = s.LicensePlate
            }).ToList()
        };

        // Enforce rule: if spot is occupied -> license plate required and non-empty
        foreach (var spot in parking.Spots)
        {
            if (spot.Status == SpotStatus.Occupied && string.IsNullOrWhiteSpace(spot.LicensePlate))
                throw new ValidationException("LicensePlate is required when spot is Occupied.");
        }

        return await repo.CreateAsync(parking, ct);
    }

    public async Task<bool> CreateSpotAsync(CreateParkingSpotRequest req, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var errors))
            throw new ValidationException(string.Join("; ", errors.SelectMany(kv => kv.Value)));

        var spot = new ParkingSpot
        {
            Status = req.Status,
            LicensePlate = req.LicensePlate
        };

        if (spot.Status == SpotStatus.Occupied && string.IsNullOrWhiteSpace(spot.LicensePlate))
            throw new ValidationException("LicensePlate is required when spot is Occupied.");

        return await repo.AddSpotAsync(req.ParkingId, spot, ct);
    }
}

public sealed class ReservationService(IParkingRepository parkingRepo, IReservationRepository reservationRepo) : IReservationService
{
    private static bool IsWeekend(DateTime d) => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public (int minutes, decimal pricePerMinute, decimal total) CalculatePrice(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc < startUtc) throw new ArgumentException("End time must be >= start time.");

        var totalMinutes = (int)Math.Ceiling((endUtc - startUtc).TotalMinutes);
        var pricePerMin = IsWeekend(startUtc) ? 0.20m : 0.15m;
        var total = totalMinutes * pricePerMin;
        return (totalMinutes, pricePerMin, total);
    }

    public async Task<string> StartAsync(StartReservationRequest req, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var errors))
            throw new ValidationException(string.Join("; ", errors.SelectMany(kv => kv.Value)));

        // Validate spot exists and is Free
        var (parking, spot) = await parkingRepo.FindSpotAsync(req.ParkingSpotId, ct);
        if (parking is null || spot is null) throw new KeyNotFoundException("Parking spot not found.");
        if (spot.Status == SpotStatus.Occupied) throw new InvalidOperationException("Spot already occupied.");

        if (string.IsNullOrWhiteSpace(req.LicensePlate))
            throw new ValidationException("LicensePlate required to start a reservation.");

        // Occupy the spot
        spot.Status = SpotStatus.Occupied;
        spot.LicensePlate = req.LicensePlate;
        await parkingRepo.UpdateParkingAsync(parking, ct);

        var reservation = new Reservation
        {
            ParkingSpotId = req.ParkingSpotId,
            UserEmail = req.UserEmail,
            StartTimeUtc = req.StartTimeUtc ?? DateTime.UtcNow
        };

        var id = await reservationRepo.CreateAsync(reservation, ct);
        return id;
    }

    public async Task<Reservation?> EndAsync(EndReservationRequest req, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var errors))
            throw new ValidationException(string.Join("; ", errors.SelectMany(kv => kv.Value)));

        var reservation = await reservationRepo.GetByIdAsync(req.ReservationId, ct);
        if (reservation is null) throw new KeyNotFoundException("Reservation not found.");
        if (reservation.EndTimeUtc is not null) return reservation; // already ended

        var endUtc = req.EndTimeUtc ?? DateTime.UtcNow;
        var (minutes, _, total) = CalculatePrice(reservation.StartTimeUtc, endUtc);

        reservation.EndTimeUtc = endUtc;
        reservation.DurationMinutes = minutes;
        reservation.TotalCost = total;
        await reservationRepo.UpdateAsync(reservation, ct);

        // Free the spot
        var (parking, spot) = await parkingRepo.FindSpotAsync(reservation.ParkingSpotId, ct);
        if (parking is not null && spot is not null)
        {
            spot.Status = SpotStatus.Free;
            spot.LicensePlate = null;
            await parkingRepo.UpdateParkingAsync(parking, ct);
        }

        return reservation;
    }
}
