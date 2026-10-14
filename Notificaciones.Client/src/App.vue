<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import HeaderNav from './components/HeaderNav.vue'
import TelemetryCards from './components/TelemetryCards.vue'
import PipelineTrack from './components/PipelineTrack.vue'
import NotificationPlayground from './components/NotificationPlayground.vue'
import LiveEventStream from './components/LiveEventStream.vue'
import PrivacyNotice from './components/PrivacyNotice.vue'
import TemplatePreviewModal from './components/TemplatePreviewModal.vue'

import type {
  HealthStatusResponse,
  NotificationLifecycleEvent,
  TelemetrySnapshot,
  TemplatePreviewResponse,
} from './types/contracts'
import {
  connectNotificationStream,
  fetchHealth,
  type SseConnectionStatus,
} from './services/api'

// Initial baseline telemetry snapshot
const snapshot = ref<TelemetrySnapshot>({
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
})

const events = ref<NotificationLifecycleEvent[]>([])
const latestEvent = ref<NotificationLifecycleEvent | null>(null)
const health = ref<HealthStatusResponse | null>(null)
const connectionStatus = ref<SseConnectionStatus>('connecting')
const transportMode = ref<string>('in-memory')
const activeListeners = ref<number>(1)
const previewData = ref<TemplatePreviewResponse | null>(null)

let disconnectSse: (() => void) | null = null

onMounted(async () => {
  // Fetch initial health diagnostics
  try {
    const healthRes = await fetchHealth()
    health.value = healthRes
    transportMode.value = healthRes.transport || 'in-memory'
  } catch (err) {
    console.warn('Initial health check failed (API might be booting):', err)
  }

  // Subscribe to real-time SSE stream
  disconnectSse = connectNotificationStream({
    onStatusChange: (status) => {
      connectionStatus.value = status
    },
    onHydrate: (payload) => {
      snapshot.value = payload.snapshot
      events.value = payload.recent || []
      activeListeners.value = payload.listeners || 1
      if (events.value.length > 0) {
        latestEvent.value = events.value[0]
      }
    },
    onLifecycle: (newEvent) => {
      latestEvent.value = newEvent
      // Prepend event to list, keep at most 100 in memory
      events.value = [newEvent, ...events.value].slice(0, 100)
    },
    onMetrics: (newSnapshot) => {
      snapshot.value = newSnapshot
    },
  })
})

onUnmounted(() => {
  if (disconnectSse) {
    disconnectSse()
  }
})

function handleClearEvents() {
  events.value = []
  latestEvent.value = null
}

function handleOpenPreview(data: TemplatePreviewResponse) {
  previewData.value = data
}

function handleClosePreview() {
  previewData.value = null
}
</script>

<template>
  <div class="app-root">
    <!-- Ambient Backdrop Effects -->
    <div class="backdrop-aurora" aria-hidden="true">
      <div class="aurora aurora-cyan"></div>
      <div class="aurora aurora-violet"></div>
      <div class="aurora aurora-amber"></div>
      <div class="grid-overlay"></div>
    </div>

    <!-- Header Navigation -->
    <HeaderNav
      :connection-status="connectionStatus"
      :transport-mode="transportMode"
      :active-listeners="activeListeners"
      :instance-id="health?.instanceId"
    />

    <!-- Main Container -->
    <main class="container main-content">
      <!-- Hero Section -->
      <section class="hero-section">
        <div class="hero-eyebrow">
          <span class="eyebrow-pill">.NET 10</span>
          <span class="eyebrow-text">Clean Architecture · MassTransit · Server-Sent Events</span>
        </div>

        <h1 class="display-title hero-heading">
          Pasarela de Notificaciones Asíncronas
          <span class="gradient-text">Observable en Tiempo Real</span>
        </h1>

        <p class="hero-lede">
          Microservicio de alto rendimiento con desacoplamiento total mediante bus de eventos, plantillas HTML/VML a prueba de clientes de correo, protección contra spam por señuelo (honeypot) y telemetría de latencia end-to-end transmitida directamente al navegador.
        </p>
      </section>

      <!-- KPI Telemetry Cards -->
      <TelemetryCards :snapshot="snapshot" />

      <!-- Interactive Pipeline Flow Track -->
      <PipelineTrack
        :latest-event="latestEvent"
        :events="events"
      />

      <!-- Two-Column Interactive Workstation -->
      <section class="workstation-grid">
        <div class="workstation-left">
          <NotificationPlayground
            @preview="handleOpenPreview"
          />
        </div>
        <div class="workstation-right">
          <LiveEventStream
            :events="events"
            @clear="handleClearEvents"
          />
        </div>
      </section>

      <!-- Security, Privacy & Health Checks -->
      <PrivacyNotice :health="health" />
    </main>

    <!-- Email Template Preview Modal -->
    <TemplatePreviewModal
      :preview="previewData"
      @close="handleClosePreview"
    />
  </div>
</template>

<style scoped>
.app-root {
  min-height: 100vh;
  position: relative;
  display: flex;
  flex-direction: column;
}

.main-content {
  flex: 1;
  padding-top: 2rem;
  padding-bottom: 3rem;
}

.hero-section {
  text-align: center;
  max-width: 860px;
  margin: 0 auto 2.5rem;
}

.hero-eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: rgba(15, 21, 42, 0.7);
  border: 1px solid var(--color-border);
  padding: 0.3rem 0.85rem;
  border-radius: var(--radius-full);
  margin-bottom: 1.25rem;
  font-size: 0.8rem;
}

.eyebrow-pill {
  font-family: var(--font-mono);
  font-weight: 700;
  color: var(--accent-cyan);
  background: rgba(56, 189, 248, 0.15);
  padding: 0.1rem 0.4rem;
  border-radius: 4px;
}

.eyebrow-text {
  color: var(--color-text-muted);
}

.hero-heading {
  font-size: 2.75rem;
  line-height: 1.15;
  margin-bottom: 1rem;
  color: var(--color-text-main);
}

.hero-lede {
  font-size: 1.05rem;
  color: var(--color-text-muted);
  line-height: 1.6;
}

.workstation-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  align-items: stretch;
}

.workstation-left,
.workstation-right {
  display: flex;
  flex-direction: column;
}

.workstation-left :deep(.playground-panel),
.workstation-right :deep(.stream-panel) {
  height: 100%;
}

@media (max-width: 1100px) {
  .workstation-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .hero-heading {
    font-size: 2rem;
  }
}
</style>
