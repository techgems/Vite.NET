import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import ViteDotNetPlugin from 'vite-dotnet'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        react(),
        ViteDotNetPlugin("src/main.tsx", "root")
    ]
})
