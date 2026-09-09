using Notifiable.Entities;

namespace Notifiable.Tests.Entities;

public class NotificationTests
{
    [Fact]
    public void Constructor_ShouldPreserveValues()
    {
        var notification = new Notification(
            "PET_NOT_FOUND",
            "petId",
            "Pet was not found.");

        Assert.Equal("PET_NOT_FOUND", notification.Key);
        Assert.Equal("petId", notification.Property);
        Assert.Equal("Pet was not found.", notification.Message);
    }

    [Fact]
    public void Notification_ShouldSupportNullProperty()
    {
        var notification = new Notification(
            "PET_NOT_FOUND",
            null,
            "Pet was not found.");

        Assert.Null(notification.Property);
    }

    [Fact]
    public void NotificationsWithSameValues_ShouldBeEqual()
    {
        var first = new Notification("A", null, "Message");
        var second = new Notification("A", null, "Message");

        Assert.Equal(first, second);
    }
}
