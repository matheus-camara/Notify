using Microsoft.Extensions.Localization;

namespace Notify;

public class LocalizedNotificationContext : ILocalizedNotificationContext
{
    private readonly INotificationContext _context;
    private readonly IStringLocalizer<LocalizedNotificationContext> _localizer;

    public LocalizedNotificationContext(INotificationContext context, IStringLocalizer<LocalizedNotificationContext> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public IReadOnlyCollection<Notification> Notifications => _context.Notifications;

    public bool HasNotifications => _context.HasNotifications;

    public bool IsEmpty => _context.IsEmpty;

    public void AddNotification(string key)
    {
        AddNotification(null, key);
    }

    public void AddNotification(string? property, string key)
    {
        AddNotification(property, key);
    }

    public void AddNotification(Notification message)
    {
        _context.AddNotification(message);
    }

    public void Clear()
    {
        _context.Clear();
    }

    public void AddNotification(string? property, LocalizedString message)
    {
# if DEBUG
        if (message.ResourceNotFound)
            throw new InvalidOperationException($"{property} is not defined on resource file.");
# endif

        AddNotification(new Notification(message.Name, property, message.Value));
    }
}