---
name: vite-dotnet
description: Integrate Vite SPAs (React, Svelte, Vue, Solid) into ASP.NET Core Razor Pages or MVC with Vite.NET — the TechGems.ViteDotNet NuGet package and the vite-dotnet Vite plugin. Use when installing or configuring the integration, wiring ViteDotNetPlugin or AddViteIntegration, rendering a SPA with the dev-vite-scripts / prod-vite-scripts tag helpers, working with manifest.dev.json or manifest.prod.json, hosting multiple SPAs in one .NET app, running the dev server alongside dotnet run, building for production, or migrating from Vite.NET v0.
license: MIT
---

# Vite.NET

Vite.NET hosts a Vite SPA **inside** an ASP.NET Core app: Vite builds the front end into
`wwwroot`, and Razor Pages/MVC serves it behind the same auth, cookies, and static-file
pipeline as the rest of the site. It is the .NET analogue to Vite Ruby or the Laravel Vite
plugin, and works with React, Svelte, Vue, and Solid.

## The core idea: the front end describes itself

The back end is **not** told the entrypoint, mount element, dev-server port, or framework.
The Vite plugin writes those into manifest files in `wwwroot`, and the back end reads them.
The only backend configuration is the SPA's **directory name**.

```
ReactApp/vite.config.ts                    appsettings: "ViteDotNet": "ReactApp"
        │                                            │
        │  ViteDotNetPlugin('src/main.tsx', 'root')  │  reads manifests from
        ▼                                            ▼  wwwroot/ReactApp/
  manifest.dev.json   ──────────────────────►  <dev-vite-scripts>    (development)
  manifest.prod.json  ──────────────────────►  <prod-vite-scripts>   (production)
```

Never reintroduce entrypoint/container-id/port/framework values into `appsettings.json` —
that is the v0 design and it no longer works.

## The two packages

| Package | Installed in | Purpose |
| --- | --- | --- |
| `TechGems.ViteDotNet` (NuGet) | the ASP.NET Core project | `AddViteIntegration` + the tag helpers |
| `vite-dotnet` (npm) | the SPA folder inside that project | Vite plugin: shapes the build, emits manifests |

Current release: **1.0.0-beta**. It is a prerelease, so the version must be explicit in both
package managers — a plain `dotnet add package` / `npm install` will skip it.

## Minimal working setup

The SPA folder lives **inside** the ASP.NET Core project (`YourApp/ReactApp/`), and its build
output goes to `YourApp/wwwroot/ReactApp/`.

1. **NuGet package + registration**

   ```bash
   dotnet add package TechGems.ViteDotNet --version 1.0.0-beta
   ```

   ```csharp title="Program.cs"
   using ViteDotNet;

   builder.Services.AddRazorPages();
   builder.Services.AddViteIntegration(builder.Configuration);
   ```

2. **Tag helpers** in `Pages/_ViewImports.cshtml` (or `Views/_ViewImports.cshtml` for MVC):

   ```razor
   @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
   @addTagHelper *, ViteDotNet
   ```

3. **Vite plugin**, from inside the SPA folder:

   ```bash
   npm install vite-dotnet@1.0.0-beta
   ```

   ```ts title="ReactApp/vite.config.ts"
   import { defineConfig } from 'vite'
   import react from '@vitejs/plugin-react'
   import ViteDotNetPlugin from 'vite-dotnet'

   export default defineConfig({
     plugins: [
       react(),
       ViteDotNetPlugin('src/main.tsx', 'root'),   // entrypoint, container element id
     ],
   })
   ```

   **The plugin must come after the framework plugin** (`react()`, `svelte()`, `vue()`). It uses
   `enforce: 'post'` and detects React by inspecting the resolved plugin list.

4. **Backend config** — the app's directory name, nothing else:

   ```json title="appsettings.Development.json"
   { "ViteDotNet": "ReactApp" }
   ```

5. **Render it** in a Razor page:

   ```razor title="Pages/Index.cshtml"
   <dev-vite-scripts app-name="ReactApp" />
   ```

   `app-name` may be omitted when exactly one app is configured; it is **required** with more
   than one.

6. **Run both processes**: `npm run dev` in the SPA folder (writes `manifest.dev.json`) and
   `dotnet run` for the .NET app. For production, `npm run build` and swap the page to
   `<prod-vite-scripts app-name="ReactApp" />`.

Framework entrypoint/container defaults: React `('src/main.tsx', 'root')`, Svelte
`('src/main.ts', 'app')`, Vue `('src/main.ts', 'app')`.

## Choosing the helper per environment

```razor
@inject IWebHostEnvironment Env

@if (Env.IsDevelopment())
{
    <dev-vite-scripts app-name="ReactApp" />
}
else
{
    <prod-vite-scripts app-name="ReactApp" />
}
```

Both helpers render only the mount `<div>` plus scripts/styles — not a full HTML document.

## Reference files

Read the relevant file before answering in depth; do not guess at APIs.

| File | Covers |
| --- | --- |
| `references/setup.md` | Full install walkthrough, project layout, per-framework configs |
| `references/plugin-and-manifests.md` | `ViteDotNetPlugin` reference, everything it configures/derives, both manifest schemas |
| `references/backend-api.md` | `AddViteIntegration` overloads, the `ViteDotNet` config section, both tag helpers in detail |
| `references/workflows.md` | Dev workflow + `RunViteDevServer`, production builds, publish automation, multiple SPAs |
| `references/without-npm.md` | Replacing the npm plugin with your own `vite.config.ts` helpers |
| `references/migrating-from-v0.md` | What changed from v0 and the upgrade steps |
| `references/troubleshooting.md` | The built-in error messages and what actually causes them |

## Rules of thumb

- Keep `entrypoint` and `containerElementId` in `vite.config.ts` only — one source of truth.
- One app per folder; each builds into its own `wwwroot/{AppFolder}/`, so several SPAs coexist.
- `app.UseStaticFiles()` must be in the pipeline — production assets are plain static files.
- Hard-coded asset URLs in markup/JSX are not rewritten by Vite: write `/{AppFolder}/logo.svg`
  or import the asset.
- `RunViteDevServer()` is development-only; never call it in production.

## Documentation

Full docs: <https://vite-dotnet.techgems.net>. Source: <https://github.com/techgems/vite-dotnet>.
