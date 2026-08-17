# The Vite plugin and the manifests

`vite-dotnet` is the front-end half of the integration. It shapes the build output for ASP.NET
Core, detects whether the app is React, and emits the manifests the back end reads.

Supported Vite versions: **3 through 8+**.

## `ViteDotNetPlugin`

```ts
import ViteDotNetPlugin from 'vite-dotnet'

function ViteDotNetPlugin(
  entrypoint: string,
  containerElementId: string,
): Plugin
```

| Parameter | Type | Description |
| --- | --- | --- |
| `entrypoint` | `string` | Path to the app's entry module, relative to the SPA folder (e.g. `src/main.tsx`). |
| `containerElementId` | `string` | The `id` of the DOM element the SPA mounts into (e.g. `root`). |

Returns a Vite `Plugin` with `enforce: 'post'`, so it must be registered **after** the framework
plugin.

```ts
export type PluginConfig = {
  entrypoint: string
  containerElementId: string
}
```

## What it configures

The plugin merges this into the Vite config:

| Config | Value | Why |
| --- | --- | --- |
| `build.outDir` | `../wwwroot/{appFolder}` | Each app gets its own folder inside the ASP.NET Core static-files directory. |
| `build.emptyOutDir` | `true` | Clean only this app's output folder, leaving other apps' assets alone. |
| `base` (build only) | `/{appFolder}/` | URLs the bundle resolves at runtime — dynamic import chunks, files copied from `public/`, `url()` in CSS — point at the app folder. Not applied in dev, where the dev server serves from its own root. |
| `build.manifest` | `manifest.json` | Vite's manifest, at the root of the app's output folder. |
| `build.rollupOptions.input` | `entrypoint` | Build from the entry module instead of an `index.html`. |
| `build.rollupOptions.output` | `main.[hash].js` etc. | Flat names — the app folder is already the output directory. |
| `server.ws` / `server.hmr` | `{ protocol: 'ws' }` | Force plain-`ws` HMR so embedding in an HTTPS page doesn't break the socket. Vite 8+ uses `server.ws`; earlier versions use `server.hmr`, and the plugin picks the right key for the running version. |

## Derived values

| Value | How it's derived |
| --- | --- |
| App folder name | The last segment of the current working directory (e.g. `ReactApp`), split on both `/` and `\`. |
| `isReact` | `true` when the resolved plugins include `vite:react-babel`, `vite:react-refresh`, or `vite:react-swc`. Captured during `configResolved`, because the Rollup context in `generateBundle` cannot see the plugin list. |

The app folder name is the value that must match the `ViteDotNet` backend configuration.

## Emitted files

| File | When | Location | Contents |
| --- | --- | --- | --- |
| `manifest.dev.json` | Dev server's `listening` event | `wwwroot/{appFolder}/` | `port`, `entrypoint`, `containerElementId`, `isReact` |
| `manifest.prod.json` | `vite build` (`generateBundle`) | `wwwroot/{appFolder}/` | `entrypoint`, `containerElementId`, `isReact` |

Both live next to the production build output, so a single folder holds everything the back end
needs.

## Manifest schemas

| Field | Type | Manifest | Description |
| --- | --- | --- | --- |
| `entrypoint` | `string` | both | The entry module passed to `ViteDotNetPlugin`. |
| `containerElementId` | `string` | both | The mount element id passed to `ViteDotNetPlugin`. |
| `isReact` | `boolean` | both | Whether a React plugin was detected in the resolved config. |
| `port` | `number` | dev only | The port the dev server bound to (`config.server.port`, default `5173`). |

### `manifest.dev.json`

Written during the dev server's `listening` event — once `npm run dev` has started and the
server has bound its port. Load the hosting page before that and the dev tag helper falls back
to its defaults and retries.

```json
{
  "port": 5173,
  "entrypoint": "src/main.tsx",
  "containerElementId": "root",
  "isReact": true
}
```

Back-end model: `record DevManifestModel(int Port, string Entrypoint, string ContainerElementId, bool IsReact)`.

### `manifest.prod.json`

Emitted from `generateBundle` during `vite build`, alongside Vite's own `manifest.json`.
Production needs no port — the assets are served statically.

```json
{
  "entrypoint": "src/main.tsx",
  "containerElementId": "root",
  "isReact": true
}
```

Back-end model: `record ProdManifestModel(string Entrypoint, string ContainerElementId, bool IsReact)`.

### Pairing with Vite's `manifest.json`

`manifest.prod.json` carries no hashed asset paths. Those live in Vite's standard build manifest
at `wwwroot/{appFolder}/manifest.json`, which maps the entrypoint to its output `file` and
associated `css`. `<prod-vite-scripts>` reads both:

- `manifest.json` → hashed JS and CSS paths for the `<script>` / `<link>` tags.
- `manifest.prod.json` → the `containerElementId` for the mount `<div>`.

## Why output goes into `wwwroot`

Because `build.outDir` is `../wwwroot/{appFolder}`, compiled assets land inside the ASP.NET Core
project's static-files directory — that is what lets .NET serve the SPA directly, with no
separate Node server and no proxy. Per-app folders mean `emptyOutDir` only clears that app's
assets, so several SPAs share one `wwwroot` without overwriting each other.
