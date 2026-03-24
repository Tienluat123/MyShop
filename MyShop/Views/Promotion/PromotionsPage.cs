using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MyShop.ViewModels;
using MyShop.Views.Promotion.Components;
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
            if (item.IsActive != switcher.IsOn) {
                ViewModel.ToggleActiveCommand.Execute(item);
            }
        }
    }

private async void AddPromotion_Click(object sender, RoutedEventArgs e)
{
    var dialog = new AddPromotionModal()
    {
        XamlRoot = this.XamlRoot
    };

    // Bỏ việc gán "var result =", vì khi dùng nút custom gọi Hide(), result luôn là None
    await dialog.ShowAsync(); 

    // Kiểm tra trực tiếp xem object NewPromotion có dữ liệu hay không
    if (dialog.NewPromotion != null)
    {
        // Thêm vào danh sách gốc
        ViewModel.Promotions.Insert(0, dialog.NewPromotion);
        
        // Cập nhật lại bộ lọc và danh sách hiển thị trên UI
        ViewModel.RebuildFilter();
    }
}
private async void EditPromotion_Click(object sender, RoutedEventArgs e)
    {
        // Lấy item được gắn vào dòng đó thông qua DataContext
        if (sender is Button btn && btn.DataContext is PromotionItem item)
        {
            var dialog = new EditPromotionModal(item)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Sau khi Modal cập nhật dữ liệu vào object 'item', 
                // ta cần báo ViewModel làm mới danh sách hiển thị
                ViewModel.RebuildFilter();
            }
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
