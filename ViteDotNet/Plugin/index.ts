import type { UserConfig } from 'vite';
import { basename, posix } from 'path';

export type PluginConfig = {
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

export function ViteDotNetPlugin(entrypoint: string) {

  return ViteDotNet({ port: defaultPort, appFolder: defaultAppFolder, entrypoint: entrypoint });
}

export default function ViteDotNet(config: PluginConfig) {
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
