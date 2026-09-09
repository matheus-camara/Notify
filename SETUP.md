# Notifiable - Setup Guide

This guide explains how to add **Notifiable** to a .NET project and use it for business notifications with optional JSON localization.

## Install

```bash
dotnet add package Notifiable
```

If developing the library together with another project:

```xml
<ProjectReference Include="..\path\to\Notifiable\src\Notifiable\Notifiable.csproj" />
```

Main namespaces:

```csharp
using Notifiable.Contracts;
using Notifiable.Contexts;
using Notifiable.Entities;
using Notifiable.Localization;
```

## Register the notification context

Register `INotificationContext` as scoped so notifications belong to the current request/operation:

```csharp
builder.Services.AddScoped<INotificationContext, NotificationContext>();
```

## JSON resources

Create culture-specific resources in the consuming project:

```text
MyProject/
└── Resources/
    ├── notifications.en-US.json
    ├── notifications.pt.json
    └── notifications.pt-BR.json
```

Example:

```json
{
  "PET_NOT_FOUND": "Pet não encontrado.",
  "PET_NAME_REQUIRED": "O nome do pet é obrigatório.",
  "PET_CREATED": "Pet {0} criado com sucesso."
}
```

Keep notification keys stable and language-independent.

## Embed the resources

Culture-named JSON files must explicitly disable MSBuild culture handling:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources/notifications.*.json">
    <WithCulture>false</WithCulture>
  </EmbeddedResource>
</ItemGroup>
```

This keeps the JSON inside the consuming assembly instead of moving it to satellite assemblies.

## Register JSON localization

```csharp
builder.Services.AddSingleton<INotificationLocalizer>(
    new JsonNotificationLocalizer(
        typeof(Program).Assembly,
        "MyProject.Resources",
        "en-US"));
```

The resource prefix must match the manifest resource name before `notifications.<culture>.json`. You can inspect manifest names with:

```csharp
var resources = typeof(Program).Assembly.GetManifestResourceNames();
```

## Request culture

`JsonNotificationLocalizer` uses `CultureInfo.CurrentUICulture`; it does not inspect HTTP headers itself. Configure ASP.NET Core request localization:

```csharp
var supportedCultures = new[]
{
    new CultureInfo("pt-BR"),
    new CultureInfo("en-US")
};

var options = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(options);
```

## Add notifications

```csharp
_notifications.AddNotification("PET_NOT_FOUND");

_notifications.AddNotification("Name", "PET_NAME_REQUIRED");

_notifications.AddNotification("PET_CREATED", pet.Name);
```

A notification contains the stable key, optional property, and localized message:

```json
{
  "key": "PET_CREATED",
  "property": null,
  "message": "Pet Rex criado com sucesso."
}
```

## Culture fallback

Resolution order is:

1. Exact UI culture, e.g. `pt-BR`
2. Language-only culture, e.g. `pt`
3. Configured default culture, e.g. `en-US`
4. The notification key itself

## API responses

Notifiable intentionally does not know about HTTP status codes. Map notifications in the API layer according to your application's convention. For example, with `422 Unprocessable Entity`:

```csharp
if (_notifications.HasNotifications)
{
    return UnprocessableEntity(_notifications.Notifications);
}
```

For larger APIs, this mapping can live in an ASP.NET Core filter or middleware.

## Testing

Test resources must also be embedded:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources/notifications.*.json">
    <WithCulture>false</WithCulture>
  </EmbeddedResource>
</ItemGroup>
```

A useful regression test is:

```csharp
var resources = typeof(JsonNotificationLocalizerTests)
    .Assembly
    .GetManifestResourceNames();

Assert.Contains(
    "MyProject.Tests.Resources.notifications.en-US.json",
    resources);
```

This catches the common case where JSON files exist in the project but were not actually embedded in the assembly.
