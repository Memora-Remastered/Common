namespace Common.Services;

public class DateTimeService : IDateTimeService
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
