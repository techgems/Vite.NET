# Workflows

Development, production builds, and hosting several SPAs in one .NET app.

`{AppFolder}` (and `{OtherAppFolder}`, where two apps are shown) stands for a SPA's **actual
directory name in the project you are working on** — read it from the `ViteDotNet` configuration
section or the folder layout and substitute it everywhere below. Never run a command with the
literal placeholder, and never assume a name like `ReactApp`.

## Development

Two processes run side by side: the Vite dev server (serving SPA modules with HMR) and the
ASP.NET Core app (serving the Razor page that hosts the SPA). `<dev-vite-scripts>` stitches them
together.

```bash
# terminal 1
cd {AppFolder}
npm run dev
```

When the server begins listening, the plugin writes `wwwroot/{AppFolder}/manifest.dev.json`
with the port, entrypoint, container id, and React flag.

```bash
# terminal 2
dotnet run
```

Open the page: `<dev-vite-scripts>` reads the manifest and loads the Vite client and entrypoint
from `http://localhost:{port}`, giving HMR inside the Razor page.

Loading the page before the dev server is listening renders a **"Vite Dev Server Not Found"**
message and retries for a short window. Start `npm run dev` and refresh — this is expected
behavior, not a broken setup.

### Launching the dev server from .NET (optional)

To avoid the second terminal, have ASP.NET Core start the Vite dev server with the
`RunViteDevServer` extension (namespace `ViteDotNet.NPM`):

```csharp title="Program.cs"
using ViteDotNet.NPM;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.RunViteDevServer();
}
```

Nothing is passed: the SPA folder comes from the `ViteDotNet` configuration, and the port is
whatever the dev server binds to — the plugin records it in `manifest.dev.json`.

With more than one app configured, name the one to launch, calling it once per app:

```csharp
app.RunViteDevServer("{AppFolder}");
app.RunViteDevServer("{OtherAppFolder}");
```

Each call runs that app's `dev` npm script in its SPA folder. The command starts once the app is
up and listening, so a single `dotnet run` brings up both processes. The call returns immediately
— there is no `Task` to await — and the dev server is terminated together with the app on
shutdown.

This is a development-only convenience. Never call `RunViteDevServer` in production: production
serves built assets from `wwwroot` through `<prod-vite-scripts>`, with no dev server involved.

## Production builds

```bash
cd {AppFolder}
npm run build
```

```razor title="Pages/Index.cshtml"
<prod-vite-scripts app-name="{AppFolder}" />
```

A build of `{AppFolder}` produces roughly:

```
wwwroot/
└── {AppFolder}/
    ├── main.[hash].js        the hashed JS bundle
    ├── main.[hash].css       the hashed CSS (if any)
    ├── manifest.json         Vite's manifest (source → hashed output)
    └── manifest.prod.json    the plugin's integration metadata
```

- `manifest.json` — Vite's standard build manifest; maps the entrypoint to its hashed output
  files and lists the CSS belonging to the entry chunk.
- `manifest.prod.json` — the plugin's integration metadata: entrypoint, container element id,
  React flag.

`<prod-vite-scripts>` reads both. Because the assets live in `wwwroot`, they are served by
ASP.NET Core's static-files middleware — `app.MapStaticAssets()` or `app.UseStaticFiles()` must be
in the pipeline. Either works for the library, but one of them always has to be called.

### Files in `public/`

Anything in the app's `public/` folder is copied into the same app folder, so it is served from
`/{AppFolder}/…` rather than the site root. The plugin sets the build `base` to `/{AppFolder}/`,
so references Vite resolves — imported assets, `url()` in CSS, dynamic import chunks — are
rewritten automatically. A path hard-coded as a string in markup or JSX (e.g.
`<img src="/logo.svg">`) is **not** rewritten: write `/{AppFolder}/logo.svg`, or import the asset
so Vite handles the URL.

## Multiple SPAs

One ASP.NET Core host can serve several integrated SPAs, each mounted into its own Razor page —
a micro-frontend architecture without a Node micro-service mesh. Every app is built by Vite into
`wwwroot` and served by the same .NET host, behind the same authentication.

```
YourApp/
├── {AppFolder}/            a React SPA, say
│   └── vite.config.ts      ViteDotNetPlugin('src/main.tsx', 'root')
├── {OtherAppFolder}/       a Svelte SPA, say
│   └── vite.config.ts      ViteDotNetPlugin('src/main.ts', 'app')
└── wwwroot/
    ├── {AppFolder}/        its build + manifests
    └── {OtherAppFolder}/   its build + manifests
```

Configuration lists every folder name:

```json title="appsettings.Development.json"
{ "ViteDotNet": [ "{AppFolder}", "{OtherAppFolder}" ] }
```

```csharp title="Program.cs"
builder.Services.AddViteIntegration(new[] { "{AppFolder}", "{OtherAppFolder}" });
```

With more than one app configured, `app-name` is **required** — omitting it throws. Each page
renders one app:

```razor title="Pages/FirstApp.cshtml"
<dev-vite-scripts app-name="{AppFolder}" />
```

```razor title="Pages/SecondApp.cshtml"
<dev-vite-scripts app-name="{OtherAppFolder}" />
```

In development each SPA runs its own dev server, each writing its own `manifest.dev.json` under
its own folder in `wwwroot/`:

```bash
cd {AppFolder} && npm run dev        # terminal 1
cd {OtherAppFolder} && npm run dev   # terminal 2
```

Give each dev server a distinct port (via its Vite config or its `dev` script) so they don't
collide. The manifests record whichever port each server actually bound to, and the tag helpers
load from the right one.

Build each app separately; because they are isolated by folder they never clash, so frameworks
can be mixed — React on one page, Svelte on another — in the same host:

```bash
cd {AppFolder} && npm run build
cd ../{OtherAppFolder} && npm run build
```
