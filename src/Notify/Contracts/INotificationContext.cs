using Notify.Entities;

namespace Notify.Contracts;

public interface INotificationContext
{
    IReadOnlyCollection<Notification> Notifications { get; }

    bool HasNotifications { get; }

    bool IsEmpty { get; }

    void AddNotification(string key, params object[] args);

    void AddNotification(string? property, string key, params object[] args);

    void AddNotification(Notification notification);

    void Clear();
}
