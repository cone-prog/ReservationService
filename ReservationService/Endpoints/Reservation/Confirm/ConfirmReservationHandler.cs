using ReservationService.Exceptions;
using ReservationService.Models;

namespace ReservationService.Endpoints.Reservation.Confirm;

public class ConfirmReservationHandler(
    IReservationRepository repository,
    IValidator<ConfirmReservationRequest> validator)
{
    public async Task ConfirmReservationAsync(
        ConfirmReservationRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request,
            cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var reservation = await repository.GetByIdAsync(request.Id,
            cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Бронирование", request.Id);

        if (reservation.Status != ReservationStatus.Pending)
            throw new ConfirmationForbiddenException(reservation.Status.ToString());

        await repository.ConfirmReservationAsync(request.Id, cancellationToken);
    }
}
