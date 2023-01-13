import { defineConfig } from 'vite'
import type { UserConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import { basename, posix } from 'path'
import { ViteDotNetPlugin } from 'vite-dotnet';


// https://vitejs.dev/config/
export default defineConfig({
  plugins: [svelte(), ViteDotNetPlugin("src/main.ts")]
})
