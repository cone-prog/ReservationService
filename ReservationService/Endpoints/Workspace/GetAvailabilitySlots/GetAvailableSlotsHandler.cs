namespace ReservationService.Endpoints.Workspace.GetAvailabilitySlots;

public class GetAvailableSlotsHandler(IWorkspaceRepository repository,
    GetAvailableSlotsRequestValidator validator)
{
    public async Task<AvailableSlots> GetAvailabilitySlotsAsync(
        GetAvailableSlotsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        var slots = await repository.GetAvailabilitySlotsAsync(
            request.WorkspaceId, request.Date, cancellationToken);
        return new AvailableSlots(request.WorkspaceId, request.Date,
            slots);
    }
}
