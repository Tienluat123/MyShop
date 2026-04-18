using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Converters;

/// <summary>
/// Converts loyalty tier string to corresponding brush color
/// </summary>
public class LoyaltyTierBrushConverter : Microsoft.UI.Xaml.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string tier)
        {
            return tier.ToLower() switch
            {
                "bronze" => new SolidColorBrush(ColorHelper.FromUint(0xFFB87333)),      // Bronze
                "silver" => new SolidColorBrush(ColorHelper.FromUint(0xFFC0C0C0)),      // Silver  
                "gold" => new SolidColorBrush(ColorHelper.FromUint(0xFFFFD700)),        // Gold
                "platinum" => new SolidColorBrush(ColorHelper.FromUint(0xFFE5E4E2)),    // Platinum
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
