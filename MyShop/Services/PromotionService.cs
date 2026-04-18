using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface cho Promotion Service - chứa business logic liên quan đến khuyến mãi
/// </summary>
public interface IPromotionService
{
    /// <summary>
    /// Lấy tất cả khuyến mãi
    /// </summary>
    Task<List<Promotion>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Lấy khuyến mãi theo ID
    /// </summary>
    Task<Promotion?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Tìm khuyến mãi theo mã code
    /// </summary>
    Task<Promotion?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Thêm khuyến mãi mới
    /// </summary>
    Task<Promotion> AddAsync(Promotion promotion, CancellationToken ct = default);

    /// <summary>
    /// Cập nhật khuyến mãi
    /// </summary>
    Task UpdateAsync(Promotion promotion, CancellationToken ct = default);

    /// <summary>
    /// Xóa khuyến mãi
    /// </summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Áp dụng mã khuyến mãi cho đơn hàng
    /// </summary>
    /// <param name="code">Mã khuyến mãi</param>
    /// <param name="orderTotal">Tổng tiền đơn hàng</param>
    /// <param name="customerTier">Hạng khách hàng (VIP/Loyalty/Regular)</param>
    /// <returns>Kết quả áp dụng (số tiền giảm, lỗi nếu có)</returns>
    Task<ApplyPromoResult> ApplyPromotionAsync(string code, decimal orderTotal, string? customerTier = null, CancellationToken ct = default);
}

/// <summary>
/// Kết quả khi áp dụng mã khuyến mãi
/// </summary>
public class ApplyPromoResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalTotal { get; set; }
    public decimal PointsMultiplier { get; set; } = 1.0m;
}

/// <summary>
/// Promotion Service Implementation - xử lý logic nghiệp vụ khuyến mãi
/// </summary>
public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promoRepository;

    public PromotionService(IPromotionRepository promoRepository)
    {
        _promoRepository = promoRepository ?? throw new ArgumentNullException(nameof(promoRepository));
    }

    /// <summary>
    /// Lấy tất cả khuyến mãi
    /// </summary>
    public async Task<List<Promotion>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            return (await _promoRepository.GetAllFreshAsync(null, ct)).Items;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách khuyến mãi", ex);
        }
    }

    /// <summary>
    /// Lấy khuyến mãi theo ID
    /// </summary>
    public async Task<Promotion?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            return await _promoRepository.GetByIdFreshAsync(id, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy khuyến mãi với ID {id}", ex);
        }
    }

    /// <summary>
    /// Tìm khuyến mãi theo mã code
    /// </summary>
    public async Task<Promotion?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        try
        {
            var allPromos = (await _promoRepository.GetAllFreshAsync(null, ct)).Items;
            return allPromos.FirstOrDefault(p =>
                p.PromoCode.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tìm khuyến mãi theo mã '{code}'", ex);
        }
    }

    /// <summary>
    /// Thêm khuyến mãi mới
    /// </summary>
    public async Task<Promotion> AddAsync(Promotion promotion, CancellationToken ct = default)
    {
        if (promotion == null)
            throw new ArgumentNullException(nameof(promotion));

        // Kiểm tra mã code không được trống
        if (string.IsNullOrWhiteSpace(promotion.PromoCode))
            throw new ArgumentException("Mã khuyến mãi không được để trống", nameof(promotion.PromoCode));

        // Kiểm tra mã không trùng lặp
        var existingPromo = await GetByCodeAsync(promotion.PromoCode, ct);
        if (existingPromo != null)
            throw new InvalidOperationException($"Mã khuyến mãi '{promotion.PromoCode}' đã tồn tại");

        // Kiểm tra ngày hợp lệ
        if (promotion.EndDate < promotion.StartDate)
            throw new InvalidOperationException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu");

        // Kiểm tra giá trị giảm
        if (promotion.DiscountValue <= 0)
            throw new InvalidOperationException("Giá trị giảm phải lớn hơn 0");

        // Nếu là phần trăm, giới hạn <= 100
        if (promotion.PromoType == "Percent" && promotion.DiscountValue > 100)
            throw new InvalidOperationException("Giảm theo phần trăm không được vượt quá 100%");

        // Kiểm tra ký tự đặc biệt trong mã code
        if (!IsValidPromoCode(promotion.PromoCode))
            throw new InvalidOperationException("Mã khuyến mãi chỉ được chứa chữ cái, số và dấu gạch dưới");

        try
        {
            return await _promoRepository.AddAsync(promotion, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi thêm khuyến mãi: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cập nhật khuyến mãi
    /// </summary>
    public async Task UpdateAsync(Promotion promotion, CancellationToken ct = default)
    {
        if (promotion == null)
            throw new ArgumentNullException(nameof(promotion));

        if (promotion.Id <= 0)
            throw new ArgumentException("ID khuyến mãi không hợp lệ", nameof(promotion.Id));

        if (string.IsNullOrWhiteSpace(promotion.PromoCode))
            throw new ArgumentException("Mã khuyến mãi không được để trống", nameof(promotion.PromoCode));

        if (promotion.EndDate < promotion.StartDate)
            throw new InvalidOperationException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu");

        if (promotion.DiscountValue <= 0)
            throw new InvalidOperationException("Giá trị giảm phải lớn hơn 0");

        if (promotion.PromoType == "Percent" && promotion.DiscountValue > 100)
            throw new InvalidOperationException("Giảm theo phần trăm không được vượt quá 100%");

        if (!IsValidPromoCode(promotion.PromoCode))
            throw new InvalidOperationException("Mã khuyến mãi chỉ được chứa chữ cái, số và dấu gạch dưới");

        try
        {
            await _promoRepository.UpdateAsync(promotion, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi cập nhật khuyến mãi ID {promotion.Id}", ex);
        }
    }

    /// <summary>
    /// Xóa khuyến mãi
    /// </summary>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            await _promoRepository.DeleteAsync(id, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xóa khuyến mãi ID {id}", ex);
        }
    }

    /// <summary>
    /// Áp dụng mã khuyến mãi cho đơn hàng
    /// Theo yêu cầu nghiệp vụ:
    /// - Không được áp dụng khuyến mãi hết hạn
    /// - Không được áp dụng khuyến mãi hai lần cho cùng đơn hàng
    /// - Points multiplier thay đổi theo hạng khách hàng
    /// </summary>
    public async Task<ApplyPromoResult> ApplyPromotionAsync(string code, decimal orderTotal, string? customerTier = null, CancellationToken ct = default)
    {
        var result = new ApplyPromoResult { Success = false };

        if (string.IsNullOrWhiteSpace(code))
        {
            result.ErrorMessage = "Mã khuyến mãi không được để trống";
            return result;
        }

        if (orderTotal <= 0)
        {
            result.ErrorMessage = "Tổng tiền đơn hàng không hợp lệ";
            return result;
        }

        try
        {
            var promo = await GetByCodeAsync(code.Trim(), ct);
            if (promo == null)
            {
                result.ErrorMessage = "Mã khuyến mãi không tồn tại";
                return result;
            }

            // Kiểm tra trạng thái hoạt động
            if (!promo.IsActive)
            {
                result.ErrorMessage = "Mã khuyến mãi đã bị vô hiệu hóa";
                return result;
            }

            // Kiểm tra TargetAudience
            if (promo.TargetAudience != "All" &&
                !string.Equals(promo.TargetAudience, customerTier, StringComparison.OrdinalIgnoreCase))
            {
                result.ErrorMessage = $"Mã khuyến mãi này chỉ dành cho {promo.TargetAudience}";
                return result;
            }

            // Kiểm tra ngày hết hạn - nếu ngoài khoảng StartDate và EndDate thì không được áp dụng
            var now = DateTime.Now;
            if (now < promo.StartDate)
            {
                result.ErrorMessage = $"Mã khuyến mãi chưa có hiệu lực. Bắt đầu từ {promo.StartDate:dd/MM/yyyy}";
                return result;
            }

            if (now > promo.EndDate)
            {
                result.ErrorMessage = "Mã khuyến mãi đã hết hạn";
                return result;
            }

            // Kiểm tra đơn tối thiểu
            if (orderTotal < promo.MinOrderAmount)
            {
                result.ErrorMessage = $"Đơn hàng tối thiểu {promo.MinOrderAmount:N0}đ để áp dụng mã này";
                return result;
            }

            // Tính số tiền giảm
            decimal discount = 0;
            if (promo.PromoType == "Percent")
            {
                discount = orderTotal * promo.DiscountValue / 100;
            }
            else // FixedAmount
            {
                discount = promo.DiscountValue;
            }

            // Không vượt quá tổng tiền
            if (discount > orderTotal)
                discount = orderTotal;

            var finalTotal = orderTotal - discount;

            // Tính points multiplier theo customer tier
            decimal pointsMultiplier = 1.0m;
            if (!string.IsNullOrWhiteSpace(customerTier))
            {
                pointsMultiplier = customerTier.ToLowerInvariant() switch
                {
                    "vip" => 3.0m,
                    "loyalty" => 2.0m,
                    "newcustomer" => 1.5m,
                    _ => 1.0m
                };
            }

            result.Success = true;
            result.DiscountAmount = discount;
            result.FinalTotal = finalTotal;
            result.PointsMultiplier = pointsMultiplier;
            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Lỗi khi áp dụng khuyến mãi: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Kiểm tra mã code hợp lệ (chỉ chứa chữ cái, số và dấu gạch dưới, không có ký tự đặc biệt)
    /// </summary>
    private static bool IsValidPromoCode(string code)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Za-z0-9_]+$");
    }
}
