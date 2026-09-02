import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import vuetify from './plugins/vuetify'
import { useAuthStore } from './stores/auth'
import './assets/main.css'

const app = createApp(App)

app.use(createPinia())
app.use(vuetify)

// Restore the session from the refresh-token cookie before the first route
// resolves, so a reload on a protected page doesn't bounce to /login.
await useAuthStore().tryRefresh()

app.use(router)

app.mount('#app')
