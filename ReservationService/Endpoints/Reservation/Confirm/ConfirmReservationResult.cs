using ReservationService.Models;

namespace ReservationService.Endpoints.Confirm;

public class ConfirmReservationResult
{
    public ConfirmReservationResultType ResultType { get; }

    public ReservationStatus? CurrentStatus { get; }

    private ConfirmReservationResult(ConfirmReservationResultType resultType, ReservationStatus? currentStatus = null)
    {
        if (resultType == ConfirmReservationResultType.Success && currentStatus is not null)
            throw new ArgumentException("Успешная операция и не пустой статус недопустимы");

        if (resultType == ConfirmReservationResultType.NotFound && currentStatus is not null)
            throw new ArgumentException("При ненайденном объекте статус бронирования должен быть null");

        if (resultType == ConfirmReservationResultType.InvalidStatusChange && currentStatus is null)
            throw new ArgumentException("При недопустимой смене статусов, статус бронированияне должен быть null");

        ResultType = resultType;
        CurrentStatus = currentStatus;
    }

    public bool IsSuccess => ResultType == ConfirmReservationResultType.Success;

    public static ConfirmReservationResult Success() => new ConfirmReservationResult(ConfirmReservationResultType.Success);

    public static ConfirmReservationResult NotFound() => new ConfirmReservationResult(ConfirmReservationResultType.NotFound);

    public static ConfirmReservationResult InvalidStatusChange(ReservationStatus status) => new ConfirmReservationResult(
        ConfirmReservationResultType.InvalidStatusChange, status);
}

public enum ConfirmReservationResultType
{
    NotFound,
    Success,
    InvalidStatusChange
}
