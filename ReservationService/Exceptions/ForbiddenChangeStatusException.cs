namespace ReservationService.Exceptions;

public class ForbiddenChangeStatusException : Exception
{
    public ForbiddenChangeStatusException(string currentStatus, string newStatus)
        : base($"Невозможно изменить статус бронирования с {currentStatus} на {newStatus}") { }
}
