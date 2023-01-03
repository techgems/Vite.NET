import { defineConfig } from 'vite'
import type { UserConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'




type PluginConfig = {
  port: number;
  appFolder: string;
  entrypoint: string;
  prodServerOrigin?: string; //Not for initial release. Use when hosting app files in a remote server such as S3 or Azure Blob.
}


const defaultPort = 5173;
const defaultAppFolder = "ClientApp";


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
          origin: mode === "development" ? `http://localhost:${config.port}` : `/${config.appFolder}`
        },
        hmr: {
          protocol: 'ws'
        },
        build: {
          outDir: `../wwwroot/${config.appFolder}`,
          emptyOutDir: true,
          manifest: true,
          rollupOptions: {
            // overwrite default .html entry
            input: config.entrypoint
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
