using ReservationService.Exceptions;

namespace ReservationService.Endpoints.Reservation.Create;

public class CreateReservationHandler(
    IWorkspaceRepository workspaceRepository,
    IReservationRepository reservationRepository,
    IValidator<CreateReservationRequest> validator)
{
    public async Task<bool> CreateReservationAsync(CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var workspace = await workspaceRepository.GetByIdAsync(
            dto.WorkspaceId, cancellationToken);

        if (workspace is null)
            throw new NotFoundException("Место", dto.WorkspaceId);

        if (!workspace.IsActive)
            throw new WorkspaceNotActiveException(workspace.Name);

        var result = await reservationRepository.CreateReservationAsync(dto,
            cancellationToken);

        if (!result)
            throw new SlotNotAvailableException(workspace.Name, request.Dto.StartAt.UtcDateTime,
                request.Dto.EndAt.UtcDateTime);

        return result;
    }
}