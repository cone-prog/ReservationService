namespace ReservationService.Endpoints.Workspace.Get;

public record GetWorkspacesRequest
(
    string? Name,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 5
);
