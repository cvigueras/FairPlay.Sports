<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { mdiCamera } from '@mdi/js'
import { ApiError, baseUrl } from '@/lib/http'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const { t } = useI18n()

const fileInput = ref<HTMLInputElement | null>(null)
const uploading = ref(false)
const errorMessage = ref('')
const showError = ref(false)
/** Bumped after each upload so the <img> refetches instead of using the cached one. */
const version = ref(0)

const initials = computed(() =>
  (auth.currentUser?.userName ?? '')
    .split(/[\s_-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join(''),
)

const photoUrl = computed(() => {
  const user = auth.currentUser
  if (!user?.hasPhoto) return null
  const bust = version.value ? `?v=${version.value}` : ''
  return `${baseUrl}/api/users/${user.id}/photo${bust}`
})

function pick() {
  fileInput.value?.click()
}

async function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return

  uploading.value = true
  try {
    await auth.uploadPhoto(file)
    version.value = Date.now()
  } catch (error) {
    errorMessage.value = error instanceof ApiError ? error.message : t('profile.photo.uploadFailed')
    showError.value = true
  } finally {
    uploading.value = false
  }
}
</script>

<template>
  <div class="profile-avatar flex-shrink-0">
    <v-avatar
      color="primary"
      size="120"
      class="profile-avatar__circle text-h4 font-weight-bold"
      role="button"
      tabindex="0"
      :aria-label="t('profile.photo.change')"
      @click="pick"
      @keydown.enter.prevent="pick"
      @keydown.space.prevent="pick"
    >
      <v-img v-if="photoUrl" :src="photoUrl" :alt="t('profile.photo.alt')" cover />
      <template v-else>{{ initials }}</template>
    </v-avatar>

    <span class="profile-avatar__badge">
      <v-progress-circular v-if="uploading" indeterminate size="18" width="2" />
      <v-icon v-else :icon="mdiCamera" size="18" />
    </span>

    <input
      ref="fileInput"
      type="file"
      accept="image/png,image/jpeg,image/webp,image/svg+xml"
      class="d-none"
      @change="onFileChange"
    />
  </div>

  <v-snackbar v-model="showError" color="error" timeout="4000">{{ errorMessage }}</v-snackbar>
</template>

<style scoped>
.profile-avatar {
  position: relative;
  width: 120px;
  height: 120px;
}
.profile-avatar__circle {
  cursor: pointer;
}
.profile-avatar__badge {
  position: absolute;
  right: 2px;
  bottom: 2px;
  display: grid;
  place-items: center;
  width: 34px;
  height: 34px;
  border-radius: 50%;
  color: #fff;
  background: rgb(var(--v-theme-primary));
  border: 3px solid rgb(var(--v-theme-surface));
  pointer-events: none;
}
</style>
