using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Services;
using System;
using System.Threading.Tasks;

namespace MyShop.Views.Controls;

public sealed partial class NotificationView : UserControl
{
    private DispatcherTimer? _dismissTimer;

    public NotificationView()
    {
        this.InitializeComponent();
    }

    public void SetNotificationService(INotificationService notificationService)
    {
        notificationService.OnNotification += async (message) =>
        {
            await ShowNotificationAsync(message);
        };
    }

    private async Task ShowNotificationAsync(NotificationMessage message)
    {
        Toast.Severity = message.Type switch
        {
            NotificationType.Success => InfoBarSeverity.Success,
            NotificationType.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };

        Toast.Message = message.Message;
        Toast.IsOpen = true;

        // Dismiss after duration
        _dismissTimer?.Stop();
        _dismissTimer = new DispatcherTimer();
        _dismissTimer.Interval = TimeSpan.FromMilliseconds(message.DurationMs);
        _dismissTimer.Tick += (s, e) =>
        {
            Toast.IsOpen = false;
            _dismissTimer.Stop();
        };
        _dismissTimer.Start();
    }

    private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // Allow click to dismiss
        Toast.IsOpen = false;
    }
}
