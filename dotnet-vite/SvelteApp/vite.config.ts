import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import ViteDotNet from 'vite-dotnet';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [svelte(), ViteDotNet("src/main.ts")]
})
