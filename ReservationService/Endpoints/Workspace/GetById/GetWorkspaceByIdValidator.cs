namespace ReservationService.Endpoints.Workspace.GetById;

public class GetWorkspaceByIdValidator
    : AbstractValidator<GetWorkspaceByIdRequest>
{
    public GetWorkspaceByIdValidator()
    {
        RuleFor(r => r.Id)
            .NotEmpty()
            .WithMessage("Id не может быть пустым");
    }
}
