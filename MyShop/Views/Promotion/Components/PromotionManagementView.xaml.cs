using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;

namespace MyShop.Views.Promotion.Components;

/// <summary>
/// Page chứa danh sách promotions và các filter buttons.
/// Toggle logic được xử lý hoàn toàn trong ViewModel thông qua
/// PromotionItem.IsActiveChanged event, không cần Toggled event handler ở đây.
/// </summary>
public sealed partial class PromotionManagementView : Page
{
    public PromotionsViewModel? ViewModel => DataContext as PromotionsViewModel;

    public PromotionManagementView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as PromotionsViewModel;
    }
}