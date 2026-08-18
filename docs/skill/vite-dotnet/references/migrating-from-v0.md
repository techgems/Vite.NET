# Migrating from v0

v1 is a breaking release, but a small one: the v0 packages required per-app backend configuration
kept in sync with `vite.config.ts` by hand, and v1 removes it. The Vite plugin **emits manifest
files** describing each app, so the back end only needs the app's directory name — the entrypoint,
container id, dev-server port, and framework detection are no longer written anywhere by hand, and
there is nothing left to keep in sync.

A v0 project does need updating before it runs on v1, since the values the back end used to read
from configuration are no longer read from there.

The previous documentation remains at <https://tech-gems.gitbook.io/v0-vite-dotnet>.

## What has to change

1. **The `ViteDotNet` config section has a new shape** — a directory name or array of names, not a
   per-app settings object. A v0-style object is ignored rather than migrated.
2. **`ViteDotNetPlugin` takes two required arguments** — `entrypoint` and `containerElementId`,
   the values that used to live in backend config.
3. **Both packages must be upgraded together** — the manifests are the contract between them, so
   mixed versions fail in either direction.
4. **`IntegrationConfig` passed to a tag helper in a view was removed** — helpers are selected by
   `app-name` only.

### Diagnosing an unmigrated project

| Symptom | Cause |
| --- | --- |
| "Vite Dev Server Not Found" while the dev server is running | No `manifest.dev.json`: plugin registered without arguments, or the dev server hasn't restarted since the upgrade. |
| "Production Bundle not found" | No `manifest.prod.json`: not rebuilt since the upgrade, or the configured directory name doesn't match `wwwroot`. |
| The tag helper throws when rendering | The `ViteDotNet` section still holds a v0 config object, so no app directories were registered. |
| Entrypoint/container/port changes have no effect | Those values come from the manifests now, not `appsettings.json`. |

## What changed

| Area | Before (v0) | Now |
| --- | --- | --- |
| Backend config | Full per-app settings (entrypoint, container id, port, framework) in `appsettings.json` | Just the app **directory name(s)** under `ViteDotNet` |
| App identity | Configured explicitly | Derived from the SPA's folder automatically |
| Dev/prod metadata | Backend config | `manifest.dev.json` / `manifest.prod.json` in `wwwroot` |

## Upgrade steps

### 1. Update both packages to v1

NuGet is at `{{NUGET_VERSION}}` and npm at `{{NPM_VERSION}}`. Both are prereleases, so the version
must be explicit (or `--prerelease`) — otherwise NuGet keeps you on v0:

```bash
dotnet add package TechGems.ViteDotNet --version {{NUGET_VERSION}}
```

```bash
cd ReactApp && npm install vite-dotnet@{{NPM_VERSION}}
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
