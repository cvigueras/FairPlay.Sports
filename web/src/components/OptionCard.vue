<script setup lang="ts">
import { computed } from 'vue'
import { mdiCheck } from '@mdi/js'

/**
 * A selectable option as a card: an icon in a tinted circle, a title, a short
 * description and a check box on the right. For yes/no choices that deserve a
 * sentence of explanation (a bare checkbox would squeeze it). Accessible as a
 * checkbox: one focusable button, `aria-checked`, toggled by click, Space or Enter.
 */
const props = defineProps<{
  modelValue: boolean
  title: string
  description?: string
  icon: string
  /** Accent colour (any CSS colour): the icon, and when checked the border and the box. */
  color: string
}>()

const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>()

const accent = computed(() => ({ '--option-color': props.color }))
</script>

<template>
  <button
    type="button"
    role="checkbox"
    :aria-checked="modelValue"
    class="option-card"
    :class="{ 'option-card--on': modelValue }"
    :style="accent"
    @click="emit('update:modelValue', !modelValue)"
  >
    <span class="option-card__icon" aria-hidden="true">
      <v-icon :icon="icon" size="22" />
    </span>

    <span class="option-card__text">
      <span class="option-card__title">{{ title }}</span>
      <span v-if="description" class="option-card__description">{{ description }}</span>
    </span>

    <span class="option-card__box" aria-hidden="true">
      <v-icon v-if="modelValue" :icon="mdiCheck" size="16" />
    </span>
  </button>
</template>

<style scoped>
.option-card {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  width: 100%;
  padding: 0.85rem 1rem;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  background: rgb(var(--v-theme-surface));
  color: #0f172a;
  font: inherit;
  text-align: left;
  cursor: pointer;
  transition:
    border-color 0.15s ease,
    background-color 0.15s ease;
}

.option-card:hover {
  border-color: color-mix(in srgb, var(--option-color) 55%, #e2e8f0);
}

.option-card--on {
  border-color: var(--option-color);
  background: color-mix(in srgb, var(--option-color) 7%, rgb(var(--v-theme-surface)));
}

.option-card__icon {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 50%;
  color: var(--option-color);
  background: color-mix(in srgb, var(--option-color) 13%, #ffffff);
}

.option-card__text {
  flex: 1 1 auto;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.option-card__title {
  font-size: 0.9375rem;
  font-weight: 600;
}

.option-card__description {
  font-size: 0.8125rem;
  line-height: 1.4;
  color: #64748b;
}

.option-card__box {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 1.375rem;
  height: 1.375rem;
  border: 1.5px solid #cbd5e1;
  border-radius: 6px;
  background: #ffffff;
  color: #ffffff;
  transition:
    background-color 0.15s ease,
    border-color 0.15s ease;
}

.option-card--on .option-card__box {
  border-color: var(--option-color);
  background: var(--option-color);
}

.option-card:focus-visible {
  outline: 2px solid rgb(var(--v-theme-primary));
  outline-offset: 2px;
}
</style>
