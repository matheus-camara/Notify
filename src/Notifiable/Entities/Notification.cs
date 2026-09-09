namespace Notifiable.Entities;

public sealed record Notification(
    string Key,
    string? Property,
    string Message);
