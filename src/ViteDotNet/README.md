# Vite.NET

**Integrate Vite SPAs into ASP.NET Core Razor Pages and MVC — one build tool, shared authentication, and micro-frontends without leaving .NET.**

📖 **Full documentation: [vite-dotnet.techgems.net](https://vite-dotnet.techgems.net)**

Vite.NET bridges a Vite-powered front end (React, Svelte, Vue, Solid) and an ASP.NET Core back end. Your SPA lives inside your .NET app, so it inherits your authentication, layouts, and static-file pipeline — while Vite gives you a fast dev server, hot module replacement, and optimized production bundles.

## Why Vite.NET

- **One build tool** — Vite compiles your front end; ASP.NET Core serves it. No second server, no separate deployment.
- **Zero-duplication config** — the Vite plugin emits manifests describing each app, so the back end only needs the app's folder name. Everything else is generated.
- **Shared authentication** — because the SPA is served by your .NET app, it sits behind the same auth and cookies as the rest of your site.
- **Micro-frontends** — embed one SPA or many, each mounted into its own Razor page, all inside a single ASP.NET Core host.

## How it fits together

An integration has three moving parts:

1. **A Vite plugin** (`vite-dotnet`) added to your SPA's `vite.config.ts`. It configures the build output and emits a small JSON manifest describing the app.
2. **This NuGet package** (`TechGems.ViteDotNet`) registered in your ASP.NET Core app. It reads those manifests and exposes the tag helpers.
3. **Two tag helpers** — `<dev-vite-scripts>` and `<prod-vite-scripts>` — that render the correct `<script>`/`<link>` tags into a Razor page for development or production.

## Quick start

Register the integration in `Program.cs`:

```csharp
builder.Services.AddViteIntegration(builder.Configuration);
```

Add the tag helpers in `Pages/_ViewImports.cshtml`:

```razor
@addTagHelper *, ViteDotNet
```

Tell the back end your SPA folder name in `appsettings.json`:

```json
{
  "ViteDotNet": "ReactApp"
}
```

Add the plugin in `ReactApp/vite.config.ts`:

```ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import ViteDotNetPlugin from 'vite-dotnet'

export default defineConfig({
  plugins: [react(), ViteDotNetPlugin('src/main.tsx', 'root')],
})
```

Render the SPA in a Razor page — `<dev-vite-scripts>` in development, `<prod-vite-scripts>` in production:

```razor
<dev-vite-scripts app-name="ReactApp" />
```

With a single app configured you can omit `app-name` entirely. See the full [Quick Start](https://vite-dotnet.techgems.net/introduction/quick-start/) for the complete walkthrough, including the production build.

## Learn more

- [Overview](https://vite-dotnet.techgems.net/introduction/overview/) — the full picture
- [Installation](https://vite-dotnet.techgems.net/introduction/installation/) — packages and setup
- [How It Works](https://vite-dotnet.techgems.net/guides/how-it-works/) — manifests and tag helpers under the hood
- [Multiple SPAs](https://vite-dotnet.techgems.net/guides/multiple-spas/) — micro-frontends in one host
- [Reference](https://vite-dotnet.techgems.net/reference/vite-plugin/) — plugin, `AddViteIntegration`, tag helpers, and manifest files

## Links

- **Documentation:** https://vite-dotnet.techgems.net
- **GitHub:** https://github.com/techgems/vite-dotnet
- **License:** MIT
