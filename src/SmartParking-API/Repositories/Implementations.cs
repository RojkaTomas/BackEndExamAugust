namespace SmartParking_API.Repositories;

public sealed class ParkingRepository(IMongoCollection<Parking> collection) : IParkingRepository
{
    public async Task<List<Parking>> GetAllAsync(CancellationToken ct)
        => await collection.Find(_ => true).ToListAsync(ct);

    public async Task<Parking?> GetByIdAsync(string id, CancellationToken ct)
        => await collection.Find(p => p.Id == id).FirstOrDefaultAsync(ct);

    public async Task<string> CreateAsync(Parking parking, CancellationToken ct)
    {
        await collection.InsertOneAsync(parking, cancellationToken: ct);
        return parking.Id!;
    }

    public async Task<bool> AddSpotAsync(string parkingId, ParkingSpot spot, CancellationToken ct)
    {
        var update = Builders<Parking>.Update.Push(p => p.Spots, spot);
        var result = await collection.UpdateOneAsync(p => p.Id == parkingId, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task<(Parking? parking, ParkingSpot? spot)> FindSpotAsync(string spotId, CancellationToken ct)
    {
        var filter = Builders<Parking>.Filter.ElemMatch(p => p.Spots, s => s.Id == spotId);
        var projection = Builders<Parking>.Projection.Include(p => p.Spots);
        var found = await collection.Find(filter).FirstOrDefaultAsync(ct);
        var spot = found?.Spots.FirstOrDefault(s => s.Id == spotId);
        return (found, spot);
    }

    public async Task UpdateParkingAsync(Parking parking, CancellationToken ct)
    {
        await collection.ReplaceOneAsync(p => p.Id == parking.Id, parking, cancellationToken: ct);
    }
}

public sealed class ReservationRepository(IMongoCollection<Reservation> collection) : IReservationRepository
{
    public async Task<string> CreateAsync(Reservation reservation, CancellationToken ct)
    {
        await collection.InsertOneAsync(reservation, cancellationToken: ct);
        return reservation.Id!;
    }

    public async Task<Reservation?> GetByIdAsync(string id, CancellationToken ct)
        => await collection.Find(r => r.Id == id).FirstOrDefaultAsync(ct);

    public async Task UpdateAsync(Reservation reservation, CancellationToken ct)
        => await collection.ReplaceOneAsync(r => r.Id == reservation.Id, reservation, cancellationToken: ct);
}
