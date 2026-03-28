using System.Collections.ObjectModel;
using MyShop.Views;
using MyShop.Views.CustomerManagement;
using MyShop.Views.Pos;
using MyShop.Views.ProductManagement;
using MyShop.Views.Promotion;
using MyShop.Views.StaffManagement;
using MyShop.Views.AppConfig;

namespace MyShop.ViewModels;

/// <summary>
/// ViewModel cho MainPage - quản lý navigation giữa các pages
/// </summary>
public class MainPageViewModel : BaseViewModel
{
    private Type? _currentPageType;
    private RelayCommand? _navigateCommand;

    /// <summary>
    /// Trạng thái current page để binding trong View
    /// </summary>
    public Type? CurrentPageType
    {
        get => _currentPageType;
        set
        {
            if (_currentPageType != value)
            {
                _currentPageType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Navigation command - được gọi khi user click menu item
    /// </summary>
    public RelayCommand NavigateCommand
    {
        get
        {
            if (_navigateCommand == null)
            {
                _navigateCommand = new RelayCommand(() => Navigate());
            }
            return _navigateCommand;
        }
    }

    public MainPageViewModel()
    {
        // Set default page to Customer Management
        CurrentPageType = typeof(CustomerPage);
    }

    /// <summary>
    /// Navigate action without parameters (required by RelayCommand)
    /// </summary>
    private void Navigate()
    {
        OnNavigate();
    }

    /// <summary>
    /// Handle navigation khi user click menu item
    /// </summary>
    private void OnNavigate()
    {
        // Navigation logic sẽ được xử lý bởi event handler trong MainPage.xaml.cs
        // vì cần truyền Tag từ NavigationViewItem
    }
}
