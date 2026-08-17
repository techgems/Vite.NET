# Vite.NET Documentation

The documentation site for [Vite.NET](https://github.com/techgems/Vite.NET), built with
[Astro](https://astro.build) and [Starlight](https://starlight.astro.build).

## 🧞 Commands

All commands are run from the root of this `Docs` folder:

| Command           | Action                                        |
| :---------------- | :-------------------------------------------- |
| `npm install`     | Installs dependencies                         |
| `npm run dev`     | Starts the local dev server at `localhost:4321` |
| `npm run build`   | Builds the production site to `./dist/`       |
| `npm run preview` | Previews the build locally, before deploying  |

## Structure

```
Docs/
├── public/                     # static assets (favicon)
├── src/
│   ├── assets/                 # images embedded in content
│   ├── components/             # Starlight component overrides
│   │   ├── DefaultDark.astro   # forces the dark theme
│   │   ├── DisableThemeColor.astro
│   │   └── HeaderLinks.astro   # adds the "v0 Docs" header link
│   ├── content/docs/           # the documentation pages (.md / .mdx)
│   └── content.config.ts
├── astro.config.mjs            # site config + sidebar
├── package.json
└── tsconfig.json
```

The header links out to the previous documentation site at
`https://tech-gems.gitbook.io/v0-vite-dotnet` via `src/components/HeaderLinks.astro`.
