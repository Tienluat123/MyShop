using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

/// <summary>
/// Chuyển đổi giữa DateTime (Model) và DateTimeOffset (CalendarDatePicker)
/// Giúp WinUI hiểu cách lưu dữ liệu từ nút chọn lịch vào Database
/// </summary>
public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    // Chuyển từ DateTime (Model) -> DateTimeOffset (UI)
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            if (dt == DateTime.MinValue) return null;
            
            // Đảm bảo không bị lỗi Out of Range của DateTimeOffset
            try { return new DateTimeOffset(dt); }
            catch { return DateTimeOffset.MinValue; }
        }
        return null;
    }

    // Chuyển từ DateTimeOffset (UI) -> DateTime (Model)
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dto)
        {
            return dto.DateTime;
        }
        return DateTime.MinValue;
    }
}
