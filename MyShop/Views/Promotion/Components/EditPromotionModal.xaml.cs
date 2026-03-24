using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using System;

namespace MyShop.Views.Promotion.Components;

public sealed partial class EditPromotionModal : ContentDialog
{
    private PromotionItem _originalItem;

    public EditPromotionModal(PromotionItem item)
    {
        this.InitializeComponent();
        _originalItem = item;
        LoadData();
    }

    private void LoadData()
    {
        CodeEntry.Text = _originalItem.Code;
        TypeCombo.SelectedIndex = (int)_originalItem.Type;
        ValueEntry.Value = (double)_originalItem.Value;
        MinOrderEntry.Value = (double)_originalItem.MinimumOrderAmount;
        TargetCombo.SelectedIndex = (int)_originalItem.Target;
        StartDatePicker.Date = _originalItem.StartDate;
        EndDatePicker.Date = _originalItem.EndDate;
        StatusSwitch.IsOn = _originalItem.IsActive;
    }
    // Đổi tương tự như trên
    private void ContentDialog_PrimaryButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _originalItem.Code = CodeEntry.Text;
        _originalItem.Type = (PromoType)TypeCombo.SelectedIndex;
        _originalItem.Value = (decimal)ValueEntry.Value;
        _originalItem.MinimumOrderAmount = (decimal)MinOrderEntry.Value;
        _originalItem.Target = (PromoTarget)TargetCombo.SelectedIndex;
        _originalItem.StartDate = StartDatePicker.Date.DateTime;
        _originalItem.EndDate = EndDatePicker.Date.DateTime;
        _originalItem.IsActive = StatusSwitch.IsOn;

        // Đóng Modal sau khi cập nhật
        this.Hide();
    }

    }