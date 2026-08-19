namespace ReservationService.Endpoints.Workspace.Dtos;

public record WorkspaceDto(
    Guid Id,
    string Name,
    string Description,
    decimal PricePerHour,
    bool IsActive
);
