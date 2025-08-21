namespace SmartParking_API.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbTestcontainer _mongo;

    public ApiFactory()
    {
        _mongo = new TestcontainersBuilder<MongoDbTestcontainer>()
            .WithImage("mongo:7")
            .WithDatabase(new MongoDbTestcontainerConfiguration())
            .Build();
    }

    public string ConnectionString => _mongo.ConnectionString;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = ConnectionString,
                ["MongoDb:DatabaseName"] = "smartparking_test",
                ["Auth:ApiKey"] = "dev-12345"
            };
            cfg.AddInMemoryCollection(dict!);
        });

        return base.CreateHost(builder);
    }

    public async Task InitializeAsync() => await _mongo.StartAsync();
    public new async Task DisposeAsync() => await _mongo.DisposeAsync();
}
