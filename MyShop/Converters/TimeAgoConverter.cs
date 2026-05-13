using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class TimeAgoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not DateTime createdAt) return "";

        var utcCreatedAt = createdAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)
            : createdAt.ToUniversalTime();

        var diff = DateTime.UtcNow - utcCreatedAt;
        if (diff.TotalSeconds < 0) return "just now";
        if (diff.TotalSeconds < 60) return $"{(int)diff.TotalSeconds}s";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h";
        return $"{(int)diff.TotalDays}d";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
