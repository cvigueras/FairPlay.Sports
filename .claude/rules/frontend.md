---
paths:
  - "web/**"
---

# Frontend (web/)

Frontend (`web/`) is out of scope for these notes
unless the task explicitly targets it. When it does: Vue 3 + Vuetify 4
(mdi-svg icons — pass icon paths, no runtime font), Pinia, vue-i18n
(switch/persist the locale via `setLocale` in `src/plugins/i18n.ts`; it is
global, so a change on any screen shows everywhere). All HTTP goes through
`src/lib/http.ts` (`get/post/put/postForm`); the access token lives in
memory in the `auth` store, the refresh token in an `HttpOnly` cookie.
`npm run build` runs the type-check (`vue-tsc`, which also covers `e2e/`) and
the build. In `handleSubmit`-style flows keep `router.push` **outside**
the try/catch that wraps the API call, so a navigation rejection is not
surfaced as an API error.

## E2E tests (Playwright)

`cd web && npm run test:e2e` runs the Playwright tests in `web/e2e/`.
`playwright.config.ts` starts the Vite dev server itself (or reuses the one on
5173). First time on a machine: `npx playwright install chromium`.

- **No API, no database.** `openAuthScreen` in `e2e/support.ts` answers every
  `/api` call with a 401 and pins the locale to English, so the selectors use the
  English texts of `src/locales/en.json` and the CSS classes of the screen
  (`.login-panel__submit`...). Keep new tests free of the API the same way.
- **The file name picks the project:** `*.mobile.spec.ts` runs at 390x664 (iPhone 13
  with Safari toolbars), `*.layout.spec.ts` at 1024x768 and 1280x800,
  `*.breakpoint.spec.ts` at 1280x800 and sets its own widths.
- **Known bugs** are written as `test.fail(true, 'Story N: ...')`: the suite stays
  green, and turns red when the bug is fixed (then remove the line).
- **Check a new test** by reverting the fix it protects: it must fail.
- **Report:** locally the HTML report opens in the browser after every run and holds
  the terminal until Ctrl+C. Claude must run `npx playwright test --reporter=list`
  (no report, no wait) so the command returns.
- CI runs them in the `frontend-e2e` job of `ci.yml`.
