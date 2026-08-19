using FluentValidation;
using MapsterMapper;
using ReservationService.Endpoints.Workspace.Dtos;
using ReservationService.Infrastructure.Repositories;

namespace ReservationService.Endpoints.Workspace.Get;

public class GetWorkspacesHandler(IWorkspaceRepository workspaceRepository,
    GetWorkspacesRequestValidator validator)
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
