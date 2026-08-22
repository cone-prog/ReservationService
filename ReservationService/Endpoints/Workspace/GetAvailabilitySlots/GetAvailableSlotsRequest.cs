namespace ReservationService.Endpoints.Workspace.GetAvailabilitySlots;

public record GetAvailableSlotsRequest(
    Guid WorkspaceId,
    DateOnly Date
);
