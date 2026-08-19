using Mapster;
using ReservationService.Endpoints.Workspace.Dtos;
using ReservationService.Models;

namespace ReservationService.Mapping;

public class RegisterMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Workspace, WorkspaceDto>()
            .RequireDestinationMemberSource(true);
    }
}
