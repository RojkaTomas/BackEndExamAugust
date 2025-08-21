namespace SmartParking_API.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly bool _useLocalMongo =
        string.Equals(Environment.GetEnvironmentVariable("SP_ITEST_LOCAL"), "1", StringComparison.OrdinalIgnoreCase);

    private readonly string _dbName = $"smartparking_test_{Guid.NewGuid():N}";
    private MongoDbContainer? _mongo;
    private string? _connString;

    public ApiFactory()
    {
        if (_useLocalMongo)
        {
            // Use your own MongoDB on localhost:27017
            _connString = "mongodb://localhost:27017";
            return;
        }

        // New v3 builder API:
        _mongo = new MongoDbBuilder()
            .WithImage("mongo:7")        // optional; defaults to a stable tag
            .WithPortBinding(0, 27017)   // map container 27017 to a random host port
            .Build();
    }

    public string ConnectionString => _connString ?? _mongo!.GetConnectionString();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = ConnectionString,
                ["MongoDb:DatabaseName"]     = _dbName,
                ["Auth:ApiKey"]              = "dev-12345"
            };
            cfg.AddInMemoryCollection(dict!);
        });

        return base.CreateHost(builder);
    }

    public async Task InitializeAsync()
    {
        if (_useLocalMongo) return;
        await _mongo!.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_useLocalMongo) return;
        await _mongo!.DisposeAsync();
    }
}
