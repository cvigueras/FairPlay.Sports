import { expect, test } from '@playwright/test'
import { layoutOf, openAuthScreen } from './support'

// Story 29: the JS flag (`narrow`) and the CSS media query used different
// widths (1145px vs 899px), leaving a half-mobile layout between 900 and
// 1144px. There is one breakpoint now: mobile up to 899px, desktop from 900px.
// The script and the stylesheet are checked together: the same width must give
// the mobile (or desktop) markup and the mobile (or desktop) CSS.

const cases = [
  { width: 899, expected: 'mobile' },
  { width: 900, expected: 'desktop' },
  { width: 1144, expected: 'desktop' },
  { width: 1280, expected: 'desktop' },
] as const

for (const { width, expected } of cases) {
  test(`login at ${width}px is the ${expected} layout`, async ({ page }) => {
    await page.setViewportSize({ width, height: 800 })
    await openAuthScreen(page, 'login')

    expect(await layoutOf(page)).toBe(expected)

    // The stylesheet agrees with the markup: the heading is hidden by CSS on
    // mobile only.
    const heading = page.locator('.login-panel__heading')
    if (expected === 'mobile') await expect(heading).toBeHidden()
    else await expect(heading).toBeVisible()
  })
}
