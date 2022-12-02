import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    origin: 'http://localhost:5173',
    proxy:{
      '*' : {
        target: 'https://localhost:7167',
        changeOrigin: true
      }
    },
    hmr: {
      protocol: 'ws'
    }
  },
  build: {
    manifest: true,
    rollupOptions: {
      // overwrite default .html entry
      input: 'src/main.ts'
    }
  },
  plugins: [svelte()]
})
