<script setup lang="ts">
import { computed } from 'vue'

/**
 * A team on/off flag ("Desafiame!", "Campo disponible") as one button: icon, a
 * label that never changes, and a small on/off track on the right. Sized to sit
 * next to the row's buttons; the parent decides the width.
 *
 * `plain` drops the switch: it keeps the accent-coloured border and tinted background
 * of the "on" state (an icon and a label), for actions that are not a flag - it just
 * emits `toggle` on click.
 */
const props = withDefaults(
  defineProps<{
    modelValue?: boolean
    label: string
    icon: string
    /** Accent colour: the icon, the label and (when on) the border and the track. */
    color?: string
    /** Darker shade of the accent, for the label on the tinted "on" background. */
    textColor?: string
    busy?: boolean
    large?: boolean
    plain?: boolean
  }>(),
  {
    modelValue: false,
    color: 'rgb(var(--v-theme-success))',
    textColor: '#166534',
    busy: false,
    large: false,
    plain: false,
  },
)

defineEmits<{ (e: 'toggle'): void }>()

const toneStyle = computed(() => ({ '--flag-color': props.color, '--flag-text': props.textColor }))
</script>

<template>
  <button
    type="button"
    :role="plain ? undefined : 'switch'"
    :aria-checked="plain ? undefined : modelValue"
    :disabled="busy"
    class="flag-switch"
    :class="{ 'flag-switch--on': modelValue || plain, 'flag-switch--large': large }"
    :style="toneStyle"
    @click="$emit('toggle')"
  >
    <v-icon :icon="icon" :size="large ? 20 : 16" />
    <span class="flag-switch__label">{{ label }}</span>
    <span v-if="!plain" class="flag-switch__track" aria-hidden="true">
      <span class="flag-switch__thumb" />
    </span>
  </button>
</template>

<style scoped>
.flag-switch {
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
  /* Same corner radius as the other buttons; only the inner track stays fully round. */
  border-radius: 4px;
  background: rgb(var(--v-theme-surface));
  color: var(--flag-color);
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

.flag-switch--large {
  --track-w: 34px;
  --track-h: 18px;
  --thumb: 14px;

  height: 44px;
  padding: 0 0.75rem 0 1rem;
  font-size: 0.9375rem;
  gap: 0.6rem;
}

.flag-switch__label {
  flex: 1 1 auto;
  text-align: left;
}

.flag-switch__track {
  position: relative;
  flex: 0 0 auto;
  width: var(--track-w);
  height: var(--track-h);
  border-radius: 999px;
  background: #cbd5e1;
  transition: background-color 0.18s ease;
}

.flag-switch__thumb {
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

.flag-switch--on {
  border-color: var(--flag-color);
  background: color-mix(in srgb, var(--flag-color) 10%, rgb(var(--v-theme-surface)));
  color: var(--flag-text);
}

.flag-switch--on .flag-switch__track {
  background: var(--flag-color);
}

.flag-switch--on .flag-switch__thumb {
  transform: translateX(calc(var(--track-w) - var(--track-h)));
}

.flag-switch:focus-visible {
  outline: 2px solid rgb(var(--v-theme-primary));
  outline-offset: 2px;
}

.flag-switch:disabled {
  opacity: 0.6;
  cursor: default;
}
</style>
