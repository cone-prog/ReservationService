using ReservationService.Endpoints.Workspace;
using ReservationService.Models;

namespace ReservationService.Infrastructure.Repositories;

public interface IWorkspaceRepository
{
    Task<List<Workspace>> GetAsync(FilterAndPaginationDto filterAndPaginationDto,
        CancellationToken cancellationToken);

    Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

