using FluentValidation.TestHelper;
using ReservationService.Endpoints.Workspace.GetById;

namespace ReservationService.UnitTests.Workspaces;

public class GetWorkspaceByIdValidatorTests
{
    [Fact]
    public async Task Validate_WhenIdEmpty_ShouldHaveValidationError()
    {
        var request = new GetWorkspaceByIdRequest(
            Guid.Empty
        );

        var validator = new GetWorkspaceByIdValidator();

        var validationResult = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None
        );

        validationResult
            .ShouldHaveValidationErrorFor(r => r.Id);
    }

    [Fact]
    public async Task Validate_WhenRequestValid_ShouldDontHaveValidationError()
    {
        var request = new GetWorkspaceByIdRequest(
            Guid.Parse("00000000-0000-0000-0000-000000000001")
        );

        var validator = new GetWorkspaceByIdValidator();

        var validationResult = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None
        );

        validationResult
            .ShouldNotHaveAnyValidationErrors();
    }
}
