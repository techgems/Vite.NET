# Backend API

The ASP.NET Core half: `AddViteIntegration`, the `ViteDotNet` configuration section, and the two
tag helpers.

## The `ViteDotNet` configuration section

The back end's configuration is the **directory name(s)** of the integrated Vite app(s). It
accepts either a single string or an array:

```json title="appsettings.Development.json"
{ "ViteDotNet": "ReactApp" }
```

```json title="appsettings.Development.json"
{ "ViteDotNet": [ "ReactApp", "SvelteApp" ] }
```

The values must match the SPA folder names — the same names the plugin derives from each app's
working directory, and the folders the manifests are written under in `wwwroot`.

If the section is missing or empty, no directories are registered and the tag helpers throw when
used.

### What is no longer configured

These came from backend config in v0 and now come from the manifests instead: the entrypoint,
the container element id, the dev-server port, and whether the app is React.

## `AddViteIntegration`

```csharp
using ViteDotNet;
```

Call once in `Program.cs`. Three overloads:

```csharp
public static void AddViteIntegration(this IServiceCollection services, IConfiguration configuration)
public static void AddViteIntegration(this IServiceCollection services, string appDirectory)
public static void AddViteIntegration(this IServiceCollection services, IEnumerable<string> appDirectories)
```

```csharp title="Program.cs"
// From the ViteDotNet configuration section
builder.Services.AddViteIntegration(builder.Configuration);

// A single directory, bypassing configuration
builder.Services.AddViteIntegration("ReactApp");

// Several directories
builder.Services.AddViteIntegration(new[] { "ReactApp", "SvelteApp" });
```

It registers the services the tag helpers depend on, all singletons:

- `IViteConfigService` — resolves which app directory a tag helper should use.
- `IManifestExtractor` — reads Vite's production `manifest.json`.
- `IDevManifestExtractor` / `IDevManifestCache` — read `manifest.dev.json`.
- `IProdManifestExtractor` / `IProdManifestCache` — read `manifest.prod.json`.

These are consumed by the tag helpers; application code does not interact with them directly.

## Registering the tag helpers

The tag helpers live in the `ViteDotNet` assembly. Register them in `_ViewImports.cshtml` — the
`Pages` folder for Razor Pages, `Views` for MVC:

```razor
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, ViteDotNet
```

This makes `<dev-vite-scripts>` and `<prod-vite-scripts>` available.

## Shared attribute

| Attribute | Type | Required | Meaning |
| --- | --- | --- | --- |
| `app-name` | `string` | Only when more than one app is configured | The SPA directory name to render. |

With exactly one configured app the attribute can be dropped entirely: `<dev-vite-scripts />`.
With several apps, omitting it throws — the helper cannot guess which SPA was meant. The value
is matched against the configured directory names case-insensitively.

Both helpers render only the SPA's mount point and its scripts — not a full HTML document. Put
them inside a layout, or in a `Layout = null` page with your own `<html>` / `<head>`.

## `<dev-vite-scripts>` — development

```razor
<dev-vite-scripts app-name="ReactApp" />
```

Reads `manifest.dev.json` and renders:

- For React apps (`isReact: true`), a `<script static-script type="module">` block injecting the
  React refresh runtime preamble.
- The container element `<div id="{containerElementId}"></div>`.
- A loader script that fetches the entrypoint from `http://localhost:{port}/{entrypoint}` and,
  once it responds, appends the Vite client (`/@vite/client`) and the entrypoint as module
  scripts — giving hot module replacement inside the Razor page.
- A fallback: if the entrypoint can't be fetched within the retry window, the container contents
  are replaced with a **"Vite Dev Server Not Found"** message.

Resolved values (defaults apply when the manifest is absent because the dev server hasn't
started):

| Property | Source | Default |
| --- | --- | --- |
| `Port` | `manifest.dev.json` → `port` | `5173` |
| `Entrypoint` | `manifest.dev.json` → `entrypoint` | `""` |
| `ContainerElementId` | `manifest.dev.json` → `containerElementId` | `"app"` |
| `IsReact` | `manifest.dev.json` → `isReact` | `false` |

`AppDirectory` is resolved from `app-name` (or the single configured app) via
`IViteConfigService`.

Requirements: the integration registered, the tag helpers registered in `_ViewImports.cshtml`,
and the Vite dev server running so `manifest.dev.json` exists and the entrypoint is reachable.

## `<prod-vite-scripts>` — production

```razor
<prod-vite-scripts app-name="ReactApp" />
```

Reads Vite's `manifest.json` for hashed asset paths and `manifest.prod.json` for the container
id, then emits:

- One `<link rel="stylesheet" href="/{css}" />` per CSS file in the entry chunk.
- `<script type="module" src="/{jsBundle}"></script>` for the hashed JS bundle.
- The container element `<div id="{containerElementId}"></div>`.

For a built `ReactApp` that is roughly:

```html
<link rel="stylesheet" href="/ReactApp/main.[hash].css" />
<script type="module" src="/ReactApp/main.[hash].js"></script>
<div id="root"></div>
```

When the production manifest is missing it renders a **"Production Bundle not found"** message
instead.

| Property | Source |
| --- | --- |
| `AppManifest` | Vite's `manifest.json` — the entry chunk, providing `file` (hashed JS) and `css` (hashed stylesheets). |
| `ProdManifest` | The plugin's `manifest.prod.json` — integration metadata. |
| `ContainerElementId` | `manifest.prod.json` → `containerElementId` (default `"app"`). |

Requirements: the integration and tag helpers registered, the SPA built (`npm run build`), and
`app.UseStaticFiles()` in the pipeline — the emitted tags reference assets served from `wwwroot`.

## Switching per environment

```razor title="Pages/Index.cshtml"
@page
@model YourApp.Pages.IndexModel
@inject IWebHostEnvironment Env
@{
    Layout = null;
}

@if (Env.IsDevelopment())
{
    <dev-vite-scripts app-name="ReactApp" />
}
else
{
    <prod-vite-scripts app-name="ReactApp" />
}
```
