using ReservationService.Endpoints.Confirm;
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

        var result = await repository.ConfirmReservationAsync(request.Id,
            cancellationToken);

        if (result.ResultType == ConfirmReservationResultType.NotFound)
            throw new NotFoundException("Бронирование", request.Id);

        if (result.ResultType == ConfirmReservationResultType.InvalidStatusChange)
            throw new ForbiddenChangeStatusException(result.CurrentStatus!.Value.ToString(),
                ReservationStatus.Confirmed.ToString());
    }
}
