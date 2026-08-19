using ReservationService.Endpoints.Workspace.Dtos;
using ReservationService.Infrastructure.Repositories;

namespace ReservationService.Endpoints.Workspace.GetById;

public class GetWorkspaceByIdHandler(IWorkspaceRepository repository,
    GetWorkspaceByIdValidator validator)
{
    public async Task<WorkspaceDto> GetWorkspaceByIdAsync(
        GetWorkspaceByIdRequest request, CancellationToken cancellationToken)
    {
        var resultValidator = await validator.ValidateAsync(request, cancellationToken);
        if (!resultValidator.IsValid)
            throw new ValidationException(resultValidator.Errors);
        var workspace = await repository.GetByIdAsync(request.Id, cancellationToken);
        return workspace is not null ? workspace.Adapt<WorkspaceDto>() : throw new Exception();
    }
}
