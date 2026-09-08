using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using ReservationService.Endpoints.Reservation.Create;
using ReservationService.Infrastructure.Repositories;

namespace ReservationService.UnitTests.Reservations;

public class CreateReservationWithCache
{
    [Fact]
    public async Task CreateReservation_WhenAddedReservation_ShouldRemoveDateFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 28, 14, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 28, 16, 00, 00, TimeSpan.Zero);
        var dto = new CreateReservationDto(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            dateStart,
            dateEnd
        );

        repository
            .Setup(r => r.CreateReservationAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result = await cacheRepository.CreateReservationAsync(
            dto,
            CancellationToken.None);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{dto.WorkspaceId}:{new DateOnly(2026, 10, 28)
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
            r => r.CreateReservationAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WhenReservationEndMidnight_ShouldRemoveOneKeyFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 3, 22, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 4, 0, 00, 00, TimeSpan.Zero);

        var dto = new CreateReservationDto(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            dateStart,
            dateEnd
        );

        repository
            .Setup(r => r.CreateReservationAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result = await cacheRepository.CreateReservationAsync(
            dto,
            CancellationToken.None);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{dto.WorkspaceId}:{dateStart
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
            r => r.CreateReservationAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WhenReservationSpansTwoDates_ShouldRemoveTwoKeysFromCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IReservationRepository>();
        var logger = new Mock<ILogger<RedisReservationCacheRepository>>();

        var dateStart = new DateTimeOffset(2026, 10, 28, 22, 00, 00, TimeSpan.Zero);
        var dateEnd = new DateTimeOffset(2026, 10, 29, 1, 00, 00, TimeSpan.Zero);

        var dto = new CreateReservationDto(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            dateStart,
            dateEnd
        );

        repository
            .Setup(r => r.CreateReservationAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result = await cacheRepository.CreateReservationAsync(
            dto,
            CancellationToken.None);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{dto.WorkspaceId}:{dateStart.Date
                    .ToString("yyyy-MM-dd")}",
                    It.IsAny<CancellationToken>()),
                Times.Once);

        distributedCache
            .Verify(d => d
                .RemoveAsync($"availability:{dto.WorkspaceId}:{dateEnd.Date
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
            r => r.CreateReservationAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WhenReservationSpansSeveralDates_ShouldRemoveSeveralKeysFromCache()
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

        var dto = new CreateReservationDto(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            dateStart,
            dateEnd
        );

        repository
            .Setup(r => r.CreateReservationAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        distributedCache
            .Setup(c => c.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheRepository = new RedisReservationCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result = await cacheRepository.CreateReservationAsync(
            dto,
            CancellationToken.None);

        foreach (var date in dates)
        {
            distributedCache
                .Verify(d => d
                    .RemoveAsync($"availability:{dto.WorkspaceId}:{date
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
            r => r.CreateReservationAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
