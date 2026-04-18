using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class LoyaltyRulesViewModel : ObservableObject
{
    private readonly ILoyaltyService _loyaltyService;
    private readonly Action _onBack;

    [ObservableProperty]
    private int _pointsPerVnd = 1000;

    [ObservableProperty]
    private int _redemptionRate = 100;

    public ObservableCollection<TierThresholdConfig> TierThresholds { get; } = new();

    [ObservableProperty]
    private string _previewOrderAmount = "100000";

    [ObservableProperty]
    private string _previewResult = "";

    [ObservableProperty]
    private string _testCustomerName = "Demo Customer";

    [ObservableProperty]
    private int _testTotalAccumulatedPoints = 4500;

    [ObservableProperty]
    private int _testCurrentAvailablePoints = 2000;

    [ObservableProperty]
    private LoyaltyTier _testTier = LoyaltyTier.Silver;

    [ObservableProperty]
    private string _testResult = "";

    [ObservableProperty]
    private int _testRedeemAmount = 100;

    [ObservableProperty]
    private string _testRedeemResult = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveRulesCommand))]
    private bool _isSaving;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    public LoyaltyRulesViewModel(ILoyaltyService loyaltyService, Action onBack)
    {
        _loyaltyService = loyaltyService ?? throw new ArgumentNullException(nameof(loyaltyService));
        _onBack = onBack;

        TierThresholds.Add(new TierThresholdConfig("Bronze", "Bronze", 0, 1.0m));
        TierThresholds.Add(new TierThresholdConfig("Silver", "Silver", 5000, 1.5m));
        TierThresholds.Add(new TierThresholdConfig("Gold", "Gold", 15000, 2.0m));
        TierThresholds.Add(new TierThresholdConfig("Platinum", "Platinum", 30000, 2.5m));

        _ = LoadConfigAsync();
    }

    [RelayCommand]
    private void Back()
    {
        _onBack?.Invoke();
    }

    private async Task LoadConfigAsync()
    {
        try
        {
            IsLoading = true;

            var allConfigs = await _loyaltyService.GetAllConfigsAsync();
            var configMap = allConfigs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue);

            if (configMap.TryGetValue(LoyaltyConfigKeys.PointsPerVnd, out var ppv)
                && int.TryParse(ppv, out var ppvVal))
                PointsPerVnd = ppvVal;

            if (configMap.TryGetValue(LoyaltyConfigKeys.RedemptionRate, out var rr)
                && int.TryParse(rr, out var rrVal))
                RedemptionRate = rrVal;

            foreach (var tier in TierThresholds)
            {
                var key = $"Tier_{tier.TierKey}";
                if (configMap.TryGetValue(key, out var json) && !string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("TierName", out var tn))
                            tier.TierName = tn.GetString() ?? tier.TierKey;
                        if (doc.RootElement.TryGetProperty("MinPoints", out var mp))
                            tier.MinPoints = mp.GetInt32();
                        if (doc.RootElement.TryGetProperty("Multiplier", out var mult))
                            tier.Multiplier = (decimal)mult.GetDouble();
                    }
                    catch { /* keep defaults */ }
                }
            }

            CalculatePreview();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSaveRules() => !IsSaving;

    [RelayCommand(CanExecute = nameof(CanSaveRules))]
    private async Task SaveRulesAsync()
    {
        try
        {
            IsSaving = true;

            if (PointsPerVnd <= 0)
            {
                StatusMessage = "Points rate must be > 0.";
                return;
            }
            if (RedemptionRate <= 0)
            {
                StatusMessage = "Redemption rate must be > 0.";
                return;
            }

            await UpsertConfigAsync(LoyaltyConfigKeys.PointsPerVnd, PointsPerVnd.ToString(),
                $"Earn: {PointsPerVnd} VND = 1 point", 1);

            await UpsertConfigAsync(LoyaltyConfigKeys.RedemptionRate, RedemptionRate.ToString(),
                $"Redeem: 1 point = {RedemptionRate} VND", 2);

            int order = 10;
            foreach (var tier in TierThresholds)
            {
                if (string.IsNullOrWhiteSpace(tier.TierName))
                {
                    StatusMessage = "Tier name cannot be empty.";
                    return;
                }
                tier.DisplayOrder = order++;
                await _loyaltyService.UpsertTierThresholdAsync(tier);
            }

            StatusMessage = "All rules saved successfully!";
            _onBack?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task UpsertConfigAsync(string key, string value, string description, int displayOrder)
    {
        await _loyaltyService.UpsertConfigAsync(new LoyaltyTierConfig
        {
            ConfigKey = key,
            ConfigValue = value,
            Description = description,
            DisplayOrder = displayOrder
        });
    }

    [RelayCommand]
    private void CalculatePreview()
    {
        if (!decimal.TryParse(PreviewOrderAmount, out var amount) || amount <= 0)
        {
            PreviewResult = "Enter a valid amount to preview.";
            return;
        }
        if (PointsPerVnd <= 0)
        {
            PreviewResult = "Points rate must be > 0.";
            return;
        }

        var lines = TierThresholds.Select(t =>
            $"{t.TierName}: {amount:N0} -> " +
            $"{Math.Floor(amount / PointsPerVnd * t.Multiplier):N0} pts (x{t.Multiplier})");

        PreviewResult = string.Join("\n", lines);
    }

    [RelayCommand]
    private async Task CalculatePointsAsync()
    {
        if (!decimal.TryParse(PreviewOrderAmount, out var amount) || amount <= 0)
        {
            TestResult = "Enter a valid order amount.";
            return;
        }

        var mockCustomer = new Customer
        {
            Id = 999,
            Name = TestCustomerName,
            TotalAccumulatedPoints = TestTotalAccumulatedPoints,
            CurrentAvailablePoints = TestCurrentAvailablePoints,
            Tier = TestTier
        };

        try
        {
            int earned = await _loyaltyService.CalculatePointsAsync(mockCustomer, amount);

            TestTotalAccumulatedPoints = mockCustomer.TotalAccumulatedPoints;
            TestCurrentAvailablePoints = mockCustomer.CurrentAvailablePoints;
            TestTier = mockCustomer.Tier;

            decimal discountValue = earned * RedemptionRate;
            TestResult =
                $"Order: {amount:N0}\n" +
                $"Earned: {earned} pts\n" +
                $"Total Accumulated: {TestTotalAccumulatedPoints:N0}\n" +
                $"Available: {TestCurrentAvailablePoints:N0}\n" +
                $"New Tier: {TestTier}\n" +
                $"Redeem Value: {discountValue:N0} ({earned} pts x {RedemptionRate})";
        }
        catch (Exception ex)
        {
            TestResult = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RedeemPointsAsync()
    {
        if (TestRedeemAmount <= 0)
        {
            TestRedeemResult = "Enter a valid points amount.";
            return;
        }

        var mockCustomer = new Customer
        {
            Id = 999,
            Name = TestCustomerName,
            TotalAccumulatedPoints = TestTotalAccumulatedPoints,
            CurrentAvailablePoints = TestCurrentAvailablePoints,
            Tier = TestTier
        };

        try
        {
            decimal discount = await _loyaltyService.RedeemPointsAsync(mockCustomer, TestRedeemAmount);
            TestCurrentAvailablePoints = mockCustomer.CurrentAvailablePoints;
            TestRedeemResult =
                $"Redeemed: {TestRedeemAmount} pts\n" +
                $"Discount: {discount:N0}\n" +
                $"Remaining Available: {TestCurrentAvailablePoints:N0}";
        }
        catch (Exception ex)
        {
            TestRedeemResult = $"Error: {ex.Message}";
        }
    }
}
