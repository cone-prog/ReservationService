using FluentValidation.TestHelper;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;

namespace ReservationService.UnitTests.Workspaces;

public class GetAvailableSlotsRequestValidatorTests
{
    [Fact]
    public async Task Validate_WhenWorkspaceIdEmpty_ShouldHaveValidationError()
    {
        var request = new GetAvailableSlotsRequest(
            Guid.Empty,
            new DateOnly(2026, 8, 25)
        );

        var validator = new GetAvailableSlotsRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None);

        result.ShouldHaveValidationErrorFor(r => r.WorkspaceId);
    }

    [Fact]
    public async Task Validate_WhenRequestValid_ShouldNotHaveValidationErrors()
    {
        var request = new GetAvailableSlotsRequest(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            new DateOnly(2026, 8, 25)
        );

        var validator = new GetAvailableSlotsRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
