using FluentValidation;
using FluentValidation.Results;
using Moq;
using ReservationService.Endpoints.Workspace.Get;
using ReservationService.Infrastructure.Repositories;
using ReservationService.Models;

namespace ReservationService.UnitTests.Workspaces;

public class GetWorkspacesHandlerTests
{
    [Fact]
    public async Task GetWorkspacesAsync_WhenRequestInvalid_ShouldThrowValidationException()
    {
        var request = new GetWorkspacesRequest(
            Name: "",
            IsActive: true,
            PageNumber: -1,
            PageSize: 5
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var validator = new Mock<IValidator<GetWorkspacesRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure("PageNumber", "Invalid")]));

        var handler = new GetWorkspacesHandler(
            workspaceRepository.Object,
            validator.Object);

        await Assert
            .ThrowsAsync<ValidationException>(
                () => handler.GetWorkspacesAsync(
                    request,
                    CancellationToken.None));

        workspaceRepository.Verify(r => r.GetAsync(
            It.IsAny<GetWorkspacesRequest>(),
            It.IsAny<CancellationToken>()), Times.Never
        );
    }

    [Fact]
    public async Task GetWorkspacesAsync_WhenRequestValid_ShouldReturnList()
    {
        var request = new GetWorkspacesRequest(
            Name: "",
            IsActive: true,
            PageNumber: 1,
            PageSize: 5
        );

        var workspaces = new List<Workspace>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Workspace 1",
                Description = "Test workspace 1",
                PricePerHour = 100m,
                IsActive = true
            },

            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Workspace 2",
                Description = "Test workspace 2",
                PricePerHour = 150m,
                IsActive = true
            }
        };

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var validator = new Mock<IValidator<GetWorkspacesRequest>>();

        workspaceRepository
            .Setup(r => r.GetAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspaces);

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());


        var handler = new GetWorkspacesHandler(
            workspaceRepository.Object,
            validator.Object);

        var result = await handler
            .GetWorkspacesAsync(
                request,
                CancellationToken.None);

        Assert.Equal(2, result.Count);

        Assert.Equal(workspaces[0].Id, result[0].Id);
        Assert.Equal(workspaces[0].Name, result[0].Name);
        Assert.Equal(workspaces[0].Description, result[0].Description);
        Assert.Equal(workspaces[0].PricePerHour, result[0].PricePerHour);
        Assert.Equal(workspaces[0].IsActive, result[0].IsActive);

        Assert.Equal(workspaces[1].Id, result[1].Id);
        Assert.Equal(workspaces[1].Name, result[1].Name);
        Assert.Equal(workspaces[1].Description, result[1].Description);
        Assert.Equal(workspaces[1].PricePerHour, result[1].PricePerHour);
        Assert.Equal(workspaces[1].IsActive, result[1].IsActive);

        workspaceRepository.Verify(r => r.GetAsync(
                request,
                CancellationToken.None),
            Times.Once);

        validator.Verify(
            v => v.ValidateAsync(
                request,
                CancellationToken.None),
            Times.Once);
    }
}
