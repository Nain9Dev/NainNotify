<script setup lang="ts">
import { computed } from 'vue'
import type { SseConnectionStatus } from '../services/api'
import { currentLocale, toggleLocale, t } from '../locales/i18n'

const props = defineProps<{
  status: SseConnectionStatus
  transport: 'in-memory' | 'rabbitmq'
}>()

const REPO_URL = 'https://github.com/Nain9Dev/NainNotify'

const statusView = computed(() => {
  switch (props.status) {
    case 'connected':
      return { label: props.transport === 'rabbitmq' ? t('liveRabbitMq') : t('liveInMemory'), tone: 'ok', live: true }
    case 'standalone':
      return { label: t('localDemo'), tone: 'info', live: false }
    case 'connecting':
      return { label: t('connecting'), tone: 'warn', live: false }
    default:
      return { label: t('reconnecting'), tone: 'danger', live: false }
  }
})
</script>

<template>
  <header class="site-header">
    <div class="container header-row">
      <a class="brand" href="#top" aria-label="NainNotify">
        <svg class="brand-mark" viewBox="0 0 32 32" aria-hidden="true">
          <defs>
            <linearGradient id="brandGrad" x1="0" y1="0" x2="32" y2="32" gradientUnits="userSpaceOnUse">
              <stop stop-color="#38bdf8" />
              <stop offset="1" stop-color="#6366f1" />
            </linearGradient>
          </defs>
          <rect width="32" height="32" rx="9" fill="url(#brandGrad)" />
          <path d="M9 12.5l7 5 7-5" stroke="#04101c" stroke-width="2.2" fill="none" stroke-linecap="round" stroke-linejoin="round" />
          <rect x="8" y="10" width="16" height="12" rx="2.5" stroke="#04101c" stroke-width="2.2" fill="none" />
        </svg>
        <span class="brand-name">Nain<strong>Notify</strong></span>
      </a>

      <nav class="header-nav" aria-label="Principal">
        <span class="pill status-pill" :class="`tone-${statusView.tone}`" :title="`Status: ${statusView.label}`">
          <span v-if="statusView.live" class="live-dot" aria-hidden="true"></span>
          <span v-else class="pill-dot" aria-hidden="true"></span>
          {{ statusView.label }}
        </span>

        <button
          type="button"
          class="btn btn-ghost btn-sm locale-toggle"
          :aria-label="currentLocale === 'es' ? 'Switch to English' : 'Cambiar a Español'"
          @click="toggleLocale"
        >
          <span :class="{ active: currentLocale === 'es' }">ES</span>
          <span class="sep">/</span>
          <span :class="{ active: currentLocale === 'en' }">EN</span>
        </button>

        <a v-if="status === 'connected'" class="btn btn-ghost btn-sm" href="/docs" target="_blank" rel="noopener">
          API
        </a>

        <a class="btn btn-ghost btn-sm icon-only-mobile" :href="REPO_URL" target="_blank" rel="noopener" aria-label="GitHub">
          <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
            <path d="M12 .5C5.65.5.5 5.65.5 12c0 5.08 3.29 9.39 7.86 10.91.58.1.79-.25.79-.56v-2.17c-3.2.7-3.88-1.37-3.88-1.37-.52-1.33-1.28-1.68-1.28-1.68-1.05-.72.08-.7.08-.7 1.16.08 1.77 1.19 1.77 1.19 1.03 1.77 2.7 1.26 3.36.96.1-.75.4-1.26.73-1.55-2.55-.29-5.24-1.28-5.24-5.68 0-1.26.45-2.28 1.19-3.09-.12-.29-.52-1.46.11-3.05 0 0 .97-.31 3.18 1.18a11 11 0 0 1 5.79 0c2.2-1.49 3.17-1.18 3.17-1.18.63 1.59.23 2.76.12 3.05.74.81 1.19 1.83 1.19 3.09 0 4.41-2.69 5.38-5.25 5.67.41.36.78 1.05.78 2.12v3.14c0 .31.21.67.8.56C20.21 21.39 23.5 17.08 23.5 12 23.5 5.65 18.35.5 12 .5z" />
          </svg>
          <span class="label">GitHub</span>
        </a>

        <a class="btn btn-secondary btn-sm site-link" href="https://www.naindev.com" target="_blank" rel="noopener">
          naindev.com
        </a>
      </nav>
    </div>
  </header>
</template>

<style scoped>
.site-header {
  position: sticky;
  top: 0;
  z-index: 50;
  background: rgba(7, 10, 18, 0.72);
  backdrop-filter: blur(18px);
  -webkit-backdrop-filter: blur(18px);
  border-bottom: 1px solid var(--line);
}

.header-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  height: 64px;
}

.brand {
  display: inline-flex;
  align-items: center;
  gap: 0.65rem;
}

.brand-mark {
  width: 32px;
  height: 32px;
}

.brand-name {
  font-family: var(--font-display);
  font-size: 1.15rem;
  font-weight: 500;
  letter-spacing: -0.01em;
}

.brand-name strong {
  font-weight: 700;
}

.header-nav {
  display: flex;
  align-items: center;
  gap: 0.35rem;
}

.status-pill {
  margin-right: 0.35rem;
}

.tone-ok { color: var(--ok); background: rgba(52, 211, 153, 0.12); border-color: rgba(52, 211, 153, 0.3); }
.tone-info { color: var(--accent); background: var(--accent-soft); border-color: var(--accent-ring); }
.tone-warn { color: var(--warn); background: rgba(251, 191, 36, 0.12); border-color: rgba(251, 191, 36, 0.3); }
.tone-danger { color: var(--danger); background: rgba(244, 63, 94, 0.12); border-color: rgba(244, 63, 94, 0.3); }

.locale-toggle {
  font-family: var(--font-mono, monospace);
  font-size: 0.78rem;
  letter-spacing: 0.04em;
  padding: 0.25rem 0.5rem;
  color: var(--text-2);
}

.locale-toggle .active {
  color: var(--accent, #38bdf8);
  font-weight: 700;
}

.locale-toggle .sep {
  opacity: 0.4;
  margin: 0 0.15rem;
}

@media (max-width: 640px) {
  .icon-only-mobile .label {
    display: none;
  }
  .status-pill {
    font-size: 0.68rem;
  }
}

@media (max-width: 520px) {
  .site-link {
    display: none;
  }
}
</style>
