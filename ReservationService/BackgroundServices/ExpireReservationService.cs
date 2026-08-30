namespace ReservationService.BackgroundServices;

public class ExpireReservationService(
    IServiceProvider serviceProvider,
    ILogger<ExpireReservationService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
            await using var scope = serviceProvider.CreateAsyncScope();
            var repository = scope.ServiceProvider
                .GetRequiredService<IReservationRepository>();
            try
            {
                await repository.ExpirePendingReservationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                if (!stoppingToken.IsCancellationRequested)
                    logger.LogError(ex, "Failed to expire pending reservations");
            }
        }
    }
}
