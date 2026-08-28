namespace ReservationService.Endpoints.Reservation.Confirm;

public class ConfirmReservationRequestValidator
    : AbstractValidator<ConfirmReservationRequest>
{
    public ConfirmReservationRequestValidator()
    {
        RuleFor(r => r.Id)
            .NotEmpty()
            .WithMessage("Id не может быть пустым");
    }
}
