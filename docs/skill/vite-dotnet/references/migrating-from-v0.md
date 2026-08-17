# Migrating from v0

The v0 packages required more configuration and made the developer keep it synchronized with
`vite.config.ts`. In the current release the Vite plugin **emits manifest files** describing each
app, so the back end only needs to know where the app is.

The previous documentation remains at <https://tech-gems.gitbook.io/v0-vite-dotnet>.

## What changed

| Area | Before (v0) | Now |
| --- | --- | --- |
| Backend config | Full per-app settings (entrypoint, container id, port, framework) in `appsettings.json` | Just the app **directory name(s)** under `ViteDotNet` |
| App identity | Configured explicitly | Derived from the SPA's folder automatically |
| Dev/prod metadata | Backend config | `manifest.dev.json` / `manifest.prod.json` in `wwwroot` |

## Upgrade steps

### 1. Update both packages to 1.0.0-beta

It is a prerelease, so the version must be explicit (or `--prerelease`) — otherwise NuGet keeps
you on v0:

```bash
dotnet add package TechGems.ViteDotNet --version 1.0.0-beta
```

```bash
cd ReactApp && npm install vite-dotnet@1.0.0-beta
```

### 2. Add the plugin arguments

Pass the entrypoint and container element id to the plugin, and register it **after** the
framework plugin:

```ts title="vite.config.ts"
export default defineConfig({
  plugins: [
    react(),
    ViteDotNetPlugin('src/main.tsx', 'root'),
  ],
})
```

### 3. Simplify the backend configuration

Replace the per-app configuration object with just the directory name(s):

```json title="appsettings.Development.json"
{ "ViteDotNet": "ReactApp" }
```

```json
{ "ViteDotNet": [ "ReactApp", "SvelteApp" ] }
```

### 4. Confirm the tag helpers

They are `<dev-vite-scripts>` and `<prod-vite-scripts>`, selected by app directory name via
`app-name` (optional with a single configured app):

```razor
<dev-vite-scripts app-name="ReactApp" />
```

Passing an `IntegrationConfig` object directly to the tag helpers in views is **no longer
supported** — the backend configuration only takes the app's location now.

### 5. Rebuild

Run `npm run dev` or `npm run build` so the plugin writes the new manifests into `wwwroot`; the
back end reads them from there. Delete stale build output from the old layout before the first
new build so old assets don't linger in `wwwroot`.
