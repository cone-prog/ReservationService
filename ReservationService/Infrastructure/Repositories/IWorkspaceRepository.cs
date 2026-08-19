using ReservationService.Endpoints.Workspace.Get;
using ReservationService.Models;

namespace ReservationService.Infrastructure.Repositories;

public interface IWorkspaceRepository
{
    Task<List<Workspace>> GetAsync(GetWorkspacesRequest filterAndPaginationDto,
        CancellationToken cancellationToken);

    Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

