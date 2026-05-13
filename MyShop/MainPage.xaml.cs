using MyShop.Views;
using MyShop.Views.AppConfig;
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views.CustomerManagement;
using MyShop.Views.Pos;
using MyShop.Views.ProductManagement;
using MyShop.Views.Promotion;
using MyShop.Views.StaffManagement;
using MyShop.Views.Report;
using MyShop.Di;
using MyShop.Services;

namespace MyShop.Views;

public sealed partial class MainPage : Page
{
    private const string KeyLastScreen = "LastScreen";

    private MainPageViewModel? _viewModel;
    private readonly IUserSettingService _userSettingService;
    private readonly ISessionService _sessionService;

    public MainPage()
    {
        this.InitializeComponent();

        _userSettingService = ServiceContainer.Instance.Get<IUserSettingService>();
        _sessionService = ServiceContainer.Instance.Get<ISessionService>();

        // ✅ Set ViewModel as DataContext
        _viewModel = new MainPageViewModel(_userSettingService, _sessionService);
        this.DataContext = _viewModel;

        // Setup NotificationService
        var notificationService = ServiceContainer.Instance.Get<INotificationService>();
        NotificationViewControl.SetNotificationService(notificationService);

        // Lấy thông tin User từ Session để hiển thị
        var user = _sessionService.CurrentUser;

        if (user != null)
        {
            ProfileNameText.Text = $"{user.FirstName} {user.LastName}";
            ProfileRoleText.Text = user.Role == "1" ? "Administrator" : "Sales Staff";
        }

        _ = LoadInitialPageAsync();
    }

    /// <summary>
    /// Handle navigation khi user click menu item
    /// </summary>
    private async void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is not string tag)
            return;

        Type? pageType = MainPageViewModel.GetPageTypeFromTag(tag);

        if (pageType != null)
        {
            // Update ViewModel current page
            if (_viewModel != null)
            {
                _viewModel.CurrentPageType = pageType;
                _viewModel.CurrentPageTag = tag;
            }

            // Navigate frame
            ContentFrame.Navigate(pageType);
            SelectNavigationItem(tag);

            // Keep content-first flow on small screens by auto-collapsing the pane.
            if (sender.DisplayMode is NavigationViewDisplayMode.Minimal or NavigationViewDisplayMode.Compact)
            {
                sender.IsPaneOpen = false;
            }

            try
            {
                var user = _sessionService.CurrentUser;
                if (user != null)
                {
                    await _userSettingService.SetAsync(user.Id, KeyLastScreen, tag);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving last screen: {ex.Message}");
            }
        }
    }

    private async Task LoadInitialPageAsync()
    {
        if (_viewModel == null)
            return;

        try
        {
            await _viewModel.LoadLastScreenAsync();

            if (_viewModel.CurrentPageType != null && ContentFrame.Content == null)
            {
                ContentFrame.Navigate(_viewModel.CurrentPageType);
                SelectNavigationItem(_viewModel.CurrentPageTag);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading last screen: {ex.Message}");
            _viewModel.CurrentPageType = typeof(DashboardPage);
            _viewModel.CurrentPageTag = "DashboardPage";
            if (ContentFrame.Content == null)
            {
                ContentFrame.Navigate(_viewModel.CurrentPageType);
                SelectNavigationItem(_viewModel.CurrentPageTag);
            }
        }
    }

    private void SelectNavigationItem(string tag)
    {
        foreach (var menuItem in NavView.MenuItems)
        {
            if (menuItem is NavigationViewItem navigationItem &&
                navigationItem.Tag is string itemTag &&
                itemTag == tag)
            {
                NavView.SelectedItem = navigationItem;
                return;
            }
        }
    }

    /// <summary>
    /// Đăng xuất và quay lại trang Login
    /// </summary>
    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        this.Frame?.Navigate(typeof(LoginPage));
    }
}
