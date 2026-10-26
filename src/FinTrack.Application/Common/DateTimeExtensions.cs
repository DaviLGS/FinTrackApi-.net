namespace FinTrack.Application.Common;

public static class DateTimeExtensions
{
    /// <summary>
    /// Normaliza para UTC (o Npgsql exige Kind = Utc em colunas timestamptz).
    /// Datas sem fuso (Unspecified) são tratadas como já estando em UTC.
    /// </summary>
    public static DateTime ParaUtc(this DateTime data) => data.Kind switch
    {
        DateTimeKind.Utc => data,
        DateTimeKind.Local => data.ToUniversalTime(),
        _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
    };
}
