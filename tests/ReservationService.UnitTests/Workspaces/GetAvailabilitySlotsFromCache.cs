using System.Text;
using System.Text.Json;
using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Infrastructure.Repositories;

namespace ReservationService.UnitTests.Workspaces;

public class GetAvailableSlotsFromCache
{
    [Fact]
    public async Task GetAvailabilitySlots_WhenTwoRequests_ShouldSendOneRequestToDatabase()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IWorkspaceRepository>();
        var logger = new Mock<ILogger<RedisWorkspaceCacheRepository>>();

        var id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var date = new DateOnly(2026, 10, 28);
        var key = $"availability:{id}:{date.ToString("yyyy-MM-dd")}";

        var result = new List<Slot>()
        {
            new Slot(
                new DateTime(date, new TimeOnly(14, 0)),
                new DateTime(date, new TimeOnly(16, 0))),

            new Slot(
                new DateTime(date, new TimeOnly(17, 0)),
                new DateTime(date, new TimeOnly(18, 0)))
        };
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));

        distributedCache
            .SetupSequence(d => d.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null)
            .ReturnsAsync(bytes);

        repository
            .Setup(r => r.GetAvailabilitySlotsAsync(id, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var cacheRepository = new RedisWorkspaceCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result1 = await cacheRepository.GetAvailabilitySlotsAsync(id, date, CancellationToken.None);
        var result2 = await cacheRepository.GetAvailabilitySlotsAsync(id, date, CancellationToken.None);

        Assert.Equal(result, result1);
        Assert.Equal(result, result2);

        repository
            .Verify(r => r.GetAvailabilitySlotsAsync(
                id, date, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAvailabilitySlots_WhenCacheMiss_ShouldSaveResultToCache()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var repository = new Mock<IWorkspaceRepository>();
        var logger = new Mock<ILogger<RedisWorkspaceCacheRepository>>();

        var id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var date = new DateOnly(2026, 10, 28);
        var key = $"availability:{id}:{date.ToString("yyyy-MM-dd")}";

        var result = new List<Slot>()
        {
            new Slot(
                new DateTime(date, new TimeOnly(14, 0)),
                new DateTime(date, new TimeOnly(16, 0))),

            new Slot(
                new DateTime(date, new TimeOnly(17, 0)),
                new DateTime(date, new TimeOnly(18, 0)))
        };

        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));

        distributedCache
            .Setup(d => d.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        repository
            .Setup(r => r.GetAvailabilitySlotsAsync(id, date,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var cacheRepository = new RedisWorkspaceCacheRepository(repository.Object,
            distributedCache.Object, logger.Object);

        var result1 = await cacheRepository.GetAvailabilitySlotsAsync(id, date, CancellationToken.None);

        Assert.Equal(result, result1);

        distributedCache
            .Verify(d => d.SetAsync(key,
            It.Is<byte[]>(x => x.SequenceEqual(bytes)),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        repository
            .Verify(r => r.GetAvailabilitySlotsAsync(
                id, date, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
