namespace SmartParking_API.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbTestcontainer _mongo;

    public ApiFactory()
    {
        var cfg = new MongoDbTestcontainerConfiguration
        {
            Database = "smartparking_test",
            Username = "root",            // required on 1.7.x
            Password = "secret"           // required on 1.7.x
        };

        _mongo = new TestcontainersBuilder<MongoDbTestcontainer>()
            .WithImage("mongo:7")
            // these env vars are what the image expects for root user + default DB
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", cfg.Username)
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", cfg.Password)
            .WithEnvironment("MONGO_INITDB_DATABASE", cfg.Database)
            .WithDatabase(cfg)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017)) // optional but helps on Windows
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
