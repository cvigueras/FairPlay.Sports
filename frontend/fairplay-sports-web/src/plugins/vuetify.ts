import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import { aliases, mdi } from 'vuetify/iconsets/mdi-svg'

/**
 * FairPlay Sports Vuetify setup. Component registration is handled by
 * `vite-plugin-vuetify` (autoImport) in `vite.config.ts`; here we only define
 * the brand theme (green primary over a slate surface), the tree-shakeable
 * mdi-svg icon set, and a few component defaults so forms and buttons stay
 * consistent across views.
 */
const fairplay = {
  dark: false,
  colors: {
    primary: '#16a34a',
    'primary-darken-1': '#15803d',
    secondary: '#334155',
    error: '#dc2626',
    success: '#15803d',
    background: '#f8fafc',
    surface: '#ffffff',
  },
}

export default createVuetify({
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets: { mdi },
  },
  theme: {
    defaultTheme: 'fairplay',
    themes: { fairplay },
  },
  defaults: {
    VTextField: {
      variant: 'outlined',
      density: 'comfortable',
      color: 'primary',
    },
    VBtn: {
      color: 'primary',
      flat: true,
    },
  },
})
