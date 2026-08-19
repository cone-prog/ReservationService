namespace ReservationService.Endpoints.Workspace.Get;

public class GetWorkspacesRequestValidator : AbstractValidator<GetWorkspacesRequest>
{
    public GetWorkspacesRequestValidator()
    {
        RuleFor(r => r.PageNumber)
            .GreaterThan(0)
            .WithMessage("Номер страницы должен быть больше нуля");

        RuleFor(r => r.PageSize)
            .GreaterThan(0)
            .WithMessage("Размер страницы должен быть больше нуля")
            .LessThan(100)
            .WithMessage("размер страницы должен быть меньше 100");

        RuleFor(r => r.Name)
            .MaximumLength(30)
            .When(r => r.Name is not null)
            .WithMessage("Название места не может превышать 30 символов");
    }
}
