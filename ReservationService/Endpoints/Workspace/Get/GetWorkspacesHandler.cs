using ReservationService.Endpoints.Workspace.Dtos;

namespace ReservationService.Endpoints.Workspace.Get;

public class GetWorkspacesHandler(IWorkspaceRepository workspaceRepository,
     IValidator<GetWorkspacesRequest> validator)
{
    public async Task<List<WorkspaceDto>> GetWorkspacesAsync(GetWorkspacesRequest requestDto,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(requestDto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        var workspaces = await workspaceRepository.GetAsync(requestDto,
            cancellationToken);
        return workspaces.Select(w => w.Adapt<WorkspaceDto>()).ToList();
    }
}
