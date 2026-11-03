namespace Common.Interfaces;

public interface IDateTimeService
{
    DateTimeOffset UtcNow { get; }
}
