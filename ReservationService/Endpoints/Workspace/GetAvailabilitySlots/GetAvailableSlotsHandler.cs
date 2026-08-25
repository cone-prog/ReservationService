using ReservationService.Exceptions;

namespace ReservationService.Endpoints.Workspace.GetAvailabilitySlots;

public class GetAvailableSlotsHandler(IWorkspaceRepository repository,
    IValidator<GetAvailableSlotsRequest> validator)
{
    public async Task<AvailableSlots> GetAvailabilitySlotsAsync(
        GetAvailableSlotsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!await repository.ExistsAsync(request.WorkspaceId, cancellationToken))
            throw new NotFoundException("Место", request.WorkspaceId);

        var slots = await repository.GetAvailabilitySlotsAsync(
            request.WorkspaceId, request.Date, cancellationToken);

        return new AvailableSlots(request.WorkspaceId, request.Date,
            slots);
    }
}
