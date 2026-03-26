using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MyShop.ViewModels;
using MyShop.Views.Promotion.Components;
using System.Linq;

namespace MyShop.Views.Promotion;

public sealed partial class PromotionsPage : Page
{
    private PromotionsViewModel ViewModel => (PromotionsViewModel)DataContext;

    public PromotionsPage()
    {
        this.InitializeComponent();
        DataContext = new PromotionsViewModel();
    }

    private void PromotionStatus_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch switcher && switcher.DataContext is PromotionItem item)
        {
            // Tránh loop khi binding TwoWay
            if (item.IsActive != switcher.IsOn)
            {
                ViewModel.ToggleActiveCommand.Execute(item);
            }
        }
    }

    private async void AddPromotion_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = new PromotionFormViewModel(isEditMode: false, promotion: null, onCancel: () => { });
        var dialog = new AddPromotionModal(viewModel)
        {
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();

        // Create and insert the new promotion into the list
        var newPromotion = new PromotionItem
        {
            Id = ViewModel.Promotions.Any() ? ViewModel.Promotions.Max(p => p.Id) + 1 : 1,
            Code = viewModel.PromotionData.Code.ToUpper(),
            Type = viewModel.PromotionData.Type,
            Value = viewModel.PromotionData.Value,
            MinimumOrderAmount = viewModel.PromotionData.MinimumOrderAmount,
            Target = viewModel.PromotionData.Target,
            StartDate = viewModel.PromotionData.StartDate,
            EndDate = viewModel.PromotionData.EndDate,
            IsActive = true
        };

        ViewModel.Promotions.Insert(0, newPromotion);
        ViewModel.RebuildFilter();
    }

    private async void EditPromotion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PromotionItem item)
        {
            var viewModel = new PromotionFormViewModel(isEditMode: true, promotion: item, onCancel: () => { });
            var dialog = new AddPromotionModal(viewModel)
            {
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();

            // The viewModel.PromotionData already has the updated values
            // Now we need to apply them to the original item
            item.Code = viewModel.PromotionData.Code;
            item.Type = viewModel.PromotionData.Type;
            item.Value = viewModel.PromotionData.Value;
            item.MinimumOrderAmount = viewModel.PromotionData.MinimumOrderAmount;
            item.Target = viewModel.PromotionData.Target;
            item.StartDate = viewModel.PromotionData.StartDate;
            item.EndDate = viewModel.PromotionData.EndDate;
            item.IsActive = viewModel.PromotionData.IsActive;

            ViewModel.RebuildFilter();
        }
    }

    private void DeletePromotion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PromotionItem item)
        {
            ViewModel.RemoveCommand.Execute(item);
        }
    }
}
