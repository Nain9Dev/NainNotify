<script setup lang="ts">
import { ref } from 'vue'
import type { TemplatePreviewResponse } from '../types/contracts'

defineProps<{
  preview: TemplatePreviewResponse | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

const activeView = ref<'html' | 'text'>('html')
</script>

<template>
  <div v-if="preview" class="modal-backdrop" @click.self="emit('close')">
    <div class="glass-panel modal-card" role="dialog" aria-modal="true" aria-labelledby="modalTitle">
      <!-- Modal Header -->
      <div class="modal-header">
        <div>
          <div class="modal-badges">
            <span class="badge badge-rendered font-mono">{{ preview.type }}</span>
            <span class="preview-metric font-mono">Render: {{ preview.renderMilliseconds }} ms</span>
            <span class="preview-metric font-mono">Tamaño: {{ (preview.bytes / 1024).toFixed(1) }} KB</span>
          </div>
          <h3 id="modalTitle" class="display-title modal-title">Vista Previa de Notificación VML</h3>
        </div>

        <div class="header-right">
          <div class="view-tabs">
            <button
              type="button"
              class="tab-pill"
              :class="{ 'tab-pill-active': activeView === 'html' }"
              @click="activeView = 'html'"
            >
              HTML
            </button>
            <button
              type="button"
              class="tab-pill"
              :class="{ 'tab-pill-active': activeView === 'text' }"
              @click="activeView = 'text'"
            >
              Texto Plano
            </button>
          </div>

          <button
            type="button"
            class="close-btn"
            aria-label="Cerrar modal"
            @click="emit('close')"
          >
            &times;
          </button>
        </div>
      </div>

      <!-- Modal Body -->
      <div class="modal-body">
        <!-- HTML View inside iframe for styling isolation -->
        <div v-show="activeView === 'html'" class="iframe-wrapper">
          <iframe
            :srcdoc="preview.html"
            title="Vista previa del correo renderizado"
            sandbox="allow-same-origin"
            class="preview-iframe"
          ></iframe>
        </div>

        <!-- Plain Text View -->
        <div v-show="activeView === 'text'" class="text-wrapper">
          <pre class="font-mono text-content">{{ preview.plainText }}</pre>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(3, 5, 10, 0.8);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
}

.modal-card {
  width: 100%;
  max-width: 900px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7);
  border: 1px solid var(--color-border-bright);
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid var(--color-border);
  gap: 1rem;
}

.modal-badges {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.35rem;
}

.preview-metric {
  font-size: 0.725rem;
  background: rgba(255, 255, 255, 0.06);
  padding: 0.15rem 0.45rem;
  border-radius: 4px;
  color: var(--color-text-dim);
}

.modal-title {
  font-size: 1.15rem;
  color: var(--color-text-main);
}

.header-right {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.view-tabs {
  display: flex;
  background: rgba(10, 14, 28, 0.8);
  border: 1px solid var(--color-border);
  padding: 0.2rem;
  border-radius: var(--radius-sm);
}

.tab-pill {
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  font-size: 0.775rem;
  font-weight: 600;
  padding: 0.25rem 0.65rem;
  border-radius: 4px;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.tab-pill-active {
  background: rgba(255, 255, 255, 0.1);
  color: var(--color-text-main);
}

.close-btn {
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  font-size: 1.5rem;
  cursor: pointer;
  line-height: 1;
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-sm);
  transition: all var(--transition-fast);
}

.close-btn:hover {
  color: var(--color-text-main);
  background: rgba(255, 255, 255, 0.1);
}

.modal-body {
  flex: 1;
  overflow: hidden;
  background: #ffffff;
  min-height: 500px;
  position: relative;
}

.iframe-wrapper {
  width: 100%;
  height: 100%;
  min-height: 500px;
}

.preview-iframe {
  width: 100%;
  height: 100%;
  min-height: 500px;
  border: none;
}

.text-wrapper {
  background: #0a0e1c;
  padding: 1.5rem;
  height: 100%;
  min-height: 500px;
  overflow-y: auto;
}

.text-content {
  color: #e2e8f0;
  font-size: 0.85rem;
  white-space: pre-wrap;
  word-break: break-word;
  line-height: 1.6;
}
</style>
