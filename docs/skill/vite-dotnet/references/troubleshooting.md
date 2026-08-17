# Troubleshooting

The integration's failure modes are few, and each has a specific cause. Diagnose from the
manifests first: they are the contract between the two halves.

## "Vite Dev Server Not Found" on the page

`<dev-vite-scripts>` rendered but could not fetch the entrypoint from the dev server within its
retry window. Check, in order:

1. Is `npm run dev` running in the SPA folder? The message is expected when the page is opened
   first — start the dev server and refresh.
2. Does `wwwroot/{AppFolder}/manifest.dev.json` exist? It is written on the dev server's
   `listening` event. If it is missing, the plugin is not registered in `vite.config.ts`, or the
   dev server never bound its port.
3. Does the `port` in the manifest match the port Vite actually printed? The manifest records
   `config.server.port` (default `5173`).
4. With multiple SPAs, are two dev servers fighting over one port? Give each a distinct port.

## "Production Bundle not found"

`<prod-vite-scripts>` could not read `manifest.prod.json`. Either the SPA has not been built, or
the configured app directory name does not match the folder in `wwwroot`. Run `npm run build`
and confirm the `ViteDotNet` config value matches the SPA folder name exactly.

## The tag helper throws when rendering

- **Multiple apps configured, no `app-name`.** The helper cannot guess which SPA was meant —
  pass `app-name`. It is optional only when exactly one app is configured.
- **`app-name` doesn't match a configured directory.** Values are matched case-insensitively
  against the names in the `ViteDotNet` section.
- **Nothing configured.** A missing or empty `ViteDotNet` section registers no directories, and
  the helpers throw when used.

## The tags don't render at all (literal `<dev-vite-scripts>` in the HTML)

The tag helpers are not registered. Add to `_ViewImports.cshtml` in `Pages` (Razor Pages) or
`Views` (MVC):

```razor
@addTagHelper *, ViteDotNet
```

## The SPA mounts in development but not in production

- Is `app.UseStaticFiles()` in the pipeline? The production tags reference plain static files
  under `wwwroot`.
- Did the build actually run for the deployed artifact? Automate it with an MSBuild target
  `BeforeTargets="Publish"` (see `workflows.md`).
- Is the page still using `<dev-vite-scripts>`? Branch on `IWebHostEnvironment`.

## React HMR doesn't work / "Refresh runtime not loaded"

`isReact` is `false` in the manifest. The React preamble is only injected when the plugin detects
a React plugin (`vite:react-babel`, `vite:react-refresh`, `vite:react-swc`) in the resolved
config, which requires `ViteDotNetPlugin(...)` to be listed **after** `react()` in the `plugins`
array. Fix the order, restart the dev server, and confirm `"isReact": true` in
`manifest.dev.json`.

## HMR websocket fails on an HTTPS page

The plugin forces `{ protocol: 'ws' }` so an HTTPS host page doesn't push the client into a
failing `wss` upgrade. If a project's own Vite config overrides `server.ws` (Vite 8+) or
`server.hmr` (earlier versions), that override wins — remove it or set the protocol back to
`ws`.

## Images or assets 404 in production

The build `base` is `/{AppFolder}/`, so Vite rewrites the URLs it resolves — imported assets,
`url()` in CSS, dynamic import chunks. Paths hard-coded as strings in markup or JSX are not
rewritten: `<img src="/logo.svg">` must become `/{AppFolder}/logo.svg`, or the asset should be
imported so Vite handles the URL.

## One app's build wipes another's assets

Each app must build into its own folder. The plugin derives that folder from the SPA's working
directory name, and `emptyOutDir` then only clears that app's output. Two SPAs in folders with
the same name — or a hand-written `build.outDir` pointing both at the same place — collide.

## Config values seem to be ignored

Entrypoint, container element id, port, and the React flag are **not** read from
`appsettings.json` in this version; they come from the manifests. Leftover v0-style
configuration objects are ignored. Change them in `vite.config.ts` and re-run the dev server or
build so the manifests are rewritten — see `migrating-from-v0.md`.
