<script setup lang="ts">
import { mdiHandshakeOutline } from '@mdi/js'
import { useI18n } from 'vue-i18n'

/**
 * "Desafiame!" as one pill: icon, a label that never changes, and a small
 * on/off track on the right. Sized to sit next to the row's buttons; the parent
 * decides the width.
 */
defineProps<{ modelValue: boolean; busy?: boolean; large?: boolean }>()
defineEmits<{ (e: 'toggle'): void }>()

const { t } = useI18n()
</script>

<template>
  <button
    type="button"
    role="switch"
    :aria-checked="modelValue"
    :disabled="busy"
    class="accept-switch"
    :class="{ 'accept-switch--on': modelValue, 'accept-switch--large': large }"
    @click="$emit('toggle')"
  >
    <v-icon :icon="mdiHandshakeOutline" :size="large ? 20 : 16" />
    <span class="accept-switch__label">{{ t('teams.challengeStatus.button') }}</span>
    <span class="accept-switch__track" aria-hidden="true">
      <span class="accept-switch__thumb" />
    </span>
  </button>
</template>

<style scoped>
.accept-switch {
  --track-w: 26px;
  --track-h: 14px;
  --thumb: 10px;

  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  width: 100%;
  height: 28px;
  padding: 0 0.5rem 0 0.7rem;
  border: 1px solid #cbd5e1;
  border-radius: 999px;
  background: rgb(var(--v-theme-surface));
  color: #475569;
  font: inherit;
  font-size: 0.75rem;
  font-weight: 500;
  white-space: nowrap;
  cursor: pointer;
  transition:
    background-color 0.18s ease,
    border-color 0.18s ease,
    color 0.18s ease;
}

.accept-switch--large {
  --track-w: 34px;
  --track-h: 18px;
  --thumb: 14px;

  height: 44px;
  padding: 0 0.75rem 0 1rem;
  font-size: 0.9375rem;
  gap: 0.6rem;
}

.accept-switch__label {
  flex: 1 1 auto;
  text-align: left;
}

.accept-switch__track {
  position: relative;
  flex: 0 0 auto;
  width: var(--track-w);
  height: var(--track-h);
  border-radius: 999px;
  background: #cbd5e1;
  transition: background-color 0.18s ease;
}

.accept-switch__thumb {
  position: absolute;
  top: calc((var(--track-h) - var(--thumb)) / 2);
  left: calc((var(--track-h) - var(--thumb)) / 2);
  width: var(--thumb);
  height: var(--thumb);
  border-radius: 50%;
  background: #ffffff;
  box-shadow: 0 1px 2px rgba(15, 23, 42, 0.3);
  transition: transform 0.18s ease;
}

.accept-switch--on {
  border-color: rgb(var(--v-theme-success));
  background: color-mix(in srgb, rgb(var(--v-theme-success)) 10%, rgb(var(--v-theme-surface)));
  color: #166534;
}

.accept-switch--on .accept-switch__track {
  background: rgb(var(--v-theme-success));
}

.accept-switch--on .accept-switch__thumb {
  transform: translateX(calc(var(--track-w) - var(--track-h)));
}

.accept-switch:focus-visible {
  outline: 2px solid rgb(var(--v-theme-primary));
  outline-offset: 2px;
}

.accept-switch:disabled {
  opacity: 0.6;
  cursor: default;
}
</style>
