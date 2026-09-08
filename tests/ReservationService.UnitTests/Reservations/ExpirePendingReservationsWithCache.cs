using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using ReservationService.Infrastructure.Repositories;
using ReservationService.Models;

namespace ReservationService.UnitTests.Reservations;

public class ExpirePendingReservationsWithCache
{
    [Fact]
    public async Task ExpireReservation_WhenExpiredTwoReservations_ShouldRemoveDateFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 28, 14, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 28, 16, 00, 00, TimeSpan.Zero);
        var reservations = new List<Reservation>()
        {
            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440000"),
                WorkspaceId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440002"),
                StartAt = new DateTimeOffset(2026, 10, 28, 14, 00, 00, TimeSpan.Zero).UtcDateTime,
                EndAt = new DateTimeOffset(2026, 10, 28, 16, 00, 00, TimeSpan.Zero).UtcDateTime,
                Status = ReservationStatus.Expired,
                CreatedAt = dateStart.UtcDateTime.AddHours(-1)
            },

            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440003"),
                WorkspaceId = Guid.Parse("350e8400-e29b-41d4-a716-446655440004"),
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440005"),
                StartAt = new DateTimeOffset(2026, 10, 29, 17, 00, 00, TimeSpan.Zero).UtcDateTime,
                EndAt = new DateTimeOffset(2026, 10, 29, 18, 00, 00, TimeSpan.Zero).UtcDateTime,
                Status = ReservationStatus.Expired,
                CreatedAt = dateStart.UtcDateTime.AddHours(-1)
            }
        };

        var dates = new List<DateOnly>()
        {
            new(2026,10,28),
            new(2026,10,29),
        };

        repository
            .Setup(r => r.ExpirePendingReservationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        await cacheRepository.ExpirePendingReservationsAsync(
            CancellationToken.None);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{reservations[0].WorkspaceId}:{dates[0]
                    .ToString("yyyy-MM-dd")}",
                    It.IsAny<CancellationToken>()),
                Times.Once);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{reservations[1].WorkspaceId}:{dates[1]
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
            r => r.ExpirePendingReservationsAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExpireReservation_WhenReservationEndMidnight_ShouldRemoveOneKeyFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 3, 22, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 4, 0, 00, 00, TimeSpan.Zero);

        var reservations = new List<Reservation>()
        {
            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440000"),
                WorkspaceId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
                StartAt = dateStart.UtcDateTime,
                EndAt = dateEnd.UtcDateTime,
                Status = ReservationStatus.Expired,
                CreatedAt = dateStart.UtcDateTime.AddHours(-1)
            }
        };

        repository
            .Setup(r => r.ExpirePendingReservationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        await cacheRepository.ExpirePendingReservationsAsync(
            CancellationToken.None);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{reservations[0].WorkspaceId}:{reservations[0].StartAt
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
            r => r.ExpirePendingReservationsAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExpireReservation_WhenReservationSpansTwoDates_ShouldRemoveTwoKeysFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 28, 22, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 29, 1, 00, 00, TimeSpan.Zero);

        var reservations = new List<Reservation>()
        {
            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440000"),
                WorkspaceId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
                StartAt = dateStart.UtcDateTime,
                EndAt = dateEnd.UtcDateTime,
                Status = ReservationStatus.Expired,
                CreatedAt = dateStart.UtcDateTime.AddHours(-1)
            },
        };

        repository
            .Setup(r => r.ExpirePendingReservationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        await cacheRepository.ExpirePendingReservationsAsync(
            CancellationToken.None);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{reservations[0].WorkspaceId}:{dateStart.Date
                    .ToString("yyyy-MM-dd")}",
                    It.IsAny<CancellationToken>()),
                Times.Once);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{reservations[0].WorkspaceId}:{dateEnd.Date
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
            r => r.ExpirePendingReservationsAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExpireReservation_WhenReservationSpansSeveralDates_ShouldRemoveSeveralKeysFromCache()
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

        var reservations = new List<Reservation>()
        {
            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440000"),
                WorkspaceId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440001"),
                StartAt = dateStart.UtcDateTime,
                EndAt = dateEnd.UtcDateTime,
                Status = ReservationStatus.Expired,
                CreatedAt = dateStart.UtcDateTime.AddHours(-1)
            }
        };

        repository
            .Setup(r => r.ExpirePendingReservationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        await cacheRepository.ExpirePendingReservationsAsync(
            CancellationToken.None);

        foreach (var date in dates)
        {
            distributedCache
                .Verify(d => d
                    .RemoveAsync($"availability:{reservations[0].WorkspaceId}:{date
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
            r => r.ExpirePendingReservationsAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
