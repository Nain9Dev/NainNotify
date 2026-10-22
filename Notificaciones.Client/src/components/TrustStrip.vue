<script setup lang="ts">
import type { HealthStatusResponse } from '../types/contracts'

defineProps<{
  health: HealthStatusResponse | null
}>()

const GUARANTEES = [
  { key: 'pii', label: 'Sin datos personales en logs', icon: 'shield' },
  { key: 'spam', label: 'Honeypot anti-spam', icon: 'bug' },
  { key: 'transport', label: 'En memoria o RabbitMQ', icon: 'layers' },
  { key: 'sse', label: 'Eventos en tiempo real', icon: 'bolt' },
] as const
</script>

<template>
  <section class="section trust-section" aria-label="Garantías">
    <div class="container">
      <ul class="guarantees">
        <li v-for="g in GUARANTEES" :key="g.key" class="guarantee">
          <span class="g-icon" aria-hidden="true">
            <svg v-if="g.icon === 'shield'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" /><path d="M9 12l2 2 4-4" />
            </svg>
            <svg v-else-if="g.icon === 'bug'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="11" width="18" height="10" rx="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" />
            </svg>
            <svg v-else-if="g.icon === 'layers'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 2l10 5-10 5L2 7l10-5z" /><path d="M2 12l10 5 10-5M2 17l10 5 10-5" />
            </svg>
            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <path d="M13 2L3 14h8l-1 8 10-12h-8l1-8z" />
            </svg>
          </span>
          <span class="g-label">{{ g.label }}</span>
        </li>
      </ul>

      <div v-if="health" class="health">
        <span class="pill health-pill" :class="health.status === 'Healthy' ? 'is-ok' : 'is-warn'">
          <span class="pill-dot" aria-hidden="true"></span>
          {{ health.status === 'Healthy' ? 'Servicio operativo' : health.status }}
        </span>
        <span v-for="(dep, name) in health.dependencies" :key="name" class="pill dep-pill mono" :class="dep.status.startsWith('Healthy') ? 'is-ok' : 'is-warn'">
          <span class="pill-dot" aria-hidden="true"></span>{{ name }}
        </span>
      </div>
    </div>
  </section>
</template>

<style scoped>
.trust-section {
  padding-top: 0;
}

.guarantees {
  list-style: none;
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.75rem;
}

.guarantee {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.9rem 1rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--line);
  background: var(--bg-panel);
}

.g-icon {
  width: 36px;
  height: 36px;
  flex-shrink: 0;
  display: grid;
  place-items: center;
  border-radius: 10px;
  background: var(--accent-soft);
  color: var(--accent);
}

.g-icon svg {
  width: 19px;
  height: 19px;
}

.g-label {
  font-size: 0.875rem;
  font-weight: 600;
  line-height: 1.25;
}

.health {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 1rem;
}

.pill.is-ok { color: var(--ok); background: rgba(52, 211, 153, 0.1); border-color: rgba(52, 211, 153, 0.25); }
.pill.is-warn { color: var(--warn); background: rgba(251, 191, 36, 0.1); border-color: rgba(251, 191, 36, 0.25); }

.dep-pill {
  text-transform: lowercase;
  font-weight: 500;
}

@media (max-width: 900px) {
  .guarantees { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 480px) {
  .guarantees { grid-template-columns: 1fr; }
}
</style>
