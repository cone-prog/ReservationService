namespace ReservationService.Exceptions;

public class WorkspaceNotActiveException : Exception
{
    public WorkspaceNotActiveException(string name)
        : base($"Место с названием '{name}' сейчас не активно") { }
}
