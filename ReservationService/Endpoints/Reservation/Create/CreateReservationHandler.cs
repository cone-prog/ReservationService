using ReservationService.Exceptions;

namespace ReservationService.Endpoints.Reservation.Create;

public class CreateReservationHandler(
    IWorkspaceRepository workspaceRepository,
    IReservationRepository reservationRepository,
    CreateReservationRequestValidator validator)
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

        var StartAtUtc = dto.StartAt.UtcDateTime;
        var EndAtUtc = dto.EndAt.UtcDateTime;

        if (!await reservationRepository.IsSlotAvailableAsync(dto.WorkspaceId, StartAtUtc,
            EndAtUtc, cancellationToken))
            throw new SlotNotAvailableException(workspace.Name, StartAtUtc, EndAtUtc);

        var result = await reservationRepository.CreateReservationAsync(dto,
            cancellationToken);

        return result;
    }
}