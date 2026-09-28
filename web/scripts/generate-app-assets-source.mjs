// One-off helper: composites the existing brand logo (which has a
// transparent background) onto flat backgrounds at the sizes
// @capacitor/assets expects for its source images, since app icons can't
// have an alpha channel (iOS rejects them) and a splash screen needs a large
// flat canvas. Not part of the app build - run manually, then `npx
// capacitor-assets generate` reads resources/icon.png and resources/splash.png.
import sharp from 'sharp'

const LOGO = 'src/assets/logo.webp'
const WHITE = { r: 255, g: 255, b: 255, alpha: 1 }

async function composite(size, logoScale, outPath) {
  const logoSize = Math.round(size * logoScale)
  const logo = await sharp(LOGO).resize(logoSize, logoSize).toBuffer()
  await sharp({
    create: { width: size, height: size, channels: 4, background: WHITE },
  })
    .composite([{ input: logo, gravity: 'center' }])
    .flatten({ background: WHITE })
    .png()
    .toFile(outPath)
}

await composite(1024, 0.65, 'resources/icon.png')
await composite(2732, 0.35, 'resources/splash.png')

console.log('Wrote resources/icon.png and resources/splash.png')
