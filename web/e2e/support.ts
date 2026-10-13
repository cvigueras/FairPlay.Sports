import { expect, type Page } from '@playwright/test'

/**
 * Opens the login or register screen with no backend: every /api call (the
 * session restore at startup, mainly) is answered with a 401, and the locale
 * is pinned to English so the selectors can use the English texts of
 * src/locales/en.json.
 */
export async function openAuthScreen(page: Page, screen: 'login' | 'register') {
  await page.addInitScript(() => localStorage.setItem('fps_locale', 'en'))
  await page.route('**/api/**', (route) =>
    route.fulfill({ status: 401, contentType: 'application/json', body: '{"error":"stub"}' }),
  )
  await page.goto(`/${screen}`, { waitUntil: 'domcontentloaded' })
  await expect(page.locator('.login-panel__submit')).toBeVisible({ timeout: 20_000 })
}

/**
 * Which layout is on screen. The desktop headline (`.login-hero__title`) is
 * rendered only on desktop, and the mobile language button only on mobile.
 */
export async function layoutOf(page: Page): Promise<'mobile' | 'desktop'> {
  const desktop = (await page.locator('.login-hero__title').count()) > 0
  const mobile = (await page.locator('.login-hero__lang').count()) > 0
  expect(desktop !== mobile, 'exactly one layout must be rendered').toBe(true)
  return desktop ? 'desktop' : 'mobile'
}
