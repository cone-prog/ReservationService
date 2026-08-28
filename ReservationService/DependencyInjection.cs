using ReservationService.Endpoints.Reservation.Confirm;
using ReservationService.Endpoints.Reservation.Create;
using ReservationService.Endpoints.Workspace.Get;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Endpoints.Workspace.GetById;
using ReservationService.Handlers;
using ReservationService.Infrastructure.Context;
using ReservationService.Infrastructure.Extensions;

namespace ReservationService;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DapperContext>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddCarter();
        services.AddScoped<GetWorkspacesHandler>();
        services.AddScoped<GetWorkspaceByIdHandler>();
        services.AddScoped<GetAvailableSlotsHandler>();
        services.AddScoped<CreateReservationHandler>();
        services.AddScoped<ConfirmReservationHandler>();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        var assembly = typeof(Program).Assembly;
        TypeAdapterConfig.GlobalSettings.Scan(assembly);
        services.AddValidatorsFromAssembly(assembly);
        services.AddOpenApi();
        return services;
    }

    public static async Task<WebApplication> UseApiServices(this WebApplication app)
    {
        await Initial.InitialDataBaseAsync(app.Configuration, app.Lifetime.ApplicationStopping);
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapCarter();
        app.UseExceptionHandler(options => { });
        app.UseHttpsRedirection();
        return app;
    }
}
