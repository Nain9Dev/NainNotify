<script setup lang="ts">
import { ref } from 'vue'
import type { TemplatePreviewResponse } from '../types/contracts'
import { formatBytes } from '../utils/format'

withDefaults(
  defineProps<{
    preview: TemplatePreviewResponse | null
    loading?: boolean
    from: string
    to: string
    subject: string
    tall?: boolean
  }>(),
  { loading: false, tall: false }
)

const emit = defineEmits<{ (e: 'expand'): void }>()
const view = ref<'html' | 'text'>('html')
</script>

<template>
  <div class="mail" :class="{ 'mail-tall': tall }">
    <div class="mail-toolbar">
      <div class="window-dots" aria-hidden="true"><i></i><i></i><i></i></div>
      <div class="view-tabs" role="tablist" aria-label="Formato">
        <button type="button" role="tab" class="tab" :aria-selected="view === 'html'" @click="view = 'html'">HTML</button>
        <button type="button" role="tab" class="tab" :aria-selected="view === 'text'" @click="view = 'text'">Texto</button>
      </div>
      <button type="button" class="btn btn-ghost btn-sm expand-btn" aria-label="Ampliar vista previa" @click="emit('expand')">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <path d="M15 3h6v6M9 21H3v-6M21 3l-7 7M3 21l7-7" />
        </svg>
      </button>
    </div>

    <div class="mail-head">
      <div class="mail-row"><span class="k">De</span><span class="v">{{ from || '—' }}</span></div>
      <div class="mail-row"><span class="k">Para</span><span class="v mono">{{ to || '—' }}</span></div>
      <div class="mail-row subject"><span class="v">{{ subject || 'Sin asunto' }}</span></div>
    </div>

    <div class="mail-body" :class="{ 'is-loading': loading }">
      <div v-if="!preview" class="mail-empty muted">Elige un escenario para ver el correo.</div>
      <template v-else>
        <iframe
          v-show="view === 'html'"
          class="mail-frame"
          :srcdoc="preview.html"
          title="Vista previa del correo"
          sandbox=""
          loading="lazy"
        ></iframe>
        <pre v-show="view === 'text'" class="mail-text mono">{{ preview.plainText }}</pre>
      </template>
      <div v-if="loading" class="mail-shimmer" aria-hidden="true"></div>
    </div>

    <div v-if="preview" class="mail-foot mono">
      <span>{{ formatBytes(preview.bytes) }}</span>
      <span>render {{ preview.renderMilliseconds }} ms</span>
    </div>
  </div>
</template>

<style scoped>
.mail {
  display: flex;
  flex-direction: column;
  border-radius: var(--radius-md);
  border: 1px solid var(--line);
  background: var(--bg-inset);
  overflow: hidden;
  min-height: 460px;
}

.mail-tall {
  min-height: min(78vh, 760px);
}

.mail-toolbar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem 0.75rem;
  border-bottom: 1px solid var(--line);
  background: rgba(255, 255, 255, 0.02);
}

.window-dots {
  display: flex;
  gap: 5px;
}

.window-dots i {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.14);
}

.view-tabs {
  display: inline-flex;
  padding: 2px;
  border-radius: var(--radius-xs);
  background: rgba(255, 255, 255, 0.05);
  margin-left: auto;
}

.tab {
  background: transparent;
  border: none;
  padding: 0.3rem 0.65rem;
  border-radius: 5px;
  font-size: 0.78rem;
  font-weight: 600;
  color: var(--text-2);
  cursor: pointer;
}

.tab[aria-selected='true'] {
  background: rgba(255, 255, 255, 0.1);
  color: var(--text);
}

.expand-btn {
  min-height: 32px;
  padding: 0 0.4rem;
}

.mail-head {
  padding: 0.65rem 0.9rem;
  border-bottom: 1px solid var(--line);
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  font-size: 0.82rem;
}

.mail-row {
  display: flex;
  gap: 0.6rem;
  min-width: 0;
}

.mail-row .k {
  width: 2.6rem;
  color: var(--text-3);
  flex-shrink: 0;
}

.mail-row .v {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: var(--text-2);
}

.mail-row.subject .v {
  color: var(--text);
  font-weight: 600;
  font-size: 0.95rem;
  margin-top: 0.15rem;
}

.mail-body {
  flex: 1;
  position: relative;
  background: #f3f4f6;
  min-height: 300px;
}

.mail-empty {
  position: absolute;
  inset: 0;
  display: grid;
  place-items: center;
  background: var(--bg-inset);
  font-size: 0.9rem;
}

.mail-frame {
  width: 100%;
  height: 100%;
  min-height: 300px;
  border: 0;
  display: block;
  background: #f3f4f6;
}

.mail-text {
  position: absolute;
  inset: 0;
  margin: 0;
  padding: 1rem;
  overflow: auto;
  white-space: pre-wrap;
  word-break: break-word;
  font-size: 0.82rem;
  line-height: 1.6;
  background: var(--bg-inset);
  color: var(--text-2);
}

.mail-shimmer {
  position: absolute;
  inset: 0;
  background: linear-gradient(100deg, transparent 20%, rgba(255, 255, 255, 0.35) 50%, transparent 80%);
  background-size: 200% 100%;
  animation: shimmer 1s linear infinite;
  pointer-events: none;
}

.is-loading .mail-frame,
.is-loading .mail-text {
  opacity: 0.6;
}

@keyframes shimmer {
  from { background-position: 200% 0; }
  to { background-position: -200% 0; }
}

.mail-foot {
  display: flex;
  justify-content: space-between;
  padding: 0.4rem 0.9rem;
  border-top: 1px solid var(--line);
  font-size: 0.7rem;
  color: var(--text-3);
}
</style>
