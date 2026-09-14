using ReservationService.Models;

namespace ReservationService.Endpoints.Reservation.Cancel;

public class CancelReservationResult
{
    public CancelReservationResultType ResultType { get; }

    public ReservationStatus? CurrentStatus { get; }

    public Guid? WorkspaceId { get; }

    public DateTime? StartAt { get; }

    public DateTime? EndAt { get; }

    private CancelReservationResult(
        CancelReservationResultType resultType,
        ReservationStatus? currentStatus = null,
        Guid? workspaceId = null,
        DateTime? startAt = null,
        DateTime? endAt = null)
    {
        if (resultType == CancelReservationResultType.Success &&
            (currentStatus is not null ||
             workspaceId is null ||
             startAt is null ||
             endAt is null))
        {
            throw new ArgumentException(
                "Для Success должны быть указаны WorkspaceId, StartAt и EndAt.");
        }

        if (resultType == CancelReservationResultType.NotFound &&
            (currentStatus is not null ||
             workspaceId is not null ||
             startAt is not null ||
             endAt is not null))
        {
            throw new ArgumentException(
                "Для NotFound дополнительные данные должны отсутствовать.");
        }

        if (resultType == CancelReservationResultType.InvalidStatusChange &&
            (currentStatus is null ||
             workspaceId is not null ||
             startAt is not null ||
             endAt is not null))
        {
            throw new ArgumentException(
                "Для InvalidStatusChange должен быть указан только CurrentStatus.");
        }

        ResultType = resultType;
        CurrentStatus = currentStatus;
        WorkspaceId = workspaceId;
        StartAt = startAt;
        EndAt = endAt;
    }

    public bool IsSuccess => ResultType == CancelReservationResultType.Success;

    public static CancelReservationResult Success(
    Guid workspaceId,
    DateTime startAt,
    DateTime endAt)
    => new(
        CancelReservationResultType.Success,
        workspaceId: workspaceId,
        startAt: startAt,
        endAt: endAt);

    public static CancelReservationResult NotFound()
        => new(CancelReservationResultType.NotFound);

    public static CancelReservationResult InvalidStatusChange(
        ReservationStatus currentStatus)
        => new(
            CancelReservationResultType.InvalidStatusChange,
            currentStatus: currentStatus);
}

public enum CancelReservationResultType
{
    NotFound,
    Success,
    InvalidStatusChange
}

