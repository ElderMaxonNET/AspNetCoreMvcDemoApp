namespace AspNetCoreMvcDemoApp.Core.Common.Extensions
{
    public static class DateFormatExtensions
    {

        public static string FormatDate(this DateTime date)
        {
            return date.ToString("dd.MM.yyyy");
        }

        public static string? FormatDate(this DateTime? date)
        {
            return date.HasValue ? date.Value.FormatDate() : null;
        }

        public static string FormatDateTime(this DateTime date)
        {
            return date.ToString("dd.MM.yyyy HH:mm");
        }

        public static string? FormatDateTime(this DateTime? date)
        {
            return date.HasValue ? date.Value.FormatDateTime() : null;
        }

        public static DateTime GetWeekStart(this DateTime date)
        {
            int diff = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
            return date.Date.AddDays(-diff);
        }

        public static DateTime? GetDateFromTimeType(this int fastTimeType)
        {
            var now = DateTime.Now.Date;

            return fastTimeType switch
            {
                1 => now, // Today
                2 => now.GetWeekStart(), // This Week
                3 => new DateTime(now.Year, now.Month, 1), // This Month
                4 => new DateTime(now.Year, 1, 1), // This Year
                _ => null
            };
        }
    }
}
