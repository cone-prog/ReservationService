using ReservationService.Models;

namespace ReservationService.Endpoints.Results;

public class ChangeStatusResult
{
    public ChangeStatusResultType ResultType { get; }

    public ReservationStatus? Status { get; }

    private ChangeStatusResult(ChangeStatusResultType resultType, ReservationStatus? status = null)
    {
        if (resultType == ChangeStatusResultType.Success && status is not null)
            throw new ArgumentException("Успешная операция и не пустой статус недопустимы");

        if (resultType == ChangeStatusResultType.NotFound && status is not null)
            throw new ArgumentException("При ненайденном объекте статус бронирования должен быть null");

        if (resultType == ChangeStatusResultType.InvalidStatusChange && status is null)
            throw new ArgumentException("При недопустимой смене статусов, статус бронированияне должен быть null");

        ResultType = resultType;
        Status = status;
    }

    public static ChangeStatusResult Success() => new ChangeStatusResult(ChangeStatusResultType.Success);

    public static ChangeStatusResult NotFound() => new ChangeStatusResult(ChangeStatusResultType.NotFound);

    public static ChangeStatusResult InvalidStatusChange(ReservationStatus status) => new ChangeStatusResult(
        ChangeStatusResultType.InvalidStatusChange, status);
}

public enum ChangeStatusResultType
{
    NotFound,
    Success,
    InvalidStatusChange
}
