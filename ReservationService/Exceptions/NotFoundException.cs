namespace ReservationService.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string type, Guid id) : base(
        $"{type} с таким id:{id} не найдено"
    )
    { }

}
