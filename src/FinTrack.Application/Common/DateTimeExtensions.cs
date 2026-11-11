namespace FinTrack.Application.Common;

public static class DateTimeExtensions
{
    public static DateTime ParaUtc(this DateTime data) => data.Kind switch
    {
        DateTimeKind.Utc => data,
        DateTimeKind.Local => data.ToUniversalTime(),
        _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
    };
}
