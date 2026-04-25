using System;
using Microsoft.UI.Xaml.Controls;
using MyShop.Di;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.Promotion.Components;

namespace MyShop.Views.Promotion;

public sealed partial class PromotionsPage : Page
{
    private PromotionsViewModel? _viewModel;

    public PromotionsPage()
    {
        InitializeComponent();
        Loaded += PromotionsPage_Loaded;
    }

    private void PromotionsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (PromotionContentFrame.Content == null)
            NavigateToManagement();
    }

    private void NavigateToManagement()
    {
        var promoService = ServiceContainer.Instance.Get<IPromotionService>();
        var configService = ServiceContainer.Instance.Get<IAppConfigService>();

        _viewModel = new PromotionsViewModel(
            promoService,
            configService,
            onAdd: () => ShowPromotionFormDialog(isEditMode: false),
            onEdit: promo => ShowPromotionFormDialog(isEditMode: true, promo));

        PromotionContentFrame.Navigate(typeof(PromotionManagementView), _viewModel);
    }

    private async void ShowPromotionFormDialog(bool isEditMode, PromotionItem? promo = null)
    {
        var promoService = ServiceContainer.Instance.Get<IPromotionService>();

        var formVm = new PromotionFormViewModel(
            isEditMode,
            promo,
            promoService,
            onCancel: () => { },
            onSuccess: OnPromotionFormSuccess);

        var dialog = new AddPromotionModal(formVm) { XamlRoot = this.XamlRoot };
        await dialog.ShowAsync();
    }

    private void OnPromotionFormSuccess(PromotionItem promo)
    {
        if (_viewModel == null) return;

        // Luôn refresh từ database sau khi thêm hoặc sửa thành công
        // promo.Id đã được gán sau khi save, không cần phân biệt add/update
        _ = _viewModel.RefreshFromDatabaseAsync();
    }
}