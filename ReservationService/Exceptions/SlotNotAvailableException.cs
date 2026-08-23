namespace ReservationService.Exceptions;

public class SlotNotAvailableException : Exception
{
    public SlotNotAvailableException(string workspaceName, DateTime start, DateTime end)
        : base($"На {workspaceName} есть брони на период {start} - {end}") { }
}
