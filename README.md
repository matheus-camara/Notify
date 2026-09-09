# Notifiable

Version-independent notification library with optional JSON localization and extensive core unit tests.

## Targets

- `net10.0`

The core notification types do not depend on ASP.NET Core.

## Install

```bash
dotnet add package Notifiable
```

## Tests

Run all tests:

```bash
dotnet test
```

## Covered behavior

- Notification value preservation/equality
- Null property handling
- Empty/new notification context
- Adding notifications
- Notification property association
- Entity-based notifications
- Null argument validation
- Empty-key validation
- Clearing notifications
- Localizer integration
- Localizer argument forwarding
- Exact culture lookup
- Language-only fallback
- Default culture fallback
- Parameterized messages
- Culture-aware number formatting
- Multiple parameters
- Missing resources

## Parameterized messages

Resource:

```json
{
  "RESULTS_FOUND": "Less than {0} results found."
}
```

Usage:

```csharp
context.AddNotification("RESULTS_FOUND", 3);
```

Result:

```text
Less than 3 results found.
```

The formatter uses `CultureInfo.CurrentUICulture`.

## Embedded resources

The JSON implementation expects resources to be embedded in the consuming assembly:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources/notifications.*.json"
                    LogicalName="MyProject.Resources.%(Filename)%(Extension)"
                    WithCulture="false" />
</ItemGroup>
```

The resource prefix is the assembly's namespace/path prefix before the resource filename.

## Architecture

The core package remains independent of ASP.NET Core. JSON localization is implemented through `INotificationLocalizer`, allowing applications to provide their own localization strategy.
