<script setup lang="ts">
import type { TelemetrySnapshot } from '../types/contracts'

defineProps<{
  snapshot: TelemetrySnapshot
}>()

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  return `${(bytes / 1024).toFixed(1)} KB`
}
</script>

<template>
  <div class="kpi-grid">
    <!-- Dispatched -->
    <article class="glass-panel kpi-card kpi-cyan">
      <div class="kpi-header">
        <span class="kpi-label">Despachadas</span>
        <svg class="kpi-icon" viewBox="0 0 20 20" fill="currentColor">
          <path d="M10.894 2.553a1 1 0 00-1.788 0l-7 14a1 1 0 001.169 1.409l5-1.429A1 1 0 009 15.571V11a1 1 0 112 0v4.571a1 1 0 00.725.962l5 1.428a1 1 0 001.17-1.408l-7-14z" />
        </svg>
      </div>
      <div class="kpi-body">
        <span class="kpi-val font-mono">{{ snapshot.dispatched }}</span>
        <span class="kpi-unit">msgs</span>
      </div>
      <div class="kpi-footer">
        <span class="kpi-sub font-mono">
          <strong :class="{ 'text-amber': snapshot.inFlight > 0 }">{{ snapshot.inFlight }}</strong> en vuelo
        </span>
        <span class="kpi-accepted font-mono">Total: {{ snapshot.accepted }}</span>
      </div>
      <div class="card-glow glow-cyan"></div>
    </article>

    <!-- Latency p95 -->
    <article class="glass-panel kpi-card kpi-violet">
      <div class="kpi-header">
        <span class="kpi-label">Latencia p95</span>
        <svg class="kpi-icon" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z" clip-rule="evenodd" />
        </svg>
      </div>
      <div class="kpi-body">
        <span class="kpi-val font-mono">{{ snapshot.p95LatencyMs.toFixed(1) }}</span>
        <span class="kpi-unit">ms</span>
      </div>
      <div class="kpi-footer">
        <span class="kpi-sub font-mono">
          Media: <strong>{{ snapshot.averageLatencyMs.toFixed(1) }}</strong> ms
        </span>
        <span class="kpi-fastest font-mono">Min: {{ snapshot.fastestLatencyMs.toFixed(1) }} ms</span>
      </div>
      <div class="card-glow glow-violet"></div>
    </article>

    <!-- Success Rate -->
    <article class="glass-panel kpi-card kpi-emerald">
      <div class="kpi-header">
        <span class="kpi-label">Tasa de Éxito</span>
        <svg class="kpi-icon" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
        </svg>
      </div>
      <div class="kpi-body">
        <span class="kpi-val font-mono">{{ snapshot.successRate.toFixed(1) }}</span>
        <span class="kpi-unit">%</span>
      </div>
      <div class="kpi-footer">
        <span class="kpi-sub font-mono">
          <strong :class="{ 'text-rose': snapshot.failed > 0 }">{{ snapshot.failed }}</strong> fallos
        </span>
        <span class="kpi-uptime font-mono">Uptime: {{ Math.floor(snapshot.uptimeSeconds / 60) }}m</span>
      </div>
      <div class="card-glow glow-emerald"></div>
    </article>

    <!-- Rendered HTML -->
    <article class="glass-panel kpi-card kpi-amber">
      <div class="kpi-header">
        <span class="kpi-label">HTML Generado</span>
        <svg class="kpi-icon" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4zm2 6a1 1 0 011-1h6a1 1 0 110 2H7a1 1 0 01-1-1zm1 3a1 1 0 100 2h6a1 1 0 100-2H7z" clip-rule="evenodd" />
        </svg>
      </div>
      <div class="kpi-body">
        <span class="kpi-val font-mono">{{ formatBytes(snapshot.renderedBytes) }}</span>
      </div>
      <div class="kpi-footer">
        <span class="kpi-sub">Motor de plantillas VML</span>
        <span class="kpi-types font-mono">{{ Object.keys(snapshot.byType).length }} tipos</span>
      </div>
      <div class="card-glow glow-amber"></div>
    </article>
  </div>
</template>

<style scoped>
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1.25rem;
}

.kpi-card {
  padding: 1.25rem 1.4rem;
  position: relative;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.kpi-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.5rem;
}

.kpi-label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.kpi-icon {
  width: 18px;
  height: 18px;
  opacity: 0.6;
}

.kpi-cyan .kpi-icon { color: var(--accent-cyan); }
.kpi-violet .kpi-icon { color: var(--accent-violet); }
.kpi-emerald .kpi-icon { color: var(--accent-emerald); }
.kpi-amber .kpi-icon { color: var(--accent-amber); }

.kpi-body {
  display: flex;
  align-items: baseline;
  gap: 0.35rem;
  margin: 0.25rem 0;
}

.kpi-val {
  font-size: 2.1rem;
  font-weight: 700;
  line-height: 1.1;
  color: var(--color-text-main);
}

.kpi-unit {
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--color-text-dim);
}

.kpi-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 0.75rem;
  padding-top: 0.6rem;
  border-top: 1px solid var(--color-border);
  font-size: 0.775rem;
  color: var(--color-text-dim);
}

.kpi-footer strong {
  color: var(--color-text-main);
}

.text-amber {
  color: var(--accent-amber);
}

.text-rose {
  color: var(--accent-rose);
}

.card-glow {
  position: absolute;
  width: 80px;
  height: 80px;
  border-radius: 50%;
  bottom: -30px;
  right: -30px;
  filter: blur(40px);
  pointer-events: none;
  opacity: 0.35;
}

.glow-cyan { background: var(--accent-cyan); }
.glow-violet { background: var(--accent-violet); }
.glow-emerald { background: var(--accent-emerald); }
.glow-amber { background: var(--accent-amber); }

@media (max-width: 1024px) {
  .kpi-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 580px) {
  .kpi-grid {
    grid-template-columns: 1fr;
  }
}
</style>
