using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

/// <summary>
/// Lưu cấu hình Loyalty Rules vào DB (key-value pairs).
/// Key: "PointsPerVnd" | "RedemptionRate" | "Tier_Bronze" | ...
/// ConfigValue: số nguyên hoặc JSON.
/// </summary>
public class LoyaltyTierConfig
{
    public int Id { get; set; }
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Keys cho ConfigKey trong LoyaltyTierConfigs.
/// </summary>
public static class LoyaltyConfigKeys
{
    public const string PointsPerVnd = "PointsPerVnd";
    public const string RedemptionRate = "RedemptionRate";
}

/// <summary>
/// Tier threshold model — observable row dùng trong ViewModel và XAML binding.
/// Properties TierName/MinPoints/Multiplier dùng [ObservableProperty] để UI tự cập nhật.
/// TierKey là plain get;init; (không cần notify).
/// </summary>
public partial class TierThresholdConfig : ObservableObject
{
    public string TierKey { get; init; } = "";

    [ObservableProperty]
    private string _tierName = "";

    [ObservableProperty]
    private int _minPoints;

    [ObservableProperty]
    private decimal _multiplier = 1.0m;

    public int DisplayOrder { get; set; }

    public TierThresholdConfig() { }

    public TierThresholdConfig(string tierKey, string tierName, int minPoints, decimal multiplier)
    {
        TierKey = tierKey;
        TierName = tierName;
        MinPoints = minPoints;
        Multiplier = multiplier;
    }
}
