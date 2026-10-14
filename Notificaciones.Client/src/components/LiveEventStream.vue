<script setup lang="ts">
import { ref, computed, nextTick, watch } from 'vue'
import type { NotificationLifecycleEvent, NotificationStage } from '../types/contracts'

const props = defineProps<{
  events: NotificationLifecycleEvent[]
}>()

const emit = defineEmits<{
  (e: 'clear'): void
}>()

const selectedStage = ref<string>('all')
const autoScroll = ref(true)
const terminalRef = ref<HTMLElement | null>(null)

const filteredEvents = computed(() => {
  if (selectedStage.value === 'all') return props.events
  return props.events.filter(e => e.stage === selectedStage.value)
})

watch(
  () => props.events.length,
  async () => {
    if (autoScroll.value && terminalRef.value) {
      await nextTick()
      terminalRef.value.scrollTop = 0
    }
  }
)

function formatTime(isoDate: string): string {
  try {
    const d = new Date(isoDate)
    const hours = String(d.getHours()).padStart(2, '0')
    const minutes = String(d.getMinutes()).padStart(2, '0')
    const seconds = String(d.getSeconds()).padStart(2, '0')
    const ms = String(d.getMilliseconds()).padStart(3, '0')
    return `${hours}:${minutes}:${seconds}.${ms}`
  } catch {
    return isoDate
  }
}

function stageBadgeClass(stage: NotificationStage): string {
  switch (stage) {
    case 'Accepted': return 'badge-accepted'
    case 'Queued': return 'badge-queued'
    case 'Received': return 'badge-received'
    case 'Rendered': return 'badge-rendered'
    case 'Dispatched': return 'badge-dispatched'
    case 'Failed': return 'badge-failed'
    default: return ''
  }
}
</script>

<template>
  <div class="glass-panel stream-panel">
    <!-- Stream Header -->
    <div class="stream-header">
      <div class="header-info">
        <div class="header-title-row">
          <span class="live-dot"></span>
          <h2 class="display-title stream-title">Telemetría Server-Sent Events (SSE)</h2>
          <span class="event-count font-mono">{{ events.length }} eventos</span>
        </div>
        <p class="stream-sub">
          Transmisión continua de eventos de ciclo de vida del dominio emitidos por el bus de mensajería.
        </p>
      </div>

      <div class="stream-controls">
        <!-- Stage Filter -->
        <select v-model="selectedStage" class="filter-select font-mono">
          <option value="all">Todas las etapas</option>
          <option value="Accepted">Accepted</option>
          <option value="Queued">Queued</option>
          <option value="Received">Received</option>
          <option value="Rendered">Rendered</option>
          <option value="Dispatched">Dispatched</option>
          <option value="Failed">Failed</option>
        </select>

        <!-- Auto-scroll button -->
        <button
          type="button"
          class="btn btn-ghost btn-sm control-btn"
          :class="{ 'btn-active': autoScroll }"
          @click="autoScroll = !autoScroll"
          :title="autoScroll ? 'Auto-scroll activo' : 'Auto-scroll pausado'"
        >
          <svg viewBox="0 0 20 20" fill="currentColor" width="14" height="14">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-11a1 1 0 10-2 0v3.586L7.707 9.293a1 1 0 00-1.414 1.414l3 3a1 1 0 001.414 0l3-3a1 1 0 00-1.414-1.414L11 10.586V7z" clip-rule="evenodd" />
          </svg>
          Scroll {{ autoScroll ? 'ON' : 'OFF' }}
        </button>

        <!-- Clear Log -->
        <button
          type="button"
          class="btn btn-ghost btn-sm control-btn"
          @click="emit('clear')"
          title="Limpiar registro de eventos"
        >
          <svg viewBox="0 0 20 20" fill="currentColor" width="14" height="14">
            <path fill-rule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clip-rule="evenodd" />
          </svg>
          Limpiar
        </button>
      </div>
    </div>

    <!-- Terminal Output -->
    <div ref="terminalRef" class="terminal-body">
      <div v-if="filteredEvents.length === 0" class="empty-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" width="36" height="36">
          <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z" />
          <polyline points="13 2 13 9 20 9" />
        </svg>
        <p>Esperando eventos del pipeline... Dispara una notificación desde la consola superior.</p>
      </div>

      <div v-else class="event-stream-list">
        <article
          v-for="ev in filteredEvents"
          :key="`${ev.notificationId}-${ev.stage}-${ev.timestamp}`"
          class="event-row"
          :class="{ 'event-row-failed': ev.stage === 'Failed' }"
        >
          <div class="row-meta">
            <span class="event-time font-mono">{{ formatTime(ev.timestamp) }}</span>
            <span class="badge" :class="stageBadgeClass(ev.stage)">{{ ev.stage }}</span>
            <span class="event-type">{{ ev.type }}</span>
            <span class="event-id font-mono" :title="`Correlation ID: ${ev.notificationId}`">
              #{{ ev.notificationId.slice(0, 8) }}
            </span>
          </div>

          <div class="row-body">
            <span class="event-detail">{{ ev.detail }}</span>
            <div class="row-tags">
              <span v-if="ev.recipient" class="tag tag-recipient font-mono" title="Destinatario con anonimización PII">
                {{ ev.recipient }}
              </span>
              <span v-if="ev.elapsedMilliseconds > 0" class="tag tag-latency font-mono">
                +{{ ev.elapsedMilliseconds.toFixed(1) }}ms
              </span>
              <span v-if="ev.payloadBytes > 0" class="tag tag-bytes font-mono">
                {{ (ev.payloadBytes / 1024).toFixed(1) }} KB
              </span>
              <span v-if="ev.channel" class="tag tag-channel">
                {{ ev.channel }}
              </span>
              <span v-if="ev.error" class="tag tag-error font-mono">
                {{ ev.error }}
              </span>
            </div>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<style scoped>
.stream-panel {
  padding: 1.5rem;
  margin-top: 1.5rem;
  display: flex;
  flex-direction: column;
}

.stream-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1.5rem;
  margin-bottom: 1.25rem;
}

.header-title-row {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  margin-bottom: 0.25rem;
}

.live-dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: var(--accent-cyan);
  box-shadow: 0 0 10px var(--accent-cyan);
  animation: pulse-stream 1.8s infinite;
}

.stream-title {
  font-size: 1.25rem;
  color: var(--color-text-main);
}

.event-count {
  font-size: 0.75rem;
  background: rgba(56, 189, 248, 0.12);
  color: var(--accent-cyan);
  padding: 0.15rem 0.5rem;
  border-radius: var(--radius-full);
  border: 1px solid rgba(56, 189, 248, 0.25);
}

.stream-sub {
  font-size: 0.85rem;
  color: var(--color-text-dim);
}

.stream-controls {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.filter-select {
  background: rgba(10, 14, 28, 0.8);
  border: 1px solid var(--color-border);
  color: var(--color-text-main);
  padding: 0.35rem 0.65rem;
  border-radius: var(--radius-sm);
  font-size: 0.8rem;
}

.control-btn {
  padding: 0.35rem 0.65rem;
  font-size: 0.8rem;
}

.btn-active {
  background: rgba(56, 189, 248, 0.15) !important;
  color: var(--accent-cyan) !important;
  border-color: rgba(56, 189, 248, 0.3) !important;
}

.terminal-body {
  background: #03050a;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  height: 380px;
  overflow-y: auto;
  padding: 0.75rem;
  position: relative;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: var(--color-text-dim);
  text-align: center;
  gap: 0.75rem;
  font-size: 0.875rem;
}

.event-stream-list {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
}

.event-row {
  background: rgba(15, 21, 42, 0.6);
  border-left: 3px solid var(--color-border);
  border-radius: 4px;
  padding: 0.5rem 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  transition: all var(--transition-fast);
}

.event-row:hover {
  background: rgba(22, 30, 58, 0.8);
}

.event-row-failed {
  border-left-color: var(--accent-rose) !important;
  background: rgba(244, 63, 94, 0.08) !important;
}

.row-meta {
  display: flex;
  align-items: center;
  gap: 0.65rem;
}

.event-time {
  font-size: 0.75rem;
  color: var(--color-text-dim);
}

.event-type {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--color-text-muted);
}

.event-id {
  font-size: 0.725rem;
  color: var(--color-text-dim);
  margin-left: auto;
}

.row-body {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.event-detail {
  font-size: 0.825rem;
  color: var(--color-text-main);
}

.row-tags {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-left: auto;
}

.tag {
  font-size: 0.7rem;
  padding: 0.1rem 0.45rem;
  border-radius: 4px;
  line-height: 1.2;
}

.tag-recipient {
  background: rgba(255, 255, 255, 0.06);
  color: var(--color-text-muted);
  border: 1px solid var(--color-border);
}

.tag-latency {
  background: rgba(167, 139, 250, 0.12);
  color: var(--accent-violet);
}

.tag-bytes {
  background: rgba(251, 191, 36, 0.12);
  color: var(--accent-amber);
}

.tag-channel {
  background: rgba(52, 211, 153, 0.12);
  color: var(--accent-emerald);
}

.tag-error {
  background: rgba(244, 63, 94, 0.2);
  color: var(--accent-rose);
  border: 1px solid var(--accent-rose);
}

@keyframes pulse-stream {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.3; transform: scale(0.85); }
}

@media (max-width: 860px) {
  .stream-header {
    flex-direction: column;
  }
}
</style>
