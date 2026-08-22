using ReservationService.Endpoints.Workspace.Get;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Models;

namespace ReservationService.Infrastructure.Repositories;

public interface IWorkspaceRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Workspace>> GetAsync(GetWorkspacesRequest filterAndPaginationDto,
        CancellationToken cancellationToken);

    Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Slot>> GetAvailabilitySlotsAsync(Guid id,
        DateOnly date, CancellationToken cancellationToken);
}

