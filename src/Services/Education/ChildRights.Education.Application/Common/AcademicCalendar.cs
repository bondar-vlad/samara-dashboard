namespace ChildRights.Education.Application.Common;

/// <summary>Helpers for reasoning about the Ukrainian academic year (starts 1 September).</summary>
public static class AcademicCalendar
{
    public static string CurrentPeriod(DateTime utcNow)
    {
        var startYear = utcNow.Month >= 9 ? utcNow.Year : utcNow.Year - 1;
        return $"{startYear}/{startYear + 1}";
    }
}
