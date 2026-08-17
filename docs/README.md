# Vite.NET Documentation

The documentation site for [Vite.NET](https://github.com/techgems/Vite.NET), built with
[Astro](https://astro.build) and [Starlight](https://starlight.astro.build).

## 🧞 Commands

All commands are run from the root of this `Docs` folder:

| Command            | Action                                            |
| :----------------- | :------------------------------------------------ |
| `npm install`      | Installs dependencies                             |
| `npm run dev`      | Starts the local dev server at `localhost:4321`    |
| `npm run build`    | Builds the production site to `./dist/`           |
| `npm run preview`  | Previews the build locally, before deploying      |
| `npm run skill:zip`| Packages the Claude Skill into `public/downloads/` |

`dev`, `start`, and `build` run `skill:zip` first, so the download is always in sync with the
skill's source files.

## Structure

```
Docs/
├── public/                     # static assets (favicon)
│   └── downloads/              # generated: the Claude Skill zip (gitignored)
├── scripts/
│   └── build-skill-zip.mjs     # packages skill/vite-dotnet into public/downloads/
├── skill/
│   └── vite-dotnet/            # the Claude Skill source (SKILL.md + references/)
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
