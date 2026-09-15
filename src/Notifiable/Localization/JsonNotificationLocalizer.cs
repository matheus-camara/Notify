using Notifiable.Contracts;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Notifiable.Localization;

public sealed class JsonNotificationLocalizer : INotificationLocalizer
{
    private readonly Assembly _assembly;
    private readonly string _resourcePrefix;
    private readonly string _defaultCulture;
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cache = [];

    public JsonNotificationLocalizer(
        Assembly assembly,
        string resourcePrefix,
        string defaultCulture = "en-US")
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourcePrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultCulture);

        _assembly = assembly;
        _resourcePrefix = resourcePrefix.TrimEnd('.');
        _defaultCulture = defaultCulture;
    }

    public string Localize(
        string key,
        params object[] arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(arguments);

        var culture = CultureInfo.CurrentUICulture;

        var template =
            FindValue(culture.Name, key)
            ?? FindValue(culture.TwoLetterISOLanguageName, key)
            ?? FindValue(_defaultCulture, key)
            ?? key;

        return arguments.Length == 0
            ? template
            : string.Format(culture, template, arguments);
    }

    private string? FindValue(string cultureName, string key)
    {
        var resources = Load(cultureName);
        return resources.TryGetValue(key, out var value) ? value : null;
    }

    private IReadOnlyDictionary<string, string> Load(string culture)
    {
        if (_cache.TryGetValue(culture, out var cached))
            return cached;

        var resourceName = $"{_resourcePrefix}.notifications.{culture}.json";

        using var stream = _assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            _cache[culture] = new Dictionary<string, string>().AsReadOnly();
            return _cache[culture];
        }

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var resources =
            JsonSerializer.Deserialize<Dictionary<string, string>>(json)?.AsReadOnly()
            ?? new Dictionary<string, string>().AsReadOnly();

        _cache[culture] = resources;
        return resources;
    }
}
