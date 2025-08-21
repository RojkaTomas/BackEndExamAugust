namespace SmartParking_API.IntegrationTests;

public class ApiIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(ApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Api-Key", "dev-12345");
    }

    [Fact]
    public async Task Get_All_Parkings_Works()
    {
        // Seed one parking
        var create = new
        {
            name = "Test Parking",
            location = new { street = "Main 1", city = "City", postalCode = "1000", latitude = 50.0, longitude = 4.0 },
            spots = new object[] { }
        };

        var post = await _client.PostAsJsonAsync("/api/parkings", create);
        post.EnsureSuccessStatusCode();

        var res = await _client.GetAsync("/api/parkings");        
        res.EnsureSuccessStatusCode();
        var list = await res.Content.ReadFromJsonAsync<List<object>>();
        list.Should().NotBeNull();
        list!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_Parking_By_Id_Works_And_404s_For_Missing()
    {
        // Create
        var create = new
        {
            name = "Another Parking",
            location = new { street = "Main 2", city = "City", postalCode = "1000", latitude = 51.0, longitude = 5.0 },
            spots = new object[] { }
        };
        var post = await _client.PostAsJsonAsync("/api/parkings", create);
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var id = created!["id"];

        // Get existing
        var ok = await _client.GetAsync($"/api/parkings/{id}");
        ok.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Get non-existent
        var notFound = await _client.GetAsync($"/api/parkings/66aaaaaaaaaaaaaaaabbbbbbb");
        notFound.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
