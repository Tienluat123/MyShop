using System.Collections.ObjectModel;
using MyShop.Views;
using MyShop.Views.CustomerManagement;
using MyShop.Views.Pos;
using MyShop.Views.ProductManagement;
using MyShop.Views.Promotion;
using MyShop.Views.StaffManagement;
using MyShop.Views.AppConfig;
using MyShop.Views.Report;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// ViewModel cho MainPage - quản lý navigation giữa các pages
/// </summary>
public class MainPageViewModel : BaseViewModel
{
    private const string KeyLastScreen = "LastScreen";
    private const string DefaultLastScreen = "DashboardPage";

    private readonly IUserSettingService _userSettingService;
    private readonly ISessionService _sessionService;
    private Type? _currentPageType;
    private string _currentPageTag = DefaultLastScreen;
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

    public string CurrentPageTag
    {
        get => _currentPageTag;
        set
        {
            if (_currentPageTag != value)
            {
                _currentPageTag = value;
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

    public MainPageViewModel(IUserSettingService userSettingService, ISessionService sessionService)
    {
        _userSettingService = userSettingService ?? throw new ArgumentNullException(nameof(userSettingService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
    }

    public async Task LoadLastScreenAsync()
    {
        var user = _sessionService.CurrentUser;
        var lastScreen = user == null
            ? DefaultLastScreen
            : await _userSettingService.GetAsync(user.Id, KeyLastScreen, DefaultLastScreen);

        CurrentPageTag = NormalizePageTag(lastScreen);
        CurrentPageType = GetPageTypeFromTag(CurrentPageTag) ?? typeof(DashboardPage);
    }

    public static Type? GetPageTypeFromTag(string? tag)
    {
        return tag switch
        {
            "DashboardPage" => typeof(DashboardPage),
            "ReportPage" => typeof(ReportPage),
            "PosPage" => typeof(PosPage),
            "ManageProductPage" => typeof(ManageProductPage),
            "CustomersPage" => typeof(CustomerPage),
            "CustomerPage" => typeof(CustomerPage),
            "PromotionsPage" => typeof(PromotionsPage),
            "StaffSchedulePage" => typeof(StaffSchedulePage),
            "AppConfigPage" => typeof(AppConfigPage),
            _ => null
        };
    }

    public static string NormalizePageTag(string? tag)
    {
        return tag switch
        {
            "DashboardPage" => "DashboardPage",
            "ReportPage" => "ReportPage",
            "PosPage" => "PosPage",
            "ManageProductPage" => "ManageProductPage",
            "CustomersPage" => "CustomersPage",
            "CustomerPage" => "CustomersPage",
            "PromotionsPage" => "PromotionsPage",
            "StaffSchedulePage" => "StaffSchedulePage",
            "AppConfigPage" => "AppConfigPage",
            _ => DefaultLastScreen
        };
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
