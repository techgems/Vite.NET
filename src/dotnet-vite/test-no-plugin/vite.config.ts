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
      { name: 'vite-dotnet-prod-manifest', apply: 'build', closeBundle: () => writeProdManifest(meta) },
    ],
    server: {
      ws: { protocol: 'ws' }
    },
    build: {
      outDir: `../wwwroot/${dir}`,
      emptyOutDir: true,
      manifest: `manifest.json`,
      rollupOptions: {
        input: meta.entrypoint,
        output: {
          entryFileNames: `[name].[hash].js`,
          chunkFileNames: `[name].[hash].js`,
          assetFileNames: `[name].[hash].[ext]`,
        },
      },
    },
  }
})
