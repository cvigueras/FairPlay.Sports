<script setup lang="ts">
withDefaults(defineProps<{ label: string; hint?: string; error?: string; required?: boolean }>(), {
  hint: '',
  error: '',
  required: false,
})
</script>

<template>
  <div class="fp-field">
    <label>{{ label }}<span v-if="required" class="fp-required-mark">*</span></label>
    <slot />
    <span v-if="error" class="fp-error">{{ error }}</span>
    <span v-else-if="hint" class="fp-hint">{{ hint }}</span>
  </div>
</template>
