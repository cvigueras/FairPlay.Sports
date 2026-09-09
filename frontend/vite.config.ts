import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import vuetify from 'vite-plugin-vuetify'
import vueI18n from '@intlify/unplugin-vue-i18n/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
    vuetify({ autoImport: true }),
    // Precompiles src/locales/*.json at build time so we ship the smaller
    // runtime-only vue-i18n (no in-browser message compiler, no unsafe-eval).
    vueI18n({
      include: [fileURLToPath(new URL('./src/locales/**', import.meta.url))],
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    // Pinned so the dev origin stays http://localhost:5173 (the API's CORS
    // allow-list). `strictPort` fails loudly instead of drifting to 5174.
    host: 'localhost',
    port: 5173,
    strictPort: true,
  },
})
