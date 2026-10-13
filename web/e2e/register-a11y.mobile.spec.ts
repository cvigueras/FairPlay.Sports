import { expect, test, type Page } from '@playwright/test'
import { openAuthScreen } from './support'

// Story 33 (not fixed yet): both tests are marked test.fail(), so they pass
// while the defect exists and turn red as soon as it is fixed. When story 33
// is done, delete the test.fail() line of each test.

/** Relative luminance of an sRGB colour, as defined by WCAG 2.x. */
function luminance([r, g, b]: number[]): number {
  const [lr, lg, lb] = [r, g, b].map((c) => {
    const s = c / 255
    return s <= 0.03928 ? s / 12.92 : ((s + 0.055) / 1.055) ** 2.4
  })
  return 0.2126 * lr + 0.7152 * lg + 0.0722 * lb
}

function contrast(a: number[], b: number[]): number {
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x)
  return (hi + 0.05) / (lo + 0.05)
}

/** Average colour of the background behind an element, measured on a screenshot. */
async function backgroundBehind(page: Page, selector: string): Promise<number[]> {
  const target = page.locator(selector)
  // Hide the text so only the photo and the gradient are measured.
  const png = await target.screenshot({
    style: `${selector} { color: transparent !important; }`,
  })
  return page.evaluate(async (base64) => {
    const image = new Image()
    image.src = `data:image/png;base64,${base64}`
    await image.decode()
    const canvas = document.createElement('canvas')
    canvas.width = image.width
    canvas.height = image.height
    const context = canvas.getContext('2d')!
    context.drawImage(image, 0, 0)
    const { data } = context.getImageData(0, 0, canvas.width, canvas.height)
    const sum = [0, 0, 0]
    for (let i = 0; i < data.length; i += 4) {
      sum[0] += data[i]
      sum[1] += data[i + 1]
      sum[2] += data[i + 2]
    }
    const pixels = data.length / 4
    return sum.map((value) => value / pixels)
  }, png.toString('base64'))
}

test.beforeEach(async ({ page }) => {
  await openAuthScreen(page, 'register')
})

test('register: the footer link has a contrast of at least 4.5:1', async ({ page }) => {
  test.fail(true, 'Story 33: the link uses the light-theme indigo over the dark photo (~2.6:1)')

  const selector = '.login-panel__switch'
  const colour = await page.locator(selector).evaluate((el) => getComputedStyle(el).color)
  const text = colour.match(/\d+/g)!.slice(0, 3).map(Number)
  const background = await backgroundBehind(page, selector)

  expect(contrast(text, background)).toBeGreaterThanOrEqual(4.5)
})

for (const width of [390, 320]) {
  test(`register: the "Confirm password" label is not clipped at ${width}px`, async ({ page }) => {
    test.fail(true, 'Story 33: the label is 137px of text in a 130px field')

    await page.setViewportSize({ width, height: 664 })

    const label = page.locator('.v-field__field label.v-label', { hasText: 'Confirm password' })
    const { scroll, client } = await label.evaluate((el) => ({
      scroll: el.scrollWidth,
      client: el.clientWidth,
    }))
    expect(scroll).toBeLessThanOrEqual(client)
  })
}
