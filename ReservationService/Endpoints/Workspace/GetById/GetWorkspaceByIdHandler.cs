using ReservationService.Endpoints.Workspace.Dtos;
using ReservationService.Exceptions;

namespace ReservationService.Endpoints.Workspace.GetById;

public class GetWorkspaceByIdHandler(IWorkspaceRepository repository,
    IValidator<GetWorkspaceByIdRequest> validator)
{
    public async Task<WorkspaceDto> GetWorkspaceByIdAsync(
        GetWorkspaceByIdRequest request, CancellationToken cancellationToken)
    {

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        var workspace = await repository.GetByIdAsync(request.Id, cancellationToken);
        return workspace is not null ? workspace.Adapt<WorkspaceDto>()
            : throw new NotFoundException("Место", request.Id);
    }
}
