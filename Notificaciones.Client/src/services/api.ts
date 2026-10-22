import type {
  DemoNotificationRequest,
  DemoNotificationResponse,
  ContactFormRequest,
  ContactFormResponse,
  HealthStatusResponse,
  NotificationLifecycleEvent,
  StreamHydrationPayload,
  TelemetrySnapshot,
  TemplatePreviewResponse,
} from '../types/contracts'
import { mockNotifyEngine } from './mockNotifyEngine'

const API_BASE = '/api/v1/Notifications'

/** True only when served by the Vite dev server, where `/api` is proxied to the gateway. */
function hasLocalGateway(): boolean {
  return window.location.hostname === 'localhost' && window.location.port === '5378'
}

async function tryJson<T>(input: string, init?: RequestInit): Promise<T | null> {
  if (!hasLocalGateway()) return null
  try {
    const res = await fetch(input, init)
    if (res.ok) return (await res.json()) as T
  } catch {
    // Gateway unreachable: the caller falls back to the standalone engine.
  }
  return null
}

function postJson(body: unknown): RequestInit {
  return {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  }
}

export async function fetchHealth(): Promise<HealthStatusResponse> {
  return (await tryJson<HealthStatusResponse>(`${API_BASE}/health`)) ?? mockNotifyEngine.fetchHealth()
}

export async function fetchMetrics(): Promise<{ snapshot: TelemetrySnapshot; listeners: number }> {
  return (
    (await tryJson<{ snapshot: TelemetrySnapshot; listeners: number }>(`${API_BASE}/metrics`)) ??
    mockNotifyEngine.fetchMetrics()
  )
}

export async function fetchActivity(limit = 40): Promise<NotificationLifecycleEvent[]> {
  return (
    (await tryJson<NotificationLifecycleEvent[]>(`${API_BASE}/activity?limit=${limit}`)) ??
    mockNotifyEngine.fetchActivity().slice(0, limit)
  )
}

export async function triggerDemoNotification(
  payload: DemoNotificationRequest
): Promise<DemoNotificationResponse> {
  return (
    (await tryJson<DemoNotificationResponse>(`${API_BASE}/demo`, postJson(payload))) ??
    mockNotifyEngine.triggerDemo(payload)
  )
}

export async function submitContactForm(payload: ContactFormRequest): Promise<ContactFormResponse> {
  return (
    (await tryJson<ContactFormResponse>(`${API_BASE}/contact`, postJson(payload))) ??
    mockNotifyEngine.submitContact(payload)
  )
}

export async function renderTemplatePreview(
  payload: DemoNotificationRequest
): Promise<TemplatePreviewResponse> {
  return (
    (await tryJson<TemplatePreviewResponse>(`${API_BASE}/preview`, postJson(payload))) ??
    mockNotifyEngine.renderPreview(payload)
  )
}

/**
 * `connected`  — live Server-Sent Events from the gateway.
 * `standalone` — the in-browser engine (GitHub Pages, or the gateway is down).
 */
export type SseConnectionStatus = 'connecting' | 'connected' | 'standalone' | 'disconnected' | 'error'

export interface SseHandlers {
  onHydrate: (payload: StreamHydrationPayload) => void
  onLifecycle: (event: NotificationLifecycleEvent) => void
  onMetrics: (snapshot: TelemetrySnapshot) => void
  onStatusChange: (status: SseConnectionStatus) => void
}

export function connectNotificationStream(handlers: SseHandlers): () => void {
  if (!hasLocalGateway()) {
    return mockNotifyEngine.connectStream(handlers)
  }

  let eventSource: EventSource | null = null
  let disconnectFallback: (() => void) | null = null
  let closed = false
  let everOpened = false

  const parse = <T>(e: MessageEvent, apply: (value: T) => void) => {
    try {
      apply(JSON.parse(e.data) as T)
    } catch (err) {
      console.error('Malformed SSE payload', err)
    }
  }

  function closeSource() {
    if (eventSource) {
      eventSource.close()
      eventSource = null
    }
  }

  function connectLive() {
    if (closed) return
    handlers.onStatusChange('connecting')
    eventSource = new EventSource(`${API_BASE}/stream`)
    eventSource.addEventListener('hydrate', (e: MessageEvent) => parse(e, handlers.onHydrate))
    eventSource.addEventListener('lifecycle', (e: MessageEvent) => parse(e, handlers.onLifecycle))
    eventSource.addEventListener('metrics', (e: MessageEvent) => parse(e, handlers.onMetrics))
    eventSource.onopen = () => {
      everOpened = true
      handlers.onStatusChange('connected')
    }
    eventSource.onerror = () => {
      closeSource()
      if (closed) return
      if (!everOpened) {
        // The gateway never answered: run the page on the standalone engine instead.
        disconnectFallback = mockNotifyEngine.connectStream(handlers)
        return
      }
      handlers.onStatusChange('disconnected')
      window.setTimeout(connectLive, 3000)
    }
  }

  connectLive()

  return () => {
    closed = true
    closeSource()
    disconnectFallback?.()
  }
}
