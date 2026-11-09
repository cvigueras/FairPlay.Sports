# fairplay-sports-web

Vue 3 + TypeScript + Vite frontend of FairPlay Sports, with Vuetify 4, Pinia and vue-i18n
(English and Spanish). It is also packaged for iOS and Android with Capacitor. See the
[root README](../README.md) for the architecture and how to run the API it talks to.

## Setup

```sh
npm install
npm run dev
```

Served at `http://localhost:5173`. The API base URL comes from `VITE_API_BASE_URL`
(default `https://localhost:7090`); override it in a git-ignored `.env.local`
(see `.env.example`).

## Scripts

| Script | What it does |
|---|---|
| `npm run dev` | Vite dev server with hot reload |
| `npm run build` | Type-check (`vue-tsc`) and production build |
| `npm run preview` | Serve the production build locally |
| `npm run test:e2e` | Playwright tests in `e2e/` (first time: `npx playwright install chromium`) |
| `npm run cap:sync` | Build and sync the web bundle into the native projects |
| `npm run cap:open:ios` / `cap:open:android` | Open the native project in Xcode / Android Studio |

The e2e tests mock every `/api` call, so they need neither the API nor a database.

## Editor

[VS Code](https://code.visualstudio.com/) + [Vue (Official)](https://marketplace.visualstudio.com/items?itemName=Vue.volar)
(disable Vetur). Volar makes TypeScript aware of `.vue` imports; the CLI uses `vue-tsc`.
