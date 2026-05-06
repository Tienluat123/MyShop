using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class TimeAgoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not DateTime createdAt) return "";

        var diff = DateTime.UtcNow - createdAt.ToUniversalTime();
        if (diff.TotalSeconds < 0) return "just now";
        if (diff.TotalSeconds < 60) return $"{(int)diff.TotalSeconds}s";
        return $"{(int)diff.TotalMinutes}m";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
