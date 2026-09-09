# Notify - Setup Guide

This guide explains how to add **Notify** to another .NET project and use it for business notifications with optional JSON-based localization.

## 1. What Notify provides

Notify separates expected business validation from exceptional failures.

A notification contains:

```csharp
public sealed record Notification(
    string Key,
    string? Property,
    string Message);
```

For example:

```json
{
  "key": "PET_NOT_FOUND",
  "property": null,
  "message": "Pet não encontrado."
}
```

The library itself does not depend on ASP.NET Core. It can therefore be used from application, domain, or other layers without coupling the core notification model to HTTP.

---

## 2. Install the library

If Notify is published as a NuGet package, install it with:

```bash
dotnet add package Notify
```

If you are developing Notify and the consuming application together, you can instead add a project reference:

```xml
<ItemGroup>
  <ProjectReference Include="..\path\to\Notify\src\Notify\Notify.csproj" />
</ItemGroup>
```

After installation, the main namespaces are:

```csharp
using Notify.Contracts;
using Notify.Contexts;
using Notify.Entities;
using Notify.Localization;
```

---

## 3. Register the notification context

`INotificationContext` should be registered as **scoped** in an ASP.NET Core application.

```csharp
builder.Services.AddScoped<INotificationContext, NotificationContext>();
```

A scoped lifetime is appropriate because notifications belong to the current application operation/request and should not leak between requests.

You can then inject the context into application services:

```csharp
public sealed class GetPetService
{
    private readonly INotificationContext _notifications;

    public GetPetService(INotificationContext notifications)
    {
        _notifications = notifications;
    }

    public Pet? Execute(Guid id)
    {
        // Example only.
        var pet = FindPet(id);

        if (pet is null)
        {
            _notifications.AddNotification("PET_NOT_FOUND");
            return null;
        }

        return pet;
    }
}
```

The service does not need to know how the notification will eventually be returned over HTTP.

---

## 4. Add JSON notification resources

Create a `Resources` directory in the consuming project:

```text
MyProject/
├── Resources/
│   ├── notifications.en-US.json
│   ├── notifications.pt.json
│   └── notifications.pt-BR.json
└── ...
```

Each file is a dictionary where the property name is the notification key and the value is the localized message.

### `notifications.pt-BR.json`

```json
{
  "PET_NOT_FOUND": "Pet não encontrado.",
  "PET_NAME_REQUIRED": "O nome do pet é obrigatório.",
  "PET_CREATED": "Pet {0} criado com sucesso."
}
```

### `notifications.en-US.json`

```json
{
  "PET_NOT_FOUND": "Pet not found.",
  "PET_NAME_REQUIRED": "Pet name is required.",
  "PET_CREATED": "Pet {0} created successfully."
}
```

The notification key should be stable and language-independent. Do not use the translated message as the key.

---

## 5. Embed the JSON resources

The JSON files **must be embedded into the consuming assembly**. Merely placing them in the project is not enough.

Add the following to the consuming project's `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources/notifications.*.json" />
</ItemGroup>
```

For culture-specific filenames such as `notifications.pt-BR.json`, explicitly disable MSBuild's culture-specific resource handling:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources/notifications.*.json">
    <WithCulture>false</WithCulture>
  </EmbeddedResource>
</ItemGroup>
```

The second form is recommended for Notify.

This ensures the JSON files remain embedded in the consuming assembly instead of being treated as satellite/culture resources.

---

## 6. Register JSON localization

Create the localizer using the consuming application's assembly.

For example, in `Program.cs`:

```csharp
using Notify.Contracts;
using Notify.Contexts;
using Notify.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<INotificationContext, NotificationContext>();

builder.Services.AddSingleton<INotificationLocalizer>(
    new JsonNotificationLocalizer(
        typeof(Program).Assembly,
        "MyProject.Resources",
        "en-US"));
```

### Resource prefix

The resource prefix must match the embedded resource name up to the `notifications.<culture>.json` part.

For the following project:

```text
MyProject
└── Resources
    ├── notifications.en-US.json
    └── notifications.pt-BR.json
```

with the normal `MyProject` root namespace, the prefix will normally be:

```text
MyProject.Resources
```

Notify will look for resources such as:

```text
MyProject.Resources.notifications.en-US.json
MyProject.Resources.notifications.pt-BR.json
```

If your project uses a different root namespace, verify the actual manifest names rather than assuming the prefix.

You can inspect them with:

```csharp
var resources = typeof(Program)
    .Assembly
    .GetManifestResourceNames();

foreach (var resource in resources)
{
    Console.WriteLine(resource);
}
```

---

## 7. Configure the request culture

`JsonNotificationLocalizer` uses:

```csharp
CultureInfo.CurrentUICulture
```

It does **not** read HTTP headers itself.

For ASP.NET Core, configure the standard request localization middleware so `CurrentUICulture` is set according to your application's rules.

Example:

```csharp
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var supportedCultures = new[]
{
    new CultureInfo("pt-BR"),
    new CultureInfo("en-US")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

var app = builder.Build();

app.UseRequestLocalization(localizationOptions);
```

With this configuration, a request that resolves to `pt-BR` causes:

```csharp
CultureInfo.CurrentUICulture.Name
```

to be `pt-BR` and Notify will load `notifications.pt-BR.json`.

If your application uses a custom culture header or another culture-selection mechanism, configure that mechanism through ASP.NET Core's request localization system rather than putting HTTP concerns into Notify.

---

## 8. Add notifications

Without parameters:

```csharp
_notifications.AddNotification("PET_NOT_FOUND");
```

With a property:

```csharp
_notifications.AddNotification(
    "Name",
    "PET_NAME_REQUIRED");
```

With parameters:

```csharp
_notifications.AddNotification(
    "PET_CREATED",
    pet.Name);
```

The resulting notification contains both the stable key and the localized message:

```csharp
new Notification(
    "PET_CREATED",
    null,
    "Pet Rex criado com sucesso.");
```

---

## 9. Parameterized messages

JSON messages use standard .NET composite formatting placeholders.

```json
{
  "PET_CREATED": "Pet {0} criado com sucesso.",
  "PETS_FOUND": "Encontrados {0} pets."
}
```

Usage:

```csharp
_notifications.AddNotification("PET_CREATED", pet.Name);
_notifications.AddNotification("PETS_FOUND", pets.Count);
```

Formatting uses the current UI culture:

```csharp
string.Format(CultureInfo.CurrentUICulture, template, arguments);
```

This means numeric and date formatting can vary according to the selected culture.

---

## 10. Culture fallback

Notify attempts to resolve a message in this order:

1. Exact current UI culture, e.g. `pt-BR`
2. Language-only culture, e.g. `pt`
3. Configured default culture, e.g. `en-US`
4. The notification key itself if no resource exists

For example, with:

```text
CurrentUICulture = pt-BR
Default culture = en-US
```

Notify first checks:

```text
notifications.pt-BR.json
```

Then:

```text
notifications.pt.json
```

Then:

```text
notifications.en-US.json
```

If the key does not exist in any of those resources, the key itself becomes the message.

---

## 11. Returning notifications from an API

Notify intentionally does not decide HTTP status codes. The API layer should translate the notification context into the application's HTTP response convention.

For example, if your API standard is `422 Unprocessable Entity` for expected business validation:

```csharp
if (_notifications.HasNotifications)
{
    return UnprocessableEntity(_notifications.Notifications);
}
```

A response can then look like:

```json
[
  {
    "key": "PET_NAME_REQUIRED",
    "property": "Name",
    "message": "O nome do pet é obrigatório."
  }
]
```

For a larger application, this HTTP mapping is a good candidate for an ASP.NET Core filter or middleware so individual controllers do not need to repeat the check.

---

## 12. Recommended project organization

A typical application can keep its notification resources alongside the application that owns them:

```text
MyProject/
├── src/
│   ├── MyProject.Api/
│   ├── MyProject.Application/
│   │   └── Resources/
│   │       ├── notifications.en-US.json
│   │       └── notifications.pt-BR.json
│   ├── MyProject.Domain/
│   └── MyProject.Infrastructure/
└── tests/
```

If the application layer owns the notification messages, register the localizer against the application assembly instead:

```csharp
builder.Services.AddSingleton<INotificationLocalizer>(
    new JsonNotificationLocalizer(
        typeof(SomeApplicationService).Assembly,
        "MyProject.Application.Resources",
        "en-US"));
```

This keeps notification content owned by the service/application that actually defines the business rules.

---

## 13. Testing a consuming project

When testing a project that uses JSON localization, embed the test resources as well:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources/notifications.*.json">
    <WithCulture>false</WithCulture>
  </EmbeddedResource>
</ItemGroup>
```

Then instantiate the localizer using the test assembly:

```csharp
var localizer = new JsonNotificationLocalizer(
    typeof(JsonNotificationLocalizerTests).Assembly,
    "MyProject.Tests.Resources",
    "en-US");
```

A useful regression test is to verify that the expected resources exist in the manifest:

```csharp
var resources = typeof(JsonNotificationLocalizerTests)
    .Assembly
    .GetManifestResourceNames();

Assert.Contains(
    "MyProject.Tests.Resources.notifications.en-US.json",
    resources);
```

This catches the most common setup error: JSON files existing in the project but not actually being embedded in the assembly.

---

## 14. Complete minimal example

### `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Notify" Version="x.y.z" />
  </ItemGroup>

  <ItemGroup>
    <EmbeddedResource Include="Resources/notifications.*.json">
      <WithCulture>false</WithCulture>
    </EmbeddedResource>
  </ItemGroup>

</Project>
```

### `Resources/notifications.pt-BR.json`

```json
{
  "PET_NOT_FOUND": "Pet não encontrado.",
  "PET_NAME_REQUIRED": "O nome do pet é obrigatório."
}
```

### `Program.cs`

```csharp
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Notify.Contracts;
using Notify.Contexts;
using Notify.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<INotificationContext, NotificationContext>();

builder.Services.AddSingleton<INotificationLocalizer>(
    new JsonNotificationLocalizer(
        typeof(Program).Assembly,
        "MyProject.Resources",
        "en-US"));

var app = builder.Build();

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = [new CultureInfo("pt-BR"), new CultureInfo("en-US")],
    SupportedUICultures = [new CultureInfo("pt-BR"), new CultureInfo("en-US")]
};

app.UseRequestLocalization(localizationOptions);

app.Run();
```

### Application service

```csharp
public sealed class GetPetService
{
    private readonly INotificationContext _notifications;

    public GetPetService(INotificationContext notifications)
    {
        _notifications = notifications;
    }

    public Pet? Execute(Guid id)
    {
        var pet = FindPet(id);

        if (pet is null)
        {
            _notifications.AddNotification("PET_NOT_FOUND");
            return null;
        }

        return pet;
    }
}
```

---

## 15. Setup checklist

Before considering the integration complete, verify:

- [ ] `Notify` is referenced by the consuming project.
- [ ] `INotificationContext` is registered as scoped.
- [ ] JSON files are under the expected `Resources` directory.
- [ ] JSON files are configured as `EmbeddedResource`.
- [ ] `<WithCulture>false</WithCulture>` is present for culture-named JSON files.
- [ ] The `JsonNotificationLocalizer` receives the correct consuming assembly.
- [ ] The resource prefix matches the actual manifest resource names.
- [ ] A default culture is configured.
- [ ] ASP.NET Core request localization is configured if culture should come from the request.
- [ ] API-level notification handling maps notifications to the application's HTTP status convention.
- [ ] Tests verify that resources are embedded and localization works for the supported cultures.
