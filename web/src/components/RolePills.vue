<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { TEAM_MEMBER_ROLES, type TeamMemberRole } from '@/types/team'

defineProps<{ modelValue: TeamMemberRole | null }>()
const emit = defineEmits<{ (e: 'update:modelValue', value: TeamMemberRole): void }>()

const { t } = useI18n()

const roles = computed(() =>
  TEAM_MEMBER_ROLES.map((role) => ({ value: role, title: t(`profile.team.memberRoles.${role}`) })),
)
</script>

<template>
  <div class="d-flex flex-wrap justify-center ga-1">
    <button
      v-for="role in roles"
      :key="role.value"
      type="button"
      class="fp-pill"
      :class="{ 'fp-pill--selected': modelValue === role.value }"
      @click="emit('update:modelValue', role.value)"
    >
      {{ role.title }}
    </button>
  </div>
</template>
