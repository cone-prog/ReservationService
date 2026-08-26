namespace ReservationService.Endpoints.Reservation.Create;

public class CreateReservationRequestValidator
    : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(r => r.Dto.WorkspaceId)
            .NotEmpty()
            .WithMessage("Id места не может быть пустым");

        RuleFor(r => r.Dto.UserId)
            .NotEmpty()
            .WithMessage("Id пользователя не может быть пустым");

        RuleFor(r => r.Dto.EndAt)
            .GreaterThan(r => r.Dto.StartAt)
            .WithMessage("Окончание бронирования должно быть позже его начала");

        RuleFor(r => r.Dto.StartAt)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Бронь на уже прошедший период невозможна");
    }
}
