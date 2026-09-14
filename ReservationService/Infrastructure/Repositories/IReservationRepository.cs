using ReservationService.Endpoints.Reservation.Cancel;
using ReservationService.Endpoints.Reservation.Create;
using ReservationService.Endpoints.Confirm;
using ReservationService.Models;

namespace ReservationService.Infrastructure.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken);

    Task<bool> CreateReservationAsync(CreateReservationDto dto,
        CancellationToken cancellationToken);

    Task<ConfirmReservationResult> ConfirmReservationAsync(Guid id,
        CancellationToken cancellationToken);

    Task<CancelReservationResult> CancelReservationAsync(Guid id,
        CancellationToken cancellationToken);

    Task<List<Reservation>> ExpirePendingReservationsAsync(
        CancellationToken cancellationToken);
}
