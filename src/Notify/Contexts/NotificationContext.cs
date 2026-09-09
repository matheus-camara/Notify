using Notify.Contracts;
using Notify.Entities;

namespace Notify.Contexts;

public sealed class NotificationContext : INotificationContext
{
    private readonly List<Notification> _notifications = [];
    private readonly INotificationLocalizer? _localizer;

    public NotificationContext(INotificationLocalizer? localizer = null)
    {
        _localizer = localizer;
    }

    public IReadOnlyCollection<Notification> Notifications =>
        _notifications.AsReadOnly();

    public bool HasNotifications => _notifications.Count > 0;

    public bool IsEmpty => !HasNotifications;

    public void AddNotification(string key, params object[] args)
    {
        AddNotification(null, key, args);
    }

    public void AddNotification(
        string? property,
        string key,
        params object[] args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var message = _localizer?.Localize(key, args) ?? key;

        AddNotification(new Notification(key, property, message));
    }

    public void AddNotification(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        _notifications.Add(notification);
    }

    public void Clear()
    {
        _notifications.Clear();
    }
}
