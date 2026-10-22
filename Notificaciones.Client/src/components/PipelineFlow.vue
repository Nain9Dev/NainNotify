<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from 'vue'
import type { NotificationLifecycleEvent, NotificationStage, TelemetrySnapshot } from '../types/contracts'
import { formatMs } from '../utils/format'

const props = defineProps<{
  events: NotificationLifecycleEvent[]
  latestEvent: NotificationLifecycleEvent | null
  snapshot: TelemetrySnapshot
}>()

/** Minimum time each stage stays lit, so a sub-50 ms real pipeline is still readable. */
const DWELL_MS = 380
const IDLE_MS = 1400

const activeStage = ref<NotificationStage | null>(null)
const queue: NotificationStage[] = []
let playing = false
let idleTimer: number | undefined

function playNext() {
  const next = queue.shift()
  if (next === undefined) {
    playing = false
    idleTimer = window.setTimeout(() => {
      activeStage.value = null
    }, IDLE_MS)
    return
  }
  playing = true
  activeStage.value = next
  window.setTimeout(playNext, DWELL_MS)
}

watch(
  () => props.latestEvent,
  (event) => {
    if (!event) return
    window.clearTimeout(idleTimer)
    if (queue.length > 12) queue.splice(0, queue.length - 12)
    queue.push(event.stage)
    if (!playing) playNext()
  }
)

onUnmounted(() => window.clearTimeout(idleTimer))

interface Node {
  key: string
  label: string
  caption: string
  stages: NotificationStage[]
  icon: 'client' | 'gateway' | 'bus' | 'worker' | 'deliver'
}

const NODES: Node[] = [
  { key: 'client', label: 'Tu app', caption: 'HTTP', stages: ['Accepted'], icon: 'client' },
  { key: 'gateway', label: 'Gateway', caption: 'ASP.NET Core', stages: ['Accepted', 'Queued'], icon: 'gateway' },
  { key: 'bus', label: 'Bus', caption: 'MassTransit', stages: ['Queued', 'Received'], icon: 'bus' },
  { key: 'worker', label: 'Worker', caption: 'Plantillas', stages: ['Received', 'Rendered'], icon: 'worker' },
  { key: 'deliver', label: 'Entrega', caption: 'SMTP / Push', stages: ['Dispatched'], icon: 'deliver' },
]

const WIRE_STAGES: NotificationStage[] = ['Accepted', 'Queued', 'Received', 'Rendered']

const stageCounts = computed(() => {
  const counts: Record<NotificationStage, number> = {
    Accepted: 0,
    Queued: 0,
    Received: 0,
    Rendered: 0,
    Dispatched: 0,
    Failed: 0,
  }
  for (const ev of props.events) counts[ev.stage]++
  return counts
})

function nodeCount(node: Node): number {
  return stageCounts.value[node.stages[node.stages.length - 1]]
}

function isNodeActive(node: Node): boolean {
  return activeStage.value !== null && node.stages.includes(activeStage.value)
}

function isWireActive(i: number): boolean {
  return activeStage.value === WIRE_STAGES[i]
}

const failed = computed(() => activeStage.value === 'Failed')

const kpis = computed(() => [
  { key: 'sent', label: 'Entregadas', value: String(props.snapshot.dispatched), tone: 'ok' },
  { key: 'flight', label: 'En vuelo', value: String(props.snapshot.inFlight), tone: props.snapshot.inFlight > 0 ? 'warn' : '' },
  { key: 'p95', label: 'Latencia p95', value: formatMs(props.snapshot.p95LatencyMs), tone: '' },
  { key: 'rate', label: 'Éxito', value: `${props.snapshot.successRate.toFixed(1)} %`, tone: props.snapshot.failed > 0 ? 'warn' : 'ok' },
])
</script>

<template>
  <section class="section flow-section" id="flow" aria-label="Recorrido de una notificación">
    <div class="container">
      <div class="panel flow-panel" :class="{ 'is-failed': failed }">
        <ol class="flow" aria-label="Etapas del pipeline">
          <template v-for="(node, i) in NODES" :key="node.key">
            <li class="node" :class="{ 'is-active': isNodeActive(node) }">
              <div class="node-icon" aria-hidden="true">
                <svg v-if="node.icon === 'client'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="4" width="18" height="13" rx="2.5" /><path d="M8 20h8M12 17v3" />
                </svg>
                <svg v-else-if="node.icon === 'gateway'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M4 7h16M4 12h16M4 17h16" /><circle cx="9" cy="7" r="1.8" fill="currentColor" stroke="none" /><circle cx="15" cy="12" r="1.8" fill="currentColor" stroke="none" /><circle cx="10" cy="17" r="1.8" fill="currentColor" stroke="none" />
                </svg>
                <svg v-else-if="node.icon === 'bus'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M12 3l8 4.5v9L12 21l-8-4.5v-9L12 3z" /><path d="M4 7.5l8 4.5 8-4.5M12 12v9" />
                </svg>
                <svg v-else-if="node.icon === 'worker'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="3" width="18" height="7" rx="2" /><rect x="3" y="14" width="18" height="7" rx="2" /><path d="M7 6.5h.01M7 17.5h.01" />
                </svg>
                <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M22 2L11 13" /><path d="M22 2l-7 20-4-9-9-4 20-7z" />
                </svg>
              </div>
              <div class="node-text">
                <span class="node-label">{{ node.label }}</span>
                <span class="node-caption">{{ node.caption }}</span>
              </div>
              <span class="node-count mono" :title="`Eventos en esta etapa`">{{ nodeCount(node) }}</span>
            </li>
            <li v-if="i < NODES.length - 1" class="wire" :class="{ 'is-active': isWireActive(i) }" aria-hidden="true">
              <span class="wire-pulse"></span>
            </li>
          </template>
        </ol>

        <dl class="kpis">
          <div v-for="k in kpis" :key="k.key" class="kpi" :class="k.tone ? `tone-${k.tone}` : ''">
            <dt>{{ k.label }}</dt>
            <dd class="mono">{{ k.value }}</dd>
          </div>
        </dl>
      </div>
    </div>
  </section>
</template>

<style scoped>
.flow-section {
  padding-top: 0;
}

.flow-panel {
  padding: clamp(1rem, 2.5vw, 1.75rem);
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  transition: border-color var(--motion) var(--ease);
}

.flow-panel.is-failed {
  border-color: rgba(244, 63, 94, 0.5);
}

.flow {
  list-style: none;
  display: flex;
  align-items: stretch;
  gap: 0;
}

.node {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.8rem 1rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--line);
  background: var(--bg-inset);
  min-width: 0;
  position: relative;
  transition:
    border-color var(--motion) var(--ease),
    box-shadow var(--motion) var(--ease),
    transform var(--motion) var(--ease);
}

.node.is-active {
  border-color: var(--accent-ring);
  box-shadow: 0 0 0 4px var(--accent-soft), 0 12px 30px -12px rgba(56, 189, 248, 0.6);
  transform: translateY(-2px);
}

.node-icon {
  width: 38px;
  height: 38px;
  flex-shrink: 0;
  display: grid;
  place-items: center;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.05);
  color: var(--text-2);
  transition: background var(--motion) var(--ease), color var(--motion) var(--ease);
}

.node-icon svg {
  width: 20px;
  height: 20px;
}

.node.is-active .node-icon {
  background: var(--accent-gradient);
  color: #04101c;
}

.node-text {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
  min-width: 0;
}

.node-label {
  font-weight: 600;
  font-size: 0.9rem;
}

.node-caption {
  font-size: 0.72rem;
  color: var(--text-3);
  white-space: nowrap;
}

.node-count {
  margin-left: auto;
  font-size: 0.72rem;
  color: var(--text-3);
  padding-left: 0.5rem;
}

.wire {
  flex: 1 1 24px;
  min-width: 14px;
  align-self: center;
  height: 2px;
  background: var(--line-strong);
  position: relative;
  overflow: hidden;
  border-radius: 1px;
}

.wire-pulse {
  position: absolute;
  inset: 0;
  transform: translateX(-100%);
  background: var(--accent-gradient);
  opacity: 0;
}

.wire.is-active .wire-pulse {
  opacity: 1;
  animation: wire-travel var(--motion-slow) linear forwards;
}

@keyframes wire-travel {
  from { transform: translateX(-100%); }
  to { transform: translateX(100%); }
}

.kpis {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.75rem;
}

.kpi {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  padding: 0.7rem 0.9rem;
  border-radius: var(--radius-sm);
  background: rgba(255, 255, 255, 0.025);
  border: 1px solid var(--line);
}

.kpi dt {
  font-size: 0.72rem;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--text-3);
}

.kpi dd {
  font-size: 1.35rem;
  font-weight: 600;
  line-height: 1.1;
}

.kpi.tone-ok dd { color: var(--ok); }
.kpi.tone-warn dd { color: var(--warn); }

@media (max-width: 900px) {
  .node-caption { display: none; }
  .node { padding: 0.65rem 0.75rem; gap: 0.5rem; }
  .node-icon { width: 32px; height: 32px; }
  .node-icon svg { width: 17px; height: 17px; }
  .node-label { font-size: 0.82rem; }
}

@media (max-width: 720px) {
  .flow {
    flex-direction: column;
    align-items: stretch;
  }
  .node-caption { display: inline; }
  .wire {
    flex: 0 0 22px;
    width: 2px;
    height: 22px;
    min-width: 0;
    margin: 0 auto;
    align-self: auto;
  }
  @keyframes wire-travel {
    from { transform: translateY(-100%); }
    to { transform: translateY(100%); }
  }
  .kpis {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
