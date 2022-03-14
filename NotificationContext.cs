using Microsoft.Extensions.Localization;

namespace Notify;

public class NotificationContext : INotificationContext
{
    private readonly IStringLocalizer _localizer;

    public NotificationContext(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

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
        AddNotification(property, _localizer.GetString(key));
    }

    public void AddNotification(string? property, LocalizedString message)
    {
        # if DEBUG
        if (message.ResourceNotFound)
            throw new InvalidOperationException($"{property} is not defined on resource file.");
        # endif

        AddNotification(new Notification(message.Name, property, message.Value));
    }

    public void AddNotification(Notification message)
    {
        _notifications.Add(message);
    }

    public void AddNotification(IEnumerable<Notification> message)
    {
        _notifications.AddRange(message);
    }

    public void Clear()
    {
        _notifications.Clear();
    }

    
}