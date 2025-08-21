var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Description = "Enter your API key (default: dev-12345)"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddMongo(builder.Configuration);

builder.Services.AddSingleton<IParkingRepository, ParkingRepository>();
builder.Services.AddSingleton<IReservationRepository, ReservationRepository>();
builder.Services.AddSingleton<IParkingService, ParkingService>();
builder.Services.AddSingleton<IReservationService, ReservationService>();

var app = builder.Build();

app.UseGlobalErrorHandling(app.Services.GetRequiredService<ILoggerFactory>());

app.UseSwagger();
app.UseSwaggerUI();

app.UseWhen(ctx =>
{
    var p = ctx.Request.Path;
    // Allow swagger UI + assets and the favicon
    return !p.StartsWithSegments("/swagger") &&
           !p.StartsWithSegments("/favicon.ico");
}, branch =>
{
    branch.UseApiKeyAuth();
});

// --- Endpoints ---
var parkings = app.MapGroup("/api/parkings").WithTags("Parkings");

parkings.MapGet("", async ([FromServices] IParkingService svc, CancellationToken ct) =>
{
    var items = await svc.GetAllAsync(ct);
    return Results.Ok(items);
});

parkings.MapGet("/{id}", async ([FromServices] IParkingService svc, string id, CancellationToken ct) =>
{
    var item = await svc.GetAsync(id, ct);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

parkings.MapPost("", async ([FromServices] IParkingService svc, [FromBody] CreateParkingRequest req, CancellationToken ct) =>
{
    try
    {
        var id = await svc.CreateAsync(req, ct);
        return Results.Created($"/api/parkings/{id}", new { id });
    }
    catch (ValidationException ex)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { { "validation", [ex.Message] } },
            statusCode: StatusCodes.Status400BadRequest);
    }
});

var spots = app.MapGroup("/api/parkingspots").WithTags("Parking Spots");

spots.MapPost("", async ([FromServices] IParkingService svc, [FromBody] CreateParkingSpotRequest req, CancellationToken ct) =>
{
    try
    {
        var ok = await svc.CreateSpotAsync(req, ct);
        return ok ? Results.Ok() : Results.NotFound(new { message = "Parking not found" });
    }
    catch (ValidationException ex)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { { "validation", [ex.Message] } },
            statusCode: StatusCodes.Status400BadRequest);
    }
});

var reservations = app.MapGroup("/api/reservations").WithTags("Reservations");

reservations.MapPost("/start", async ([FromServices] IReservationService svc, [FromBody] StartReservationRequest req, CancellationToken ct) =>
{
    try
    {
        var id = await svc.StartAsync(req, ct);
        return Results.Created($"/api/reservations/{id}", new { id });
    }
    catch (ValidationException ex)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { { "validation", [ex.Message] } },
            statusCode: StatusCodes.Status400BadRequest);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
    }
});

reservations.MapPost("/end", async ([FromServices] IReservationService svc, [FromBody] EndReservationRequest req, CancellationToken ct) =>
{
    try
    {
        var result = await svc.EndAsync(req, ct);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }
    catch (ValidationException ex)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { { "validation", [ex.Message] } },
            statusCode: StatusCodes.Status400BadRequest);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

app.Run();

public partial class Program { }
