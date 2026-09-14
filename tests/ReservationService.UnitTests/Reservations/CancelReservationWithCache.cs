using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using ReservationService.Endpoints.Confirm;
using ReservationService.Endpoints.Reservation.Cancel;
using ReservationService.Infrastructure.Repositories;
using ReservationService.Models;

namespace ReservationService.UnitTests.Reservations;

public class CancelReservationWithCache
{
    [Fact]
    public async Task CancelReservation_WhenCancelledReservation_ShouldRemoveDateFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 28, 14, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 28, 16, 00, 00, TimeSpan.Zero);
        var reservation = new Reservation()
        {
            Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440000"),
            WorkspaceId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
            UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
            StartAt = dateStart.UtcDateTime,
            EndAt = dateEnd.UtcDateTime,
            Status = ReservationStatus.Confirmed,
            CreatedAt = dateStart.UtcDateTime.AddHours(-1)
        };

        repository
            .Setup(r => r.CancelReservationAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CancelReservationResult.Success(reservation.WorkspaceId,
                reservation.StartAt, reservation.EndAt));

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result = await cacheRepository.CancelReservationAsync(
            reservation.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{reservation.WorkspaceId}:{new DateOnly(2026, 10, 28)
                    .ToString("yyyy-MM-dd")}",
                    It.IsAny<CancellationToken>()),
                Times.Once);

        distributedCache
                 .Verify(d => d
                     .RemoveAsync(
                         It.IsAny<string>(),
                         It.IsAny<CancellationToken>()),
                     Times.Once);

        repository.Verify(
            r => r.CancelReservationAsync(
                reservation.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelReservation_WhenReservationEndMidnight_ShouldRemoveOneKeyFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 3, 22, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 4, 0, 00, 00, TimeSpan.Zero);

        var reservation = new Reservation()
        {
            Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440000"),
            WorkspaceId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
            UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
            StartAt = dateStart.UtcDateTime,
            EndAt = dateEnd.UtcDateTime,
            Status = ReservationStatus.Confirmed,
            CreatedAt = dateStart.UtcDateTime.AddHours(-1)
        };

        repository
            .Setup(r => r.CancelReservationAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CancelReservationResult.Success(reservation.WorkspaceId,
                reservation.StartAt, reservation.EndAt));

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result = await cacheRepository.CancelReservationAsync(
            reservation.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{reservation.WorkspaceId}:{dateStart
                    .ToString("yyyy-MM-dd")}",
                    It.IsAny<CancellationToken>()),
                Times.Once);

        distributedCache
            .Verify(d => d
                .RemoveAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

        repository.Verify(
            r => r.CancelReservationAsync(
                reservation.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelReservation_WhenReservationSpansTwoDates_ShouldRemoveTwoKeysFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 28, 22, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 29, 1, 00, 00, TimeSpan.Zero);

        var reservation = new Reservation()
        {
            Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440000"),
            WorkspaceId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
            UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
            StartAt = dateStart.UtcDateTime,
            EndAt = dateEnd.UtcDateTime,
            Status = ReservationStatus.Confirmed,
            CreatedAt = dateStart.UtcDateTime.AddHours(-1)
        };

        repository
            .Setup(r => r.CancelReservationAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CancelReservationResult.Success(reservation.WorkspaceId,
                reservation.StartAt, reservation.EndAt));

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result = await cacheRepository.CancelReservationAsync(
            reservation.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{reservation.WorkspaceId}:{dateStart.Date
                    .ToString("yyyy-MM-dd")}",
                    It.IsAny<CancellationToken>()),
                Times.Once);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{reservation.WorkspaceId}:{dateEnd.Date
                    .ToString("yyyy-MM-dd")}",
                    It.IsAny<CancellationToken>()),
                Times.Once);

        distributedCache
                .Verify(d => d
                    .RemoveAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()),
                    Times.Exactly(2));

        repository.Verify(
            r => r.CancelReservationAsync(
                reservation.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelReservation_WhenReservationSpansSeveralDates_ShouldRemoveSeveralKeysFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 3, 22, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 7, 1, 00, 00, TimeSpan.Zero);

        var dates = new List<DateOnly>()
        {
            new DateOnly(2026,10,3),
            new DateOnly(2026,10,4),
            new DateOnly(2026,10,5),
            new DateOnly(2026,10,6),
            new DateOnly(2026,10,7),
        };

        var reservation = new Reservation()
        {
            Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440000"),
            WorkspaceId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
            UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
            StartAt = dateStart.UtcDateTime,
            EndAt = dateEnd.UtcDateTime,
            Status = ReservationStatus.Confirmed,
            CreatedAt = dateStart.UtcDateTime.AddHours(-1)
        };

        repository
            .Setup(r => r.CancelReservationAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CancelReservationResult.Success(reservation.WorkspaceId,
                reservation.StartAt, reservation.EndAt));


        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result = await cacheRepository.CancelReservationAsync(
            reservation.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        foreach (var date in dates)
        {
            distributedCache
                .Verify(d => d
                    .RemoveAsync($"availability:{reservation.WorkspaceId}:{date
                        .ToString("yyyy-MM-dd")}",
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        distributedCache
                .Verify(d => d
                    .RemoveAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()),
                    Times.Exactly(dates.Count));

        repository.Verify(
            r => r.CancelReservationAsync(
                reservation.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
