using Notifiable.Contexts;
using Notifiable.Contracts;
using Notifiable.Entities;

namespace Notifiable.Tests.Contexts;

public class NotificationContextTests
{
    [Fact]
    public void NewContext_ShouldBeEmpty()
    {
        var context = new NotificationContext();

        Assert.True(context.IsEmpty);
        Assert.False(context.HasNotifications);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public void AddNotification_ShouldCreateNotificationUsingKeyAsFallbackMessage()
    {
        var context = new NotificationContext();

        context.AddNotification("PET_NOT_FOUND");

        var notification = Assert.Single(context.Notifications);

        Assert.Equal("PET_NOT_FOUND", notification.Key);
        Assert.Null(notification.Property);
        Assert.Equal("PET_NOT_FOUND", notification.Message);
    }

    [Fact]
    public void AddNotification_ShouldPreserveProperty()
    {
        var context = new NotificationContext();

        context.AddNotification("petId", "PET_NOT_FOUND");

        var notification = Assert.Single(context.Notifications);

        Assert.Equal("petId", notification.Property);
    }

    [Fact]
    public void AddNotification_ShouldUseLocalizer()
    {
        var localizer = new FakeLocalizer("Pet was not found.");
        var context = new NotificationContext(localizer);

        context.AddNotification("PET_NOT_FOUND");

        var notification = Assert.Single(context.Notifications);

        Assert.Equal("Pet was not found.", notification.Message);
        Assert.Equal("PET_NOT_FOUND", notification.Key);
    }

    [Fact]
    public void AddNotification_ShouldPassArgumentsToLocalizer()
    {
        var localizer = new FakeLocalizer("Less than 3 results found.");
        var context = new NotificationContext(localizer);

        context.AddNotification("RESULTS_FOUND", 3);

        Assert.Equal(new object[] { 3 }, localizer.Arguments);
    }

    [Fact]
    public void AddNotification_WithEntity_ShouldStoreEntity()
    {
        var context = new NotificationContext();
        var notification = new Notification("A", "field", "Message");

        context.AddNotification(notification);

        Assert.Same(notification, Assert.Single(context.Notifications));
    }

    [Fact]
    public void AddNotification_WithNullEntity_ShouldThrow()
    {
        var context = new NotificationContext();

        Assert.Throws<ArgumentNullException>(
            () => context.AddNotification((Notification)null!));
    }

    [Fact]
    public void AddNotification_WithEmptyKey_ShouldThrow()
    {
        var context = new NotificationContext();

        Assert.Throws<ArgumentException>(
            () => context.AddNotification(""));
    }

    [Fact]
    public void Clear_ShouldRemoveAllNotifications()
    {
        var context = new NotificationContext();

        context.AddNotification("A");
        context.AddNotification("B");

        context.Clear();

        Assert.True(context.IsEmpty);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public void Notifications_ShouldNotAllowExternalMutation()
    {
        var context = new NotificationContext();
        context.AddNotification("A");

        var notifications = context.Notifications;

        Assert.IsAssignableFrom<IReadOnlyCollection<Notification>>(notifications);
        Assert.Single(notifications);
    }

    private sealed class FakeLocalizer(string message) : INotificationLocalizer
    {
        public IReadOnlyCollection<object> Arguments { get; private set; } = [];

        public string Localize(
            string key,
            params object[] arguments)
        {
            Arguments = arguments;
            return message;
        }
    }
}
