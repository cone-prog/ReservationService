namespace ReservationService.Endpoints.Workspace.GetAvailabilitySlots;

public class GetAvailableSlotsRequestValidator
    : AbstractValidator<GetAvailableSlotsRequest>
{
    public GetAvailableSlotsRequestValidator()
    {
        RuleFor(r => r.WorkspaceId)
            .NotEmpty()
            .WithMessage("Id не может быть пустым");
    }
}
