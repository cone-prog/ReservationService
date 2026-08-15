namespace ReservationService;

public static class DependencyInjection
{
    public static IServiceCollection AddAllServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        return services;
    }

    public static WebApplication UseAppServices(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseHttpsRedirection();
        return app;
    }
}
