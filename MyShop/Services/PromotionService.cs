using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MyShop.Services;

/// <summary>
/// Interface cho Promotion Service - chứa business logic liên quan đến khuyến mãi
/// </summary>
public interface IPromotionService
{
    /// <summary>
    /// Lấy tất cả khuyến mãi với phân trang
    /// </summary>
    Task<PagedResult<Promotion>> GetAllAsync(PagingRequest? paging = null, CancellationToken ct = default);

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

    Task<ApplyPromoResult> ApplyPromotionAsync(string code, decimal orderTotal, Customer? customer, CancellationToken ct = default);

    bool IsPromotionApplicable(Promotion promo, decimal orderTotal, Customer? customer, out string? errorMessage);
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
    /// Lấy tất cả khuyến mãi với phân trang
    /// </summary>
    public async Task<PagedResult<Promotion>> GetAllAsync(PagingRequest? paging = null, CancellationToken ct = default)
    {
        try
        {
            Expression<Func<Promotion, bool>>? filter = null;

            if (paging != null)
            {
                var filters = new List<Expression<Func<Promotion, bool>>>();

                if (!string.IsNullOrWhiteSpace(paging.SearchText))
                {
                    var keyword = paging.SearchText.Trim();
                    filters.Add(p => 
                        EF.Functions.ToTsVector("simple", p.PromoCode).Matches(EF.Functions.PlainToTsQuery("simple", keyword)) ||
                        EF.Functions.ILike(p.PromoCode, $"%{keyword}%") ||
                        (p.TargetAudience != null && EF.Functions.ILike(p.TargetAudience, $"%{keyword}%"))
                    );
                }

                // Status Filter from Dictionary
                if (paging.Filters != null && paging.Filters.TryGetValue("Status", out var status) && status != "All")
                {
                    var now = DateTime.Now;
                    switch (status)
                    {
                        case "Active":
                            filters.Add(p => p.IsActive && now >= p.StartDate && now <= p.EndDate);
                            break;
                        case "Expired":
                            filters.Add(p => now > p.EndDate);
                            break;
                        case "Upcoming":
                            filters.Add(p => p.IsActive && now < p.StartDate);
                            break;
                        case "Inactive":
                            filters.Add(p => !p.IsActive);
                            break;
                    }
                }

                // Combine filters
                if (filters.Any())
                {
                    var parameter = Expression.Parameter(typeof(Promotion), "p");
                    Expression? combined = null;

                    foreach (var f in filters)
                    {
                        var body = new ParameterReplacer(f.Parameters[0], parameter).Visit(f.Body);
                        combined = combined == null ? body : Expression.AndAlso(combined, body!);
                    }

                    if (combined != null)
                    {
                        filter = Expression.Lambda<Func<Promotion, bool>>(combined, parameter);
                    }
                }
            }

            return await _promoRepository.GetAllFreshAsync(paging, filter, ct == default ? CancellationToken.None : ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load promotions", ex);
        }
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }

    /// <summary>
    /// Lấy khuyến mãi theo ID
    /// </summary>
    public async Task<Promotion?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));

        try
        {
            return await _promoRepository.GetByIdFreshAsync(id, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load promotion with ID {id}", ex);
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
            var pagedResult = await _promoRepository.GetAllFreshAsync(null, null, ct);
            var allPromos = pagedResult.Items;
            return allPromos.FirstOrDefault(p =>
                p.PromoCode.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to find promotion by code '{code}'", ex);
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
            throw new ArgumentException("Promotion code is required", nameof(promotion.PromoCode));

        // Kiểm tra mã không trùng lặp
        var existingPromo = await GetByCodeAsync(promotion.PromoCode, ct);
        if (existingPromo != null)
            throw new InvalidOperationException($"Promotion code '{promotion.PromoCode}' already exists");

        // Kiểm tra ngày hợp lệ
        if (promotion.EndDate < promotion.StartDate)
            throw new InvalidOperationException("End date must be greater than or equal to start date");

        // Kiểm tra giá trị giảm
        if (promotion.DiscountValue <= 0)
            throw new InvalidOperationException("Discount value must be greater than 0");

        // Nếu là phần trăm, giới hạn <= 100
        if (promotion.PromoType == "Percent" && promotion.DiscountValue > 100)
            throw new InvalidOperationException("Percentage discount cannot exceed 100%");

        // Kiểm tra ký tự đặc biệt trong mã code
        if (!IsValidPromoCode(promotion.PromoCode))
            throw new InvalidOperationException("Promotion code can only contain letters, numbers, and underscores");

        try
        {
            return await _promoRepository.AddAsync(promotion, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to add promotion: {ex.Message}", ex);
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
            throw new ArgumentException("Invalid promotion ID", nameof(promotion.Id));

        if (string.IsNullOrWhiteSpace(promotion.PromoCode))
            throw new ArgumentException("Promotion code is required", nameof(promotion.PromoCode));

        if (promotion.EndDate < promotion.StartDate)
            throw new InvalidOperationException("End date must be greater than or equal to start date");

        if (promotion.DiscountValue <= 0)
            throw new InvalidOperationException("Discount value must be greater than 0");

        if (promotion.PromoType == "Percent" && promotion.DiscountValue > 100)
            throw new InvalidOperationException("Percentage discount cannot exceed 100%");

        if (!IsValidPromoCode(promotion.PromoCode))
            throw new InvalidOperationException("Promotion code can only contain letters, numbers, and underscores");

        try
        {
            await _promoRepository.UpdateAsync(promotion, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update promotion ID {promotion.Id}", ex);
        }
    }

    /// <summary>
    /// Xóa khuyến mãi
    /// </summary>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));

        try
        {
            await _promoRepository.DeleteAsync(id, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete promotion ID {id}", ex);
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
            result.ErrorMessage = "Promotion code is required";
            return result;
        }

        if (orderTotal <= 0)
        {
            result.ErrorMessage = "Invalid order total";
            return result;
        }

        try
        {
            var promo = await GetByCodeAsync(code.Trim(), ct);
            if (promo == null)
            {
                result.ErrorMessage = "Promotion code does not exist";
                return result;
            }

            // Kiểm tra trạng thái hoạt động
            if (!promo.IsActive)
            {
                result.ErrorMessage = "Promotion code has been disabled";
                return result;
            }

            // Kiểm tra TargetAudience
            if (promo.TargetAudience != "All" &&
                !string.Equals(promo.TargetAudience, customerTier, StringComparison.OrdinalIgnoreCase))
            {
                result.ErrorMessage = $"This promotion code is only for {promo.TargetAudience}";
                return result;
            }

            // Kiểm tra ngày hết hạn - nếu ngoài khoảng StartDate và EndDate thì không được áp dụng
            var now = DateTime.Now;
            if (now < promo.StartDate)
            {
                result.ErrorMessage = $"Promotion code is not active yet. Starts on {promo.StartDate:dd/MM/yyyy}";
                return result;
            }

            if (now > promo.EndDate)
            {
                result.ErrorMessage = "Promotion code has expired";
                return result;
            }

            // Kiểm tra đơn tối thiểu
            if (orderTotal < promo.MinOrderAmount)
            {
                result.ErrorMessage = $"Minimum order amount is {promo.MinOrderAmount:N0} VND to apply this code";
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
            result.ErrorMessage = $"Failed to apply promotion: {ex.Message}";
            return result;
        }
    }

    public async Task<ApplyPromoResult> ApplyPromotionAsync(string code, decimal orderTotal, Customer? customer, CancellationToken ct = default)
    {
        var result = new ApplyPromoResult { Success = false };

        if (string.IsNullOrWhiteSpace(code))
        {
            result.ErrorMessage = "Promotion code is required";
            return result;
        }

        if (orderTotal <= 0)
        {
            result.ErrorMessage = "Invalid order total";
            return result;
        }

        try
        {
            var promo = await GetByCodeAsync(code.Trim(), ct);
            if (promo == null)
            {
                result.ErrorMessage = "Promotion code does not exist";
                return result;
            }

            if (!IsPromotionApplicable(promo, orderTotal, customer, out var errorMessage))
            {
                result.ErrorMessage = errorMessage;
                return result;
            }

            var discount = CalculateDiscount(promo, orderTotal);
            result.Success = true;
            result.DiscountAmount = discount;
            result.FinalTotal = orderTotal - discount;
            result.PointsMultiplier = GetPointsMultiplier(customer);
            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Failed to apply promotion: {ex.Message}";
            return result;
        }
    }

    public bool IsPromotionApplicable(Promotion promo, decimal orderTotal, Customer? customer, out string? errorMessage)
    {
        errorMessage = null;

        if (!promo.IsActive)
        {
            errorMessage = "Promotion code has been disabled";
            return false;
        }

        var now = DateTime.Now;
        if (now < promo.StartDate)
        {
            errorMessage = $"Promotion code is not active yet. Starts on {promo.StartDate:dd/MM/yyyy}";
            return false;
        }

        if (now > promo.EndDate)
        {
            errorMessage = "Promotion code has expired";
            return false;
        }

        if (orderTotal < promo.MinOrderAmount)
        {
            errorMessage = $"Minimum order amount is {promo.MinOrderAmount:N0} VND to apply this code";
            return false;
        }

        if (!IsTargetAudienceMatch(promo.TargetAudience, customer))
        {
            errorMessage = promo.TargetAudience?.Equals("All", StringComparison.OrdinalIgnoreCase) == true
                ? null
                : $"This promotion code is only for {promo.TargetAudience}";
            return false;
        }

        return true;
    }

    private static decimal CalculateDiscount(Promotion promo, decimal orderTotal)
    {
        var discount = promo.PromoType == "Percent"
            ? orderTotal * promo.DiscountValue / 100
            : promo.DiscountValue;

        return Math.Min(discount, orderTotal);
    }

    private static bool IsTargetAudienceMatch(string? targetAudience, Customer? customer)
    {
        if (string.IsNullOrWhiteSpace(targetAudience) ||
            targetAudience.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (customer == null)
        {
            return false;
        }

        return targetAudience.Trim().ToLowerInvariant() switch
        {
            "vip" => customer.Tier is LoyaltyTier.Gold or LoyaltyTier.Platinum,
            "newcustomer" => customer.TotalVisits == 0,
            "loyalty" => true,
            _ => targetAudience.Equals(customer.Tier.ToString(), StringComparison.OrdinalIgnoreCase)
        };
    }

    private static decimal GetPointsMultiplier(Customer? customer)
    {
        if (customer == null) return 1.0m;

        return customer.Tier switch
        {
            LoyaltyTier.Silver => 1.5m,
            LoyaltyTier.Gold => 2.0m,
            LoyaltyTier.Platinum => 2.5m,
            _ => 1.0m
        };
    }

    /// <summary>
    /// Kiểm tra mã code hợp lệ (chỉ chứa chữ cái, số và dấu gạch dưới, không có ký tự đặc biệt)
    /// </summary>
    private static bool IsValidPromoCode(string code)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Za-z0-9_]+$");
    }
}
