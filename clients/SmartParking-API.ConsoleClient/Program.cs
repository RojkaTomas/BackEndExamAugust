using System.Net.Http.Json;

var baseUrl = Environment.GetEnvironmentVariable("SP_BASE_URL") ?? "http://localhost:5000";
var apiKey  = Environment.GetEnvironmentVariable("SP_API_KEY") ?? "dev-12345";

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

Console.WriteLine($"SmartParking Console Client\nBase: {baseUrl}\n");

while (true)
{
    Console.WriteLine("""
    Choose:
      1) GET /api/parkings
      2) GET /api/parkings/{id}
      3) POST /api/parkings (create)
      4) POST /api/parkingspots (add spot)
      5) POST /api/reservations/start
      6) POST /api/reservations/end
      7) Exit
    """);

    Console.Write("> ");
    var choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                {
                    var res = await http.GetAsync("/api/parkings");
                    Console.WriteLine(await res.Content.ReadAsStringAsync());
                    break;
                }
            case "2":
                {
                    Console.Write("Parking id: ");
                    var id = Console.ReadLine();
                    var res = await http.GetAsync($"/api/parkings/{id}");
                    Console.WriteLine($"{(int)res.StatusCode} {res.StatusCode}");
                    Console.WriteLine(await res.Content.ReadAsStringAsync());
                    break;
                }
            case "3":
                {
                    Console.Write("Name: "); var name = Console.ReadLine() ?? "Garage";
                    Console.Write("Street: "); var street = Console.ReadLine() ?? "Main 1";
                    Console.Write("City: "); var city = Console.ReadLine() ?? "City";
                    Console.Write("Postal: "); var postal = Console.ReadLine() ?? "1000";
                    Console.Write("Lat: "); var lat = double.Parse(Console.ReadLine() ?? "50.0");
                    Console.Write("Lng: "); var lng = double.Parse(Console.ReadLine() ?? "4.0");

                    var body = new
                    {
                        name,
                        location = new { street, city, postalCode = postal, latitude = lat, longitude = lng },
                        spots = Array.Empty<object>()
                    };

                    var res = await http.PostAsJsonAsync("/api/parkings", body);
                    Console.WriteLine($"{(int)res.StatusCode} {res.StatusCode}");
                    Console.WriteLine(await res.Content.ReadAsStringAsync());
                    break;
                }
            case "4":
                {
                    Console.Write("Parking id: "); var parkingId = Console.ReadLine();
                    Console.Write("Status (0=Free, 1=Occupied): "); var status = int.Parse(Console.ReadLine() ?? "0");
                    Console.Write("License plate (leave empty if Free): "); var lp = Console.ReadLine();

                    var body = new { parkingId, licensePlate = string.IsNullOrWhiteSpace(lp) ? null : lp, status };
                    var res = await http.PostAsJsonAsync("/api/parkingspots", body);
                    Console.WriteLine($"{(int)res.StatusCode} {res.StatusCode}");
                    Console.WriteLine(await res.Content.ReadAsStringAsync());
                    break;
                }
            case "5":
                {
                    Console.Write("Spot id: "); var spotId = Console.ReadLine();
                    Console.Write("User email: "); var email = Console.ReadLine() ?? "user@example.com";
                    Console.Write("License plate: "); var plate = Console.ReadLine() ?? "1ABC234";

                    var body = new { parkingSpotId = spotId, userEmail = email, licensePlate = plate };
                    var res = await http.PostAsJsonAsync("/api/reservations/start", body);
                    Console.WriteLine($"{(int)res.StatusCode} {res.StatusCode}");
                    Console.WriteLine(await res.Content.ReadAsStringAsync());
                    break;
                }
            case "6":
                {
                    Console.Write("Reservation id: "); var rid = Console.ReadLine();
                    var body = new { reservationId = rid };
                    var res = await http.PostAsJsonAsync("/api/reservations/end", body);
                    Console.WriteLine($"{(int)res.StatusCode} {res.StatusCode}");
                    Console.WriteLine(await res.Content.ReadAsStringAsync());
                    break;
                }
            case "7":
                return;
            default:
                Console.WriteLine("Unknown choice.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    Console.WriteLine();
}
