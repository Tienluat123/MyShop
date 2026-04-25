using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.ViewModels;

public partial class PromotionFormViewModel : ObservableObject
{
    private readonly Action onCancel;

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

    public PromotionFormViewModel(bool isEditMode, PromotionItem? promotion, Action onCancel)
    {
        this.onCancel = onCancel;
        IsEditMode = isEditMode;

        if (isEditMode && promotion is not null)
        {
            FormTitle = "Edit Promotion";
            Subtitle = "Modify the details of this promotion";
            SubmitButtonText = "Save Changes";
            CancelButtonText = "Discard";
            PromotionData = promotion;
        }
        else
        {
            FormTitle = "New Promotion";
            Subtitle = "Setup a new discount campaign for your shop";
            SubmitButtonText = "Create Promotion";
            CancelButtonText = "Cancel";
            PromotionData = new PromotionItem
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                IsActive = true
            };
        }
    }

    [RelayCommand]
    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(PromotionData.Code))
            return;

        onCancel();
    }

    [RelayCommand]
    private void Cancel()
    {
        onCancel();
    }
}
