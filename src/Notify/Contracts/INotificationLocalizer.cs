namespace Notify.Contracts;

public interface INotificationLocalizer
{
    string Localize(
        string key,
        params object[] arguments);
}
