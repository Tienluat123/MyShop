using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface cho Loyalty Service — CRUD cấu hình loyalty và tính điểm
/// </summary>
public interface ILoyaltyService
{
    // ===== Config CRUD =====
    Task<LoyaltyTierConfig?> GetConfigAsync(string configKey);
    Task<List<LoyaltyTierConfig>> GetAllConfigsAsync();
    Task UpsertConfigAsync(LoyaltyTierConfig config);

    // ===== Tier thresholds =====
    Task<List<TierThresholdConfig>> GetTierThresholdsAsync();
    Task UpsertTierThresholdAsync(TierThresholdConfig tier);

    // ===== Points calculation =====
    /// <summary>
    /// Tính và cập nhật điểm cho khách hàng sau order hoàn thành.
    /// Trả về số điểm đã cộng thêm.
    /// Tự động xét nâng hạng nếu TotalAccumulatedPoints vượt MinPoints.
    /// </summary>
    Task<int> CalculatePointsAsync(Customer customer, decimal orderSubTotal);

    /// <summary>
    /// Thực hiện đổi điểm — khấu trừ CurrentAvailablePoints.
    /// Trả về số tiền được giảm (= points * RedemptionRate).
    /// </summary>
    Task<decimal> RedeemPointsAsync(Customer customer, int pointsToRedeem);

    /// <summary>
    /// Lấy RedemptionRate từ DB (1 điểm = ?).
    /// </summary>
    Task<int> GetRedemptionRateAsync();
}
