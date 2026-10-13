import { expect, test } from '@playwright/test'
import { layoutOf, openAuthScreen } from './support'

// Runs at 1024x768 and 1280x800 (projects tablet-1024 and desktop-1280): both
// are on the desktop side of the 900px breakpoint, so the desktop layout shows.

for (const screen of ['login', 'register'] as const) {
  test(`${screen}: desktop layout (headline, solid panel, no mobile language button)`, async ({
    page,
  }) => {
    await openAuthScreen(page, screen)

    expect(await layoutOf(page)).toBe('desktop')
    await expect(page.locator('.login-panel__heading')).toBeVisible()
    await expect(page.locator('.login-panel__lang')).toBeVisible()
  })
}
