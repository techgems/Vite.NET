import { defineConfig } from 'vite'
import type { UserConfig } from 'vite'
import react from '@vitejs/plugin-react'

type PluginConfig = {
  port: number;
  appFolder: string;
  entrypoint: string;
  prodServerOrigin?: string; //Not for initial release. Use when hosting app files in a remote server such as S3 or Azure Blob.
}

const defaultPort = 5174;
const defaultAppFolder = "ReactApp";


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
  plugins: [react(), ViteDotNetPlugin("src/main.tsx")],
})
