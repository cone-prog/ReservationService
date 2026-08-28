namespace ReservationService.Exceptions;

public class ConfirmationForbiddenException : Exception
{
    public ConfirmationForbiddenException(string currentStatus)
        : base($"Невозможно подтвердить бронирование, т.к текущий статус {currentStatus}") { }
}
