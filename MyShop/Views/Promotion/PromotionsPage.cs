using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views.Promotion.Components;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MyShop.Views.Promotion;

public sealed partial class PromotionsPage : Page
{
    private PromotionsViewModel? viewModel;

    public PromotionsPage()
    {
        InitializeComponent();
        Loaded += PromotionsPage_Loaded;
    }

    private void PromotionsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (PromotionContentFrame.Content is null)
        {
            NavigateToManagement();
        }
    }

    private void NavigateToManagement()
    {
        viewModel = new PromotionsViewModel(
            onAdd: () => ShowPromotionFormDialog(isEditMode: false),
            onEdit: promo => ShowPromotionFormDialog(isEditMode: true, promo));

        PromotionContentFrame.Navigate(typeof(PromotionManagementView), viewModel);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private async void ShowPromotionFormDialog(bool isEditMode, PromotionItem? promo = null)
    {
        var formViewModel = new PromotionFormViewModel(isEditMode, promo, onCancel: () => { });
        var dialog = new AddPromotionModal(formViewModel)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && isEditMode == false)
        {
            // If it's a new promotion, we need to add it to the list
            // Note: In a real app, the ViewModel might handle this via a service,
            // but here we follow the existing pattern of ViewModel holding mock data.
            var newPromo = new PromotionItem
            {
                Id = viewModel?.PageItems.Any() == true ? viewModel.PageItems.Max(p => p.Id) + 1 : 1,
                Code = formViewModel.PromotionData.Code,
                Type = formViewModel.PromotionData.Type,
                Value = formViewModel.PromotionData.Value,
                MinimumOrderAmount = formViewModel.PromotionData.MinimumOrderAmount,
                Target = formViewModel.PromotionData.Target,
                StartDate = formViewModel.PromotionData.StartDate,
                EndDate = formViewModel.PromotionData.EndDate,
                IsActive = true
            };
            viewModel?.AddNewPromotion(newPromo);
        }
        else if (result == ContentDialogResult.Primary && isEditMode && promo != null)
        {
            // For edit mode, the data is typically bound or updated manually
            // The existing RebuildFilter will refresh the view
            viewModel?.RebuildFilter();
        }
    }
}
