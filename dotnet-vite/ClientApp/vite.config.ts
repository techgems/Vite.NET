import { defineConfig } from 'vite'
import type { UserConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import { basename, posix } from 'path'

type PluginConfig = {
  port: number;
  appFolder: string;
  entrypoint: string;
  prodServerOrigin?: string; //Not for initial release. Use when hosting app files in a remote server such as S3 or Azure Blob.
}

const defaultPort = 5173;
const defaultAppFolder = "ClientApp";

function outputOptions (assetsDir: string) {
  // Internal: Avoid nesting entrypoints unnecessarily.
  const outputFileName = (ext: string) => ({ name }: { name: string }) => {
    const shortName = basename(name).split('.')[0]
    return posix.join(assetsDir, `${shortName}.[hash].${ext}`)
  }

  return {
    entryFileNames: outputFileName('js'),
    chunkFileNames: outputFileName('js'),
    assetFileNames: outputFileName('[ext]'),
  }
}

function ViteDotNetPlugin(entrypoint: string) {

  return ViteDotNet({ port: defaultPort, appFolder: defaultAppFolder, entrypoint: entrypoint });
}

function ViteDotNet(config: PluginConfig) {
  return {
    name: 'ViteDotNet',
    enforce: "post" as const,
    config: (userConfig: UserConfig, { command, mode }) => {

      //https://vitejs.dev/config/server-options.html#server-origin

      return {
        server: {
          strictPort: true,
          origin: `http://localhost:${config.port}`,
          /*proxy:{
            '*' : {
              target: 'https://localhost:7167',
              changeOrigin: true
            }
          },*/
          hmr: {
            protocol: 'ws'
          }
        },
        build: {
          outDir: `../wwwroot`,
          emptyOutDir: false,
          manifest: `${config.appFolder}/manifest.json`,
          rollupOptions: {
            // overwrite default .html entry
            input: config.entrypoint,
            output: outputOptions(config.appFolder)
          }
        }
      }
    }
  };
};

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [svelte(), ViteDotNetPlugin("src/main.ts")]
})
