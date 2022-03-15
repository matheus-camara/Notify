namespace Notify;

public interface INotificationContext
{
    IReadOnlyCollection<Notification> Notifications { get; }
    bool HasNotifications { get; }
    bool IsEmpty { get; }
    void AddNotification(string? property, string key);
    void AddNotification(string key);
    void AddNotification(Notification notification);
    void Clear();
}