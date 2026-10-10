import { expect, test, type Page } from '@playwright/test'
import { openAuthScreen } from './support'

// Runs at 390x664 only (project mobile-390): iPhone 13 with Safari's toolbars.

test.beforeEach(async ({ page }) => {
  await openAuthScreen(page, 'register')
})

// Story 26: the submit button and the footer line were cut off and unreachable.
async function expectReachable(page: Page) {
  const panel = page.locator('.login-panel')
  await panel.evaluate((el) => el.scrollTo(0, el.scrollHeight))

  const [panelBox, buttonBox, footerBox] = await Promise.all([
    panel.boundingBox(),
    page.locator('.login-panel__submit').boundingBox(),
    page.locator('.login-panel__footer').boundingBox(),
  ])

  expect(panelBox && buttonBox && footerBox).toBeTruthy()
  const panelBottom = panelBox!.y + panelBox!.height
  // The panel itself must fit the screen: if the grid row grows past the
  // viewport, the bottom of the panel is cut off and cannot be scrolled to.
  expect(panelBottom).toBeLessThanOrEqual(page.viewportSize()!.height + 0.5)
  expect(buttonBox!.y).toBeGreaterThanOrEqual(panelBox!.y)
  expect(buttonBox!.y + buttonBox!.height).toBeLessThanOrEqual(footerBox!.y)
  expect(footerBox!.y + footerBox!.height).toBeLessThanOrEqual(panelBottom - 32 + 0.5)
  await expect(page.locator('.login-panel__submit')).toBeInViewport()
  await expect(page.locator('.login-panel__switch')).toBeInViewport()
}

test('register: submit button and footer are reachable, with 32px free under the footer', async ({
  page,
}) => {
  await expectReachable(page)
})

// The errors make the form taller, which is when the grid row used to grow past
// the screen and cut the button off.
test('register: submit button and footer stay reachable after an empty submit', async ({
  page,
}) => {
  await page.locator('.login-panel__submit').click()
  await expectReachable(page)
})

// Story 26: a missing role is flagged by red pills, not by a visible message.
test('register: an empty submit turns every role pill red and shows no role message', async ({
  page,
}) => {
  await page.locator('.login-panel__submit').click()

  const pills = page.locator('.login-panel__pill')
  await expect(pills).toHaveCount(5)
  await expect(page.locator('.login-panel__pill--error')).toHaveCount(5)

  // The message stays in the DOM for screen readers but takes no visible space.
  const message = page.getByRole('alert').filter({ hasText: 'main role' })
  await expect(message).toHaveCount(1)
  const box = await message.boundingBox()
  expect(box!.width).toBeLessThanOrEqual(1)
  expect(box!.height).toBeLessThanOrEqual(1)
})
