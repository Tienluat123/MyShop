using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System.Collections;
using MyShop.Models;

namespace MyShop.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // 1. Kiểm tra xem giá trị có trống hay không
        bool isEmpty = value == null || (value is WorkSchedule ws && ws.User == null);

        // Nếu là một danh sách, kiểm tra xem có phần tử nào không
        if (value is ICollection collection)
        {
            isEmpty = collection.Count == 0;
        }

        // 2. Kiểm tra tham số truyền vào từ XAML
        // Nếu truyền ConverterParameter='Inverse' thì logic sẽ bị đảo ngược
        bool inverse = parameter as string == "Inverse";

        // 3. Quyết định trạng thái hiển thị
        bool isVisible;
        if (inverse)
        {
            // Nếu Inverse: Trống thì HIỆN, Có thì ẨN (Dành cho dấu +)
            isVisible = isEmpty;
        }
        else
        {
            // Nếu Bình thường: Có thì HIỆN, Trống thì ẨN (Dành cho nút nhân viên)
            isVisible = !isEmpty;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}