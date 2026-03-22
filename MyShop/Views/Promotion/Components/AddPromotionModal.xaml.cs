using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using System;

namespace MyShop.Views.Promotion.Components;

public sealed partial class AddPromotionModal : ContentDialog
{
    public PromotionItem NewPromotion { get; private set; }

    public AddPromotionModal()
    {
        this.InitializeComponent();
        StartDatePicker.Date = DateTimeOffset.Now;
        EndDatePicker.Date = DateTimeOffset.Now.AddDays(30);
    }

    // Đổi từ (ContentDialog sender, ContentDialogButtonClickEventArgs args)
// Thành (object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    private void ContentDialog_PrimaryButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CodeEntry.Text))
        {
            // Vì RoutedEventArgs không có thuộc tính Cancel, 
            // nếu bạn muốn ngăn không cho đóng Dialog khi lỗi, 
            // bạn có thể thực hiện logic thông báo ở đây.
            return; 
        }

        NewPromotion = new PromotionItem
        {
            Code = CodeEntry.Text.ToUpper(),
            Type = (PromoType)TypeCombo.SelectedIndex,
            Value = (decimal)ValueEntry.Value,
            MinimumOrderAmount = (decimal)MinOrderEntry.Value,
            Target = (PromoTarget)TargetCombo.SelectedIndex,
            StartDate = StartDatePicker.Date.DateTime,
            EndDate = EndDatePicker.Date.DateTime,
            IsActive = true
        };

        // Quan trọng: Vì ta đang dùng nút custom, phải gọi Hide() để đóng Modal
        this.Hide(); 
    }
}