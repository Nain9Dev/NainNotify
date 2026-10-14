<script setup lang="ts">
import type { HealthStatusResponse } from '../types/contracts'

defineProps<{
  health: HealthStatusResponse | null
}>()
</script>

<template>
  <div class="glass-panel security-panel">
    <div class="panel-header">
      <div>
        <h3 class="display-title panel-title">Garantías de Seguridad & Arquitectura Determinista</h3>
        <p class="panel-sub">
          Diseñado con estándares de confidencialidad estricta, aislamiento de datos personales (PII) y resiliencia ante fallos.
        </p>
      </div>
      <div v-if="health" class="health-badge font-mono" :class="health.status === 'Healthy' ? 'health-ok' : 'health-warn'">
        Estado: {{ health.status }} ({{ health.checkDurationMs }} ms)
      </div>
    </div>

    <div class="features-grid">
      <!-- Feature 1 -->
      <div class="feature-card">
        <div class="feature-icon icon-cyan">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
          </svg>
        </div>
        <div class="feature-body">
          <h4 class="feature-title">PrivacyMasker Determinista</h4>
          <p class="feature-text">
            Los eventos de telemetría y logs anonimizan automáticamente cualquier dirección de correo (p. ej. <code>e*****r@c*****p.com</code>). Los datos personales nunca escapan al panel público ni a los agregadores de trazas.
          </p>
        </div>
      </div>

      <!-- Feature 2 -->
      <div class="feature-card">
        <div class="feature-icon icon-amber">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
            <path d="M7 11V7a5 5 0 0 1 10 0v4" />
          </svg>
        </div>
        <div class="feature-body">
          <h4 class="feature-title">Sandbox Recipient Guard</h4>
          <p class="feature-text">
            La política de demostración pública redirige las solicitudes de prueba hacia un receptor seguro interno a menos que el destinatario pertenezca a la lista de dominios evaluadores explícitamente autorizados.
          </p>
        </div>
      </div>

      <!-- Feature 3 -->
      <div class="feature-card">
        <div class="feature-icon icon-violet">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="22 12 18 12 15 21 9 3 6 12 2 12" />
          </svg>
        </div>
        <div class="feature-body">
          <h4 class="feature-title">Dual-Transport Resiliente</h4>
          <p class="feature-text">
            El microservicio opera tanto en modo <em>In-Memory</em> (sin dependencias para evaluaciones locales o integración continua) como enlazado a un cluster distribuido de <em>RabbitMQ</em> con reintentos exponenciales y cola de mensajes muertos (DLQ).
          </p>
        </div>
      </div>

      <!-- Feature 4 -->
      <div class="feature-card">
        <div class="feature-icon icon-emerald">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10" />
            <path d="M12 6v6l4 2" />
          </svg>
        </div>
        <div class="feature-body">
          <h4 class="feature-title">Observabilidad Server-Sent Events</h4>
          <p class="feature-text">
            Sin sondeos agresivos (polling). La interfaz se suscribe reactivamente a un canal de streaming unidireccional con pulsos de latido y búfer circular acotado, minimizando el consumo de CPU y ancho de banda.
          </p>
        </div>
      </div>
    </div>

    <!-- Active Dependencies Health Diagnostics -->
    <div v-if="health && Object.keys(health.dependencies).length > 0" class="dependencies-section">
      <h5 class="dependencies-title">Sondas de Diagnóstico de Salud (Health Checks):</h5>
      <div class="dep-grid">
        <div
          v-for="(dep, name) in health.dependencies"
          :key="name"
          class="dep-item font-mono"
        >
          <span class="dep-status-dot" :class="dep.status === 'Healthy' ? 'dot-healthy' : 'dot-unhealthy'"></span>
          <span class="dep-name">{{ name }}</span>
          <span class="dep-dur">{{ dep.durationMs }}ms</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.security-panel {
  padding: 1.5rem;
  margin-top: 1.5rem;
  margin-bottom: 3rem;
}

.panel-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.panel-title {
  font-size: 1.25rem;
  color: var(--color-text-main);
  margin-bottom: 0.25rem;
}

.panel-sub {
  font-size: 0.85rem;
  color: var(--color-text-dim);
}

.health-badge {
  font-size: 0.75rem;
  padding: 0.25rem 0.65rem;
  border-radius: var(--radius-full);
  white-space: nowrap;
}

.health-ok {
  background: rgba(52, 211, 153, 0.15);
  color: var(--accent-emerald);
  border: 1px solid rgba(52, 211, 153, 0.3);
}

.health-warn {
  background: rgba(245, 158, 11, 0.15);
  color: var(--accent-amber);
  border: 1px solid rgba(245, 158, 11, 0.3);
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.25rem;
}

.feature-card {
  display: flex;
  gap: 1rem;
  padding: 1rem;
  background: rgba(10, 14, 28, 0.6);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  transition: all var(--transition-fast);
}

.feature-card:hover {
  background: rgba(15, 21, 42, 0.8);
  border-color: var(--color-border-bright);
}

.feature-icon {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.feature-icon svg {
  width: 20px;
  height: 20px;
}

.icon-cyan { background: rgba(56, 189, 248, 0.15); color: #38bdf8; }
.icon-amber { background: rgba(251, 191, 36, 0.15); color: #fbbf24; }
.icon-violet { background: rgba(167, 139, 250, 0.15); color: #a78bfa; }
.icon-emerald { background: rgba(52, 211, 153, 0.15); color: #34d399; }

.feature-title {
  font-size: 0.925rem;
  color: var(--color-text-main);
  margin-bottom: 0.35rem;
  font-weight: 600;
}

.feature-text {
  font-size: 0.8rem;
  color: var(--color-text-muted);
  line-height: 1.5;
}

.feature-text code {
  font-family: var(--font-mono);
  background: rgba(255, 255, 255, 0.08);
  padding: 0.1rem 0.3rem;
  border-radius: 4px;
  color: var(--color-text-main);
}

.dependencies-section {
  margin-top: 1.25rem;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border);
}

.dependencies-title {
  font-size: 0.8rem;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin-bottom: 0.5rem;
}

.dep-grid {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.dep-item {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  background: rgba(10, 14, 28, 0.8);
  border: 1px solid var(--color-border);
  padding: 0.25rem 0.65rem;
  border-radius: 4px;
  font-size: 0.75rem;
}

.dep-status-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
}

.dot-healthy { background: var(--accent-emerald); }
.dot-unhealthy { background: var(--accent-rose); }

.dep-name { color: var(--color-text-main); }
.dep-dur { color: var(--color-text-dim); font-size: 0.7rem; }

@media (max-width: 860px) {
  .features-grid {
    grid-template-columns: 1fr;
  }
}
</style>
