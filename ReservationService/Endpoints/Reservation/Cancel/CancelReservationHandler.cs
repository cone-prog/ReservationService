using ReservationService.Exceptions;
using ReservationService.Models;

namespace ReservationService.Endpoints.Reservation.Cancel;

public class CancelReservationHandler(IReservationRepository repository,
    IValidator<CancelReservationRequest> validator)
{
    public async Task CancelReservationAsync(CancelReservationRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request,
            cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var reservation = await repository.GetByIdAsync(request.Id,
            cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Бронирование", request.Id);

        if (!(reservation.Status == ReservationStatus.Pending
            || reservation.Status == ReservationStatus.Confirmed))
            throw new ForbiddenChangeStatusException(reservation.Status.ToString(),
                ReservationStatus.Cancelled.ToString());

        await repository.CancelReservationAsync(request.Id, cancellationToken);
    }
}
