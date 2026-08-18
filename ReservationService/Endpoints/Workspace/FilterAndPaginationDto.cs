namespace ReservationService.Endpoints.Workspace;

public record FilterAndPaginationDto
(
    string? Name,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 5
);
