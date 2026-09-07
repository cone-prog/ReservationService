using Microsoft.Extensions.Caching.Distributed;
using ReservationService.Endpoints.Reservation.Create;
using ReservationService.Models;
using StackExchange.Redis;

namespace ReservationService.Infrastructure.Repositories;

public class RedisReservationCacheRepository(IReservationRepository reservationRepository,
    IDistributedCache distributedCache, ILogger<RedisReservationCacheRepository> logger)
        : IReservationRepository
{
    public async Task<Reservation?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken)
    {
        return await reservationRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> CreateReservationAsync(CreateReservationDto dto,
        CancellationToken cancellationToken)
    {
        var result = await reservationRepository.CreateReservationAsync(dto, cancellationToken);
        if (!result) return false;
        var dates = GetAllDates(dto.StartAt.UtcDateTime, dto.EndAt.UtcDateTime);

        foreach (var date in dates)
        {
            try
            {
                var key = $"availability:{dto.WorkspaceId}:{date}";
                await distributedCache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(
                    ex,
                    "Failed to read availability from Redis for workspace {WorkspaceId} on {Date}",
                    dto.WorkspaceId,
                    date);
            }
        }
        return result;
    }

    public async Task ConfirmReservationAsync(Guid id, CancellationToken cancellationToken)
    {
        await reservationRepository.ConfirmReservationAsync(id, cancellationToken);
    }

    public async Task CancelReservationAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        await reservationRepository.CancelReservationAsync(reservation, cancellationToken);

        var dates = GetAllDates(reservation.StartAt, reservation.EndAt);

        foreach (var date in dates)
        {
            var repeatCount = 0;
            do
            {
                try
                {
                    var key = $"availability:{reservation.WorkspaceId}:{date}";
                    await distributedCache.RemoveAsync(key, cancellationToken);
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(
                        ex,
                        "Failed to invalidate availability from Redis for workspace {WorkspaceId} on {Date}",
                        reservation.WorkspaceId,
                        date);

                    repeatCount++;
                    if (repeatCount != 3) await Task.Delay(1000, cancellationToken);
                }
            }
            while (repeatCount < 3);
        }

    }

    public async Task<List<Reservation>> ExpirePendingReservationsAsync(CancellationToken cancellationToken)
    {
        var reservations = await reservationRepository
            .ExpirePendingReservationsAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            var dates = GetAllDates(reservation.StartAt, reservation.EndAt);

            foreach (var date in dates)
            {
                var repeatCount = 0;
                do
                {
                    try
                    {
                        var key = $"availability:{reservation.WorkspaceId}:{date}";
                        await distributedCache.RemoveAsync(key, cancellationToken);
                        break;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogWarning(
                            ex,
                            "Failed to invalidate availability from Redis for workspace {WorkspaceId} on {Date}",
                            reservation.WorkspaceId,
                            date);

                        repeatCount++;
                        if (repeatCount != 3) await Task.Delay(1000, cancellationToken);
                    }
                }
                while (repeatCount < 3);
            }
        }

        return reservations;
    }

    private List<string> GetAllDates(DateTime startingDate,
        DateTime endingDate)
    {
        var allDates = new List<string>();
        startingDate = startingDate.Date;
        for (DateTime date = startingDate; date < endingDate; date = date.AddDays(1))
            allDates.Add(date.ToString("yyyy-MM-dd"));

        return allDates;
    }
}
