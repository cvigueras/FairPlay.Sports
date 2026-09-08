import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import { aliases, mdi } from 'vuetify/iconsets/mdi-svg'
import { createVueI18nAdapter } from 'vuetify/locale/adapters/vue-i18n'
import { useI18n } from 'vue-i18n'
import i18n from './i18n'

type VueI18nAdapterParams = Parameters<typeof createVueI18nAdapter>[0]

/**
 * FairPlay Sports Vuetify setup. Component registration is handled by
 * `vite-plugin-vuetify` (autoImport) in `vite.config.ts`; here we only define
 * the brand theme (green primary over a slate surface), the tree-shakeable
 * mdi-svg icon set, the vue-i18n locale adapter (so Vuetify's own component
 * strings follow the app language), and a few component defaults so forms and
 * buttons stay consistent across views.
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

// Used only by the auth screens: the login/register card is a translucent
// "glass" panel over the stadium photo, so its contents need a dark palette
// (light text, brighter green) to stay legible.
const fairplayDark = {
  dark: true,
  colors: {
    primary: '#34d399',
    'primary-darken-1': '#10b981',
    secondary: '#94a3b8',
    error: '#fb7185',
    success: '#4ade80',
    background: '#0f172a',
    surface: '#0f172a',
  },
}

export default createVuetify({
  locale: {
    // The adapter only reads `global.locale/fallbackLocale/t`; the cast just
    // widens our literal `'es' | 'en'` locale union to the `string` it expects.
    adapter: createVueI18nAdapter({ i18n, useI18n } as unknown as VueI18nAdapterParams),
  },
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets: { mdi },
  },
  theme: {
    defaultTheme: 'fairplay',
    themes: { fairplay, fairplayDark },
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
