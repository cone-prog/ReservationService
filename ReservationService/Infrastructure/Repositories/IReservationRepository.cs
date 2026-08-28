using ReservationService.Endpoints.Reservation.Create;
using ReservationService.Models;

namespace ReservationService.Infrastructure.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken);

    Task<bool> IsSlotAvailableAsync(Guid workspaceId,
        DateTime start, DateTime end, CancellationToken cancellationToken);

    Task<bool> CreateReservationAsync(CreateReservationDto dto,
        CancellationToken cancellationToken);

    Task ConfirmReservationAsync(Guid id,
        CancellationToken cancellationToken);
}
