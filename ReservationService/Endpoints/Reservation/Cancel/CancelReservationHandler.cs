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

        var result = await repository.CancelReservationAsync(request.Id,
            cancellationToken);

        if (result.ResultType == CancelReservationResultType.NotFound)
            throw new NotFoundException("Бронирование", request.Id);

        if (result.ResultType == CancelReservationResultType.InvalidStatusChange)
            throw new ForbiddenChangeStatusException(result.CurrentStatus!.Value.ToString(),
                ReservationStatus.Cancelled.ToString());

    }
}
