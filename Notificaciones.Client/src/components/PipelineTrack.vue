<script setup lang="ts">
import { computed } from 'vue'
import type { NotificationLifecycleEvent, NotificationStage } from '../types/contracts'

const props = defineProps<{
  latestEvent: NotificationLifecycleEvent | null
  events: NotificationLifecycleEvent[]
}>()

const activeStage = computed<NotificationStage | null>(() => props.latestEvent?.stage ?? null)

// Count transitions per stage
const stageCounts = computed(() => {
  const counts: Record<NotificationStage, number> = {
    Accepted: 0,
    Queued: 0,
    Received: 0,
    Rendered: 0,
    Dispatched: 0,
    Failed: 0,
  }
  for (const ev of props.events) {
    if (counts[ev.stage] !== undefined) {
      counts[ev.stage]++
    }
  }
  return counts
})
</script>

<template>
  <div class="glass-panel pipeline-container">
    <div class="pipeline-header">
      <div>
        <h2 class="display-title pipeline-title">Flujo Asíncrono del Microservicio</h2>
        <p class="pipeline-subtitle">
          Cada nodo se activa en tiempo real cuando el worker y el gateway emiten eventos de ciclo de vida por MassTransit.
        </p>
      </div>

      <div class="stage-legend">
        <span class="legend-item" :class="{ 'legend-active': activeStage === 'Accepted' }">
          <i class="dot-acc"></i>Accepted
        </span>
        <span class="legend-item" :class="{ 'legend-active': activeStage === 'Queued' }">
          <i class="dot-que"></i>Queued
        </span>
        <span class="legend-item" :class="{ 'legend-active': activeStage === 'Received' }">
          <i class="dot-rec"></i>Received
        </span>
        <span class="legend-item" :class="{ 'legend-active': activeStage === 'Rendered' }">
          <i class="dot-ren"></i>Rendered
        </span>
        <span class="legend-item" :class="{ 'legend-active': activeStage === 'Dispatched' }">
          <i class="dot-dis"></i>Dispatched
        </span>
      </div>
    </div>

    <!-- Track Nodes -->
    <div class="nodes-track">
      <!-- 1. Client -->
      <div class="track-node">
        <div class="node-box" :class="{ 'node-active': activeStage === 'Accepted' }">
          <div class="node-icon icon-client">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="2" y="3" width="20" height="14" rx="2" />
              <path d="M8 21h8M12 17v4" />
            </svg>
          </div>
          <div class="node-content">
            <span class="node-label">Cliente HTTP</span>
            <span class="node-tech">naindev.com</span>
          </div>
          <span class="node-counter font-mono">{{ stageCounts.Accepted }}</span>
        </div>
      </div>

      <div class="track-connector" :class="{ 'wire-active': activeStage === 'Accepted' || activeStage === 'Queued' }">
        <div class="wire-pulse"></div>
      </div>

      <!-- 2. Gateway -->
      <div class="track-node">
        <div class="node-box" :class="{ 'node-active': activeStage === 'Accepted' || activeStage === 'Queued' }">
          <div class="node-icon icon-gateway">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M4 6h16M4 12h16M4 18h16" />
              <circle cx="8" cy="6" r="2" fill="currentColor" />
              <circle cx="16" cy="12" r="2" fill="currentColor" />
              <circle cx="10" cy="18" r="2" fill="currentColor" />
            </svg>
          </div>
          <div class="node-content">
            <span class="node-label">API Gateway</span>
            <span class="node-tech">ASP.NET Core</span>
          </div>
          <span class="node-counter font-mono">{{ stageCounts.Queued }}</span>
        </div>
      </div>

      <div class="track-connector" :class="{ 'wire-active': activeStage === 'Queued' || activeStage === 'Received' }">
        <div class="wire-pulse"></div>
      </div>

      <!-- 3. Broker Exchange -->
      <div class="track-node">
        <div class="node-box" :class="{ 'node-active': activeStage === 'Queued' || activeStage === 'Received' }">
          <div class="node-icon icon-broker">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z" />
              <polyline points="3.27 6.96 12 12.01 20.73 6.96" />
              <line x1="12" y1="22.08" x2="12" y2="12" />
            </svg>
          </div>
          <div class="node-content">
            <span class="node-label">Message Bus</span>
            <span class="node-tech">MassTransit Exchange</span>
          </div>
          <span class="node-counter font-mono">{{ stageCounts.Received }}</span>
        </div>
      </div>

      <div class="track-connector" :class="{ 'wire-active': activeStage === 'Received' || activeStage === 'Rendered' }">
        <div class="wire-pulse"></div>
      </div>

      <!-- 4. Worker Consumer -->
      <div class="track-node">
        <div class="node-box" :class="{ 'node-active': activeStage === 'Received' || activeStage === 'Rendered' }">
          <div class="node-icon icon-worker">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="2" y="2" width="20" height="8" rx="2" />
              <rect x="2" y="14" width="20" height="8" rx="2" />
              <line x1="6" y1="6" x2="6.01" y2="6" />
              <line x1="6" y1="18" x2="6.01" y2="18" />
            </svg>
          </div>
          <div class="node-content">
            <span class="node-label">Worker Consumer</span>
            <span class="node-tech">Clean Architecture</span>
          </div>
          <span class="node-counter font-mono">{{ stageCounts.Rendered }}</span>
        </div>
      </div>

      <div class="track-connector" :class="{ 'wire-active': activeStage === 'Rendered' || activeStage === 'Dispatched' }">
        <div class="wire-pulse"></div>
      </div>

      <!-- 5. Delivery Relay -->
      <div class="track-node">
        <div class="node-box" :class="{ 'node-active': activeStage === 'Dispatched' }">
          <div class="node-icon icon-dispatch">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M22 2L11 13" />
              <path d="M22 2l-7 20-4-9-9-4 20-7z" />
            </svg>
          </div>
          <div class="node-content">
            <span class="node-label">Entrega / Sandbox</span>
            <span class="node-tech">SMTP Relay / Mail</span>
          </div>
          <span class="node-counter font-mono">{{ stageCounts.Dispatched }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.pipeline-container {
  padding: 1.5rem;
  margin-top: 1.5rem;
}

.pipeline-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1.5rem;
  margin-bottom: 1.75rem;
}

.pipeline-title {
  font-size: 1.25rem;
  color: var(--color-text-main);
  margin-bottom: 0.25rem;
}

.pipeline-subtitle {
  font-size: 0.85rem;
  color: var(--color-text-dim);
}

.stage-legend {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  flex-wrap: wrap;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.775rem;
  color: var(--color-text-dim);
  transition: all var(--transition-fast);
}

.legend-item i {
  display: inline-block;
  width: 7px;
  height: 7px;
  border-radius: 50%;
}

.legend-active {
  color: var(--color-text-main);
  font-weight: 600;
  transform: scale(1.05);
}

.dot-acc { background: #38bdf8; }
.dot-que { background: #a78bfa; }
.dot-rec { background: #60a5fa; }
.dot-ren { background: #fbbf24; }
.dot-dis { background: #34d399; }

.nodes-track {
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: relative;
  overflow-x: auto;
  padding: 0.5rem 0;
}

.track-node {
  flex: 1;
  min-width: 170px;
}

.node-box {
  background: rgba(10, 14, 28, 0.9);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 0.85rem 1rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  position: relative;
  transition: all var(--transition-normal);
}

.node-active {
  border-color: var(--accent-cyan);
  box-shadow: 0 0 16px rgba(56, 189, 248, 0.3);
  transform: translateY(-2px);
}

.node-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.node-icon svg {
  width: 18px;
  height: 18px;
}

.icon-client { background: rgba(56, 189, 248, 0.15); color: #38bdf8; }
.icon-gateway { background: rgba(167, 139, 250, 0.15); color: #a78bfa; }
.icon-broker { background: rgba(249, 115, 22, 0.15); color: #f97316; }
.icon-worker { background: rgba(96, 165, 250, 0.15); color: #60a5fa; }
.icon-dispatch { background: rgba(52, 211, 153, 0.15); color: #34d399; }

.node-content {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0;
}

.node-label {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--color-text-main);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.node-tech {
  font-size: 0.725rem;
  color: var(--color-text-dim);
  white-space: nowrap;
}

.node-counter {
  font-size: 0.75rem;
  font-weight: 600;
  background: rgba(255, 255, 255, 0.06);
  padding: 0.15rem 0.45rem;
  border-radius: 4px;
  color: var(--color-text-muted);
}

.track-connector {
  width: 28px;
  height: 2px;
  background: var(--color-border);
  position: relative;
  flex-shrink: 0;
}

.wire-active {
  background: linear-gradient(90deg, var(--accent-cyan), var(--accent-violet));
  box-shadow: 0 0 8px rgba(56, 189, 248, 0.6);
}

.wire-pulse {
  position: absolute;
  top: -2px;
  left: 0;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #fff;
  opacity: 0;
}

.wire-active .wire-pulse {
  opacity: 1;
  animation: movePulse 1s linear infinite;
}

@keyframes movePulse {
  0% { left: 0%; }
  100% { left: 100%; }
}

@media (max-width: 900px) {
  .pipeline-header {
    flex-direction: column;
  }
}
</style>
