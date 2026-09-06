using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ReservationService.Endpoints.Workspace.Get;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Models;

namespace ReservationService.Infrastructure.Repositories;

public class RedisWorkspaceCacheRepository(IWorkspaceRepository workspaceRepository,
    IDistributedCache distributedCache, ILogger<RedisWorkspaceCacheRepository> logger)
    : IWorkspaceRepository
{
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await workspaceRepository.ExistsAsync(id, cancellationToken);
    }

    public async Task<List<Workspace>> GetAsync(GetWorkspacesRequest filterAndPaginationDto,
        CancellationToken cancellationToken)
    {
        return await workspaceRepository.GetAsync(filterAndPaginationDto, cancellationToken);
    }

    public async Task<List<Slot>> GetAvailabilitySlotsAsync(Guid id, DateOnly date,
        CancellationToken cancellationToken)
    {
        var key = $"availability:{id}:{date.ToString("yyyy-MM-dd")}";
        byte[]? availabilitySlots = null;

        try
        {
            availabilitySlots = await distributedCache.GetAsync(key,
                cancellationToken);
        }

        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "Failed to read availability from Redis for workspace {WorkspaceId} on {Date}",
                id,
                date);
        }


        if (availabilitySlots is not null)
        {
            var slots = JsonSerializer.Deserialize<List<Slot>>(availabilitySlots);
            return slots!;
        }

        var result = await workspaceRepository.GetAvailabilitySlotsAsync(id,
            date, cancellationToken);
        var value = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));

        try
        {
            await distributedCache.SetAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            },
                cancellationToken);
        }

        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "Failed to write availability from Redis for workspace {WorkspaceId} on {Date}",
                id,
                date);
        }

        return result;
    }

    public async Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await workspaceRepository.GetByIdAsync(id, cancellationToken);
    }
}
