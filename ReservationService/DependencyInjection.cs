using ReservationService.Endpoints.Workspace.Get;
using ReservationService.Endpoints.Workspace.GetById;
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
        services.AddCarter();
        services.AddScoped<GetWorkspacesHandler>();
        services.AddScoped<GetWorkspaceByIdHandler>();
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
        app.UseHttpsRedirection();
        return app;
    }
}
