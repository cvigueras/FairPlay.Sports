import { createI18n } from 'vue-i18n'
import { en as vuetifyEn, es as vuetifyEs } from 'vuetify/locale'
import en from '@/locales/en.json'
import es from '@/locales/es.json'

export const SUPPORTED_LOCALES = ['es', 'en'] as const
export type AppLocale = (typeof SUPPORTED_LOCALES)[number]

const DEFAULT_LOCALE: AppLocale = 'es'
const STORAGE_KEY = 'fps_locale'

function isAppLocale(value: unknown): value is AppLocale {
  return value === 'es' || value === 'en'
}

/** The remembered choice from a previous visit, otherwise Spanish. */
function initialLocale(): AppLocale {
  try {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (isAppLocale(saved)) return saved
  } catch {
    // localStorage may be unavailable (private mode, blocked cookies).
  }
  return DEFAULT_LOCALE
}

const i18n = createI18n({
  legacy: false,
  locale: initialLocale(),
  fallbackLocale: DEFAULT_LOCALE,
  // Vuetify's own component strings live under `$vuetify` so the vue-i18n
  // locale adapter can resolve them from the same message tree.
  messages: {
    es: { ...es, $vuetify: vuetifyEs },
    en: { ...en, $vuetify: vuetifyEn },
  },
})

document.documentElement.lang = i18n.global.locale.value

/** Switch the active locale and persist it for the next visit. */
export function setLocale(locale: AppLocale): void {
  i18n.global.locale.value = locale
  document.documentElement.lang = locale
  try {
    localStorage.setItem(STORAGE_KEY, locale)
  } catch {
    // Persisting is best-effort; the switch still applies for this session.
  }
}

export default i18n
