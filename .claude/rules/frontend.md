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
`npm run build` runs the type-check (`vue-tsc`) and the build; there are no
frontend tests. In `handleSubmit`-style flows keep `router.push` **outside**
the try/catch that wraps the API call, so a navigation rejection is not
surfaced as an API error.
