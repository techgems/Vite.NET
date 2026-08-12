# What is Vite.NET

Vite.NET is an ASP.NET Core library to support the integration of Vite SPAs and Razor Pages and MVC views. The purpose of this integration is to provide better authentication support, sharing of static assets and the possibility of creating micro-frontend like applications inside an ASP.NET Core app.

It is compatible with Vue, Svelte, React and Solid.

It's easier to think of this package as an analogue to existing solutions for other stacks, such as [Vite Ruby](https://vite-ruby.netlify.app/) or [Laravel Vite](https://github.com/laravel/vite-plugin).

For all these reasons, this package is not meant to work on it's own, it is instead a companion Vite Plugin for the C# ASP.NET Core library [Vite.NET](https://www.nuget.org/packages/TechGems.ViteDotNet).

For better information on how to fully integrate it, visit our [official documentation](https://vite-dotnet.techgems.net/) site.

## Plugin configuration

Add the plugin to your SPA's `vite.config.ts` alongside your framework plugin. It takes two arguments:

- `entrypoint` — the path to your app's entry module, relative to the SPA folder (e.g. `src/main.tsx`).
- `containerElementId` — the `id` of the DOM element the SPA mounts into (e.g. `root`).

```ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import ViteDotNetPlugin from 'vite-dotnet'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        react(),
        ViteDotNetPlugin("src/main.tsx", "root")
    ]
})
```

That's the entire configuration. The plugin derives everything else it needs automatically:

- The **app folder name** is taken from the current working directory (e.g. `ReactApp`). This becomes the key the ASP.NET Core library uses to look up your SPA, so no folder name has to be duplicated in backend config.
- The **build output** is written to `../wwwroot`, so the compiled assets land inside your ASP.NET Core project's static files directory.

> The plugin must be registered **after** your framework plugin (it uses `enforce: 'post'`), so React detection can see the framework plugin in the resolved config.

## Understanding the manifest files

Vite.NET's backend library needs a small amount of metadata about each SPA — its entrypoint, its container element id, and whether it's a React app — plus, in development, the dev server's port. The plugin produces this metadata as a JSON manifest, and it emits a different one depending on the mode.

### Development manifest (`manifest.dev.json`)

When you run `vite` (dev server), the plugin waits for the server to start listening and then writes a `manifest.dev.json` file to `../wwwroot/{appFolder}/` (e.g. `../wwwroot/ReactApp/manifest.dev.json`), so it sits alongside where the production build would go rather than in `dist`.

```json
{
  "port": 5173,
  "entrypoint": "src/main.tsx",
  "containerElementId": "root",
  "isReact": true
}
```

The backend reads this to know which port to proxy HMR/module requests to and how to inject the dev scripts into the Razor page.

### Production manifest (`manifest.prod.json`)

When you run `vite build`, the plugin emits `manifest.prod.json` into the build output at `{appFolder}/manifest.prod.json` (inside `../wwwroot`), right next to Vite's own hashed `manifest.json`.

```json
{
  "entrypoint": "src/main.tsx",
  "containerElementId": "root",
  "isReact": true
}
```

Production doesn't require a `port` — the backend serves the hashed assets directly. It combines this integration metadata with Vite's standard `manifest.json` (which maps entrypoints to their hashed output files) to render the correct `<script>` and `<link>` tags into the page.

Because both manifests carry `entrypoint`, `containerElementId`, and `isReact`, the ASP.NET Core side needs no hand-written configuration beyond pointing at the app folder — every value it needs is generated from your `vite.config.ts` at build time.
