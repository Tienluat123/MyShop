namespace MyShop.Helpers;

public static class DateHelper
{
    public static DateOnly GetFirstDayOfWeek(DateTime date)
    {
        // Logic tìm ngày Thứ 2 gần nhất
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return DateOnly.FromDateTime(date.AddDays(-1 * diff));
    }
}