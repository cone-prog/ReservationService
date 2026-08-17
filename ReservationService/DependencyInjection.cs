using ReservationService.Infrastructure.Extensions;

namespace ReservationService;

public static class DependencyInjection
{
    public static async Task<IServiceCollection> AddApiServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        await Initial.InitialDataBaseAsync(configuration, new CancellationToken());
        services.AddOpenApi();
        return services;
    }

    public static async Task<WebApplication> UseApiServices(this WebApplication app)
    {
        var connectionString = app.Configuration.GetConnectionString("PgConnection");
        await Initial.InitialDataBaseAsync(app.Configuration, app.Lifetime.ApplicationStopping);
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseHttpsRedirection();
        return app;
    }
}
