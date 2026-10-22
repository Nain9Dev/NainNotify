<script setup lang="ts">
import { computed, ref } from 'vue'
import type { NotificationLifecycleEvent, NotificationStage, NotificationType } from '../types/contracts'
import { formatMs, formatTime } from '../utils/format'
import StageStepper from './StageStepper.vue'

const props = defineProps<{
  events: NotificationLifecycleEvent[]
}>()

const emit = defineEmits<{ (e: 'clear'): void }>()

const MAX_ROWS = 25

const TYPE_LABEL: Record<NotificationType, string> = {
  SystemAlert: 'Alerta',
  WelcomeMessage: 'Bienvenida',
  ContactFormSubmission: 'Contacto',
  TestDemo: 'Prueba',
}

const STAGE_LABEL: Record<NotificationStage, string> = {
  Accepted: 'Aceptada',
  Queued: 'En cola',
  Received: 'Recibida',
  Rendered: 'Renderizada',
  Dispatched: 'Entregada',
  Failed: 'Fallida',
}

interface Row {
  id: string
  subject: string
  recipient: string
  type: NotificationType
  stages: NotificationStage[]
  failed: boolean
  latestStage: NotificationStage
  latencyMs: number
  lastAt: string
  events: NotificationLifecycleEvent[]
}

const rows = computed<Row[]>(() => {
  const map = new Map<string, Row>()
  // Events arrive newest first; walk oldest → newest so "latest" is right.
  for (let i = props.events.length - 1; i >= 0; i--) {
    const ev = props.events[i]
    let row = map.get(ev.notificationId)
    if (!row) {
      row = {
        id: ev.notificationId,
        subject: ev.subject,
        recipient: ev.recipient,
        type: ev.type,
        stages: [],
        failed: false,
        latestStage: ev.stage,
        latencyMs: 0,
        lastAt: ev.timestamp,
        events: [],
      }
      map.set(ev.notificationId, row)
    }
    row.stages.push(ev.stage)
    row.events.unshift(ev)
    row.latestStage = ev.stage
    row.failed = row.failed || ev.stage === 'Failed'
    row.latencyMs = Math.max(row.latencyMs, ev.elapsedMilliseconds)
    row.lastAt = ev.timestamp
  }
  return [...map.values()]
    .sort((a, b) => (a.lastAt < b.lastAt ? 1 : -1))
    .slice(0, MAX_ROWS)
})

const expanded = ref<string | null>(null)

function toggle(id: string) {
  expanded.value = expanded.value === id ? null : id
}
</script>

<template>
  <section class="section activity-section" id="activity" aria-labelledby="activity-title">
    <div class="container">
      <div class="section-head">
        <h2 class="section-title" id="activity-title">
          Actividad
          <span class="count mono">{{ rows.length }}</span>
        </h2>
        <button v-if="events.length" type="button" class="btn btn-ghost btn-sm" @click="emit('clear')">Limpiar</button>
      </div>

      <div class="panel feed">
        <div v-if="rows.length === 0" class="empty">
          <p class="muted">Aún no hay actividad.</p>
          <a class="btn btn-secondary btn-sm" href="#studio">Enviar la primera</a>
        </div>

        <ul v-else class="rows" role="list">
          <li v-for="row in rows" :key="row.id" class="row-item">
            <button
              type="button"
              class="row"
              :class="{ 'is-open': expanded === row.id, 'is-failed': row.failed }"
              :aria-expanded="expanded === row.id"
              @click="toggle(row.id)"
            >
              <StageStepper :reached="row.stages" :failed="row.failed" />
              <span class="row-main">
                <span class="row-subject">{{ row.subject || 'Sin asunto' }}</span>
                <span class="row-meta mono">{{ row.recipient }}</span>
              </span>
              <span class="pill type-pill">{{ TYPE_LABEL[row.type] ?? row.type }}</span>
              <span class="row-stage" :class="`stage-${row.latestStage}`">{{ STAGE_LABEL[row.latestStage] }}</span>
              <span class="row-latency mono">{{ row.latencyMs > 0 ? formatMs(row.latencyMs) : '—' }}</span>
              <span class="row-time mono">{{ formatTime(row.lastAt) }}</span>
              <svg class="chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M6 9l6 6 6-6" /></svg>
            </button>

            <ol v-if="expanded === row.id" class="trace" aria-label="Eventos de la notificación">
              <li v-for="ev in row.events" :key="`${ev.stage}-${ev.timestamp}`" class="trace-row">
                <span class="pill" :class="`stage-${ev.stage}`">{{ STAGE_LABEL[ev.stage] }}</span>
                <span class="trace-detail">{{ ev.detail }}</span>
                <span class="trace-src mono">{{ ev.source }}</span>
                <span class="trace-ms mono">+{{ formatMs(ev.elapsedMilliseconds) }}</span>
                <span v-if="ev.error" class="trace-error mono">{{ ev.error }}</span>
              </li>
            </ol>
          </li>
        </ul>
      </div>
    </div>
  </section>
</template>

<style scoped>
.activity-section {
  padding-top: 0;
}

.count {
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--accent);
  background: var(--accent-soft);
  border: 1px solid var(--accent-ring);
  border-radius: var(--radius-full);
  padding: 0.15rem 0.5rem;
  margin-left: 0.4rem;
  vertical-align: middle;
}

.feed {
  overflow: hidden;
}

.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  padding: 2.5rem 1rem;
}

.rows {
  list-style: none;
}

.row-item + .row-item {
  border-top: 1px solid var(--line);
}

.row {
  width: 100%;
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto auto auto auto auto;
  align-items: center;
  gap: 1rem;
  padding: 0.8rem 1.1rem;
  background: transparent;
  border: 0;
  text-align: left;
  cursor: pointer;
  transition: background var(--motion-fast) var(--ease);
}

.row:hover,
.row.is-open {
  background: var(--bg-hover);
}

.row.is-failed {
  box-shadow: inset 3px 0 0 var(--stage-failed);
}

.row-main {
  display: flex;
  flex-direction: column;
  min-width: 0;
  line-height: 1.25;
}

.row-subject {
  font-weight: 600;
  font-size: 0.9rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.row-meta {
  font-size: 0.72rem;
  color: var(--text-3);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.type-pill {
  color: var(--text-2);
  background: rgba(255, 255, 255, 0.05);
  border-color: var(--line);
}

.row-stage {
  font-size: 0.8rem;
  font-weight: 600;
  min-width: 5.5rem;
}

.row-latency,
.row-time {
  font-size: 0.78rem;
  color: var(--text-3);
}

.chevron {
  width: 16px;
  height: 16px;
  color: var(--text-3);
  transition: transform var(--motion-fast) var(--ease);
}

.row.is-open .chevron {
  transform: rotate(180deg);
}

.trace {
  list-style: none;
  padding: 0.25rem 1.1rem 0.9rem 1.1rem;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  background: var(--bg-inset);
  border-top: 1px dashed var(--line);
}

.trace-row {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto auto;
  align-items: center;
  gap: 0.75rem;
  font-size: 0.8rem;
  padding: 0.3rem 0;
}

.trace-detail {
  color: var(--text-2);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.trace-src,
.trace-ms {
  font-size: 0.72rem;
  color: var(--text-3);
}

.trace-error {
  grid-column: 1 / -1;
  color: var(--danger);
  font-size: 0.72rem;
}

@media (max-width: 860px) {
  .row {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem 0.75rem;
  }
  .row-main {
    flex: 1 1 0;
    min-width: 0;
  }
  .chevron {
    order: 1;
  }
  .type-pill,
  .row-stage,
  .row-latency,
  .row-time {
    order: 2;
    flex: 0 0 auto;
  }
  .row-stage {
    min-width: 0;
  }
  .row-time {
    margin-left: auto;
  }
  .trace-row {
    grid-template-columns: auto minmax(0, 1fr) auto;
  }
  .trace-src {
    display: none;
  }
}
</style>
