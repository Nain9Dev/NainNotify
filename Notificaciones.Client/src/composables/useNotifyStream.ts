import { computed, onMounted, onUnmounted, ref } from 'vue'
import type {
  HealthStatusResponse,
  NotificationLifecycleEvent,
  TelemetrySnapshot,
} from '../types/contracts'
import { connectNotificationStream, fetchHealth, type SseConnectionStatus } from '../services/api'

const MAX_EVENTS = 150

export function emptySnapshot(): TelemetrySnapshot {
  return {
    uptimeSeconds: 0,
    accepted: 0,
    dispatched: 0,
    failed: 0,
    inFlight: 0,
    successRate: 100,
    averageLatencyMs: 0,
    p95LatencyMs: 0,
    fastestLatencyMs: 0,
    renderedBytes: 0,
    byType: {},
    byChannel: {},
    throughputSeries: [],
    lastEventAt: null,
  }
}

/** Owns the live telemetry state for the whole page (one SSE subscription). */
export function useNotifyStream() {
  const snapshot = ref<TelemetrySnapshot>(emptySnapshot())
  const events = ref<NotificationLifecycleEvent[]>([])
  const latestEvent = ref<NotificationLifecycleEvent | null>(null)
  const health = ref<HealthStatusResponse | null>(null)
  const status = ref<SseConnectionStatus>('connecting')
  const listeners = ref(1)

  const isLive = computed(() => status.value === 'connected')
  const isStandalone = computed(() => status.value === 'standalone')
  const transport = computed(() => {
    if (isStandalone.value) return 'in-memory'
    return (health.value?.transport || 'in-memory').toLowerCase().includes('rabbit') ? 'rabbitmq' : 'in-memory'
  })

  let disconnect: (() => void) | null = null

  onMounted(async () => {
    disconnect = connectNotificationStream({
      onStatusChange: (next) => {
        status.value = next
      },
      onHydrate: (payload) => {
        snapshot.value = payload.snapshot
        events.value = payload.recent ?? []
        listeners.value = payload.listeners || 1
        latestEvent.value = events.value[0] ?? null
      },
      onLifecycle: (event) => {
        latestEvent.value = event
        events.value = [event, ...events.value].slice(0, MAX_EVENTS)
      },
      onMetrics: (next) => {
        snapshot.value = next
      },
    })

    try {
      health.value = await fetchHealth()
    } catch {
      health.value = null
    }
  })

  onUnmounted(() => {
    disconnect?.()
  })

  function clearEvents() {
    events.value = []
    latestEvent.value = null
  }

  return {
    snapshot,
    events,
    latestEvent,
    health,
    status,
    listeners,
    isLive,
    isStandalone,
    transport,
    clearEvents,
  }
}
