namespace ReservationService.Endpoints.Reservation.Cancel;

public class CancelReservationRequestValidator
    : AbstractValidator<CancelReservationRequest>
{
    public CancelReservationRequestValidator()
    {
        RuleFor(r => r.Id)
            .NotEmpty()
            .WithMessage("Id не может быть пустым");
    }
}
