<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import type { TemplatePreviewResponse } from '../types/contracts'
import EmailPreview from './EmailPreview.vue'

defineProps<{
  preview: TemplatePreviewResponse | null
  from: string
  to: string
  subject: string
}>()

const emit = defineEmits<{ (e: 'close'): void }>()

function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape') emit('close')
}

onMounted(() => window.addEventListener('keydown', onKey))
onUnmounted(() => window.removeEventListener('keydown', onKey))
</script>

<template>
  <div v-if="preview" class="backdrop" @click.self="emit('close')">
    <div class="panel dialog" role="dialog" aria-modal="true" aria-label="Vista previa del correo">
      <button type="button" class="close" aria-label="Cerrar" @click="emit('close')">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" aria-hidden="true"><path d="M6 6l12 12M18 6L6 18" /></svg>
      </button>
      <EmailPreview :preview="preview" :from="from" :to="to" :subject="subject" tall @expand="emit('close')" />
    </div>
  </div>
</template>

<style scoped>
.backdrop {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: grid;
  place-items: center;
  padding: 1rem;
  background: rgba(3, 5, 10, 0.75);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  animation: fade-up var(--motion) var(--ease-out) both;
}

.dialog {
  position: relative;
  width: min(960px, 100%);
  padding: 0.75rem;
  box-shadow: var(--shadow-2);
}

.close {
  position: absolute;
  top: -14px;
  right: -14px;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: 1px solid var(--line-strong);
  background: var(--bg-raised);
  color: var(--text);
  display: grid;
  place-items: center;
  cursor: pointer;
  z-index: 1;
}

.close svg {
  width: 16px;
  height: 16px;
}
</style>
