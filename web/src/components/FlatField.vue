<script setup lang="ts">
withDefaults(defineProps<{ label: string; hint?: string; error?: string }>(), { hint: '', error: '' })
</script>

<template>
  <div class="fp-field">
    <label>{{ label }}</label>
    <slot />
    <span v-if="error" class="fp-error">{{ error }}</span>
    <span v-else-if="hint" class="fp-hint">{{ hint }}</span>
  </div>
</template>
