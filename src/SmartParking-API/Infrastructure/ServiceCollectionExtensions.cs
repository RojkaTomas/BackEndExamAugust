namespace SmartParking_API.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration config)
    {
        var settings = config.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>() ?? new MongoDbSettings();
        services.AddSingleton(settings);

        services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName));

        services.AddSingleton(sp =>
            sp.GetRequiredService<IMongoDatabase>().GetCollection<Parking>("parkings"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<IMongoDatabase>().GetCollection<Reservation>("reservations"));

        return services;
    }
}
