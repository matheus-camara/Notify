using System.Globalization;
using Notify.Localization;

namespace Notify.Tests.Localization;

public class JsonNotificationLocalizerTests
{
    [Fact]
    public void NotificationResources_ShouldBeEmbeddedInTestAssembly()
    {
        var resources = typeof(JsonNotificationLocalizerTests)
            .Assembly
            .GetManifestResourceNames();

        Assert.Contains("Notify.Tests.Resources.notifications.en-US.json", resources);
        Assert.Contains("Notify.Tests.Resources.notifications.pt-BR.json", resources);
        Assert.Contains("Notify.Tests.Resources.notifications.pt.json", resources);
    }

    private const string ResourcePrefix = "Notify.Tests.Resources";
    private readonly JsonNotificationLocalizer _localizer =
        new(typeof(JsonNotificationLocalizerTests).Assembly, ResourcePrefix, "en-US");

    [Fact]
    public void ShouldResolveExactCulture()
    {
        using var scope = CreateCultureScope("pt-BR");

        var result = _localizer.Localize("GREETING", []);

        Assert.Equal("Olá.", result);
    }

    [Fact]
    public void ShouldFallbackToLanguageOnlyCulture()
    {
        using var scope = CreateCultureScope("pt-PT");

        var result = _localizer.Localize("LANGUAGE_ONLY", []);

        Assert.Equal("Português.", result);
    }

    [Fact]
    public void ShouldFallbackToDefaultCulture()
    {
        using var scope = CreateCultureScope("fr-FR");

        var result = _localizer.Localize("GREETING", []);

        Assert.Equal("Hello.", result);
    }

    [Fact]
    public void ShouldFallbackToKeyWhenNoTranslationOrFallbackExists()
    {
        using var scope = CreateCultureScope("fr-FR");

        var result = _localizer.Localize("MISSING", []);

        Assert.Equal("MISSING", result);
    }

    [Fact]
    public void ShouldFormatParameterizedMessage()
    {
        using var scope = CreateCultureScope("en-US");

        var result = _localizer.Localize("RESULTS_FOUND", [3]);

        Assert.Equal("Less than 3 results found.", result);
    }

    [Fact]
    public void ShouldFormatUsingCurrentUICulture()
    {
        using var scope = CreateCultureScope("pt-BR");

        var result = _localizer.Localize("NUMBER", [1234.5]);

        Assert.Equal("Número: 1.234,5", result);
    }

    [Fact]
    public void ShouldSupportMultipleArguments()
    {
        using var scope = CreateCultureScope("en-US");

        var result = _localizer.Localize("MULTIPLE", ["John", 4]);

        Assert.Equal("John has 4 pets.", result);
    }

    [Fact]
    public void MissingResource_ShouldNotThrow()
    {
        using var scope = CreateCultureScope("fr-FR");

        var result = _localizer.Localize("GREETING", []);

        Assert.Equal("Hello.", result);
    }

    [Fact]
    public void InvalidKey_ShouldThrow()
    {
        using var scope = CreateCultureScope("en-US");

        Assert.Throws<ArgumentException>(
            () => _localizer.Localize("", []));
    }

    private static IDisposable CreateCultureScope(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        return new CultureScope(culture);
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _originalUICulture = CultureInfo.CurrentUICulture;

        public CultureScope(CultureInfo culture)
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUICulture;
        }
    }
}
