# Using the integration without the npm package

For SPAs with a specialized config, the `vite-dotnet` npm package can be replaced with a couple
of helper functions in the project. The integration's whole job is to emit the build into
`wwwroot/{app}` and write `manifest.dev.json` / `manifest.prod.json` next to it — that is
expressible directly in a `vite.config.ts`.

This is optional; for most projects the npm package is simpler. Owning this code means
maintaining it as Vite.NET evolves, though it rarely changes. The manifests produced are
identical to the package's, so the back end is unchanged.

## 1. Manifest helpers

Add next to `vite.config.ts`:

```ts title="ReactApp/vite-dotnet.ts"
import { writeFileSync, mkdirSync } from 'node:fs'
import { join, resolve } from 'node:path'

export type IntegrationMeta = {
  entrypoint: string
  containerElementId: string
  isReact: boolean
}

export function appFolder(): string {
  return process.cwd().split(/[\\/]/).pop()!
}

export function writeDevManifest(meta: IntegrationMeta): void {
  writeManifest('manifest.dev.json', { ...meta })
}

export function writeProdManifest(meta: IntegrationMeta): void {
  writeManifest('manifest.prod.json', meta)
}

function writeManifest(fileName: string, data: unknown): void {
  const dir = resolve(process.cwd(), '..', 'wwwroot', appFolder())
  mkdirSync(dir, { recursive: true })
  writeFileSync(join(dir, fileName), JSON.stringify(data, null, 2))
}
```

The plain-JavaScript equivalent (`vite-dotnet.js`) is the same file with the types removed.

## 2. The Vite config

Send the build into `wwwroot/{app}`, force plain-`ws` HMR (the SPA is embedded in an ASP.NET page
that may be served over https), write the dev manifest when serving, and write the prod manifest
after the build:

```ts title="ReactApp/vite.config.ts"
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { appFolder, writeDevManifest, writeProdManifest, type IntegrationMeta } from './vite-dotnet.ts'

const meta: IntegrationMeta = { entrypoint: 'src/main.tsx', containerElementId: 'root', isReact: true }

export default defineConfig(({ command }) => {
  if (command === 'serve') writeDevManifest(meta)

  const dir = appFolder()

  return {
    plugins: [
      react(),
      // emptyOutDir clears wwwroot/{app} during the build, so the prod manifest is written afterwards.
      { name: 'vite-dotnet-prod-manifest', apply: 'build', closeBundle: () => writeProdManifest(meta) },
    ],
    server: { ws: { protocol: 'ws' } },
    build: {
      outDir: `../wwwroot/${dir}`,
      emptyOutDir: true,
      manifest: 'manifest.json',
      rollupOptions: {
        input: meta.entrypoint,
        output: {
          entryFileNames: '[name].[hash].js',
          chunkFileNames: '[name].[hash].js',
          assetFileNames: '[name].[hash].[ext]',
        },
      },
    },
  }
})
```

Notes and differences from the packaged plugin:

- `isReact` is stated by hand here rather than detected from the resolved plugin list.
- `server.ws` is the Vite 8+ key; on Vite 7 and earlier use `server.hmr: { protocol: 'ws' }`.
- The prod manifest is written from `closeBundle` because `emptyOutDir` clears the folder during
  the build.
- Add `base: '/{app}/'` for builds if the app resolves URLs at runtime (dynamic imports,
  `public/` files, `url()` in CSS), matching what the plugin does.

## 3. TypeScript notes

The helpers use Node's `fs` and `path`, so the Node types are needed:

```bash
npm install -D @types/node
```

Importing the helpers with the `.ts` extension requires `allowImportingTsExtensions` in the
tsconfig — the tsconfig current Vite scaffolds generate already enables it.

Everything downstream is unchanged: the same `ViteDotNet` backend configuration and the same
`<dev-vite-scripts>` / `<prod-vite-scripts>` tag helpers.
