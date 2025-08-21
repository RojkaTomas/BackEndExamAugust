# SmartParking-API (Minimal API, .NET 9, MongoDB)

> ⚠️ Note on naming: C# namespaces cannot contain hyphens. The requirement said “namespace must start with `SmartParking-API`”; to satisfy the spirit, this solution uses the namespace root **`SmartParking_API`** (underscore instead of hyphen). The project folder is still `SmartParking-API`.

## What you’ll build

A Smart Parking API that manages **Parkings**, **Parking Spots**, and **Reservations** with:
- ASP.NET **Minimal API** on **.NET 9**
- **MongoDB** for storage
- **Repository** and **Service** patterns
- **API key** auth via `X-Api-Key` header (stored in *appsettings.json*)
- **xUnit** unit tests for pricing
- **Integration tests** using **MongoDB Testcontainers**

## Quick start

1. **Prereqs**
   - .NET 9 SDK
   - Docker Desktop (for Testcontainers)
   - Local MongoDB (or update the connection string)

2. **Open & restore**
   ```bash
   cd SmartParking_API
   dotnet restore
   ```

3. **Run API**
   ```bash
   dotnet run --project src/SmartParking-API
   ```
   The API serves Swagger at `/swagger` (Development only).  
   Add header: `X-Api-Key: dev-12345` to all requests.

4. **Run tests**
   ```bash
   dotnet test
   ```

## API Overview

All endpoints require **`X-Api-Key`** header.

### Parkings
- `GET /api/parkings` — all parkings
- `GET /api/parkings/{id}` — parking by id (404 if not found)
- `POST /api/parkings` — create parking
  ```json
  {
    "name": "Central Garage",
    "location": { "street": "Main 1", "city": "Brussels", "postalCode": "1000", "latitude": 50.85, "longitude": 4.35 },
    "spots": [ { "licensePlate": null, "status": 0 } ]
  }
  ```

### Parking Spots
- `POST /api/parkingspots` — add spot to a parking
  ```json
  {
    "parkingId": "<parking-id>",
    "licensePlate": null,
    "status": 0
  }
  ```
  > If `status` is `Occupied`, **licensePlate is required** and must be alphanumeric.

### Reservations
- `POST /api/reservations/start`
  ```json
  {
    "parkingSpotId": "<spot-id>",
    "userEmail": "user@example.com",
    "startTimeUtc": "2025-08-21T10:00:00Z",
    "licensePlate": "1ABC234"
  }
  ```
- `POST /api/reservations/end`
  ```json
  {
    "reservationId": "<reservation-id>",
    "endTimeUtc": "2025-08-21T12:05:00Z"
  }
  ```
  Billing:
  - Weekend start → **€0.20/min**
  - Weekday start → **€0.15/min**
  - Duration rounded **up** to whole minutes.

## Design Notes

- **Validation**: DataAnnotations via `MiniValidation`. Service layer enforces “license plate required when occupied.”
- **Exception handling**: global middleware → ProblemDetails with proper status codes.
- **API key**: header `X-Api-Key` validated against `Auth:ApiKey` in config.
- **Mongo**: `parkings` collection stores embedded `spots`. `reservations` separate collection.

## Project layout

```
src/SmartParking-API
  ├── Program.cs
  ├── GlobalUsings.cs
  ├── appsettings.json
  ├── Models/ (Parking, ParkingSpot, Reservation, Location, enums)
  ├── DTOs/
  ├── Infrastructure/ (Mongo settings, API key & error middleware, DI)
  ├── Repositories/ (interfaces + Mongo implementations)
  └── Services/ (business logic, pricing)
tests/
  ├── SmartParking-API.UnitTests (xUnit pricing tests)
  └── SmartParking-API.IntegrationTests (API tests + Mongo Testcontainers)
```

## Tips for full marks

- Keep API key secret in real apps (user secrets / env vars).
- Add more tests (invalid payloads, conflict states).
- Consider adding indexes (e.g., on `spots.id`), swagger security header, and email format checks.
