namespace MoqDemo.Core.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(string message);
    Task NotifyAdminAsync(string message);
    event EventHandler<string>? NotificationSent;
}
