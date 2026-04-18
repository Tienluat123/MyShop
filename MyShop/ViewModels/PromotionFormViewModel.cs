using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// ViewModel cho form tạo / chỉnh sửa khuyến mãi
/// Theo cấu trúc ValidateForm + Upsert Logic trong Reference.md
/// </summary>
public partial class PromotionFormViewModel : ObservableObject
{
    private readonly IPromotionService _promoService;
    private readonly Action onCancel;
    private readonly Action<PromotionItem>? onSuccess; // callback khi save thành công

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string formTitle = string.Empty;

    [ObservableProperty]
    private string subtitle = string.Empty;

    [ObservableProperty]
    private string submitButtonText = "Create Promotion";

    [ObservableProperty]
    private string cancelButtonText = "Cancel";

    [ObservableProperty]
    private PromotionItem promotionData = new();

    /// <summary>
    /// Validation error messages (hiển thị bên dưới field tương ứng)
    /// Key = tên property, Value = thông báo lỗi
    /// </summary>
    [ObservableProperty]
    private Dictionary<string, string> validationErrors = new();

    [ObservableProperty]
    private bool isSaving = false;

    [ObservableProperty]
    private string? errorMessage;

    // ===== Helper properties cho XAML ComboBox binding =====
    // ComboBox.SelectedIndex bind với int, không bind trực tiếp với enum
    // Index: 0=Percent, 1=FixedAmount
    [ObservableProperty]
    private int selectedTypeIndex = 0;

    // Index: 0=All, 1=VIP, 2=NewCustomer, 3=Loyalty
    [ObservableProperty]
    private int selectedTargetIndex = 0;

    public PromotionFormViewModel(
        bool isEditMode,
        PromotionItem? promotion,
        IPromotionService promoService,
        Action onCancel,
        Action<PromotionItem>? onSuccess = null)
    {
        _promoService = promoService ?? throw new ArgumentNullException(nameof(promoService));
        this.onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));
        this.onSuccess = onSuccess;
        IsEditMode = isEditMode;

        if (isEditMode && promotion != null)
        {
            FormTitle = "Edit Promotion";
            Subtitle = "Modify the details of this promotion";
            SubmitButtonText = "Save Changes";
            CancelButtonText = "Discard";
            // Clone để tránh thay đổi trực tiếp vào list khi chưa save
            PromotionData = ClonePromotionItem(promotion);

            // Sync helper properties
            SelectedTypeIndex = PromotionData.Type == PromoType.FixedAmount ? 1 : 0;
            SelectedTargetIndex = (int)PromotionData.Target;
        }
        else
        {
            FormTitle = "New Promotion";
            Subtitle = "Setup a new discount campaign for your shop";
            SubmitButtonText = "Create Promotion";
            CancelButtonText = "Cancel";
            PromotionData = new PromotionItem
            {
                Type = PromoType.Percent,
                Target = PromoTarget.All,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                IsActive = true
            };

            // Sync helper properties
            SelectedTypeIndex = 0;
            SelectedTargetIndex = 0;
        }
    }

    /// <summary>
    /// Tạo bản sao của PromotionItem để edit mà không ảnh hưởng list gốc
    /// </summary>
    private static PromotionItem ClonePromotionItem(PromotionItem source)
    {
        return new PromotionItem
        {
            Id = source.Id,
            Code = source.Code,
            Type = source.Type,
            Value = source.Value,
            MinimumOrderAmount = source.MinimumOrderAmount,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Target = source.Target,
            IsActive = source.IsActive
        };
    }

    /// <summary>
    /// Nút Submit — kiểm tra validation trước khi lưu
    /// Theo pattern ValidateForm trong Reference.md
    /// </summary>
    [RelayCommand]
    private async Task SubmitAsync()
    {
        // Xóa lỗi cũ
        ValidationErrors.Clear();
        ErrorMessage = null;

        // 1. Validate form — nếu fail thì không submit
        if (!await ValidateFormAsync()) return;

        try
        {
            IsSaving = true;

            var promo = PromotionData;
            var entity = promo.ToEntity();

            if (promo.Id == 0) // Thêm mới
            {
                entity.Id = 0; // đảm bảo EF tạo mới
                var saved = await _promoService.AddAsync(entity);

                // Cập nhật lại ID cho item
                promo.Id = saved.Id;

                onSuccess?.Invoke(promo);
            }
            else // Cập nhật
            {
                await _promoService.UpdateAsync(entity);
                onSuccess?.Invoke(promo);
            }

            // Dialog sẽ tự đóng khi ErrorMessage == null && ValidationErrors rỗng
        }
        catch (InvalidOperationException ex)
        {
            // Lỗi nghiệp vụ (trùng mã, ngày không hợp lệ...)
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi lưu khuyến mãi: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        onCancel();
    }

    /// <summary>
    /// Validate tất cả các trường trong form
    /// Theo yêu cầu trong Requirement.md:
    /// - Discount value > 0
    /// - Start date <= End date
    /// - Promo code unique (không trùng, không ký tự đặc biệt)
    /// </summary>
    private async Task<bool> ValidateFormAsync()
    {
        var errors = new Dictionary<string, string>();

        // === Mã khuyến mãi ===
        if (string.IsNullOrWhiteSpace(PromotionData.Code))
        {
            errors["Code"] = "Mã khuyến mãi không được để trống";
        }
        else
        {
            // Không chứa ký tự đặc biệt
            if (!System.Text.RegularExpressions.Regex.IsMatch(PromotionData.Code, @"^[A-Za-z0-9_]+$"))
            {
                errors["Code"] = "Mã chỉ được chứa chữ cái, số và dấu gạch dưới (_)";
            }
        }

        // === Giá trị giảm > 0 ===
        if (PromotionData.Value <= 0)
        {
            errors["Value"] = "Giá trị giảm phải lớn hơn 0";
        }

        // Nếu là phần trăm, không được vượt quá 100
        if (PromotionData.Type == PromoType.Percent && PromotionData.Value > 100)
        {
            errors["Value"] = "Giảm theo phần trăm không được vượt quá 100%";
        }

        // === Ngày tháng hợp lệ: StartDate <= EndDate ===
        if (PromotionData.EndDate < PromotionData.StartDate)
        {
            errors["EndDate"] = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu";
        }

        // === Mã không trùng lặp ===
        if (!string.IsNullOrWhiteSpace(PromotionData.Code))
        {
            try
            {
                var existing = await _promoService.GetByCodeAsync(PromotionData.Code);
                if (existing != null && existing.Id != PromotionData.Id)
                {
                    errors["Code"] = $"Mã '{PromotionData.Code}' đã tồn tại. Vui lòng chọn mã khác.";
                }
            }
            catch
            {
                // Ignore lỗi query khi validate
            }
        }

        ValidationErrors = errors;
        return errors.Count == 0;
    }

    /// <summary>
    /// Kiểm tra form có lỗi không (dùng cho disabling submit button)
    /// </summary>
    public bool HasErrors => ValidationErrors.Count > 0;

    // ===== Sync ComboBox index ↔ PromotionData type/target =====

    partial void OnSelectedTypeIndexChanged(int value)
    {
        PromotionData.Type = value == 1 ? PromoType.FixedAmount : PromoType.Percent;
    }

    partial void OnSelectedTargetIndexChanged(int value)
    {
        if (value >= 0 && value <= 3)
            PromotionData.Target = (PromoTarget)value;
    }
}