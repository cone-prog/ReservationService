using ReservationService.Endpoints.Reservation.Create;

namespace ReservationService.Infrastructure.Repositories;

public interface IReservationRepository
{
    Task<bool> IsSlotAvailableAsync(Guid workspaceId,
        DateTime start, DateTime end, CancellationToken cancellationToken);

    Task<bool> CreateReservationAsync(CreateReservationDto dto, CancellationToken cancellationToken);
}
