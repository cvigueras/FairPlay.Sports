<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { mdiChevronDown } from '@mdi/js'
import { KIT_COLOR_PALETTE } from '@/lib/kitColors'

defineProps<{ modelValue: string }>()
const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()

const { t } = useI18n()
const open = ref(false)
const root = ref<HTMLElement | null>(null)

function toggle() {
  open.value = !open.value
}

function select(value: string) {
  emit('update:modelValue', value)
  open.value = false
}

function onDocumentClick(event: MouseEvent) {
  if (root.value && !root.value.contains(event.target as Node)) open.value = false
}

onMounted(() => document.addEventListener('click', onDocumentClick))
onBeforeUnmount(() => document.removeEventListener('click', onDocumentClick))
</script>

<template>
  <div ref="root" class="fp-color-select">
    <button type="button" class="fp-color-trigger" :aria-expanded="open" @click="toggle">
      <span class="fp-color-bar" :style="{ background: modelValue }" />
      <v-icon :icon="mdiChevronDown" size="18" />
    </button>
    <div v-if="open" class="fp-color-menu">
      <button
        v-for="color in KIT_COLOR_PALETTE"
        :key="color.value"
        type="button"
        class="fp-color-option"
        :class="{ 'fp-color-option--selected': color.value.toLowerCase() === modelValue.toLowerCase() }"
        @click="select(color.value)"
      >
        <span class="fp-color-bar fp-color-bar--option" :style="{ background: color.value }" />
        <span class="fp-color-name">{{ t(`profile.team.colorNames.${color.labelKey}`) }}</span>
      </button>
    </div>
  </div>
</template>

<style scoped>
.fp-color-select {
  position: relative;
}

.fp-color-trigger {
  height: 44px;
  width: 100%;
  padding: 6px 10px;
  border-radius: 10px;
  border: 1.5px solid #cbd5e1;
  background: rgb(var(--v-theme-surface));
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  color: #94a3b8;
}
.fp-color-trigger:hover {
  border-color: #94a3b8;
}

.fp-color-bar {
  height: 22px;
  border-radius: 6px;
  border: 1px solid rgba(15, 23, 42, 0.12);
  flex: 1;
  min-width: 0;
}

.fp-color-menu {
  position: absolute;
  z-index: 20;
  top: calc(100% + 6px);
  left: 0;
  right: 0;
  max-height: 260px;
  overflow-y: auto;
  padding: 8px;
  border-radius: 12px;
  border: 1.5px solid #e2e8f0;
  background: rgb(var(--v-theme-surface));
  box-shadow: 0 16px 40px rgba(15, 23, 42, 0.18);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.fp-color-option {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 6px 8px;
  border-radius: 8px;
  border: none;
  background: transparent;
  cursor: pointer;
  font: inherit;
  font-size: 13px;
  color: #334155;
  text-align: left;
}
.fp-color-option:hover {
  background: rgb(var(--v-theme-background));
}
.fp-color-option--selected {
  background: rgba(var(--v-theme-primary), 0.08);
  color: rgb(var(--v-theme-primary-darken-1));
  font-weight: 700;
}

.fp-color-option .fp-color-bar--option {
  width: 56px;
  height: 18px;
  flex-shrink: 0;
}

.fp-color-name {
  flex: 1;
}
</style>
