import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import ViteDotNet from 'vite-dotnet'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), ViteDotNet("src/main.tsx", 5174, "ReactApp")]
})
