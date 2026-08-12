import { writeFileSync, mkdirSync } from 'fs';
import { basename, join, posix, resolve } from 'path';
import { version as viteVersion } from 'vite';

export type PluginConfig = {
  entrypoint: string;
  containerElementId: string;
}

// Derive the SPA's folder name (e.g. "ReactApp") from the current working directory.
// Splits on both separators so it works on Windows and POSIX file systems.
function retrieveAppFolder(): string {
  const currentDir = process.cwd().split(/[\\/]/).pop();
  return currentDir!;
}

function outputOptions(assetsDir: string) {
  // Internal: Avoid nesting entrypoints unnecessarily.
  const outputFileName = (ext: string) => ({ name }: { name: string | undefined }) => {
    const shortName = basename(name!).split('.')[0];
    return posix.join(assetsDir, `${shortName}.[hash].${ext}`);
  };

  return {
    entryFileNames: outputFileName('js'),
    chunkFileNames: outputFileName('js'),
    assetFileNames: outputFileName('[ext]'),
  };
}

// Vite 8 renamed the deprecated `server.hmr.*` websocket options to `server.ws.*`. Emit
// whichever key is current for the running Vite version so a single codebase stays
// warning-free from Vite 3 through 8+. If the version can't be determined we fall back to
// `hmr`, which is understood by the widest range of versions.
// The protocol is forced to `ws` because the SPA is embedded in an ASP.NET page that may be
// served over https, which would otherwise make the HMR client attempt a failing wss upgrade.
function websocketServerOptions() {
  const major = parseInt((viteVersion ?? '').split('.')[0], 10);
  const options = { protocol: 'ws' as const };
  return major >= 8 ? { ws: options } : { hmr: options };
}

// The internal plugin names registered by @vitejs/plugin-react and @vitejs/plugin-react-swc.
const REACT_PLUGIN_NAMES = ['vite:react-babel', 'vite:react-refresh', 'vite:react-swc'];

// Determine if this is a React app based on the plugins it contains.
function isReactApp(plugins: readonly { name: string }[]): boolean {
  return plugins.some((plugin) => REACT_PLUGIN_NAMES.includes(plugin.name));
}

export default function ViteDotNetPlugin(entrypoint: string, containerElementId: string) {
  return ViteDotNet({ entrypoint, containerElementId });
}

function ViteDotNet(plugConfig: PluginConfig) {
  // Captured from the resolved config so it can be reused by both the dev-server and build hooks.
  let isReact = false;

  return {
    name: 'ViteDotNet',
    enforce: 'post' as const,
    configResolved(config: any) {
      // The resolved config exposes the full plugin list here (unlike generateBundle, whose
      // Rollup context does not), so React detection happens once and is reused everywhere.
      isReact = isReactApp(config.plugins);
    },
    config: () => {
      const currentDir = retrieveAppFolder();

      return {
        server: {
          ...websocketServerOptions()
        },
        build: {
          outDir: `../wwwroot`,
          emptyOutDir: true,
          manifest: `${currentDir}/manifest.json`,
          rollupOptions: {
            // overwrite default .html entry
            input: plugConfig.entrypoint,
            output: outputOptions(currentDir)
          }
        }
      };
    },
    configureServer(server: any) {
      const generateDevManifest = () => {
        const { config } = server;
        const port = config.server.port || 5173;

        const currentDir = retrieveAppFolder();

        const devManifest = {
          port,
          entrypoint: plugConfig.entrypoint,
          containerElementId: plugConfig.containerElementId,
          isReact
        };

        // Write the dev manifest into ../wwwroot/{appName} so it lives alongside the
        // production build output (outDir is ../wwwroot) rather than in the dist folder.
        const outputDir = resolve(process.cwd(), '..', 'wwwroot', currentDir);
        mkdirSync(outputDir, { recursive: true });

        writeFileSync(
          join(outputDir, 'manifest.dev.json'),
          JSON.stringify(devManifest, null, 2)
        );
      };

      // Generate the manifest once the server handles its first request.
      server.httpServer?.once('listening', generateDevManifest);
    },
    generateBundle(this: any) {
      // Emit the production integration metadata alongside Vite's own manifest.json so the
      // backend can read containerElementId/isReact without any hand-written config.
      const currentDir = retrieveAppFolder();

      const prodManifest = {
        entrypoint: plugConfig.entrypoint,
        containerElementId: plugConfig.containerElementId,
        isReact
      };

      this.emitFile({
        type: 'asset',
        fileName: `${currentDir}/manifest.prod.json`,
        source: JSON.stringify(prodManifest, null, 2)
      });
    }
  };
}
