namespace MyShop.Services;

public interface INotificationService
{
    void ShowSuccess(string message, int durationMs = 1000);
    void ShowError(string message, int durationMs = 1000);
    void ShowInfo(string message, int durationMs = 1000);

    event Action<NotificationMessage>? OnNotification;
}

public class NotificationMessage
{
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int DurationMs { get; set; }
}

public enum NotificationType
{
    Success,
    Error,
    Info
}

public class NotificationService : INotificationService
{
    public event Action<NotificationMessage>? OnNotification;

    public void ShowSuccess(string message, int durationMs = 1000)
    {
        Show(message, NotificationType.Success, durationMs);
    }

    public void ShowError(string message, int durationMs = 1000)
    {
        Show(message, NotificationType.Error, durationMs);
    }

    public void ShowInfo(string message, int durationMs = 1000)
    {
        Show(message, NotificationType.Info, durationMs);
    }

    private void Show(string message, NotificationType type, int durationMs)
    {
        OnNotification?.Invoke(new NotificationMessage
        {
            Message = message,
            Type = type,
            DurationMs = durationMs
        });
    }
}
