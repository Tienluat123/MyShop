using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.Services;
using System;
using System.Threading.Tasks;

namespace MyShop.Views.Components;

public sealed partial class NotificationView : UserControl
{
    private INotificationService? _notificationService;
    private DispatcherTimer? _dismissTimer;

    public NotificationView()
    {
        this.InitializeComponent();
    }

    public void Initialize(INotificationService notificationService)
    {
        SetNotificationService(notificationService);
    }

    public void SetNotificationService(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _notificationService.OnNotification += ShowNotification;
    }

    private void ShowNotification(NotificationMessage message)
    {
        // Remove previous notification if any
        NotificationContainer.Children.Clear();

        // Create notification UI
        var border = new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            MaxWidth = 300,
            Margin = new Thickness(0, 0, 16, 16)
        };

        // Set background and text color based on type
        var (bgColor, textColor) = GetColorsForType(message.Type);
        border.Background = new SolidColorBrush(bgColor);

        var textBlock = new TextBlock
        {
            Text = message.Message,
            Foreground = new SolidColorBrush(textColor),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14
        };

        border.Child = textBlock;
        NotificationContainer.Children.Add(border);

        // Set up auto-dismiss
        if (_dismissTimer != null)
        {
            _dismissTimer.Stop();
        }

        _dismissTimer = new DispatcherTimer();
        _dismissTimer.Interval = TimeSpan.FromMilliseconds(message.DurationMs);
        _dismissTimer.Tick += (s, e) =>
        {
            _dismissTimer?.Stop();
            NotificationContainer.Children.Clear();
        };
        _dismissTimer.Start();
    }

    private (Windows.UI.Color bg, Windows.UI.Color text) GetColorsForType(NotificationType type) => type switch
    {
        NotificationType.Success => (Windows.UI.Color.FromArgb(255, 76, 175, 80), Windows.UI.Color.FromArgb(255, 255, 255, 255)), // Green
        NotificationType.Error => (Windows.UI.Color.FromArgb(255, 244, 67, 54), Windows.UI.Color.FromArgb(255, 255, 255, 255)),   // Red
        NotificationType.Info => (Windows.UI.Color.FromArgb(255, 33, 150, 243), Windows.UI.Color.FromArgb(255, 255, 255, 255)),   // Blue
        _ => (Windows.UI.Color.FromArgb(255, 255, 193, 7), Windows.UI.Color.FromArgb(255, 0, 0, 0))  // Yellow
    };
}
