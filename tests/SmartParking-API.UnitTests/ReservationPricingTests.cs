namespace SmartParking_API.UnitTests;

public class ReservationPricingTests
{
    [Theory]
    [InlineData("2025-08-16T10:00:00Z", "2025-08-16T10:01:00Z", 1, 0.20)]
    [InlineData("2025-08-18T10:00:00Z", "2025-08-18T10:01:00Z", 1, 0.15)] 
    public void Calculates_Price_Based_On_Start_Day(string startIso, string endIso, int minutes, decimal perMinute)
    {
        var svc = new ReservationService(null!, null!);
        var (mins, ppm, total) = svc.CalculatePrice(DateTime.Parse(startIso), DateTime.Parse(endIso));

        mins.Should().Be(minutes);
        ppm.Should().Be(perMinute);
        total.Should().Be(minutes * perMinute);
    }

    [Fact]
    public void Rounds_Up_Partial_Minutes()
    {
        var svc = new ReservationService(null!, null!);
        var start = DateTime.Parse("2025-08-18T10:00:00Z"); 
        var end = start.AddSeconds(30);
        var (mins, _, total) = svc.CalculatePrice(start, end);

        mins.Should().Be(1);
        total.Should().Be(0.15m);
    }
}
