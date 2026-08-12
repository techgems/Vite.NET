import { defineConfig } from 'vite'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'
import { writeFileSync, mkdirSync } from 'fs';
import { resolve, join } from 'path';
//import ViteDotNet from 'vite-dotnet'

import { basename } from 'path';

export type PluginConfig = {
    entrypoint: string;
    containerElementId: string;
}

function retrieveAppFolder(): string {
    const fullCurrentDir = process.cwd();
    const currentDir = fullCurrentDir.split('\\').pop();

    return currentDir!;
}

function outputOptions() {
    // Internal: Avoid nesting entrypoints unnecessarily. The bundle already lands in the app's own
    // folder (build.outDir is ../wwwroot/{appFolder}), so file names carry no directory prefix.
    const outputFileName = (ext: string) => ({ name }: { name: string | undefined }) => {
        const shortName = basename(name!).split('.')[0]
        return `${shortName}.[hash].${ext}`
    }

    return {
        entryFileNames: outputFileName('js'),
        chunkFileNames: outputFileName('js'),
        assetFileNames: outputFileName('[ext]'),
    }
}

// The internal plugin names registered by @vitejs/plugin-react and @vitejs/plugin-react-swc.
const REACT_PLUGIN_NAMES = ['vite:react-babel', 'vite:react-refresh', 'vite:react-swc'];

// Determine if this is a React app based on the plugins it contains.
function isReactApp(plugins: readonly { name: string }[]): boolean {
    const reactPlugins = plugins.filter((plugin) => REACT_PLUGIN_NAMES.includes(plugin.name));
    return reactPlugins.length > 0;
}

function ViteDotNetPlugin(entrypoint: string, containerElementId: string, ) {
    return ViteDotNet({ containerElementId, entrypoint });
}

function ViteDotNet(plugConfig: PluginConfig) {
    // Captured from the resolved config so it can be reused by both the dev-server and build hooks.
    let isReact = false;

    return {
        name: 'ViteDotNet',
        enforce: "post" as const,
        configResolved(config: any) {
            // The resolved config exposes the full plugin list here (unlike generateBundle, whose
            // Rollup context does not), so React detection happens once and is reused everywhere.
            isReact = isReactApp(config.plugins);
        },
        config: (_userConfig: any, env: { command: string }) => {
            const currentDir = retrieveAppFolder();

            return {
                // Each app owns its own output folder, so `emptyOutDir` only clears that app's assets.
                // The build `base` points runtime-resolved urls (dynamic import chunks, public/ files,
                // css url() references) at /{appFolder}/.
                ...(env.command === 'build' ? { base: `/${currentDir}/` } : {}),
                server: {
                    /*proxy:{
                      '*' : {
                        target: 'https://localhost:7167',
                        changeOrigin: true
                      }
                    },*/
                    ws: {
                        protocol: 'ws'
                    }
                },
                build: {
                    outDir: `../wwwroot/${currentDir}`,
                    emptyOutDir: true,
                    manifest: `manifest.json`,
                    rollupOptions: {
                        // overwrite default .html entry
                        input: plugConfig.entrypoint,
                        output: outputOptions()
                    }
                }
            };
        },
        configureServer(server: any) {
            const generateDevManifest = () => {
                const { config } = server;
                // Determine the correct local dev server URL
                const port = config.server.port || 5173;

                //console.log(config);
                const currentDir = retrieveAppFolder();
                console.log(currentDir);

                const devManifest = {
                    port: port,
                    entrypoint: plugConfig.entrypoint,
                    containerElementId: plugConfig.containerElementId,
                    isReact: isReact
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

            // Generate the manifest once the server handles its first request
            server.httpServer?.once('listening', generateDevManifest);

        },
        generateBundle(this: any) { //TODO: understand this further
            // Emit the production integration metadata alongside Vite's own manifest.json so the
            // backend can read containerElementId/isReact without any hand-written config.
            const prodManifest = {
                entrypoint: plugConfig.entrypoint,
                containerElementId: plugConfig.containerElementId,
                isReact: isReact
            };

            this.emitFile({
                type: 'asset',
                fileName: `manifest.prod.json`,
                source: JSON.stringify(prodManifest, null, 2)
            });
        }
    };
};

// https://vite.dev/config/
export default defineConfig({
    plugins: [
        react(),
        babel({ presets: [reactCompilerPreset()] }),
        /*ViteDotNet("src/main.tsx", 5174, "LatestViteReact")*/
        ViteDotNetPlugin("src/main.tsx", "root") as any
    ],
})
