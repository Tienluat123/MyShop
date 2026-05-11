using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Loyalty Service — thực hiện CRUD và business logic loyalty
/// </summary>
public class LoyaltyService : ILoyaltyService
{
    private readonly AppDbContextFactory _contextFactory;
    private readonly IRepository<Customer> _customerRepo;

    public LoyaltyService(AppDbContextFactory contextFactory, IRepository<Customer> customerRepo)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _customerRepo = customerRepo ?? throw new ArgumentNullException(nameof(customerRepo));
    }

    private AppDbContext CreateContext() => _contextFactory.CreateDbContext(Array.Empty<string>());

    // =====================================================================
    // CONFIG CRUD
    // =====================================================================

    public async Task<LoyaltyTierConfig?> GetConfigAsync(string configKey)
    {
        using var context = CreateContext();
        return await context.LoyaltyTierConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConfigKey == configKey);
    }

    public async Task<List<LoyaltyTierConfig>> GetAllConfigsAsync()
    {
        using var context = CreateContext();
        return await context.LoyaltyTierConfigs.AsNoTracking().ToListAsync();
    }

    public async Task UpsertConfigAsync(LoyaltyTierConfig config)
    {
        using var context = CreateContext();
        var existing = await context.LoyaltyTierConfigs
            .FirstOrDefaultAsync(c => c.ConfigKey == config.ConfigKey);

        if (existing != null)
        {
            existing.ConfigValue = config.ConfigValue;
            existing.Description = config.Description;
            existing.DisplayOrder = config.DisplayOrder;
        }
        else
        {
            context.LoyaltyTierConfigs.Add(config);
        }
        await context.SaveChangesAsync();
    }

    // =====================================================================
    // TIER THRESHOLDS
    // =====================================================================

    public async Task<List<TierThresholdConfig>> GetTierThresholdsAsync()
    {
        using var context = CreateContext();
        var configs = await context.LoyaltyTierConfigs
            .AsNoTracking()
            .Where(c => c.ConfigKey.StartsWith("Tier_"))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var result = new List<TierThresholdConfig>();
        foreach (var cfg in configs)
        {
            try
            {
                var doc = JsonDocument.Parse(cfg.ConfigValue);
                var tier = new TierThresholdConfig
                {
                    TierKey = cfg.ConfigKey.Replace("Tier_", ""),
                    TierName = cfg.Description?.Split(':')[0] ?? cfg.ConfigKey.Replace("Tier_", ""),
                    DisplayOrder = cfg.DisplayOrder
                };
                if (doc.RootElement.TryGetProperty("MinPoints", out var mp))
                    tier.MinPoints = mp.GetInt32();
                if (doc.RootElement.TryGetProperty("Multiplier", out var mult))
                    tier.Multiplier = (decimal)mult.GetDouble();
                result.Add(tier);
            }
            catch { /* skip malformed config */ }
        }
        return result;
    }

    public async Task UpsertTierThresholdAsync(TierThresholdConfig tier)
    {
        var key = $"Tier_{tier.TierKey}";
        var value = JsonSerializer.Serialize(new { tier.MinPoints, tier.Multiplier });
        var desc = $"{tier.TierName}: from {tier.MinPoints:N0} pts, x{tier.Multiplier}";

        var config = new LoyaltyTierConfig
        {
            ConfigKey = key,
            ConfigValue = value,
            Description = desc,
            DisplayOrder = tier.DisplayOrder > 0 ? tier.DisplayOrder : 10
        };
        await UpsertConfigAsync(config);
    }

    // =====================================================================
    // POINTS CALCULATION
    // =====================================================================

    /// <summary>
    /// Tính điểm tích lũy và cập nhật vào Customer.
    /// - Cộng điểm vào cả TotalAccumulatedPoints và CurrentAvailablePoints.
    /// - Tự động xét nâng hạng dựa trên TotalAccumulatedPoints vs TierThresholds.
    /// </summary>
    public async Task<int> CalculatePointsAsync(Customer customer, decimal orderSubTotal)
    {
        // Load PointsPerVnd from DB (default 1000)
        var ppvConfig = await GetConfigAsync(LoyaltyConfigKeys.PointsPerVnd);
        int pointsPerVnd = ppvConfig != null && int.TryParse(ppvConfig.ConfigValue, out var v) ? v : 1000;

        // Load tiers from DB for auto-upgrade check
        var tiers = await GetTierThresholdsAsync();

        // Get multiplier for current tier
        decimal multiplier = GetMultiplierForTier(tiers, customer.Tier);

        // Calculate earned points
        int earnedPoints = (int)Math.Floor(orderSubTotal / pointsPerVnd * multiplier);

        // Update customer — add to both accumulators
        customer.TotalAccumulatedPoints += earnedPoints;
        customer.CurrentAvailablePoints += earnedPoints;

        // Auto-upgrade tier based on new TotalAccumulatedPoints
        var newTier = DetermineTier(tiers, customer.TotalAccumulatedPoints);
        if (newTier != customer.Tier)
        {
            customer.Tier = newTier;
        }

        // Persist to DB
        await _customerRepo.UpdateAsync(customer);
        await _customerRepo.SaveChangesAsync();

        System.Diagnostics.Debug.WriteLine(
            $"[Loyalty] Earned {earnedPoints} pts. Total={customer.TotalAccumulatedPoints}, Available={customer.CurrentAvailablePoints}, Tier={customer.Tier}");

        return earnedPoints;
    }

    // =====================================================================
    // REDEMPTION
    // =====================================================================

    /// <summary>
    /// Khấu trừ điểm từ CurrentAvailablePoints.
    /// Validate: điểm > 0 và điểm <= số dư hiện có.
    /// Trả về số tiền được giảm = points * redemptionRate.
    /// </summary>
    public async Task<decimal> RedeemPointsAsync(Customer customer, int pointsToRedeem)
    {
        // Load redemption rate from DB (default 100 = 1pt = 100)
        int redemptionRate = await GetRedemptionRateAsync();

        if (pointsToRedeem <= 0)
            throw new InvalidOperationException("Points to redeem must be greater than 0.");

        if (pointsToRedeem > customer.CurrentAvailablePoints)
            throw new InvalidOperationException(
                $"Insufficient points. Current available: {customer.CurrentAvailablePoints}, requested: {pointsToRedeem}.");

        // Deduct points
        customer.CurrentAvailablePoints -= pointsToRedeem;
        await _customerRepo.UpdateAsync(customer);
        await _customerRepo.SaveChangesAsync();

        decimal discount = pointsToRedeem * redemptionRate;
        System.Diagnostics.Debug.WriteLine(
            $"[Loyalty] Redeemed {pointsToRedeem} pts = {discount:N0} discount. Remaining: {customer.CurrentAvailablePoints}");

        return discount;
    }

    public async Task<int> GetRedemptionRateAsync()
    {
        var config = await GetConfigAsync(LoyaltyConfigKeys.RedemptionRate);
        return config != null && int.TryParse(config.ConfigValue, out var v) ? v : 100;
    }

    // =====================================================================
    // PRIVATE HELPERS
    // =====================================================================

    private decimal GetMultiplierForTier(List<TierThresholdConfig> tiers, LoyaltyTier tier)
    {
        var t = tiers.FirstOrDefault(x =>
            x.TierKey.Equals(tier.ToString(), StringComparison.OrdinalIgnoreCase));
        return t?.Multiplier ?? (tier switch
        {
            LoyaltyTier.Silver => 1.5m,
            LoyaltyTier.Gold => 2.0m,
            LoyaltyTier.Platinum => 2.5m,
            _ => 1.0m
        });
    }

    /// <summary>
    /// Determine highest tier customer qualifies for based on accumulated points.
    /// Uses thresholds from DB, sorted descending by MinPoints.
    /// </summary>
    private LoyaltyTier DetermineTier(List<TierThresholdConfig> tiers, int totalPoints)
    {
        // Sort tiers by MinPoints descending — first match is the highest eligible tier
        var sorted = tiers.OrderByDescending(t => t.MinPoints);
        foreach (var tier in sorted)
        {
            if (totalPoints >= tier.MinPoints)
            {
                return Enum.TryParse<LoyaltyTier>(tier.TierKey, ignoreCase: true, out var result)
                    ? result
                    : LoyaltyTier.Bronze;
            }
        }
        return LoyaltyTier.Bronze; // Default
    }
}