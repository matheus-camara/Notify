namespace Notify;

public class NotificationContext : INotificationContext
{
    private readonly List<Notification> _notifications = new();
    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();
    public bool HasNotifications => _notifications.Any();
    public bool IsEmpty => !HasNotifications;

    public void AddNotification(string key)
    {
        AddNotification(null, key);
    }

    public void AddNotification(string? property, string key)
    {
        AddNotification(new Notification(string.Empty, property, key));
    }

    public void AddNotification(Notification message)
    {
        _notifications.Add(message);
    }

    public void Clear()
    {
        _notifications.Clear();
    }
}