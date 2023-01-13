import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import type { PluginConfig } from 'vite-dotnet'
import ViteDotNet from 'vite-dotnet'

const config: PluginConfig = {
  port: 5174,
  appFolder: "ReactApp",
  entrypoint: "src/main.tsx"
}

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), ViteDotNet(config)],
})
