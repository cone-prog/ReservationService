using ReservationService.Infrastructure.Context;
using ReservationService.Infrastructure.Extensions;
using ReservationService.Infrastructure.Repositories;

namespace ReservationService;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DapperContext>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddOpenApi();
        return services;
    }

    public static async Task<WebApplication> UseApiServices(this WebApplication app)
    {
        await Initial.InitialDataBaseAsync(app.Configuration, app.Lifetime.ApplicationStopping);
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseHttpsRedirection();
        return app;
    }
}
