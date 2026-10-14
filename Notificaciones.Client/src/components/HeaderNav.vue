<script setup lang="ts">
import type { SseConnectionStatus } from '../services/api'

defineProps<{
  connectionStatus: SseConnectionStatus
  transportMode: string
  activeListeners: number
  instanceId?: string
}>()
</script>

<template>
  <header class="header-nav">
    <div class="container nav-content">
      <div class="brand">
        <svg class="brand-logo" viewBox="0 0 32 32" aria-hidden="true">
          <defs>
            <linearGradient id="navLogoGrad" x1="0" y1="0" x2="32" y2="32" gradientUnits="userSpaceOnUse">
              <stop stop-color="#38bdf8" />
              <stop offset="1" stop-color="#a78bfa" />
            </linearGradient>
          </defs>
          <rect width="32" height="32" rx="8" fill="url(#navLogoGrad)" fill-opacity="0.15" />
          <rect x="0.5" y="0.5" width="31" height="31" rx="7.5" fill="none" stroke="url(#navLogoGrad)" stroke-opacity="0.5" />
          <path d="M7 11l9 6 9-6" stroke="url(#navLogoGrad)" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round" />
          <rect x="6" y="9" width="20" height="14" rx="2" stroke="url(#navLogoGrad)" stroke-width="2" fill="none" />
        </svg>
        <div class="brand-meta">
          <div class="brand-title">
            <span class="brand-name">NAIN</span><span class="brand-highlight">NOTIFY</span>
            <span class="brand-version">v2.0</span>
          </div>
          <span class="brand-sub">Cloud Notification Gateway · .NET 10 & MassTransit</span>
        </div>
      </div>

      <div class="nav-actions">
        <!-- Transport Badge -->
        <div class="status-pill transport-pill" title="Mecanismo de transporte activo">
          <span class="pill-dot" :class="transportMode === 'rabbitmq' ? 'dot-rabbitmq' : 'dot-memory'"></span>
          <span class="pill-text">{{ transportMode === 'rabbitmq' ? 'RabbitMQ Cluster' : 'In-Memory Bus' }}</span>
        </div>

        <!-- SSE Stream Connection Status -->
        <div class="status-pill connection-pill" :title="`Conexión SSE: ${connectionStatus}`">
          <span
            class="pill-dot"
            :class="{
              'dot-connected': connectionStatus === 'connected',
              'dot-connecting': connectionStatus === 'connecting',
              'dot-disconnected': connectionStatus === 'disconnected' || connectionStatus === 'error'
            }"
          ></span>
          <span class="pill-text">
            {{ connectionStatus === 'connected' ? 'SSE Live' : connectionStatus === 'connecting' ? 'Conectando...' : 'Reconectando' }}
          </span>
          <span v-if="activeListeners > 0" class="pill-badge" title="Clientes SSE concurrentes">
            {{ activeListeners }}
          </span>
        </div>

        <!-- Links -->
        <a class="btn btn-ghost btn-sm" href="/docs" target="_blank" rel="noopener">
          <svg viewBox="0 0 16 16" width="14" height="14" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round">
            <path d="M2 4h12M2 8h12M2 12h8" />
          </svg>
          API Docs
        </a>

        <a class="btn btn-secondary btn-sm" href="https://www.naindev.com" target="_blank" rel="noopener">
          NainDev.com
          <svg viewBox="0 0 16 16" width="12" height="12" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <path d="M6 3h7v7M13 3L5 11" />
          </svg>
        </a>
      </div>
    </div>
  </header>
</template>

<style scoped>
.header-nav {
  position: sticky;
  top: 0;
  z-index: 100;
  background: rgba(5, 7, 14, 0.85);
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  border-bottom: 1px solid var(--color-border);
  padding: 0.75rem 0;
}

.nav-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1.5rem;
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.85rem;
}

.brand-logo {
  width: 34px;
  height: 34px;
  flex-shrink: 0;
}

.brand-meta {
  display: flex;
  flex-direction: column;
}

.brand-title {
  display: flex;
  align-items: baseline;
  gap: 0.35rem;
}

.brand-name {
  font-family: var(--font-display);
  font-weight: 800;
  font-size: 1.15rem;
  letter-spacing: 0.04em;
  color: var(--color-text-main);
}

.brand-highlight {
  font-family: var(--font-display);
  font-weight: 800;
  font-size: 1.15rem;
  letter-spacing: 0.04em;
  color: var(--accent-cyan);
}

.brand-version {
  font-family: var(--font-mono);
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.1rem 0.35rem;
  border-radius: 4px;
  background: rgba(56, 189, 248, 0.12);
  color: var(--accent-cyan);
  border: 1px solid rgba(56, 189, 248, 0.25);
}

.brand-sub {
  font-size: 0.75rem;
  color: var(--color-text-dim);
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.status-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: rgba(15, 21, 42, 0.8);
  border: 1px solid var(--color-border);
  padding: 0.35rem 0.75rem;
  border-radius: var(--radius-full);
  font-size: 0.8rem;
  font-weight: 500;
  color: var(--color-text-muted);
}

.pill-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.dot-connected {
  background: var(--accent-emerald);
  box-shadow: 0 0 8px var(--accent-emerald);
}

.dot-connecting {
  background: var(--accent-amber);
  box-shadow: 0 0 8px var(--accent-amber);
  animation: pulse 1.5s infinite;
}

.dot-disconnected {
  background: var(--accent-rose);
  box-shadow: 0 0 8px var(--accent-rose);
}

.dot-rabbitmq {
  background: #f97316;
  box-shadow: 0 0 8px #f97316;
}

.dot-memory {
  background: var(--accent-violet);
  box-shadow: 0 0 8px var(--accent-violet);
}

.pill-badge {
  font-family: var(--font-mono);
  font-size: 0.7rem;
  background: rgba(255, 255, 255, 0.1);
  padding: 0.1rem 0.35rem;
  border-radius: 4px;
  color: var(--color-text-main);
}

.btn-sm {
  padding: 0.4rem 0.85rem;
  font-size: 0.825rem;
}

@keyframes pulse {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.4; transform: scale(0.9); }
}

@media (max-width: 860px) {
  .brand-sub, .transport-pill {
    display: none;
  }
}
</style>
